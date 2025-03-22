$(document).on('shown.bs.modal', '.modal', function () {
    $(this).find('.select-search').not('.dropdown-menu .select-search').select2({
        dropdownParent: $(this)
    });
});

function getDivComponentValues(divId) {
    var paramsFromForm = {};
    $.each($("#" + divId).serializeArray(), function (index, value) {
        paramsFromForm[value.name] = paramsFromForm[value.name] ? paramsFromForm[value.name] || value.value : value.value;
    });

    return paramsFromForm;
}

function ArrayInSearch(arr, obj) {
    for (var i = 0; i < arr.length; i++) {
        if (arr[i] == obj) return true;
    }
}

$('.phone-number-control').on('keypress', function (e) {
    var key = e.charCode || e.keyCode || 0;
    var phone = $(this);

    if (!(event.shiftKey == false && (key == 46 || key == 8 ||
        key == 37 || key == 39 || (key >= 48 && key <= 57)))) {
        event.preventDefault();
        ShowMessage("info", "Bilgi", "Sadece numara girilebilir");
    }

    // Allow numeric (and tab, backspace, delete) keys only
    return (key == 8 ||
        key == 9 ||
        key == 46 ||
        (key >= 48 && key <= 57) ||
        (key >= 96 && key <= 105));
}).on('focus', function () {
    phone = $(this);
}).on('blur', function () {
    $phone = $(this);

});

$('.phone-format').keydown(function (e) {
    var key = e.which || e.charCode || e.keyCode || 0;
    $phone = $(this);

    if ($phone.val().length === 1 && (key === 8 || key === 46)) {
        $phone.val('(');
        return false;
    } else if ($phone.val().charAt(0) !== '(') {
        $phone.val('(' + $phone.val());
    }

    if (key !== 8 && key !== 9) {
        if ($phone.val().length === 4) {
            $phone.val($phone.val() + ')');
        }
        if ($phone.val().length === 5) {
            $phone.val($phone.val() + ' ');
        }
        if ($phone.val().length === 9) {
            $phone.val($phone.val() + '-');
        }
    }

    return (key == 8 ||
        key == 9 ||
        key == 46 ||
        (key >= 48 && key <= 57) ||
        (key >= 96 && key <= 105));
}).keyup(function (e) {
    $phone = $(this);

    if ($phone.val().length == 2 && $phone.val() == "(0")
        return $phone.val('(');

}).bind('focus click', function () {
    $phone = $(this);

    if ($phone.val().length === 0) {
        $phone.val('(');
    }
    else {
        var val = $phone.val();
        $phone.val('').val(val);
    }
}).blur(function () {
    $phone = $(this);

    if ($phone.val() === '(') {
        $phone.val('');
    }
});

// verilen class içindeki active class'ları temizler
function activeLiRemove(className) {
    var cls = className;
    var elems = document.querySelectorAll(cls);
    [].forEach.call(elems, function (el) {
        el.classList.remove("active");
    });
}

// String koordinatları array dizisine çevirir.
function CoordinateStringToArray(coordString) {
    var coords = coordString.split(",");
    var arr = [];
    for (var i = 0; i < coords.length; i++) {
        if (i == 0)
            arr.push(coords[i]);
        else
            arr.push(parseFloat(coords[i]));
    }
    return arr;
}

function stringToIntArray(arr) {
    var result = arr.split(',').map(function (item) {
        return parseInt(item, 10);
    });
    return result;
}

// date.addDays(day) 12/12/2008
Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return (date.getDate().toString() +
        '/' +
        (date.getMonth() + 1).toString() +
        '/' +
        date.getFullYear().toString());
};

// Ajax form model gönderir
function sendAjxForm(model, url, func, type, loadingEffect = true) {
    if (url != "") {
        $.ajax({
            url: window.location.origin + url,
            type: type,
            dataType: "json",
            async: true,
            beforeSend: function (x) {
                if (loadingEffect)
                    $('#loadingEffect').css("display", "flex");
                if (x && x.overrideMimeType) {
                    x.overrideMimeType("application/json;charset=UTF-8");
                };
                //x.setRequestHeader('RequestVerificationToken', document.getElementById('RequestVerificationToken').value);
            },
            success: function (data) {
                func(data);
                if (loadingEffect)
                    $('#loadingEffect').css("display", "none");
            },
            data: model,
            error: function (err) {
                if (loadingEffect)
                    $('#loadingEffect').css("display", "none");
                if (err.responseText == 'NotAuthorizationCore') {
                    var message = "Bu işlemi yapmaya yetkiniz yok. Adminle iletişime geçiniz";
                    ShowMessage("error", "Yetkisiz İşlem", message);
                }
            }
        });
    }
}

function sendAjxFormFileUpload(model, url, func, type, funcErr = null) {
    if (url != "") {
        $.ajax(
            {
                url: url,
                type: type,
                data: model,
                async: false,
                processData: false,
                contentType: false,
                success: func,
                timeout: 50000,
                error: function (err) {
                    if (funcErr != null)
                        funcErr();
                    else {
                        $('#loadingEffect').css("display", "none");
                        if (err.responseText == 'NotAuthorizationCore') {
                            var message = "Bu işlemi yapmaya yetkiniz yok. Adminle iletişime geçiniz";
                            ShowMessage("error", "Yetkisiz İşlem", message);
                        } //else
                        //ShowMessage("warning", "Bilgi", "Hata oluştu");
                    }
                }
            }
        );
    }
}

function sendAjxFormAsyncFalse(model, url, func, type) {
    if (url != "") {
        $.ajax({
            url: window.location.origin + url,
            type: type,
            async: false,
            dataType: "json",
            beforeSend: function (x) {
                if (x && x.overrideMimeType) {
                    x.overrideMimeType("application/json;charset=UTF-8");
                };
                //x.setRequestHeader('RequestVerificationToken', document.getElementById('RequestVerificationToken').value);
            },
            success: func,
            data: model,
        });
    }
}

//Sayfaya verile url adresindeki js dosyalarını yükler. Örnek:    dynamicallyLoadScript("/assets_limitless/js/pages/form_input_groups.js");
function dynamicallyLoadScript(url) {
    var script = document.createElement("script");  // create a script DOM node
    script.src = url;  // set its src to the provided URL
    document.head.appendChild(script);  // add it to the end of the head section of the page (could change 'head' to 'body' to add it to the end of the body section instead)
}

