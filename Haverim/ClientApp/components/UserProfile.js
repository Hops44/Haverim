import React from "react";
import "../css/UserProfile.css";
import { GET } from "../RestMethods";
import { getUser } from "../GlobalRequests";
import { Link } from "react-router-dom";

class Profile extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = {
      modalOpen: false,
      followers: [],
      lastRequestFollowers: null
    };
    this.followersModal = this.followersModal.bind(this);
    this.closeModal = this.closeModal.bind(this);
  }
  followersModal(followers) {
    var result = GET(
      `api/users/GetUserFollowers/${this.props.username}/${followers}`
    );
    result = result
      .substring(1, result.length - 1)
      .split("\\")
      .join("");
    if (result.split(":")[0] == "error") {
      console.log(result);
      return;
    }
    result = JSON.parse(result);
    this.setState({
      modalOpen: true,
      followers: result,
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
            close={this.closeModal}
            followers={this.state.lastRequestFollowers}
            users={this.state.followers}
          />
        )}
        <img className="profile-image" src={this.props.profilepic} />
        <p className="username-label noselect">{this.props.displayName}</p>
        <div
          onClick={() => this.followersModal(true)}
          className="followers-container noselect"
        >
          <p className="follow-label">Followers</p>
          <p className="follow-count-label">{this.props.followers}</p>
        </div>
        <div
          onClick={() => this.followersModal(false)}
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

class FollowersModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      max: 10,
      loaded: true,
      users: this.props.users,
      loadedUsers: []
    };
    this.getUsers = this.getUsers.bind(this);
  }
  getUsers() {
    let users = [];
    for (let i = 0; i < 10 && i < this.state.users.length; i++) {
      var result = getUser(this.state.users[i]);
      if (result.length != 1) {
        users.push({
          profilePic: result.ProfilePic,
          username: result.Username,
          displayName: result.DisplayName
        });
      } else {
        console.log("Error:" + result);
      }
    }
    this.setState({ loadedUsers: users, loaded: true });
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
        <div className="followers-modal-body">
          <h2 className="followers-modal-header">
            {this.props.followers ? "Followers" : "Following"}
          </h2>
          <div className="followers-modal-split" />
          <ul className="followers-modal-list">
            {this.state.loaded ? (
              this.state.users.length == 0 ? (
                <h1>No Followers</h1>
              ) : (
                [...Array(10)].map((v, index) => {
                  if (index <= this.state.loadedUsers.length - 1) {
                    return (
                      <FollowersModalListItem
                        key={this.state.loadedUsers[index].username}
                        pic={this.state.loadedUsers[index].profilePic}
                        username={this.state.loadedUsers[index].username}
                        displayName={this.state.loadedUsers[index].displayName}
                      />
                    );
                  }
                })
              )
            ) : (
              <h1>Loading...</h1>
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
