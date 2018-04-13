import React from "react";
import Post from "./Post";
import { FieldInput } from "./FieldInput";
import "../css/PostFeed.css";
import { POST, GET } from "../RestMethods";
import { getUser } from "../GlobalRequests";

export class PostFeed extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      postList: this.props.posts ? this.props.posts : [],
      finishedLoading: false
    };
    this.addPostToFeed = this.addPostToFeed.bind(this);
    this.formatTime = this.formatTime.bind(this);
    this.requestPostsFromServer = this.requestPostsFromServer.bind(this);
    /*
        1. Start animation with a timer
        2. fetch feed from server
        3. if feed fetch takes more than 20 seconds stop animation and display an error message
        4. if the fetch action return in less than 20 seconds , stop the animation and display posts
        */
  }

  requestPostsFromServer(key) {
    var body = {
      Token: key,
      index: 0
    };
    var result = POST("/api/posts/GetPostFeed", JSON.stringify(body));
    var hasError = result.split(":")[0] == "error";
    if (hasError) {
      console.log(result);
      this.setState({ finishedLoading: true });
      return;
    }

    var username = this.props.currentUser.username;
    var originElement = this;

    var posts = JSON.parse(result).map(
      function(value) {
        var user = getUser(value.publisherId);
        if (user.length == 1) {
          return;
        }
        return (
          <Post
            currentUserProfilepic={this.props.currentUser.profilePic}
            currentUsername={this.props.currentUser.username}
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
      }.bind(this)
    );
    this.setState({ postList: posts, finishedLoading: true });
  }

  addPostToFeed(profilepic, displayName, username, body, time, key) {
    const postToAdd = (
      <Post
        profilepic={profilepic}
        displayName={displayName}
        username={username}
        body={body}
        unixTime={time}
        key={key}
        upvoteCount={0}
      />
    );
    this.setState(prevState => ({
      postList: [postToAdd].concat(prevState.postList.slice())
    }));
  }

  formatTime(unix) {
    const now = Date.now() / 1000;
    const diff = Math.floor(now - unix);

    if (diff < 60) return Math.floor(diff) + "s";
    else if (diff < 3600) return Math.floor(diff / 60) + "m";
    else if (diff < 86400) return Math.floor(diff / 3600) + "h";
    else if (diff < 2629743) return Math.floor(diff / 86400) + "d";
    else if (diff < 31556926) {
      var date = new Date(this.props.unixTime * 1000);
      return date.getDate() + " " + this.formatMonth(date.getMonth());
    } else {
      var date = new Date(this.props.unixTime * 1000);
      return (
        date.getDate() +
        " " +
        this.formatMonth(date.getMonth()) +
        " " +
        date
          .getFullYear()
          .toString()
          .substring(2, 4)
      );
    }
  }
  formatMonth(monthNum) {
    var monthNames = [
      "January",
      "February",
      "March",
      "April",
      "May",
      "June",
      "July",
      "August",
      "September",
      "October",
      "November",
      "December"
    ];
    return monthNames[monthNum];
  }
  logKey(e) {
    console.log(e);
  }
  componentWillMount() {
    this.requestPostsFromServer(sessionStorage.getItem("jwtkey"));
  }

  render() {
    return (
      <div>
        {!this.props.noFieldInput && (
          <FieldInput
            username={this.props.currentUser.username}
            displayName={this.props.currentUser.displayName}
            addFunction={this.addPostToFeed}
            profilepic={this.props.currentUser.profilePic}
            isPost={true}
          />
        )}
        <ul className="feed-ul">
          {this.state.finishedLoading ? (
            this.state.postList.length > 0 ? (
              this.state.postList.map(item => (
                <li key={item.key} className="feed-post-item">
                  {item}
                </li>
              ))
            ) : (
              <h1
                style={{
                  textAlign: "center"
                }}
              >
                No Aviable Posts
              </h1>
            )
          ) : (
            <img
              className="load-animation"
              src="http://www.iceflowstudios.com/v3/wp-content/uploads/2013/01/LoadingCircle_firstani.gif"
            />
          )}
        </ul>
      </div>
    );
  }
}
//*"/Assets/load-animation.svg"*/
