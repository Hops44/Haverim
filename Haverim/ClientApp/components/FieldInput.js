import React from "react";
import "../css/FieldInput.css";

export class FieldInput extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      isExpanded:
        this.props.isExpanded == undefined ? false : this.props.isExpanded,
      text: ""
    };

    this.fieldFocus = this.fieldFocus.bind(this);
    this.fieldLostFocus = this.fieldLostFocus.bind(this);
    this.fieldTextChanged = this.fieldTextChanged.bind(this);
    this.sendComment = this.sendComment.bind(this);
  }
  render() {
    return (
      <div
        className={
          this.state.isExpanded
            ? "field-container field-container-expanded"
            : "field-container"
        }
      >
        <img className="field-profilepic" src={this.props.profilepic} />
        <textarea
          autoFocus={this.props.autoFocus}
          spellCheck={false}
          onFocus={this.fieldFocus}
          onBlur={this.fieldLostFocus}
          onChange={this.fieldTextChanged}
          maxLength={240}
          value={this.state.text}
          className={
            this.state.isExpanded ? "field-input expanded" : "field-input"
          }
          placeholder={
            this.props.isPost ? "Share The World..." : "Comment this post...."
          }
        />
        {this.state.isExpanded && (
          <button
            disabled={this.state.text < 2}
            className={
              this.state.text.length < 2
                ? "field-post-button field-post-button-disabled"
                : "field-post-button"
            }
            onClick={this.sendComment}
          >
            {this.props.isPost ? "Post" : "Reply"}
          </button>
        )}
      </div>
    );
  }

  sendComment() {
    if (this.state.text.length >= 2) {
      this.props.addFunction(
        this.props.profilepic,
        this.props.displayName,
        this.props.username,
        this.state.text,
        this.getTime(),
        Math.floor(Math.random() * 400)
      );
      this.setState({ text: "", isExpanded: false });
    }
  }
  fieldTextChanged(e) {
    this.setState({ text: e.target.value });
  }
  fieldFocus() {
    this.setState({ isExpanded: true });
    console.log("Focus");
  }
  fieldLostFocus() {
    if (!this.props.cantLoseFocus) {
      if (this.state.text == "") this.setState({ isExpanded: false });
      console.log("lost");
    }
  }
  getTime() {
    return Math.floor(Date.now() / 1000);
  }
}
