import React from "react";
import "../css/Comments.css";
import { getUser } from "../GlobalRequests";
import { Link } from "react-router-dom";

export class Comment extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = { isUpvoted: this.props.isUpvoted };
    this.upvoteClick = this.upvoteClick.bind(this);
  }
  render() {
    return (
      <div className="comment-container">
        <hr className="comment-split" />
        <Link to={`/profile/${this.props.username}`}>
          <img className="comment-profile-pic" src={this.props.profilepic} />
        </Link>
        <div className="comment-body-container">
          <Link to={`/profile/${this.props.username}`}>
            <p className="comment-displayname">{this.props.displayName}</p>
            <p className="comment-username">{this.props.username}</p>
          </Link>
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
}

export class CommentsList extends React.PureComponent {
  constructor(props) {
    super(props);
  }
  render() {
    console.log(this.props.comments);

    return (
      <div>
        <ul className="comments-list-list">
          {this.props.comments.map(item => {
            var user = getUser(item.PublisherId);
            if (user.length == 1) {
              console.log("error:" + user, item.PublisherId);
            }
            return (
              <li
                className="comments-list-item"
                key={item.PublisherId + item.Body}
              >
                <Comment
                  profilepic={user.ProfilePic}
                  displayName={user.DisplayName}
                  username={item.PublisherId}
                  body={item.Body}
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
