mergeInto(LibraryManager.library, {
	ShowFileInput: function (gameObjectNamePtr, methodNamePtr, acceptPtr) {
		var gameObjectName = UTF8ToString(gameObjectNamePtr);
		var methodName = UTF8ToString(methodNamePtr);
		var accept = UTF8ToString(acceptPtr);

		var fileInput = document.getElementById("jsonFileInput");
		if (!fileInput) {
			fileInput = document.createElement("input");
			fileInput.setAttribute("type", "file");
			fileInput.setAttribute("id", "jsonFileInput");
			fileInput.style.display = "none";
			document.body.appendChild(fileInput);
		}

		fileInput.setAttribute("accept", accept);
		fileInput.onchange = null;

		fileInput.onchange = function (evt) {
			if (!evt.target.files.length) return;

			var file = evt.target.files[0];
			var reader = new FileReader();

			reader.onload = function (e) {
				var base64 = e.target.result.split(",")[1];
				var canvas = document.querySelector("#unity-canvas");
				if (canvas && canvas.unityInstance) {
					canvas.unityInstance.SendMessage(gameObjectName, methodName, base64);
				} else if (window.unityInstance) {
					window.unityInstance.SendMessage(gameObjectName, methodName, base64);
				}
			};

			reader.readAsDataURL(file);
		};

		fileInput.click();
	},
});
