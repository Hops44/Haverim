import { GET, POST } from "./RestMethods";

export function getUser(id) {
  var result = GET("/api/users/getuser/" + id);
  var split = result.split(":");
  if (split[0] == "error") {
    return split[1];
  }
  var user = result;

  return JSON.parse(user);
}

export function getUserFollowers(id, isFollowers) {
  var result = GET(`/api/users/GetUserFollowers/${id}/${isFollowers}/true/0`);
  var split = result.split(":");
  if (split[0] == "error") {
    return split[1];
  }
  var user = result;

  return JSON.parse(user);
}
