import React from "react";
import "../css/Comments.css";
import { getUser } from "../GlobalRequests";
import { Link } from "react-router-dom";
import { POST } from "../RestMethods";

export class Comment extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = {
      isUpvoted: this.props.upvotedUsers.includes(this.props.currentUsername),
      upvoteCount: this.props.upvotedUsers != null ? this.props.upvotedUsers.length : 0
    };
    this.upvoteClick = this.upvoteClick.bind(this);
  }
  render() {
    return (
      <div className="comment-container">
        <hr className="comment-split" />
        <Link style={{ gridColumn: 1, gridRow: "1/5" }} to={`/profile/${this.props.username}`}>
          <img className="comment-profile-pic" src={this.props.profilepic} />
        </Link>
        <div className="comment-body-container">
          <Link to={`/profile/${this.props.username}`}>
            <p className="comment-displayname">{this.props.displayName}</p>
            <p className="comment-username">{this.props.username}</p>
          </Link>
          <p className="comment-body">{this.props.body}</p>
        </div>
        <div className="comment-upvote-container">
          <img
            onClick={this.upvoteClick}
            className="comment-upvote"
            src={this.state.isUpvoted ? "/Assets/upvote-filled.svg" : "/Assets/upvote.svg"}
          />
          <p className="comment-upvote-count">{this.state.upvoteCount}</p>
        </div>
        {/* <hr className="comment-split" /> */}
      </div>
    );
  }
  upvoteClick() {
    var commentId = this.props.commentId;
    if (commentId.length == 37) {
      commentId = commentId.substring(0, 36);
    }
    if (this.state.isUpvoted) {
      // REMOVE UPVOTE
      var result = POST(
        "/api/posts/RemoveUpvoteFromComment",
        JSON.stringify({
          Token: sessionStorage.getItem("jwtkey"),
          postId: this.props.postId,
          commentId: commentId
        })
      );
    } else {
      //UPVOTE
      var result = POST(
        "/api/posts/UpvoteComment",
        JSON.stringify({
          Token: sessionStorage.getItem("jwtkey"),
          postId: this.props.postId,
          commentId: commentId
        })
      );
    }
    this.setState(prevState => ({
      upvoteCount: prevState.isUpvoted ? prevState.upvoteCount - 1 : prevState.upvoteCount + 1,
      isUpvoted: !prevState.isUpvoted
    }));
  }
}

export class CommentsList extends React.PureComponent {
  constructor(props) {
    super(props);
  }
  render() {
    return (
      <div>
        <ul className="comments-list-list">
          {this.props.comments.map(item => {
            var user = getUser(item.publisherId);
            if (user.length == 1) {
              console.log("error:" + user, item.publisherId);
            }
            return (
              <li className="comments-list-item" key={item.id}>
                <Comment
                  currentUsername={this.props.currentUsername}
                  profilepic={user.profilePic}
                  displayName={user.displayName}
                  username={item.publisherId}
                  body={item.body}
                  commentId={item.id}
                  upvotedUsers={item.upvotedUsers ? item.upvotedUsers : []}
                  postId={this.props.postId}
                />
              </li>
            );
          })}
        </ul>
      </div>
    );
  }
}

//export Comment;
