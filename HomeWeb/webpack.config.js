const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const webpack = require("webpack");
const CleanWebpackPlugin = require('clean-webpack-plugin');

module.exports = {
    mode: 'development',
    resolve: {
        extensions: ['.js', '.ts']
    },
    entry: {
        polyfills: "./client/src/polyfills.ts",
        main: "./client/src/main.ts"
    },
    output: {
        path: path.resolve(__dirname, 'wwwroot'), // output directory
        filename: "[name].js", // name of the generated bundle,
        publicPath: '/' // Webpack dev middleware, if enabled, handles requests for this URL prefix
    },
    module: {
        rules: [
            {
                test: /\.html$/,
                loader: "html-loader"
            }, {
                test: /\.css$/,
                loader: ["style-loader", "css-loader"]
            },
        {
            test: /\.ts$/,
            loaders: ["awesome-typescript-loader", "angular2-template-loader"],
            exclude: [/node_modules/, /\.(spec|e2e)\.ts$/]
        },
        {
            test: /\.ts$/,
            enforce: "pre",
            loader: 'tslint-loader'
        },
        {
            test: /\.scss$/,
            loader: ["to-string-loader", "style-loader", "css-loader?sourceMap", "sass-loader?sourceMap"],
            //use: [
            //    {
            //        loader: "to-string-loader"  //
            //    }]
        },
            {
                test: /\.jpe?g$|\.ico$|\.gif$|\.png$|\.svg$|\.woff$|\.ttf$|\.wav$|\.mp3$/,
                loader: 'file-loader?name=[name].[ext]'  // <-- retain original file name
            }
        ]
    },
    optimization: {
        splitChunks: {
            cacheGroups: {
                vendor: {
                    chunks: 'initial',
                    name: 'vendor',
                    test: 'vendor'
                }
            }
        }
    },
    plugins: [
      new HtmlWebpackPlugin({
          template: "client/src/index.html",
          favicon: "client/src/favicon.ico",
          inject: "body"
      }),
      new webpack.ContextReplacementPlugin(
        /\@angular(\\|\/)core(\\|\/)fesm5/,
        path.resolve(__dirname, 'client/src'), {}
      )
    ],
    devtool: "source-map",
    devServer: {
        historyApiFallback: true
    }
};
