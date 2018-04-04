import React from "react";
import Navbar from "./Navbar";
import Profile from "./UserProfile";
import { PostFeed } from "./PostFeed";
import { FriendsList } from "./FriendsList";
import { FriendListItem } from "./FriendsList";
import "../css/MainPage.css";
// import "./jquery-3.3.1.min";
import { POST, GET } from "../RestMethods";
import { Redirect, Route, Link } from "react-router-dom";

export function checkIfLogged() {
  var sessionStorage = window.sessionStorage;
  var JWTkey = sessionStorage.getItem("jwtkey");

  var payload = `{"key":"${JWTkey}"}`;
  var url = "/api/users/GetUserByToken";
  var result = POST(url, payload);

  // User Logged In
  if (result.split(":")[0] == '"success') {
    result = result.substring(9, result.length - 1);
    console.log("%c You're okay! ", "background: #222; color: #bada55");
    result = result.split("\\").join("");
    //User is not logged in, redirect to login page
    return result;
  } else {
    console.log(result);
    return false;
  }
}

export class MainPage extends React.Component {
  constructor(props) {
    super(props);
    var isLoggedIn = checkIfLogged();

    var width =
      window.innerWidth ||
      document.documentElement.clientWidth ||
      document.body.clientWidth;
    this.state = {
      screenWidth: width,
      isLoggedIn: isLoggedIn,
      currentUser: JSON.parse(isLoggedIn)
    };

    this.getScreenWidth = this.getScreenWidth.bind(this);

    window.addEventListener("resize", this.getScreenWidth);
  }

  componentWillUnmount() {
    window.removeEventListener("resize", this.getScreenWidth);
  }

  getScreenWidth() {
    var width =
      window.innerWidth ||
      document.documentElement.clientWidth ||
      document.body.clientWidth;
    this.setState({ screenWidth: width });
  }
  render() {
    return !this.state.isLoggedIn ? (
      <Redirect to="/login" />
    ) : (
      <div className="main-page">
        <Navbar profilepic={this.state.currentUser.ProfilePic} />
        <div className="main-page-content">
          {this.state.screenWidth > 1000 && (
            <div className="main-page-user-profile">
              <Profile
                username={this.state.currentUser.Username}
                displayName={this.state.currentUser.DisplayName}
                following={this.state.currentUser.Following.length}
                followers={this.state.currentUser.Followers.length}
                profilepic={this.state.currentUser.ProfilePic}
              />
            </div>
          )}
          <div
            className={
              this.state.screenWidth > 1000 ? "main-page-input-posts" : "full"
            }
          >
            <PostFeed currentUser={this.state.currentUser} />
          </div>
          {this.state.screenWidth > 1000 && (
            <div className="main-page-friends-list">
              {" "}
              <FriendsList
              // friends = { [ < FriendListItem username = "@dsmith" profilepic =
              // "Assets/davidprofile.png" dispayName = "David Smith" isOnline = {     true }
              // />, < FriendListItem username = "@dsmith" profilepic =
              // "Assets/davidprofile.png" dispayName = "David Smith" isOnline = {     false }
              // />, < FriendListItem username = "@dsmith" profilepic =
              // "Assets/davidprofile.png" dispayName = "David Smith" isOnline = {     false }
              // /> ] }
              />{" "}
            </div>
          )}
        </div>
      </div>
    );
  }
}
