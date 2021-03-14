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
                swalWarnMessage('Warning', objError.responseJSON.message);
                $('#loader').fadeOut();
            },
            400: function (objError) {
                console.warn('[site.js] StatusCode Response => 400 Bad Request');
                console.log(objError);
                swalWarnMessage('Warning', objError.responseJSON.message);
                $('#loader').fadeOut();
            },
            404: function (objError) {
                console.warn('[site.js] StatusCode Response => 404 Not Found');
                console.log(objError);
                swalWarnMessage('Warning', objError.responseJSON.message);
                $('#loader').fadeOut();
            },
            500: function (objError) {
                console.warn('[site.js] StatusCode Response => 500 Internal Server Error');
                console.log(objError);
                swalErrorMessage('ERROR', objError.responseJSON.message);
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
        //Populate grid in iteration
        $.each(response.data, function (key, entry) {
            var tr = $("<tr>", { id: 'trStat' + entry.item });
            var th = $("<th>", { scope: 'row', text: entry.item });
            var tdName = $("<td>", { text: entry.name });
            var tdCases = $("<td>", { text: entry.casesStr });
            var tdDeaths = $("<td>", { text: entry.deathsStr });
            $('#tbodyGrid').append(tr);
            $('#trStat' + entry.item).append(th);
            $('#trStat' + entry.item).append(tdName);
            $('#trStat' + entry.item).append(tdCases);
            $('#trStat' + entry.item).append(tdDeaths);
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

/*
=========================================================
* ONCLICK - XML DOWNLOAD FILE BUTTON
=========================================================
*/
$('#btnXML').click(function () {
    downloadFile(window.XMLDownloadEndpoint, window.DownloadedFileName, 'xml');
});

/*
=========================================================
* ONCLICK - JSON DOWNLOAD FILE BUTTON
=========================================================
*/
$('#btnJSON').click(function () {
    downloadFile(window.JSONDownloadEndpoint, window.DownloadedFileName, 'json');
});

/*
=========================================================
* ONCLICK - CSV DOWNLOAD FILE BUTTON
=========================================================
*/
$('#btnCSV').click(function () {
    downloadFile(window.CSVDownloadEndpoint, window.DownloadedFileName, 'csv');
});

/*
=========================================================
* DOWNLOAD FILE (GENERIC FUNCTION)
=========================================================
*/
function downloadFile(urlService, fileName, fileExtension) {
    fetch(urlService, { cache: 'no-cache' })
        .then(resp => resp.blob())
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            a.download = fileName + '.' + fileExtension; //Filename
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            swalSuccessMessage('Your XML file has been created successfully.')
        })
        .catch(() => swalErrorMessage('ERROR', 'An error has ocurred during the XML file creation'));
}

/*
=========================================================
* SWEETALERT (SUCCESS ALERTS)
=========================================================
*/
function swalSuccessMessage(successMsg) {
    removeScroll();
    Swal.fire({
        icon: 'success',
        html: '<p>' + successMsg + '</p>',
        showClass: {
            popup: 'animated fadeInDown faster'
        },
        hideClass: {
            popup: 'animated fadeOutUp faster'
        },
        timer: 3000,
        timerProgressBar: true,
        showConfirmButton: false,
        showCancelButton: false,
    }).then(function (result) {
        addScroll();
    });
}

/*
=========================================================
* SWEETALERT (ALERT MESSAGES)
=========================================================
*/
function swalWarnMessage(title, warnMsg) {
    removeScroll();
    Swal.fire({
        icon: 'warning',
        title: title,
        text: warnMsg,
        showClass: {
            popup: 'animated fadeInDown faster'
        },
        hideClass: {
            popup: 'animated fadeOutUp faster'
        },
        confirmButtonColor: '#757575',
        confirmButtonText: 'OK'
    }).then(function (result) {
        addScroll();
    });
}

/*
=========================================================
* SWEETALERT (ERROR ALERT)
=========================================================
*/
function swalErrorMessage(title, errorMsg) {
    removeScroll();
    Swal.fire({
        icon: 'error',
        title: title,
        text: errorMsg,
        showClass: {
            popup: 'animated fadeInDown faster'
        },
        hideClass: {
            popup: 'animated fadeOutUp faster'
        },
        confirmButtonColor: '#757575',
        confirmButtonText: 'OK'
    }).then(function (result) {
        addScroll();
    });
}

/*
=========================================================
* REMOVE SCROLL FOR PROBLEMS IN MOBILES - MODALS
=========================================================
*/
var scrollPosition = 0;

function removeScroll() {
    scrollPosition = $(window).scrollTop();
    $('html, body').css({ overflow: 'hidden' });
};

/*
=========================================================
* ADD SCROLL FOR PROBLEMS IN MOBILES - MODALS
=========================================================
*/
function addScroll() {
    $('html, body').css({ overflow: 'auto', });
    $('html, body').animate({ scrollTop: scrollPosition }, 0);
};