$(window).load(function() {
    $(".loader").fadeOut(1000, function () { $("#loader").hide() });//.fadeOut("slow", 0.4);
});

// Numeric only control handler
$.fn.ForceNumericOnly = function () {
    $(this).keydown(function (e) {
        var term = e.key || String.fromCharCode(e.keyCode);
        var key = e.charCode || e.keyCode || 0;
        var re = new RegExp("^([0-9])$");
        if ((re.test(term) || (key === 8 || key === 9 || key === 13 || key === 27 || key === 35 || key === 36 || key === 38 || key === 39 || key === 40 || key === 46) /*Teclas direccinales y de control*/)) {
            return true;
        } else {
            return false;
        }
    });
};

// AlfaNumeric WithOut Point only control handler
$.fn.ForceAlfaNumericWithoutPoint = function () {
    $(this).keydown(function (e) {
        var term = e.key;
        var key = e.charCode || e.keyCode || 0;
        var re = new RegExp("^([a-zñA-ZÑ0-9-])$");
        if ((re.test(term) || (key === 8 || key === 9 || key === 13 || key === 27 || key === 35 || key === 36 || key === 38 || key === 39 || key === 40 || key === 46) /*Teclas direccinales y de control*/)) {
            return true;
        } else {
            return false;
        }
    });
};

// AlfaNumeric With Point only control handler
$.fn.ForceAlfaNumericWithPoint = function () {
    $(this).keydown(function (e) {
        var term = e.key;
        var key = e.charCode || e.keyCode || 0;
        var re = new RegExp("^([a-zñA-ZÑ0-9.-])$");
        if ((re.test(term) || (key === 8 || key === 9 || key === 13 || key === 27 || key === 35 || key === 36 || key === 38 || key === 39 || key === 40 || key === 46) /*Teclas direccinales y de control*/)) {
            return true;
        } else {
            return false;
        }
    });
};

// AlfaNumeric With Point only control handler
$.fn.ForceAlfaNumericDescriptionService = function () {
    $(this).keydown(function (e) {
        var term = e.key;
        var key = e.charCode || e.keyCode || 0;
        var re = new RegExp("^[^'']*$");
        if ((re.test(term) || (key === 8 || key === 9 || key === 13 || key === 27 || key === 35 || key === 36 || key === 38 || key === 39 || key === 40 || key === 46) /*Teclas direccinales y de control*/)) {
            return true;
        } else {
            return false;
        }
    });
};


///Funcion para abrir el modal solicitado.
var funcOpenModal = function (titelModal, dvModal, gridModal, ctrlCodModal, ctrlDescripModal) {
    if (gridModal.data("kendoGrid") !== undefined) {
        gridModal.data("kendoGrid").dataSource.data([]);
        gridModal.removeClass("k-grid k-widget k-secondary").empty();
    }
    ctrlCodModal.data("kendoAutoComplete").value("");
    ctrlDescripModal.data("kendoAutoComplete").value("");
    dvModal.show();
    dvModal.kendoWindow({
        modal: true,
        width: "50%",
        //height: "50%",
        title: titelModal,
        visible: false,
        position: {
            top: "100px",
            left: "20%"
        },
        actions: [
            //"Pin",
            //"Minimize",
            //"Maximize",
            "Close"
        ],
    }).data("kendoWindow").center().open();
    dvModal.scrollTop(0);
};

//Función para crear la grilla de los modal de busqueda cuando sea solicitado
var CreateGridBusqueda = function (data, columns, grid, dvModal, scrollTop) {
    grid.kendoGrid({
        dataSource: {
            type: 'odata',
            pageSize: 20,
            data: data
        },
        selectable: true,
        scrollable: true,
        //height: (data.length > 20 ? "750px" : "auto"),
        dataBound: function (e) {
            $('.k-grid table').height($(window).height() - 1850);
        },
        sortable: true,
        pageable: {
            buttonCount: 10
        },
        columns: columns
    }).data("kendoGrid");

    dvModal.closest(".k-window").css({
        top: scrollTop,
        left: 450
    });

    console.log("scrollTop: ", scrollTop)
    var scrollTop1 = $(window).scrollTop();
    console.log("scrollTop 1: ", scrollTop1)
    $(window).scrollTop(scrollTop);
    console.log("scrollTop 3: ", $(window).scrollTop())
}

