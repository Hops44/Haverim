export function POST(url, payload) {
    var client = new XMLHttpRequest();

    client.open("POST", url, false);
    client.setRequestHeader("Content-Type", "application/json");
    client.send(payload);
    return client.responseText;
}

export function GET(url) {
    var client = new XMLHttpRequest();

    client.open("GET", url, false);
    client.setRequestHeader("Content-Type", "application/json");
    client.send();
    return client.responseText;
}