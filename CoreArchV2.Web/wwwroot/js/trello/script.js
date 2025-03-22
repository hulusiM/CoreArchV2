loadData();

var id = 0;
var idCard;
var idArray;
const myModal = new bootstrap.Modal(document.getElementById("modal-card"));

var arrayCard = [];
var arrayType = [
    ["Önemli", "#f9f9f9", "#ff0000"],
    ["Normal", "#f9f9f9", "#66BB6A"]
];

function getIdArrayCard(id) {
    var cpt = 0;
    var save = null;
    arrayCard.forEach((element) => {
        if (element[0] == id) {
            save = cpt;
        }
        cpt++;
    });
    return save;
}

function openModalCard(ev) {
    var title = ev.currentTarget.getElementsByTagName("h6")[0].innerText;
    var text = ev.currentTarget.getElementsByTagName("span")[0].innerText;
    var dbId = ev.currentTarget.children[0].value;//database id
    var vehicleId = ev.currentTarget.children[1].value;//vehicle id
    var type = ev.currentTarget.parentNode.id.split('_')[1];//column type
    var importanceLevel = ev.currentTarget.getElementsByClassName("badge-important")[0].innerText;
    var cellId = ev.currentTarget.id;

    var comboboId = importanceLevel == "Önemli" ? 0 : 1;
    $('#importantSelect').val(comboboId);

    if (vehicleId > 0)
        $('#modal-title').selectpicker('val', vehicleId);
    else
        $('#modal-title').selectpicker('val', 0);

    $('#oneNoteId').val(dbId);
    $("#cellId").val(cellId);
    $('#oneNoteType').val(type);
    $("#modal-content").val(text);

    idCard = ev.currentTarget.id;
    idArray = getIdArrayCard(idCard);
    myModal.show();

    setTimeout(function () { document.getElementById("modal-content").focus(); }, 10);
}

function saveModifCard() {
    var vehicleId = $('#modal-title').val();
    var plate = $('#modal-title option:selected').text();
    var text = $("#modal-content").val();
    var dbId = $('#oneNoteId').val();
    var type = $('#oneNoteType').val();
    var cellId = $('#cellId').val();

    if (vehicleId == 0 || vehicleId == null) {
        ShowMessage("info", "Bilgi", "lütfen <b>plaka</b> giriniz");
        return;
    }

    if (text == "") {
        ShowMessage("info", "Bilgi", "Lütfen <b>içerik</b> giriniz");
        return;
    }

    var card = document.getElementById(idCard);
    card.getElementsByTagName("h6")[0].innerText = plate;
    card.getElementsByTagName("span")[0].innerText = text;

    var smallText = text.substr(0, 10);
    card.getElementsByClassName("small-content-text")[0].innerHTML = smallText + "...";

    var impText = "";
    var importanceLevel = document.getElementById("importantSelect").value;
    if (importanceLevel == "0") {
        impText = 'Önemli';
        card.getElementsByClassName("badge-important")[0].style.background = "#ff0000";
    } else {
        impText = 'Normal';
        card.getElementsByClassName("badge-important")[0].style.background = "#66BB6A";
    }
    card.getElementsByClassName("badge-important")[0].innerHTML = impText;

    myModal.hide();
    arrayCard[idArray][1] = document.getElementById("importantSelect").value;

    var model = {
        Id: dbId,
        VehicleId: vehicleId,
        //Header: title,
        Description: text,
        ImportanceLevel: importanceLevel,
        Type: type
    };
    funcInsertUpdateCardDb(model, cellId);
}

function funcInsertUpdateCardDb(model, cellId) {
    var cell = cellId;
    sendAjxForm(model, '/OneNote/InsertUpdate',
        function (e) {
            if (e.IsSuccess) {
                if (cellId !== null)
                    $('#' + cell + " .cardId").val(e.Id);
                ShowMessage("success", "Bilgi", "İşlem başarılı");
            } else
                ShowMessage("warning", "Bilgi", e.Message);
        }, 'POST');
}

function addCard(ev) {
    var idCell = "cel" + id;
    ev.currentTarget.outerHTML = '<div class="card card-col" draggable="true" ondragstart="drag(event)" id="' + idCell + '" onclick="openModalCard(event)">' +
        '<input class="cardId" type="text" value="0" hidden/>' +
        '<input class="vehicleId" type="text" value="0" hidden/>' +
        '<div class="card-body" style="background-color: #ddfbc8;">' +
        '<div class="card-action">' +
        '<h6 class="card-title" onclick="event.stopPropagation();">' +
        'Kart Başlık' +
        '</h6>' +
        '<div class="dropdown">' +
        '<i class="fas fa-ellipsis-v" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false" onclick="event.stopPropagation();"></i>' +
        '<ul class="dropdown-menu" aria-labelledby="dropdownMenuButton1"  onclick="event.stopPropagation();">' +
        '<li><div class="delet-btn" onclick="deletCard(event,0)">&nbsp;<i style="color:red;" class="fas fa-trash-alt i"></i> Kartı Sil</div></li>' +
        '</ul>' +
        '</div>' +
        '</div>' +
        '<span class="card-text fs-6 full-content-text"></span>' +
        '<p class="small-content-text">...</p>' +
        '<span class="badge badge-important" style="background-color:#66BB6A;color:#f9f9f9">Normal</span>' +
        '<span class="badge" style="background-color:#FFB74D;color:#f9f9f9">' + $('#userName').val() + '</span>' +
        '</div>' +
        '</div >' + ev.currentTarget.outerHTML;

    var newCard = [idCell, "-1"];
    id++;
    arrayCard.push(newCard);
}

