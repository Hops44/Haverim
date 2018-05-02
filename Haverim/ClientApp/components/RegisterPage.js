import React from "react";
import { LoginInput } from "./LoginPage";
import * as Cropper from "cropperjs";
import "../../node_modules/cropperjs/dist/cropper.min.css";
import "../css/RegisterPage.css";
import Countries from "../_contriesList";
import { POSTAsync, POST, GETAsync } from "../RestMethods";
import { TypeFormatFlags } from "typescript";
import { Redirect } from "react-router";

if (Countries[0] !== "Country") {
  Countries.unshift("Country");
}

export default class RegisterPage extends React.Component {
  constructor(props) {
    super(props);

    var tenYearsOffset = 24 * 60 * 60 * 1000 * 3652.42199 * 2;
    var selectedDate = new Date();
    selectedDate.setTime(selectedDate.getTime() - tenYearsOffset);

    this.state = {
      inputUsername: "",
      inputDisplayname: "",
      inputEmail: "",
      inputPassword: "",
      inputConfirmPassword: "",
      selectedDate: selectedDate,
      selectedGender: "",
      selectedCountry: "",
      profileImage: false,
      waitingForServer: false,
      redirect: false
    };
    this.registerUser = this.registerUser.bind(this);
  }

  registerUser() {
    if (this.validateFormFields(this.state)) {
      var requestBody = JSON.stringify({
        Username: this.state.inputUsername,
        DisplayName: this.state.inputDisplayname,
        Email: this.state.inputEmail,
        Password: this.state.inputPassword,
        BirthDateUnix: parseInt(this.state.selectedDate.getTime() / 1000),
        Country: this.state.selectedCountry,
        IsMale: this.state.selectedGender === "Male",
        ProfilePicBase64: this.state.profileImage ? this.state.profileImage : ""
      });
      this.setState({ waitingForServer: true });
      POSTAsync(
        "/api/Users/RegisterUser",
        requestBody,
        function(result) {
          result = result.substring(1, result.length - 1);
          let split = result.split(":");
          if (split[0] == "success") {
            this.interval = setInterval(
              function() {
                GETAsync(
                  "/api/Users/IsUsernameTaken/" + this.state.inputUsername,
                  function(result) {
                    console.log(result);
                    if (result == "true") {
                      localStorage.setItem("jwtkey", split[1]);
                      this.setState({ redirect: true });
                      clearInterval(this.interval);
                    }
                  }.bind(this)
                );
              }.bind(this),
              500
            );
          } else {
            this.setState({ waitingForServer: false });
            console.log(result);
            alert("An error has occurred");
          }
        }.bind(this)
      );
    }
  }
  validateFormFields(fields) {
    var formInputsFilled = true;
    for (const field in fields) {
      const value = fields[field];
      if (field.substring(0, 5) != "input") {
        continue;
      }
      if (value == "") {
        formInputsFilled = false;
        break;
      }
    }
    if (!formInputsFilled) {
      alert("Fill all fields first");
      return;
    }
    if (fields.inputUsername.length < 3) {
      alert("Username is too short");
      return false;
    }
    if (fields.inputDisplayname.length < 3) {
      alert("Display name is too short");
      return false;
    }
    if (!this.validateEmail(fields.inputEmail)) {
      alert("Invalid email");
      return false;
    }
    if (!this.validatePassword(fields.inputPassword, fields.inputConfirmPassword)) {
      alert("Password does not have atleast 6 characters long or passwords do not match");
      return false;
    }
    if (!(fields.selectedGender === "Male" || fields.selectedGender == "Female")) {
      alert("Select a gender");
      return false;
    }
    if (fields.selectedCountry === "Country") {
      alert("Select a country");
      return false;
    }
    return true;
  }
  validateEmail(email) {
    return true;
    try {
      address = new MailAddress(address).Address;
      return true;
    } catch (FormatException) {
      return false;
    }
  }
  validatePassword(password, confrim) {
    let passwordRegex = /^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$/;
    return passwordRegex.test(password) && password == confrim;
  }

