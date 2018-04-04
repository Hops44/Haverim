import React from "react";
import { Notification } from "./Notification";
import "../css/DropDown.css";
import { POST } from "../RestMethods";

export class DropdownList extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      notificationList: [],
      loaded: false
    };
    this.closeDropDown = this.closeDropDown.bind(this);
    this.requestNotifcations = this.requestNotifcations.bind(this);
    this.finishLoading = this.finishLoading.bind(this);
    this.requestNotifcations();
  }

  render() {
    return (
      <div className="drop-down-list-background">
        <div
          onClick={this.closeDropDown}
          className="drop-down-list-background"
        />
        <ul className="drop-down-list">
          {!this.state.loaded ? (
            <div className="no-notification-container">
              <p className="no-notification-text">Loading...</p>
            </div>
          ) : this.state.notificationList.length == 0 ? (
            <div className="no-notification-container">
              <p className="no-notification-text">No Notifications</p>
            </div>
          ) : (
            this.state.notificationList.map(noti => {
              return <li key={noti.props.id}>{noti}</li>;
            })
          )}
        </ul>
      </div>
    );
  }

  closeDropDown() {
    this.props.close();
  }

  requestNotifcations() {
    setTimeout(
      function() {
        var result = POST(
          "/api/users/GetNotifications",
          JSON.stringify({
            Token: sessionStorage.getItem("jwtkey"),
            index: 0
          })
        );
        result = result.substring(1, result.length - 1);
        if (result.split(":")[0] == "error") {
          this.finishLoading();
          return;
        }
        var notifications = JSON.parse(result.split("\\").join(""));
        var keyIndex = 0;
        this.setState({
          loaded: true,
          notificationList: notifications.map(notification => {
            return (
              <Notification
                id={keyIndex++}
                username={notification.TargetUsername}
                unixTime={new Date(notification.PublishDate).getTime() / 1000}
                type={notification.Type}
              />
            );
          })
        });

        this.finishLoading();
      }.bind(this),
      1000
    );
  }
  finishLoading() {
    this.setState({ loaded: true });
  }
}
