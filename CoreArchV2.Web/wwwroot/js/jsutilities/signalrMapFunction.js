
connectionMap.start().then(function () {
    //loadVehicles();
    loadGeoLocation();
}).catch(function (err) {
    console.log("error: " + err);
});

connectionMap.on("ArventoMapPosition", function (vehicleList) {
    loadVehicles(vehicleList);
});

