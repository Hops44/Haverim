import React from "react";
import Modal from "./Modal";
import { QuickReply } from "./QuickReply";
import "../css/Post.css";
import { POST } from "../RestMethods";
import { Link } from "react-router-dom";

class Post extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = {
      isUpvoted: this.props.isUpvoted ? this.props.isUpvoted : false,
      modalVisibility: false,
      quickReplyVisibility: false,
      upvoteCount: this.props.upvoteCount,
      commentCount: this.props.comments ? this.props.comments.length : 0,
      currentUnixTime: Date.now() / 1000
    };
    this.upvoteClick = this.upvoteClick.bind(this);
    this.openPostModal = this.openPostModal.bind(this);
    this.openQuickReply = this.openQuickReply.bind(this);
    this.closeQuickReply = this.closeQuickReply.bind(this);
    this.formatTime = this.formatTime.bind(this);
    this.generateText = this.generateText.bind(this);
    this.incrementCommentCount = this.incrementCommentCount.bind(this);
    this.getModal = this.getModal.bind(this);
    this.targetUserProfilePageUrl =
      this.props.username == this.props.currentUsername
        ? "/profile"
        : `/profile/${this.props.username}`;
  }

  incrementCommentCount() {
    this.setState(prevState => ({
      commentCount: prevState.commentCount + 1
    }));
  }
  openQuickReply() {
    this.setState({ quickReplyVisibility: true });
  }
  closeQuickReply() {
    this.setState({ quickReplyVisibility: false });
  }
  upvoteClick() {
    var result = POST(
      `/api/posts/${this.state.isUpvoted ? "RemoveUpvoteFromPost" : "UpvotePost"}`,
      JSON.stringify({
        Token: sessionStorage.getItem("jwtkey"),
        PostId: this.props.postId
      })
    );
    console.log(result);

    this.setState(prevState => ({
      upvoteCount: prevState.isUpvoted ? prevState.upvoteCount - 1 : prevState.upvoteCount + 1,
      isUpvoted: !prevState.isUpvoted
    }));
  }
  openPostModal() {
    if (this.state.modalVisibility === true) {
      this.props.scrollRef.current.style.overflowY = "scroll";
    }
    this.setState(prevState => ({
      modalVisibility: !prevState.modalVisibility
    }));
  }
  formatTime() {
    const now = this.state.currentUnixTime;
    const diff = Math.floor(now - this.props.unixTime);

    if (diff < 60) return "less than a minute ago";
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
  generateText() {
    var text = this.props.extraInfo.user.displayName + " ";

    function generateActionType(num) {
      switch (num) {
        case 0:
          return "Replied";
        case 1:
          return "Posted";
        case 2:
          return "Upvoted";
      }
    }
    var types = this.props.extraInfo.type
      .toString()
      .split("")
      .map(val => Number(val));
    types.sort((a, b) => a - b);

    switch (types.length) {
      case 1:
        text += generateActionType(types[0]);
        break;
      case 2:
        text += generateActionType(types[0]) + " and " + generateActionType(types[1]);
        break;
      case 3:
        text +=
          generateActionType(types[1]) +
          " , " +
          generateActionType(types[0]) +
          " and " +
          generateActionType(types[2]);
        break;
    }
    return text;
  }
  getModal() {
    this.props.scrollRef.current.style.overflow = "hidden";

    return (
      <div
        className="modal-container-top"
        style={{
          top: this.props.scrollRef.current.scrollTop.toString() + "px"
        }}
      >
        <Modal
          upvoteCount={this.state.upvoteCount}
          displayName={this.props.displayName}
          username={this.props.username}
          profilepic={this.props.profilepic}
          body={this.props.body}
          closeFunction={this.openPostModal}
          isUpvoted={this.state.isUpvoted}
          upvoteFunction={this.upvoteClick}
          comments={this.props.comments}
          postId={this.props.postId}
          currentUsername={this.props.currentUsername}
          currentUserProfilepic={this.props.currentUserProfilepic}
          incrementCommentCount={this.incrementCommentCount}
        />
      </div>
    );
  }

  render() {
    return (
      <React.Fragment>
        {this.state.modalVisibility && this.getModal()}
        {this.props.extraInfo && (
          <Link
            to={
              this.props.extraInfo.user.username == this.props.currentUsername
                ? "/profile"
                : `/profile/${this.props.extraInfo.user.username}`
            }
          >
            <p className="post-other-user-info noselect">{this.generateText()}</p>
          </Link>
        )}
        <div className="post-container">
          <div className="post-user-container noselect">
            <Link className="Link" to={this.targetUserProfilePageUrl}>
              <img className="post-profile-pic ProfilePicture" src={this.props.profilepic} />
            </Link>
            <div className="post-user-text-container">
              <Link className="Link" to={this.targetUserProfilePageUrl}>
                <p className="post-displayname">{this.props.displayName}</p>
                <p className="post-username">{this.props.username}</p>
              </Link>
              <p style={{ cursor: "default" }} className="post-time">
                {this.formatTime(this.props.unixTime)}
              </p>
            </div>
          </div>
          <p onClick={this.openPostModal} className="post-body noselect">
            {this.props.body}
          </p>
          <div className="post-social-icons">
            <div className="post-upvote-count-container">
              <p className="post-upvote-count noselect">{this.state.upvoteCount}</p>
            </div>
            <img
              onClick={this.upvoteClick}
              className="social-icon"
              src={this.state.isUpvoted ? "/Assets/upvote-filled.svg" : "/Assets/upvote.svg"}
            />
            <div className="post-upvote-count-container">
              <p className="post-upvote-count noselect">{this.state.commentCount}</p>
            </div>
            <img onClick={this.openPostModal} className="social-icon" src="/Assets/reply.svg" />
            <img
              onClick={this.openQuickReply}
              className="social-icon"
              src="/Assets/quick-reply.svg"
            />
          </div>
        </div>

        {this.state.quickReplyVisibility && (
          <QuickReply
            profilepic={this.props.currentUserProfilepic}
            closeFunction={this.closeQuickReply}
            postId={this.props.postId}
          />
        )}
      </React.Fragment>
    );
  }
}
export default Post;
