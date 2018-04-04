import React from "react";
import "../css/Friends.css";

export class FriendListItem extends React.PureComponent {
  constructor(props) {
    super(props);
    console.log(this.props.isOnline);
  }
  render() {
    return (
      <div
        className="friend-item-container"
        onClick={this
        .redirectToUser
        .bind(this)}>
        <img className="friend-item-profilepic" src={this.props.profilepic}/>
        <p className="friend-item-display-name">{this.props.dispayName}</p>
        <div className="friend-item-status-container">
          <span
            style={this.props.isOnline
            ? {
              backgroundColor: "#6fcf97"
            }
            : {
              backgroundColor: "#E0E0E0"
            }}
            className="friend-item-status-icon"/>
          <p
            style={this.props.isOnline
            ? {
              color: "#6fcf97"
            }
            : {
              color: "#E0E0E0"
            }}
            className="friend-item-status-text">
            {this.props.isOnline
              ? "Online"
              : "Offline"}
          </p>
        </div>
      </div>
    );
  }
  redirectToUser() {
    alert("Redirect To " + this.props.username);
  }
}

export class FriendsList extends React.Component {
  constructor(props) {
    super(props);
  }
  render() {
    return (this.props.friends != null && <ul className="friends-list-ul">
      {this
        .props
        .friends
        .map(item => <li className="friend-li">{item}</li>)}
    </ul>);
  }
}
