function setVehicle(vehicleId, comboId) {
    sendAjxForm({ vehicleId: vehicleId }, "/Utility/GetVehicleById",
        function (e) {
            if (e.Id > 0)
                funcCustomComboBoxSingle(comboId, e.Id, e.Name);
        }, 'GET');
}

function setUser(userId, comboId) {
    sendAjxForm({ userId: userId }, "/Utility/GetUserById",
        function (e) {
            if (e.Id > 0)
                funcCustomComboBoxSingle(comboId, e.Id, e.Name);
        }, 'GET');
}

//ID'ye göre Birimi set eder
function setUnit(unitId, comboId) {
    sendAjxForm({ unitId: unitId }, "/Utility/GetUnitById",
        function (e) {
            if (e.Id > 0)
                funcCustomComboBoxSingle(comboId, e.Id, e.Name);
        }, 'GET');
}

function setBrand(brandId, comboId) {
    sendAjxForm({ brandId: brandId }, "/Utility/GetBrandById",
        function (e) {
            if (e.Id > 0)
                funcCustomComboBoxSingle(comboId, e.Id, e.Name);
        }, 'GET');
}

function SetCityId(cityId, comboId) {
    sendAjxForm({ cityId: cityId }, "/Utility/GetCityById",
        function (e) {
            if (e.Id > 0)
                funcCustomComboBoxSingle(comboId, e.Id, e.Name);
        }, 'GET');
}

function GetAllLoadTable(incomingUrl) {
    sendAjxForm("", incomingUrl, funcDataTable, "POST");
}

//Tender-> kurum ve müdürlük
function setInstitution(institutionId, comboId) {
    sendAjxForm({ id: institutionId }, "/Combo/GetInstitutionAddManagerCmbx",
        function (e) {
            if (e[0].id > 0)
                funcCustomComboBoxSingle(comboId, e[0].id, e[0].text);
        }, 'GET');
}