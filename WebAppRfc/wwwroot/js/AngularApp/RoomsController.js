"use strict";

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