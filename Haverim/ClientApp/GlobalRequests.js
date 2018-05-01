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

export function getUserActivityFeed(targetUser, token, index, context) {
  function getExistingTypesFromActivityFeed(parsed, id) {
    let tags = [];
    for (const item of parsed) {
      if (item.post.id == id && !tags.includes(item.activity.type)) {
        tags.push(item.activity.type);
      }
    }

    return tags.join("");
  }

  function removeDups(list) {
    var newList = [];
    for (const item of list) {
      var found = false;
      for (const newListItem of newList) {
        if (newListItem.key == item.key) {
          found = true;
          break;
        }
      }
      if (!found) {
        newList.push(item);
      }
    }
    return newList;
  }

  var body = {
    Token: token,
    index: index,
    TargetUser: targetUser
  };
  POSTAsync("/api/posts/GetUserActivityFeed", JSON.stringify(body), result => {
    var hasError = result.split(":")[0] == '"error';
    if (hasError) {
      context.setState({
        finishedLoading: true,
        loadingMore: false,
        canLoadMore: false
      });
      console.log(result);
      return;
    }

    let parsed = JSON.parse(result);
    var feed = [];
    for (const feedItem of parsed) {
      getUserAsync(feedItem.post.publisherId).then(user => {
        if (user.length == 1) {
          return;
        }
        feed.push(
          <Post
            extraInfo={{
              user: {
                displayName: context.props.targetUser.displayName,
                username: context.props.targetUser.username
              },
              type: getExistingTypesFromActivityFeed(parsed, feedItem.post.id)
            }}
            activityUnix={feedItem.activity.date}
            scrollRef={context.props.scrollRef}
            currentUserProfilepic={context.props.currentUser.profilePic}
            currentUsername={context.props.currentUser.username}
            postId={feedItem.post.id}
            displayName={user.displayName}
            profilepic={user.profilePic}
            username={feedItem.post.publisherId}
            body={feedItem.post.body}
            upvoteCount={feedItem.post.upvotedUsers.length}
            isUpvoted={feedItem.post.upvotedUsers.includes(context.props.currentUser.username)}
            unixTime={new Date(feedItem.post.publishDate).getTime() / 1000}
            comments={feedItem.post.comments}
            key={feedItem.post.id}
          />
        );
      });
    }
    var interval = setInterval(function() {
      if (feed != null && feed.length == parsed.length) {
        clearInterval(interval);
        feed = feed.concat(context.state.postList);
        feed.sort((a, b) => b.props.activityUnix - a.props.activityUnix);
        context.setState(prevState => ({
          postList: removeDups(feed),
          finishedLoading: true,
          loadingMore: false,
          canLoadMore: feed.length == 10
        }));
      }
    }, 20);
  });
}

export function uploadImage(base64Data, type) {
  return new Promise((resolve, reject) => {
    POSTAsync(
      "/api/Users/ChangeUserPictrue",
      JSON.stringify({
        Token: sessionStorage.getItem("jwtkey"),
        ImageBase64Data: base64Data,
        Type: type
      }),
      function(result) {
        resolve(result);
      }
    );
  });
}
