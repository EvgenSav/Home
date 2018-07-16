// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("UpdateDevView", function (rfdevice) {
    var ctrlscope = angular.element(document.getElementById("app_controller")).scope();
    ctrlscope.$apply(function () {
        ctrlscope.UpdateDevView(rfdevice);
    })
    console.log(rfdevice.value);
});

connection.start().catch(err => console.error(err.toString()));

