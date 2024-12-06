using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Supabase;
using Supabase.Storage;
using System.IO;
using MegaBook;
using Postgrest.Models;
using Postgrest.Responses;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using Newtonsoft.Json;

/// <summary>
/// Configuration settings for Supabase connection.
/// </summary>
[CreateAssetMenu(fileName = "SupabaseConfig", menuName = "Config/SupabaseConfig")]
public class SupabaseSettings : ScriptableObject
{
    /// <summary>
    /// Base URL of the Supabase project.
    /// </summary>
    public string SupabaseURL;

    /// <summary>
    /// Anonymous API key for Supabase authentication.
    /// </summary>
    public string SupabaseAnonKey;
}

/// <summary>
/// Manages database operations for book projects using Supabase.
/// Handles asset uploads, project data persistence, and state management.
/// </summary>
public class DatabaseController : MonoBehaviour
{
    /// <summary>
    /// Configuration settings for Supabase connection.
    /// </summary>
    [SerializeField] private SupabaseSettings supabaseSettings;

    /// <summary>
    /// Reference to the book component for accessing book data.
    /// </summary>
    [SerializeField] private MegaBookBuilder bookComponent;

    /// <summary>
    /// Reference to the image upload controller for texture management.
    /// </summary>
    [SerializeField] private ImageUploadController imageController;

    /// <summary>
    /// Reference to the audio upload controller for sound management.
    /// </summary>
    [SerializeField] private AudioUploadController audioController;

    /// <summary>
    /// Reference to the export controller for exporting book data.
    /// </summary>
    [SerializeField] private ExportController exportController;

    private Supabase.Client _client;
    private bool _isInitialized = false;
    private TaskCompletionSource<bool> _initializationComplete = new TaskCompletionSource<bool>();
    private const string BUCKET_NAME = "book-assets";

    /// <summary>
    /// Initializes Supabase client and storage on component start.
    /// </summary>
    private async void Start()
    {
        await InitializeClient();
    }