function formatNumber(num) {
    return num.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1.');
}

function numberWithCommas(x) {
    var parts = x.toString().split(".");
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return parts.join(".");
}

// Verilen değere göre küçükten büyüğe sıralar
function JavascriptSortByKey(array, key) {
    return array.sort(function (a, b) {
        var x = parseFloat(a[key]);
        var y = parseFloat(b[key]);
        return ((x < y) ? -1 : ((x > y) ? 1 : 0));
    });
}

// var groupCodeGroupBy = JavascriptGroupBy(data, "GroupCode");
function JavascriptGroupBy(collection, property) {
    var i = 0,
        val,
        index,
        values = [],
        result = [];
    for (; i < collection.length; i++) {
        val = collection[i][property];
        index = values.indexOf(val);
        if (index > -1)
            result[index].push(collection[i]);
        else {
            values.push(val);
            result.push([collection[i]]);
        }
    }
    return result;
}

//JavascriptSum(groupCodeGroupBy[i], "AmountTotal")
function JavascriptSum(collection, property) {
    var sum = 0.0;
    for (var i = 0; i < collection.length; i++) {
        sum = parseFloat(sum) + parseFloat(collection[i][property]);
    }
    return sum.toFixed(2);
}

function ShowMessage(messageType, title, content, duration = "5000") {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "500",
        "hideDuration": "1000",
        "timeOut": duration,
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
    Command: toastr[messageType](content, title);
}

//--------------------Combobox fill--------------------- //
var Add = [];
var ComboCount = 0;
var ComboHtmlCont = "";
var dataLabel = "";
var elementId = "";
for (var i = 0; i < $("select[table-tool-type='comboBox']").length; i++) {
    var elementUrl = $("select[table-tool-type='comboBox']")[i].attributes["table-model-url"].value;
    dataLabel = $("select[table-tool-type='comboBox']")[i].attributes["data-option-label"].value;
    elementId = $("select[table-tool-type='comboBox']")[i].attributes["id"].value;
    if (elementUrl != "")
        sendAjxFormAsyncFalse("", elementUrl, funcComboBox, "POST"); //async=false olması gerekiyor
    else {
        ComboHtmlCont = "<option value=''>" + dataLabel + "</option>";
        $("#" + elementId).append(ComboHtmlCont);
        dataLabel = "";
    }
}

function funcComboBox(e) {
    ComboHtmlCont = "";
    if (dataLabel != "")
        ComboHtmlCont += "<option value=''>" + dataLabel + "</option>";

    for (var j = 0; j < e.length; j++)
        ComboHtmlCont += "<option value='" + e[j].Id + "'>" + e[j].Name + "</option>";

    $("#" + elementId).append(ComboHtmlCont);
    ComboHtmlCont = "";
}

function funcCustomComboBox(e, ComboId, HeaderName) { // Example --> funcCustomComboBox(e, "MemberId33", "Kullanıcı Seçiniz");
    $('#' + ComboId).empty();
    var htmlCombo = "<option value=''>" + HeaderName + "</option>";
    for (var j = 0; j < e.length; j++)
        htmlCombo += "<option value='" + e[j].Id + "'>" + e[j].Name + "</option>";

    $("#" + ComboId).append(htmlCombo);
    htmlCombo = "";
}

function funcCustomComboBoxSingle(comboId, id, optionValue) { // Example --> funcCustomComboBoxSingle("MemberId33",5,"Beyaz");
    $('#' + comboId).empty();
    var htmlCombo = "<option value='" + id + "'>" + optionValue + "</option>";

    $("#" + comboId).append(htmlCombo);
}

function reloadCombobox(url, model, comboId, headerName) {
    $('#' + comboId + " :first-child").nextAll().remove();
    sendAjxForm(model, url, function (e) {
        if (e.length > 0) {
            funcCustomComboBox(e, comboId, headerName);
            setTimeout(function () {
                $('#UnitId').val('').trigger('change')
            }, 2000);
        }
    }, "POST");
}

//-----------------------------------------------------------//

function FormGetAllValue(e) {
    htmlTag = $("" + e + "");
    var model = {};
    for (var i = 0; i < htmlTag.length; i++) {
        var isRequired = htmlTag[i].required;
        var valueInput = htmlTag[i].value;
        var isHidden = htmlTag[i].hidden;
        if (valueInput == 0)
            valueInput = "";

        if (valueInput == "" && isRequired) {
            model = null;
            var message = "<b style='color:red;'>" +
                htmlTag[i].attributes["table-description"].value +
                "</b> alanı boş geçilemez.";
            ShowMessage("info", "Zorunlu Alan Bildirimi", message);
            return model;
        } else {
            if (htmlTag[i].type == "checkbox")
                model[htmlTag[i].attributes["table-param"].value] = $('#' + htmlTag[i].id).is(':checked');
            else if (htmlTag[i].type == "text" ||
                htmlTag[i].type == "number" ||
                htmlTag[i].type == "textarea" ||
                htmlTag[i].type == "date" ||
                htmlTag[i].type == "hidden" ||
                htmlTag[i].type == "month" ||
                htmlTag[i].type == "datetime-local" ||
                htmlTag[i].type == "password") {
                if (htmlTag[i].className.indexOf('moneyCurrency') > 0 && htmlTag[i].value != '') {
                    var first = htmlTag[i].value.split(',')[0];
                    var second = htmlTag[i].value.split(',')[1];
                    if (second == undefined || second == '' || second == null)
                        second = '00';
                    model[htmlTag[i].attributes["table-param"].value] = first.replaceAll('.', '') + ',' + second;
                }
                else
                    model[htmlTag[i].attributes["table-param"].value] = htmlTag[i].value;
            } else if (htmlTag[i].localName == "select") {
                if (htmlTag[i].type == "select-multiple") {
                    var data = [];
                    for (var j = 0; j < htmlTag[i].selectedOptions.length; j++)
                        data.push(htmlTag[i].selectedOptions[j].value);

                    model[htmlTag[i].attributes["table-param"].value] = data;
                } else
                    model[htmlTag[i].attributes["table-param"].value] = htmlTag[i].value;
            } else if (htmlTag[i].type == "checkbox" && htmlTag[i].className.indexOf("make-switch") > -1 ||
                htmlTag[i].className.indexOf("switch") > -1)
                model[htmlTag[i].attributes["table-param"].value] = htmlTag[i].checked;
        }
    }
    return model;
}

