const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const webpack = require("webpack");
const CleanWebpackPlugin = require('clean-webpack-plugin');
const { CheckerPlugin } = require('awesome-typescript-loader');

module.exports = {
  mode: 'development',
  resolve: {
    extensions: ['.js', '.ts']
  },
  entry: {
    polyfills: "./src/polyfills.ts",
    main: "./src/app.ts",
  },
  output: {
    path: path.resolve(__dirname, 'wwwroot'), // output directory
    filename: "[name].js", // name of the generated bundle,
    //publicPath: '/dist/' // Webpack dev middleware, if enabled, handles requests for this URL prefix
  },
  module: {
    rules: [{
      test: /\.css$/,
      loader: ["style-loader", "css-loader"]
    },
    {
      test: /\.ts$/,
      loader: "awesome-typescript-loader"
    },
    {
      test: /\.ts$/,
      enforce: "pre",
      loader: 'tslint-loader'
    },
    {
      test: /\.scss$/,
      loader: ["style-loader", "css-loader?sourceMap", "sass-loader?sourceMap"]
    }
    ]
  },
  optimization: {
    splitChunks: {
      cacheGroups: {
        vendor: {
          chunks: 'initial',
          name: 'vendor',
          test: 'vendor',
        },
      },
    }
  },
  plugins: [    
    new HtmlWebpackPlugin({
      template: "src/index.html",
      inject: "body"
    }),
    new webpack.ContextReplacementPlugin(
      /\@angular(\\|\/)core(\\|\/)fesm5/,
      path.resolve(__dirname, 'src'), {}
    ),
    //new CheckerPlugin(),
    //new webpack.HotModuleReplacementPlugin()
  ],
  devtool: "source-map",
  devServer: {
    historyApiFallback: true,
    //hot: true
  }
};
