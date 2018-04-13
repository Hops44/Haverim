const path = require("path");
const webpack = require("webpack");

module.exports = {
  entry: { main: ["./ClientApp/boot.js"] },
  output: {
    path: path.resolve(__dirname, "wwwroot/dist"),
    filename: "js/[name].js",
    publicPath: "/",
    hotUpdateChunkFilename: "hot/hot-update.js",
    hotUpdateMainFilename: "hot/hot-update.json"
  },
  devtool: "source-map",
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        loaders: ["babel-loader?retainLines=true"],
        include: path.join(__dirname, "ClientApp")
      },
      {
        test: /(\.css$)/,
        loaders: ["style-loader", "css-loader"]
      },
      {
        test: /\.(png|woff|woff2|eot|ttf|svg)$/,
        loader: "url-loader?limit=100000"
      }
    ]
  }
};
