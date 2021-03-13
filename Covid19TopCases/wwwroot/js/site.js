/*
=========================================================
* AJAX REQUEST API
=========================================================
*/
function executeAjaxAPI(url, request, requestType, callback) {
    $('#loader').fadeIn();
    $.ajax({
        cache: false,
        url: url,
        data: JSON.stringify(request),
        dataType: 'json',
        type: requestType,
        contentType: 'application/json; charset=utf-8',
        statusCode: {
            200: function (data) {
                console.info('[site.js] StatusCode Response => 200 OK');
                callback(data);
                //$('#loader').fadeOut();
            },
            401: function (objError) {
                console.warn('[site.js] StatusCode Response => 401 Unauthorized');
                console.log(objError);
                $('#loader').fadeOut();
            },
            400: function (objError) {
                console.warn('[site.js] StatusCode Response => 400 Bad Request');
                console.log(objError);
                $('#loader').fadeOut();
            },
            404: function (objError) {
                console.warn('[site.js] StatusCode Response => 404 Not Found');
                console.log(objError);
                $('#loader').fadeOut();
            },
            500: function (objError) {
                console.warn('[site.js] StatusCode Response => 500 Internal Server Error');
                console.log(objError);
                $('#loader').fadeOut();
            }
        }
    });
}

/*
=========================================================
* DOCUMENT - READY
=========================================================
*/
$(document).ready(function () {
    getRegionsCatalog(window.RegionsCatalogEndpoint);
});

/*
=========================================================
* REGIONS CATALOG
=========================================================
*/
function getRegionsCatalog(url) {
    executeAjaxAPI(url, null, 'GET', function (response) {
        console.log('[site.js] Service response => ' + url);
        console.log(response);
        let dropdown = $('#cmbRegion');
        $.each(response.data, function (key, entry) {
            dropdown.append($('<option></option>').attr('value', entry.iso).text(entry.name));
        });
        $('#loader').fadeOut(); //Comment later
    });
}

