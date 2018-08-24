// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("UpdateDevView", function (rfdevice) {
    var appCtrlScope = angular.element(document.getElementById("app_controller")).scope();
    try {
        appCtrlScope.$apply(function () {
            appCtrlScope.devView.UpdateDevView(rfdevice.value);
        });
    } catch (err) {
        appCtrlScope = angular.element(document.getElementById("log_controller")).scope();
        try {
            appCtrlScope.$apply(function () {
                appCtrlScope.devLog.UpdateDevView(rfdevice.value);
            });
        } catch (err) {
        };
    } finally {
        console.log("Device info updated!");
    }


    console.log(rfdevice.value);
});

connection.on("BindReceived", function (rfdevice, status) {
    var appCtrlScope = angular.element(document.getElementById("addNewDev_controller")).scope();
    appCtrlScope.$apply(function () {
        appCtrlScope.new.BindReceived(rfdevice, status);
    });
});

connection.on("AddNewResult", function (rfdevice, status) {
    var appCtrlScope = angular.element(document.getElementById("app_controller")).scope();
    try {
        appCtrlScope.$apply(function () {
            if (status == "Device added") {
                appCtrlScope.devView.AddNew(rfdevice);
            }
            appCtrlScope.devView.myFactory.Status = status;
        });
    } catch (err) {
        appCtrlScope = angular.element(document.getElementById("addNewDev_controller")).scope();
        appCtrlScope.$apply(function () {
            if (status == "Device added") {
                appCtrlScope.new.myFactory.AddToBase(rfdevice);
            }
            appCtrlScope.new.myFactory.Status = status;
        });
    }

    console.log(status);
    console.log(rfdevice.value);
});

connection.on("RemoveResult", function (devices, status) {
    var appCtrlScope = angular.element(document.getElementById("app_controller")).scope();
    try {
        appCtrlScope.$apply(function () {
            if (status == "ok") {
                appCtrlScope.devView.myFactory.DevBase = devices;
            }
        });
    } catch (err) {
        appCtrlScope = angular.element(document.getElementById("remove_controller")).scope();
        appCtrlScope.$apply(function () {
            if (status == "ok") {
                appCtrlScope.remove.myFactory.DevBase = devices;
            }
        });
    }
    console.log("RemoveResult at client side");
    console.log(status);
    console.log(devices);
});

connection.start().catch(err => console.error(err.toString()));

