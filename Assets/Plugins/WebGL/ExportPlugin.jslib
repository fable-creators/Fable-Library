/**
 * WebGL plugin for handling file downloads in Unity.
 * Provides functionality to trigger file downloads from Unity to the user's browser.
 */
mergeInto(LibraryManager.library, {
    /**
     * Downloads a file to the user's browser.
     * Creates a temporary link element, triggers the download, and cleans up.
     * 
     * @param {string} filename - The name of the file to be downloaded
     * @param {string} data - Base64 encoded data to be downloaded
     * 
     * @example
     * // In Unity C#:
     * [DllImport("__Internal")]
     * private static extern void DownloadFile(string filename, string data);
     */
    DownloadFile: function (filename, data) {
        var filenameStr = UTF8ToString(filename);
        var dataStr = UTF8ToString(data);
        
        // Create temporary link element
        var link = document.createElement('a');
        link.download = filenameStr;
        link.href = 'data:application/json;base64,' + dataStr;
        
        // Add to document, trigger download, and cleanup
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
});