//Dosya ve model dataları append eder
function FormDataAppendModelValue(formdata, e) {
    htmlTag = $("" + e + "");
    for (var i = 0; i < htmlTag.length; i++) {
        if (htmlTag[i].type == "checkbox")
            formdata.append(htmlTag[i].attributes["table-param"].value, $('#' + htmlTag[i].id).is(':checked'));
        else if (htmlTag[i].type == "text" ||
            htmlTag[i].type == "number" ||
            htmlTag[i].type == "textarea" ||
            htmlTag[i].type == "date" ||
            htmlTag[i].type == "month" ||
            htmlTag[i].type == "datetime-local" ||
            htmlTag[i].type == "password") {
            if (htmlTag[i].className.indexOf('moneyCurrency') > 0) {
                var first = htmlTag[i].value.split(',')[0];
                var second = htmlTag[i].value.split(',')[1];
                if (second == undefined || second == '' || second == null)
                    second = '00';
                formdata.append([htmlTag[i].attributes["table-param"].value], first.replaceAll('.', '') + ',' + second);
            }
            else
                formdata.append(htmlTag[i].attributes["table-param"].value, htmlTag[i].value);
        } else if (htmlTag[i].localName == "select") {
            if (htmlTag[i].type == "select-multiple") {
                var data = [];
                for (var j = 0; j < htmlTag[i].selectedOptions.length; j++)
                    data.push(htmlTag[i].selectedOptions[j].value);

                formdata.append(htmlTag[i].attributes["table-param"].value, data);
            } else
                formdata.append(htmlTag[i].attributes["table-param"].value, htmlTag[i].value);
        } else if (htmlTag[i].type == "checkbox" && htmlTag[i].className.indexOf("make-switch") > -1 ||
            htmlTag[i].className.indexOf("switch") > -1)
            formdata.append(htmlTag[i].attributes["table-param"].value, htmlTag[i].checked);
    }
    return formdata;
}

function FormSetAllValue(e) {
    htmlTag = $("" + e.selector + "");
    for (var i = 0; i < htmlTag.length; i++) {
        if (htmlTag[i].localName == "select") {
            e.data[htmlTag[i].attributes["table-param"].value] =
                e.data[htmlTag[i].attributes["table-param"].value] == null ? "null" : e.data[htmlTag[i].attributes["table-param"].value];
            if (e.data[htmlTag[i].attributes["table-param"].value] == "null" || e.data[htmlTag[i].attributes["table-param"].value] == '')//select değeri boşsa temizle
                $('#' + htmlTag[i].id).val('').change();
            else if (htmlTag[i].className.indexOf("select2") > -1) {
                if (htmlTag[i].type == "select-one") {
                    if (htmlTag[i].className.indexOf("select-remote-data") > -1)
                        $(htmlTag[i]).select2("val",
                            e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : 'All');
                    else {
                        $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : 'null').trigger('change');
                    }
                } else if (htmlTag[i].type == "select-multiple") {
                    $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : '')
                        .trigger('change'); //$("#CityId").val([1, 2,5]).trigger('change')
                } else {
                    $(htmlTag[i]).select2()
                        .val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : 'null')
                        .trigger('change');
                }
            } else {
                if (htmlTag[i].attributes["data-nullable"] != undefined) {
                    if (htmlTag[i].attributes["data-nullable"].value != "true")
                        $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : 'null')
                            .trigger('change');
                    else
                        $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : 0)
                            .trigger('change');
                } else
                    $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : 'null')
                        .trigger('change');
            }
        } else if (htmlTag[i].type == "text") {
            if (htmlTag[i].parentNode.className.indexOf("date-picker") != -1)
                $(htmlTag[i]).datepicker('setDate',
                    (e.data != "" ? ConvertJSONDate(e.data[htmlTag[i].attributes["table-param"].value]) : ""));
            else if (htmlTag[i].className.indexOf("datetime") != -1) {
                $(htmlTag[i])
                    .val(e.data != ''
                        ? ConvertJSONDateWithMonthAndTimeNumber(e.data[htmlTag[i].attributes['table-param'].value])
                        : '').trigger('change');
            }
            else if (htmlTag[i].className.indexOf("date") != -1) {
                $(htmlTag[i])
                    .val(e.data != ''
                        ? formatDate(e.data[htmlTag[i].attributes['table-param'].value])
                        : '').trigger('change');
            } else {
                if (htmlTag[i].className.indexOf("moneyCurrency") > -1)
                    $(htmlTag[i]).val(e.data != "" ? (e.data[htmlTag[i].attributes["table-param"].value] != null ? formatMoney(e.data[htmlTag[i].attributes["table-param"].value]) : '') : "")
                        .trigger('change');
                else
                    $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : "")
                        .trigger('change');
            }
        } else if (htmlTag[i].type == "textarea") {
            $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : "").trigger('change');
        } else if (htmlTag[i].type == "checkbox") {
            if (htmlTag[i].className.indexOf("make-switch") > -1 || htmlTag[i].className.indexOf("switch") > -1)
                $(htmlTag[i]).bootstrapSwitch('state',
                    e.data != ""
                        ? e.data[htmlTag[i].attributes["table-param"].value]
                        : ($(htmlTag[i])[0].defaultValue != "")
                            ? $(htmlTag[i]).defaultChecked
                                ? 1
                                : 0
                            : 1);
            else {
                $(htmlTag[i]).prop("checked",
                    e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : false);
                $.uniform.update();
            }
        } else if (htmlTag[i].type == "number") {
            $(htmlTag[i]).val(e.data != "" ? e.data[htmlTag[i].attributes["table-param"].value] : null)
                .trigger('change');
        } else if (htmlTag[i].type == "date") {
            if (e.data[htmlTag[i].attributes["table-param"].value] !== null &&
                e.data[htmlTag[i].attributes["table-param"].value] !== "0001-01-01T00:00:00")
                $('input[table-param=' + htmlTag[i].attributes["table-param"].value + ']').val(e.data[htmlTag[i].attributes["table-param"].value].split('T')[0]);
        } else if (htmlTag[i].type == "datetime-local") {
            //$('input[table-param="NotificationDate"]').val('2020-05-06T11:11')
            if (e.data[htmlTag[i].attributes["table-param"].value] != "0001-01-01T00:00:00")
                $('input[table-param=' + htmlTag[i].attributes["table-param"].value + ']').val(e.data[htmlTag[i].attributes["table-param"].value]);
        }
    }
}

