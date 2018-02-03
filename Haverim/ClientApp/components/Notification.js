import React from "react";

export class Notification extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = {
      currentUnixTime: Date.now() / 1000
    };

    this.generateText = this.generateText.bind(this);
    this.formatTime = this.formatTime.bind(this);
  }
  render() {
    return (
      <div className="notification">
        <p className="notification-text">{this.generateText()}</p>
        <p className="notification-time">{this.formatTime()}</p>
      </div>
    );
  }
  generateText() {
    var text = this.props.displayName + " ";
    switch (this.props.type) {
      case 0:
        text += "Has Replied To Your Post";
        break;
      case 1:
        text += "Has Tagged You In Their Post";
        break;
      case 2:
        text += "Has Upvoted Your Post";
        break;
      case 3:
          text += "Has Upvoted Your Comment";
    }
    return text;
  }
  formatTime() {
    const now = this.state.currentUnixTime;
    const diff = Math.floor(now - this.props.unixTime);

    if (diff < 60) return Math.floor(diff) + "s";
    else if (diff < 3600) return Math.floor(diff / 60) + "m";
    else if (diff < 86400) return Math.floor(diff / 3600) + "h";
    else if (diff < 2629743) return Math.floor(diff / 86400) + "d";
    else if (diff < 31556926) {
      var date = new Date(this.props.unixTime * 1000);
      return date.getDate() + " " + this.formatMonth(date.getMonth());
    } else {
      var date = new Date(this.props.unixTime * 1000);
      return (
        date.getDate() +
        " " +
        this.formatMonth(date.getMonth()) +
        " " +
        date
          .getFullYear()
          .toString()
          .substring(2, 4)
      );
    }
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
}
