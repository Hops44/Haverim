import React from "react";
import { Redirect } from "react-router";
import { checkIfLogged } from "./MainPage";
import "./../css/LoginPage.css";
import { POSTAsync, POST } from "../RestMethods";

export class LoginPage extends React.Component {
  constructor(props) {
    super(props);
    var logged = checkIfLogged();
    this.state = {
      redirect: logged != false,
      rememberChecked: false,
      waitingForServer: false,
      loginUsername: "",
      loginPassword: "",
      registerEmail: ""
    };
    this.updateServerResultStatus = this.updateServerResultStatus.bind(this);
  }
  render() {
    return (
      <React.Fragment>
        {this.state.redirect && <Redirect to="/" />}
        {this.props.isDebug === true ? (
          <React.Fragment>
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
          </React.Fragment>
        ) : (
          <div className="login-page">
            {this.state.redirect !== false && <Redirect to={this.state.redirect} />}
            <h1 className="login-page-header">Haverim</h1>
            <div className="login-page-login-container">
              <div className="login-page-inputs-container">
                <LoginInput
                  className="login-page-input"
                  disabled={this.state.waitingForServer}
                  updateValue={e => this.setState({ loginUsername: e })}
                  value={this.state.loginUsername}
                  placeholder="Username"
                />
                <LoginInput
                  className="login-page-input"
                  disabled={this.state.waitingForServer}
                  isPassword={true}
                  updateValue={e => this.setState({ loginPassword: e })}
                  value={this.state.loginPassword}
                  placeholder="Password"
                />
                <RadioButton
                  disabled={this.state.waitingForServer}
                  toggleChecked={() =>
                    this.setState(prevState => ({
                      rememberChecked: !prevState.rememberChecked
                    }))
                  }
                  isChecked={this.state.rememberChecked}
                />
              </div>
              <div className="login-page-buttons-container">
                <button
                  disabled={this.state.waitingForServer}
                  onClick={this.login.bind(this)}
                  className="login-page-login-button noselect"
                >
                  Login
                </button>
                <button
                  disabled={this.state.waitingForServer}
                  className="login-page-login-button noselect"
                  onClick={() => this.setState({ redirect: "/register" })}
                >
                  Register
                </button>
              </div>
            </div>
          </div>
        )}
      </React.Fragment>
    );
  }
  updateServerResultStatus(waiting, redirect = false) {
    this.setState(prevState => ({
      waitingForServer: waiting,
      loginPassword: waiting === false ? "" : prevState.loginPassword,
      redirect: redirect === false ? false : redirect
    }));
    console.log(this.state.redi);
  }

  login() {
    this.updateServerResultStatus(true);
    let username = this.state.loginUsername;
    let password = this.state.loginPassword;

    if (this._validateFields(username, password) === false) {
      this.updateServerResultStatus(false);
      alert("Please fill all fields");
      return;
    }

    let body = JSON.stringify({
      Username: username,
      Password: password
    });

    POSTAsync(
      "/api/Users/LoginUser",
      body,
      function(result) {
        result = result.substring(1, result.length - 1);
        let resultSplit = result.split(":");
        if (resultSplit[0] === "success") {
          sessionStorage.clear();
          localStorage.clear();
          if (this.state.rememberChecked === true) {
            localStorage.setItem("jwtkey", resultSplit[1]);
          }
          sessionStorage.setItem("jwtkey", resultSplit[1]);
          this.updateServerResultStatus(false, "/");
        } else {
          //Wrong credentials
          this.updateServerResultStatus(false);
          console.log(result);
          alert("Wrong username or password");
          return;
        }
        console.log(result);
      }.bind(this)
    );
  }
  _validateFields(username, password) {
    if (username == "" || password == "") {
      return false;
    }
    return true;
  }
}

class RadioButton extends React.PureComponent {
  constructor(props) {
    super(props);
  }
  render() {
    return (
      <div
        onClick={() => this.props.toggleChecked()}
        className={
          this.props.disabled
            ? "login-page-radio-button-container login-page-radio-button-container-disabled "
            : "login-page-radio-button-container"
        }
      >
        <div className="login-page-input-checkbox">
          {this.props.isChecked && (
            <img className="login-page-input-checkbox-tick" src="/assets/tick.svg" />
          )}
          <p className="login-page-input-checkbox-label noselect">Remember me</p>
        </div>
      </div>
    );
  }
}

export class LoginInput extends React.PureComponent {
  render() {
    return (
      <input
        required
        onKeyDown={function(e) {
          //Focus next input on enter
          if (e.keyCode == 13) {
            e.target.nextSibling.focus();
          }
        }}
        type={this.props.isPassword ? "password" : "text"}
        onChange={e => this.props.updateValue(e.target.value)}
        disabled={this.props.disabled}
        className={this.props.className}
        spellCheck={false}
        value={this.props.value}
        placeholder={this.props.placeholder}
      />
    );
  }
}