function funcCancelModal(id) {
    swal({
        title: "Emin misiniz?",
        text: "Değişiklikler kaybolabilir !",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#EF5350",
        confirmButtonText: "Evet, Gitsin!",
        cancelButtonText: "Hayır, Kalsın!",
        closeOnConfirm: false,
        closeOnCancel: false
    },
        function (isConfirm) {
            if (isConfirm) {
                var modalClose = $(".modal.in");
                $(modalClose[(modalClose.length - 1)]).modal("hide");
                FormAllInputsClear(id);

                $(".cancel").click();
                $(".confirm").click();
                editModalOpen = false;
            } else {
                swal({
                    title: "İsteğiniz üzere iptal Edildi",
                    text: "Kayıtlar güvende.",
                    confirmButtonColor: "#2196F3",
                    type: "info"
                });
            }
        });
}
//   FormAllInputsClear('#Criminal_FormId [table-param]')
function FormAllInputsClear(id) {
    $(id.replace(' [table-param]', '')).find('input[type=datetime-local]').val('');
    $('#EditInsertModal').find('input:text').val('');

    htmlTag = $("" + id + "");
    for (var i = 0; i < htmlTag.length; i++) {
        if (htmlTag[i].type == "text" || htmlTag[i].type == "date" || htmlTag[i].type == "textarea" || htmlTag[i].type == "number")
            htmlTag[i].value = "";
        else if (htmlTag[i].localName == "select")
            $('#' + htmlTag[i].id).val('').change();
    }

    var modalClose = $(".modal.in");
    $(modalClose[(modalClose.length - 1)]).modal("hide");
}

function FormAllInputsClearNoCloseModal(id) {
    $(id.replace(' [table-param]', '')).find('input[type=datetime-local]').val('');
    $(id.replace(' [table-param]', '')).find('input:text').val('');

    htmlTag = $("" + id + "");
    for (var i = 0; i < htmlTag.length; i++) {
        if (htmlTag[i].type == "text" || htmlTag[i].type == "date" || htmlTag[i].type == "textarea" || htmlTag[i].type == "number")
            htmlTag[i].value = "";
        else if (htmlTag[i].localName == "select")
            $('#' + htmlTag[i].id).val('').change();
    }
}

function formatDate(date) {
    var d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [year, month, day].join('/');
}

// Example--> 25/06/2015
function ConvertJSONDate(incomingDate) {
    if (incomingDate != null) {
        var date = new Date(parseInt(incomingDate.substr(6)));
        return (date.getDate().toString() +
            '/' +
            (date.getMonth() + 1).toString() +
            '/' +
            date.getFullYear().toString());
    } else {
        return "";
    }
}
// Example--> YYYY-MM-DD
function ConvertJSONDate2(incomingDate) {
    if (incomingDate != null) {
        var date = new Date(parseInt(incomingDate.substr(6)));
        return (date.getFullYear().toString() +
            '-' +
            (date.getMonth() + 1).toString() +
            '-' +
            date.getDate().toString());
    } else {
        return "";
    }
}
// Example--> 25 Mayıs 2018
function ConvertJSONDateWithMonth(incomingDate) {
    if (incomingDate != null) {
        var date = new Date(parseInt(incomingDate.substr(6)));
        var monthNames = new Array("Ocak",
            "Şubat",
            "Mart",
            "Nisan",
            "Mayıs",
            "Haziran",
            "Temmuz",
            "Ağustos",
            "Eylül",
            "Ekim",
            "Kasım",
            "Aralık");
        return (date.getDate().toString() + ' ' + monthNames[date.getMonth()] + ' ' + date.getFullYear().toString());
    } else {
        return "";
    }
}

// Example--> 25 Mayıs 2018 17:30
function ConvertJSONDateWithMonthAndTime(incomingDate) {
    if (incomingDate != null) {
        var date = new Date(parseInt(incomingDate.substr(6)));
        var monthNames = new Array("Ocak",
            "Şubat",
            "Mart",
            "Nisan",
            "Mayıs",
            "Haziran",
            "Temmuz",
            "Ağustos",
            "Eylül",
            "Ekim",
            "Kasım",
            "Aralık");
        return (date.getDate().toString() +
            ' ' +
            monthNames[date.getMonth()] +
            ' ' +
            date.getFullYear().toString() +
            ' ' +
            (date.getHours() < 10 ? '0' : '') +
            date.getHours() +
            ":" +
            (date.getMinutes() < 10 ? '0' : '') +
            date.getMinutes());
    } else {
        return "";
    }
}

//ISO Format!!! 
function ConvertJSONDateFormatISO(incomingDate) {
    if (incomingDate != null) {
        var date = incomingDate.substring(0, 10);
        var splitDate = date.split('-');
        var yy = splitDate[0];
        var mm = splitDate[1];
        var dd = splitDate[2];
        var hours = incomingDate.substring(11, 19);
        return '<span class="label bg-primary-300">' + dd + "-" + mm + "-" + yy + " " + hours + '</span>';
    } else {
        return "";
    }
}

function ConvertJSONJustDateFormatISO(incomingDate) {
    if (incomingDate != null) {
        var date = incomingDate.substring(0, 10);
        var splitDate = date.split('-');
        var yy = splitDate[0];
        var mm = splitDate[1];
        var dd = splitDate[2];
        return '<span class="label bg-primary-300">' + dd + "-" + mm + "-" + yy + '</span>';
    } else {
        return "";
    }
}

