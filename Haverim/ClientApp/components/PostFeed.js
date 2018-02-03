import React from "react";
import Post from "./Post";
import { FieldInput } from "./FieldInput";
import "../css/PostFeed.css";

export class PostFeed extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      postList: this.props.posts ? this.props.posts : [],
      finishedLoading: false
    };
    this.addPostToFeed = this.addPostToFeed.bind(this);
    this.formatTime = this.formatTime.bind(this);
    this.up = this.up.bind(this);
    this.timeOut = setTimeout(this.up, 700);

    /*
        1. Start animation with a timer
        2. fetch feed from server
        3. if feed fetch takes more than 20 seconds stop animation and display an error message
        4. if the fetch action return in less than 20 seconds , stop the animation and display posts 
        */
  }
  up() {
    this.setState({
      postList: [
        <Post
          displayName="Omer Nahum"
          username="@omern"
          profilepic="/Assets/profilepic.jpg"
          body="Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! Hello World! "
          unixTime={1451606400}
          extraInfo={{
            user: {
              username: "@omern",
              displayName: "Omer Nahum"
            },
            type: 0
          }}
        />,
        <Post
          displayName="Omer Nahum"
          username="@omern"
          profilepic="/Assets/profilepic.jpg"
          body="Hello World!"
          unixTime={1451606400}
          extraInfo={{
            user: {
              username: "@dsmith",
              displayName: "David Smith"
            },
            type: 1
          }}
        />,
        <Post
          displayName="Omer Nahum"
          username="@omern"
          profilepic="/Assets/profilepic.jpg"
          body="Hello World!"
          unixTime={1451606400}
        />
      ],
      finishedLoading: true
    });
  }
  componentWillUnmount() {
    clearTimeout(this.timeOut);
  }
  addPostToFeed(profilepic, displayName, username, body, time, key) {
    const postToAdd = (
      <Post
        profilepic={profilepic}
        displayName={displayName}
        username={username}
        body={body}
        unixTime={time}
        key={key}
      />
    );
    this.setState(prevState => ({
      postList: [postToAdd].concat(prevState.postList.slice())
    }));
  }
  formatTime(unix) {
    const now = Date.now() / 1000;
    const diff = Math.floor(now - unix);

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
  logKey(e) {
    console.log(e);
  }
  render() {
    return (
      <div>
        {!this.props.noFieldInput && (
          <FieldInput
            username={this.props.username}
            displayName={this.props.displayName}
            addFunction={this.addPostToFeed}
            profilepic={"/Assets/profilepic.jpg"}
            isPost={true}
          />
        )}
        <ul className="feed-ul">
          {this.state.finishedLoading ? (
            this.state.postList.length > 0 ? (
              this.state.postList.map(item => (
                <li className="feed-post-item">{item}</li>
              ))
            ) : (
              <h1 style={{ textAlign: "center" }}>No Aviable Posts</h1>
            )
          ) : (
            <img className="load-animation" src="/Assets/load-animation.svg" />
          )}
        </ul>
      </div>
    );
  }
}
