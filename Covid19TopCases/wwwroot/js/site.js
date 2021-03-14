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
        dropdown.append('<option value="" selected="">All regions</option>'); //Adding "all" filter
        //Populate dropdown region in iteration
        $.each(response.data, function (key, entry) {
            dropdown.append($('<option></option>').attr('value', entry.iso).text(entry.name));
        });
        getGlobalReport(window.GlobalReportEndpoint); //Consume global data service when the page is initialized
    });
}


/*
=========================================================
* GLOBAL REPORT
=========================================================
*/
function getGlobalReport(url) {
    //Remove past elements from the grid
    $('#tbodyGrid').remove();
    //Preparing the request
    var regionIsoP = $('#cmbRegion').val();
    var request = {
        regionIso: regionIsoP
    }
    $('#thFilterType').text((request.regionIso === '') ? 'REGION' : 'PROVINCE'); //Setting the filter type at the column header of the grid
    console.log(request);
    executeAjaxAPI(url, request, 'POST', function (response) {
        console.log('[site.js] Service response => ' + url);
        console.log(response);
        var tbodyGrid = $("<tbody>", { id: 'tbodyGrid' });
        tbodyGrid.appendTo('#gridData');
        var nextTr = 1;
        //Populate grid in iteration
        $.each(response.data, function (key, entry) {
            var tr = $("<tr>", { id: 'trStat' + nextTr });
            var th = $("<th>", { scope: 'row', text: nextTr });
            var tdName = $("<td>", { text: entry.name });
            var tdCases = $("<td>", { text: entry.casesStr });
            var tdDeaths = $("<td>", { text: entry.deathsStr });
            $('#tbodyGrid').append(tr);
            $('#trStat' + nextTr).append(th);
            $('#trStat' + nextTr).append(tdName);
            $('#trStat' + nextTr).append(tdCases);
            $('#trStat' + nextTr).append(tdDeaths);
            nextTr++;
        });
        $('#loader').fadeOut();
    });
}

/*
=========================================================
* ONCLICK - REPORT BUTTON
=========================================================
*/
$('#btnReport').click(function () {
    getGlobalReport(window.GlobalReportEndpoint); //Consume global data service
});