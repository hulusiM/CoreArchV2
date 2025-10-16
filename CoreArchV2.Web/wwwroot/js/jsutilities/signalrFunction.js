var connection = new signalR.HubConnectionBuilder().withUrl("/SignalRHub").build();
var connectionMap = new signalR.HubConnectionBuilder().withUrl("/ArventoMapHub").build();
var connectionMap2 = new signalR.HubConnectionBuilder().withUrl("/BasaranVehicleMapHub").build();

connection.start().then(function () {
    //loadSignalR();
}).catch(function (err) {
});

//connection.invoke('Mesaj', 'Merhaba SignalR');
connection.on("receivePnMessage", function (type, head, message) {
    ShowMessage(type, head, message);
    getNavbarMessages();
});

connection.on('setChatMessages', function (data) {
    //setTextMessage(data);
});

connection.on('onlineUser', function (data) {
    //loadOnlineUser();
});

connection.on('OnLeft', function () {
    var ss = 5;
});

function loadSignalR() {
    if (connection.connectionState == "Connected") //connectSuccess
    {
        var model = {
            connectionId: connection.connectionId,
            Url: window.location.pathname
        }
        $.ajax({
            url: window.location.origin + "/Login/UserConnectionInsertSignalR",
            type: "POST",
            dataType: "json",
            async: true,
            success: function (data) { },
            data: model,
            error: function (err) { }
        });
        //sendAjxForm(model, "/Login/UserConnectionInsertSignalR", function (e) { }, 'POST');
    }
}