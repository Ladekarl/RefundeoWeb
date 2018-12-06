var map;
var infowindow;
var searchwords = "agence+web";

// Initiate Map
function initMap() {
    var paris = {lat: 48.8704907, lng: 2.3309359};

    map = new google.maps.Map(document.getElementById('map'), {
        center: paris,
        zoom: 13,
        styles: [{
            stylers: [{ visibility: 'simplified' }]
        }, {
            elementType: 'labels',
            stylers: [{ visibility: 'off' }]
        }]
    });

    infowindow = new google.maps.InfoWindow();

    var populationOptions = {
        strokeColor: '#FF0000',
        strokeOpacity: 0.1,
        strokeWeight: 2,
        fillColor: '#FF0000',
        fillOpacity: 0.075,
        map: map,
        center: paris,
        radius: 7000
    };
    // Add the circle for this city to the map.
    cityCircle = new google.maps.Circle(populationOptions);

    var request = {
        location: paris,
        radius: 7000
    }

    var service = new google.maps.places.PlacesService(map);
    service.nearbySearch(request, callback);
}

var agencies = [];

function callback(results, status) {
    console.log(results);
    if (status === google.maps.places.PlacesServiceStatus.OK) {
        for (var i = 0; i < results.length; i++) {

            //Using setTimeout and closure because limit of 10 queries /second for getDetails */
            (function (j) {
                var request = {
                    placeId: results[i]['place_id']
                };

                service = new google.maps.places.PlacesService(map);
                setTimeout(function() {
                    service.getDetails(request, callback);
                }, j*100);


            })(i);

            function callback(place, status) {
                if (status == google.maps.places.PlacesServiceStatus.OK) {
                    createMarker(place);
                    console.log(place.name +  results.length + agencies.length);
                    agencies.push([place.name, place.website, place.rating]);

                    // if(results.length == agencies.length){
                    //     console.log(agencies);
                    //     var request = new XMLHttpRequest();
                    //     request.open('POST', 'http://localhost/agency-map/src/save.php', true);
                    //     request.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded; charset=UTF-8');
                    //     request.send(JSON.stringify(agencies));
                    // }
                }
            }
        }
    }
}

function createMarker(place) {
    var photos = place.photos;
    if (!photos) {
        return;
    }
    var placeLoc = place.geometry.location;
    var marker = new google.maps.Marker({
        map: map,
        position: place.geometry.location,
        title: place.name,
        icon: photos[0].getUrl({'maxWidth': 50, 'maxHeight': 50})
    });

    google.maps.event.addListener(marker, 'click', function() {
        infowindow.setContent(place.name + " : " + place.website);
        infowindow.open(map, this);
    })
}