//Örn: 25-01-2020
function ConvertJSONJustDateFormatISONoBackgroundColor(incomingDate) {
    if (incomingDate != null) {
        var date = incomingDate.substring(0, 10);
        var splitDate = date.split('-');
        var yy = splitDate[0];
        var mm = splitDate[1];
        var dd = splitDate[2];
        return dd + "-" + mm + "-" + yy;
    } else {
        return "";
    }
}

//Örn: 25-01-2020
function ConvertJSONJustDateFormatISO2(incomingDate) {
    if (incomingDate != null) {
        var date = incomingDate.substring(0, 10);
        var splitDate = date.split('-');
        var yy = splitDate[0];
        var mm = splitDate[1];
        var dd = splitDate[2];
        return mm + "/" + dd + "/" + yy.slice(-2);
    } else {
        return "";
    }
}

function ConvertJSONDateFormatISONoBackgroundColor(incomingDate) {
    if (incomingDate != null) {
        var date = incomingDate.substring(0, 10);
        var splitDate = date.split('-');
        var yy = splitDate[0];
        var mm = splitDate[1];
        var dd = splitDate[2];
        var hours = incomingDate.substring(11, 19);
        return dd + "-" + mm + "-" + yy + " " + hours;
    } else {
        return "";
    }
}

function ConvertJsonDateFormatIsoMonthAndTime(incomingDate) {
    if (incomingDate != null) {
        var date = incomingDate.substring(0, 10);
        var splitDate = date.split('-');
        var yy = splitDate[0];
        var mm = splitDate[1];
        var dd = splitDate[2];
        var monthNames = new Array("", "Ocak",
            "Şubat",
            "Mart",
            "Nisan",
            "Mayıs",
            "Haziran",
            "Temmuz",
            "Ağustos",
            "Eylül",
            "Ekim",
            "Kasım",
            "Aralık");
        return (dd + ' ' + (mm < 10 ? monthNames[mm.replace('0', '')] : monthNames[mm]) + ' ' + yy);
    } else {
        return "";
    }
}

// Example--> 25 12 2018 17:30
function ConvertJSONDateWithMonthAndTimeNumber(incomingDate) {
    if (incomingDate != null) {
        var date = new Date(parseInt(incomingDate.substr(6)));
        var monthNames = new Array("01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12");
        return (date.getDate().toString() +
            ' ' +
            monthNames[date.getMonth()] +
            ' ' +
            date.getFullYear().toString() +
            ' ' +
            (date.getHours() < 10 ? '0' : '') +
            date.getHours() +
            ":" +
            (date.getMinutes() < 10 ? '0' : '') +
            date.getMinutes());
    } else {
        return "";
    }
}

function formatJSONDate(jsonDate) {
    var newDate = dateFormat(jsonDate, "mm/dd/yyyy");
    return newDate;
}

function isoDate(date) {
    if (!date) {
        return null
    }
    date = moment(date).toDate()
    var month = 1 + date.getMonth()
    if (month < 10) {
        month = '0' + month
    }
    var day = date.getDate()
    if (day < 10) {
        day = '0' + day
    }
    return date.getFullYear() + '-' + month + '-' + day
}

// Example--> 25 Aralık 2018 17:30:21
function ConvertJSONDateWithSecondNumber(incomingDate) {
    if (incomingDate != null) {
        var date = new Date(parseInt(incomingDate.substr(6)));
        var monthNames = new Array("Ocak",
            "Şubat",
            "Mart",
            "Nisan",
            "Mayıs",
            "Haziran",
            "Temmuz",
            "Ağustos",
            "Eylül",
            "Ekim",
            "Kasım",
            "Aralık");
        return (date.getDate().toString() +
            ' ' +
            monthNames[date.getMonth()] +
            ' ' +
            date.getFullYear().toString() +
            ' ' +
            (date.getHours() < 10 ? '0' : '') +
            date.getHours() +
            ":" +
            (date.getMinutes() < 10 ? '0' : '') +
            date.getMinutes() + ':' + date.getSeconds());
    } else {
        return "";
    }
}

function diff_hours(endDate, startDate) {
    var diff = (endDate.getTime() - startDate.getTime()) / 1000;
    diff /= (60 * 60);
    return Math.abs(Math.round(diff));
}
//-------------------- Fill DataTables ---------------------- //
var dtSayac = 0;
var dtHtmlContent = "";
var tableName = "";
var formId = "";
var pageStartCounter = 0;

for (var s = 0; s < $("table[table-tool-type='dataTable']").length; s++) {

    tableName = $("[table-id='" + $("table[table-tool-type='dataTable']")[s]
        .attributes["table-id"].value + "']")[0]
        .attributes["table-DatabaseName"].value;
    sendAjxForm("",
        $("[table-id='" + $("table[table-tool-type='dataTable']")[s]
            .attributes["table-id"].value + "']")[0]
            .attributes["table-model-url"].value,
        funcDataTable, "POST");

}

var EditInsertButtonId = document.getElementById("EditInsertButtonId");
//if (EditInsertButtonId != null)
//    document.getElementById("EditInsertButtonId").setAttribute("onClick", "FuncSaveModel('#Category_FormId [table-param]')");

