import * as React from "react";
import { BrowserRouter as Router, Route, Switch } from "react-router-dom";

import MainPage from "./Components/MainPage";
import { LoginPage } from "./components/LoginPage";
import ProfilePage from "./components/ProfilePage";
import { ErrorPage } from "./components/ErrorPage";
import RegisterPage from "./components/RegisterPage";
import SearchPage from "./components/SearchPage";

export const routes = (
  <Router>
    <div>
      <Switch>
        <Route exact path="/" component={MainPage} />
        <Route path="/login" component={() => <LoginPage isDebug={false} />} />
        <Route path="/debug" component={() => <LoginPage isDebug={true} />} />
        <Route exact path="/profile" component={ProfilePage} />
        <Route exact path="/register" component={RegisterPage} />
        <Route
          exact
          path="/profile/:user"
          component={({ match }) => <ProfilePage target={match.params.user} />}
        />
        <Route path="/search" component={() => <SearchPage searchQuery="" />} />
        <Route
          exact
          path="/search/:query"
          component={({ match }) => <SearchPage searchQuery={match.params.query} />}
        />
        <Route exact path="/invalid" component={ErrorPage} />
      </Switch>
    </div>
  </Router>
);
