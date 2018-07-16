

angular.module("app").controller("appController", function ($scope, $http) {
    $scope.SetBright = function (devkey,brightlvl) {
        $http.post("http://192.168.0.100/Home/setbright?devkey=" + devkey + "&bright=" + brightlvl);
    }
    $scope.GetDevBase = function () {
        $http.get("http://192.168.0.100/Home/devbase").
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
        $http.post("http://192.168.0.100/Home/switchdev?devkey=" + devKey).
            then(function successCallback(response) {
                console.log(response);
                if ($scope.State == null) {
                    $scope.State = {};
                }
                $scope.State[devKey] = response.data.state;
                //console.log(State);
                if ($scope.Bright == null) {
                    $scope.Bright = {};
                }
                $scope.Bright[devKey] = response.data.bright;
                //console.log(Bright);
            }, function errorCallback(response) { });
    }
    $scope.UpdateDevView = function (rfdevice) {
        $scope.DevBase[rfdevice.value.key] = rfdevice.value;
    };
});
