
    var faceMeshArray1 = [];
    var _overlaySceneName = "overlay-room-geometry";
    var _overlaySceneName2 = "bb";
    var _overlaySceneName3 = "tri";
    var _overlaySceneName4 = "4";
    var _overlaySceneName5 = "canvas";
    var _overlaySceneName6 = "triangles1";
    var UniId = 100;
    var material1 = createFaceMaterial("#b4ff77", 0.9, true); //green
    var material2 = createFaceMaterial("#f72727", 0.9, true); //red
    var material3 = createFaceMaterial("#0000FF", 0.9, true); //blue
    var material4 = createFaceMaterial("#0000FF", 0.9, true); //blue
    var mats;
    var colorchange = 0;
    var _customMaterialPrefix = 'forge-material-face-';
    var _specific_Floor_Rooms_Array = [];
    var countx = 0;
    var consoledata = new Array();
    var Slopedata = '';
    var SurfaceIddata = '';
    var PointCordinatesdata = '';
    var slopecheck = ''
    var Bulkdata = '';
    var Bulkdataindex = 0;
    var oldTextures = new Array();
    var oldColors = new Array();
    var vectorarrayX1Data = [];
    var vectorarrayX2Data = [];
    var Object3dArr1 = [];

        function RunShader(ForgeIds, checkslope, RevitID, BaseLevel) {

        faceMeshArray = [];
    colorchange = 0;
    _specific_Floor_Rooms_Array = [];
    SlopeClearAll();
    SlopeClearAll2();
    slopecheck = checkslope;
    Bulkdata = [];
    Bulkdataindex = 0;
            for (var i = 0; i < ForgeIds.length; i++) {
        _specific_Floor_Rooms_Array.push({
            roomid: ForgeIds[i],
            revid: RevitID[i],
            baseid: BaseLevel[i],
            defaultcolor: null,
            facemeshes: null
        });
    }
    return renderRoomShaderSlope();

}

        function renderRoomShaderSlope() {

            var XlsData = [];
    var textforXls = '';
    var vectorarrayNoDup3 = [];
    viewer.impl.createOverlayScene(_overlaySceneName);
    material1.name = "ADN-Material1";
    material2.name = "ADN-Material2";
    material3.name = "ADN-Material3";
    viewer.impl.matman().addMaterial('ADN-Material1', material1, true);
    viewer.impl.matman().addMaterial('ADN-Material2', material2, true);
    viewer.impl.matman().addMaterial('ADN-Material3', material3, true);
    mats = viewer.impl.matman()._materials;

    oldTextures = new Array();
    oldColors = new Array();
            for (index in mats) {
        m = mats[index];
    oldTextures[index] = m.map;
    oldColors[index] = m.color;
}

var colorIndex = 0;
Bulkdataindex1 = 1;
Bulkdataindex2 = 1;
$.each(_specific_Floor_Rooms_Array,
                function (num, room) {


        vectorarray = [];
    vectorarrayX1 = [];
    vectorarrayX2 = [];
    countx = 0;
    Slopedata = ''
    SurfaceIddata = ''
    PointCordinatesdata = ''
    var renderProxy = '';
    var instanceTree = viewer.model.getData().instanceTree;
                    instanceTree.enumNodeFragments(room.roomid, function (fragId) {

        renderProxy = viewer.impl.getRenderProxy(
            viewer.model,
            fragId);

    var matrix = renderProxy.matrixWorld;
    var indices = renderProxy.geometry.ib;
    var positions = renderProxy.geometry.vb;
    var stride = renderProxy.geometry.vbstride;
    var offsets = renderProxy.geometry.offsets;

                        if (!offsets || offsets.length === 0) {
        offsets = [{ start: 0, count: indices.length, index: 0 }];
    }

                        for (var oi = 0, ol = offsets.length; oi < ol; ++oi) {
                            var start = offsets[oi].start;
    var count = offsets[oi].count;
    var index = offsets[oi].index;

    var checkFace = 0;
    //var ip = countx + 1;

                            for (var i = start, il = start + count; i < il; i += 3) {

                                var vA = new THREE.Vector3();
    var vB = new THREE.Vector3();
    var vC = new THREE.Vector3();
    var a = index + indices[i];
    var b = index + indices[i + 1];
    var c = index + indices[i + 2];
    vA.fromArray(positions, a * stride);
    vB.fromArray(positions, b * stride);
    vC.fromArray(positions, c * stride);
    vA.applyMatrix4(matrix);
    vB.applyMatrix4(matrix);
    vC.applyMatrix4(matrix);
    var slope = 0;
    colorchange = 0;
    var a, b, c, zAxis, normal, axis, sin1, sin2, cos1;
    a = vA;
    b = vB;
    c = vC;
    zAxis = new THREE.Vector3(0, 0, 1);
    normal = new THREE.Vector3().crossVectors(
        new THREE.Vector3().subVectors(b, a),
        new THREE.Vector3().subVectors(c, a)
    ).normalize();

    sin1 = new THREE.Vector3().crossVectors(
        normal,
        zAxis
    );
    sin2 = math.norm([sin1.x, sin1.y, sin1.z])
    cos1 = math.dot([normal.x, normal.y, normal.z], [zAxis.x, zAxis.y, zAxis.z])
    slope = Math.abs(sin2 / cos1);
    slope = CheckMinSlope(slope);

    var invalidVal = 0;
                                if (slope == 'Infinity') {
        invalidVal = 1;
    //continue;
    }
                                if (isNaN(slope)) {
        invalidVal = 1;
    //continue;
    }

                                if (invalidVal == 0) {
        vectorarray.push(vA);
    vectorarray.push(vB);
    vectorarray.push(vC);

                                    if (slope < slopecheck) {
        colorchange = 0;
    }
                                    else {
        colorchange = 1;
    }

    var faceGeometry = createFaceGeometry(vA, vB, vC);
    faceMeshArray1.push(faceGeometry.name);
    var faces = faceGeometry.faces;
                                    for (var f = 0; f < faces.length; f++) {
                                        var faceMesh = drawFaceMesh(faceGeometry);
}
var area1 = triangle_area(vA.x, vA.y, vB.x, vB.y, vC.x, vC.y)
var centerX = ((parseFloat(vA.x) + parseFloat(vB.x) + parseFloat(vC.x)) / 3);
var centerY = ((parseFloat(vA.y) + parseFloat(vB.y) + parseFloat(vC.y)) / 3);
var centerZ = ((parseFloat(vA.z) + parseFloat(vB.z) + parseFloat(vC.z)) / 3);
vectorarrayX1.push(vA.x + "," + vA.y + "," + vA.z + "," + vB.x + "," + vB.y + "," + vB.z + "," + vC.x + "," + vC.y + "," + vC.z + "," + centerX + "," + centerY + "," + centerZ + "," + area1 + "," + slope);
}
}

}

//////////////
var Object3dTop = [];
//console.log(Object3dArr1.length);
                        for (var ig = 0; ig < vectorarrayX1.length; ig += 1) {

                            //var MeshObj = Object3dArr1[ig];
                            var TopVertex = vectorarrayX1[ig];
    var pointArray = TopVertex.split(',');
    var vA = new THREE.Vector3(parseFloat(pointArray[0]), parseFloat(pointArray[1]), parseFloat(pointArray[2]));
    var vB = new THREE.Vector3(parseFloat(pointArray[3]), parseFloat(pointArray[4]), parseFloat(pointArray[5]));
    var vC = new THREE.Vector3(parseFloat(pointArray[6]), parseFloat(pointArray[7]), parseFloat(pointArray[8]));
    var Cent = new THREE.Vector3(parseFloat(pointArray[9]), parseFloat(pointArray[10]), parseFloat(pointArray[11]));
    var Arr = parseFloat(pointArray[12]);
    var Slp = parseFloat(pointArray[13]);
    var psource = new THREE.Vector3(vA.x, vA.y, vA.z - 0.1);
    var ptarget = new THREE.Vector3(parseFloat(psource.x), parseFloat(psource.y), parseFloat(psource.z) - 1.5);
    var vray = new THREE.Vector3(ptarget.x - psource.x, ptarget.y - psource.y, ptarget.z - psource.z);
    vray.normalize();
    var ray = new THREE.Raycaster(psource, vray, 0, 1.5);
    var intersectResults = ray.intersectObjects(Object3dArr1);

                            if (intersectResults.length > 0) {

        intersectResults[0].object.visible = false;

    viewer.impl.removeOverlay(_overlaySceneName, intersectResults[0]);

    //Object3dTop.push(vA.x + "," + vA.y + "," + vA.z + "," + vB.x + "," + vB.y + "," + vB.z + "," + vC.x + "," + vC.y + "," + vC.z + "," + centerX + "," + centerY + "," + centerZ + "," + area1 + "," + slope);

    countx = countx + 1;
    var SurfaceId = room.roomid + '_' + countx;
    var PointCordinates = vA.x + ':' + vA.y + ':' + vA.z
        + ',' + vB.x + ':' + vB.y + ':' + vB.z
        + ',' + vC.x + ':' + vC.y + ':' + vC.z
                                if (countx == 1) {
        Slopedata = Slopedata + Slp;
    SurfaceIddata = SurfaceIddata + SurfaceId;
    PointCordinatesdata = PointCordinatesdata + PointCordinates;
}
                                else {
        Slopedata = Slopedata + '#' + Slp;
    SurfaceIddata = SurfaceIddata + '#' + SurfaceId;
    PointCordinatesdata = PointCordinatesdata + '#' + PointCordinates;
}

Bulkdata.push(room.revid + '|' + room.roomid + '|' + SurfaceId + '|' + Slp + '|' + room.baseid + '|' + PointCordinates);

}
}
//console.log(Object3dArr1.length);
});

});
//alert(JSON.stringify(Bulkdata));
return Bulkdata;
//   BindAdvaSearchForSlope(Bulkdata);

}

        function createFaceGeometry(vA, vB, vC, geom, color) {

            if (!geom) {
                var geom = new THREE.Geometry();
}

var vIndex = geom.vertices.length;

var vAi = new THREE.Vector3(vA.x, vA.y, vA.z + 0.1);
var vBi = new THREE.Vector3(vB.x, vB.y, vB.z + 0.1);
var vCi = new THREE.Vector3(vC.x, vC.y, vC.z + 0.1);

geom.vertices.push(vAi.clone());
geom.vertices.push(vBi.clone());
geom.vertices.push(vCi.clone());
var face = new THREE.Face3(vIndex, vIndex + 1, vIndex + 2);
geom.faces.push(face);
geom.computeFaceNormals();
geom.name = "CustomMesh" + UniId;
UniId = UniId + 1;

return geom;
}

        function drawFaceMesh(geom, overlaySceneName, material, mesh) {
            if (colorchange == 0) {
                var material = material1;
}
            if (colorchange == 1) {
                var material = material2;
}

var faceMesh = new THREE.Mesh(geom, material);
Object3dArr1.push(faceMesh);
//faceMesh.visible  = false;
viewer.impl.addOverlay(_overlaySceneName, faceMesh);
return faceMesh;
}

        function createFaceMaterial(colorhex, opacity, transparent) {
            var colorHexStr = colorhex;
    var colorThreeStr = colorHexStr.replace('#', '0x');
    var colorValue = parseInt(colorThreeStr, 16);

            var material = new THREE.MeshPhongMaterial({
        color: colorValue,
    opacity: opacity,
    transparent: transparent,
    side: THREE.DoubleSide,
});

return material;
}

        function newGUID() {
            var d = new Date().getTime();
    var guid = 'xxxx-xxxx-xxxx-xxxx-xxxx'.replace(
        /[xy]/g,
                function (c) {
                    var r = (d + Math.random() * 16) % 16 | 0;
    d = Math.floor(d / 16);
    return (c == 'x' ? r : (r & 0x7 | 0x8)).toString(16);
});

return guid;
}

        function SlopeClearAll2() {
        viewer.impl.removeOverlayScene(_overlaySceneName5);
    for (var i = 0; i < Object3dArr1.length; i += 1) {
        Object3dArr1[i].visible = true;
    }
    viewer.impl.invalidate(true, true, false);
    viewer.impl.sceneUpdated(true);
}

        function SlopeClearAll() {
        viewer.impl.removeOverlayScene(_overlaySceneName5);
    for (var i = 0; i < Object3dArr1.length; i += 1) {
        Object3dArr1[i].visible = false;
    }
    viewer.impl.invalidate(true, true, false);
    viewer.impl.sceneUpdated(true);
}

        function DrawTriangle(points, ForgeId) {

        viewer.impl.invalidate(true, true, false);

    viewer.impl.removeOverlayScene(_overlaySceneName4);
    viewer.impl.removeOverlayScene(_overlaySceneName5);
    viewer.impl.removeOverlayScene(_overlaySceneName6);
    viewer.impl.createOverlayScene(_overlaySceneName6);
            if (points != '' || points != null || points != undefined) {
                var pointArray = points.split(',');
    // console.log(pointArray);
    var point1 = pointArray[0];
    // console.log(point1);
    point1 = point1.split(':')
    var point2 = pointArray[1];
    point2 = point2.split(':')
    var point3 = pointArray[2];
    point3 = point3.split(':')
    var pt1 = new THREE.Vector3(point1[0], point1[1], parseFloat(point1[2]) + 0.1);
    var pt2 = new THREE.Vector3(point2[0], point2[1], parseFloat(point2[2]) + 0.1);
    var pt3 = new THREE.Vector3(point3[0], point3[1], parseFloat(point3[2]) + 0.1);
    var geom3 = new THREE.Geometry();
    var vIndex = geom3.vertices.length;
    geom3.vertices.push(pt1.clone());
    geom3.vertices.push(pt2.clone());
    geom3.vertices.push(pt3.clone());
    var face = new THREE.Face3(vIndex, vIndex + 1, vIndex + 2);
    geom3.faces.push(face);
    geom3.computeFaceNormals();
    var material = material3;
    var faceMesh = new THREE.Mesh(geom3, material4);
    viewer.impl.addOverlay(_overlaySceneName6, faceMesh);
    viewer.impl.sceneUpdated(true);


    //var cen1 = (parseFloat(pt1.x) + parseFloat(pt2.x) + parseFloat(pt3.x)) / 3;
    //var cen2 = (parseFloat(pt1.y) + parseFloat(pt2.y) + parseFloat(pt3.y)) / 3;
    //var geometry = new THREE.SphereGeometry(10, 10, 10);
                //var material5 = new THREE.MeshBasicMaterial({color: 0x39d2dbe7fff39d2 });
    //var meshX = new THREE.Mesh(geometry, material5);
    //meshX.position.set(parseFloat(cen1), parseFloat(cen2), pt1.z);
    //viewer.impl.addOverlay(_overlaySceneName6, meshX);
    //viewer.impl.sceneUpdated(true);

    //var id1 = parseInt(id);
    //viewer.showAll();
    //viewer.impl.selector.setSelection(id1, viewer.model);
    //viewer.fitToView(id1);
    //viewer.isolateById(id1);
    //viewer.impl.selector.setSelection(id1, viewer.model);
    //viewer.fitToView(id1);
    //viewer.isolateById(id1);

    //var cam = viewer.getCamera();
    //cam.position.set(cen1, cen2, pt3.z);

}

}

        function DrawTriangle2(points) {

        viewer.impl.removeOverlayScene(_overlaySceneName6);
    SlopeClearAll();
    viewer.impl.invalidate(true, true, false);
    viewer.impl.createOverlayScene(_overlaySceneName5);

            var matBlue = new THREE.LineBasicMaterial({color: 0x000000 });
            var matGreen = new THREE.LineBasicMaterial({color: 0x000000 });
            var matRed = new THREE.LineBasicMaterial({color: 0x000000 });

            ////$('#slopeCanvas').remove(); // this is my <canvas> element
            ////$('#divCanvas').append('<canvas id="slopeCanvas" width="500" height="300" style="border:1px solid #d3d3d3;"></canvas>');

            if (points != '' || points != null || points != undefined) {
                var pointArray = points.split(',');
        var point1 = pointArray[0];
        point1 = point1.split(':')
        var point2 = pointArray[1];
        point2 = point2.split(':')
        var point3 = pointArray[2];
        point3 = point3.split(':')
        var pt1 = new THREE.Vector3(point1[0], point1[1], parseFloat(point1[2]));
        var pt2 = new THREE.Vector3(point2[0], point2[1], parseFloat(point2[2]));
        var pt3 = new THREE.Vector3(point3[0], point3[1], parseFloat(point3[2]));


        var zVal = [pt1.z, pt2.z, pt3.z];
                zVal.sort(function (a, b) { return b - a });

        var zVal2 = [pt1.z, pt2.z, pt3.z];
                zVal2.sort(function (a, b) { return a - b });

        console.log("zVal1 - " + zVal);
        console.log("zVal2 - " + zVal2);

        var geometry = new THREE.Geometry();
        geometry.vertices.push(new THREE.Vector3(pt1.x, pt1.y, pt1.z + 0.5));
        geometry.vertices.push(new THREE.Vector3(pt2.x, pt2.y, pt2.z + 0.5));
        geometry.computeLineDistances();
        var line = new THREE.Line(geometry, matBlue);
        viewer.impl.addOverlay(_overlaySceneName5, line);

        var geometry = new THREE.Geometry();
        geometry.vertices.push(new THREE.Vector3(pt2.x, pt2.y, pt2.z + 0.5));
        geometry.vertices.push(new THREE.Vector3(pt3.x, pt3.y, pt3.z + 0.5));
        geometry.computeLineDistances();
        var line = new THREE.Line(geometry, matGreen);
        viewer.impl.addOverlay(_overlaySceneName5, line);

        var geometry = new THREE.Geometry();
        geometry.vertices.push(new THREE.Vector3(pt3.x, pt3.y, pt3.z + 0.5));
        geometry.vertices.push(new THREE.Vector3(pt1.x, pt1.y, pt1.z + 0.5));
        geometry.computeLineDistances();
        var line = new THREE.Line(geometry, matRed);
        viewer.impl.addOverlay(_overlaySceneName5, line);

        var area = triangle_area(pt1.x, pt1.y, pt2.x, pt2.y, pt3.x, pt3.y)

        var slope1 = 0;
        slope1 = ThreeDSlope(point1[0], point1[1], point1[2], point2[0], point2[1], point2[2]);
        slope1 = CheckMinSlope(slope1);
        var slope2 = 0;
        slope2 = ThreeDSlope(point2[0], point2[1], point2[2], point3[0], point3[1], point3[2]);
        slope2 = CheckMinSlope(slope2);
        var slope3 = 0;
        slope3 = ThreeDSlope(point3[0], point3[1], point3[2], point1[0], point1[1], point1[2]);
        slope3 = CheckMinSlope(slope3);


        // viewer.impl.removeOverlayScene(_overlaySceneName5);
        //viewer.impl.createOverlayScene(_overlaySceneName5);
        //console.log(area);
        textht = 40;
                if (area > 40) {
            textht = 40;
        }
                if (area < 40) {
            textht = area;
        }
                if (area < 10) {
            textht = 10;
        }

        var pt1pt2cenX = (parseFloat(pt1.x) + parseFloat(pt2.x)) / 2;
        var pt1pt2ceny = (parseFloat(pt1.y) + parseFloat(pt2.y)) / 2;
        var pt1pt2cenz = (parseFloat(pt1.z) + parseFloat(pt2.z)) / 2;
        var canvas1 = document.createElement('canvas');
        var context1 = canvas1.getContext('2d');
        context1.font = "Bold " + textht + "px Arial";
        context1.fillStyle = "rgba(255,0,0,0.95)";
        context1.fillText('Slope1=' + slope1, 0, 50);
        var texture1 = new THREE.Texture(canvas1);
        texture1.needsUpdate = true;
                var material5 = new THREE.MeshBasicMaterial({map: texture1, side: THREE.DoubleSide });
        material5.transparent = true;
        var mesh1 = new THREE.Mesh(new THREE.PlaneGeometry(canvas1.width / 50, canvas1.height / 50), material5);
        mesh1.position.set(parseFloat(pt1pt2cenX), parseFloat(pt1pt2ceny), parseFloat(pt1pt2cenz) + 0.2);
        viewer.impl.addOverlay(_overlaySceneName5, mesh1);

        var pt1pt2cenX = (parseFloat(pt2.x) + parseFloat(pt3.x)) / 2;
        var pt1pt2ceny = (parseFloat(pt2.y) + parseFloat(pt3.y)) / 2;
        var pt1pt2cenz = (parseFloat(pt2.z) + parseFloat(pt3.z)) / 2;
        var canvas1 = document.createElement('canvas');
        var context1 = canvas1.getContext('2d');
        context1.font = "Bold " + textht + "px Arial";
        context1.fillStyle = "rgba(255,0,0,0.95)";
        context1.fillText('Slope2=' + slope2, 0, 50);
        var texture1 = new THREE.Texture(canvas1);
        texture1.needsUpdate = true;
                var material5 = new THREE.MeshBasicMaterial({map: texture1, side: THREE.DoubleSide });
        material5.transparent = true;
        var mesh1 = new THREE.Mesh(new THREE.PlaneGeometry(canvas1.width / 50, canvas1.height / 50), material5);
        mesh1.position.set(parseFloat(pt1pt2cenX), parseFloat(pt1pt2ceny), parseFloat(pt1pt2cenz) + 0.2);
        viewer.impl.addOverlay(_overlaySceneName5, mesh1);

        var pt1pt2cenX = (parseFloat(pt3.x) + parseFloat(pt1.x)) / 2;
        var pt1pt2ceny = (parseFloat(pt3.y) + parseFloat(pt1.y)) / 2;
        var pt1pt2cenz = (parseFloat(pt3.z) + parseFloat(pt1.z)) / 2;
        var canvas1 = document.createElement('canvas');
        var context1 = canvas1.getContext('2d');
        context1.font = "Bold " + textht + "px Arial";
        context1.fillStyle = "rgba(255,0,0,0.95)";
        context1.fillText('Slope3=' + slope3, 0, 50);
        var texture1 = new THREE.Texture(canvas1);
        texture1.needsUpdate = true;
                var material5 = new THREE.MeshBasicMaterial({map: texture1, side: THREE.DoubleSide });
        material5.transparent = true;
        var mesh1 = new THREE.Mesh(new THREE.PlaneGeometry(canvas1.width / 50, canvas1.height / 50), material5);
        mesh1.position.set(parseFloat(pt1pt2cenX), parseFloat(pt1pt2ceny), parseFloat(pt1pt2cenz) + 0.2);
        viewer.impl.addOverlay(_overlaySceneName5, mesh1);


        //    //$("#NonCompliantLineslope tbody").html('');
                //    //$("#NonCompliantLineslope tbody").append('<tr><td style="width:4%;background-color:#0000FF;color:white">1</td><td style="width:4%">' + slope1 + '</td></tr>');
                //    //$("#NonCompliantLineslope tbody").append('<tr><td style="width:4%;background-color:#00FF00;color:white">2</td><td style="width:4%">' + slope2 + '</td></tr>');
                //    //$("#NonCompliantLineslope tbody").append('<tr><td style="width:4%;background-color:#FF0000;color:white">3</td><td style="width:4%">' + slope3 + '</td></tr>');
        //    //$("#NonCompliantLineslope").show();
    }
}

        function triangle_area(x1, y1, x2, y2, x3, y3) {
            var answer = Math.abs(0.5 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)))
            if (answer <= 0) {
            answer = 0;
        }
        return answer;
    }

        function ThreeDSlope(x1, y1, z1, x2, y2, z2) {
            var rise = Math.abs(parseFloat(z2) - parseFloat(z1));
        var delx = Math.abs(parseFloat(x2) - parseFloat(x1));
        var dely = Math.abs(parseFloat(y2) - parseFloat(y1));
        var run = Math.sqrt(delx * delx + dely * dely);
        var answer = Math.abs(rise / run);
            if (rise <= 0) {
            answer = 0;
        }
        return answer;
    }

        function CheckMinSlope(slope) {
            slope = parseFloat(slope);
        if (slope <= 0.000001) {
            slope = slope;
        }
            else {
            slope = roundToXDigit(slope, 4);
        }
        return slope;
    }

        function roundToXDigit(value, digits) {
            if (!digits) {
            digits = 2;
        }
        value = value * Math.pow(10, digits);
        value = Math.round(value);
        value = value / Math.pow(10, digits);
        return value;
    }

        function GetUnique(inputArray) {
            var outputArray = [];

            for (var i = 0; i < inputArray.length; i++) {
                if ((jQuery.inArray(inputArray[i], outputArray)) == -1) {
            outputArray.push(inputArray[i]);
        }
    }
    return outputArray;
}

        function getid(SelId) {

            $("#ADAdatatableForSlopeCompliant tbody tr td:nth-child(2)").each(function () {

                var btnValue = $(this).find("input").val();
                if (btnValue.contains(SelId)) {
                    var prevCell = $(this).prev('td').find("input").val();
                    $(this).prev('td').find("input").css('background-color', '#4EB1EA');
                    $(this).prev('td').find("input").focus();
                }


            });

        $("#ADAdatatableForSlopeNonCompliant tbody tr td:nth-child(2)").each(function () {

                var btnValue = $(this).find("input").val();
                if (btnValue.contains(SelId)) {
                    var prevCell = $(this).prev('td').find("input").val();
        $(this).prev('td').find("input").css('background-color', '#4EB1EA');
        $(this).prev('td').find("input").focus();
    }

});

}

