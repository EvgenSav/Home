﻿"use strict";

app.controller("newDevController", function ($http, myFactory) {
    this.myFactory = myFactory; //gives possibility to accecc myFactory in html tempplate page
    this.bindingStep = 0;
    let devToBind = {};
    let NewDev = {};
    this.GetRooms = function () {
        $http.get(`http://${myFactory.Host}/Rooms/GetRooms`).then(
            function successCallback(response) {
                myFactory.Rooms = response.data;
                console.log(response.data);
            }, function errorCallback(response) {
            });
    };
    this.RoomSelected = function (newdev) {
        console.log('Room selected:');
        console.log(newdev);
    };

    this.StartBind = function (newdev) {
        let newDev = {
            Name: newdev.Name,
            Room: newdev.SelectedRoom,
            DevType: newdev.SelectedType.id
        };
        NewDev = newDev;
        console.log('Start bind:');
        console.log(newDev);
        $http.post(`http://${myFactory.Host}/AddDevice/StartBind`, newDev).then(
            function successCallback(response) {
                console.log(response.data);
                myFactory.Status = response.data.status;
            }, function errorCallback(response) {
            }
        );
        this.bindingStep = 1;
    };
    this.EnterService = function () {
        this.bindingStep = 2;
    };
    this.Bind = function () {
        $http.get(`http://${myFactory.Host}/AddDevice/SendBind`).then(
            function successCallback(response) {
                myFactory.Status = response.data.status;
            }, function errorCallback(response) {
            }
        );
        this.bindingStep = 3;
    };

    //calls by server
    this.BindReceived = function (device, status) {
        myFactory.Status = status;
        console.log(`Received bind for:`);
        console.log(device);
        console.log(status);
        devToBind = device;
    };

    this.CheckBind = function () {
        $http.post(`http://${myFactory.Host}/AddDevice/CheckBind`, devToBind).then(
            function successCallback(response) {
            }, function errorCallback(response) {
            }
        );
        this.bindingStep = 4;
    };

    this.CancelBind = function () {
        $http.post(`http://${myFactory.Host}/AddDevice/CancelBind`, devToBind).then(
            function successCallback(response) {
            }, function errorCallback(response) {

            }
        );
        this.bindingStep = 0;
    };
    this.Add = function () {
        $http.get(`http://${myFactory.Host}/AddDevice/Add`).then(
            function successCallback(response) {
                myFactory.AddToBase(response.data.device);
                myFactory.Status = response.data.status;
            }, function errorCallback(response) { });
        this.Dev.Name = "";
        this.Dev.SelectedType = {};
        this.Dev.SelectedRoom = "";
        this.bindingStep = 0;
    };
});