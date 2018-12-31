var timeBetweenCalls = 3000;

function callFaceApi() {
    postMessage("event!");
}

setTimeout("callFaceApi()", timeBetweenCalls);