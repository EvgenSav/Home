"use strict";
var app = angular.module("app", ["ngRoute"]);
app.config(function ($routeProvider) {
    $routeProvider
        .when("/", {
            templateUrl: "/AngularViewTemplates/devices.html",
            controller: "appController"
        })
        .when("/addnew", {
            templateUrl: "/AngularViewTemplates/addnew.html",
            controller: "newDevController"
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












