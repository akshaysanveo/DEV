

        function HighlightboxRoomwise(id)
        {
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

            var id1 = parseInt(id);

            var bbox = getModifiedWorldBoundingBoxRoomwise(
            id1,
           viewer.model.getFragmentList()
          );
            alert('ANDF;d')
            var middle= drawBoxRoomwise(bbox.min, bbox.max);

            return middle;

        }

        function getModifiedWorldBoundingBoxRoomwise(id1, fragList) {

            let bounds = new THREE.Box3()
          , box = new THREE.Box3()
          , instanceTree = viewer.impl.model.getData().instanceTree


            instanceTree.enumNodeFragments(id1, function (fragId) {
                fragList.getWorldBounds(fragId, box)
                bounds.union(box);
            }, true)

            return bounds;
        }

        function drawBoxRoomwise(min, max) {

            var material2 = new THREE.LineBasicMaterial({
                color: 0xff0000,
                linewidth: 5,
                linecap: 'round', //ignored by WebGLRenderer
                linejoin: 'round' //ignored by WebGLRenderer

            });

            var  middle=  drawLinesRoomwise([

                {x: min.x, y: min.y, z: min.z},
                {x: max.x, y: min.y, z: min.z},

                {x: max.x, y: min.y, z: min.z},
                {x: max.x, y: min.y, z: max.z},

                {x: max.x, y: min.y, z: max.z},
                {x: min.x, y: min.y, z: max.z},

                {x: min.x, y: min.y, z: max.z},
                {x: min.x, y: min.y, z: min.z},

                {x: min.x, y: max.y, z: max.z},
                {x: max.x, y: max.y, z: max.z},

                {x: max.x, y: max.y, z: max.z},
                {x: max.x, y: max.y, z: min.z},

                {x: max.x, y: max.y, z: min.z},
                {x: min.x, y: max.y, z: min.z},

                {x: min.x, y: max.y, z: min.z},
                {x: min.x, y: max.y, z: max.z},

                {x: min.x, y: min.y, z: min.z},
                {x: min.x, y: max.y, z: min.z},

                {x: max.x, y: min.y, z: min.z},
                {x: max.x, y: max.y, z: min.z},

                {x: max.x, y: min.y, z: max.z},
                {x: max.x, y: max.y, z: max.z},

                {x: min.x, y: min.y, z: max.z},
                {x: min.x, y: max.y, z: max.z}],

                material2);

            //console.log("Box Completed");

            //viewer.impl.sceneUpdated(true);

            return middle;
        }

        function drawLinesRoomwise(coordsArray, material) {
            console.log(coordsArray.length);

            geometry1 = new THREE.Geometry();

            for (var i = 0; i < coordsArray.length; i+=2) {

                var start = coordsArray[i];
                var end = coordsArray[i+1];

                var geometry = new THREE.Geometry();

                geometry.vertices.push(new THREE.Vector3(
                    start.x, start.y, start.z));

                geometry.vertices.push(new THREE.Vector3(
                    end.x, end.y, end.z));

                geometry.computeLineDistances();

                var line = new THREE.Line(geometry, material);

                // viewer.impl.scene.add(line);


                // for center point
                geometry1.vertices.push(new THREE.Vector3(
                start.x, start.y, start.z));

                geometry1.vertices.push(new THREE.Vector3(
                    end.x, end.y, end.z));

                geometry1.computeLineDistances();

            }

            var box1 = new THREE.Mesh(geometry1, material);
            var mid= getCenterPoint(box1,material);
            //console.log("Lines Completed");
            return mid;
        }

        function getCenterPoint(mesh,material) {
            var middle = new THREE.Vector3();
            var geometry = mesh.geometry;

            geometry.computeBoundingBox();

            middle.x = (geometry.boundingBox.max.x + geometry.boundingBox.min.x) / 2;
            middle.y = (geometry.boundingBox.max.y + geometry.boundingBox.min.y) / 2;
            middle.z = (geometry.boundingBox.max.z + geometry.boundingBox.min.z) / 2;

            mesh.localToWorld( middle );

            //var geometry2 = new THREE.Geometry();

            //geometry2.vertices.push(new THREE.Vector3(
            //    middle.x, middle.y, middle.z));

            //geometry2.vertices.push(new THREE.Vector3(
            //    middle.x+1, middle.y+1, middle.z+1));

            //geometry2.computeLineDistances();

            //var line2 = new THREE.Line(geometry2, material);

            //viewer.impl.scene.add(line2);


            return middle;
        }

        // For Bounding Co-ordinates


        var _specific_Floor_Rooms_Array = [];

        function RoomBox()
        {

            $('iframe[title=QuantityTest]').contents().find("#Modelloader").show();

            if(_URN_=="")
            {
                alert("Please Load Model")
                $('iframe[title=QuantityTest]').contents().find("#Modelloader").hide();
                return;
            }

            var moddata = JSON.stringify({
                'Urn': _URN_,
                'VersionNo': Version_No
            });

            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("GetModelForgeid", "QuantityTest")',
                data: moddata,
                dataType: "json",
                success: function (data) {
                    // console.log(data.length);

                    var DataStore="";
                    var DataStoreMiddle="";
                    var DataRoomBoundingBox="";
                    var DataRoomBoundingBoxmin="";

                    var   ForgeArrayPoint=[];

                    _specific_Floor_Rooms_Array=[];

                    for (var i = 0; i < data.length; i++) {

                        var forgeidExact=  data[i].Forgeid;
                        var Category_NameExact=  data[i].Category_Name;
                        var Family_TypeExact=  data[i].Family_Type;
                        var Instance_NameExact=  data[i].Instance_Name;
                        var UrnExact=  data[i].MGuid;
                        var VersionExact=  data[i].Version;
                        var CompIdExact=  data[i].CompId;


                        //console.log(forgeidExact);
                        if(Category_NameExact=="Rooms")
                        {
                            ForgeArrayPoint.push(forgeidExact);

                            //  var roomCoordinate= getRooms(ForgeArrayPoint);
                            //  console.log(roomCoordinate);

                            _specific_Floor_Rooms_Array.push({roomid:forgeidExact,
                                defaultcolor:null,
                                facemeshes:null})
                        }


                    }

                    //_specific_Floor_Rooms_Array.push({roomid:7099,
                    //    defaultcolor:null,
                    //    facemeshes:null})

                    var roomCoordinate= renderRoomShader();
                    //console.log("---Finallll----");
                    // console.log(roomCoordinate);

                    var RoomDataMAx = roomCoordinate.split('^');
                    var maxp=RoomDataMAx[0];
                    var minp=RoomDataMAx[1];

                    var RoomData = maxp.split('~');
                    var RoomDatamin = minp.split('~');

                    for (var i = 0; i < RoomData.length; i++) {

                        var rt = RoomData[i].split('$');

                        for (var k = 0; k < data.length; k++)
                        {
                            var forgeidExact=  data[k].Forgeid;
                            var Category_NameExact=  data[k].Category_Name;
                            var Family_TypeExact=  data[k].Family_Type;
                            var Instance_NameExact=  data[k].Instance_Name;
                            var UrnExact=  data[k].MGuid;
                            var VersionExact=  data[k].Version;
                            var CompIdExact=  data[k].CompId;

                            if(rt[0]==forgeidExact)
                            {
                                DataRoomBoundingBox+=forgeidExact + '~}' + Category_NameExact + '~}' + Family_TypeExact + '~}' + Instance_NameExact + '~}' + UrnExact + '~}' + VersionExact + '~}' + CompIdExact + '~}' + rt[1]+ '$';

                            }
                        }

                    }

                    for (var i = 0; i < RoomDatamin.length; i++) {

                        var rt1 = RoomDatamin[i].split('$');

                        for (var k = 0; k < data.length; k++)
                        {
                            var forgeidExact=  data[k].Forgeid;
                            var Category_NameExact=  data[k].Category_Name;
                            var Family_TypeExact=  data[k].Family_Type;
                            var Instance_NameExact=  data[k].Instance_Name;
                            var UrnExact=  data[k].MGuid;
                            var VersionExact=  data[k].Version;
                            var CompIdExact=  data[k].CompId;

                            if(rt1[0]==forgeidExact)
                            {
                                DataRoomBoundingBoxmin+=forgeidExact + '~}' + Category_NameExact + '~}' + Family_TypeExact + '~}' + Instance_NameExact + '~}' + UrnExact + '~}' + VersionExact + '~}' + CompIdExact + '~}' + rt1[1]+ '$';

                            }
                        }

                    }


                    for (var i = 0; i < data.length; i++) {

                        var forgeid=  data[i].Forgeid;
                        var Category_Name=  data[i].Category_Name;
                        var Family_Type=  data[i].Family_Type;
                        var Instance_Name=  data[i].Instance_Name;
                        var Urn=  data[i].MGuid;
                        var Version=  data[i].Version;
                        var CompId=  data[i].CompId;

                        var id1 = parseInt(forgeid);

                        var bbox = getModifiedWorldBoundingBoxRoomwise(
                        id1,
                       viewer.model.getFragmentList()
                      );

                        DataStore+=forgeid + '~}' + Category_Name + '~}' + Family_Type + '~}' + Instance_Name + '~}' + Urn + '~}' + Version + '~}' + CompId + '~}' +  bbox.min.x + '~}' + bbox.max.x + '~}' + bbox.min.y + '~}' + bbox.max.y + '~}' + bbox.min.z + '~}' + bbox.max.z+ '#'  ;

                        // For Middle point

                        var CenterPointX ="";
                        var CenterPointY ="";
                        var CenterPointZ ="";

                        CenterPointX = ((bbox.max.x - bbox.min.x) / 2) + bbox.min.x;
                        CenterPointY = ((bbox.max.y  -  bbox.min.y) / 2) +  bbox.min.y;
                        CenterPointZ = (( bbox.max.z - bbox.min.z ) / 2) + bbox.min.z ;

                        DataStoreMiddle+=forgeid + '~}' + Category_Name  + '~}' + Family_Type + '~}' + Instance_Name + '~}' + Urn + '~}' + Version + '~}' + CompId + '~}' +  CenterPointX + '~}' + CenterPointY+ '~}' + CenterPointZ + '#'  ;
                    }

                    //  console.log(DataStore);

                    // For Middle point
                    //for (var i = 0; i < data.length; i++) {
                    //    var forgeid=  data[i].Forgeid;
                    //    var Category_Name=  data[i].Category_Name;
                    //    var Family_Type=  data[i].Family_Type;
                    //    var Instance_Name=  data[i].Instance_Name;
                    //    var Urn=  data[i].MGuid;
                    //    var Version=  data[i].Version;
                    //    var CompId=  data[i].CompId;

                    //    var middle= HighlightboxRoomwise(forgeid);

                    //    DataStoreMiddle+=forgeid + '~}' + Category_Name  + '~}' + Family_Type + '~}' + Instance_Name + '~}' + Urn + '~}' + Version + '~}' + CompId + '~}' +  middle.x + '~}' + middle.y + '~}' + middle.z + '#'  ;

                    //}

                    // console.log("doneee");

                    var value = JSON.stringify({
                        'Urn': _URN_,
                        'VersionNo': Version_No,
                        'DataStore': DataStore,
                    });

                    $.ajax({
                        type: "POST",
                        contentType: 'application/json',
                        url: '@Url.Action("SaveBoundingPoints", "QuantityTest")',
                        data: value,
                        dataType: "json",
                        success: function (result) {

                            var value = JSON.stringify({
                                'Urn': _URN_,
                                'VersionNo': Version_No,
                                'DataStoreMiddle': DataStoreMiddle
                            });


                            $.ajax({
                                type: "POST",
                                contentType: 'application/json',
                                url: '@Url.Action("SaveBoundingMiddlePoints", "QuantityTest")',
                                data: value,
                                dataType: "json",
                                success: function (result) {

                                    var value = JSON.stringify({
                                        'Urn': _URN_,
                                        'VersionNo': Version_No,
                                        'DataRoomBoundingBox': DataRoomBoundingBox,
                                        'DataRoomBoundingBoxmin':DataRoomBoundingBoxmin
                                    });


                                    $.ajax({
                                        type: "POST",
                                        contentType: 'application/json',
                                        url: '@Url.Action("SaveRoomPoints", "QuantityTest")',
                                        data: value,
                                        dataType: "json",
                                        success: function (result) {

                                            alert("Geometry Saved Successfully");
                                            $('iframe[title=QuantityTest]').contents().find("#Modelloader").hide();
                                        },
                                        error: function (result) {
                                            alert("Error in  Saving  Geometry");
                                            $('iframe[title=QuantityTest]').contents().find("#Modelloader").hide();

                                        }
                                    });



                                },
                                error: function (result) {
                                    alert("Error in  Saving  Geometry");
                                    $('iframe[title=QuantityTest]').contents().find("#Modelloader").hide();

                                }
                            });

                            $('iframe[title=QuantityTest]').contents().find("#Modelloader").hide();

                        },
                        error: function (result) {
                            alert("Error in  Saving  Geometry");
                            $('iframe[title=QuantityTest]').contents().find("#Modelloader").hide();
                        }
                    });


                },
                error: function (result) {
                    alert("Error1");
                }
            });

        }



        function getpoint(Roomid,Elementid)
        {
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

            var HomeMesh="";
            var robotmesh="";
            var id1 = parseInt(Roomid);
            var id2 = parseInt(Elementid);


            var bbox = getModifiedWorldBoundingGetPoint(
            id1,
           viewer.model.getFragmentList()
          );

            HomeMesh= drawBoxGetPoint(bbox.min, bbox.max);

            var bbox = getModifiedWorldBoundingGetPoint(
            id2,
           viewer.model.getFragmentList()
          );

            robotmesh= drawBoxGetPoint(bbox.min, bbox.max);

            var mid= getCenterPoint(robotmesh,material2);

            var flag=  getFlag(HomeMesh,mid);
            console.log(flag);
            return flag;
        }

        function getModifiedWorldBoundingGetPoint(id1, fragList) {

            let bounds = new THREE.Box3()
          , box = new THREE.Box3()
          , instanceTree = viewer.impl.model.getData().instanceTree


            instanceTree.enumNodeFragments(id1, function (fragId) {
                fragList.getWorldBounds(fragId, box)
                bounds.union(box);
            }, true)

            return bounds;
        }

        function drawBoxGetPoint(min, max) {

            var material2 = new THREE.LineBasicMaterial({
                color: 0xff0000,
                linewidth: 5,
                linecap: 'round', //ignored by WebGLRenderer
                linejoin: 'round' //ignored by WebGLRenderer

            });

            var  middle=  drawLinesGetPoint([

                {x: min.x, y: min.y, z: min.z},
                {x: max.x, y: min.y, z: min.z},

                {x: max.x, y: min.y, z: min.z},
                {x: max.x, y: min.y, z: max.z},

                {x: max.x, y: min.y, z: max.z},
                {x: min.x, y: min.y, z: max.z},

                {x: min.x, y: min.y, z: max.z},
                {x: min.x, y: min.y, z: min.z},

                {x: min.x, y: max.y, z: max.z},
                {x: max.x, y: max.y, z: max.z},

                {x: max.x, y: max.y, z: max.z},
                {x: max.x, y: max.y, z: min.z},

                {x: max.x, y: max.y, z: min.z},
                {x: min.x, y: max.y, z: min.z},

                {x: min.x, y: max.y, z: min.z},
                {x: min.x, y: max.y, z: max.z},

                {x: min.x, y: min.y, z: min.z},
                {x: min.x, y: max.y, z: min.z},

                {x: max.x, y: min.y, z: min.z},
                {x: max.x, y: max.y, z: min.z},

                {x: max.x, y: min.y, z: max.z},
                {x: max.x, y: max.y, z: max.z},

                {x: min.x, y: min.y, z: max.z},
                {x: min.x, y: max.y, z: max.z}],

                material2);

            return middle;
        }

        function drawLinesGetPoint(coordsArray, material) {
            //console.log(coordsArray.length);

            geometry1 = new THREE.Geometry();

            for (var i = 0; i < coordsArray.length; i+=2) {

                var start = coordsArray[i];
                var end = coordsArray[i+1];

                var geometry = new THREE.Geometry();

                geometry.vertices.push(new THREE.Vector3(
                    start.x, start.y, start.z));

                geometry.vertices.push(new THREE.Vector3(
                    end.x, end.y, end.z));

                geometry.computeLineDistances();

                var line = new THREE.Line(geometry, material);

                // viewer.impl.scene.add(line);


                // for center point
                geometry1.vertices.push(new THREE.Vector3(
                start.x, start.y, start.z));

                geometry1.vertices.push(new THREE.Vector3(
                    end.x, end.y, end.z));

                geometry1.computeLineDistances();

            }

            var box1 = new THREE.Mesh(geometry1, material);
            //var flag= getFlag(box1,material);

            return box1;
        }

        function getFlag(homemesh,robotmesh) {

            //var robotBB = new THREE.Box3().setFromObject(robotmesh);
            //  console.log("robotmesh");
            // console.log(robotmesh);

            var homeBB = new THREE.Box3().setFromObject(homemesh);
            //  console.log("homemesh");
            // console.log(homeBB);


            var robotIsHome = homeBB.containsPoint(robotmesh);
            //  console.log("finalresult");
            // console.log(robotIsHome);

            return robotIsHome;
        }

        function GetRoomElement(Roomid)
        {

            var value = JSON.stringify({
                'Urn': _URN_,
                'VersionNo': Version_No,
            });


            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("GetModelForgeid", "QuantityTest")',
                data: value,
                dataType: "json",
                success: function (data) {

                    $('iframe[title=QuantityTest]').contents().find("#roomdatatable tbody").html('');

                    var Category_Name= "";
                    var loop=0;
                    var count=0;

                    for (var i = 0; i < data.length; i++) {
                        var forgeid=  data[i].Forgeid;

                        var boolen= getpoint(Roomid,forgeid)

                        if(boolen==true)
                        {

                            if(Category_Name!="" && Category_Name!= data[i].Category_Name)
                            {
                                $('iframe[title=QuantityTest]').contents().find("#roomdatatable tbody").append('<tr><td style="width:4%">'+Category_Name+'</td><td style="width:2%">'+count+'</td></tr>');

                                count=0;
                            }

                            Category_Name=  data[i].Category_Name;


                            $('iframe[title=QuantityTest]').contents().find("#roomdatatable tbody").append('<tr><td style="width:4%">'+Category_Name+'</td><td style="width:2%"><input type ="button" value = '+ forgeid +'  id ='+ forgeid +' onclick="window.parent.ADAHighlightbox1(this.id)"/></td></tr>');

                            count++;
                        }
                    }

                    $('iframe[title=QuantityTest]').contents().find("#roomdatatable tbody").append('<tr><td style="width:4%">'+Category_Name+'</td><td style="width:2%">'+count+'</td></tr>');
                    $('iframe[title=QuantityTest]').contents().find("#loading").hide();
                    $('iframe[title=QuantityTest]').contents().find("#roomdatatable").show();

                },
                error: function (result) {
                    alert("Error2");

                }
            });


        }



        function renderRoomShader()
        {

            //console.log('room number in this specific floor:'
              //  +  _specific_Floor_Rooms_Array.length);

            var  colorIndex = 0;
            var vAPoint = [];
            var vAPointMin = [];

            var storecooradinates="";
            var storecooradinatesmin="";

            $.each( _specific_Floor_Rooms_Array,
                function(num,room){

                    //console.log('room dbid:' + room.roomid);

                    if(colorIndex > 5)
                        colorIndex = 0;

                    var faceMeshArray = [];


                    var vBPointz = [];
                    var vCPoint = [];
                    vAPoint = [];
                    vAPointMin=[];

                    let bounds = new THREE.Box3()
                    , box = new THREE.Box3()
                      , instanceTree = viewer.impl.model.getData().instanceTree

                   // var instanceTree =  viewer.model.getData().instanceTree;
                    instanceTree.enumNodeFragments(room.roomid, function(fragId){

                        var bbox = getModifiedWorldBoundingGetPoint(
                               room.roomid,
                              viewer.model.getFragmentList()
                           );

                        var renderProxy = viewer.impl.getRenderProxy(
                             viewer.model,
                            fragId);

                        var matrix = renderProxy.matrixWorld;
                        var indices = renderProxy.geometry.ib;
                        var positions = renderProxy.geometry.vb;
                        var stride = renderProxy.geometry.vbstride;
                        var offsets = renderProxy.geometry.offsets;

                        if (!offsets || offsets.length === 0) {
                            offsets = [{start: 0, count: indices.length, index: 0}];
                        }

                        var vA = new THREE.Vector3();
                        var vB = new THREE.Vector3();
                        var vC = new THREE.Vector3();


                        var MaxPoint = "";
                        var MinPoint = "";

                        for (var oi = 0, ol = offsets.length; oi < ol; ++oi) {

                            var start = offsets[oi].start;
                            var count = offsets[oi].count;
                            var index = offsets[oi].index;

                            var checkFace = 0;

                            for (var i = start, il = start + count; i < il; i += 3) {

                                var vz = new THREE.Vector3();

                                var a = index + indices[i];
                                var b = index + indices[i + 1];
                                var c = index + indices[i + 2];


                                vA.fromArray(positions, a * stride);
                                vB.fromArray(positions, b * stride);
                                vC.fromArray(positions, c * stride);


                                //vBPointz.push(vA.z);
                                //vBPointz.push(vB.z);
                                //vBPointz.push(vC.z);

                                vA.applyMatrix4(matrix);
                                vB.applyMatrix4(matrix);
                                vC.applyMatrix4(matrix);

                                var minz= Math.round(bbox.min.z * 100) / 100;
                                var maxz= Math.round(bbox.max.z * 100) / 100;
                                var Vcz=Math.round(vC.z * 100) / 100;

                                //console.log("Z VAlues----");
                                //console.log(maxz);
                                //console.log("With Matrix");
                                //console.log(Vcz);

                                // ---For Upper Point---

                                if(vAPoint.length==0)
                                {
                                    if(maxz==Vcz)
                                    {
                                        vAPoint.push(vA.x + ":" + vA.y + ":" + 0.0);
                                        vAPoint.push(vB.x + ":" + vB.y + ":" + 0.0);
                                        vAPoint.push(vC.x + ":" + vC.y + ":" + 0.0);

                                    }
                                }
                                else
                                {
                                    if(maxz==Vcz)
                                    {
                                        var leng=vAPoint.length;
                                        //console.log("Lenth-------");
                                        //console.log(leng);
                                        var flagx=0;
                                        var flagy=0;
                                        var flagz=0;
                                        for(var j=0; j< leng;j++)
                                        {
                                            var w= vAPoint[j].split(':');

                                            if(Math.abs(vA.x-w[0])< 0.5 && Math.abs(vA.y-w[1])< 0.5)
                                            {
                                                console.log("--valuess1---");
                                                console.log(Math.abs(vA.x-w[0]));
                                                console.log(Math.abs(vA.y-w[1]));

                                                flagx=1;
                                                // vAPoint.push(vA.x + ":" + vA.y + ":" + 0.0);
                                            }

                                            if(Math.abs(vB.x-w[0])< 0.5 && Math.abs(vB.y-w[1])< 0.5)
                                            {
                                                console.log("--valuess2---");
                                                console.log(Math.abs(vB.x-w[0]));
                                                console.log(Math.abs(vB.y-w[1]));

                                                flagy=1;
                                                // vAPoint.push(vB.x + ":" + vB.y + ":" + 0.0);
                                            }

                                            if(Math.abs(vC.x-w[0])< 0.5 && Math.abs(vC.y-w[1])< 0.5)
                                            {
                                                console.log("--valuess3---");
                                                console.log(Math.abs(vC.x-w[0]));
                                                console.log(Math.abs(vC.y-w[1]));

                                                flagz=1;
                                                // vAPoint.push(vC.x + ":" + vC.y + ":" + 0.0);

                                            }
                                        }

                                        if(flagx==0)
                                        {
                                            vAPoint.push(vA.x + ":" + vA.y + ":" + 0.0);

                                        }
                                        if(flagy==0)
                                        {
                                            vAPoint.push(vB.x + ":" + vB.y + ":" + 0.0 );
                                        }
                                        if(flagz==0)
                                        {
                                            vAPoint.push(vC.x + ":" + vC.y + ":" + 0.0 );
                                        }
                                    }

                                }


                                // ---For Lower Point---

                                if(vAPointMin.length==0)
                                {
                                    if(minz==Vcz)
                                    {

                                        vAPointMin.push(vA.x + ":" + vA.y + ":" + 0.0);
                                        vAPointMin.push(vB.x + ":" + vB.y + ":" + 0.0);
                                        vAPointMin.push(vC.x + ":" + vC.y + ":" + 0.0) ;
                                    }
                                }
                                else
                                {
                                    if(minz==Vcz)
                                    {
                                        var leng=vAPointMin.length;
                                        // console.log("Lenth-------");
                                        //  console.log(leng);
                                        var flagx=0;
                                        var flagy=0;
                                        var flagz=0;
                                        for(var j=0; j< leng;j++)
                                        {
                                            var w= vAPointMin[j].split(':');

                                            if(Math.abs(vA.x-w[0])< 0.5 && Math.abs(vA.y-w[1])< 0.5)
                                            {
                                                //  console.log("--valuess1---");
                                                //  console.log(Math.abs(vA.x-w[0]));
                                                //  console.log(Math.abs(vA.y-w[1]));

                                                flagx=1;
                                                // vAPointMin.push(vA.x + ":" + vA.y + ":" + 0.0);
                                            }

                                            if(Math.abs(vB.x-w[0])< 0.5 && Math.abs(vB.y-w[1])< 0.5)
                                            {
                                                // console.log("--valuess2---");
                                                //  console.log(Math.abs(vB.x-w[0]));
                                                //  console.log(Math.abs(vB.y-w[1]));

                                                flagy=1;
                                                // vAPointMin.push(vB.x + ":" + vB.y + ":" + 0.0);
                                            }

                                            if(Math.abs(vC.x-w[0])< 0.5 && Math.abs(vC.y-w[1])< 0.5)
                                            {
                                                // console.log("--valuess3---");
                                                //console.log(Math.abs(vC.x-w[0]));
                                                // console.log(Math.abs(vC.y-w[1]));

                                                flagz=1;
                                                // vAPointMin.push(vC.x + ":" + vC.y + ":" + 0.0);

                                            }
                                        }

                                        if(flagx==0)
                                        {
                                            vAPointMin.push(vA.x + ":" + vA.y + ":" + 0.0 );
                                        }
                                        if(flagy==0)
                                        {
                                            vAPointMin.push(vB.x + ":" + vB.y + ":" + 0.0 );
                                        }
                                        if(flagz==0)
                                        {
                                            vAPointMin.push(vC.x + ":" + vC.y + ":" + 0.0 );
                                        }


                                    }

                                }


                                //   var faceGeometry = createFaceGeometry(vA, vB, vC);
                                //   var faces = faceGeometry.faces;


                            }
                        }


                        console.log("-----Ppoints------------");
                        vAPoint = GetUnique(vAPoint);
                        vAPointMin=GetUnique(vAPointMin);
                        console.log(vAPoint);


                            if(vAPoint.length <=200)
                            {
                                storecooradinates+= room.roomid +'$';

                                for (var i = 0; i < vAPoint.length; i++) {
                                    storecooradinates += vAPoint[i]+'#';
                                }

                                storecooradinates= storecooradinates.substring(0, storecooradinates.length-1);

                                storecooradinates+='~';
                            }

                            if(vAPointMin.length <=200)
                            {
                                storecooradinatesmin+= room.roomid +'$';

                                for (var i = 0; i < vAPointMin.length; i++) {
                                    storecooradinatesmin += vAPointMin[i]+'#';
                                }

                                storecooradinatesmin= storecooradinatesmin.substring(0, storecooradinatesmin.length-1);

                                storecooradinatesmin+='~';
                            }




                    });

                    // room.defaultcolor = _materialArray[colorIndex];
                    // room.facemeshes = faceMeshArray;

                    colorIndex++;


                });

            storecooradinatesmin= storecooradinatesmin.substring(0, storecooradinatesmin.length-1);
            storecooradinates= storecooradinates.substring(0, storecooradinates.length-1);

            var newstorecooradinates= storecooradinates+'^'+storecooradinatesmin

            return newstorecooradinates;
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


        function fnSlopeCoordinate()
        {

            $('iframe[title=ADAClearance]').contents().find("#Modelloader").show();

            if(_URN_=="")
            {
                alert("Please Load Model")
                $('iframe[title=ADAClearance]').contents().find("#Modelloader").hide();
                return;
            }

            var moddata = JSON.stringify({
                'Urn': _URN_,
                'VersionNo': Version_No
            });

            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("GetModelForgeid", "QuantityTest")',
                data: moddata,
                dataType: "json",
                success: function (data) {
                    // console.log(data.length);
                    var FloorDataStore="";

                    var   ForgeArrayPoint=[];
                    var DataStore="";
                    _specific_Floor_Rooms_Array=[];

                    for (var i = 0; i < data.length; i++) {

                        var forgeidExact=  data[i].Forgeid;
                        var Category_NameExact=  data[i].Category_Name;
                        var Family_TypeExact=  data[i].Family_Type;
                        var Instance_NameExact=  data[i].Instance_Name;
                        var UrnExact=  data[i].MGuid;
                        var VersionExact=  data[i].Version;
                        var CompIdExact=  data[i].CompId;

                        if(Category_NameExact=="Floors" || Category_NameExact=="Ramps")
                        {
                            ForgeArrayPoint.push(forgeidExact);

                            //  var roomCoordinate= getRooms(ForgeArrayPoint);
                            console.log(Category_NameExact);

                            _specific_Floor_Rooms_Array.push({roomid:forgeidExact,
                                defaultcolor:null,
                                facemeshes:null})
                        }


                        var id1 = parseInt(forgeidExact);

                        var bbox = getModifiedWorldBoundingBoxRoomwise(
                        id1,
                       viewer.model.getFragmentList()
                      );

                        DataStore+=forgeidExact + '~}' + Category_NameExact + '~}' + Family_TypeExact + '~}' + Instance_NameExact + '~}' + UrnExact + '~}' + VersionExact + '~}' + CompIdExact + '~}' +  bbox.min.x + '~}' + bbox.max.x + '~}' + bbox.min.y + '~}' + bbox.max.y + '~}' + bbox.min.z + '~}' + bbox.max.z+ '#'  ;



                    }


                    var roomCoordinate= funSlopeco();
                    //console.log("---Finallll----");
                    // console.log(roomCoordinate);

                    var RoomData = roomCoordinate.split('~');

                    for (var i = 0; i < RoomData.length; i++) {

                        var rt = RoomData[i].split('$');

                        for (var k = 0; k < data.length; k++)
                        {
                            var forgeidExact=  data[k].Forgeid;
                            var Category_NameExact=  data[k].Category_Name;
                            var Family_TypeExact=  data[k].Family_Type;
                            var Instance_NameExact=  data[k].Instance_Name;
                            var UrnExact=  data[k].MGuid;
                            var VersionExact=  data[k].Version;
                            var CompIdExact=  data[k].CompId;

                            if(rt[0]==forgeidExact)
                            {
                                FloorDataStore+=forgeidExact + '~}' + Category_NameExact + '~}' + Family_TypeExact + '~}' + Instance_NameExact + '~}' + UrnExact + '~}' + VersionExact + '~}' + CompIdExact + '~}' + rt[1]+ '$';

                            }


                        }

                    }

                    var value = JSON.stringify({
                        'Urn': _URN_,
                        'VersionNo': Version_No,
                        'FloorDataStore': FloorDataStore,
                    });

                    $.ajax({
                        type: "POST",
                        contentType: 'application/json',
                        url: '@Url.Action("SaveSlopePoints", "ADAClearance")',
                        data: value,
                        dataType: "json",
                        success: function (result) {

                            var value = JSON.stringify({
                                'Urn': _URN_,
                                'VersionNo': Version_No,
                                'DataStore': DataStore,
                            });

                            $.ajax({
                                type: "POST",
                                contentType: 'application/json',
                                url: '@Url.Action("SaveBoundingPoints", "QuantityTest")',
                                data: value,
                                dataType: "json",
                                success: function (result) {

                                    alert("Geometry Saved Successfully");
                                    $('iframe[title=ADAClearance]').contents().find("#Modelloader").hide();

                                },
                                error: function (result) {
                                    $('iframe[title=ADAClearance]').contents().find("#Modelloader").hide();
                                    alert("Error1");
                                }
                            });

                        },
                        error: function (result) {
                            $('iframe[title=ADAClearance]').contents().find("#Modelloader").hide();
                            alert("Error1");
                        }
                    });



                    $('iframe[title=ADAClearance]').contents().find("#Modelloader").hide();
                },
                error: function (result) {
                    alert("Error1");
                    $('iframe[title=ADAClearance]').contents().find("#Modelloader").hide();
                }
            });


        }


        function funSlopeco()
        {
            console.log('room number in this specific floor:'
                      +  _specific_Floor_Rooms_Array.length);

            var  colorIndex = 0;
            var vAPoint = [];
            var vAPointMin = [];

            var storecooradinates="";

            $.each( _specific_Floor_Rooms_Array,
                  function(num,room){

                      console.log('room dbid:' + room.roomid);

                      if(colorIndex > 5)
                          colorIndex = 0;

                      var faceMeshArray = [];


                      var vBPointz = [];
                      var vCPoint = [];
                      vAPoint = [];
                      vAPointMin = [];




                      let bounds = new THREE.Box3()
                     , box = new THREE.Box3()
                       , instanceTree = viewer.impl.model.getData().instanceTree



                      // var instanceTree =  viewer.impl.model.getData().instanceTree;
                      instanceTree.enumNodeFragments(room.roomid, function(fragId){
                          var bbox = getModifiedWorldBoundingGetPoint(
                                 room.roomid,
                                viewer.model.getFragmentList()
                             );

                          console.log("boxxx");
                          console.log(bbox)
                          var renderProxy = viewer.impl.getRenderProxy(
                               viewer.model,
                              fragId);

                          var matrix = renderProxy.matrixWorld;
                          var indices = renderProxy.geometry.ib;
                          var positions = renderProxy.geometry.vb;
                          var stride = renderProxy.geometry.vbstride;
                          var offsets = renderProxy.geometry.offsets;

                          if (!offsets || offsets.length === 0) {
                              offsets = [{start: 0, count: indices.length, index: 0}];
                          }

                          var vA = new THREE.Vector3();
                          var vB = new THREE.Vector3();
                          var vC = new THREE.Vector3();


                          var MaxPoint = "";
                          var MinPoint = "";

                          for (var oi = 0, ol = offsets.length; oi < ol; ++oi) {

                              var start = offsets[oi].start;
                              var count = offsets[oi].count;
                              var index = offsets[oi].index;

                              var checkFace = 0;

                              for (var i = start, il = start + count; i < il; i += 3) {

                                  var vz = new THREE.Vector3();

                                  var a = index + indices[i];
                                  var b = index + indices[i + 1];
                                  var c = index + indices[i + 2];


                                  vA.fromArray(positions, a * stride);
                                  vB.fromArray(positions, b * stride);
                                  vC.fromArray(positions, c * stride);


                                  //vBPointz.push(vA.z);
                                  //vBPointz.push(vB.z);
                                  //vBPointz.push(vC.z);

                                  vA.applyMatrix4(matrix);
                                  vB.applyMatrix4(matrix);
                                  vC.applyMatrix4(matrix);

                                  var minz= Math.round(bbox.min.z * 100) / 100;
                                  var maxz= Math.round(bbox.max.z * 100) / 100;
                                  var Vcz=Math.round(vC.z * 100) / 100;


                               //   console.log("Z VAlues----");
                                 // console.log(maxz);
                                 // console.log("With Matrix");
                                 // console.log(Vcz);

                                  // ---For Upper Point---

                                  if(vAPoint.length==0)
                                  {
                                      if(maxz==Vcz)
                                      {

                                          vAPoint.push(vA.x + ":" + vA.y + ":" + vA.z);
                                          vAPoint.push(vB.x + ":" + vB.y + ":" + vB.z);
                                          vAPoint.push(vC.x + ":" + vC.y + ":" + vC.z) ;
                                      }
                                  }
                                  else
                                  {
                                      if(maxz==Vcz)
                                      {
                                          var leng=vAPoint.length;
                                         // console.log("Lenth-------");
                                        //  console.log(leng);
                                          var flagx=0;
                                          var flagy=0;
                                          var flagz=0;
                                          for(var j=0; j< leng;j++)
                                          {
                                              var w= vAPoint[j].split(':');

                                              if(Math.abs(vA.x-w[0])< 0.5 && Math.abs(vA.y-w[1])< 0.5)
                                              {
                                                //  console.log("--valuess1---");
                                                 // console.log(Math.abs(vA.x-w[0]));
                                                //  console.log(Math.abs(vA.y-w[1]));

                                                  flagx=1;
                                                  // vAPoint.push(vA.x + ":" + vA.y + ":" + 0.0);
                                              }

                                              if(Math.abs(vB.x-w[0])< 0.5 && Math.abs(vB.y-w[1])< 0.5)
                                              {
                                                 // console.log("--valuess2---");
                                                 // console.log(Math.abs(vB.x-w[0]));
                                                //  console.log(Math.abs(vB.y-w[1]));

                                                  flagy=1;
                                                  // vAPoint.push(vB.x + ":" + vB.y + ":" + 0.0);
                                              }

                                              if(Math.abs(vC.x-w[0])< 0.5 && Math.abs(vC.y-w[1])< 0.5)
                                              {
                                                  //console.log("--valuess3---");
                                                 // console.log(Math.abs(vC.x-w[0]));
                                                 // console.log(Math.abs(vC.y-w[1]));

                                                  flagz=1;
                                                  // vAPoint.push(vC.x + ":" + vC.y + ":" + 0.0);

                                              }
                                          }

                                          if(flagx==0)
                                          {
                                              vAPoint.push(vA.x + ":" + vA.y + ":" + vA.z );
                                          }
                                          if(flagy==0)
                                          {
                                              vAPoint.push(vB.x + ":" + vB.y + ":" + vB.z );
                                          }
                                          if(flagz==0)
                                          {
                                              vAPoint.push(vC.x + ":" + vC.y + ":" + vC.z );
                                          }


                                      }

                                  }

                                  // ---For Lower Point---

                                  //if(vAPointMin.length==0)
                                  //{
                                  //    if(minz==Vcz)
                                  //    {

                                  //        vAPointMin.push(vA.x + ":" + vA.y + ":" + vA.z);
                                  //        vAPointMin.push(vB.x + ":" + vB.y + ":" + vB.z);
                                  //        vAPointMin.push(vC.x + ":" + vC.y + ":" + vC.z) ;
                                  //    }
                                  //}
                                  //else
                                  //{
                                  //    if(minz==Vcz)
                                  //    {
                                  //        var leng=vAPointMin.length;
                                  //       // console.log("Lenth-------");
                                  //      //  console.log(leng);
                                  //        var flagx=0;
                                  //        var flagy=0;
                                  //        var flagz=0;
                                  //        for(var j=0; j< leng;j++)
                                  //        {
                                  //            var w= vAPointMin[j].split(':');

                                  //            if(Math.abs(vA.x-w[0])< 0.5 && Math.abs(vA.y-w[1])< 0.5)
                                  //            {
                                  //              //  console.log("--valuess1---");
                                  //              //  console.log(Math.abs(vA.x-w[0]));
                                  //              //  console.log(Math.abs(vA.y-w[1]));

                                  //                flagx=1;
                                  //                // vAPointMin.push(vA.x + ":" + vA.y + ":" + 0.0);
                                  //            }

                                  //            if(Math.abs(vB.x-w[0])< 0.5 && Math.abs(vB.y-w[1])< 0.5)
                                  //            {
                                  //               // console.log("--valuess2---");
                                  //              //  console.log(Math.abs(vB.x-w[0]));
                                  //              //  console.log(Math.abs(vB.y-w[1]));

                                  //                flagy=1;
                                  //                // vAPointMin.push(vB.x + ":" + vB.y + ":" + 0.0);
                                  //            }

                                  //            if(Math.abs(vC.x-w[0])< 0.5 && Math.abs(vC.y-w[1])< 0.5)
                                  //            {
                                  //               // console.log("--valuess3---");
                                  //                //console.log(Math.abs(vC.x-w[0]));
                                  //               // console.log(Math.abs(vC.y-w[1]));

                                  //                flagz=1;
                                  //                // vAPointMin.push(vC.x + ":" + vC.y + ":" + 0.0);

                                  //            }
                                  //        }

                                  //        if(flagx==0)
                                  //        {
                                  //            vAPointMin.push(vA.x + ":" + vA.y + ":" + vA.z );
                                  //        }
                                  //        if(flagy==0)
                                  //        {
                                  //            vAPointMin.push(vB.x + ":" + vB.y + ":" + vB.z );
                                  //        }
                                  //        if(flagz==0)
                                  //        {
                                  //            vAPointMin.push(vC.x + ":" + vC.y + ":" + vC.z );
                                  //        }


                                  //    }

                                  //}

                                  //   var faceGeometry = createFaceGeometry(vA, vB, vC);
                                  //   var faces = faceGeometry.faces;


                              }
                          }


                         // console.log("-----Ppoints------------");
                          vAPoint = GetUnique(vAPoint);
                          vAPointMin = GetUnique(vAPointMin);

                          console.log("-----lengthsss------------");
                          console.log(room.roomid);
                          console.log(vAPoint.length);
                          console.log(vAPointMin.length);

                          //if(vAPoint.length < vAPointMin.length)
                          //{
                              if(vAPoint.length <=200)
                              {
                                  storecooradinates+= room.roomid +'$';

                                  for (var i = 0; i < vAPoint.length; i++) {
                                      storecooradinates += vAPoint[i]+'#';
                                  }

                                  storecooradinates= storecooradinates.substring(0, storecooradinates.length-1);

                                  storecooradinates+='~';
                              }
                          //}
                          //else
                          //{
                              //if(vAPointMin.length <=200)
                              //{
                              //    storecooradinates+= room.roomid +'$';

                              //    for (var i = 0; i < vAPointMin.length; i++) {
                              //        storecooradinates += vAPointMin[i]+'#';
                              //    }

                              //    storecooradinates= storecooradinates.substring(0, storecooradinates.length-1);

                              //    storecooradinates+='~';
                              //}
                         // }

                          // console.log(vAPoint);

                      },true);

                      // room.defaultcolor = _materialArray[colorIndex];
                      // room.facemeshes = faceMeshArray;

                      colorIndex++;


                  });

            storecooradinates= storecooradinates.substring(0, storecooradinates.length-1);
            //console.log(" dasdsadasd");
            //  console.log(storecooradinates);
            return storecooradinates;

        }

        //========================= Conduits Function==========================================

        var  _specific_conduit_Array = [];

        function fnConduitCoordinate() {
            console.log("Within fnConduitCoordinate");

            $('iframe[title=Electrical-360]').contents().find("#Modelloader").show();

            if(_URN_=="")
            {
                alert("Please Load Model")
                $('iframe[title=Electrical-360]').contents().find("#Modelloader").hide();
                return;
            }


            var moddata = JSON.stringify({
                'Urn': _URN_,
                'VersionNo': Version_No
            });
            console.log("_URN_");
            console.log(_URN_);
            console.log("Version_No");
            console.log(Version_No);

            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("GetModelForgeid", "QuantityTest")',
                data: moddata,
                dataType: "json",
                success: function (data) {
                    console.log("data");
                    console.log(data);
                    console.log("data.length");
                    console.log(data.length);
                    var ConduitsDataStore = "";

                    var ForgeArrayPoint = [];
                    var DataStore = "";
                      _specific_conduit_Array = [];

                    for (var i = 0; i < data.length; i++) {

                        var forgeidExact = data[i].Forgeid;
                        var Category_NameExact = data[i].Category_Name;
                        var Family_TypeExact = data[i].Family_Type;
                        var Instance_NameExact = data[i].Instance_Name;
                        var UrnExact = data[i].MGuid;
                        var VersionExact = data[i].Version;
                        var CompIdExact = data[i].CompId;

                        if (Category_NameExact == "Conduits" || Category_NameExact == "Conduit Fittings") {
                            ForgeArrayPoint.push(forgeidExact);

                            console.log(Category_NameExact);

                            _specific_conduit_Array.push({
                                conduitid: forgeidExact,
                                defaultcolor: null,
                                facemeshes: null
                            })
                        }

                        var id1 = parseInt(forgeidExact);

                        var bbox = getModifiedWorldBoundingBoxRoomwise(
                        id1,
                       viewer.model.getFragmentList()
                      );

                        DataStore+=forgeidExact + '~}' + Category_NameExact + '~}' + Family_TypeExact + '~}' + Instance_NameExact + '~}' + UrnExact + '~}' + VersionExact + '~}' + CompIdExact + '~}' +  bbox.min.x + '~}' + bbox.max.x + '~}' + bbox.min.y + '~}' + bbox.max.y + '~}' + bbox.min.z + '~}' + bbox.max.z+ '#'  ;


                    }


                    var ConduitCoordinate = funConduitco(_specific_conduit_Array);
                    //console.log("---Finallll----");

                     var ConduitsData = ConduitCoordinate.split('~');

                     for (var i = 0; i < ConduitsData.length; i++) {

                         var rt = ConduitsData[i].split('$');

                        for (var k = 0; k < data.length; k++) {
                            var forgeidExact = data[k].Forgeid;
                            var Category_NameExact = data[k].Category_Name;
                            var Family_TypeExact = data[k].Family_Type;
                            var Instance_NameExact = data[k].Instance_Name;
                            var UrnExact = data[k].MGuid;
                            var VersionExact = data[k].Version;
                            var CompIdExact = data[k].CompId;

                            if (rt[0] == forgeidExact) {
                                ConduitsDataStore += forgeidExact + '~}' + Category_NameExact + '~}' + Family_TypeExact + '~}' + Instance_NameExact + '~}' + UrnExact + '~}' + VersionExact + '~}' + CompIdExact + '~}' + rt[1] + '$';

                            }


                        }

                    }

                    var value = JSON.stringify({
                        'Urn': _URN_,
                        'VersionNo': Version_No,
                        'ConduitsDataStore': ConduitsDataStore,
                    });

                    $.ajax({
                        type: "POST",
                        contentType: 'application/json',
                        url: '@Url.Action("SaveCoduitsPoints", "Electrical360")',
                        data: value,
                        dataType: "json",
                        success: function (result) {

                            var value = JSON.stringify({
                                'Urn': _URN_,
                                'VersionNo': Version_No,
                                'DataStore': DataStore,
                            });

                            $.ajax({
                                type: "POST",
                                contentType: 'application/json',
                                url: '@Url.Action("SaveBoundingPoints", "QuantityTest")',
                                data: value,
                                dataType: "json",
                                success: function (result) {

                                    alert("Geometry Saved Successfully");
                                    $('iframe[title=Electrical-360]').contents().find("#Modelloader").hide();

                                },
                                error: function (result) {
                                    $('iframe[title=Electrical-360]').contents().find("#Modelloader").hide();
                                    alert("Error1");
                                }
                            });

                        },
                        error: function (result) {
                            $('iframe[title=Electrical-360]').contents().find("#Modelloader").hide();
                            alert("Error1");
                        }
                    });

                },
                error: function (result) {
                    $('iframe[title=Electrical-360]').contents().find("#Modelloader").hide();
                    alert("Error1");
                }
            });
        }

        function funConduitco(_specific_conduit_Array) {

            console.log("Within funConduitco");

            console.log('conduit id in this specific floor:'
                      + _specific_conduit_Array.conduitid);

            var colorIndex = 0;
            var vAPoint = [];
            var vAPointMin = [];

            var storecooradinates = "";

            $.each(_specific_conduit_Array,
                  function (num, conduit) {

                      console.log('conduit dbid:' + conduit.conduitid);

                      if (colorIndex > 5)
                          colorIndex = 0;

                      var faceMeshArray = [];


                      var vBPointz = [];
                      var vCPoint = [];
                      vAPoint = [];
                      vAPointMin = [];

                      let bounds = new THREE.Box3()
                     , box = new THREE.Box3()
                       , instanceTree = viewer.impl.model.getData().instanceTree

                      // var instanceTree =  viewer.impl.model.getData().instanceTree;
                      instanceTree.enumNodeFragments(conduit.conduitid, function (fragId) {
                          var bbox = getModifiedWorldBoundingGetPoint(
                                 conduit.conduitid,
                                viewer.model.getFragmentList()
                             );

                          console.log("boxxx");
                          console.log(bbox)
                          var renderProxy = viewer.impl.getRenderProxy(
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

                          var vA = new THREE.Vector3();
                          var vB = new THREE.Vector3();
                          var vC = new THREE.Vector3();


                          var MaxPoint = "";
                          var MinPoint = "";

                          for (var oi = 0, ol = offsets.length; oi < ol; ++oi) {

                              var start = offsets[oi].start;
                              var count = offsets[oi].count;
                              var index = offsets[oi].index;

                              var checkFace = 0;

                              for (var i = start, il = start + count; i < il; i += 3) {

                                  var vz = new THREE.Vector3();

                                  var a = index + indices[i];
                                  var b = index + indices[i + 1];
                                  var c = index + indices[i + 2];


                                  vA.fromArray(positions, a * stride);
                                  vB.fromArray(positions, b * stride);
                                  vC.fromArray(positions, c * stride);


                                  vA.applyMatrix4(matrix);
                                  vB.applyMatrix4(matrix);
                                  vC.applyMatrix4(matrix);


                                  console.log("vA");
                                  console.log(vA);
                                  console.log("vB");
                                  console.log(vB);
                                  console.log("vC");
                                  console.log(vC);

                                  vAPoint.push(vA.x + ":" + vA.y + ":" + vA.z);
                                  vAPoint.push(vB.x + ":" + vB.y + ":" + vB.z);
                                  vAPoint.push(vC.x + ":" + vC.y + ":" + vC.z) ;

                              }
                          }


                          // console.log("-----Ppoints------------");
                       //   vAPoint = GetUnique(vAPoint);
                         // vAPointMin = GetUnique(vAPointMin);

                         // console.log("-----lengthsss------------");
                        ///  console.log(conduit.conduitid);
                         // console.log(vAPoint.length);
                        //  console.log(vAPointMin.length);

                          //if(vAPoint.length < vAPointMin.length)
                          //{
                          if (vAPoint.length <= 200) {
                              storecooradinates += conduit.conduitid + '$';

                              for (var i = 0; i < vAPoint.length; i++) {
                                  storecooradinates += vAPoint[i] + '#';
                              }

                              storecooradinates = storecooradinates.substring(0, storecooradinates.length - 1);

                              storecooradinates += '~';
                          }

                      }, true);

                      colorIndex++;
                  });

            storecooradinates = storecooradinates.substring(0, storecooradinates.length - 1);

            return storecooradinates;

        }



        function getWindowHeadercolor() {

            var storedColor = $('.navbar').css('background-color');
            $('.k-header').css('backgroundColor', storedColor);
        }



