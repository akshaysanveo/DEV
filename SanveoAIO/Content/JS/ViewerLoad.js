

    viewer.window = '';
        function authMe() { return (_ACCESS_TOKEN_); }
        function initialize (u) {
            var globalOffset='';
            var options = {
        document: "urn:" + u,
    env: 'AutodeskProduction',
    getAccessToken: authMe,
    globalOffset:'',
};
            if(viewer){
                if(viewer.impl)
                {
                    if(viewer.impl.selector){
        viewer.tearDown();
    viewer.finish();
    viewer=null;
}
}
}

            var config3d = {
        extensions: ['MyAwesomeExtension']
};



var viewerElement = document.getElementById('viewer');
            viewer = new Autodesk.Viewing.Private.GuiViewer3D(viewerElement, {});
            Autodesk.Viewing.Initializer(options, function () {
        viewer.initialize();
    //console.log(u);
    for (var i = 0; i < u.length; i++) {
                    ////console.log(u[i]);
                    //loadDocument(viewer, "urn:" + u[i]);
                    if(i>0){
        viewer.addEventListener(Autodesk.Viewing.GEOMETRY_LOADED_EVENT, function () {
            globalOffset = viewer.model.getData().globalOffset;
            options = { globalOffset: globalOffset }
            //console.log("globalOffset");
            //console.log(globalOffset);
        });
    loadDocument(viewer, "urn:" + u[i]);
}
else
    loadDocument(viewer, "urn:" + u[i]);
}
});



            //Autodesk.Viewing.Initializer(options, function () {

        //    var config3d = {
        //        extensions: ['MyAwesomeExtension']
        //    };

        //    for (var i = 0; i < u.length; i++) {
        //        viewer = new Autodesk.Viewing.ViewingApplication('viewer');
        //        viewer.registerViewer(viewer.k3D, Autodesk.Viewing.Private.GuiViewer3D, config3d);
        //        viewer.loadDocument("urn:" + u[0], onDocumentLoadSuccess, onDocumentLoadFailure);
        //    }


        //});
    }

    function loadDocument(viewer, documentId) {
        Autodesk.Viewing.Document.load(
            documentId,
            function (doc) {
                var geometryItems = [];
                geometryItems = Autodesk.Viewing.Document.getSubItemsWithProperties(
                    doc.getRootItem(),
                    { 'type': 'geometry', 'role': '3d' },
                    true
                );

                viewer.loadExtension("Autodesk.InViewerSearch");
                $(".adsk-button-icon-search").addClass("fa fa-search");

                if (geometryItems.length <= 0) {
                    geometryItems = Autodesk.Viewing.Document.getSubItemsWithProperties(
                        doc.getRootItem(),
                        { 'type': 'geometry', 'role': '2d' },
                        true
                    );
                }
                if (geometryItems.length > 0)
                    viewer.load(
                        doc.getViewablePath(geometryItems[0])//,
                    );
            },
            function (errorMsg) { // onErrorCallback
                alert("Load Error: " + errorMsg);
            }
        );
    }

        function onDocumentLoadSuccess(doc) {

        // We could still make use of Document.getSubItemsWithProperties()
        // However, when using a ViewingApplication, we have access to the **bubble** attribute,
        // which references the root node of a graph that wraps each object from the Manifest JSON.

        console.log("onDocumentLoadSuccess(doc)");
    viewer.impl.canvas.addEventListener('mousedown', function (e) {

                 //Get 2D drawing dimension
                var layoutBox = viewer.impl.getVisibleBounds();
    var width = layoutBox.max.x - layoutBox.min.x;
    var height = layoutBox.max.y - layoutBox.min.y;

    var viewport = viewer.impl.clientToViewport(e.clientX, e.clientY);
    var point = [viewport.x * width, viewport.y * height, viewport.z];

    // Show the 2D drawing X, Y coordinates on mouse click
    console.log("point");
    console.log(point);
});
          viewer.bubble.search({'type': 'geometry' });
            if (viewables.length === 0) {
        console.error('Document contains no viewables.');
    return;
}
viewer.selectItem(viewables[0].data, onItemLoadSuccess, onItemLoadFail);
}

        function onDocumentLoadFailure(viewerErrorCode) {
        console.error('onDocumentLoadFailure() - errorCode:' + viewerErrorCode);
    }

        function onItemLoadSuccess(viewer, item) {

    }

    function onItemLoadFail(errorCode) {
        console.error('onItemLoadFail() - errorCode:' + errorCode);
    }


        function onDocumentLoadSuccess(doc) {

        console.log("onDocumentLoadSuccess(viewer, item)");
    viewer.impl.canvas.addEventListener('mousedown', function (e) {
                // Get 2D drawing dimension
                var layoutBox = viewer.impl.getVisibleBounds();
    var width = layoutBox.max.x - layoutBox.min.x;
    var height = layoutBox.max.y - layoutBox.min.y;

    var viewport = viewer.impl.clientToViewport(e.clientX, e.clientY);
    var point = [viewport.x * width, viewport.y * height, viewport.z];

    // Show the 2D drawing X, Y coordinates on mouse click
    console.log("point");
    console.log(point);
});


}


        function onItemLoadFail(errorCode) {
        console.error('onItemLoadFail() - errorCode:' + errorCode);
    }


        function onDocumentLoadFailure(viewerErrorCode) {
        console.error('onDocumentLoadFailure() - errorCode:' + viewerErrorCode);
    }

    var modelid = '';
        function onLoadModelError(viewerErrorCode) {
        console.error('onLoadModelError() - errorCode:' + viewerErrorCode);
    }

        function Forgeid(id) {
            //  var DextractText = "Dextracting " + DextractFileName;
            var selectedfilename = selectedfilename.trim();
    DextractFileName = DextractFileName.trim();

    //var afterDot = DextractFileName.substr(DextractFileName.lastIndexOf('.') + 1);
    if (DextractFileName == selectedfilename)
            {
                var id1 = parseInt(id);
    viewer.showAll();
    viewer.impl.selector.setSelection(id1, viewer.model);
    viewer.fitToView(id1);
    viewer.isolateById(id1);

    viewer.impl.selector.setSelection(id1, viewer.model);
    viewer.fitToView(id1);
    viewer.isolateById(id1);
}
else
            {
        alert('Please load Model for selected file');
    }

}


        function Forgeid(id, selectedfilename) {
            //  var DextractText = "Dextracting " + DextractFileName;
            var selectedfilename = selectedfilename.trim();
    DextractFileName = DextractFileName.trim();

    //var afterDot = DextractFileName.substr(DextractFileName.lastIndexOf('.') + 1);
    if (DextractFileName == selectedfilename)
            {
                var id1 = parseInt(id);
    viewer.showAll();
    viewer.impl.selector.setSelection(id1, viewer.model);
    viewer.fitToView(id1);
    viewer.isolateById(id1);

    viewer.impl.selector.setSelection(id1, viewer.model);

    viewer.fitToView(id1);
    viewer.isolateById(id1);
}
else
            {
        alert('Please load Model for selected file');
    }

}

