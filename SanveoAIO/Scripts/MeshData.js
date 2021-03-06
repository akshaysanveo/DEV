﻿
///////////////////////////////////////////////////////////////////////////////
// MeshData viewer extension
// by Philippe Leefsma, July 2015
//
///////////////////////////////////////////////////////////////////////////////
AutodeskNamespace("Autodesk.ADN.Viewing.Extension");

Autodesk.ADN.Viewing.Extension.MeshData = function (viewer, options) {
    //alert("within Autodesk.ADN.Viewing");

    Autodesk.Viewing.Extension.call(this, viewer, options);

    var _self = this;

    var _lineMaterial = null;

    var _vertexMaterial = null;

    ///////////////////////////////////////////////////////////////////////////
    // load callback
    //
    ///////////////////////////////////////////////////////////////////////////
    _self.load = function () {
        //alert("within _self.load");

        _lineMaterial = createLineMaterial();

        _vertexMaterial = createVertexMaterial();

        viewer.addEventListener(
          Autodesk.Viewing.SELECTION_CHANGED_EVENT,
          onSelectionChanged);

        console.log('Autodesk.ADN.Viewing.Extension.MeshData loaded');

        return true;
    };

    ///////////////////////////////////////////////////////////////////////////
    // unload callback
    //
    ///////////////////////////////////////////////////////////////////////////
    _self.unload = function () {

        console.log('Autodesk.ADN.Viewing.Extension.MeshData unloaded');

        return true;
    };

    ///////////////////////////////////////////////////////////////////////////
    // selection changed callback
    //
    ///////////////////////////////////////////////////////////////////////////
    function onSelectionChanged(event) {
        //alert("within onSelectionChanged")

        event.fragIdsArray.forEach(function(fragId){

            drawMeshData(fragId);
        });

        viewer.impl.sceneUpdated(true);
    }

    ///////////////////////////////////////////////////////////////////////////
    // draw vertices and faces
    //
    ///////////////////////////////////////////////////////////////////////////
    function drawMeshData(fragId) {

        var fragProxy = viewer.impl.getFragmentProxy(
          viewer.model,
          fragId);

        var renderProxy = viewer.impl.getRenderProxy(
          viewer.model,
          fragId);

        fragProxy.updateAnimTransform();

        var matrix = new THREE.Matrix4();
        fragProxy.getWorldMatrix(matrix);

        var geometry = renderProxy.geometry;

        var attributes = geometry.attributes;

        var vA = new THREE.Vector3();
        var vB = new THREE.Vector3();
        var vC = new THREE.Vector3();
		
		var countx=0;
		//var consoledata='';
		var consoledata = new Array();

        if (attributes.index !== undefined) {

            var indices = attributes.index.array || geometry.ib;
            var positions = geometry.vb ? geometry.vb : attributes.position.array;
            var stride = geometry.vb ? geometry.vbstride : 3;
            var offsets = geometry.offsets;

            if (!offsets || offsets.length === 0) {

                offsets = [{start: 0, count: indices.length, index: 0}];
            }

            for (var oi = 0, ol = offsets.length; oi < ol; ++oi) {

                var start = offsets[oi].start;
                var count = offsets[oi].count;
                var index = offsets[oi].index;

                for (var i = start, il = start + count; i < il; i += 3) {

                    var a = index + indices[i];
                    var b = index + indices[i + 1];
                    var c = index + indices[i + 2];

                    vA.fromArray(positions, a * stride);
                    vB.fromArray(positions, b * stride);
                    vC.fromArray(positions, c * stride);

                    vA.applyMatrix4(matrix);
                    vB.applyMatrix4(matrix);
                    vC.applyMatrix4(matrix);

                    drawVertex (vA, 0.05);
                    drawVertex (vB, 0.05);
                    drawVertex (vC, 0.05);

                    drawLine(vA, vB);
                    drawLine(vB, vC);
                    drawLine(vC, vA);
					var slope=0;
					if(vA.x==vB.x){
					if(vA.y==vB.y){
						slope='Error';
					}
					else{
					slope=(vC.z-vA.z)/(vA.y-vB.y);
					slope=Math.abs(slope);
					}
				}
				else if(vA.y==vB.y){
					if(vA.x==vB.x){
						slope='Error';
					}
					else{
					slope=(vC.z-vA.z)/(vA.x-vB.x);
					slope=Math.abs(slope);
					}
				}
					countx=countx+1;
					consoledata.push("Set"+countx+" ([" + vA.x + "," + vA.y + "," + vA.z + "]+[" + vB.x + "," + vB.y + "," + vB.z + "]+[" + vC.x + "," + vC.y + "," + vC.z + "]+[slope="+slope+"])"+"\n");
					
                //console.log("Set"+countx+" ([" + vA.x + "," + vA.y + "," + vA.z + "]+[" + vB.x + "," + vB.y + "," + vB.z + "]+[" + vC.x + "," + vC.y + "," + vC.z + "]+[slope="+slope+"])");
                }
            }
        }
        else {

            var positions = geometry.vb ? geometry.vb : attributes.position.array;
            var stride = geometry.vb ? geometry.vbstride : 3;

            for (var i = 0, j = 0, il = positions.length; i < il; i += 3, j += 9) {

                var a = i;
                var b = i + 1;
                var c = i + 2;

                vA.fromArray(positions, a * stride);
                vB.fromArray(positions, b * stride);
                vC.fromArray(positions, c * stride);

                vA.applyMatrix4(matrix);
                vB.applyMatrix4(matrix);
                vC.applyMatrix4(matrix);

                drawVertex (vA, 0.05);
                drawVertex (vB, 0.05);
                drawVertex (vC, 0.05);

                drawLine(vA, vB);
                drawLine(vB, vC);
                drawLine(vC, vA);
				var slope=0;
				if(vA.x==vB.x){
					if(vA.y==vB.y){
						slope='Error';
					}
					else{
					slope=(vC.z-vA.z)/(vA.y-vB.y);
					slope=Math.abs(slope);
					}
				}
				else if(vA.y==vB.y){
					if(vA.x==vB.x){
						slope='Error';
					}
					else{
					slope=(vC.z-vA.z)/(vA.x-vB.x);
					slope=Math.abs(slope);
					}
				}
				countx=countx+1;
				
				consoledata.push("Set"+countx+" ([" + vA.x + "," + vA.y + "," + vA.z + "]+[" + vB.x + "," + vB.y + "," + vB.z + "]+[" + vC.x + "," + vC.y + "," + vC.z + "]+[slope="+slope+"])"+"\n");
				
                //console.log("Set"+countx+"  ([" + vA.x + "," + vA.y + "," + vA.z + "]+[" + vB.x + "," + vB.y + "," + vB.z + "]+[" + vC.x + "," + vC.y + "," + vC.z + "]+[slope="+slope+"])");
            }

        }
		console.log(consoledata)
	}

    ///////////////////////////////////////////////////////////////////////////
    // vertex material
    //
    ///////////////////////////////////////////////////////////////////////////
    function createVertexMaterial() {

        var material = new THREE.MeshPhongMaterial({ color: 0xff0000 });

        viewer.impl.matman().addMaterial(
          'adn-material-vertex',
          material,
          true);

        return material;
    }

    ///////////////////////////////////////////////////////////////////////////
    // line material
    //
    ///////////////////////////////////////////////////////////////////////////
    function createLineMaterial() {

        var material = new THREE.LineBasicMaterial({
            color: 0x0000ff,
            linewidth: 10,
           // linecap: 'round', //ignored by WebGLRenderer
           // linejoin: 'round' //ignored by WebGLRenderer
        });

        //viewer.impl.matman().addMaterial(
        //  'adn-material-line',
        //  material,
        //  true);

        return material;
    }

    ///////////////////////////////////////////////////////////////////////////
    // draw a line
    //
    ///////////////////////////////////////////////////////////////////////////
    function drawLine(start, end) {

        var geometry = new THREE.Geometry();

        var pt1 = new THREE.Vector3(start.x, start.y, start.z);
        var pt2 = new THREE.Vector3(end.x, end.y, end.z);

        geometry.vertices.push(pt1);
        geometry.vertices.push(pt2);

        //geometry.vertices.push(new THREE.Vector3(
        //  start.x, start.y, start.z));

        //geometry.vertices.push(new THREE.Vector3(
        //  end.x, end.y, end.z));

        var line = new THREE.Line(geometry, _lineMaterial);

        viewer.impl.scene.add(line);
    }

    ///////////////////////////////////////////////////////////////////////////
    // draw a vertex
    //
    ///////////////////////////////////////////////////////////////////////////
    function drawVertex (v, radius) {

        var vertex = new THREE.Mesh(
          new THREE.SphereGeometry(radius, 20),
          _vertexMaterial);

        vertex.position.set(v.x, v.y, v.z);
        // console.log("-----vertex---");
        // console.log('x:' + vertex.position.x + ',y:' + vertex.position.y+ ',z:' + vertex.position.z)
        
        viewer.impl.scene.add(vertex);
    }
};

(function(console){

console.save = function(data, filename){

    if(!data) {
        console.error('Console.save: No data')
        return;
    }

    if(!filename) filename = 'console.json'

    if(typeof data === "object"){
        data = JSON.stringify(data, undefined, 4)
    }

    var blob = new Blob([data], {type: 'text/json'}),
        e    = document.createEvent('MouseEvents'),
        a    = document.createElement('a')

    a.download = filename
    a.href = window.URL.createObjectURL(blob)
    a.dataset.downloadurl =  ['text/json', a.download, a.href].join(':')
    e.initMouseEvent('click', true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null)
    a.dispatchEvent(e)
 }
})(console)

Autodesk.ADN.Viewing.Extension.MeshData.prototype =
  Object.create(Autodesk.Viewing.Extension.prototype);

Autodesk.ADN.Viewing.Extension.MeshData.prototype.constructor =
  Autodesk.ADN.Viewing.Extension.MeshData;

Autodesk.Viewing.theExtensionManager.registerExtension(
  'Autodesk.ADN.Viewing.Extension.MeshData',
  Autodesk.ADN.Viewing.Extension.MeshData);
