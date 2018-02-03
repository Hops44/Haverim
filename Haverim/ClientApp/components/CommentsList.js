import React from "react";
import "../css/Comments.css";

export class Comment extends React.Component {
  constructor(props) {
    super(props);
    this.state = { isUpvoted: this.props.isUpvoted };
    this.upvoteClick = this.upvoteClick.bind(this);
    this.redirectToUserPage = this.redirectToUserPage.bind(this);
  }
  render() {
    return (
      <div className="comment-container">
        <hr className="comment-split" />
        <img
          onClick={this.redirectToUserPage}
          className="comment-profile-pic"
          src={this.props.profilepic}
        />
        <div className="comment-body-container">
          <p onClick={this.redirectToUserPage} className="comment-displayname">
            {this.props.displayName}
          </p>
          <p onClick={this.redirectToUserPage} className="comment-username">
            {this.props.username}
          </p>
          <p className="comment-body">{this.props.body}</p>
        </div>
        <img
          onClick={this.upvoteClick}
          className="comment-upvote"
          src={
            this.state.isUpvoted
              ? "/Assets/upvote-filled.svg"
              : "/Assets/upvote.svg"
          }
        />
        <hr className="comment-split" />
      </div>
    );
  }
  upvoteClick() {
    this.setState(prevState => ({
      isUpvoted: !prevState.isUpvoted
    }));
  }
  redirectToUserPage() {
    console.log("Redirect To ->", this.props.username);
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
          {this.props.comments.map(item => (
            <li className="comments-list-item" key={item.key}>
              {item}
            </li>
          ))}
        </ul>
      </div>
    );
  }
}

//export Comment;
