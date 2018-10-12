
        var DextractFileName = "";
        $("#BtnDeleteFile").click(function () {

            var moddata = JSON.stringify({
            'ModelName': modelnames
    });
            if (confirm("Do you really want to delete?")) {
            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("DeleteModelFile", "Home")',
                data: moddata,
                dataType: "json",
                success: function (result) {
                    alert("Model File Deleted Successfully");
                },
                error: function (result) {
                    alert("Error in deleting File");
                }
            });
        }
            else {}

        });

        //$("#btnGetData").click(function (e)
        function btnGetData()
        {

            if((_URN_ == "" || _URN_ == undefined || _URN_ == null) || (Version_No == "" || Version_No == undefined || Version_No == null))
            {
            alert("Please select model");
        e.preventDefault();
    }
            else {
                if (confirm("Do you really want to Dextract. It will take some time?")) {
                    var DextractText="Dextracting "+DextractFileName;


        var afterDot = DextractFileName.substr(DextractFileName.lastIndexOf('.') + 1);
        console.log(afterDot);

        document.getElementById("h4msg").innerHTML = DextractText;

        $("#DextractModal").modal('show');
        progress();
                    var moddata = JSON.stringify({
            'ModelData': _URN_,
        'VersionNo': Version_No
    });

    if(afterDot=="nwd ")
                    {
            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("SaveModelDataNwd", "Home")',
                data: moddata,
                dataType: "json",
                success: function (result) {
                    $("#DextractModal").modal('hide');
                    $("#treeviewmodel li span[id='" + _URN_ + "']").remove();
                    $("#treeview span[id='" + _URN_ + "']").remove();
                    $("#prjname  span[id='" + _URN_ + "']").remove();
                },
                error: function (result) {
                    alert("Select Model or Try After Some Time.");
                }
            });
        }
        else
                    {
            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("SaveModelData", "Home")',
                data: moddata,
                dataType: "json",
                success: function (result) {
                    $("#DextractModal").modal('hide');
                    $("#treeviewmodel li span[id='" + _URN_ + "']").remove();
                    $("#treeview span[id='" + _URN_ + "']").remove();
                    $("#prjname  span[id='" + _URN_ + "']").remove();
                },
                error: function (result) {
                    alert("Select Model or Try After Some Time.");
                }
            });
        }
    }
}
}
//);

        $("#btnZoom").click(function () {
            viewer.clearSelection();
        viewer.fitToView();

    });

        function GetCompanyLogo(){
            $.ajax({
                type: "POST",
                contentType: 'application/json',
                url: '@Url.Action("GetCompanyLogo", "Inspire")',
                dataType: "json",
                // data: value,
                success: function (result) {
                    var path = "@Url.Content("~")" + "Content/" + result;
                    //console.log(result);
                    //console.log(path);
                    var path1 = "~/content/" + path;
                    //console.log(path1);
                    if (result != "")
                        $("#CompanyLogo").attr('src', path);


                },
                error: function (result) {
                    // alert(result);

                }
            });
        }

