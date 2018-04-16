import { GET, POST, GETAsync, POSTAsync } from "./RestMethods";
import React from "react";
import Post from "./components/Post";

export function getUser(id) {
  var result = GET("/api/users/getuser/" + id);
  var split = result.split(":");
  if (split[0] == "error") {
    return split[1];
  }
  var user = result;

  return JSON.parse(user);
}

export function getUserAsync(id) {
  return new Promise((resolve, reject) => {
    GETAsync("/api/users/getuser/" + id, function(result) {
      var split = result.split(":");
      if (split[0] == "error") {
        resolve(split[1]);
      }
      var user = result;

      resolve(JSON.parse(user));
    });
  });
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

export function getUserFeed(token, index, context) {
  var body = {
    Token: token,
    index: index
  };
  POSTAsync("/api/posts/GetPostFeed", JSON.stringify(body), function(result) {
    var hasError = result.split(":")[0] == '"error';
    if (hasError) {
      context.setState({
        finishedLoading: true,
        loadingMore: false,
        canLoadMore: false
      });
      return;
    }

    var username = context.props.currentUser.username;
    var originElement = context;

    var posts = [];
    var postsResult = JSON.parse(result);
    var postsLength = postsResult.length;
    postsResult.map(function(value) {
      var u = getUserAsync(value.publisherId).then(user => {
        if (user.length == 1) {
          return;
        }
        posts.push(
          <Post
            scrollRef={context.props.scrollRef}
            currentUserProfilepic={context.props.currentUser.profilePic}
            currentUsername={context.props.currentUser.username}
            postId={value.id}
            displayName={user.displayName}
            profilepic={user.profilePic}
            username={value.publisherId}
            body={value.body}
            upvoteCount={value.upvotedUsers.length}
            isUpvoted={value.upvotedUsers.includes(username)}
            unixTime={new Date(value.publishDate).getTime() / 1000}
            comments={value.comments}
            key={value.id}
          />
        );
      });
    });
    var interval = setInterval(function() {
      if (posts != undefined && posts.length == postsLength) {
        clearInterval(interval);
        posts.sort(function(a, b) {
          return b.props.unixTime - a.props.unixTime;
        });
        context.setState(prevState => ({
          postList: prevState.postList.concat(posts),
          finishedLoading: true,
          loadingMore: false,
          canLoadMore: posts.length == 10
        }));
      }
    }, 20);
  });
}