function funcDataTable(e) {

    if (e.length > 0) {
        var dataId = $("table[table-tool-type='dataTable']")[dtSayac].attributes["table-id"].value;
        var column = $("[table-id='" + dataId + "'] th[data-value]");
        var count = e[0].PageStartCount;

        for (var j = 0; j < e.length; j++) {
            dtHtmlContent += "<tr  style='text-align: center;' id='" + dataId + "-" + j + "'>";

            for (var k = 0; k < column.length - 1; k++) {
                var columnName = column[k].attributes["data-value"].value;

                if (k == 0) // Sıra no için
                {
                    dtHtmlContent += '<td style="display:none;"><input id="' +
                        e[j].Id +
                        '" table-param="Id_' +
                        e[j].Id +
                        '" value="' +
                        e[j].Id +
                        '" hidden/></td>';
                    dtHtmlContent += '<td>' + (parseInt(count * 10) + parseInt(j + 1)) + '</td>';
                }

                if (e[j][columnName] != undefined || e[j][columnName] == null) {
                    if (column[k].attributes["hidden"] == undefined) {
                        if (e[j][columnName] != null) {
                            if (column[k].attributes["data-type"] != undefined && column[k].attributes["data-type"].value == "datetime") {
                                dtHtmlContent += "<td>" +
                                    (e[j][columnName] != null
                                        ? (ConvertJSONDateFormatISO(e[j][columnName].split('T')[0] + " " + e[j][columnName].split('T')[1]))
                                        : "") +
                                    "</td>";
                            } else if (column[k].attributes["data-type"] != undefined && column[k].attributes["data-type"].value == "date") {
                                dtHtmlContent += "<td>" +
                                    (e[j][columnName] != null
                                        ? (ConvertJSONDateFormatISO(e[j][columnName].split('T')[0]))
                                        : "") +
                                    "</td>";
                            } else if (column[k].attributes["data-type"] != undefined && column[k].attributes["data-type"].value == "money") {
                                dtHtmlContent += "<td>" +
                                    (e[j][columnName] != null
                                        ? ("<span style='width: 100%;' class='label bg-teal-300'>" +
                                            formatMoney(e[j][columnName]) +
                                            " ₺</span>")
                                        : "") +
                                    "</td>";
                            } else if (column[k].attributes["data-type"] != undefined && column[k].attributes["data-type"].value == "decimal") {
                                dtHtmlContent += "<td>" +
                                    (e[j][columnName] != null
                                        ? formatMoney(e[j][columnName])
                                        : "") +
                                    "</td>";
                            }
                            else
                                dtHtmlContent += "<td>" + e[j][columnName] + "</td>";
                        } else
                            dtHtmlContent += "<td></td>";
                    } else
                        dtHtmlContent += "<td hidden>" + e[j][columnName] + "</td>";
                }
            }

            dtHtmlContent += "<td>" +
                "<ul class='icons-list'>";

            //Button edit Auth
            if (e[j].EditButtonActive) {
                var idE = e[j].ID == undefined ? e[j].Id : e[j].ID;
                dtHtmlContent += "<li title='Satırı Düzenle' class='text-primary-800'><a data-toggle='modal' onclick='funcGetByIdModal(" +
                    e[j].Id +
                    ");'><i class='icon-pencil5'></i></a></li>";
            }

            if (e[j].CustomButton != null)
                dtHtmlContent += e[j].CustomButton;

            //Button delete Auth
            if (e[j].DeleteButtonActive) {
                var idD = e[j].ID == undefined ? e[j].Id : e[j].ID;
                dtHtmlContent += "<li title='Sil' class='text-danger-800'><a onclick='funcDeleteModal(" +
                    e[j].Id +
                    ")'><i class='icon-trash'></i></a></li>";
            }

            dtHtmlContent += "</ul></td></tr>";
        }

        $("table[table-tool-type='dataTable'] tbody")[0].innerHTML = dtHtmlContent;

        if ($('#TableFooter')[0] != undefined)
            sendAjxForm("", "/Utility/GetPageList", LoadPageList, "POST");

        dtHtmlContent = "";
    } else {
        $('#divPagedList').empty();
        $('#tBody tr').empty();
        ShowMessage("info", "Bilgi", "Filtreye uygun veri bulunamadı");
    }

    //datatable animation
    setTimeout(function () { $(".table").removeClass('table-loader').show() }, 2000);
}

function funcCustomDataTable(e, dataId) {
    var dtCustomHtmlContent = "";
    if (e.length > 0) {
        var column = $("[table-id='" + dataId + "'] th[data-value]");
        var modelLength = e.length;
        var count = e[0].PageStartCount == undefined ? 0 : e[0].PageStartCount;
        for (var j = 0; j < modelLength; j++) {
            dtCustomHtmlContent += "<tr  style='text-align: center;' id='" + dataId + "-" + j + "'>";

            for (var k = 0; k < column.length; k++) {
                if (k == 0) // Sıra no için
                    dtCustomHtmlContent += '<td>' + (parseInt(count * 10) + parseInt(j + 1)) + '</td>';

                var columnName = column[k].attributes["data-value"].value;
                if (e[j][columnName] != undefined || e[j][columnName] == null) {
                    if (column[k].attributes["hidden"] == undefined) {
                        if (e[j][columnName] != null) {
                            if (column[k].attributes["data-type"] != undefined && column[k].attributes["data-type"].value == "date") {
                                dtCustomHtmlContent += "<td><span style='width: 100%;' class='label bg-danger-300'>" + e[j][columnName].split('T')[0] + "</span></td>";
                            } else if (column[k].attributes["data-type"] != undefined && column[k].attributes["data-type"].value == "money") {
                                dtCustomHtmlContent += "<td><span style='width: 100%;' class='label bg-teal-300'>" + formatMoney(e[j][columnName], 2) + " ₺</span></td>";
                            } else
                                dtCustomHtmlContent += "<td>" + e[j][columnName] + "</td>";
                        }
                        else
                            dtCustomHtmlContent += "<td></td>";
                    }
                    else
                        dtCustomHtmlContent += "<td hidden>" + e[j][columnName] + "</td>";
                }
            }
            dtCustomHtmlContent += "</tr>";
        }
    }
    $("[table-id='" + dataId + "'] tbody")[0].innerHTML = dtCustomHtmlContent;
    dtCustomHtmlContent = "";
}

function funcCustomDataTableForExcel(e, id) {
    var column = $("[id='" + id + "'] th[data-value]");
    var modelLength = e.length;
    var count = e[0].PageStartCount;
    var dtCustomHtmlContent = "";
    for (var j = 0; j < modelLength; j++) {
        dtCustomHtmlContent += "<tr  style='text-align: center;' id='" + id + "-" + j + "'>";

        for (var k = 0; k < column.length; k++) {

            if (k == 0) // Sıra no için
                dtCustomHtmlContent += '<td class="bg-slate-300">' + (parseInt(count * 10) + parseInt(j + 1)) + '</td>';

            var columnName = column[k].attributes["data-value"].value;
            if (k == 1)
                dtCustomHtmlContent += "<td class='bg-primary-300'>" + e[j][columnName] + "</td>";
            else {
                if (e[j][columnName] != undefined || e[j][columnName] == null) {
                    if (e[j][columnName] != null) {
                        dtCustomHtmlContent += "<td>" + e[j][columnName] + "</td>";
                    } else
                        dtCustomHtmlContent += "<td></td>";
                }
            }

        }
        dtCustomHtmlContent += "</tr>";
    }

    $("[id='" + id + "'] tbody")[0].innerHTML = dtCustomHtmlContent;
    dtCustomHtmlContent = "";
}

