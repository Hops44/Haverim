import { GET, POST } from "./RestMethods";

export function getUser(id) {
  var result = GET("/api/users/getuser/" + id);
  result = result.substring(1, result.length - 1);
  var split = result.split(":");
  if (split[0] == "error") {
    return split[1];
  }
  var user = result.split("\\").join("");

  return JSON.parse(user);
}

export function getUserFollowers(id, isFollowers) {
  var result = GET(`/api/users/GetUserFollowers/${id}/${isFollowers}`);
  result = result.substring(1, result.length - 1);
  var split = result.split(":");
  if (split[0] == "error") {
    return split[1];
  }
  var user = result.split("\\").join("");

  return JSON.parse(user);
}
