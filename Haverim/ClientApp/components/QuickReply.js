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
        <div
          onClick={this.closeQuickReply}
          className="modal-background"
          style={{ position: "fixed" }}
        />
        <div className="quick-reply-field-container">
          <FieldInput
            profilepic={this.props.profilepic}
            autoFocus={true}
            cantLoseFocus={true}
            isExpanded={true}
            isPost={false}
            username={this.props.username}
            addFunction={this.sendComment}
            postId={this.props.postId}
          />
        </div>
      </div>
    );
  }
  sendComment() {
    this.closeQuickReply();
  }
  closeQuickReply() {
    this.props.closeFunction();
  }
}
