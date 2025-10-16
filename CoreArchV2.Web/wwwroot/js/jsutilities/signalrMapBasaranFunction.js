
connectionMap2.start().then(function () {
    //loadVehicles();
    loadGeoLocation2();
}).catch(function (err) {
    console.log("error: " + err);
});

connectionMap2.on("BasaranMapPosition", function (vehicleList) {
    loadVehicles2(vehicleList);
});