"use strict";

angular.module("app").controller("MainCtrl", function ($http, myFactory) {
    this.ShowBaseFromMainCtrl = function () {
        console.log(myFactory.DevBase);
    }
});

angular.module("app").factory("myFactory", function () {
    return {
        DevBase: [],
        Rooms: [],
        DevCount: 0,
        Host: window.location.host
    }
});

angular.module("app").controller("appController", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.GetDevBase = function () {
        //var host = window.location.host;
        console.log("Host: " + myFactory.Host);
        $http.get(`http://${myFactory.Host}/Home/devbase`).
            then(function successCallback(response) {
                myFactory.DevBase = response.data;
                myFactory.DevCount = 0;
                for (let dev in myFactory.DevBase) {
                    myFactory.DevCount++;
                }
                console.log("Device count: " + myFactory.DevCount);
                console.log("devbase receive success!");
            }, function errorCallback(response) {
            });
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
    };
});

angular.module("app").controller("addNewDev", function ($http, myFactory) {
    this.myFactory = myFactory; //gives possibility to accecc myFactory in html tempplate page
    this.DevTypes = [
        {
            id: 0,
            name: "Пульт"
        }, {
            id: 1,
            name: "Силовой блок"
        }, {
            id: 2,
            name: "Силовой блок с обр. св."
        }, {
            id: 3,
            name: "Датчик"
        }];
    this.NooMods = [
        {
            id: 0,
            name: "Tx"
        }, {
            id: 1,
            name: "Rx"
        }, {
            id: 2,
            name: "F-Tx"
        }];
    var DevBase = this;
    let Status = this;
    this.OpenWindow = function () {
        window.open(`http://${myFactory.Host}/NewDevice/`);
    }

    this.GetRooms = function () {
        $http.get(`http://${myFactory.Host}/NewDevice/GetRooms`).then(
            function successCallback(response) {
                myFactory.Rooms = response.data;
                console.log(response.data);
            }, function errorCallback(response) {
            });
    }

    this.RoomSelected = function (name, room, type) {
        $http.get(`http://${myFactory.Host}/NewDevice/RoomSelected?name=${name}&room=${room}&mode=${type.id}`).then(
            function successCallback(response) {
                console.log(`Adding dev: \nName: ${name}\nType: ${type.name}\nRoom: ${room}\nChannel: ${response.data.channel} \nStatus: ${response.data.status}`);
                Status = response.data.status;
            }, function errorCallback(response) { });
    }

    this.BindClicked = function () {
        $http.get(`http://${myFactory.Host}/NewDevice/SendBind`).then(
            function successCallback(response) {
                Status = response.data.status;
            }, function errorCallback(response) {
            });
    }

    this.AddClicked = function () {
        $http.get(`http://${myFactory.Host}/NewDevice/Add`).then(
            function successCallback(response) {
                console.log(response.data);
                myFactory.DevBase.push(response.data);
            }, function errorCallback(response) { });
    }

    this.ConfirmAddDev = function (device) {
        console.log(device);
    }
});


