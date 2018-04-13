import React from "react";
import { CommentsList, Comment } from "./CommentsList";
import { FieldInput } from "./FieldInput";
import "../css/Modal.css";
import { POST } from "../RestMethods";
import { Link } from "react-router-dom";

class Modal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      comments: this.getComments(),
      modalWillClose: false,
      isUpvoted: this.props.isUpvoted,
      upvoteCount: this.props.upvoteCount
    };
    this.getComments();
    this.upvoteClick = this.upvoteClick.bind(this);
    this.addComment = this.addComment.bind(this);
    this.getComments = this.getComments.bind(this);
  }
  getComments() {
    var result = POST(
      "/api/posts/GetCommentsFromPost",
      JSON.stringify({
        Token: sessionStorage.getItem("jwtkey"),
        PostId: this.props.postId
      })
    );
    var split = result.split(":");
    if (split[0] != "error") {
      return JSON.parse(result);
    }
    console.log(result, sessionStorage.getItem("jwtkey"), this.props.postId);
    return [];
  }
  render() {
    //(e == 27 ? this.closeFunction : undefined)
    return (
      <div>
        <div onClick={this.props.closeFunction} className="modal-background" />
        <div className="modal-container">
          <Link
            to={`profile/${this.props.username}`}
            className="modal-user-container"
          >
            <img className="modal-profile-image" src={this.props.profilepic} />
            <div className="modal-user-info-container">
              <p className="modal-displayname Link">{this.props.displayName}</p>
              <p className="modal-label Link">{this.props.username}</p>
            </div>
          </Link>
          <hr className="modal-split" />
          <p className="modal-body">{this.props.body}</p>
          <div className="modal-social-icon-container">
            <div className="modal-upvote-icon-container">
              <img
                onClick={this.upvoteClick}
                className="social-icon"
                src={
                  this.state.isUpvoted
                    ? "/Assets/upvote-filled.svg"
                    : "/Assets/upvote.svg"
                }
                style={{ marginBottom: "-3px" }}
              />
            </div>
            <div className="modal-upvote-count-container">
              <p className="modal-upvote-count noselect">
                {this.state.upvoteCount}
              </p>
            </div>
          </div>
          <hr className="modal-split" />
          <FieldInput
            postId={this.props.postId}
            displayName={this.props.displayName}
            username={this.props.username}
            addFunction={this.addComment}
            profilepic={this.props.currentUserProfilepic}
          />
          <CommentsList
            currentUsername={this.props.currentUsername}
            postId={this.props.postId}
            comments={this.state.comments}
          />
        </div>
      </div>
    );
  }

  upvoteClick() {
    this.props.upvoteFunction();
    this.setState(prevState => ({
      upvoteCount: prevState.isUpvoted
        ? prevState.upvoteCount - 1
        : prevState.upvoteCount + 1,
      isUpvoted: !prevState.isUpvoted
    }));
  }
  addComment(username, body, commentId) {
    this.props.incrementCommentCount();
    const commentToAdd = {
      body: body,
      id: commentId,
      publishDate: new Date(),
      publisherId: username
    };
    this.setState(prevState => ({
      comments: prevState.comments.concat(commentToAdd)
    }));
  }
}
export default Modal;
