import React from "react";
import "../css/UserProfile.css";

class Profile extends React.PureComponent {
  constructor(props) {
    super(props);
  }
  render() {
    return (
      <div className="profile-container">
        <img className="profile-image" src={this.props.profilepic} />
        <p className="username-label">{this.props.displayName}</p>
        <div className="followers-container">
          <p className="follow-label">Followers</p>
          <p className="follow-count-label">{this.props.followers}</p>
        </div>
        <div className="following-container">
          <p className="follow-label">Following</p>
          <p className="follow-count-label">{this.props.following}</p>
        </div>
      </div>
    );
  }
}
export default Profile;
