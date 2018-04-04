import React from "react";
import Navbar from "./Navbar";
import { PostFeed } from "./PostFeed";
import "../css/ProfilePage.css";
import { getUser, getUserFollowers } from "../GlobalRequests";
import { checkIfLogged } from "./MainPage";
import { Redirect } from "react-router";

export class ProfilePage extends React.Component {
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
    console.log(this.targetUser);
    var isFollowing = false;
    if (
      currentUser !== false &&
      currentUser.Username !== this.targetUser.Username
    ) {
      var targetFollowers = getUserFollowers(this.targetUser.Username, true);
      if (targetFollowers.length != 1) {
        if (targetFollowers.includes(currentUser.Username)) {
          isFollowing = true;
        }
      }
    }

    this.state = {
      isFollowing: isFollowing,
      currentUser: currentUser
    };

    this.toggleFollow = this.toggleFollow.bind(this);
    this.getBirthDate = this.getDate.bind(this);
  }

  toggleFollow() {
    this.setState(prevState => ({
      isFollowing: !prevState.isFollowing
    }));
  }
  getDate(isJoin) {
    let date = isJoin
      ? new Date(this.targetUser.JoinDate)
      : new Date(this.targetUser.BirthDate);

    if (isJoin) {
      return this.formatMonth(date.getMonth()) + " " + date.getFullYear();
    }
    return (
      this.formatMonth(date.getMonth()) +
      " " +
      date.getDate() +
      " " +
      date.getFullYear()
    );
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

  render() {
    return (
      <div className="user-profile-container">
        {this.state.currentUser === false && <Redirect to="/login" />}
        {this.targetUser === "redirect" && <Redirect to="/invalid" />}
        <Navbar profilepic={this.state.currentUser.ProfilePic} />
        <div className={"user-profile-info-container"}>
          {this.targetUser.ProfilePagePicture == null ? (
            <div className="user-profile-no-picture" />
          ) : (
            <img
              className={"user-profile-back-image"}
              src={this.targetUser.ProfilePagePicture}
            />
          )}

          <div className={"user-profile-main-info"}>
            <p className={"user-profile-displayname"}>
              {this.targetUser.DisplayName}
            </p>
            <img
              className={"user-profile-profilepic"}
              src={this.targetUser.ProfilePic}
            />
            <p className={"user-profile-username"}>
              {this.targetUser.Username}
            </p>
          </div>

          <div className={"user-profile-more-info"}>
            <p className={"user-profile-more-info-join-date"}>
              {this.getDate(true)}
            </p>
            <p className={"user-profile-more-info-birth-date"}>
              {this.getDate(false)}
            </p>
            {this.targetUser.Username != this.state.currentUser.Username && (
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
            <p className={"user-profile-more-info-country"}>
              {this.targetUser.Country}
            </p>
            <p className={"user-profile-more-info-gender"}>
              {this.targetUser.isMale ? "Male" : "Female"}
            </p>
          </div>
        </div>
        <div className="user-profile-post-feed">
          <PostFeed currentUser={this.state.currentUser} noFieldInput={true} />
        </div>
      </div>
    );
  }
}
