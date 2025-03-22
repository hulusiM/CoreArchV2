
connectionMap.start().then(function () {
    //loadVehicles();
    loadGeoLocation();
}).catch(function (err) {
    console.log("error: " + err);
});

connectionMap.on("ReceiveVehiclePosition", function (vehicleList) {
    loadVehicles(vehicleList);
});


