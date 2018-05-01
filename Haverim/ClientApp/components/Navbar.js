import React from "react";
import "../css/Navbar.css";
import { DropdownList } from "./DropdownList";
import { Link, Redirect } from "react-router-dom";

class NavBar extends React.PureComponent {
  constructor(props) {
    super(props);
    this.state = {
      notificationExpanded: false,
      searchInputOpen: false,
      searchValue: ""
    };
    this.redirectRef = React.createRef();
    this.usingDefaultPic =
      this.props.profilepic.substring(this.props.profilepic.length - "default".length) == "default";
    this.searchInputRef = React.createRef();
  }

  render() {
    return (
      <div className="nav-bar">
        {this.state.redirect &&
          function() {
            this.interval = setInterval(
              function() {
                if (this.redirectRef.current) {
                  document.getElementById("Nahum").click();
                  clearInterval(this.interval);
                }
              }.bind(this),
              20
            );
            return (
              <Link ref={this.redirectRef} id="Nahum" to={"/search/" + this.state.searchValue} />
            );
          }.bind(this)()}
        {/* {this.state.redirect && <Redirect to={"/search/" + this.state.searchValue} />} */}
        <div id="centerrr" />
        <img
          onClick={this.notificationIconClicked.bind(this)}
          className="notification-img"
          src={"/Assets/notification.svg"}
        />
        {this.state.notificationExpanded && (
          <DropdownList close={this.notificationIconClicked.bind(this)} />
        )}
        <img
          onClick={() =>
            this.state.searchInputOpen === false
              ? window.innerWidth < 950
                ? this.setState({ searchInputOpen: "redirect" })
                : function() {
                    this.searchInputRef.current.focus();
                    return this.setState({ searchInputOpen: true });
                  }.bind(this)()
              : this.setState({ searchInputOpen: false, searchValue: "" })
          }
          className="search-img"
          src="/Assets/search-icon.svg"
        />
        <div className="navbar-search-input-container">
          <input
            ref={this.searchInputRef}
            value={this.state.searchValue}
            onChange={e => this.setState({ searchValue: e.target.value })}
            onKeyDown={e => e.keyCode == 13 && this.setState({ redirect: true })}
            style={{
              width: this.state.searchInputOpen === true ? "250px" : "0px",
              opacity: this.state.searchInputOpen === true ? "1" : "0"
            }}
            spellCheck={false}
            className="navbar-search-input"
            placeholder="Search Haverim"
          />
        </div>

        <div className="haverim-header-container noselect">
          <Link to="/">
            <p className="haverim-header">Haverim</p>
          </Link>
        </div>
        <Link to={"/profile"}>
          <img
            className={this.usingDefaultPic ? "profile-img profile-img-default" : "profile-img"}
            src={this.props.profilepic}
          />
        </Link>
      </div>
    );
  }

  notificationIconClicked() {
    this.setState(prevState => ({
      notificationExpanded: !prevState.notificationExpanded
    }));
  }
}
export default NavBar;
