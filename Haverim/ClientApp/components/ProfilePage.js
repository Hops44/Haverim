import React from "react";
import Navbar from "./Navbar";
import { PostFeed } from "./PostFeed";
import "../css/ProfilePage.css";
import { getUser, getUserFollowers } from "../GlobalRequests";
import { checkIfLogged } from "./MainPage";
import { Redirect, withRouter } from "react-router";
import { POST } from "../RestMethods";
import { ImageUploadModal } from "./ImageUploadModal";

class ProfilePage extends React.Component {
  constructor(props) {
    super(props);
    var isLogged = checkIfLogged();
    var currentUser = isLogged === false ? false : JSON.parse(isLogged);

    if (this.props.target == undefined) {
      this.targetUser = currentUser;
    } else {
      this.targetUser = getUser(this.props.target);
      if (this.targetUser.length == 1) {
        this.targetUser = "redirect";
      }
    }
    var isFollowing = false;
    if (currentUser !== false && currentUser.username !== this.targetUser.username) {
      var targetFollowers = getUserFollowers(this.targetUser.username, true);

      isFollowing = targetFollowers.includes(currentUser.username);
    }

    this.state = {
      isFollowing: isFollowing,
      currentUser: currentUser,
      modalOpen: false
    };
    this.editable = currentUser.username == this.targetUser.username;

    this.toggleFollow = this.toggleFollow.bind(this);
    this.getBirthDate = this.getDate.bind(this);
    this.userUpdated = this.userUpdated.bind(this);
    this.scrollRef = React.createRef();
  }

  toggleFollow() {
    var url;
    if (this.state.isFollowing) {
      url = "api/users/UnFollowUser";
    } else {
      url = "api/users/FollowUser";
    }
    var result = POST(
      url,
      JSON.stringify({
        Token: sessionStorage.getItem("jwtkey"),
        TargetUser: this.targetUser.username
      })
    );
    console.log(result);
    this.setState(prevState => ({
      isFollowing: !prevState.isFollowing
    }));
  }
  getDate(isJoin) {
    let date = isJoin ? new Date(this.targetUser.joinDate) : new Date(this.targetUser.birthDate);

    if (isJoin) {
      return this.formatMonth(date.getMonth()) + " " + date.getFullYear();
    }
    return this.formatMonth(date.getMonth()) + " " + date.getDate() + " " + date.getFullYear();
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
  userUpdated() {
    var isLogged = checkIfLogged();
    var currentUser = isLogged === false ? false : JSON.parse(isLogged);
    if (this.props.target == undefined) {
      this.targetUser = currentUser;
    } else {
      this.targetUser = getUser(this.props.target);
      if (this.targetUser.length == 1) {
        this.targetUser = "redirect";
      }
    }
    var isFollowing = false;
    if (currentUser !== false && currentUser.username !== this.targetUser.username) {
      var targetFollowers = getUserFollowers(this.targetUser.username, true);

      isFollowing = targetFollowers.includes(currentUser.username);
    }

    this.setState({
      isFollowing: isFollowing,
      currentUser: currentUser,
      modalOpen: false
    });
  }

  render() {
    return (
      <div className="user-profile-container">
        {this.state.modalOpen !== false && (
          <ImageUploadModal
            type={this.state.modalOpen}
            reloadFunction={() => this.userUpdated()}
            closeFunction={() => this.setState({ modalOpen: false })}
          />
        )}
        {this.state.currentUser === false && <Redirect to="/login" />}
        {this.targetUser === "redirect" && <Redirect to="/invalid" />}
        <Navbar profilepic={this.state.currentUser.profilePic} />
        <div ref={this.scrollRef} className="profile-page-contents Scrollbar">
          <div className={"user-profile-info-container"}>
            {this.targetUser.profilePagePicture == null ||
            this.targetUser.profilePagePicture[this.targetUser.profilePagePicture.length - 1] ==
              "=" ? (
              <div className={"user-profile-no-picture-container"}>
                <div
                  onClick={this.editable ? () => this.setState({ modalOpen: 1 }) : null}
                  className={
                    this.editable ? "user-profile-no-picture editable" : "user-profile-no-picture"
                  }
                />
              </div>
            ) : (
              <div
                onClick={() => this.setState({ modalOpen: 1 })}
                className={
                  this.editable
                    ? "user-profile-no-picture-container editable"
                    : "user-profile-no-picture-container"
                }
              >
                <img
                  draggable={false}
                  className="user-profile-back-image"
                  src={this.targetUser.profilePagePicture}
                />
              </div>
            )}

            <div className={"user-profile-main-info"}>
              <p className={"user-profile-displayname"}>{this.targetUser.displayName}</p>
              <img
                onClick={() => this.setState({ modalOpen: 0 })}
                draggable={false}
                className={
                  this.editable
                    ? "user-profile-profilepic editable-filter"
                    : "user-profile-profilepic"
                }
                src={this.targetUser.profilePic}
              />
              <p className={"user-profile-username"}>{this.targetUser.username}</p>
            </div>

            <div className={"user-profile-more-info"}>
              <p className={"user-profile-more-info-join-date"}>{this.getDate(true)}</p>
              <p className={"user-profile-more-info-birth-date"}>{this.getDate(false)}</p>
              {this.targetUser.username != this.state.currentUser.username && (
                <button
                  onClick={this.toggleFollow}
                  className={
                    this.state.isFollowing
                      ? "user-profile-more-info-follow-button"
                      : "not-following-button"
                  }
                >
                  {this.state.isFollowing ? "Following" : "Follow"}
                </button>
              )}
              <p className={"user-profile-more-info-country"}>{this.targetUser.country}</p>
              <p className={"user-profile-more-info-gender"}>
                {this.targetUser.isMale ? "Male" : "Female"}
              </p>
            </div>
          </div>
          <div className="user-profile-post-feed">
            <PostFeed
              scrollRef={this.scrollRef}
              activityFeed={true}
              targetUser={this.targetUser}
              currentUser={this.state.currentUser}
              noFieldInput={true}
            />
          </div>
        </div>
      </div>
    );
  }
}

export default ProfilePage;
