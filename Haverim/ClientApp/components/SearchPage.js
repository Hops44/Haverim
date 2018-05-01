import React from "react";
import NavBar from "./Navbar";
import { Redirect } from "react-router";
import { checkIfLogged } from "./MainPage";
import "../css/SearchPage.css";
import { GETAsync } from "../RestMethods";
import { Link } from "react-router-dom";

class SearchPage extends React.Component {
  constructor(props) {
    super(props);

    let windowLocation = window.location.href;
    windowLocation = windowLocation.substring(
      windowLocation.indexOf("search") + "search".length + 1
    );
    windowLocation = windowLocation.split("%20").join(" ");
    this.state = {
      results: [],
      searchQuery: windowLocation
    };
    var isLogged = checkIfLogged();
    this.currentUser = isLogged === false ? false : JSON.parse(isLogged);

    this.searchUser = this.searchUser.bind(this);
    this.searchInputRef = React.createRef();
  }
  componentDidMount() {
    this.searchUser();
  }

  searchUser() {
    GETAsync("/api/Users/SearchUser/" + this.searchInputRef.current.value, result => {
      let searchResult = JSON.parse(result);
      this.setState({ results: searchResult });
    });
  }

  render() {
    return (
      <div>
        {this.state.currentUser === false && <Redirect to="/" />}
        <NavBar profilepic={this.currentUser.profilePic} />
        <div className="search-page-content Scrollbar">
          <div className="search-page-search-bar-container">
            <input
              value={this.state.searchQuery}
              onChange={e => this.setState({ searchQuery: e.target.value })}
              onKeyDown={e => e.keyCode == 13 && this.searchUser()}
              ref={this.searchInputRef}
              spellCheck={false}
              className="search-page-search-bar"
            />
            <div onClick={() => this.searchUser()} className="search-page-search-bar-icon" />
          </div>
          <div className="search-page-result-list">
            {this.state.results &&
              this.state.results.map(item => (
                <SearchResultItem
                  key={item.username}
                  displayName={item.displayName}
                  username={item.username}
                />
              ))}
          </div>
        </div>
      </div>
    );
  }
}
class SearchResultItem extends React.PureComponent {
  constructor(props) {
    super(props);
  }
  render() {
    return (
      <React.Fragment>
        <Link
          style={{ textDecoration: "none", color: "black" }}
          to={"/profile/" + this.props.username}
        >
          <div className="search-result-item-container noselect">
            <img
              className="search-result-item-image"
              src="https://www.w3schools.com/howto/img_fjords.jpg"
            />
            <p className="search-result-item-displayname">{this.props.displayName}</p>
            <p className="search-result-item-username">{this.props.username}</p>
          </div>
        </Link>
      </React.Fragment>
    );
  }
}

export default SearchPage;