  render() {
    return this.state.redirect === true ? (
      <Redirect to="/" />
    ) : (
      <div className="register-page">
        <div className="register-page-header-container">
          <h1 className="register-page-header noselect">Haverim</h1>
        </div>
        <div className="register-page-content">
          <form className="register-page-form">
            <p className="register-page-register-header">Register</p>
            <LoginInput
              placeholder="Username"
              value={this.state.inputUsername}
              updateValue={e => this.setState({ inputUsername: e })}
              className="register-page-input"
              disabled={this.state.waitingForServer}
            />
            <LoginInput
              placeholder="Display Name"
              value={this.state.inputDisplayname}
              updateValue={e => this.setState({ inputDisplayname: e })}
              className="register-page-input"
              disabled={this.state.waitingForServer}
            />
            <LoginInput
              placeholder="Email"
              value={this.state.inputEmail}
              updateValue={e => this.setState({ inputEmail: e })}
              className="register-page-input"
              disabled={this.state.waitingForServer}
            />
            <LoginInput
              placeholder="Password"
              value={this.state.inputPassword}
              updateValue={e => this.setState({ inputPassword: e })}
              className="register-page-input"
              isPassword={true}
              disabled={this.state.waitingForServer}
            />
            <LoginInput
              placeholder="Confirm Password"
              value={this.state.inputConfirmPassword}
              updateValue={e => this.setState({ inputConfirmPassword: e })}
              className="register-page-input"
              isPassword={true}
              disabled={this.state.waitingForServer}
            />
            <div className="register-page-birthday-container">
              <p className="register-page-birthday-header">Birthday</p>
              <DatePicker
                updateDate={d => this.setState({ selectedDate: d })}
                selectedDate={this.state.selectedDate}
              />
            </div>
            <GenderSelection
              changeSelection={s => this.setState({ selectedGender: s })}
              selected={this.state.selectedGender}
            />
            <div className="register-page-country-selector-container">
              <select
                onChange={e =>
                  this.setState({
                    selectedCountry: e.target.value
                  })
                }
                value={this.state.selectedCountry == "" ? "Country" : this.state.selectedCountry}
                className="register-page-country-selector"
              >
                {Countries.map((c, index) => (
                  <option disabled={index == 0} key={c} value={c}>
                    {c}
                  </option>
                ))}
              </select>
            </div>
          </form>
          <div className="register-page-picture-upload-container">
            <p className="register-page-picture-upload-header">Profile Picture</p>
            {this.state.profileImage === false ? (
              <React.Fragment>
                <div
                  onClick={e => {
                    let input = document.createElement("input");
                    input.setAttribute("type", "file");
                    input.accept = "image/*";
                    input.click();
                    input.oninput = function(e) {
                      let fr = new FileReader();
                      fr.onload = function(e) {
                        this.setState({
                          profileImage: e.srcElement.result
                        });
                      }.bind(this);
                      fr.readAsDataURL(e.target.files[0]);
                    }.bind(this);
                  }}
                  className="register-page-picture-upload"
                >
                  <div className="register-page-picture-upload-center">
                    <img
                      className="register-page-picture-upload-icon"
                      src="/Assets/upload-picture.svg"
                    />
                    <p className="register-page-picture-upload-text noselect">Click to upload</p>
                  </div>
                </div>
              </React.Fragment>
            ) : (
              <ImageCropper
                ratio={1 / 1}
                updateCroppedValue={data => this.setState({ profileImage: data })}
                resetImage={() => this.setState({ profileImage: false })}
                imageData={this.state.profileImage}
              />
            )}
            <button onClick={this.registerUser} className="register-page-register-button">
              Register
            </button>
          </div>
        </div>
      </div>
    );
  }
}

class GenderSelection extends React.PureComponent {
  render() {
    return (
      <div className="gender-selection-container">
        <div onClick={() => this.props.changeSelection("Male")} className="gender-selection-item">
          <div className="gender-selection">
            {this.props.selected == "Male" && <div className="gender-selection-selected" />}
          </div>
          <p className="gender-selection-item-header noselect">Male</p>
        </div>
        <div
          onClick={() => this.props.changeSelection("Female")}
          style={{ float: "right" }}
          className="gender-selection-item"
        >
          <div className="gender-selection">
            {this.props.selected == "Female" && <div className="gender-selection-selected" />}
          </div>
          <p className="gender-selection-item-header noselect">Female</p>
        </div>
      </div>
    );
  }
}