$('.genericDatatables').css({ "font-size": "11px", "white-space": "nowrap", "overflow": "scroll" });
function LoadPageList(e) {
    $('#divPagedList').remove();
    $('#totalPagedList').remove();
    $('.dataTables_filter').html;

    var elements = $('.genericDatatables');
    for (var n = 0; n < elements.length; n++) {
        if ($(elements[n]).find("table")[0].className.indexOf("notPaged") < 0) {
            $(elements[n]).append(e);
        }
    }
    $('.dataTables_info').attr("style", "display:none");
}

function funcDeleteModal(id) {
    var model = { Id: id };
    var urlDelete = tableName + "/Delete";
    swal({
        title: "Emin misiniz?",
        text: "Bu kayıt silinecektir !",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#EF5350",
        confirmButtonText: "Evet, Kaydı Sil!",
        cancelButtonText: "Hayır, Kayıt Kalsın!",
        closeOnConfirm: false,
        closeOnCancel: false
    },
        function (isConfirm) {
            if (isConfirm) {
                sendAjxForm(model,
                    urlDelete,
                    function (e) {
                        if (e.IsSuccess == true) {
                            swal({
                                title: "Başarılı!",
                                text: "Kayıt silindi.",
                                confirmButtonColor: "#66BB6A",
                                type: "success"
                            });
                            funcFilterTable();
                            ShowMessage("success", "Bilgi", "Silme Başarılı.");
                        } else if (e.IsSuccess == false) {
                            $(".cancel").click();
                            $(".confirm").click();
                            ShowMessage("warning", "Bilgi", e.Message);
                        }
                    },
                    "POST");
            } else {
                swal({
                    title: "İsteğiniz üzere iptal Edildi",
                    text: "Kayıt güvende.",
                    confirmButtonColor: "#2196F3",
                    type: "error"
                });
            }
        });
}

function funcSwalMessage(model, url, title, returnFunction) {
    swal({
        title: "Onaylıyor musunuz ?",
        text: title,
        type: "warning",
        html: '',
        showCancelButton: true,
        confirmButtonColor: "#EF5350",
        confirmButtonText: "Evet, Devam et!",
        cancelButtonText: "Hayır, Kalsın!",
        closeOnConfirm: false,
        closeOnCancel: false
    },
        function (isConfirm) {
            if (isConfirm) {
                sendAjxForm(model,
                    url,
                    function (e) {
                        $(".cancel").click();
                        $(".confirm").click();
                        if (e.IsSuccess === true) {
                            swal({
                                title: "Başarılı!",
                                text: "İşlem başarıyla tamamlandı.",
                                confirmButtonColor: "#66BB6A",
                                type: "success"
                            });
                            funcFilterTable();
                            //ShowMessage("success", "Bilgi", "İşlem Başarılı.");
                        } else if (e.IsSuccess === false) {
                            funcFilterTable();
                            ShowMessage("warning", "Bilgi", "Hata Oluştu!");
                        }
                        returnFunction(e);
                    }, "POST");
            } else {
                swal({
                    title: "İsteğiniz üzere iptal Edildi",
                    text: "Kayıt güvende.",
                    confirmButtonColor: "#2196F3",
                    type: "error"
                });
            }
        });
}

function FuncSaveModel(id) {
    var model = FormGetAllValue(id);
    if (model != null) {
        var urlInsert = tableName + "/Insert";
        sendAjxForm(model,
            urlInsert,
            function (e) {
                if (e.IsSuccess == true) {
                    ShowMessage("success", "Bilgi", "İşlem Başarılı.");
                    FormAllInputsClear(id);
                    funcFilterTable();
                } else if (e.IsSuccess == false)
                    ShowMessage("error", "Hata", e.Message);

            }, 'POST');
    }
}

function funcGetByIdModal(id) {
    var model = { Id: id };
    var urlGetById = tableName + "/GetById";
    var formId = '#' + tableName.substr(1, tableName.length) + '_FormId [table-param]';
    sendAjxForm(model,
        urlGetById,
        function (e) {
            if (e.Id > 0) {
                var model = {
                    selector: formId,
                    data: e
                };
                FormSetAllValue(model);
                document.getElementById("EditInsertButtonId").setAttribute("onClick", "funcEditModal('" + formId + "');");
                $('#EditInsertModal').modal("show");
            } else
                ShowMessage("error", "Hata", "Hata oluştu!");

        }, 'GET');
}

function funcEditModal(id) {
    var model = FormGetAllValue(id);
    if (model != null) {
        var urlUpdate = tableName + "/Update";
        sendAjxForm(model,
            urlUpdate,
            function (e) {
                if (e.IsSuccess == true) {
                    ShowMessage("success", "Bilgi", "Düzenleme Başarılı.");
                    FormAllInputsClear(id);
                    funcFilterTable();
                } else if (e.IsSuccess == false)
                    ShowMessage("error", "Hata", "Düzenleme esnasında hata oluştu!");

            }, 'POST');
    }
}

function EditInsertOpenModal() {
    var formId = '#' + tableName.substr(1, tableName.length) + '_FormId [table-param]';
    FormAllInputsClear(formId);
    document.getElementById("EditInsertButtonId").setAttribute("onClick", "FuncSaveModel('" + formId + "');");
    $('#EditInsertModal').modal("show");
}

function resetForm(id) {
    document.querySelector('[table-id="' + id + '"]').reset();
    $('[table-id="' + id + '"] .select').select2('val', 'All');
    $('[table-id="' + id + '"] label').removeClass('is-visible')
}

var IsSearchOpen = false;
function funcSearchForm() {
    if (!IsSearchOpen) {
        $('#FormSearchTable').show();
        IsSearchOpen = true;
    } else {
        $('#FormSearchTable').hide();
        IsSearchOpen = false;
    }
}

