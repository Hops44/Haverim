import React from "react";
import { FieldInput } from "../Components/FieldInput";
import "../css/QuickReply.css";

export class QuickReply extends React.Component {
  constructor(props) {
    super(props);
    this.state = { text: "" };
    this.closeQuickReply = this.closeQuickReply.bind(this);
    this.sendComment = this.sendComment.bind(this);
  }
  render() {
    return (
      <div>
        <div onClick={this.closeQuickReply} className="modal-background" />
        <div className="quick-reply-field-container">
          <FieldInput
            autoFocus={true}
            cantLoseFocus={true}
            isExpanded={true}
            profilepic={this.props.profilepic}
            isPost={false}
            displayName={this.props.displayName}
            username={this.props.username}
            addFunction={this.sendComment}
          />
        </div>
      </div>
    );
  }
  sendComment() {
    console.log("Sent ->", arguments[3]);
    this.closeQuickReply();
  }
  closeQuickReply() {
    this.props.closeFunction();
  }
}
