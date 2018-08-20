"use strict";
app.controller("appController", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.selectedRoom = "All";
    this.SetSelectedRoom = function (roomName) {
        this.selectedRoom = roomName;
        console.log(`Selected room tab: ${roomName}`);
    }

    this.SetSwitch = function (devKey) {
        console.log("clicked dev:" + devKey);
        $http.post(`http://${myFactory.Host}/Home/switchdev?devkey=${devKey}`);
    }
    this.SetBright = function (devkey, brightlvl) {
        $http.post(`http://${myFactory.Host}/Home/setbright?devkey=${devkey}&bright=${brightlvl}`);
    }

    this.UpdateDevView = function (rfdevice) {
        myFactory.DevBase[rfdevice.key] = rfdevice;
        console.log(`View of ${rfdevice.name} updated!`);
    }
    this.AddNew = function (rfdevice) {
        myFactory.AddToBase(rfdevice);
        myFactory.Status = "Ready";
    }
});