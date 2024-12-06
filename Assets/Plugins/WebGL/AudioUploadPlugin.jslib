/**
 * WebGL plugin for handling audio file uploads in Unity.
 * Provides functionality to open file input dialogs and handle audio file selection.
 */
mergeInto(LibraryManager.library, {
	/**
	 * Shows a file input dialog for audio file selection.
	 * Creates or reuses a file input element and handles the file selection.
	 * 
	 * @param {string} gameObjectNamePtr - Pointer to the name of the Unity GameObject to receive callbacks
	 * @param {string} methodNamePtr - Pointer to the name of the method to call with the result
	 * @param {string} acceptPtr - Pointer to the string containing accepted file types
	 * 
	 * @example
	 * // In Unity C#:
	 * [DllImport("__Internal")]
	 * private static extern void ShowFileInput(string gameObjectName, string methodName, string accept);
	 */
	ShowFileInput: function (gameObjectNamePtr, methodNamePtr, acceptPtr) {
		var gameObjectName = UTF8ToString(gameObjectNamePtr);
		var methodName = UTF8ToString(methodNamePtr);
		var accept = UTF8ToString(acceptPtr);

		var fileInput = document.getElementById("audioFileInput");
		if (!fileInput) {
			fileInput = document.createElement("input");
			fileInput.setAttribute("type", "file");
			fileInput.setAttribute("id", "audioFileInput");
			fileInput.style.display = "none";
			document.body.appendChild(fileInput);
		}

		fileInput.setAttribute("accept", accept);
		fileInput.onchange = null;

		fileInput.onchange = function (evt) {
			if (!evt.target.files.length) return;

			var file = evt.target.files[0];
			var blobUrl = URL.createObjectURL(file);

			var canvas = document.querySelector("#unity-canvas");
			if (canvas && canvas.unityInstance) {
				canvas.unityInstance.SendMessage(gameObjectName, methodName, blobUrl);
			} else if (window.unityInstance) {
				window.unityInstance.SendMessage(gameObjectName, methodName, blobUrl);
			} else {
				console.error(
					"No Unity instance found. Make sure the game is fully loaded."
				);
			}
		};

		fileInput.click();
	},
});
