export function POST(url, payload) {
  var xhr = new XMLHttpRequest();

  xhr.open("POST", url, false);
  xhr.setRequestHeader("Content-Type", "application/json");
  xhr.send(payload);
  return xhr.responseText;
}

export function POSTAsync(url, payload, done) {
  var xhr = new XMLHttpRequest();
  xhr.open("POST", url, true);
  xhr.setRequestHeader("Content-Type", "application/json");

  xhr.onload = function(e) {
    done(xhr.responseText);
  };

  xhr.send(payload);
}

export function GET(url) {
  var client = new XMLHttpRequest();

  client.open("GET", url, false);
  client.setRequestHeader("Content-Type", "application/json");
  client.send();
  return client.responseText;
}

export function GETAsync(url, done) {
  var xhr = new XMLHttpRequest();
  xhr.open("GET", url, true);
  xhr.onload = function(e) {
    done(xhr.responseText);
  };

  xhr.send(null);
}
