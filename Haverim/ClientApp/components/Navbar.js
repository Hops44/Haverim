import React from "react";
import "../css/Navbar.css";
import { DropdownList } from "./DropdownList";

class NavBar extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      src: "/Assets/notification.svg",
      notificationExpanded: false
    };
  }
  render() {
    return (
      <div className="nav-bar">
        <div id="centerrr" />

        <img
          onClick={this.notificationIconClicked.bind(this)}
          className="notification-img"
          src={this.state.src}
        />
        {this.state.notificationExpanded && (
          <DropdownList close={this.notificationIconClicked.bind(this)} />
        )}
        <img className="inbox-img" src="/Assets/inbox.svg" />
        <div className="haverim-header-container noselect">
          <p
            onClick={this.redirectToHome.bind(this)}
            className="haverim-header"
          >
            Haverim
          </p>
        </div>
        <img
          onClick={this.redirectToUserPage.bind(this)}
          className="profile-img"
          src={this.props.profilepic}
        />
      </div>
    );
  }
  redirectToHome() {
    //window.location.href = "/";
    alert("Home");
  }
  redirectToUserPage() {
    alert("User page");
  }
  notificationIconClicked() {
    this.setState(prevState => ({
      notificationExpanded: !prevState.notificationExpanded
    }));
  }
}
export default NavBar;
