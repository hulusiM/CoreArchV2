$("#imageUploadForm").fileinput({
    language: "tr",
    theme: "fas",
    uploadUrl: "/test/test",
    initialPreviewDownloadUrl: "/File/FileDownloadLogistics",
    allowedFileExtensions: ["jpg","jpeg", "png", "pdf","txt","xls","xlsx","doc","docx"],
    initialPreviewFileType: 'image',
    maxFileCount: 9,
    overwriteInitial: false,
    browseOnZoneClick: true,
    showUpload: false,
    maxFileSize: 5 * 1024,
    msgSizeTooLarge: 'Yüklemeye çalışılan dosya boyutu: (<b>{size} KB</b>)'
        + ' Maksimum <b>5 MB</b> boyutunda dosya yüklebilirsiniz.',
    fileActionSettings: {
        showUpload: false,
        showZoom: true
    },
    initialPreviewAsData: false,
    dropZoneEnabled: false,
    uploadExtraData: {
        testId: "1000",
    }
});

$("#imageVehicleLoadFromUser").fileinput({
    language: "tr",
    theme: "fas",
    uploadUrl: "/test/test",
    initialPreviewDownloadUrl: "/File/FileDownloadUserPhysicalImage",
    allowedFileExtensions: ["jpg", "jpeg", "png", "pdf", "txt", "xls", "xlsx", "doc", "docx"],
    initialPreviewFileType: 'image',
    maxFileCount: 9,
    overwriteInitial: false,
    browseOnZoneClick: true,
    showUpload: false,
    maxFileSize: 5 * 1024,
    msgSizeTooLarge: 'Yüklemeye çalışılan dosya boyutu: (<b>{size} KB</b>)'
        + ' Maksimum <b>5 MB</b> boyutunda dosya yüklebilirsiniz.',
    fileActionSettings: {
        showUpload: false,
        showZoom: true
    },
    initialPreviewAsData: false,
    dropZoneEnabled: false,
    uploadExtraData: {
        testId: "1000",
    }
});

$("#imageUploadFormDELETEVEHICLE").fileinput({
    language: "tr",
    theme: "fas",
    uploadUrl: "/test/test",
    allowedFileExtensions: ["jpg","jpeg", "png", "pdf", "txt", "xls", "xlsx", "doc", "docx"],
    initialPreviewFileType: 'image',
    maxFileCount: 9,
    overwriteInitial: false,
    browseOnZoneClick: true,
    dropZoneEnabled: false,
    showUpload: false,
    maxFileSize: 5 * 1024,
    msgSizeTooLarge: 'Yüklemeye çalışılan dosya boyutu: (<b>{size} KB</b>)'
        + ' Maksimum <b>5 MB</b> boyutunda dosya yüklebilirsiniz.',
    fileActionSettings: {
        showUpload: false,
        showZoom: true
    },
    initialPreviewAsData: false,
    uploadExtraData: {
        testId: "1000",
    }
});

//$('#imageUploadFormDELETEVEHICLE').on('change', function (event) {
//    var filename = event.currentTarget.files[0].name.split('.')[1];
//    var allowedExtentions = $("#imageUploadFormDELETEVEHICLE").data('fileinput').allowedFileExtensions;
//    if ($.inArray(filename.toLowerCase(), allowedExtentions) < 0) 
//        ShowMessage('error', 'Dosya uzantı bildirimi', 'Bu dosya türünü desteklememektedir. Lütfen "jpg","jpeg", "png", "pdf" dosya uzantılarını seçiniz.');
//});

function setImage(prevArr, deleteObj) {
    if (prevArr == null)
        prevArr = new Array();

    $("#imageUploadForm").fileinput('destroy');
    $("#imageUploadForm").fileinput({
        language: "tr",
        theme: "fas",
        uploadUrl: "/test/test",
        initialPreviewDownloadUrl: "/File/FileDownloadLogistics",
        allowedFileExtensions: ["jpg","jpeg", "png", "pdf", "txt", "xls", "xlsx", "doc", "docx"],
        initialPreviewFileType: 'image',
        maxFileCount: 9,
        overwriteInitial: false,
        browseOnZoneClick: true,
        showUpload: false,
        maxFileSize: 5 * 1024,
        msgSizeTooLarge: 'Yüklemeye çalışılan resim boyutu: (<b>{size} KB</b>)'
            + ' Maksimum <b>5 MB</b> boyutunda resim yüklebilirsiniz.',
        fileActionSettings: {
            showUpload: false,
            showZoom: true
        },
        initialPreviewAsData: false,
        initialPreview: prevArr,
        dropZoneEnabled: false,
        initialPreviewConfig: deleteObj,
        uploadExtraData: {
            userId: "1000",
        }
    });
    $('#kvFileinputModal').css("z-index", "9999999");
}

