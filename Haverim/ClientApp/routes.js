import * as React from "react";
import { BrowserRouter as Router, Route, Switch } from "react-router-dom";

import { MainPage } from "./Components/MainPage";
import { LoginPage } from "./components/LoginPage";
import { ProfilePage } from "./components/ProfilePage";
import { ErrorPage } from "./components/ErrorPage";

export const routes = (
  <Router>
    <div>
      <Switch>
        <Route exact path="/" component={MainPage} />
        <Route path="/login" component={LoginPage} />
        <Route exact path="/profile" component={ProfilePage} />
        <Route
          exact
          path="/profile/:user"
          component={({ match }) => <ProfilePage target={match.params.user} />}
        />
        <Route exact path="/invalid" component={ErrorPage} />
      </Switch>
    </div>
  </Router>
);
