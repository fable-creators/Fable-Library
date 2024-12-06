mergeInto(LibraryManager.library, {
	/**
	 * Opens a native file picker dialog for image selection.
	 * Sends the selected file's blob URL back to Unity via SendMessage.
	 */
	OpenFilePicker: function () {
		var fileInput = document.createElement("input");
		fileInput.type = "file";
		fileInput.accept = "image/*";
		fileInput.style.display = "none";

		fileInput.onchange = function (e) {
			var file = e.target.files[0];
			var blobUrl = URL.createObjectURL(file);
			
			var canvas = document.querySelector("#unity-canvas");
			if (canvas && canvas.unityInstance) {
				canvas.unityInstance.SendMessage(gameObject.name, "OnImageUploaded", blobUrl);
			} else if (window.unityInstance) {
				window.unityInstance.SendMessage(gameObject.name, "OnImageUploaded", blobUrl);
			} else {
				console.error("No Unity instance found. Make sure the game is fully loaded.");
			}
		};

		fileInput.click();
	},

	/**
	 * Sets up drag and drop event handlers for image files.
	 * Prevents default browser behavior and handles dropped image files.
	 * @param {string} eventName - Name of the event to handle
	 */
	HandleDragEvent: function (eventName) {
		var dropZone = document.getElementById("unity-canvas");
		dropZone.addEventListener('dragover', function(e) {
			e.preventDefault();
			e.stopPropagation();
		});

		dropZone.addEventListener('drop', function(e) {
			e.preventDefault();
			e.stopPropagation();
			
			var file = e.dataTransfer.files[0];
			if (file && file.type.startsWith('image/')) {
				var blobUrl = URL.createObjectURL(file);
				
				var canvas = document.querySelector("#unity-canvas");
				if (canvas && canvas.unityInstance) {
					canvas.unityInstance.SendMessage(gameObject.name, "OnImageUploaded", blobUrl);
				} else if (window.unityInstance) {
					window.unityInstance.SendMessage(gameObject.name, "OnImageUploaded", blobUrl);
				}
			}
		});
	}
});
