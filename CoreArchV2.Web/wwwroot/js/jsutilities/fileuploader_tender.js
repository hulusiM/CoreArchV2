$("#imageUploadFormTender").fileinput({//tender
    language: "tr",
    theme: "fas",
    uploadUrl: "/test/test",
    initialPreviewDownloadUrl: "/File/FileDownloadTender",
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

$("#imageUploadFormTenderDetailSingle").fileinput({//single tenderDetail
    language: "tr",
    theme: "fas",
    uploadUrl: "/test/test",
    initialPreviewDownloadUrl: "/File/FileDownloadTender",
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

function setImageTender(imageDivId = "imageUploadFormTender", prevArr, deleteObj) {
    if (prevArr == null)
        prevArr = new Array();

    $("#" + imageDivId).fileinput('destroy');
    $("#" + imageDivId).fileinput({
        language: "tr",
        theme: "fas",
        uploadUrl: "/test/test",
        initialPreviewDownloadUrl: "/File/FileDownloadTender",
        allowedFileExtensions: ["jpg", "jpeg", "png", "pdf", "txt", "xls", "xlsx", "doc", "docx"],
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