function setImageVehicleLoadFromUser(prevArr, deleteObj) {
    if (prevArr == null)
        prevArr = new Array();

    $("#imageVehicleLoadFromUser").fileinput('destroy');
    $("#imageVehicleLoadFromUser").fileinput({
        language: "tr",
        theme: "fas",
        uploadUrl: "/test/test",
        initialPreviewDownloadUrl: "/File/FileDownloadUserPhysicalImage",
        allowedFileExtensions: ["jpg", "jpeg", "png"],
        initialPreviewFileType: 'image',
        maxFileCount: 9,
        overwriteInitial: false,
        browseOnZoneClick: true,
        showUpload: false,
        maxFileSize: 5 * 1024,
        msgSizeTooLarge: 'Yüklemeye çalışılan resim boyutu: (<b>{size} KB</b>)'
            + ' Maksimum <b>5 MB</b> boyutunda resim yüklebilirsiniz.',
        fileActionSettings: {
            showUpload: false,
            showZoom: true
        },
        initialPreviewAsData: false,
        initialPreview: prevArr,
        dropZoneEnabled: false,
        initialPreviewConfig: deleteObj,
        uploadExtraData: {
            userId: "1000",
        }
    });
    $('#kvFileinputModal').css("z-index", "9999999");
}

function setImageDeleteVehicle(prevArr, deleteObj) {
    if (prevArr == null)
        prevArr = new Array();

    $("#imageUploadFormDELETEVEHICLE").fileinput('destroy');
    $("#imageUploadFormDELETEVEHICLE").fileinput({
        language: "tr",
        theme: "fas",
        uploadUrl: "/test/test",
        initialPreviewDownloadUrl: "/File/FileDownloadLogistics",
        allowedFileExtensions: ["jpg","jpeg", "png", "pdf", "txt", "xls", "xlsx", "doc", "docx"],
        initialPreviewFileType: 'image',
        maxFileCount: 9,
        overwriteInitial: false,
        browseOnZoneClick: true,
        showUpload: false,
        maxFileSize: 5 * 1024,
        msgSizeTooLarge: 'Yüklemeye çalışılan resim boyutu: (<b>{size} KB</b>)'
            + ' Maksimum <b>5 MB</b> boyutunda resim yüklebilirsiniz.',
        fileActionSettings: {
            showUpload: false,
            showZoom: true
        },
        initialPreviewAsData: false,
        initialPreview: prevArr,
        dropZoneEnabled: false,
        initialPreviewConfig: deleteObj,
        uploadExtraData: {
            userId: "1000",
        }
    });
    $('#kvFileinputModal').css("z-index", "9999999");
}

function reloadFileZone(zoneId) {
    $("#" + zoneId).fileinput('destroy');
    $("#" + zoneId).fileinput({
        language: "tr",
        theme: "fas",
        uploadUrl: "/test/test",
        initialPreviewDownloadUrl: "/File/FileDownloadLogistics",
        allowedFileExtensions: ["jpg","jpeg", "png", "pdf", "txt", "xls", "xlsx", "doc", "docx"],
        initialPreviewFileType: 'image',
        maxFileCount: 9,
        overwriteInitial: false,
        browseOnZoneClick: true,
        showUpload: false,
        maxFileSize: 5 * 1024,
        msgSizeTooLarge: 'Yüklemeye çalışılan resim boyutu: (<b>{size} KB</b>)'
            + ' Maksimum <b>5 MB</b> boyutunda resim yüklebilirsiniz.',
        fileActionSettings: {
            showUpload: false,
            showZoom: true
        },
        initialPreviewAsData: false,
        dropZoneEnabled: false,
        initialPreview: [],
        initialPreviewDownloadUrl: "/File/FileDownloadLogistics",
        initialPreviewConfig: [],
        uploadExtraData: {
            userId: "1000",
        }
    });
}

