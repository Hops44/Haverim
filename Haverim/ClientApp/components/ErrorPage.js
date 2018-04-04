import React from "react";

export class ErrorPage extends React.Component {
  constructor(props) {
    super(props);
    this.state = {};
  }
  render() {
    return (
      <div>
        <h1>Whoops wrong path!</h1>
      </div>
    );
  }
}
