(function (document) {
    var
        head = document.head = document.getElementsByTagName('head')[0] || document.documentElement,
        elements = 'article aside audio bdi canvas data datalist details figcaption figure footer header hgroup mark meter nav output picture progress section summary time video x'.split(' '),
        elementsLength = elements.length,
        elementsIndex = 0,
        element;

    while (elementsIndex < elementsLength) {
        element = document.createElement(elements[++elementsIndex]);
    }

    element.innerHTML = 'x<style>' +
        'article,aside,details,figcaption,figure,footer,header,hgroup,nav,section{display:block}' +
        'audio[controls],canvas,video{display:inline-block}' +
        '[hidden],audio{display:none}' +
        'mark{background:#FF0;color:#000}' +
        '</style>';

    return head.insertBefore(element.lastChild, head.firstChild);
})(document);

/* ================================ Prototyping ========================================== */

(function (window, ElementPrototype, ArrayPrototype, polyfill) {
    function NodeList() { [polyfill] }
    NodeList.prototype.length = ArrayPrototype.length;

    ElementPrototype.matchesSelector = ElementPrototype.matchesSelector ||
        ElementPrototype.mozMatchesSelector ||
        ElementPrototype.msMatchesSelector ||
        ElementPrototype.oMatchesSelector ||
        ElementPrototype.webkitMatchesSelector ||
        function matchesSelector(selector) {
            return ArrayPrototype.indexOf.call(this.parentNode.querySelectorAll(selector), this) > -1;
        };

    ElementPrototype.ancestorQuerySelectorAll = ElementPrototype.ancestorQuerySelectorAll ||
        ElementPrototype.mozAncestorQuerySelectorAll ||
        ElementPrototype.msAncestorQuerySelectorAll ||
        ElementPrototype.oAncestorQuerySelectorAll ||
        ElementPrototype.webkitAncestorQuerySelectorAll ||
        function ancestorQuerySelectorAll(selector) {
            for (var cite = this, newNodeList = new NodeList; cite = cite.parentElement;) {
                if (cite.matchesSelector(selector)) ArrayPrototype.push.call(newNodeList, cite);
            }

            return newNodeList;
        };

    ElementPrototype.ancestorQuerySelector = ElementPrototype.ancestorQuerySelector ||
        ElementPrototype.mozAncestorQuerySelector ||
        ElementPrototype.msAncestorQuerySelector ||
        ElementPrototype.oAncestorQuerySelector ||
        ElementPrototype.webkitAncestorQuerySelector ||
        function ancestorQuerySelector(selector) {
            return this.ancestorQuerySelectorAll(selector)[0] || null;
        };
})(this, Element.prototype, Array.prototype);

/* ================================ Helper Functions ========================================== */

function generateTableRow(rowCount, idVal = 0) {
    var emptyColumn = document.createElement('tr');
    emptyColumn.setAttribute("id", ("tr_" + rowCount));
    emptyColumn.innerHTML = '<td><a class="cut">-</a><input id="id_' + rowCount + '" value="' + idVal + '" hidden/><input id="checkboxFirm_' + rowCount + '" type="checkbox" checked></td>' + /*firma*/
        '<td><input id="productName_' + rowCount + '" autocomplete="off" type="text" class="form-control input-TenderDetail"></td>' + /*ürün adı*/
        '<td><input id="Stock_' + rowCount + '" autocomplete="off" type="text" class="form-control input-TenderDetail"></td>' + /*stok kodu*/
        '<td><input id="inputPiece_' + rowCount + '" autocomplete="off" type="text" onkeyup="updateNumber(this)" class="form-control input-TenderDetail text-center"/></td>' + /*miktar*/
        '<td style="text-align: inherit;"><select style="width: 100%;border: 1px solid white;" id="comboUnitType_' + rowCount + '"></select></td>' + /*birim*/
        '<td><input id="inputPrice_' + rowCount + '" autocomplete="off" type="text" onkeyup="updateNumber(this)" class="form-control input-TenderDetail text-center"/></td>' + /*fiyat*/
        '<td><input id="inputTotal_' + rowCount + '" type="text" onkeyup="updateNumber(this)" class="form-control input-TenderDetail text-center" disabled/></td>' + /*toplam*/
        '<td><select id="comboUnit_' + rowCount + '" style="width: 100%;border: 1px solid white;"></select></td>' + /*Müdürlük*/
        '<td><input id="inputLastTotal_' + rowCount + '" autocomplete="off" onkeyup="updateNumber(this)" type="text" class="form-control input-TenderDetail text-center"/></td>' + /*Son fiyat*/
        '<td><button id="getLastPrice_' + rowCount + '" onclick="setLastPrice(' + rowCount + ')" type="button" class="btn border-warning text-warning-600 btn-flat btn-icon"><i class="icon-copy3"></i></button></td>' + /*fiyat kopyala*/
        //'<td><span contenteditable></span></td>' + /*belge*/
        '<td><input id="checkboxJobIncrease_' + rowCount + '" type="checkbox"></td>'; /*iş arttırımı*/
    return emptyColumn;
}