function deletCard(ev, id) {
    ev.onClick = false;
    if (id > 0)
        funcDeleteCardDb(ev, id);
    else
        removeCardHtml(ev);
}

function removeCardHtml(ev) {
    ev.stopPropagation();
    var element = ev.target.parentNode;
    var element1 = element.parentNode;
    var element2 = element1.parentNode;
    var element3 = element2.parentNode;
    var cardbody = element3.parentNode;
    var card = cardbody.parentNode;
    card.remove();
}

function funcDeleteCardDb(ev, id) {
    sendAjxForm({ id: id }, '/OneNote/Delete',
        function (e) {
            if (e.IsSuccess) {
                removeCardHtml(ev);
                ShowMessage("success", "Bilgi", "Silme başarılı");
            } else
                ShowMessage("warning", "Bilgi", e.Message);
        }, 'POST');
}

function allowDrop(ev) {
    ev.preventDefault();
}

function drag(ev) {
    ev.dataTransfer.setData("Text", ev.target.id);
}

function drop(ev) {
    var data = ev.dataTransfer.getData("Text");
    var elem = ev.target;
    if (elem.tagName != "DIV" || elem.id == "")
        while (elem.tagName != "DIV" || elem.id == "")
            elem = elem.parentNode;

    if (elem.tagName == "DIV" && elem.id != "") {
        if (elem.classList.contains("col")) {
            elem.insertBefore(
                document.getElementById(data),
                elem.childNodes[elem.childNodes.length - 2]
            );
        } else
            elem.after(document.getElementById(data));
        ev.preventDefault();
    }

    var cell = $('#' + ev.dataTransfer.getData('text'))[0];
    var title = cell.getElementsByTagName("h6")[0].innerText;
    var text = cell.getElementsByTagName("span")[0].innerText;
    var dbId = cell.children[0].value;//database id
    var vehicleId = cell.children[1].value;//vehicle id
    var type = cell.parentNode.id.split('_')[1];//column type
    var importanceLevel = cell.getElementsByClassName("badge-important")[0].innerText;

    var model = {
        Id: dbId,
        VehicleId: vehicleId,
        Description: text,
        ImportanceLevel: importanceLevel == "Önemli" ? 0 : 1,
        Type: type
    };
    funcInsertUpdateCardDb(model, null);
}

function loadData() {
    sendAjxForm("", '/OneNote/GetAllOneNote',
        function (e) {
            if (e != null) {
                dynamicLoadDiv("btnTodo", e.ToDo);
                dynamicLoadDiv("btnProcess", e.Process);
                dynamicLoadDiv("btnFinished", e.Finished);
            }
        }, 'POST');
}

function dynamicLoadDiv(divId, e) {
    var result = "";
    var userId = $('#userId').val();
    for (var i = 0; i < e.length; i++) {
        var idCell = "cel" + id;
        var backColor = "";
        if (userId == e[i].CreatedBy)
            backColor = "background-color: #ddfbc8;";

        result += '<div class="card card-col" draggable="true" ondragstart="drag(event)" id="' +
            idCell +
            '" onclick="openModalCard(event)">' +
            '<input class="cardId" type="text" value="' + e[i].Id + '" hidden/>' +
            '<input class="vehicleId" type="text" value="' + e[i].VehicleId + '" hidden/>' +
            '<div class="card-body" style="' + backColor + '">' +
            '<div class="card-action">' +
            '    <h6 class="card-title" onclick="event.stopPropagation();"> ' +
            e[i].Plate +
            '</h6>';

        if (userId == e[i].CreatedBy) {
            result += '<div class="dropdown">' +
                '        <i class="fas fa-ellipsis-v" id="dropdownMenuButton1" data-bs-toggle="dropdown" aria-expanded="false" onclick="event.stopPropagation();"></i>' +
                '        <ul class="dropdown-menu" aria-labelledby="dropdownMenuButton1" onclick="event.stopPropagation();">' +
                '            <li>' +
                '                <div class="delet-btn" onclick="deletCard(event,' + e[i].Id + ')">' +
                '                    &nbsp;<i style="color:red;" class="fas fa-trash-alt i"></i> Kartı Sil' +
                '                </div>' +
                '            </li>' +
                '        </ul>' +
                '    </div>';
        }

        result += '</div>' +
            '<span class="card-text fs-6 full-content-text">' +
            e[i].Description +
            '</span>' +
            '<p class="small-content-text">' +
            e[i].Description.substr(0, 50) +
            ' ...</p>';

        if (e[i].ImportanceLevel == "Önemli")
            result += '<span class="badge badge-important" style="background-color:#ff0000;color:#f9f9f9">Önemli</span>';
        else
            result += '<span class="badge badge-important" style="background-color:#66BB6A;color:#f9f9f9">Normal</span>';

        result += ' <span class="badge" style="background-color:#FFB74D;color:#f9f9f9">' + e[i].NameSurname + '</span></div></div>';

        var newCard = [idCell, "-1"];
        id++;
        arrayCard.push(newCard);
    }

    result += '<div id="' + divId + '" class="card card-col" onclick="addCard(event)" data-col="2"><div class="card-body new-col" ><i class="fas fa-plus text-center"></i></div ></div>';
    $('#' + divId)[0].outerHTML = result;
}

$('#modal-card').on('shown.bs.modal', function () {
    $(this).find('textarea[id="modal-content"]').focus();
});
