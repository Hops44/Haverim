import React from "react";
import "../css/Navbar.css";
import { DropdownList } from "./DropdownList";
import { Link } from "react-router-dom";

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
          <Link to="/">
            <p className="haverim-header">Haverim</p>
          </Link>
        </div>
        <Link to={"/profile"}>
          <img className="profile-img" src={this.props.profilepic} />
        </Link>
      </div>
    );
  }

  notificationIconClicked() {
    this.setState(prevState => ({
      notificationExpanded: !prevState.notificationExpanded
    }));
  }
}
export default NavBar;
