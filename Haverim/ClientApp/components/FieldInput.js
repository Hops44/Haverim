import React from "react";
import "../css/FieldInput.css";
import { POST } from "../RestMethods";
import { checkIfLogged } from "./MainPage";

export class FieldInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      isExpanded: this.props.isExpanded == undefined ? false : this.props.isExpanded,
      text: ""
    };

    this.fieldFocus = this.fieldFocus.bind(this);
    this.fieldLostFocus = this.fieldLostFocus.bind(this);
    this.fieldTextChanged = this.fieldTextChanged.bind(this);
    this.sendToServer = this.sendToServer.bind(this);
  }
  render() {
    return (
      <div
        className={
          this.state.isExpanded ? "field-container field-container-expanded" : "field-container"
        }
      >
        <img className="field-profilepic" src={this.props.profilepic} />
        <textarea
          ref={"input-value"}
          autoFocus={this.props.autoFocus}
          spellCheck={false}
          onFocus={this.fieldFocus}
          onBlur={this.fieldLostFocus}
          onChange={this.fieldTextChanged}
          maxLength={240}
          value={this.state.text}
          className={this.state.isExpanded ? "field-input expanded" : "field-input"}
          placeholder={this.props.isPost ? "Share The World..." : "Comment this post...."}
        />
        {this.state.isExpanded && (
          <button
            disabled={this.state.text < 2}
            className={
              this.state.text.length < 2
                ? "field-post-button field-post-button-disabled"
                : "field-post-button"
            }
            onClick={this.sendToServer}
          >
            {this.props.isPost ? "Post" : "Reply"}
          </button>
        )}
      </div>
    );
  }
  sendToServer() {
    if (this.state.text.length >= 2) {
      //
      // POST
      //
      if (this.props.isPost) {
        var result = POST(
          "/api/posts/createpost",
          JSON.stringify({
            Token: sessionStorage.getItem("jwtkey"),
            Body: this.state.text,
            Tags: getTags(this.state.text)
          })
        );
        var split = result.split(":");
        if (split[0] == "error") {
          alert("There was an error sending that post\nerror " + split[1]);
        } else {
          addFunction.bind(this)(JSON.parse(result));
        }
      } else {
        //
        // COMMENT
        //
        var isLogged = checkIfLogged();
        if (isLogged === false) {
          console.log(isLogged);
          return;
        } else {
          var result = POST(
            "/api/posts/ReplyToPost",
            JSON.stringify({
              Token: sessionStorage.getItem("jwtkey"),
              Body: this.state.text,
              PostId: this.props.postId
            })
          );
          var split = result.split(":");
          if (split[0] == "error") {
            console.log(result);
            return;
          }

          var user = JSON.parse(isLogged);
          this.props.addFunction(user.username, this.state.text, split[1]);
          this.setState({ text: "", isExpanded: false });
        }
      }
    }
  }
  fieldTextChanged(e) {
    this.setState({ text: e.target.value });
  }
  fieldFocus() {
    this.setState({ isExpanded: true });
    //console.log("Focus");
  }
  fieldLostFocus() {
    if (!this.props.cantLoseFocus) {
      if (this.state.text == "") this.setState({ isExpanded: false });
      //console.log("lost");
    }
  }
  getTime() {
    return Math.floor(Date.now() / 1000);
  }
}

function addFunction(serverResponse) {
  let split = serverResponse.split(":");
  if (split[0] == "error") {
    alert("An error has occurred");
    console.log(split[1]);
  }
  this.props.addFunction(
    this.props.profilepic,
    this.props.displayName,
    this.props.username,
    this.state.text,
    this.getTime(),
    split[1]
  );
  this.setState({ text: "", isExpanded: false });
}

function getTags(text) {
  var split = text
    .split("\n")
    .join(" ")
    .split(" ");
  var tags = [];
  for (const word of split) {
    if (word[0] == "#") {
      tags.push(word.substring(1));
    }
  }
  return tags;
}
