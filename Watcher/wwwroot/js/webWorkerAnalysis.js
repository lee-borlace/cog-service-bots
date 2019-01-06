var delay = 10000;
let _dataBlob;

async function analyse() {

    var apiResult = await fetch(
        '/api/imageAnalysis/analyse',
        {
            method: 'POST',
            headers: {
                "Content-Type": "application/octet-stream",
            },
            body: _dataBlob
        });

    var analysisResult = await apiResult.json();

    postMessage(analysisResult);
}



self.onmessage = function (msg) {
    _dataBlob = msg.data.dataBlob;
    setTimeout("analyse()", delay);
}

