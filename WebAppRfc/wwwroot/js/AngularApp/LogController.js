"use strict";

app.controller("logController", function (myFactory) {
    this.myFactory = myFactory;
    this.UpdateDevView = function (rfdevice) {
        myFactory.DevBase[rfdevice.key] = rfdevice;
        console.log(`View of ${rfdevice.name} updated!`);
    };
});