"use strict";

app.controller("mainCtrl", function ($http, myFactory) {
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