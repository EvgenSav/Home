﻿"use strict";
var app = angular.module("app", ["ngRoute"]);
app.config(function ($routeProvider) {
    $routeProvider
        .when("/", {
            templateUrl: "/AngularViewTemplates/devices.html",
            controller: "appController"
        })
        .when("/addnew", {
            templateUrl: "/AngularViewTemplates/addnew.html",
            controller: "addNewDev"
        })
        .when("/rooms", {
            templateUrl: "/AngularViewTemplates/rooms.html",
            controller: "roomsController"
        })
        .when("/log", {
            templateUrl: "/AngularViewTemplates/log.html",
            controller: "logController"
        })
        .when("/remove", {
            templateUrl: "/AngularViewTemplates/remove.html",
            controller: "removeController"
        })
});

app.factory("myFactory", function ($location) {
    let devBase = {};

    return {
        Status: "Nothing yet...",
        CurLogKey: 0,
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
            this.DevCount++;
        },
        AddTest: function () {
            var devItem = this.DevBase[3];
            var newDev = Object.create(devItem);
            newDev.key = this.Key;
            this.DevBase[this.Key] = newDev;
            this.Key++;
        },
        isActive: function (viewlocation) {
            return viewlocation === $location.path();
        },
        UpdateDevView: function (rfdevice) {
            this.DevBase[rfdevice.key] = rfdevice;
            console.log(`View of ${rfdevice.name} updated!`);
        }
    }
});

app.controller("MainCtrl", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.GetBase = function () {
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
        $http.get(`http://${myFactory.Host}/Rooms/GetRooms`).then(
            function successCallback(response) {
                myFactory.Rooms = response.data;
                //console.log(response.data);
            }, function errorCallback(response) {
            });
    }
    this.ShowBaseFromMainCtrl = function () {
        console.log(myFactory.DevBase);
    }
});

app.controller("removeController", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.SendUnbind = function (dev) {
        $http.post(`http://${myFactory.Host}/RemoveDevice/Unbind?devkey=${dev.key}`)
            .then(
                function successCallbask(response) {
                    //console.log(response.data);
                }, function errorCallbask(response) {

                }
            );
    };
    this.CheckUnbind = function (dev) {
        $http.post(`http://${myFactory.Host}/Home/SwitchDev?devkey=${dev.key}`)
            .then(
                function successCallbask(response) {
                    console.log(response.data);
                }, function errorCallbask(response) {

                }
            );
    };
    this.RemoveDev = function (dev) {
        $http.post(`http://${myFactory.Host}/RemoveDevice/RemoveDev?devkey=${dev.key}`)
            .then(
            function successCallbask(response) {
                console.log("Response on RemoveDev" + response);
                }, function errorCallbask(response) {

                }
            );
    };
   
});
app.controller("logController", function (myFactory) {
    this.myFactory = myFactory;
    this.UpdateDevView = function (rfdevice) {
        myFactory.DevBase[rfdevice.key] = rfdevice;
        console.log(`View of ${rfdevice.name} updated!`);
    };
});

app.controller("roomsController", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.AddRoom = function (roomName) {
        $http.post(`http://${myFactory.Host}/Rooms/AddRoom?roomName=${roomName}`).then(
            function successCallback(response) {
                myFactory.Rooms = response.data;
            }, function errorCallback(reponse) {

            });
    }
    this.RemoveRoom = function (roomName) {
        $http.post(`http://${myFactory.Host}/Rooms/RemoveRoom?roomName=${roomName}`).then(
            function successCallback(response) {
                myFactory.Rooms = response.data;
            }, function errorCallback(reponse) {

            });
    }
});
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

app.controller("addNewDev", function ($http, myFactory) {
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
    this.GetRooms = function () {
        $http.get(`http://${myFactory.Host}/Rooms/GetRooms`).then(
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
                myFactory.Status = response.data.status;
            }, function errorCallback(response) { });
    }
    this.BindClicked = function () {
        $http.get(`http://${myFactory.Host}/NewDevice/SendBind`).then(
            function successCallback(response) {
                myFactory.Status = response.data.status;
            }, function errorCallback(response) {
            });
    }
    this.BindReceived = function (device) {
        console.log(`Received bind for ${device.name}`);
        this.Device = device;
    };
    this.ShowBaseFromNewDev = function () {
        console.log(myFactory.DevBase);
    }
    this.CheckBind = function () {

    }
    this.Add = function () {
        console.log("myFactory before add:");
        console.log(myFactory.DevBase);
        $http.get(`http://${myFactory.Host}/NewDevice/Add`).then(
            function successCallback(response) {
                //console.log("Response data:");
                //console.log(response.data);
                //myFactory.AddToBase(response.data.device);
                //myFactory.Status = response.data.status;
                //console.log("myFactory after add:");
                //console.log(myFactory.DevBase);
            }, function errorCallback(response) { });
        this.Name = "";
        this.selectedType = {};
        this.selectedRoom = "";
    }
});


