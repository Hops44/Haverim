import React from "react";
import "../css/UserProfile.css";
import { GET, GETAsync } from "../RestMethods";
import { getUser } from "../GlobalRequests";
import { Link } from "react-router-dom";

class Profile extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = {
      modalOpen: false,
      lastRequestFollowers: null
    };
    this.followersModal = this.followersModal.bind(this);
    this.closeModal = this.closeModal.bind(this);
  }
  followersModal(followers) {
    this.setState({
      modalOpen: true,
      lastRequestFollowers: followers
    });
  }
  closeModal() {
    this.setState({ modalOpen: false });
  }
  render() {
    return (
      <div className="profile-container">
        {this.state.modalOpen && (
          <FollowersModal
            close={() => {
              this.closeModal();
              this.props.toggleModal(false);
            }}
            followers={this.state.lastRequestFollowers}
            username={this.props.username}
            isFollowers={this.state.lastRequestFollowers}
          />
        )}
        <img className="profile-image" src={this.props.profilepic} />
        <Link className="username-label noselect Link" to={"/profile"}>
          {this.props.displayName}
        </Link>
        <div
          onClick={() => {
            this.followersModal(true);
            this.props.toggleModal(true);
          }}
          className="followers-container noselect"
        >
          <p className="follow-label">Followers</p>
          <p className="follow-count-label">{this.props.followers}</p>
        </div>
        <div
          onClick={() => {
            this.followersModal(false);
            this.props.toggleModal(true);
          }}
          className="following-container noselect"
        >
          <p className="follow-label">Following</p>
          <p className="follow-count-label">{this.props.following}</p>
        </div>
      </div>
    );
  }
}
export default Profile;

function fakeGetUsers(index) {
  var length = 0;
  var arr = [];
  if (index + 10 >= length) {
    if (index >= length) {
      return false;
    }
    for (let i = 0; i < length - index; i++) {
      arr.push({
        profilePic:
          "https://www.ienglishstatus.com/wp-content/uploads/2018/04/Anonymous-Whatsapp-profile-picture.jpg",
        username: "ranuser" + (i + index),
        displayName: "Random User"
      });
    }
  } else {
    var count = 0;
    for (let i = 0; i < 10; i++) {
      arr.push({
        profilePic:
          "https://www.ienglishstatus.com/wp-content/uploads/2018/04/Anonymous-Whatsapp-profile-picture.jpg",
        username: "ranuser" + (i + index),
        displayName: "Random User"
      });
    }
  }
  return new Promise((resolve, reject) =>
    setTimeout(function() {
      resolve(arr);
    }, 1000)
  );
}

export class FollowersModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      loadedUsers: [],
      isLoading: true,
      canLoadMore: true,
      index: 0
    };
    this.getUsers = this.getUsers.bind(this);
  }
  getUsers() {
    this.setState({
      isLoading: true
    });

    var result = GETAsync(
      `/api/users/GetUserFollowers/${this.props.username}/${
        this.props.isFollowers
      }/false/${this.state.index}`,
      function(result) {
        var users = JSON.parse(result);
        this.setState(prevState => ({
          index: prevState.index + users.length,
          canLoadMore: users.length == 10,
          isLoading: false,
          loadedUsers: prevState.loadedUsers.concat(users)
        }));
      }.bind(this)
    );
  }

  componentWillMount() {
    this.getUsers();
  }
  render() {
    return (
      <div>
        <div
          onClick={this.props.close}
          className="followers-modal-background"
        />
        <div className="followers-modal-body Scrollbar">
          <h2 className="followers-modal-header">
            {this.props.followers ? "Followers" : "Following"}
          </h2>
          <div className="followers-modal-split" />
          <ul ref={"followList"} className="followers-modal-list">
            {this.state.loadedUsers.map((user, index) => (
              <FollowersModalListItem
                key={index}
                pic={user.profilePic}
                username={user.username}
                displayName={user.displayName}
              />
            ))}
            {this.state.isLoading && (
              <p className="followers-modal-loading-header">Loading</p>
            )}
            {this.state.canLoadMore &&
              !this.state.isLoading && (
                <button
                  className="followers-modal-load-button"
                  onClick={function() {
                    this.getUsers();
                    console.log(this.refs.followList);
                  }.bind(this)}
                >
                  Load More
                </button>
              )}
          </ul>
        </div>
      </div>
    );
  }
}
class FollowersModalListItem extends React.PureComponent {
  constructor(props) {
    super(props);
  }
  render() {
    return (
      <Link
        to={`/profile/${this.props.username}`}
        className="followers-modal-list-item"
        style={{ textDecoration: "none" }}
      >
        <img
          className="followers-modal-list-item-profilepic ProfilePicture"
          src={this.props.pic}
        />
        <div className="followers-modal-list-item-user-container">
          <p className="followers-modal-list-item-displayname Link">
            {this.props.displayName}
          </p>
          <p className="followers-modal-list-item-username Link">
            {this.props.username}
          </p>
        </div>
      </Link>
    );
  }
}
