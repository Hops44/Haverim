import React from "react";
import Navbar from "./Navbar";
import UserProfile from "./UserProfile";
import { PostFeed } from "./PostFeed";
import { FriendsList } from "./FriendsList";
import { FriendListItem } from "./FriendsList";
import "../css/MainPage.css";

export class MainPage extends React.Component {
  constructor(props) {
    super(props);
    var width =
      window.innerWidth ||
      document.documentElement.clientWidth ||
      document.body.clientWidth;
    this.state = { screenWidth: width };
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
    return (
      <div className="main-page">
        <Navbar profilepic={this.props.currentUser.profilepic} />
        <div className="main-page-content">
          {this.state.screenWidth > 1000 && (
            <div className="main-page-user-profile">
              <UserProfile
                displayName={this.props.currentUser.displayName}
                following={this.props.currentUser.following}
                followers={this.props.currentUser.followers}
                profilepic={this.props.currentUser.profilepic}
              />
            </div>
          )}
          <div
            className={
              this.state.screenWidth > 1000 ? "main-page-input-posts" : "full"
            }
          >
            <PostFeed
              displayName={this.props.currentUser.displayName}
              username={this.props.currentUser.username}
              profilepic={this.props.currentUser.profilepic}
            />
          </div>
          {this.state.screenWidth > 1000 && (
            <div className="main-page-friends-list">
              <FriendsList
                friends={[
                  <FriendListItem
                    username="@dsmith"
                    profilepic="Assets/davidprofile.png"
                    dispayName="David Smith"
                    isOnline={true}
                  />,
                  <FriendListItem
                    username="@dsmith"
                    profilepic="Assets/davidprofile.png"
                    dispayName="David Smith"
                    isOnline={false}
                  />,
                  <FriendListItem
                    username="@dsmith"
                    profilepic="Assets/davidprofile.png"
                    dispayName="David Smith"
                    isOnline={false}
                  />
                ]}
              />
            </div>
          )}
        </div>
      </div>
    );
  }
}