function MenuParentChildRecursive(arr) {
    var tree = [],
        mappedArr = {},
        arrElem,
        mappedElem;

    for (var i = 0, len = arr.length; i < len; i++) {
        arrElem = arr[i];
        mappedArr[arrElem.Id] = arrElem;
        mappedArr[arrElem.Id]['children'] = [];
    }

    for (var id in mappedArr) {
        if (mappedArr.hasOwnProperty(id)) {
            mappedElem = mappedArr[id];
            if (mappedElem.ParentId) {
                mappedArr[mappedElem['ParentId']]['children'].push(mappedElem);
            } else {
                tree.push(mappedElem);
            }
        }
    }
    return tree;
}

function isInt(n) {
    return Number(n) === n && n % 1 === 0;
}

function isFloat(n) {
    return Number(n) === n && n % 1 !== 0;
}

function isEmpty(str) {
    if (typeof str == 'undefined' || str == null || !str || str.length === 0 || str === "" || !/[^\s]/.test(str) || /^\s*$/.test(str) || str.replace(/\s/g, "") === "")
        return true;
    else
        return false;
}

//modal içinde modal sorunu için
$(document).on('show.bs.modal', '.modal', function (event) {
    var zIndex = 1040 + (10 * $('.modal:visible').length);
    $(this).css('z-index', zIndex);
    setTimeout(function () {
        $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
    }, 100);
    $('.modal').on("hidden.bs.modal", function (e) { //fire on closing modal box
        if ($('.modal:visible').length) { // check whether parent modal is opend after child modal close
            $('body').addClass('modal-open'); // if open mean length is 1 then add a bootstrap css class to body of the page
        }
    });
});

$(document).ready(function () {
    $("input").attr("autocomplete", "off");
});


function formatMoney(amount, decimalCount = 2, decimal = ",", thousands = ".") {
    try {
        decimalCount = Math.abs(decimalCount);
        decimalCount = isNaN(decimalCount) ? 2 : decimalCount;

        const negativeSign = amount < 0 ? "-" : "";

        let i = parseInt(amount = Math.abs(Number(amount) || 0).toFixed(decimalCount)).toString();
        let j = (i.length > 3) ? i.length % 3 : 0;

        return negativeSign + (j ? i.substr(0, j) + thousands : '') + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousands) + (decimalCount ? decimal + Math.abs(amount - i).toFixed(decimalCount).slice(2) : "");
    } catch (e) {
        console.log(e)
    }
};

//verilen url'ye parametre ekler
function insertParam(url, object) {
    var params = "?";
    var lengthObj = Object.keys(object).length;
    for (var i = 0; i < lengthObj; i++) {
        if (Object.values(object)[i] != "") {
            params += Object.keys(object)[i] + "=" + Object.values(object)[i];

            if (i != lengthObj - 1)
                params += "&";
        }
    }
    return url + params;
}

function insertFormDataParam(formData, object) {
    var lengthObj = Object.keys(object).length;
    for (var i = 0; i < lengthObj; i++)
        if (Object.values(object)[i] != "")
            formData.append(Object.keys(object)[i], Object.values(object)[i]);

    return formData;
}

function insertFormDataParamReturnString(object) {
    var result = "?";
    var lengthObj = Object.keys(object).length;
    for (var i = 0; i < lengthObj; i++)
        if (Object.values(object)[i] != "")
            result += (Object.keys(object)[i] + "=" + Object.values(object)[i]) + "&";

    return result;
}

//TabIndex fix problem
var tabindex = 1;
$('input,select').each(function () {
    if (this.type != "hidden") {
        $(this).attr("tabindex", tabindex);
        tabindex++;
    }
});

$(document).on('focus', '.select2', function (e) {
    if (e.originalEvent) {
        var s2element = $(this).siblings('select');
        //s2element.select2('open');
        // Set focus back to select2 element on closing.
        s2element.on('select2:closing', function (e) {
            s2element.select2('focus');
        });
    }
});

function exportExcelWithJs(div, excelName) {
    var tT = new XMLSerializer().serializeToString(document.getElementById(div)); //Serialised table
    var tF = excelName + '.xls'; //Filename
    var tB = new Blob([tT]); //Blub

    if (window.navigator.msSaveOrOpenBlob) {
        //Store Blob in IE
        window.navigator.msSaveOrOpenBlob(tB, tF)
    }
    else {
        //Store Blob in others
        var tA = document.body.appendChild(document.createElement('a'));
        tA.href = URL.createObjectURL(tB);
        tA.download = tF;
        tA.style.display = 'none';
        tA.click();
        tA.parentNode.removeChild(tA);
    }
}

function exportPrintWithJs(div) {
    var divToPrint = document.getElementById(div);
    newWin = window.open("");
    newWin.document.write(divToPrint.outerHTML);
    newWin.print();
    newWin.close();
}


//var sTimeout = 10000;// 5 * 60 * 1000;
//setTimeout('SessionEnd()', sTimeout);
//function SessionEnd() {
//    var pathName = window.location.pathname;
//    window.location = "/Login/Index?redirectUrl=" + pathName;
//}

$(function () {
    var pathName = window.location.pathname;
    var startHour = 360 * 24 * (60 * 60 * 1000);//360 gün
    var extraMinute = 60 * 1000;
    $.sessionTimeout({
        heading: 'h5',
        title: 'Oturum Zaman Aşımı Bildirimi',
        message: 'Oturumunuzun süresi 60 sn sonra dolmak üzere,sonrasında tekrar giriş yapmanız gerekmektedir.',
        ignoreUserActivity: true,
        warnAfter: startHour - extraMinute,
        redirAfter: startHour + extraMinute,
        keepAliveUrl: '/',
        logoutBtnText: 'Çıkış Yap',
        keepBtnText: 'Bağlantıda Kalın',
        keepBtnClass: ' hidden',
        redirUrl: '/login?redirectUrl=' + pathName,
        logoutUrl: '/login?redirectUrl=' + pathName
    });

});



//function test() {
//    $("#NoticeUnit_FormId").animate({ width: "toggle" });

//    //$("#NoticeUnitVehicleDiv").animate({
//    //    width: "toggle"
//    //});

//    //$('#NoticeUnit_FormId').hide();
//    $("#NoticeUnitVehicleDiv").toggle("slide", { direction: "left" });
//    //$("#NoticeUnit_FormId").toggle("slide", { direction: "left" });
//}



