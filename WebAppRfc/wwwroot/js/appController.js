﻿

angular.module("app").controller("appController", function ($scope, $http) {
    $scope.GetDevBase = function () {
        var host = window.location.host;
        console.log("Host: " + host);
        $http.get("http://" + host + "/Home/devbase").
            then(function successCallback(response) {
                if ($scope.DevBase == null) {
                    $scope.DevBase = {};
                }
                $scope.DevBase = response.data;
                $scope.DevCount = 0;
                for (dev in $scope.DevBase) {
                    $scope.DevCount++;
                }
                console.log("Device count: " + $scope.DevCount);
                console.log("devbase receive success!");
            }, function errorCallback(response) {
            });
    }
    $scope.SetSwitch = function (devKey) {
        console.log("clicked dev:" + devKey);
        var host = window.location.host;
        $http.post("http://" + host + "/Home/switchdev?devkey=" + devKey);
    }
    $scope.SetBright = function (devkey, brightlvl) {
        var host = window.location.host;
        $http.post("http://" + host + "/Home/setbright?devkey=" + devkey + "&bright=" + brightlvl);
    }

    $scope.UpdateDevView = function (rfdevice) {
        $scope.DevBase[rfdevice.key] = rfdevice;
        console.log("View of " + rfdevice.name + " updated!");
    };
});
angular.module("app").controller("addNewDev", function ($scope, $http) {
    $scope.NooDevTypes = [
        {
            id: 0,
            name: "Tx"
        }, {
            id: 1,
            name:"Rx"
        }, {
            id: 2,
            name:"F-Tx"
        }];
    $scope.OpenWindow = function () {
        var host = document.location.host
        window.open("http://" + host + "/Hello/addNewDevice");
    }
    $scope.GetRooms = function () {
        var host = document.location.host
        $http.get("http://" + host + "/Hello/GetRooms").then(
            function successCallback(response) {
                $scope.Rooms = response.data;
                console.log(response.data);
            }, function errorCallback(response) {
        });
    }
    $scope.AddNewDev = function (name, type, room) {
        console.log("Adding dev: Name: " + name + "Type: " + type.name + "Room: " + room);
    }
    $scope.DevModeSelected = function (type) {
        console.log(type);
    }
});

