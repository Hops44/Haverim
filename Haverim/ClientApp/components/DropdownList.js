import React from "react";
import { Notification } from "./Notification";
import "../css/DropDown.css";
import { POST, POSTAsync } from "../RestMethods";

export class DropdownList extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      notificationList: [],
      loaded: false,
      canLoadMore: true,
      loadingMore: false
    };
    this.closeDropDown = this.closeDropDown.bind(this);
    this.requestNotifcations = this.requestNotifcations.bind(this);
    this.finishLoading = this.finishLoading.bind(this);
    this.listRef = React.createRef();
  }

  render() {
    return (
      <div className="drop-down-list-background">
        <div
          onClick={this.closeDropDown}
          className="drop-down-list-background"
        />
        <ul ref={this.listRef} className="drop-down-list Scrollbar">
          {
            (!this.state.loaded ? (
              <div className="no-notification-container">
                <p className="no-notification-text">Loading...</p>
              </div>
            ) : (
              this.state.loadingMore && <h1>Loading</h1>
            ),
            this.state.notificationList.length == 0 ? (
              <div className="no-notification-container">
                <p className="no-notification-text">No Notifications</p>
              </div>
            ) : (
              this.state.notificationList.map(noti => {
                return <li key={noti.props.id}>{noti}</li>;
              })
            ))
          }
        </ul>
      </div>
    );
  }

  onScrollHandler(e) {
    if (!this.state.loadingMore && this.state.canLoadMore) {
      var windowHeight = 400;
      var docHeight = e.target.scrollHeight;
      var scrollTop = e.target.scrollTop;

      var trackLength = docHeight - windowHeight;
      var scrollPercentage = scrollTop / trackLength;

      if (scrollPercentage > 0.98) {
        this.setState({
          loadingMore: true
        });
      }
    }
  }
  componentDidMount() {
    this.listRef.current.onscroll = this.onScrollHandler.bind(this);
  }

  componentDidUpdate() {
    if (this.state.canLoadMore && this.state.loadingMore) {
      this.requestNotifcations();
    }
  }

  componentWillMount() {
    this.requestNotifcations();
  }

  closeDropDown() {
    this.props.close();
  }

  requestNotifcations() {
    POSTAsync(
      "/api/users/GetNotifications",
      JSON.stringify({
        Token: sessionStorage.getItem("jwtkey"),
        index: this.state.notificationList.length
      }),
      function(result) {
        if (result.split(":")[0] == "error") {
          this.finishLoading();
          return;
        }
        var notifications = JSON.parse(result);
        var keyIndex = -1;
        this.setState(prevState => ({
          loaded: true,
          loadingMore: false,
          canLoadMore: notifications.length == 10,
          notificationList: prevState.notificationList.concat(
            notifications.map(notification => {
              return (
                <Notification
                  id={++keyIndex + prevState.notificationList.length}
                  username={notification.targetUsername}
                  unixTime={new Date(notification.publishDate).getTime() / 1000}
                  type={notification.type}
                />
              );
            })
          )
        }));
      }.bind(this)
    );
  }

  finishLoading() {
    this.setState({ loaded: true, canLoadMore: false, loadingMore: false });
  }
}
