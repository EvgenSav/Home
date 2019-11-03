const merge = require('webpack-merge');
const common = require('./webpack.config.js');

const webpackDevServerPort = 8080;
const host = "localhost";

module.exports = merge(common, {
    mode: 'development',
    devServer: {
        historyApiFallback: true,
        proxy: {
            '/api': {
                target: `http://${host}/`     /* specify your correct IIS port here */
            },
            '/devicesHub': {
                target: `ws://${host}/`,     /* specify your correct IIS port here #1# */
                ws: true
            }
        },
        port: webpackDevServerPort
    },
    stats: {
        warnings: true
    }
});