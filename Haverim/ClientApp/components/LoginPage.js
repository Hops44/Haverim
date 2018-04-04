import React from "react";
import { Redirect } from "react-router";
import { checkIfLogged } from "./MainPage";

export class LoginPage extends React.Component {
  constructor(props) {
    super(props);
    var logged = checkIfLogged();
    this.state = { redirect: logged != false };
  }
  render() {
    return (
      <div>
        {this.state.redirect && <Redirect to="/" />}
        <h1>Login Here</h1>
        <button
          onClick={() => {
            sessionStorage.setItem(
              "jwtkey",
              "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJVc2VybmFtZSI6ImFubmxhbmU4MSJ9.D4REmxX0rbo3JFWsPcfqjug2DtbbTP0vqHaHb0J9H6A"
            );
            this.setState({ redirect: true });
          }}
        >
          SetKey
        </button>
      </div>
    );
  }
}
