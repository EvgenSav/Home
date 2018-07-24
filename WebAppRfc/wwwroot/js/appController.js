

angular.module("app").controller("MainCtrl", function ($http, myFactory) {
    this.ShowBaseFromMainCtrl = function () {
        console.log(myFactory.DevBase);
    }
});

angular.module("app").factory("myFactory", function () {
    const DevBase = {};
    return DevBase;
});

angular.module("app").controller("appController", function ($http, myFactory) {
    this.myFactory = myFactory;
    this.GetDevBase = function () {
        var host = window.location.host;
        console.log("Host: " + host);
        $http.get(`http://${host}/Home/devbase`).
            then(function successCallback(response) {
                //if ($scope.DevBase == null) {
                //    $scope.DevBase = {};
                //}
                myFactory.DevBase = response.data;
                //$scope.DevBase = response.data;
                this.DevCount = 0;
                for (dev in myFactory.DevBase) {
                    this.DevCount++;
                }
                console.log("Device count: " + this.DevCount);
                console.log("devbase receive success!");
            }, function errorCallback(response) {
            });
    }

    this.SetSwitch = function (devKey) {
        console.log("clicked dev:" + devKey);
        var host = window.location.host;
        $http.post(`http://${host}/Home/switchdev?devkey=${devKey}`);
    }
    this.SetBright = function (devkey, brightlvl) {
        var host = window.location.host;
        $http.post(`http://${host}/Home/setbright?devkey=${devkey}&bright=${brightlvl}`);
    }

    this.UpdateDevView = function (rfdevice) {
        myFactory.DevBase[rfdevice.key] = rfdevice;
        console.log(`View of ${rfdevice.name} updated!`);
    };
});
angular.module("app").controller("addNewDev", function ($http, myFactory) {
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
    this.Rooms = [ "All", "lol", "LOLOLOLO"];

    this.OpenWindow = function () {
        var host = document.location.host
        window.open(`http://${host}/NewDevice/`);
    }

    this.GetRooms = function () {
        var host = document.location.host
        $http.get(`http://${host}/NewDevice/GetRooms`).then(
            function successCallback(response) {
                this.Rooms = response.data;
                console.log(response.data);
            }, function errorCallback(response) {
            });
    }
  
    this.RoomSelected = function (name, room, type) {
        var host = document.location.host;
        $http.get(`http://${host}/NewDevice/RoomSelected?name=${name}&room=${room}&mode=${type.id}`).then(
            function successCallback(response) {
                console.log("Adding dev: \nName: " + name + "\nType: " + type.name + "\nRoom: " + room + "\nChannel: " + response.data.channel + "\nStatus: " + response.data.status);
                this.Status = response.data.status;
            }, function errorCallback(response) { });
    }

    this.BindClicked = function () {
        var host = document.location.host;
        $http.get(`http://${host}/NewDevice/SendBind`).then(
            function successCallback(response) {
                this.Status = response.data.status;
            }, function errorCallback(response) {
            });
    }

    this.AddClicked = function () {
        var host = document.location.host;
        $http.get(`http://${host}/NewDevice/Add`).then(
            function successCallback(response) {
                console.log(response.data);
                myFactory.DevBase=response.data;
            }, function errorCallback(response) { });
    }

    this.ConfirmAddDev = function (device) {
        console.log(device);
    }
});


