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
      isUpvoted: this.props.isUpvoted
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
    result = result.substring(1, result.length - 1);
    var split = result.split(":");
    if (split[0] != "error") {
      return JSON.parse(result.split("\\").join(""));
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
              <p className="modal-displayname">{this.props.displayName}</p>
              <p className="modal-label">{this.props.username}</p>
            </div>
          </Link>
          <hr className="modal-split" />
          <p className="modal-body">{this.props.body}</p>
          <div className="modal-social-icon-container">
            <img
              onClick={this.upvoteClick}
              className="social-icon"
              src={
                this.state.isUpvoted
                  ? "/Assets/upvote-filled.svg"
                  : "/Assets/upvote.svg"
              }
            />
          </div>
          <hr className="modal-split" />
          <FieldInput
            postId={this.props.postId}
            displayName={this.props.displayName}
            username={this.props.username}
            addFunction={this.addComment}
            profilepic={this.props.currentUserProfilepic}
          />
          <CommentsList comments={this.state.comments} />
        </div>
      </div>
    );
  }

  upvoteClick() {
    this.props.upvoteFunction();
    this.setState(prevState => ({
      isUpvoted: !prevState.isUpvoted
    }));
  }
  addComment(username, body, commentId) {
    const commentToAdd = {
      Body: body,
      Id: commentId,
      PublishDate: new Date(),
      PublisherId: username
    };
    this.setState(prevState => ({
      comments: prevState.comments.concat(commentToAdd)
    }));
  }
}
export default Modal;
