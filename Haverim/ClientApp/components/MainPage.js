import React from "react";
import Navbar from "./Navbar";
import Profile from "./UserProfile";
import { PostFeed } from "./PostFeed";
import { FriendsList } from "./FriendsList";
import { FriendListItem } from "./FriendsList";
import "../css/MainPage.css";
import { POST, GET } from "../RestMethods";
import { Redirect, Route, Link } from "react-router-dom";

export function checkIfLogged() {
  var sessionStorage = window.sessionStorage;
  var JWTkey = sessionStorage.getItem("jwtkey");

  var payload = `{"key":"${JWTkey}"}`;
  var url = "/api/users/GetUserByToken";
  var result = POST(url, payload);
  // User Logged In
  if (result.split(":")[0] !== '"error') {
    console.log("%c You're okay! ", "background: #222; color: #bada55");

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
      currentUser: JSON.parse(isLoggedIn),
      isModalOpen: false
    };
    this.switchValue = 1000;

    this.getScreenWidth = this.getScreenWidth.bind(this);
    this.toggleModal = this.toggleModal.bind(this);
    window.addEventListener("resize", this.getScreenWidth);
  }

  componentWillUnmount() {
    window.removeEventListener("resize", this.getScreenWidth);
  }
  toggleModal(state) {
    this.setState({ isModalOpen: state });
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
        <Navbar profilepic={this.state.currentUser.profilePic} />
        <div
          className={
            this.state.isModalOpen
              ? "main-page-content Scrollbar noscroll"
              : "main-page-content Scrollbar"
          }
        >
          {this.state.screenWidth > this.switchValue && (
            <div className="main-page-user-profile">
              <Profile
                toggleModal={this.toggleModal}
                username={this.state.currentUser.username}
                displayName={this.state.currentUser.displayName}
                following={this.state.currentUser.following.length}
                followers={this.state.currentUser.followers.length}
                profilepic={this.state.currentUser.profilePic}
              />
            </div>
          )}
          <div
            className={
              this.state.screenWidth > this.switchValue
                ? "main-page-input-posts"
                : "full"
            }
          >
            <PostFeed currentUser={this.state.currentUser} />
          </div>
          {this.state.screenWidth > this.switchValue && (
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
