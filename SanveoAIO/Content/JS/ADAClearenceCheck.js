
        function ADAClearanceCheck(forgeids, alldbId, filenameset1, categoryset1, revitidset1, filenameset2, categoryset2, revitidset2) {
            //var progressbarwidth=20;
            var _viewer = viewer;
            var instanceTree;
            var set1Ids = new Array();
            var set2Ids = new Array();

            ///click id////////
            //var screenPoint = {
            //    x: event.clientX,
            //    y: event.clientY
            //};

            //var n = normalize(screenPoint);
            //var dbId = getHitDbId(n.x, n.y);
            //console.log("ClickID");
            //console.log(dbId);
            //set1Ids.push(dbId);

            ///click id////////

            var iv;
            var obj1x;
            for (let iv = viewer.impl.scene.children.length - 1; iv >= 0; iv--) {
                obj1x = viewer.impl.scene.children[iv];
                viewer.impl.scene.remove(obj1x);
            }

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

            set1Ids = forgeids;
            set2Ids = alldbId;

            var front_clear_value = $('iframe[title=ADA]').contents().find("#clear_fronttext").val();
            var side_clear_value = $('iframe[title=ADA]').contents().find("#clear_sidetext").val();
            var top_clear_value = $('iframe[title=ADA]').contents().find("#clear_toptext").val();
            var bot_clear_value = $('iframe[title=ADA]').contents().find("#clear_bottomtext").val();
            var back_clear_value = $('iframe[title=ADA]').contents().find("#clear_backtext").val();

            var front_clear = parseInt(front_clear_value);
            var side_clear = parseInt(side_clear_value);
            var top_clear = parseInt(top_clear_value);
            var bot_clear = parseInt(bot_clear_value);
            var back_clear = parseInt(back_clear_value);

            console.log("boxx values");
            console.log(front_clear);
            console.log(side_clear);
            console.log(top_clear);
            console.log(bot_clear);
            console.log(back_clear);
            // //var w = set1Ids.length/100;
            // alert(w);
            var w = (70 / set1Ids.length);
            var progressbarwidth = 30;
            for (var i = 0; i < set1Ids.length; i++) {
                progressbarwidth += w;
                $('iframe[title=ADA]').contents().find("#myBar").css("width", progressbarwidth + '%');

                var id1 = parseInt(set1Ids[i]);
                var filenameset1value = filenameset1[i];
                var categoryset1value = categoryset1[i];
                var revitidset1value = revitidset1[i];

                let bounds = new THREE.Box3()
                    , box = new THREE.Box3()
                    , instanceTree = viewer.impl.model.getData().instanceTree
                    , fragList = viewer.impl.model.getFragmentList()

                instanceTree.enumNodeFragments(id1, function (fragId) {
                    fragList.getWorldBounds(fragId, box)
                    bounds.union(box);
                }, true)

                var bottomPnt1 = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
                var bottomPnt2 = new THREE.Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
                var bottomPnt3 = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
                var bottomPnt4 = new THREE.Vector3(bounds.min.x, bounds.max.y, bounds.min.z);


                //var side_angle1 = Math.round(bottomPnt3.angleTo(bottomPnt4));
                var side_angle1 = Math.atan2(bottomPnt3.y - bottomPnt4.y, bottomPnt3.x - bottomPnt4.x);
                var pnt1 = bottomPnt4.x + Math.cos(side_angle1 - Math.PI) * side_clear;
                var pnt2 = bottomPnt4.y + Math.sin(side_angle1 - Math.PI) * side_clear;
                var bottomPnt1_Side1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Side1x);

                var pnt1 = bottomPnt4.x + Math.cos(side_angle1 + Math.PI) * side_clear;
                var pnt2 = bottomPnt4.y + Math.sin(side_angle1 + Math.PI) * side_clear;
                var bottomPnt1_Side1y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Side1y);

                var side_angle2 = Math.atan2(bottomPnt4.y - bottomPnt3.y, bottomPnt4.x - bottomPnt3.x);
                var pnt1 = bottomPnt3.x + Math.cos(side_angle2 - Math.PI) * side_clear;
                var pnt2 = bottomPnt3.y + Math.sin(side_angle2 - Math.PI) * side_clear;
                var bottomPnt1_Side2x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Side2x);

                var pnt1 = bottomPnt3.x + Math.cos(side_angle2 + Math.PI) * side_clear;
                var pnt2 = bottomPnt3.y + Math.sin(side_angle2 + Math.PI) * side_clear;
                var bottomPnt1_Side2y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Side2y);

                var bottomPnt1_Top1 = new THREE.Vector3(bottomPnt3.x, bottomPnt3.y, bounds.max.z + top_clear);
                bounds.expandByPoint(bottomPnt1_Top1);

                var bottomPnt1_Bot1 = new THREE.Vector3(bottomPnt3.x, bottomPnt3.y, bounds.min.z - bot_clear);
                bounds.expandByPoint(bottomPnt1_Bot1);

                //var front_angle1 = Math.round(bottomPnt1.angleTo(bottomPnt4));
                var front_angle1 = Math.atan2(bottomPnt1.y - bottomPnt4.y, bottomPnt1.x - bottomPnt4.x);
                var front_angle2 = Math.atan2(bottomPnt4.y - bottomPnt1.y, bottomPnt4.x - bottomPnt1.x);

                var pnt1 = bottomPnt1.x + Math.cos(front_angle1) * front_clear;
                var pnt2 = bottomPnt1.y + Math.sin(front_angle1) * front_clear;
                var bottomPnt1_Front1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Front1x);

                var pnt1 = bottomPnt1.x + Math.cos(front_angle2) * front_clear;
                var pnt2 = bottomPnt1.y + Math.sin(front_angle2) * front_clear;
                var bottomPnt1_Front1y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Front1y);

                ////var back_angle1 = Math.round(bottomPnt1.angleTo(bottomPnt4));
                var back_angle1 = Math.atan2(bottomPnt1.y - bottomPnt4.y, bottomPnt1.x - bottomPnt4.x);
                var back_angle2 = Math.atan2(bottomPnt4.y - bottomPnt1.y, bottomPnt4.x - bottomPnt1.x);

                var pnt1 = bottomPnt4.x + Math.cos(back_angle1) * back_clear;
                var pnt2 = bottomPnt4.y + Math.sin(back_angle1) * back_clear;
                var bottomPnt1_Back1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Back1x);

                var pnt1 = bottomPnt4.x + Math.cos(back_angle2) * back_clear;
                var pnt2 = bottomPnt4.y + Math.sin(back_angle2) * back_clear;
                var bottomPnt1_Back1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
                bounds.expandByPoint(bottomPnt1_Back1x);

                var bottomPnt1x = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
                var bottomPnt2x = new THREE.Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
                var bottomPnt3x = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
                var bottomPnt4x = new THREE.Vector3(bounds.min.x, bounds.max.y, bounds.min.z);


                var topPnt1 = new THREE.Vector3(bottomPnt1x.x, bottomPnt1x.y, bounds.max.z);
                var topPnt2 = new THREE.Vector3(bottomPnt2x.x, bottomPnt2x.y, bounds.max.z);
                var topPnt3 = new THREE.Vector3(bottomPnt3x.x, bottomPnt3x.y, bounds.max.z);
                var topPnt4 = new THREE.Vector3(bottomPnt4x.x, bottomPnt4x.y, bounds.max.z);


                geometry = new THREE.Geometry();
                geometry.vertices.push(bottomPnt1x);
                geometry.vertices.push(bottomPnt2x);
                geometry.vertices.push(bottomPnt3x);
                geometry.vertices.push(bottomPnt4x);
                geometry.vertices.push(bottomPnt1x);
                line = new THREE.Line(geometry, material2);
                // viewer.impl.scene.add(line);

                geometry = new THREE.Geometry();
                geometry.vertices.push(topPnt1);
                geometry.vertices.push(topPnt2);
                geometry.vertices.push(topPnt3);
                geometry.vertices.push(topPnt4);
                geometry.vertices.push(topPnt1);
                line = new THREE.Line(geometry, material2);
                //  viewer.impl.scene.add(line);

                geometry = new THREE.Geometry();
                geometry.vertices.push(topPnt1);
                geometry.vertices.push(bottomPnt1x);
                line = new THREE.Line(geometry, material2);
                // viewer.impl.scene.add(line);

                geometry = new THREE.Geometry();
                geometry.vertices.push(topPnt2);
                geometry.vertices.push(bottomPnt2x);
                line = new THREE.Line(geometry, material2);
                // viewer.impl.scene.add(line);

                geometry = new THREE.Geometry();
                geometry.vertices.push(topPnt3);
                geometry.vertices.push(bottomPnt3x);
                line = new THREE.Line(geometry, material2);
                // viewer.impl.scene.add(line);

                geometry = new THREE.Geometry();
                geometry.vertices.push(topPnt4);
                geometry.vertices.push(bottomPnt4x);
                line = new THREE.Line(geometry, material2);
                // viewer.impl.scene.add(line);

                var pt1 = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
                var pt2 = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
                var bBox_new1a = new THREE.Box3(pt1, pt2);


                geometry = new THREE.Geometry();
                geometry.vertices.push(pt1);
                geometry.vertices.push(pt2);
                line = new THREE.Line(geometry, material1);
                // viewer.impl.scene.add(line);

                for (var y = 0; y < set2Ids.length; y++) {
                    var id2 = parseInt(set2Ids[y]);
                    var filenameset2value = filenameset2[i];
                    var categoryset2value = categoryset2[i];
                    var revitidset2value = revitidset2[i];

                    let bound2 = new THREE.Box3()
                        , box2 = new THREE.Box3()
                        , instanceTree = viewer.impl.model.getData().instanceTree
                        , fragList = viewer.impl.model.getFragmentList()

                    instanceTree.enumNodeFragments(id2, function (fragId) {
                        fragList.getWorldBounds(fragId, box2)
                        bound2.union(box2);
                    }, true)

                    var pt1x = new THREE.Vector3(bound2.min.x, bound2.min.y, bound2.min.z);
                    var pt2x = new THREE.Vector3(bound2.max.x, bound2.max.y, bound2.max.z);
                    var pt3x = new THREE.Vector3(bound2.min.x, bound2.max.y, bound2.max.z);
                    var pt4x = new THREE.Vector3(bound2.max.x, bound2.min.y, bound2.min.z);
                    var bBox_new2a = new THREE.Box3(pt1x, pt2x);


                    if (bBox_new2a.isIntersectionBox(bBox_new1a) == true) {
                        console.log("final result");
                        console.log((id1 + "/" + id2 + ","));
                        $('iframe[title=ADA]').contents().find("#clearancedatatable tbody").append('<tr><td style="width:12%">' + filenameset1value + '</td><td style="width:12%">' + categoryset1value + '</td><td style="width:12%">' + revitidset1value + '</td><td style="width:12%"><input type ="button" value = ' + id1 + '  id =' + id1 + ' onclick="window.parent.Forgeid(this.id)"/></td><td style="width:12%"><input type ="button" value = ' + id1 + '  id =' + id1 + ' onclick="window.parent.ADAHighlightbox1(this.id)"/></td><td style="width:12%">' + filenameset2value + '</td><td style="width:12%">' + categoryset2value + '</td><td style="width:12%">' + revitidset2value + '</td><td style="width:12%"><input type ="button" value = ' + id2 + '  id ="' + id2 + '" onclick="window.parent.Forgeid(this.id)"/></td><td style="width:12%"><input type ="button" value = ' + id2 + '  id =' + id2 + ' onclick="window.parent.ADAHighlightbox2(this.id)"/></td></tr>');

                        //<td style="width:15px">' + id1 + '</td> <td>' + id2 + '</td>);
                        $('iframe[title=ADA]').contents().find("#clearancedatatable").show();
                        // $("#showids").append(id1 + "/" + id2 + ",")
                        //alert(id1)                        }
                    }
                    var size = "";
                    if (size == null) return;
                };
                //set1Ids = [];
                //set2Ids = [];
            }
            if ($('iframe[title=ADA]').contents().find("#clearancedatatable tbody").children().length == 0) {
                $('iframe[title=ADA]').contents().find("#clearancedatatable tbody").append('<tr><td colspan="2">No records found !!</td></tr>');
                $('iframe[title=ADA]').contents().find("#clearancedatatable").show();
                var ADAWindow = $('#ADAWindow').data('kendoWindow');
                kendo.ui.progress(ADAWindow.element, false);
            }
            var ADAWindow = $('#ADAWindow').data('kendoWindow');
            kendo.ui.progress(ADAWindow.element, false);
        }

        function ADAHighlightbox1(id) {
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

            //  let bounds = new THREE.Box3()
            //, box = new THREE.Box3()
            //, instanceTree = viewer.impl.model.getData().instanceTree
            //, fragList = viewer.impl.model.getFragmentList()

            //  instanceTree.enumNodeFragments(id1, function (fragId) {
            //      fragList.getWorldBounds(fragId, box)
            //      bounds.union(box);
            //  }, true)



            var bbox = getModifiedWorldBoundingBox(
                id1,
                viewer.model.getFragmentList()
            );

            drawBox(bbox.min, bbox.max);



            //-----older method

            //  let bounds = new THREE.Box3()
            //, box = new THREE.Box3()
            //, instanceTree = viewer.impl.model.getData().instanceTree
            //, fragList = viewer.impl.model.getFragmentList()

            //  instanceTree.enumNodeFragments(id1, function (fragId) {
            //      fragList.getWorldBounds(fragId, box)
            //      bounds.union(box);
            //  }, true)

            //  var bottomPnt1 = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt2 = new THREE.Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt3 = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            //  var bottomPnt4 = new THREE.Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            //  console.log("Co-ordinate");
            //  console.log(bottomPnt1);
            //  console.log(bottomPnt2);
            //  console.log(bottomPnt3);
            //  console.log(bottomPnt4);

            //  var side_angle1 = Math.atan2(bottomPnt3.y - bottomPnt4.y, bottomPnt3.x - bottomPnt4.x);
            //  var pnt1 = bottomPnt4.x + Math.cos(side_angle1 - Math.PI) * side_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(side_angle1 - Math.PI) * side_clear;
            //  var bottomPnt1_Side1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side1x);

            //  var pnt1 = bottomPnt4.x + Math.cos(side_angle1 + Math.PI) * side_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(side_angle1 + Math.PI) * side_clear;
            //  var bottomPnt1_Side1y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side1y);

            //  var side_angle2 = Math.atan2(bottomPnt4.y - bottomPnt3.y, bottomPnt4.x - bottomPnt3.x);
            //  var pnt1 = bottomPnt3.x + Math.cos(side_angle2 - Math.PI) * side_clear;
            //  var pnt2 = bottomPnt3.y + Math.sin(side_angle2 - Math.PI) * side_clear;
            //  var bottomPnt1_Side2x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side2x);

            //  var pnt1 = bottomPnt3.x + Math.cos(side_angle2 + Math.PI) * side_clear;
            //  var pnt2 = bottomPnt3.y + Math.sin(side_angle2 + Math.PI) * side_clear;
            //  var bottomPnt1_Side2y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side2y);

            //  var bottomPnt1_Top1 = new THREE.Vector3(bottomPnt3.x, bottomPnt3.y, bounds.max.z + top_clear);
            //  bounds.expandByPoint(bottomPnt1_Top1);

            //  var bottomPnt1_Bot1 = new THREE.Vector3(bottomPnt3.x, bottomPnt3.y, bounds.min.z - bot_clear);
            //  bounds.expandByPoint(bottomPnt1_Bot1);

            //  var front_angle1 = Math.atan2(bottomPnt1.y - bottomPnt4.y, bottomPnt1.x - bottomPnt4.x);
            //  var front_angle2 = Math.atan2(bottomPnt4.y - bottomPnt1.y, bottomPnt4.x - bottomPnt1.x);

            //  var pnt1 = bottomPnt1.x + Math.cos(front_angle1 ) * front_clear;
            //  var pnt2 = bottomPnt1.y + Math.sin(front_angle1 ) * front_clear;
            //  var bottomPnt1_Front1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Front1x);

            //  var pnt1 = bottomPnt1.x + Math.cos(front_angle2 ) * front_clear;
            //  var pnt2 = bottomPnt1.y + Math.sin(front_angle2 ) * front_clear;
            //  var bottomPnt1_Front1y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Front1y);

            //  var back_angle1 = Math.atan2(bottomPnt1.y - bottomPnt4.y, bottomPnt1.x - bottomPnt4.x);
            //  var back_angle2 = Math.atan2(bottomPnt4.y - bottomPnt1.y, bottomPnt4.x - bottomPnt1.x);

            //  var pnt1 = bottomPnt4.x + Math.cos(back_angle1) * back_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(back_angle1) * back_clear;
            //  var bottomPnt1_Back1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Back1x);

            //  var pnt1 = bottomPnt4.x + Math.cos(back_angle2) * back_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(back_angle2) * back_clear;
            //  var bottomPnt1_Back1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Back1x);

            //  var bottomPnt1x = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt2x = new THREE.Vector3(bounds.max.x, bounds.min.y, bounds.min.z);// Bottom right X:Bottom Right:Y
            //  var bottomPnt3x = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            //  var bottomPnt4x = new THREE.Vector3(bounds.min.x, bounds.max.y, bounds.min.z); //top left-X :top left-Y

            //  var topPnt1 = new THREE.Vector3(bottomPnt1x.x, bottomPnt1x.y, bounds.max.z);
            //  var topPnt2 = new THREE.Vector3(bottomPnt2x.x, bottomPnt2x.y, bounds.max.z); // Bottom right X:Bottom Right:Y
            //  var topPnt3 = new THREE.Vector3(bottomPnt3x.x, bottomPnt3x.y, bounds.max.z);
            //  var topPnt4 = new THREE.Vector3(bottomPnt4x.x, bottomPnt4x.y, bounds.max.z); //top left-X :top left-Y

            //  console.log("Set1IDS.............");
            //  console.log(id);
            //  console.log(bottomPnt1x);
            //  console.log(bottomPnt2x);
            //  console.log(bottomPnt3x);
            //  console.log(bottomPnt4x);

            //  console.log(topPnt1);
            //  console.log(topPnt2);
            //  console.log(topPnt3);
            //  console.log(topPnt4);




            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(bottomPnt1x);
            //  geometry.vertices.push(bottomPnt2x);
            //  geometry.vertices.push(bottomPnt3x);
            //  geometry.vertices.push(bottomPnt4x);
            //  geometry.vertices.push(bottomPnt1x);
            //  line = new THREE.Line(geometry, material2);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt1);
            //  geometry.vertices.push(topPnt2);
            //  geometry.vertices.push(topPnt3);
            //  geometry.vertices.push(topPnt4);
            //  geometry.vertices.push(topPnt1);
            //  line = new THREE.Line(geometry, material2);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt1);
            //  geometry.vertices.push(bottomPnt1x);
            //  line = new THREE.Line(geometry, material2);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt2);
            //  geometry.vertices.push(bottomPnt2x);
            //  line = new THREE.Line(geometry, material2);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt3);
            //  geometry.vertices.push(bottomPnt3x);
            //  line = new THREE.Line(geometry, material2);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt4);
            //  geometry.vertices.push(bottomPnt4x);
            //  line = new THREE.Line(geometry, material2);
            //  viewer.impl.scene.add(line);


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
        var drawboxtoggle = 0;

        function drawBox(min, max) {

            if (drawboxtoggle == 0) {
                drawboxtoggle = 1;


                var material2 = new THREE.LineBasicMaterial({
                    color: 0xff0000,
                    linewidth: 5,
                    linecap: 'round', //ignored by WebGLRenderer
                    linejoin: 'round' //ignored by WebGLRenderer

                });

                //viewer.impl.matman().addMaterial(
                //    'ADN-Material-Line',
                //    material,
                //    true);

                drawLines([

                    { x: min.x, y: min.y, z: min.z },
                    { x: max.x, y: min.y, z: min.z },

                    { x: max.x, y: min.y, z: min.z },
                    { x: max.x, y: min.y, z: max.z },

                    { x: max.x, y: min.y, z: max.z },
                    { x: min.x, y: min.y, z: max.z },

                    { x: min.x, y: min.y, z: max.z },
                    { x: min.x, y: min.y, z: min.z },

                    { x: min.x, y: max.y, z: max.z },
                    { x: max.x, y: max.y, z: max.z },

                    { x: max.x, y: max.y, z: max.z },
                    { x: max.x, y: max.y, z: min.z },

                    { x: max.x, y: max.y, z: min.z },
                    { x: min.x, y: max.y, z: min.z },

                    { x: min.x, y: max.y, z: min.z },
                    { x: min.x, y: max.y, z: max.z },

                    { x: min.x, y: min.y, z: min.z },
                    { x: min.x, y: max.y, z: min.z },

                    { x: max.x, y: min.y, z: min.z },
                    { x: max.x, y: max.y, z: min.z },

                    { x: max.x, y: min.y, z: max.z },
                    { x: max.x, y: max.y, z: max.z },

                    { x: min.x, y: min.y, z: max.z },
                    { x: min.x, y: max.y, z: max.z }],

                    material2);

                console.log("Box Completed");


                viewer.impl.sceneUpdated(true);

            }
            else {
                drawboxtoggle = 0;
                viewer.impl.removeOverlayScene(_overlaySceneName4);
            }

            //var material = new THREE.LineBasicMaterial({
            //    color: 0xffff00,
            //    linewidth: 2
            //});




        }

        function drawLines(coordsArray, material) {
            console.log(coordsArray.length);
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

                //viewer.impl.scene.add(line);
                viewer.impl.addOverlay(_overlaySceneName4, line);
            }

            console.log("Lines Completed");
        }


        function ADAHighlightbox2(id) {

            var material1 = new THREE.LineBasicMaterial({
                color: 0xff0000,
                linewidth: 5,
                linecap: 'round', //ignored by WebGLRenderer
                linejoin: 'round' //ignored by WebGLRenderer
            });

            var material2 = new THREE.LineBasicMaterial({
                color: 0x0077ff,
                linewidth: 5,
                linecap: 'round', //ignored by WebGLRenderer
                linejoin: 'round' //ignored by WebGLRenderer
            });

            var front_clear_value = $('iframe[title=ADA]').contents().find("#clear_fronttext").val();
            var side_clear_value = $('iframe[title=ADA]').contents().find("#clear_sidetext").val();
            var top_clear_value = $('iframe[title=ADA]').contents().find("#clear_toptext").val();
            var bot_clear_value = $('iframe[title=ADA]').contents().find("#clear_bottomtext").val();
            var back_clear_value = $('iframe[title=ADA]').contents().find("#clear_backtext").val();


            var front_clear = parseInt(front_clear_value);
            var side_clear = parseInt(side_clear_value);
            var top_clear = parseInt(top_clear_value);
            var bot_clear = parseInt(bot_clear_value);
            var back_clear = parseInt(back_clear_value);

            var id1 = parseInt(id);


            var bBox = getModifiedWorldBoundingBox(
                id1,
                viewer.impl.model.getFragmentList()
            );

            console.log(bBox.min);
            console.log(bBox.max);

            drawBox(bBox.min, bBox.max);


            //  let bounds = new THREE.Box3()
            //, box = new THREE.Box3()
            //, instanceTree = viewer.impl.model.getData().instanceTree
            //, fragList = viewer.impl.model.getFragmentList()

            //  instanceTree.enumNodeFragments(id1, function (fragId) {
            //      //fragList.getWorldBounds(fragId, box)
            //      //bounds.union(box);

            //  }, true)


            //  var bottomPnt1 = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt2 = new THREE.Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt3 = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            //  var bottomPnt4 = new THREE.Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            //  console.log("Co-ordinate");
            //  console.log(bottomPnt1);
            //  console.log(bottomPnt2);
            //  console.log(bottomPnt3);
            //  console.log(bottomPnt4);

            //  var side_angle1 = Math.atan2(bottomPnt3.y - bottomPnt4.y, bottomPnt3.x - bottomPnt4.x);
            //  var pnt1 = bottomPnt4.x + Math.cos(side_angle1 - Math.PI) * side_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(side_angle1 - Math.PI) * side_clear;
            //  var bottomPnt1_Side1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side1x);

            //  var pnt1 = bottomPnt4.x + Math.cos(side_angle1 + Math.PI) * side_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(side_angle1 + Math.PI) * side_clear;
            //  var bottomPnt1_Side1y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side1y);

            //  var side_angle2 = Math.atan2(bottomPnt4.y - bottomPnt3.y, bottomPnt4.x - bottomPnt3.x);
            //  var pnt1 = bottomPnt3.x + Math.cos(side_angle2 - Math.PI) * side_clear;
            //  var pnt2 = bottomPnt3.y + Math.sin(side_angle2 - Math.PI) * side_clear;
            //  var bottomPnt1_Side2x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side2x);

            //  var pnt1 = bottomPnt3.x + Math.cos(side_angle2 + Math.PI) * side_clear;
            //  var pnt2 = bottomPnt3.y + Math.sin(side_angle2 + Math.PI) * side_clear;
            //  var bottomPnt1_Side2y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Side2y);

            //  var bottomPnt1_Top1 = new THREE.Vector3(bottomPnt3.x, bottomPnt3.y, bounds.max.z + top_clear);
            //  bounds.expandByPoint(bottomPnt1_Top1);

            //  var bottomPnt1_Bot1 = new THREE.Vector3(bottomPnt3.x, bottomPnt3.y, bounds.min.z - bot_clear);
            //  bounds.expandByPoint(bottomPnt1_Bot1);

            //  var front_angle1 = Math.atan2(bottomPnt1.y - bottomPnt4.y, bottomPnt1.x - bottomPnt4.x);
            //  var front_angle2 = Math.atan2(bottomPnt4.y - bottomPnt1.y, bottomPnt4.x - bottomPnt1.x);

            //  var pnt1 = bottomPnt1.x + Math.cos(front_angle1 ) * front_clear;
            //  var pnt2 = bottomPnt1.y + Math.sin(front_angle1 ) * front_clear;
            //  var bottomPnt1_Front1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Front1x);

            //  var pnt1 = bottomPnt1.x + Math.cos(front_angle2 ) * front_clear;
            //  var pnt2 = bottomPnt1.y + Math.sin(front_angle2 ) * front_clear;
            //  var bottomPnt1_Front1y = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Front1y);

            //  var back_angle1 = Math.atan2(bottomPnt1.y - bottomPnt4.y, bottomPnt1.x - bottomPnt4.x);
            //  var back_angle2 = Math.atan2(bottomPnt4.y - bottomPnt1.y, bottomPnt4.x - bottomPnt1.x);

            //  var pnt1 = bottomPnt4.x + Math.cos(back_angle1) * back_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(back_angle1) * back_clear;
            //  var bottomPnt1_Back1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Back1x);

            //  var pnt1 = bottomPnt4.x + Math.cos(back_angle2) * back_clear;
            //  var pnt2 = bottomPnt4.y + Math.sin(back_angle2) * back_clear;
            //  var bottomPnt1_Back1x = new THREE.Vector3(pnt1, pnt2, bottomPnt4.z);
            //  bounds.expandByPoint(bottomPnt1_Back1x);


            //  var bottomPnt1x = new THREE.Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt2x = new THREE.Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            //  var bottomPnt3x = new THREE.Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            //  var bottomPnt4x = new THREE.Vector3(bounds.min.x, bounds.max.y, bounds.min.z);

            //  var topPnt1 = new THREE.Vector3(bottomPnt1x.x, bottomPnt1x.y, bounds.max.z);
            //  var topPnt2 = new THREE.Vector3(bottomPnt2x.x, bottomPnt2x.y, bounds.max.z);
            //  var topPnt3 = new THREE.Vector3(bottomPnt3x.x, bottomPnt3x.y, bounds.max.z);
            //  var topPnt4 = new THREE.Vector3(bottomPnt4x.x, bottomPnt4x.y, bounds.max.z);

            //  console.log("Set2IDS.............");
            //  console.log(id);
            //  console.log(bottomPnt1x);
            //  console.log(bottomPnt2x);
            //  console.log(bottomPnt3x);
            //  console.log(bottomPnt4x);

            //  console.log(topPnt1);
            //  console.log(topPnt2);
            //  console.log(topPnt3);
            //  console.log(topPnt4);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(bottomPnt1x);
            //  geometry.vertices.push(bottomPnt2x);
            //  geometry.vertices.push(bottomPnt3x);
            //  geometry.vertices.push(bottomPnt4x);
            //  geometry.vertices.push(bottomPnt1x);
            //  line = new THREE.Line(geometry, material1);
            //viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt1);
            //  geometry.vertices.push(topPnt2);
            //  geometry.vertices.push(topPnt3);
            //  geometry.vertices.push(topPnt4);
            //  geometry.vertices.push(topPnt1);
            //  line = new THREE.Line(geometry, material1);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt1);
            //  geometry.vertices.push(bottomPnt1x);
            //  line = new THREE.Line(geometry, material1);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt2);
            //  geometry.vertices.push(bottomPnt2x);
            //  line = new THREE.Line(geometry, material1);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt3);
            //  geometry.vertices.push(bottomPnt3x);
            //  line = new THREE.Line(geometry, material1);
            //  viewer.impl.scene.add(line);

            //  geometry = new THREE.Geometry();
            //  geometry.vertices.push(topPnt4);
            //  geometry.vertices.push(bottomPnt4x);
            //  line = new THREE.Line(geometry, material1);
            //  viewer.impl.scene.add(line);
        }

        //6th #change
        //var points =[]
        //$('#viewer').click (function (e) {

        //    // Get 2D drawing dimension
        //    //alert("Within EventListener")
        //    var layoutBox = viewer.impl.getVisibleBounds();
        //    var width = layoutBox.max.x - layoutBox.min.x;
        //    var height = layoutBox.max.y - layoutBox.min.y;
        //    var zvalue = layoutBox.max.z - layoutBox.min.z;

        //    var viewport = viewer.impl.clientToViewport(e.clientX, e.clientY);
        //    var point = [viewport.x*width, viewport.y*height, viewport.z*zvalue];

        //    // Show the 2D drawing X, Y coordinates on mouse click
        //    console.log("point");
        //    console.log(point);

        //    var min = { x:154.9046989660603,y:45.57690748558723,z:0}
        //    var max = { x:101.35828895326821,y:-60.633027156665094,z:-26.364577293395996}

        //    console.log(min)
        //    console.log(max)
        //    drawBox(min, max)
        //    //console.log("bb")
        //    //console.log(bb)

        //    points.push(point)
        //    console.log("points")
        //    console.log(points)
        //});
        //6th #change

