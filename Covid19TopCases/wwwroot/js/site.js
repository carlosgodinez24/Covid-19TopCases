/*
=========================================================
* DOCUMENT - READY
=========================================================
*/
$(document).ready(function () {
    getRegionsCatalog(window.RegionsCatalogEndpoint); //Consume region service catalog when the page is initialized
    getGlobalReport(window.GlobalReportEndpoint); //Consume global data service when the page is initialized 
});

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
* REGIONS CATALOG
=========================================================
*/
function getRegionsCatalog(url) {
    executeAjaxAPI(url, null, 'GET', function (response) {
        console.log('[site.js] url regions => ' + url);
        console.log(response);
        $('#combobox').html($('#regionsTemplate').render(response));
        $('#regionSelect').addClass('selectpicker'); //Adding 'selectpicker' class once the component is rendered
        $('.selectpicker').selectpicker('refresh'); //Init and refresh selectpicker function
    });
}

/*
=========================================================
* GLOBAL REPORT
=========================================================
*/
function getGlobalReport(url) {
    //Preparing the request
    var request = {
        regionIso: $('#regionSelect').val() ? $('#regionSelect').val() : ''
    }
    console.log('[site.js] url global report => ' + url);
    console.log('[site.js] Request global report => ');
    console.log(request);
    executeAjaxAPI(url, request, 'POST', function (response) {
        response.data.gridHeader = request.regionIso === '' ? 'REGION' : 'PROVINCE'; //Adding "gridHeader" property
        console.log(response);
        $('#grid').html($('#globalReportTemplate').render(response));
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
            swalSuccessMessage('Your ' + fileExtension.toUpperCase() +' file has been created successfully.')
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