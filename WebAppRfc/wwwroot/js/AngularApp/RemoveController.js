"use strict";

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