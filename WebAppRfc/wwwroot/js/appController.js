

angular.module("app").controller("appController", function ($scope, $http) {
    $scope.GetDevBase = function () {
        var host = window.location.host;
        console.log("Host: ${host}");
        $http.get("http://" + host + "/Home/devbase").
            then(function successCallback(response) {
                if ($scope.DevBase == null) {
                    $scope.DevBase = {};
                }
                $scope.DevBase = response.data;
                console.log("devbase receive success!");
                for (var rfdev in $scope.DevBase) {
                    $scope.UpdateView(rfdev);
                }
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
    $scope.UpdateView = function (rfdev) {
        console.log("view upd ng-change");
        var stateLabel = document.getElementById("stateId" + rfdev.key);
        if (rfdev.state != 0) {
            stateLabel.textContent = "On";
            stateLabel.style.backgroundColor = "lightgreen";
        } else {
            stateLabel.textContent = "Off";
            stateLabel.style.backgroundColor = "white";
        }
    }
});