var drawboxtoggle = 0;

        function ADAHighlightbox1(id) {
            if (drawboxtoggle == 0) {

            drawboxtoggle = 1;
        var id1 = parseInt(id);
        viewer.showAll();
        viewer.impl.selector.setSelection(id1, viewer.model);
        viewer.fitToView(id1);
        viewer.isolateById(id1);
        viewer.impl.selector.setSelection(id1, viewer.model);
        viewer.fitToView(id1);
        viewer.isolateById(id1);

        viewer.impl.removeOverlayScene(_overlaySceneName4);
        viewer.impl.removeOverlayScene(_overlaySceneName5);
        viewer.impl.removeOverlayScene(_overlaySceneName6);
        SlopeClearAll();
        viewer.impl.invalidate(true, true, false);

                var material1 = new THREE.LineBasicMaterial({
            color: 0x0077ff,
        linewidth: 5,
        linecap: 'round', //ignored by WebGLRenderer
        linejoin: 'round' //ignored by WebGLRenderer
    });

                var material2 = new THREE.LineBasicMaterial({
            color: 0xff0000,
        linewidth: 5,
        linecap: 'round', //ignored by WebGLRenderer
        linejoin: 'round' //ignored by WebGLRenderer
    });


    var front_clear_value = $('iframe[title=ADA]').contents().find("#clear_fronttext").val();
    var side_clear_value = $('iframe[title=ADA]').contents().find("#clear_sidetext").val();
    var top_clear_value = $('iframe[title=ADA]').contents().find("#clear_toptext").val();
    var bot_clear_value = $('iframe[title=ADA]').contents().find("#clear_bottomtext").val();
    var back_clear_value = $('iframe[title=ADA]').contents().find("#clear_backtext").val();


    var front_clear = parseInt(0);
    var side_clear = parseInt(0);
    var top_clear = parseInt(0);
    var bot_clear = parseInt(0);
    var back_clear = parseInt(0);

    var id1 = parseInt(id);

    var bbox = getModifiedWorldBoundingBox(
        id1,
        viewer.model.getFragmentList()
    );

    drawBox(bbox.min, bbox.max);
}
            else {
            viewer.showAll();
        drawboxtoggle = 0;
        viewer.impl.removeOverlayScene(_overlaySceneName4);
    }
}

        function getModifiedWorldBoundingBox(id1, fragList) {

            let bounds = new THREE.Box3()
            , box = new THREE.Box3()
            , instanceTree = viewer.impl.model.getData().instanceTree

            instanceTree.enumNodeFragments(id1, function (fragId) {
            fragList.getWorldBounds(fragId, box)
                bounds.union(box);
    }, true)

    return bounds;
}

        function drawBox(min, max) {

            var material2 = new THREE.LineBasicMaterial({
            color: 0xff0000,
        linewidth: 5,
        linecap: 'round', //ignored by WebGLRenderer
        linejoin: 'round' //ignored by WebGLRenderer

    });

    drawLines([

                {x: min.x, y: min.y, z: min.z },
                {x: max.x, y: min.y, z: min.z },

                {x: max.x, y: min.y, z: min.z },
                {x: max.x, y: min.y, z: max.z },

                {x: max.x, y: min.y, z: max.z },
                {x: min.x, y: min.y, z: max.z },

                {x: min.x, y: min.y, z: max.z },
                {x: min.x, y: min.y, z: min.z },

                {x: min.x, y: max.y, z: max.z },
                {x: max.x, y: max.y, z: max.z },

                {x: max.x, y: max.y, z: max.z },
                {x: max.x, y: max.y, z: min.z },

                {x: max.x, y: max.y, z: min.z },
                {x: min.x, y: max.y, z: min.z },

                {x: min.x, y: max.y, z: min.z },
                {x: min.x, y: max.y, z: max.z },

                {x: min.x, y: min.y, z: min.z },
                {x: min.x, y: max.y, z: min.z },

                {x: max.x, y: min.y, z: min.z },
                {x: max.x, y: max.y, z: min.z },

                {x: max.x, y: min.y, z: max.z },
                {x: max.x, y: max.y, z: max.z },

                {x: min.x, y: min.y, z: max.z },
                {x: min.x, y: max.y, z: max.z }],
        material2);
    viewer.impl.sceneUpdated(true);

}

        function drawLines(coordsArray, material) {
            //  console.log(coordsArray.length);
            viewer.impl.removeOverlayScene(_overlaySceneName4);
        viewer.impl.createOverlayScene(_overlaySceneName4);
            for (var i = 0; i < coordsArray.length; i += 2) {
                var start = coordsArray[i];
        var end = coordsArray[i + 1];
        var geometry = new THREE.Geometry();
        geometry.vertices.push(new THREE.Vector3(
            start.x, start.y, start.z));
        geometry.vertices.push(new THREE.Vector3(
            end.x, end.y, end.z));
        geometry.computeLineDistances();
        var line = new THREE.Line(geometry, material);
        viewer.impl.addOverlay(_overlaySceneName4, line);
    }
    //console.log("Lines Completed");
}

        function ShowClick() {
            SlopeClearAll2();
        viewer.impl.invalidate(true, true, false);
        //alert("show");
    }

        function HideClick() {
            //alert("Hide");
            SlopeClearAll();
        viewer.impl.invalidate(true, true, false);
    }

