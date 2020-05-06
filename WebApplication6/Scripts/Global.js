var app = angular.module("MyApp", []);

app.controller('myCtrl', ['$scope', function ($scope) {

    $scope.uploadedCount = 0;
    $scope.TotalParts = 0;
    $scope.starttime = 0;
    $scope.endtime = 0;

    $scope.myFunc = function () {
        $scope.UploadFile($('#uploadFile')[0].files);
    };

    $scope.UploadFile = function (TargetFile) {
        var startdate = new Date();
        starttime = startdate.getTime();

        var FileChunk = [];
        var file = TargetFile[0];
        var MaxFileSizeMB = 1;
        var BufferChunkSize = 1048575;// mulpiple of 3

        var ReadBuffer_Size = 1024;
        var FileStreamPos = 0;
        var EndPos = BufferChunkSize;
        var Size = file.size;

        while (FileStreamPos < Size) {
            FileChunk.push(file.slice(FileStreamPos, EndPos));
            FileStreamPos = EndPos;
            EndPos = FileStreamPos + BufferChunkSize;
        }
        TotalParts = FileChunk.length;
        var PartCount = 0;

        uploadedCount = 0;
        while (chunk = FileChunk.shift()) {
            PartCount++;
            var FilePartName = file.name + ".part_" + PartCount + "." + TotalParts;
            $scope.UploadFileChunk(chunk, FilePartName);
        }
    }
    
    $scope.UploadFileChunk = function (Chunk, FileName) {
        var FD = new FormData();
        FD.append('file', Chunk, FileName);
        $.ajax({
            type: "POST",
            url: '/Home/UploadFile',
            contentType: false,
            processData: false,
            data: FD,
            success: function (data) {
                console.log(FileName + ": success");
                uploadedCount++;
                if (uploadedCount == TotalParts) {
                    var enddate = new Date();
                    endtime = enddate.getTime();
                    console.log("Total time: " + (endtime - starttime));
                    alert("Total time: " + (endtime - starttime) + "ms");
                }
            }
        });
    }
    


}]);