class DatePicker extends React.PureComponent {
  constructor(props) {
    super(props);

    var daysInMonth = this.getNumberOfDaysInMonth(
      this.props.selectedDate.getMonth() + 1,
      this.props.selectedDate.getFullYear()
    );

    this.months = [
      "Jan",
      "Feb",
      "Mar",
      "Apr",
      "May",
      "Jun",
      "Jul",
      "Aug",
      "Sep",
      "Oct",
      "Nov",
      "Dec"
    ];
    this.years = [...Array(90).keys()].map(v => new Date().getFullYear() - v - 13);

    this.syncDaysInMonth = this.syncDaysInMonth.bind(this);
  }
  syncDaysInMonth() {
    var daysInMonth = this.getNumberOfDaysInMonth(
      this.props.selectedDate.getMonth() + 1,
      this.props.selectedDate.getFullYear()
    );

    this.daysInMonth = daysInMonth;
    this.days = this.getDaysArray(daysInMonth);
  }

  componentWillMount() {
    this.syncDaysInMonth();
  }

  componentWillUpdate() {
    this.syncDaysInMonth();
  }

  render() {
    return (
      <div className="date-picker-container">
        <select
          className="date-picker-item"
          onChange={function(e) {
            let index = e.target.selectedIndex;
            let prevDate = this.props.selectedDate;
            prevDate.setMonth(index);
            this.props.updateDate(prevDate);
          }.bind(this)}
          defaultValue={this.months[this.props.selectedDate.getMonth()]}
        >
          {this.months.map((month, index) => (
            <option key={month} value={month}>
              {month}
            </option>
          ))}
        </select>
        <select
          className="date-picker-item"
          defaultValue={this.props.selectedDate.getDate()}
          onChange={function(e) {
            let newDate = this.props.selectedDate;
            newDate.setDate(e.target.selectedIndex + 1);
            this.props.updateDate(newDate);
          }.bind(this)}
        >
          {this.days.map(day => (
            <option key={day} value={day}>
              {day}
            </option>
          ))}
        </select>
        <select
          className="date-picker-item"
          onChange={function(e) {
            let newDate = this.props.selectedDate;
            newDate.setYear(e.target.value);
            this.props.updateDate(newDate);
          }.bind(this)}
          defaultValue={this.props.selectedDate.getFullYear()}
        >
          {this.years.map(year => (
            <option key={year} value={year}>
              {year}
            </option>
          ))}
        </select>
      </div>
    );
  }
  getDaysArray(n) {
    return Array.apply(null, Array(n))
      .map(Number.prototype.valueOf, 0)
      .map(function(v, i) {
        return i + 1;
      });
  }
  getNumberOfDaysInMonth(month, year) {
    return new Date(year, month, 0).getDate();
  }
}

export class ImageCropper extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      disabled: false
    };
    this.imageRef = React.createRef();
  }
  componentDidMount() {
    var image = this.imageRef.current;
    this.cropper = new Cropper.default(image, {
      aspectRatio: this.props.ratio == 0 ? null : this.props.ratio,
      viewMode: 2
    });
  }
  render() {
    return (
      <div className="image-cropper-container noselect">
        <img
          draggable={false}
          className="image-cropper-image"
          ref={this.imageRef}
          src={this.state.disabled === false ? this.props.imageData : this.state.disabled}
        />
        <div className="register-page-uploaded-picture-reset-container">
          <p onClick={this.props.resetImage} className="register-page-uploaded-picture-reset">
            Reset Image
          </p>
          <p
            onClick={() => {
              if (this.state.disabled === false) {
                // Pulls base64 data from the cropped image canvas
                let data = this.cropper.getCroppedCanvas().toDataURL();
                // Sets the new cropped image data
                this.setState({ disabled: data });
                this.props.updateCroppedValue(data);
                // Removes the cropper
                this.cropper.destroy();
              } else {
                if (this.props.finishFunction != undefined) {
                  this.props.finishFunction();
                }
              }
            }}
            className={
              this.props.finishFunction != undefined
                ? "register-page-uploaded-picture-reset"
                : this.state.disabled
                  ? "register-page-uploaded-picture-reset register-page-uploaded-picture-reset-disabled"
                  : "register-page-uploaded-picture-reset"
            }
          >
            {this.state.disabled
              ? this.props.finishFunction != undefined
                ? "Upload Image"
                : "Crop Image"
              : "Crop Image"}
          </p>
        </div>
      </div>
    );
  }
}
