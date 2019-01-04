var delay = 5000;
let _dataBlob;

async function callFaceApi() {

    var apiResult = await fetch(
        '/api/face/analyse',
        {
            method: 'POST',
            headers: {
                "Content-Type": "application/octet-stream",
            },
            body: _dataBlob
        });

    var faceAnalysisResult = await apiResult.text();

    postMessage(faceAnalysisResult);
}



self.onmessage = function (msg) {
    _dataBlob = msg.data.dataBlob;
    setTimeout("callFaceApi()", delay);
}