function parseFloatHTML(element) { return parseFloat(element.innerHTML.replace(/[^\d\.\-]+/g, '')) || 0; }
function parsePrice(number) { return number.toFixed(2).replace(/(\d)(?=(\d\d\d)+([^\d]|$))/g, '$1,'); }

/* ------------------------------- Update Combo's ------------------------------- */
function loadUnitType(id, val_, e) {
    var comboText = "<option value='0'>Birim</option>";
    for (var i = 0; i < e.length; i++)
        comboText += '<option value="' + e[i].Id + '">' + e[i].Name + '</option>';

    $('#' + id).append(comboText);
    if (val_ > 0)
        $('#' + id).val(val_);
}

function loadUnit(id, val_, e) {
    var comboText = "<option value=''>Mudurluk</option>";
    for (var i = 0; i < e.length; i++)
        comboText += '<option value="' + e[i].Id + '">' + e[i].Name + '</option>';

    $('#' + id).append(comboText);
    if (val_ > 0)
        $('#' + id).val(val_);
}

function comboUnitType(id, val_ = 0) {
    if (val_ == 0) { //yeni parametre eklemiş olabilir!
        sendAjxForm("", "/Combo/GetByLookUpListTypeId?typeId=22", function (e) {
            loadUnitType(id, val_, e);
        }, "GET");
    } else//edit button click 
        loadUnitType(id, val_, unitTypeArr);
}

function comboUnit(id, val_ = 0) {
    if (val_ == 0) { //yeni parametre eklemiş olabilir!
        sendAjxForm("", "/Combo/GetUnitAndParentCmbx?isTenderVisible=true", function (e) {
            loadUnit(id, val_, e);
        }, "GET");
    } else//edit button click 
        loadUnit(id, val_, unitArr);
}

var unitTypeArr = new Array();
var unitArr = new Array();
$(document).ready(function () {
    loadUnitTypeData();
    loadUnitData();

    function loadUnitTypeData() { sendAjxForm("", "/Combo/GetByLookUpListTypeId?typeId=22", function (e) { unitTypeArr = e; }, "GET"); }
    function loadUnitData() { sendAjxForm("", "/Combo/GetUnitAndParentCmbx?isTenderVisible=true", function (e) { unitArr = e; }, "GET"); }
});

/* ------------------------------- Update Number ------------------------------- */

function updateNumber(e) {
    var activeElement = document.activeElement, value = parseFloat(activeElement.value);
    formatCurrencyTender(activeElement);

    //if (!isNaN(value) && (e.keyCode == 38 || e.keyCode == 40 || e.wheelDeltaY)) {
    //    e.preventDefault();

    //    value += e.keyCode == 38 ? 1 : e.keyCode == 40 ? -1 : Math.round(e.wheelDelta * 0.025);
    //    value = Math.max(value, 0);

    //    activeElement.innerHTML = wasPrice ? parsePrice(value) : value;
    //    //updateInvoice();
    //}
}

function formatCurrencyTender(input, blur) {
    var inputVal = input.value;
    var inputId = input.id.split('_')[1];
    if (inputVal.replace(/\s+/, "") == "" || inputVal == undefined) { $('#inputTotal_' + inputId).val(''); return; }

    var originalLen = inputVal.length;
    var caretPos = input.selectionStart;

    if (inputVal.indexOf(",") >= 0) {
        var decimalPos = inputVal.indexOf(",");

        var leftSide = inputVal.substring(0, decimalPos);
        var rightSide = inputVal.substring(decimalPos);

        leftSide = formatNumber(leftSide);
        rightSide = formatNumber(rightSide);

        if (blur === "blur")
            rightSide += "00";

        rightSide = rightSide.substring(0, 2);
        inputVal = leftSide + "," + rightSide;
    } else {
        inputVal = formatNumber(inputVal);
        if (blur === "blur")
            inputVal += ",00";
    }

    $('#' + input.id).val(inputVal);
    var updatedLen = inputVal.length;
    caretPos = updatedLen - originalLen + caretPos;
    input.setSelectionRange(caretPos, caretPos);
    totalPriceCalc(inputId);
}

