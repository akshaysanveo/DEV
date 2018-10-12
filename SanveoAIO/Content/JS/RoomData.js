




    var _self = this;
    _self.tool = null;
    this.createpolygonMode = false;
    this.createpolygonTopMode = false;
    this.topPlane = null;
    this.lastSelectedPoint = null;
    this.i = 0;
    var line;
    var MAX_POINTS = 500;
    var drawCount;
    var splineArray = [];
    var niceRadius = null,
        material_red = null;
    var polygonVertices = [],
        polygonBaseVertices = [], customObjectArray = [];
    var height = null,
        heightY = false;
    var toolName = "Autodesk.ADN.Viewing.Tool.CustomTool";
    var group;
    var object;
    var elementViewer = document.getElementById("viewer");
    var INTERSECTED, SELECTED;

    //custom features
    var mouseDown = false;

        _self.load = function () {

        initScopeBox();
    return true;
};

        _self.unload = function () {

        alert('Autodesk.ADN.Viewing.Extension.PolygonLine unloaded');
    viewer.toolController.deactivateTool(toolName);

    return true;
};

        function initScopeBox() {

        document.addEventListener('keydown', function (event) {
            // alert(event.keyCode);
            if (event.keyCode == 65) {
                viewer.setViewCube("top");
                group = new THREE.Group();
                elementViewer.addEventListener("click", onMouseClick);
            }
            else if (event.keyCode == 90) {
                //viewer.setViewCube("front");
                projectPolyGonToHeight();
                elementViewer.removeEventListener("click", onMouseClick, false);
                for (var i = 0; i < customObjectArray.length; i++) {
                    var object = customObjectArray[i];
                    viewer.impl.scene.remove(object);
                }

                polygonVertices = [];
                this.lastSelectedPoint = null;
                polygonBaseVertices = [];
                customObjectArray = [];


            }

        });
    }

        function createSphereonPoint(material_red, intersects) {

            var spherePoint = new THREE.Mesh(new THREE.SphereGeometry(niceRadius, 20), material_red);
    spherePoint.position.set(intersects.x, intersects.y, intersects.z);
    viewer.impl.scene.add(spherePoint);
    customObjectArray.push(spherePoint);
    viewer.impl.invalidate(true);
}

        function offset(el) {
            var rect = el.getBoundingClientRect(),
        scrollLeft = window.pageXOffset || document.documentElement.scrollLeft,
        scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            return {
        top: rect.top + scrollTop,
    left: rect.left + scrollLeft
}
}

        function getPos(el) {
            for (var lx = 0, ly = 0; el != null; lx += el.offsetLeft, ly += el.offsetTop, el = el.offsetParent);
            return {
        x: lx,
    y: ly
};
}

        function onMouseClick(event) {


            var viewHeight = viewer.canvas.height;
    var viewWidth = viewer.canvas.width;
    var vec = new THREE.Vector2(); // create once and reuse
    var pos = new THREE.Vector3(); // create once and reuse
    var camera = viewer.impl.camera;
    var offsetobj = getPos(viewer.canvas);
    var offsetX = offsetobj.x;
    var offsetY = offsetobj.y;
            if (this.topPlane == null) {
                var boundingBox = viewer.model.getBoundingBox();
    var maxpt = boundingBox.max;
    var minpt = boundingBox.min;

    var xdiff = maxpt.x - minpt.x;
    var ydiff = maxpt.y - minpt.y;
    var zdiff = maxpt.z - minpt.z;

    //set a nice radius in the model size

    niceRadius = Math.pow((xdiff * xdiff + ydiff * ydiff + zdiff * zdiff), 0.5) / 200;
    var centerpoint = new THREE.Vector3((maxpt.x + minpt.x) / 2, (maxpt.y + minpt.y) / 2, (maxpt.z + minpt.z) / 2);

    var lookAtVector = new THREE.Vector3(0, 0, -1);
    lookAtVector.applyQuaternion(camera.quaternion);

    var worldirection = new THREE.Vector3();
    camera.getWorldDirection(worldirection);

    var raycaster = new THREE.Raycaster();
    raycaster.set(centerpoint, worldirection.negate());
    var RayTowardsCamera = raycaster.ray;

    var height = 0;
                if (RayTowardsCamera.direction.y == 1 || RayTowardsCamera.direction.y == -1) {
        height = maxpt.y - minpt.y;
    heightY = true;
                } else if (RayTowardsCamera.direction.z == 1 || RayTowardsCamera.direction.z == -1) {
        height = maxpt.z - minpt.z;
    heightY = false;
}
var pointOnplane = new THREE.Vector3();
RayTowardsCamera.at(height / 2, pointOnplane);

this.topPlane = new THREE.Plane();
this.topPlane.setFromNormalAndCoplanarPoint(lookAtVector, pointOnplane);
}



vec.set((event.clientX - offsetX) / viewWidth * 2 - 1,
-(event.clientY - offsetY) / viewHeight * 2 + 1,
);

const pointerVector = new THREE.Vector3();
const pointerDir = new THREE.Vector3();
var newmouseRaycast = new THREE.Raycaster();
            if (camera.isPerspective) {
        pointerVector.set((event.clientX - offsetX) / viewWidth * 2 - 1,
            -(event.clientY - offsetY) / viewHeight * 2 + 1, 0.5);
    pointerVector.unproject(camera);
    newmouseRaycast.set(camera.position, pointerVector.sub(camera.position).normalize());
            } else {
        pointerVector.set((event.clientX - offsetX) / viewWidth * 2 - 1,
            -(event.clientY - offsetY) / viewHeight * 2 + 1, -1);
    pointerVector.unproject(camera);
    pointerDir.set(0, 0, -1);
    newmouseRaycast.set(pointerVector, pointerDir.transformDirection(camera.matrixWorld));
}

var intersects = new THREE.Vector3();
newmouseRaycast.ray.intersectPlane(this.topPlane, intersects);
var distance = -camera.position.y / vec.y;
pos.copy(camera.position).add(vec.multiplyScalar(distance));
var point = pos;
            material_red = new THREE.MeshPhongMaterial({
        color: 0xff0000
});
viewer.impl.matman().addMaterial('ADN-Material' + 'red', material_red, true);
var boundingBox =

    viewer.model.getBoundingBox();
var maxpt = boundingBox.max;
var minpt = boundingBox.min;

createSphereonPoint(material_red, intersects);
polygonVertices.push(intersects);

            if (this.lastSelectedPoint == null) {
        this.lastSelectedPoint = new THREE.Vector3();
    this.lastSelectedPoint.copy(intersects);
            } else {
        createAxisLine(this.lastSelectedPoint, intersects);
    this.lastSelectedPoint.copy(intersects);
}
var inputheight = 10; //Number(document.getElementById('scopeboxheight').value)
            if (heightY) {
                var basePoint = new THREE.Vector3(intersects.x, intersects.y - inputheight, intersects.z);
    console.log(intersects.x, intersects.y - inputheight, intersects.z);
    polygonBaseVertices.push(new THREE.Vector3(Number(basePoint.x.toPrecision(3)), Number(basePoint.y.toPrecision(3)), Number(basePoint.z.toPrecision(3))));
            } else {
        console.log(intersects.x, intersects.y, intersects.z - inputheight);
    var basePoint = new THREE.Vector3(intersects.x, intersects.y, intersects.z - inputheight);
    polygonBaseVertices.push(new THREE.Vector3(Number(basePoint.x.toPrecision(3)), Number(basePoint.y.toPrecision(3)), Number(basePoint.z.toPrecision(3))));
}


}

        function projectPolyGonToHeight() {

            //CreatePoints(polygonBaseVertices);

            var meshMaterial = new THREE.MeshPhongMaterial({
        color: 0x0080ff,
    side: THREE.DoubleSide,
    opacity: 0.3,
    transparent: true,
})

            var material = new THREE.MeshBasicMaterial({
        color: 0x00ff00
});

viewer.impl.matman().addMaterial('ADN-Material' + 'shapeMaterial', meshMaterial, true);



//group = new THREE.Group();
//viewer.impl.sceneAfter.add(group);

// points

var vertices = [];
            polygonVertices.forEach(element => {
        vertices.push(element);
    });
            polygonBaseVertices.forEach(element => {
        vertices.push(element);
    });

    var verticesString1 = "";
    var MinZ = "";

            for (i = 0; i < polygonVertices.length; i++) {
                var ptx = "";
    var pt1 = new THREE.Vector3(polygonVertices[i].x, polygonVertices[i].y, polygonVertices[i].z);
    ptx = pt1.x + ":" + pt1.y + ":" + pt1.z + "#";
    verticesString1 = verticesString1 + ptx;
    MinZ = pt1.z;
}
console.log("polygonVertices1");
console.log(verticesString1);
sessionStorage.setItem("PolygonVertices1", verticesString1);
sessionStorage.setItem("MinZ", MinZ);


var verticesString2 = "";
var MaxZ = "";
            for (i = 0; i < polygonBaseVertices.length; i++) {
                var ptx = "";
    var pt1 = new THREE.Vector3(polygonBaseVertices[i].x, polygonBaseVertices[i].y, polygonBaseVertices[i].z);
    ptx = pt1.x + ":" + pt1.y + ":" + pt1.z + "#";
    verticesString2 = verticesString2 + ptx;
    MaxZ = pt1.z
}
console.log("polygonVertices2");
console.log(verticesString2);
sessionStorage.setItem("PolygonVertices2", verticesString2);
sessionStorage.setItem("MaxZ", MaxZ);

var meshGeometry = new THREE.ConvexGeometry(vertices);

var mesh = new THREE.Mesh(meshGeometry, meshMaterial);
mesh.material.side = THREE.DoubleSide; // back faces
//group.add(mesh);

object = mesh;

viewer.impl.sceneAfter.add(mesh);
viewer.impl.invalidate(true);
viewer.setGhosting(false)

// var mesh = new THREE.Mesh( meshGeometry, meshMaterial.clone() );
// mesh.material.side = THREE.FrontSide; // front faces
// mesh.renderOrder = 1;
// group.add( mesh );
}



        function createAxisLine(point1, point2) {

            var unitSize = 1;

    var lineOverlay = 'myOverlay' + point1.x;
    var offset = viewer.model.getData().globalOffset;

    point1 = new THREE.Vector3(point1.x.toPrecision(3), point1.y.toPrecision(3), point1.z.toPrecision(3));
    point2 = new THREE.Vector3(point2.x.toPrecision(3), point2.y.toPrecision(3), point2.z.toPrecision(3));


    //line1
    var dash = unitSize,
        gap = unitSize * 5,
        lineWd = 3;
    var geometry1 = new THREE.Geometry();
    geometry1.vertices.push(point1);
    geometry1.vertices.push(point2);

    geometry1.computeLineDistances();
            var material = new THREE.LineBasicMaterial({
        color: 0xff0000,
    linewidth: 30
});
var line1 = new THREE.Line(geometry1, material);
viewer.impl.scene.add(line1);
customObjectArray.push(line1);

viewer.impl.invalidate(true);
}

        function createOverlay(overlayName) {
        viewer.impl.createOverlayScene(overlayName);
    }

        function addOverlay(overlayName, mesh) {
        viewer.impl.addOverlay(overlayName, mesh);
    }

    // update positions
        function updatePositions() {

            var positions = line.geometry.attributes.position.array;

    var index = 0;

            for (var i = 0; i < splineArray.length; i++) {

        positions[index++] = splineArray[i].x;
    positions[index++] = splineArray[i].y;
    positions[index++] = splineArray[i].z;


}
}

        function onMouseMove(evt) {
            if (renderer) {

                var x = (event.clientX / window.innerWidth) * 2 - 1;
    var y = -(event.clientY / window.innerHeight) * 2 + 1;
    var vNow = new THREE.Vector3(x, y, 0);

    vNow.unproject(camera);
    splineArray.push(vNow);

}
}

        function onMouseUp(evt) {
        document.removeEventListener("mousemove", onMouseMove, false);
    }

        function onMouseDown(evt) {

            if (evt.which == 3) return;


    var x = (event.clientX / window.innerWidth) * 2 - 1;
    var y = -(event.clientY / window.innerHeight) * 2 + 1;

    // do not register if right mouse button is pressed.

    var vNow = new THREE.Vector3(x, y, 0);
    vNow.unproject(camera);
    console.log(vNow.x + " " + vNow.y + " " + vNow.z);

    document.addEventListener("mousemove", onMouseMove, false);
}
