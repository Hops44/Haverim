import React from "react";
import Post from "./Post";
import { FieldInput } from "./FieldInput";
import "../css/PostFeed.css";
import { POST, GET, POSTAsync } from "../RestMethods";
import { getUser, getUserFeed, getUserActivityFeed } from "../GlobalRequests";

export class PostFeed extends React.Component {
  constructor(props) {
    super(props);

    this.state = {
      postList: [],
      finishedLoading: false,
      loadingMore: false,
      canLoadMore: true
    };
    this.addPostToFeed = this.addPostToFeed.bind(this);
    this.formatTime = this.formatTime.bind(this);
    this.requestPostsFromServer = this.requestPostsFromServer.bind(this);
    this.onScrollHandler = this.onScrollHandler.bind(this);
    this.interval = setInterval(this.onScrollHandler, 1);
  }

  onScrollHandler() {
    this.props.scrollRef.current.onscroll = function(e) {
      if (!this.state.loadingMore && this.state.canLoadMore) {
        var windowHeight = window.innerHeight - 47.5;
        var docHeight = e.target.scrollHeight;
        var scrollTop = e.target.scrollTop;

        var trackLength = docHeight - windowHeight;
        var scrollPercentage = scrollTop / trackLength;

        if (scrollPercentage > 0.95) {
          this.setState({
            loadingMore: true
          });
        }
      }
    }.bind(this);
    clearInterval(this.interval);
  }

  componentDidUpdate() {
    if (this.state.loadingMore) {
      this.requestPostsFromServer();
    }
  }

  requestPostsFromServer() {
    if (this.props.activityFeed === true) {
      getUserActivityFeed(
        this.props.targetUser.username,
        sessionStorage.getItem("jwtkey"),
        this.state.postList.length,
        this
      );
    } else {
      getUserFeed(sessionStorage.getItem("jwtkey"), this.state.postList.length, this);
    }
    //this.setState({ postList: posts, finishedLoading: true });
  }

  addPostToFeed(profilepic, displayName, username, body, time, key) {
    console.log(key);
    const postToAdd = (
      <Post
        scrollRef={this.props.scrollRef}
        profilepic={profilepic}
        displayName={displayName}
        username={username}
        body={body}
        unixTime={time}
        postId={key}
        upvoteCount={0}
        comments={[]}
        currentUserProfilepic={this.props.currentUser.profilePic}
        currentUsername={this.props.currentUser.username}
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

  componentWillMount() {
    this.requestPostsFromServer();
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
          {this.state.loadingMore && <h1>Loading More</h1>}
        </ul>
      </div>
    );
  }
}
