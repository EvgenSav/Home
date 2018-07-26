"use strict";
var app = angular.module("app", ["ngRoute"]);
app.config(function ($routeProvider) {
    $routeProvider
        .when("/", {
            templateUrl: "/AngularViewTemplates/Devices.html",
            controller: "appController"
        })
        .when("/addnew", {
            templateUrl: "/AngularViewTemplates/addnew.html",
            controller: "addNewDev"
        })
});
app.factory("myFactory", function () {
    let devBase = {};
    return {
        Key: 999,
        get DevBase() {
            return devBase;
        },
        set DevBase(value) {
            devBase = value;
        },
        Rooms: [],
        DevCount: 0,
        Host: window.location.host,
        AddToBase: function (rfdev) {
            this.DevBase[rfdev.key] = rfdev;
        },
        AddTest: function () {
            var devItem = this.DevBase[3];
            var newDev = Object.create(devItem);
            newDev.key = this.Key;
            this.DevBase[this.Key] = newDev;
            this.Key++;
        }
    }
});

app.controller("MainCtrl", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.ShowBaseFromMainCtrl = function () {
        console.log(myFactory.DevBase);
    }
});

app.controller("appController", function ($http, myFactory) {
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

app.controller("addNewDev", function ($http, myFactory) {
    this.myFactory = myFactory; //gives possibility to accecc myFactory in html tempplate page
    this.Init = function () {
        console.log("myFactory.Devbase before request");
        console.log(myFactory.DevBase);
        $http.get(`http://${myFactory.Host}/Home/devbase`).then(
            function successCallback(response) {
                console.log("Devbase from request");
                console.log(response.data);
                myFactory.DevBase = response.data;
            }, function errorCallback(response) {
            });
        console.log("myFactory.Devbase after request");
        console.log(myFactory.DevBase);
        $http.get(`http://${myFactory.Host}/NewDevice/GetRooms`).then(
            function successCallback(response) {
                myFactory.Rooms = response.data;
                //console.log(response.data);
            }, function errorCallback(response) {
            });
    }
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
    let Status = this;
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
        console.log("myFactory before add:");
        console.log(myFactory.DevBase);
        $http.get(`http://${myFactory.Host}/NewDevice/Add`).then(
            function successCallback(response) { 
                console.log("Response data:");
                console.log(response.data);
                myFactory.AddToBase(response.data);
                console.log("myFactory after add:");
                console.log(myFactory.DevBase);
            }, function errorCallback(response) { });
    }

    this.ConfirmAddDev = function (device) {
        console.log(device);
    }
    this.ShowBaseFromNewDev = function () {
        console.log(myFactory.DevBase);
    }
});