    private async Task InitializeClient()
    {
        try 
        {
            var options = new SupabaseOptions { AutoRefreshToken = true };
            _client = new Supabase.Client(supabaseSettings.SupabaseURL, supabaseSettings.SupabaseAnonKey, options);
            await InitializeStorage();
            _isInitialized = true;
            _initializationComplete.SetResult(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Supabase client: {e.Message}");
            _initializationComplete.SetException(e);
        }
    }

    /// <summary>
    /// Creates or ensures existence of the storage bucket.
    /// </summary>
    private async Task InitializeStorage()
    {
        try
        {
            var bucket = await _client.Storage.GetBucket(BUCKET_NAME);
            if (bucket == null)
            {
                await _client.Storage.CreateBucket(BUCKET_NAME, new BucketUpsertOptions { Public = true });
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Storage init failed: {e.Message}");
        }
    }

    /// <summary>
    /// Saves the current book project to the database.
    /// Uploads all textures and audio assets, then creates database records.
    /// </summary>
    /// <param name="projectName">Name of the project to save.</param>
    public async Task<BookExportData> SaveProject(string projectName)
    {
        Debug.Log($"Starting save project: {projectName}");
        try
        {
            // Wait for initialization to complete
            if (!_isInitialized)
            {
                Debug.Log("Waiting for client initialization...");
                await _initializationComplete.Task;
            }

            Debug.Log("Creating book export data...");
            var bookData = BookExportData.FromBookBuilder(bookComponent);
            var serializableData = bookData.ToSerializableDict();

            Debug.Log("Starting page texture upload...");
            var pageAssets = await UploadPageTextures();
            Debug.Log($"Uploaded {pageAssets.Count} page textures");

            Debug.Log("Starting audio assets upload...");
            var audioAssets = await UploadAudioAssets();
            Debug.Log($"Uploaded {audioAssets.Count} audio assets");

            Debug.Log("Creating project record...");
            var completeConfig = new Dictionary<string, object>(serializableData)
            {
                // Add the assets data to the config
                { "pageAssets", pageAssets },
                { "audioAssets", audioAssets }
            };

            var projectData = new Dictionary<string, object>
            {
                { "p_name", projectName },
                { "p_config", completeConfig },
                { "p_thumbnail_url", pageAssets.Count > 0 ? pageAssets[0].texture_url : null }
            };

            Debug.Log($"Project data to send: {JsonConvert.SerializeObject(projectData, Formatting.Indented)}");

            Debug.Log("About to make RPC call...");
            Debug.Log($"Client status: {_client != null}");
            var response = await _client.Rpc("insert_book_project", projectData);
            
            if (response == null)
            {
                throw new Exception("Failed to save book project - no response from database");
            }

            Debug.Log($"Raw response type: {response.GetType()}");
            Debug.Log($"Raw response content: {response}");

            // Try to get the data from the BaseResponse
            string responseData = response.Content?.ToString();
            Debug.Log($"Response content: {responseData}");

            Dictionary<string, object> savedBook;
            try 
            {
                savedBook = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);
                Debug.Log($"Book saved successfully with ID: {savedBook["id"]}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse response: {e.Message}");
                // Fallback: try to get ID directly from response
                savedBook = new Dictionary<string, object>
                {
                    { "id", response.Content?.ToString() }
                };
                Debug.Log($"Using fallback ID: {savedBook["id"]}");
            }

            // Return the complete book data for export
            bookData.id = savedBook["id"].ToString();
            bookData.name = projectName;
            bookData.pageAssets = pageAssets;
            bookData.audioAssets = audioAssets;
            bookData.savedAt = DateTime.UtcNow;

            // Export the complete book data to JSON
            if (exportController != null)
            {
                exportController.ExportBookData(bookData);
            }
            else
            {
                Debug.LogWarning("ExportController not assigned - skipping JSON export");
            }

            return bookData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e}");
            Debug.LogError($"Save failed at: {e.TargetSite?.Name}");
            Debug.LogError($"Error message: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads a book project and its assets from the database.
    /// Applies configuration and downloads/applies all textures and audio.
    /// </summary>
    /// <param name="projectId">ID of the project to load.</param>
    public async Task LoadProject(string projectId)
    {
        try
        {
            var project = await _client.From<BookProject>()
                .Select("*")
                .Filter("Id", Postgrest.Constants.Operator.Equals, projectId)
                .Single();

            var configData = JsonConvert.DeserializeObject<Dictionary<string, object>>(project.config.ToString());
            
            // Apply book configuration
            ApplyBookConfig(configData);

            // Load page assets
            var pageAssets = JsonConvert.DeserializeObject<List<PageAssetData>>(configData["pageAssets"].ToString());
            foreach (var asset in pageAssets)
            {
                await LoadTexture(asset.page_number, asset.texture_url);
            }

            // Load audio assets
            var audioAssets = JsonConvert.DeserializeObject<List<AudioAssetData>>(configData["audioAssets"].ToString());
            foreach (var asset in audioAssets)
            {
                await LoadAudio(asset.audio_type, asset.audio_url);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Uploads all page textures to storage and returns their URLs.
    /// </summary>
    private async Task<List<PageAssetData>> UploadPageTextures()
    {
        Debug.Log("Starting UploadPageTextures...");
        var assets = new List<PageAssetData>();
        var textures = GetPageTextures();
        Debug.Log($"Found {textures.Count} textures to upload");

        foreach (var texture in textures)
        {
            try
            {
                if (texture.Value == null)
                {
                    Debug.Log($"Skipping null texture for page {texture.Key}");
                    continue;
                }

                Debug.Log($"Processing texture for page {texture.Key}");
                var bytes = texture.Value.EncodeToPNG();
                if (bytes == null || bytes.Length == 0)
                {
                    Debug.LogError($"Failed to encode texture for page {texture.Key}");
                    continue;
                }

                var filename = $"page_{texture.Key}_{Guid.NewGuid()}.png";
                var path = $"pages/{filename}";
                Debug.Log($"Uploading texture to path: {path}");

                await _client.Storage
                    .From(BUCKET_NAME)
                    .Upload(bytes, path);

                var url = _client.Storage
                    .From(BUCKET_NAME)
                    .GetPublicUrl(path);
                Debug.Log($"Texture uploaded successfully. URL: {url}");

                assets.Add(new PageAssetData
                {
                    page_number = texture.Key,
                    texture_url = url
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to upload texture for page {texture.Key}");
                Debug.LogError($"Error: {e.Message}");
                // Continue with other textures instead of throwing
                continue;
            }
        }

        return assets;
    }

    /// <summary>
    /// Uploads all audio clips to storage and returns their URLs.
    /// Handles page turn and background music assets.
    /// </summary>
    /// <returns>List of audio asset data including URLs and types.</returns>
    private async Task<List<AudioAssetData>> UploadAudioAssets()
    {
        Debug.Log("Starting UploadAudioAssets...");
        var assets = new List<AudioAssetData>();
        var audioClips = GetAudioClips();
        Debug.Log($"Found {audioClips.Count} audio clips to upload");

        foreach (var clip in audioClips)
        {
            try
            {
                if (clip.Value == null)
                {
                    Debug.Log($"Skipping null audio clip for type: {clip.Key}");
                    continue;
                }

                Debug.Log($"Processing audio clip: {clip.Key}");
                var bytes = WavUtility.FromAudioClip(clip.Value);
                var filename = $"{Guid.NewGuid()}.wav";
                var path = $"audio/{filename}";
                Debug.Log($"Uploading audio to path: {path}");

                await _client.Storage
                    .From(BUCKET_NAME)
                    .Upload(bytes, path);

                var url = _client.Storage
                    .From(BUCKET_NAME)
                    .GetPublicUrl(path);
                Debug.Log($"Audio uploaded successfully. URL: {url}");

                assets.Add(new AudioAssetData
                {
                    audio_type = clip.Key,
                    audio_url = url
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to upload audio clip {clip.Key}");
                Debug.LogError($"Error: {e.Message}");
                throw;
            }
        }

        return assets;
    }

    /// <summary>
    /// Downloads and applies a texture to a specific page.
    /// </summary>
    /// <param name="pageNumber">Page number to apply the texture to.</param>
    /// <param name="url">URL of the texture to download.</param>
    private async Task LoadTexture(int pageNumber, string url)
    {
        var bytes = await _client.Storage
            .From(BUCKET_NAME)
            .Download(GetPathFromUrl(url), null);

        var texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        imageController.ApplyImageToPage(texture);
    }

    /// <summary>
    /// Downloads and applies an audio clip to the appropriate audio source.
    /// </summary>
    /// <param name="type">Type of audio (page_turn or background).</param>
    /// <param name="url">URL of the audio to download.</param>
    private async Task LoadAudio(string type, string url)
    {
        var bytes = await _client.Storage
            .From(BUCKET_NAME)
            .Download(GetPathFromUrl(url), null);

        var clip = WavUtility.ToAudioClip(bytes);
        StartCoroutine(audioController.LoadWebAudioClip(type, clip));
    }

    /// <summary>
    /// Extracts the storage path from a public URL.
    /// </summary>
    private string GetPathFromUrl(string url)
    {
        var uri = new Uri(url);
        return uri.AbsolutePath.Split(new[] { "/storage/v1/object/public/" }, StringSplitOptions.None)[1];
    }

    /// <summary>
    /// Collects all page textures from the book component.
    /// </summary>
    private Dictionary<int, Texture2D> GetPageTextures()
    {
        var textures = new Dictionary<int, Texture2D>();
        try
        {
            if (bookComponent?.pageparams == null)
            {
                Debug.LogError("Book component or page parameters are null");
                return textures;
            }

            Debug.Log($"Processing {bookComponent.pageparams.Count} pages");
            for (int i = 0; i < bookComponent.pageparams.Count; i++)
            {
                var param = bookComponent.pageparams[i];
                if (param == null) continue;

                if (param.front is Texture2D frontTexture)
                {
                    textures[i * 2] = frontTexture;
                    Debug.Log($"Added front texture for page {i * 2}");
                }

                if (param.back is Texture2D backTexture)
                {
                    textures[i * 2 + 1] = backTexture;
                    Debug.Log($"Added back texture for page {i * 2 + 1}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error collecting textures: {e.Message}");
        }

        return textures;
    }

    /// <summary>
    /// Collects all audio clips from the audio controller.
    /// </summary>
    /// <returns>Dictionary mapping audio types to their clips.</returns>
    private Dictionary<string, AudioClip> GetAudioClips()
    {
        return new Dictionary<string, AudioClip> {
           { "page_turn", audioController.pageTurnSource?.clip },
           { "background", audioController.backgroundMusicSource?.clip }
       };
    }

    /// <summary>
    /// Applies loaded configuration data to the book component.
    /// Handles conversion of serialized data back to Unity types.
    /// </summary>
    /// <param name="data">Dictionary containing book configuration data.</param>
    private void ApplyBookConfig(Dictionary<string, object> data)
    {
        bookComponent.pageWidth = Convert.ToSingle(data["pageWidth"]);
        bookComponent.pageLength = Convert.ToSingle(data["pageLength"]);
        bookComponent.pageHeight = Convert.ToSingle(data["pageHeight"]);
        bookComponent.bookthickness = Convert.ToSingle(data["bookThickness"]);
        bookComponent.pageGap = Convert.ToSingle(data["pageGap"]);
        bookComponent.spineradius = Convert.ToSingle(data["spineRadius"]);
        bookComponent.NumPages = Convert.ToInt32(data["numPages"]);
        bookComponent.WidthSegs = Convert.ToInt32(data["widthSegs"]);
        bookComponent.LengthSegs = Convert.ToInt32(data["lengthSegs"]);
        bookComponent.HeightSegs = Convert.ToInt32(data["heightSegs"]);
        bookComponent.autoFit = Convert.ToBoolean(data["autoFit"]);

        var autoFitSizeJson = JsonConvert.SerializeObject(data["autoFitSize"]);
        var autoFitSize = JsonConvert.DeserializeObject<SerializableVector3>(autoFitSizeJson);
        bookComponent.autoFitSize = autoFitSize.ToVector3();

        bookComponent.spineScale = Convert.ToSingle(data["spineScale"]);

        var coverScaleJson = JsonConvert.SerializeObject(data["coverScale"]);
        var coverScale = JsonConvert.DeserializeObject<SerializableVector3>(coverScaleJson);
        bookComponent.coverScale = coverScale.ToVector3();
        bookComponent.edgeUVSize = Convert.ToSingle(data["edgeUVSize"]);
        bookComponent.edgeUVOff = Convert.ToSingle(data["edgeUVOffset"]);
    }

    /// <summary>
    /// Connects save functionality to a UI button.
    /// </summary>
    /// <param name="saveButton">Button to connect save functionality to.</param>
    public void ConnectUI(Button saveButton)
    {
        saveButton.clicked += async () =>
        {
            try
            {
                await SaveProject("Book Project"); // Or get name from UI
                Debug.Log("Project saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        };
    }
}

/// <summary>
/// Data structure for page asset information.
/// </summary>
public class PageAssetData
{
    /// <summary>
    /// Page number in the book.
    /// </summary>
    public int page_number { get; set; }

    /// <summary>
    /// URL of the page texture.
    /// </summary>
    public string texture_url { get; set; }
}

/// <summary>
/// Data structure for audio asset information.
/// </summary>
public class AudioAssetData
{
    /// <summary>
    /// Type of audio (page_turn or background).
    /// </summary>
    public string audio_type { get; set; }

    /// <summary>
    /// URL of the audio file.
    /// </summary>
    public string audio_url { get; set; }
}

/// <summary>
/// Database model for page assets.
/// </summary>
public class PageAsset : BaseModel
{
    /// <summary>
    /// ID of the associated book project.
    /// </summary>
    public string book_id { get; set; }

    /// <summary>
    /// Page number in the book.
    /// </summary>
    public int page_number { get; set; }

    /// <summary>
    /// URL of the page texture.
    /// </summary>
    public string texture_url { get; set; }
}

/// <summary>
/// Database model for audio assets.
/// </summary>
public class AudioAsset : BaseModel
{
    /// <summary>
    /// ID of the associated book project.
    /// </summary>
    public string book_id { get; set; }

    /// <summary>
    /// Type of audio (page_turn or background).
    /// </summary>
    public string audio_type { get; set; }

    /// <summary>
    /// URL of the audio file.
    /// </summary>
    public string audio_url { get; set; }
}

/// <summary>
/// Database model for book projects.
/// </summary>
public class BookProject : BaseModel
{
    [Postgrest.Attributes.Column("id")]
    public string id { get; set; }
    
    /// <summary>
    /// Name of the book project.
    /// </summary>
    [Postgrest.Attributes.Column("name")]
    public string name { get; set; }

    /// <summary>
    /// JSON configuration data for the book.
    /// </summary>
    [Postgrest.Attributes.Column("config")]
    public object config { get; set; }

    /// <summary>
    /// URL of the project thumbnail image.
    /// </summary>
    [Postgrest.Attributes.Column("thumbnail_url")]
    public string thumbnail_url { get; set; }

    /// <summary>
    /// Timestamp of project creation.
    /// </summary>
    [Postgrest.Attributes.Column("created_at")]
    public DateTime? created_at { get; set; }

    /// <summary>
    /// Timestamp of last project update.
    /// </summary>
    [Postgrest.Attributes.Column("updated_at")]
    public DateTime? updated_at { get; set; }
}

/// <summary>
/// Utility class for converting between AudioClip and WAV format.
/// </summary>
public static class WavUtility
{
    /// <summary>
    /// Converts an AudioClip to WAV format byte array.
    /// Writes RIFF header and PCM audio data according to WAV specification.
    /// </summary>
    /// <param name="clip">AudioClip to convert.</param>
    /// <returns>Byte array containing WAV data.</returns>
    public static byte[] FromAudioClip(AudioClip clip)
    {
        var data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);

        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + data.Length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(clip.frequency * clip.channels * 2);
                writer.Write((short)(clip.channels * 2));
                writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(data.Length * 2);

                foreach (float sample in data)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return stream.ToArray();
        }
    }

    /// <summary>
    /// Converts WAV format byte array back to Unity AudioClip.
    /// Uses UnityWebRequest to handle audio decoding.
    /// </summary>
    /// <param name="wavData">WAV format byte array.</param>
    /// <returns>Unity AudioClip.</returns>
    public static AudioClip ToAudioClip(byte[] wavData)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("data://." + Convert.ToBase64String(wavData), AudioType.WAV))
        {
            var operation = www.SendWebRequest();
            while (!www.isDone) { }

            if (www.result == UnityWebRequest.Result.Success)
            {
                return DownloadHandlerAudioClip.GetContent(www);
            }
            Debug.LogError($"Failed to load audio clip: {www.error}");
            return null;
        }
    }
}

/// <summary>
/// Serializable wrapper for Vector3 to support JSON conversion.
/// </summary>
public class SerializableVector3
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    /// <summary>
    /// Creates a serializable vector from Unity's Vector3.
    /// </summary>
    /// <param name="vector">Vector3 to convert.</param>
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    /// <summary>
    /// Converts back to Unity's Vector3.
    /// </summary>
    /// <returns>Unity Vector3.</returns>
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}