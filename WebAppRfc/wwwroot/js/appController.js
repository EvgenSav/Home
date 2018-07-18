

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
