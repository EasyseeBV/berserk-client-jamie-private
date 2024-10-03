var SaveTextFilePlugin = {
	download: function(content_ptr, fileName_ptr, contentType_ptr) {
		
		var content = Pointer_stringify(content_ptr);
		var fileName = Pointer_stringify(fileName_ptr);
		var contentType = Pointer_stringify(contentType_ptr);

		var link = document.createElement('a');
		var file = new Blob([content], {type: contentType});
		
		link.href = URL.createObjectURL(file);
		link.download = fileName;
		link.click();
	}
};

mergeInto(LibraryManager.library, SaveTextFilePlugin);
