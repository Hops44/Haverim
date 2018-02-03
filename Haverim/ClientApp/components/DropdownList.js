import React from "react";
import { Notification } from "./Notification";
import "../css/DropDown.css";

export class DropdownList extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      notificationList: []
    };
    this.closeDropDown = this.closeDropDown.bind(this);
    this.addNotification = this.addNotification.bind(this);
  }
  render() {
    return (
      <div className="drop-down-list-background">
        <div
          onClick={this.closeDropDown}
          className="drop-down-list-background"
        />
        <ul className="drop-down-list">
          {this.state.notificationList.map(noti => <li>{noti}</li>)}
          {this.state.notificationList.length == 0 && (
            <div className="no-notification-container">
              <p className="no-notification-text">No Notifications</p>
            </div>
          )}
        </ul>
      </div>
    );
  }
  addNotification(displayName, username, unixTime, type) {
    const temp = (
      <Notification
        displayName={displayName}
        username={username}
        unixTime={unixTime}
        type={type}
      />
    );
    this.setState(prevState => ({
      notificationList: prevState.notificationList.concat(temp)
    }));
  }
  closeDropDown() {
    this.props.close();
  }
}