function totalPriceCalc(id) {
    var piece = $('#inputPiece_' + id).val();
    var price = $('#inputPrice_' + id).val();
    if (piece == '') piece = "0.0";
    if (price == '') price = "0.0";

    piece = parseFloat(piece.replaceAll('.', '').replace(',', '.'));
    price = parseFloat(price.replaceAll('.', '').replace(',', '.'));

    var total = formatMoney((piece * price));
    $('#inputTotal_' + id).val(parseInt(total) > 0 ? total : '');
    updateInvoice();
}

/* ------------------------------- Update Invoice ------------------------------- */

function updateInvoice() {
    var total = 0;
    var oTable = document.getElementById('tenderDetailTable');
    var rowLength = oTable.rows.length;
    var price, piece;
    for (var i = 1; i < rowLength; i++) {
        var id = (oTable.rows.item(i).id).split("_")[1];
        piece = $('#inputPiece_' + id).val();
        price = $('#inputPrice_' + id).val();
        if (parseFloat(piece) > 0 && parseFloat(price) > 0) {
            var x = parseFloat(piece.replaceAll('.', '').replace(',', '.'));
            var y = parseFloat(price.replaceAll('.', '').replace(',', '.'));
            total += (x * y);
        }
    }
    $('#tenderDetailTotal').html(formatMoney(total) + "₺");
    if (parseFloat(piece) > 0 && parseFloat(price) > 0) changeColor();
}

function changeColor() {
    $('#tenderDetailTotal').css("background-color", "#e4ed58");
    setTimeout(function () { $('#tenderDetailTotal').css({ "background-color": "white", "transition": "background 3s" }) }, 150);
    $('#tenderDetailTotal').css("transition", "");
}

/* ------------------------------- On Content Load ------------------------------- */
var tableRow = 0;
function onContentLoad() {
    //updateInvoice();
    var input = document.querySelector('input'),
        image = document.querySelector('img');

    function onClick(e) {
        var element = e.target.querySelector('[contenteditable]'), row;
        element && e.target != document.documentElement && e.target != document.body && element.focus();

        if (e.target.matchesSelector('#tenderDetailAddRow')) {
            document.querySelector('.tender_table.inventory tbody').appendChild(generateTableRow(tableRow));
            setTimeout(function () {
                comboUnitType("comboUnitType_" + tableRow);
                comboUnit("comboUnit_" + tableRow)
                tableRow++;
            }, 50);
        }
        else if (e.target.className == 'cut') {
            row = e.target.ancestorQuerySelector('tr');
            row.parentNode.removeChild(row);
        }
        //updateInvoice();
    }

    function onEnterCancel(e) {
        e.preventDefault();
        image.classList.add('hover');
    }

    function onLeaveCancel(e) {
        e.preventDefault();
        image.classList.remove('hover');
    }

    function onFileInput(e) {
        image.classList.remove('hover');
        var reader = new FileReader(),
            files = e.dataTransfer ? e.dataTransfer.files : e.target.files,
            i = 0;

        reader.onload = onFileLoad;
        while (files[i]) reader.readAsDataURL(files[i++]);
    }

    function onFileLoad(e) {
        var data = e.target.result;
        image.src = data;
    }

    if (window.addEventListener) {
        document.addEventListener('click', onClick);

        //document.addEventListener('mousewheel', updateNumber);
        //document.addEventListener('keyup', updateNumber);

        //document.addEventListener('keydown', updateInvoice);
        //document.addEventListener('keyup', updateInvoice);

        //input.addEventListener('focus', onEnterCancel);
        //input.addEventListener('mouseover', onEnterCancel);
        //input.addEventListener('dragover', onEnterCancel);
        //input.addEventListener('dragenter', onEnterCancel);

        //input.addEventListener('blur', onLeaveCancel);
        //input.addEventListener('dragleave', onLeaveCancel);
        //input.addEventListener('mouseout', onLeaveCancel);

        //input.addEventListener('drop', onFileInput);
        //input.addEventListener('change', onFileInput);
    }
}

window.addEventListener && document.addEventListener('DOMContentLoaded', onContentLoad);



