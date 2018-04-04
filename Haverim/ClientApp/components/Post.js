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
      currentUnixTime: Date.now() / 1000
    };
    this.upvoteClick = this.upvoteClick.bind(this);
    this.openPostModal = this.openPostModal.bind(this);
    this.openQuickReply = this.openQuickReply.bind(this);
    this.closeQuickReply = this.closeQuickReply.bind(this);
    this.formatTime = this.formatTime.bind(this);
    this.generateText = this.generateText.bind(this);
    console.log('shalom');
  }

  openQuickReply() {
    this.setState({ quickReplyVisibility: true });
  }
  closeQuickReply() {
    this.setState({ quickReplyVisibility: false });
  }
  upvoteClick() {
    var result = POST(
      `/api/posts/${
        this.state.isUpvoted ? "RemoveUpvoteFromPost" : "UpvotePost"
      }`,
      JSON.stringify({
        Token: sessionStorage.getItem("jwtkey"),
        PostId: this.props.postId
      })
    );

    this.setState(prevState => ({
      isUpvoted: !prevState.isUpvoted
    }));
  }
  openPostModal() {
    this.setState(prevState => ({
      modalVisibility: !prevState.modalVisibility
    }));
  }

  formatTime() {
    const now = this.state.currentUnixTime;
    const diff = Math.floor(now - this.props.unixTime);

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
  generateText() {
    var text = this.props.extraInfo.user.displayName + " ";
    switch (this.props.extraInfo.type) {
      case 0:
        text += "Replied";
        break;
      case 1:
        text += "Upvoted";
        break;
      case 2:
        text += "Posted";
    }
    return text;
  }

  render() {
    return (
      <div>
        {this.props.extraInfo && (
          <Link to={`/profile/${this.props.extraInfo.user.username}`}>
            <p className="post-other-user-info noselect">
              {this.generateText()}
            </p>
          </Link>
        )}
        <div className="post-container">
          <div className="post-user-container noselect">
            <Link className="Link" to={`/profile/${this.props.username}`}>
              <img
                className="post-profile-pic ProfilePicture"
                src={this.props.profilepic}
              />
            </Link>
            <div className="post-user-text-container">
              <Link className="Link" to={`/profile/${this.props.username}`}>
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
          {/* <div
            onClick={() => this.setState({ modalVisibility: true })}
            className="post-split"
          >
            <div className="post-split-content" />
          </div> */}
          <div className="post-social-icons">
            <img
              onClick={this.upvoteClick}
              className="social-icon"
              src={
                this.state.isUpvoted
                  ? "/Assets/upvote-filled.svg"
                  : "/Assets/upvote.svg"
              }
            />
            <img
              onClick={this.openQuickReply}
              className="social-icon"
              src="/Assets/quick-reply.svg"
            />
            <img
              onClick={this.openPostModal}
              className="social-icon"
              src="/Assets/reply.svg"
            />
          </div>
        </div>
        {this.state.modalVisibility && (
          <Modal
            displayName={this.props.displayName}
            username={this.props.username}
            profilepic={this.props.profilepic}
            body={this.props.body}
            closeFunction={this.openPostModal}
            isUpvoted={this.state.isUpvoted}
            upvoteFunction={this.upvoteClick}
            comments={this.props.comments}
            postId={this.props.postId}
            currentUserProfilepic={this.props.currentUserProfilepic}
          />
        )}
        {this.state.quickReplyVisibility && (
          <QuickReply
            profilepic={this.props.currentUserProfilepic}
            closeFunction={this.closeQuickReply}
            postId={this.props.postId}
          />
        )}
      </div>
    );
  }
}
export default Post;
