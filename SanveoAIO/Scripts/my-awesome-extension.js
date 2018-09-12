function MyAwesomeExtension(viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
}

MyAwesomeExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
MyAwesomeExtension.prototype.constructor = MyAwesomeExtension;

MyAwesomeExtension.prototype.load = function () {
    alert('MyAwesomeExtension is loaded!');
    return true;
};

MyAwesomeExtension.prototype.unload = function () {
    alert('MyAwesomeExtension is now unloaded!');
    return true;
};

MyAwesomeExtension.prototype.load = function () {
    // alert('MyAwesomeExtension is loaded!');

    var viewer = this.viewer;

    var PolylineButton = document.getElementById('PolylineButton');
    PolylineButton.addEventListener('click', function () {
        viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
        NOP_VIEWER.loadExtension('Autodesk.Viewing.MarkupsCore').then(function (markupsExt) {
            markup = markupsExt;
            console.log(markup);
        });

        viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, function (event) {

            markup.enterEditMode();
            var cloud = new Autodesk.Viewing.Extensions.Markups.Core.EditModePolyline(markup)
            markup.changeEditMode(cloud)
        });

    });

    var unlockBtn = document.getElementById('UnloadMarkup');
    unlockBtn.addEventListener('click', function () {
        viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
    });

    var RectangleButton = document.getElementById('RectangleButton');
    RectangleButton.addEventListener('click', function () {
          viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
        NOP_VIEWER.loadExtension('Autodesk.Viewing.MarkupsCore').then(function (markupsExt) {
            markup = markupsExt;
            console.log(markup);
        });

        viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, function (event) {
            markup.enterEditMode();
            var cloud = new Autodesk.Viewing.Extensions.Markups.Core.EditModeRectangle(markup)
            markup.changeEditMode(cloud)
        });
    });

    var ArrowButton = document.getElementById('ArrowButton');
    ArrowButton.addEventListener('click', function () {
        //viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
        NOP_VIEWER.loadExtension('Autodesk.Viewing.MarkupsCore').then(function (markupsExt) {
            markup = markupsExt;
            console.log(markup);
        });

        viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, function (event) {
            markup.enterEditMode();
            var cloud = new Autodesk.Viewing.Extensions.Markups.Core.EditModeArrow(markup)
            markup.changeEditMode(cloud)
        });
    });

    var CircleButton = document.getElementById('CircleButton');
    CircleButton.addEventListener('click', function () {
        viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
        NOP_VIEWER.loadExtension('Autodesk.Viewing.MarkupsCore').then(function (markupsExt) {
            markup = markupsExt;
            console.log(markup);
        });

        viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, function (event) {
            markup.enterEditMode();
            var cloud = new Autodesk.Viewing.Extensions.Markups.Core.EditModeCircle(markup)
            markup.changeEditMode(cloud)
        });
    });

    var CloudButton = document.getElementById('CloudButton');
    CloudButton.addEventListener('click', function () {
        viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
        NOP_VIEWER.loadExtension('Autodesk.Viewing.MarkupsCore').then(function (markupsExt) {
            markup = markupsExt;
            console.log(markup);
        });

        viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, function (event) {
            markup.enterEditMode();
            var cloud = new Autodesk.Viewing.Extensions.Markups.Core.EditModeCloud(markup)
            markup.changeEditMode(cloud)
        });
    });

    var FreehandButton = document.getElementById('FreehandButton');
    FreehandButton.addEventListener('click', function () {
        viewer.unloadExtension("Autodesk.Viewing.MarkupsCore");
        NOP_VIEWER.loadExtension('Autodesk.Viewing.MarkupsCore').then(function (markupsExt) {
            markup = markupsExt;
            console.log(markup);
        });

        viewer.addEventListener(Autodesk.Viewing.SELECTION_CHANGED_EVENT, function (event) {
            markup.enterEditMode();
            var cloud = new Autodesk.Viewing.Extensions.Markups.Core.EditModeFreehand(markup)
            markup.changeEditMode(cloud)
        });
    });

    var GetData = document.getElementById('GetDataMarkup');
    GetData.addEventListener('click', function () {
        //viewer.setNavigationLock(false);

        // markups we just created as a string
        var markupsPersist = markup.generateData()
        console.log(markupsPersist)

        var type = "text/plain";
        var text = markupsPersist;
        var a = document.getElementById("a");
        var file = new Blob([markupsPersist], { type: type });
        a.href = URL.createObjectURL(file);
        a.download = name;

        //file.open("w"); // open file with write access
        //file.writeln("First line of text");
        //file.writeln("Second line of text " + str);
        //file.write(str);
        //file.close();


        // current view state (zoom, direction, sections)
        var viewerStatePersist = markup.viewer.getState()
        console.log(viewerStatePersist)
        // finish edit of markup
       // markup.leaveEditMode()
        // hide markups (and restore Viewer tools)
      //  markup.hide()
    });

 
    return true;
};



Autodesk.Viewing.theExtensionManager.registerExtension('MyAwesomeExtension', MyAwesomeExtension);