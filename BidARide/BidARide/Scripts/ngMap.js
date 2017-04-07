var app = angular.module('myApp', ['uiGmapgoogle-maps']);
app.controller('mapController', function ($scope, $http) {
    $scope.map = { center: { latitude: 10.59, longitude: 106 }, zoom: 15 }

    $scope.markers = [];
    $scope.lstdrivers = [];
    $scope.distance = 0;
    $scope.price = 0;
    $scope.noti = "";
    $http.get('/trip/GetDirection').then(function (data) {
        $scope.distance = data.data.distance_text;
        $scope.price = data.data.distance_value / 1000 * 4000;

        var map = new google.maps.Map(document.getElementById('map'), {
            zoom: 13,
            center: new google.maps.LatLng(data.data.start.lat, data.data.start.lng),
            mapTypeId: google.maps.MapTypeId.ROADMAP
        });

        var infowindow = new google.maps.InfoWindow();

        var marker, i;
        marker = new google.maps.Marker({
            position: new google.maps.LatLng(data.data.start.lat, data.data.start.lng),
            map: map
        });

        marker = new google.maps.Marker({
            position: new google.maps.LatLng(data.data.end.lat, data.data.end.lng),
            map: map,
            icon: "http://maps.google.com/mapfiles/ms/icons/green-dot.png"
        });

        $http.get('/trip/GetNearDriver').then(function (data) {
            $scope.lstdrivers = data.data;
            if ($scope.lstdrivers.length == 0)
            {
                $scope.noti = "No driver near your location.";
            }
            for (i = 0; i < $scope.lstdrivers.length; i++) {
                marker = new google.maps.Marker({
                    position: new google.maps.LatLng($scope.lstdrivers[i].latitude, $scope.lstdrivers[i].longtitude),
                    map: map,
                    icon: "../Content/bike.png"
                });
                google.maps.event.addListener(marker, 'click', (function (marker, i) {
                    return function () {
                        infowindow.setContent($scope.lstdrivers[i].driverFullname);
                        infowindow.open(map, marker);

                        $scope.driver.drivername = $scope.lstdrivers[i].driverFullname;
                        $scope.driver.driverphone = $scope.lstdrivers[i].driverPhone;
                    }
                })(marker, i));
            }
        }, function () {
            console.log(data);
            alert("Error");
        });
    }, function () {
        console.log(data);
        alert("Error");
    });


    $scope.booking = function (l) {
        $http.post('/trip/Booking/', l).success(function (data) {
            alert("Booking Successfully!!");

        }).error(function (data) {
            alert("Booking Error!!");
        });
    }
});

var getLocation = function (address) {
    var geocoder = new google.maps.Geocoder();
    var location = [];
    geocoder.geocode({ 'address': address }, function (results, status) {

        if (status == google.maps.GeocoderStatus.OK) {
            location[0] = results[0].geometry.location.lat();
            location[1] = results[0].geometry.location.lng();
        }
    });

    return location;
}