import * as React from "react";
import { Route } from "react-router-dom";

import { MainPage } from "./Components/MainPage";
import { ProfilePage } from "./Components/ProfilePage";

const mp = () => {
  return (
    <MainPage currentUser={{ displayName: "Omer Nahum", username: "@omern" }} />
  );
};
const pp = () => {
  return (
    <ProfilePage
      profilepic="/Assets/profilepic.jpg"
      currentUser={{
        displayName: "Omer Nahum",
        username: "@omern"
      }}
      isFollowing={false}
      destinyUser={{
        displayName: "Eviyatar Mizrahi",
        username: "@evimizrahi",
        joinDate: 1514024991,
        birthDate: 644063391,
        country: "United States",
        gender: 0
      }}
    />
  );
};
export const routes = (
  <div>
    <Route exact path="/" component={mp} />
    <Route exact path="/profile" component={pp} />
  </div>
);