function DisableElements(dbIds)
        {
        console.log(dbIds);
    //viewer.showAll();
    //viewer.hide(dbIds);
    var vm = new Autodesk.Viewing.Private.VisibilityManager(viewer.impl, viewer.model);
            for (var i = 0; i < dbIds.length; i++) {
        vm.setNodeOff(dbIds[i], true);
    }
}

        function EnableElements(urn){
            //console.log(dbIds);
            //var vm = new Autodesk.Viewing.Private.VisibilityManager(viewer.impl, viewer.model);
            //for (var i = 0; i < dbIds.length; i++) {
            //    vm.setNodeOn(dbIds[i], true);
            //}
            var dd = [];
    dd[0] = urn;
    console.log(urn);
    initialize(dd);
    // loadDocument(viewer,"urn:" +urn);
}

        function Load2d3d(){
            if (viewer==undefined) {
                var inURN=[];
    inURN.push($("#pu_FileToLoad option:selected").val());
    //console.log(inURN);
    _ACCESS_TOKEN_ = "";
    _URN_ = "";
                $.ajax({
        type: "POST",
    contentType: 'application/json',
    url: '@Url.Action("GetAuthentication", "Home")',
    dataType: "json",
                    success: function (result) {
        _ACCESS_TOKEN_ = result.auth;
    initialize(inURN);
    loadInitialModel($("#pu_FileToLoad option:selected").val());
},
});
}
            else{
        loadInitialModel($("#pu_FileToLoad option:selected").val());
    }
}

        function Show2D(){
            if(viewer!=undefined)
            {
        $('#pu_FileToLoad').val(global2dfilevalue);
    }
    if( $('#pu_viewToLoad').css('display') == 'none')
            {
        $('#pu_viewToLoad').css('display', 'block');
    $('#pu_FileToLoad').css('display','block');
    $(".draggable").css("display", "block");
    $('#txtSearch').css('display','none');
    $("#BindAutoText").css("display", "none");
    $("#Divviewer2D").data("kendoWindow").open();
  //  $("#btnAutoSearch").css("display", "none");
    $("#DivAutotext").data("kendoWindow").close();
}
else
            {
        $('#pu_viewToLoad').css('display', 'none');
    $('#pu_FileToLoad').css('display','none');
    $(".draggable").css("display", "none");
    $('#txtSearch').css('display','none');
    $("#BindAutoText").css("display", "none");
    $("#Divviewer2D").data("kendoWindow").close();
  //  $("#btnAutoSearch").css("display", "block");
    $("#DivAutotext").data("kendoWindow").close();
}

if((_URN_ == "" || _URN_ == undefined || _URN_ == null) || (Version_No == "" || Version_No == undefined || Version_No == null))
            {
        e.preventDefault();
    }
    else
            {
        Load2d3d();
    }
}

        $(document).ready(function () {
        $("#Divviewer2D").kendoWindow({
            modal: false,
            resizable: true,
            draggable: true,
            title: "2D view",
            width: "25%",
            height: "40%",
            position: {
                top: "35%",
                left: "70%"
            },
            close: function () {
                $('#pu_viewToLoad').css('display', 'none');
                $('#pu_FileToLoad').css('display', 'none');
            }
        }).data("kendoWindow");
    // $("#Divviewer2D").parent().find(".k-window-action").css("visibility", "hidden");

    var  kwindow= $("#DivAutotext").kendoWindow({
        modal: false,
    resizable: true,
    draggable: true,
    title:"Search",
    width:"35%",
    height:"65%",
                position: {
        top: "7%",
    left: "60%"
},
actions: ["Close"],
}).data("kendoWindow");
// $("#DivAutotext").parent().find(".k-window-action").css("visibility", "hidden");

            var  kwindow= $("#DivMarkUp").kendoWindow({
        modal: false,
    resizable: true,
    draggable: true,
    title:"Markup Extension",
    width:"25%",
    height:"38%",
                position: {
        top: "7%",
    left: "60%"
},
actions: ["Close"],
}).data("kendoWindow");

});


