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

export function GETAsync(url, done) {
  var xhr = new XMLHttpRequest();
  xhr.open("GET", url, true);
  xhr.onload = function(e) {
    done(xhr.responseText);
  };

  xhr.send(null);
}
