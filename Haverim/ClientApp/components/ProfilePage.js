import React from "react";
import Navbar from "./Navbar";
import { PostFeed } from "./PostFeed";
import "../css/ProfilePage.css";

export class ProfilePage extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      isFollowing: this.props.isFollowing,
      screenWidth: width
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
    let unix = isJoin
      ? this.props.destinyUser.joinDate
      : this.props.destinyUser.birthDate;
    let date = new Date(unix * 1000);
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
        <Navbar profilepic={this.props.profilepic} />
        <div className={"user-profile-info-container"}>
          <img
            className={"user-profile-back-image"}
            src="Assets/userprofilepagewallimage.jpg"
          />
          <div className={"user-profile-main-info"}>
            <p className={"user-profile-displayname"}>
              {this.props.destinyUser.displayName}
            </p>
            <img
              className={"user-profile-profilepic"}
              src="Assets/profilepic.jpg"
            />
            <p className={"user-profile-username"}>
              {this.props.destinyUser.username}
            </p>
          </div>

          <div className={"user-profile-more-info"}>
            <p className={"user-profile-more-info-join-date"}>
              {this.getDate(true)}
            </p>
            <p className={"user-profile-more-info-birth-date"}>
              {this.getDate(false)}
            </p>
            {this.props.destinyUser.username !=
              this.props.currentUser.userrname && (
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
              {this.props.destinyUser.country}
            </p>
            <p className={"user-profile-more-info-gender"}>
              {this.props.destinyUser.gender ? "Female" : "Male"}
            </p>
          </div>
        </div>
        <div className="user-profile-post-feed">
          <PostFeed noFieldInput={true} />
        </div>
      </div>
    );
  }
}
