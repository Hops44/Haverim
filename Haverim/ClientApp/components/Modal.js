import React from "react";
import { CommentsList, Comment } from "./CommentsList";
import { FieldInput } from "./FieldInput";
import "../css/Modal.css";

class Modal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      comments: [],
      modalWillClose: false,
      isUpvoted: this.props.isUpvoted
    };
    this.upvoteClick = this.upvoteClick.bind(this);
    this.addComment = this.addComment.bind(this);
  }

  render() {
    //(e == 27 ? this.closeFunction : undefined)
    return (
      <div>
        <div onClick={this.props.closeFunction} className="modal-background" />
        <div className="modal-container">
          <div className="modal-user-container">
            <img className="modal-profile-image" src={this.props.profilepic} />
            <div className="modal-user-info-container">
              <p className="modal-displayname">{this.props.displayName}</p>
              <p className="modal-label">{this.props.username}</p>
            </div>
          </div>
          <hr className="modal-split" />
          <p className="modal-body">{this.props.body}</p>
          <img
            onClick={this.upvoteClick}
            className="modal-social-icon"
            src={
              this.state.isUpvoted
                ? "/Assets/upvote-filled.svg"
                : "/Assets/upvote.svg"
            }
          />
          <hr className="modal-split" />
          <FieldInput
            displayName={this.props.displayName}
            username={this.props.username}
            addFunction={this.addComment}
            profilepic={this.props.profilepic}
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
  addComment(profilepic, displayName, username, body, key) {
    const commentToAdd = (
      <Comment
        profilepic={profilepic}
        displayName={displayName}
        username={username}
        body={body}
        key={key}
      />
    );

    this.setState(prevState => ({
      comments: prevState.comments.concat(commentToAdd)
    }));
  }
}
export default Modal;
