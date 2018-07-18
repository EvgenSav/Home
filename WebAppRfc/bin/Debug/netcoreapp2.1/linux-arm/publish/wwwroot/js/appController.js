

angular.module("app").controller("appController", function ($scope, $http) {
    $scope.GetDevBase = function () {
        var host = window.location.hostname + ":" + window.location.port;
        console.log(window.location.hostname+":"+window.location.port);
        $http.get("http://"+host+"/Home/devbase").
            then(function successCallback(response) {
                if ($scope.DevBase == null) {
                    $scope.DevBase = {};
                }
                $scope.DevBase = response.data;
                console.log("devbase receive success!");
            }, function errorCallback(response) {
            });
    }
    $scope.SetSwitch = function (devKey) {
        console.log("clicked dev:" + devKey);
        $http.post("http://localhost:5000/Home/switchdev?devkey=" + devKey);
    }
    $scope.SetBright = function (devkey, brightlvl) {
        $http.post("http://localhost:5000/Home/setbright?devkey=" + devkey + "&bright=" + brightlvl);
    }

    $scope.UpdateDevView = function (rfdevice) {
        $scope.DevBase[rfdevice.key] = rfdevice;
        var stateLabel = document.getElementById("stateId" + rfdevice.key);
        if (rfdevice.state != 0) {
            stateLabel.textContent = "On";
            stateLabel.style.backgroundColor = "lightgreen";
        } else {
            stateLabel.textContent = "Off";
            stateLabel.style.backgroundColor = "white";
        }
        console.log("View of {rfddevice.value.name} updated!");
    };
    $scope.UpdateView = function () {
        console.log("view upd ng-change");
    }
});
