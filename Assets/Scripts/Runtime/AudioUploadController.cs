using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using UnityEngine.Networking;
using System.Collections;
using System.Runtime.InteropServices;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controls audio upload and playback functionality for page turn sounds and background music.
/// Handles both WebGL and Editor environments with different upload mechanisms.
/// </summary>
public class AudioUploadController : MonoBehaviour
{
    // Required for WebGL file input functionality
    [DllImport("__Internal")] private static extern void ShowFileInput(string gameObjectName, string methodName, string accept);


    /// <summary>
    /// UI Document containing the audio control elements.
    /// </summary>
    [SerializeField] private UIDocument document;
    /// <summary>
    /// Audio source for playing page turn sound effects.
    /// </summary>
    [SerializeField] public AudioSource pageTurnSource;
    /// <summary>
    /// Audio source for playing background music.
    /// </summary>
    [SerializeField] public AudioSource backgroundMusicSource;

    private DropdownField pageTurnSoundDropdown;
    private DropdownField backgroundMusicDropdown;
    private Button uploadSoundButton;
    private Button uploadMusicButton;
    private Slider effectsVolumeSlider;
    private Slider musicVolumeSlider;
    private Label pageTurnFileName;
    private Label backgroundMusicFileName;

    /// <summary>
    /// Default page turn sound loaded from Resources folder.
    /// </summary>
    public AudioClip defaultPageTurnSound;
    /// <summary>
    /// Custom uploaded page turn sound clip.
    /// </summary>
    public AudioClip customPageTurnSound;
    /// <summary>
    /// Custom uploaded background music clip.
    /// </summary>
    public AudioClip customBackgroundMusic;

    private bool isAudioInitialized = false;  // WebGL requires user interaction before playing audio

    private void OnEnable()
    {
        if (document == null) document = GetComponent<UIDocument>();
        SetupAudioSources();
        SetupUI();
        RegisterCallbacks();
    }

    private void SetupAudioSources()
    {
        if (pageTurnSource == null)
        {
            pageTurnSource = gameObject.AddComponent<AudioSource>();
            pageTurnSource.playOnAwake = false;
        }

        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
        }

        LoadDefaultSounds();
    }

    private void LoadDefaultSounds()
    {
        defaultPageTurnSound = Resources.Load<AudioClip>("Audio/DefaultPageTurn");
        if (defaultPageTurnSound != null)
        {
            pageTurnSource.clip = defaultPageTurnSound;
        }
    }

    private void SetupUI()
    {
        var root = document.rootVisualElement;

        pageTurnSoundDropdown = root.Q<DropdownField>("pageTurnSoundDropdown");
        backgroundMusicDropdown = root.Q<DropdownField>("backgroundMusicDropdown");
        uploadSoundButton = root.Q<Button>("uploadSoundButton");
        uploadMusicButton = root.Q<Button>("uploadMusicButton");
        effectsVolumeSlider = root.Q<Slider>("effectsVolumeSlider");
        musicVolumeSlider = root.Q<Slider>("musicVolumeSlider");
        pageTurnFileName = root.Q<Label>("pageTurnFileName");
        backgroundMusicFileName = root.Q<Label>("backgroundMusicFileName");

        if (effectsVolumeSlider != null)
        {
            effectsVolumeSlider.value = 0.5f;
            pageTurnSource.volume = 0.5f;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.5f;
            backgroundMusicSource.volume = 0.5f;
        }
    }

    private void RegisterCallbacks()
    {
        if (uploadSoundButton != null)
            uploadSoundButton.clicked += () =>
            {
                if (!isAudioInitialized)
                {
                    StartCoroutine(InitializeWebAudio());
                }
                OpenAudioFileDialog(true);
            };

        if (uploadMusicButton != null)
            uploadMusicButton.clicked += () =>
            {
                if (!isAudioInitialized)
                {
                    StartCoroutine(InitializeWebAudio());
                }
                OpenAudioFileDialog(false);
            };

        if (effectsVolumeSlider != null)
            effectsVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                if (pageTurnSource != null)
                {
                    pageTurnSource.volume = evt.newValue;
                    PlayPageTurnSound();
                }
            });

        if (musicVolumeSlider != null)
            musicVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                if (backgroundMusicSource != null) backgroundMusicSource.volume = evt.newValue;
            });

        if (pageTurnSoundDropdown != null)
        {
            pageTurnSoundDropdown.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case "Default":
                        if (defaultPageTurnSound != null)
                        {
                            pageTurnSource.clip = defaultPageTurnSound;
                            PlayPageTurnSound();
                        }
                        break;
                    case "Custom":
                        if (customPageTurnSound != null)
                        {
                            pageTurnSource.clip = customPageTurnSound;
                            PlayPageTurnSound();
                        }
                        break;
                    case "None":
                        pageTurnSource.Stop();
                        pageTurnSource.clip = null;
                        break;
                }
            });
        }

        if (backgroundMusicDropdown != null)
        {
            backgroundMusicDropdown.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case "None":
                        backgroundMusicSource.Stop();
                        backgroundMusicSource.clip = null;
                        break;
                    case "Custom":
                        if (customBackgroundMusic != null)
                        {
                            backgroundMusicSource.clip = customBackgroundMusic;
                            backgroundMusicSource.Play();
                        }
                        break;
                }
            });
        }
    }

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Start with audio paused in WebGL
        AudioListener.pause = true;
