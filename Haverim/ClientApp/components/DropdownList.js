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
  componentWillMount() {
    this.mounted = true;
  }
  componentWillUnmount() {
    this.mounted = false;
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
        var result = POST(
          "/api/users/GetNotifications",
          JSON.stringify({
            Token: sessionStorage.getItem("jwtkey"),
            index: 0
          })
        );
        if (result.split(":")[0] == "error") {
          this.finishLoading();
          return;
        }
        var notifications = JSON.parse(result);
        var keyIndex = 0;
        if (this.mounted) {
          this.setState({
            loaded: true,
            notificationList: notifications.map(notification => {
              return (
                <Notification
                  id={keyIndex++}
                  username={notification.targetUsername}
                  unixTime={new Date(notification.publishDate).getTime() / 1000}
                  type={notification.type}
                />
              );
            })
          });

          this.finishLoading();
        }
      }
  }
  finishLoading() {
    this.setState({ loaded: true });
  }
}