#endif
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Initialize audio on first user interaction
        StartCoroutine(InitializeWebAudio());
#endif
    }

    private IEnumerator InitializeWebAudio()
    {
        // Ensures audio playback only starts after user interaction in WebGL
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.anyKeyDown);
        AudioListener.pause = false;
        isAudioInitialized = true;

        // Resume any pending background music
        if (backgroundMusicSource != null && backgroundMusicSource.clip != null &&
            backgroundMusicDropdown.value == "Custom")
        {
            backgroundMusicSource.Play();
        }
    }

    /// <summary>
    /// Plays the currently assigned page turn sound if audio is initialized.
    /// </summary>
    public void PlayPageTurnSound()
    {
        if (pageTurnSource != null && pageTurnSource.clip != null && isAudioInitialized)
        {
            pageTurnSource.PlayOneShot(pageTurnSource.clip);
        }
    }

    /// <summary>
    /// Callback for when a page turn sound file is selected in WebGL.
    /// </summary>
    /// <param name="base64Data">Base64 encoded audio data or blob URL.</param>
    public void OnPageTurnFileSelected(string base64Data)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (AudioListener.pause)
            {
                StartCoroutine(InitializeWebAudio());
            }
#endif
        StartCoroutine(LoadWebAudioClip(base64Data, true));
    }

    /// <summary>
    /// Opens a file dialog for audio file selection.
    /// Uses EditorUtility in Editor and JavaScript bridge in WebGL.
    /// </summary>
    /// <param name="isPageTurnSound">True for page turn sound, false for background music.</param>
    private void OpenAudioFileDialog(bool isPageTurnSound)
    {
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Select Audio", "", "mp3,wav,ogg");
            if (!string.IsNullOrEmpty(path))
            {
                StartCoroutine(LoadAudioClip(path, isPageTurnSound));
            }
#elif UNITY_WEBGL
            ShowFileInput(gameObject.name, isPageTurnSound ? "OnPageTurnFileSelected" : "OnBackgroundMusicFileSelected", ".mp3,.wav,.ogg");
#endif
    }

    /// <summary>
    /// Callback for when a background music file is selected in WebGL.
    /// </summary>
    /// <param name="base64Data">Base64 encoded audio data or blob URL.</param>
    public void OnBackgroundMusicFileSelected(string base64Data)
    {
        StartCoroutine(LoadWebAudioClip(base64Data, false));
    }

    /// <summary>
    /// Loads an audio clip from a blob URL in WebGL.
    /// </summary>
    /// <param name="blobUrl">URL of the audio blob.</param>
    /// <param name="isPageTurnSound">True for page turn sound, false for background music.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public IEnumerator LoadWebAudioClip(string blobUrl, bool isPageTurnSound)
    {
        // Determines audio format from URL and loads it via UnityWebRequest
        AudioType audioType = AudioType.UNKNOWN;
        if (blobUrl.Contains(".mp3")) audioType = AudioType.MPEG;
        else if (blobUrl.Contains(".wav")) audioType = AudioType.WAV;
        else if (blobUrl.Contains(".ogg")) audioType = AudioType.OGGVORBIS;
        else audioType = AudioType.MPEG; // Fallback format

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(blobUrl, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    if (isPageTurnSound)
                    {
                        customPageTurnSound = clip;
                        pageTurnSource.clip = clip;
                        pageTurnSoundDropdown.value = "Custom";
                        pageTurnFileName.text = "Custom Page Turn Sound";

                        if (isAudioInitialized)
                        {
                            PlayPageTurnSound();
                        }
                    }
                    else
                    {
                        customBackgroundMusic = clip;
                        backgroundMusicSource.clip = clip;
                        backgroundMusicFileName.text = "Custom Background Music";
                        backgroundMusicDropdown.value = "Custom";

                        if (isAudioInitialized)
                        {
                            backgroundMusicSource.Play();
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to create AudioClip from data");
                }
            }
            else
            {
                Debug.LogError($"Error loading audio: {www.error}");
            }
        }
    }

    /// <summary>
    /// Handles a loaded audio clip and assigns it to the appropriate source.
    /// </summary>
    /// <param name="clip">The loaded AudioClip.</param>
    /// <param name="isPageTurnSound">True for page turn sound, false for background music.</param>
    public void HandleLoadedAudioClip(AudioClip clip, bool isPageTurnSound)
    {
        if (isPageTurnSound)
        {
            customPageTurnSound = clip;
            pageTurnSource.clip = clip;
            pageTurnSoundDropdown.value = "Custom";
            if (pageTurnFileName != null)
                pageTurnFileName.text = "Custom Page Turn";

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                StartCoroutine(WaitForInteractionAndPlay());
            else
                PlayPageTurnSound();
        }
        else
        {
            customBackgroundMusic = clip;
            backgroundMusicSource.clip = clip;
            backgroundMusicDropdown.value = "Custom";
            if (backgroundMusicFileName != null)
                backgroundMusicFileName.text = "Custom Music";

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                StartCoroutine(WaitForInteractionAndPlay(false));
            else
                backgroundMusicSource.Play();
        }
    }

    /// <summary>
    /// Waits for user interaction before playing audio in WebGL.
    /// </summary>
    /// <param name="isPageTurn">True to play page turn sound, false for background music.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    private IEnumerator WaitForInteractionAndPlay(bool isPageTurn = true)
    {
        // WebGL-specific handler to delay audio playback until after user interaction
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.anyKeyDown);

        if (isPageTurn)
        {
            PlayPageTurnSound();
        }
        else if (backgroundMusicSource != null && backgroundMusicSource.clip != null)
        {
            backgroundMusicSource.Play();
        }
    }

    /// <summary>
    /// Loads an audio clip from a local file path in the Editor.
    /// </summary>
    /// <param name="path">Path to the audio file.</param>
    /// <param name="isPageTurnSound">True for page turn sound, false for background music.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    private IEnumerator LoadAudioClip(string path, bool isPageTurnSound)
    {
        // Editor-only method to load audio from local filesystem
        // Supports MP3, WAV, and OGG formats

        string extension = Path.GetExtension(path).ToLower();
        AudioType audioType = AudioType.UNKNOWN;

        switch (extension)
        {
            case ".mp3": audioType = AudioType.MPEG; break;
            case ".wav": audioType = AudioType.WAV; break;
            case ".ogg": audioType = AudioType.OGGVORBIS; break;
        }

        if (audioType == AudioType.UNKNOWN)
        {
            Debug.LogError("Unsupported audio format");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                clip.name = Path.GetFileName(path);

                if (isPageTurnSound)
                {
                    customPageTurnSound = clip;
                    pageTurnSource.clip = clip;
                    pageTurnSoundDropdown.value = "Custom";
                    if (pageTurnFileName != null)
                        pageTurnFileName.text = clip.name;
                    PlayPageTurnSound();
                }
                else
                {
                    customBackgroundMusic = clip;
                    backgroundMusicSource.clip = clip;
                    backgroundMusicSource.Play();
                    backgroundMusicDropdown.value = "Custom";
                    if (backgroundMusicFileName != null)
                        backgroundMusicFileName.text = clip.name;
                }
            }
            else
            {
                Debug.LogError($"Error loading audio clip: {www.error}");
            }
        }
    }

    private void OnDisable()
    {
        if (backgroundMusicSource != null)
            backgroundMusicSource.Stop();
    }
}