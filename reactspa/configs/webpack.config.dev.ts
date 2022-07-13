import 'webpack-dev-server';
import baseConfig from './webpack.config.base';
import webpack from 'webpack';
import path from 'path';
import merge from 'webpack-merge';
import checkNodeEnv from '../scripts/check-node-env';
import ReactRefreshWebpackPlugin from '@pmmmwh/react-refresh-webpack-plugin';
import HtmlWebpackPlugin from 'html-webpack-plugin';
import webpackPaths from './webpack.paths';
import chalk from 'chalk';
import paths from './webpack.paths'
import prepareProxy from '../scripts/WebpackDevServerUtils';

if (process.env.NODE_ENV === 'production') {
  checkNodeEnv('development');
}

const port = process.env.PORT || 3001;
const proxySetting=require(paths.packagePath).proxy;
const proxyConfig=prepareProxy(
  proxySetting,
  paths.appPublic,
  paths.publicUrlOrPath
)

const proxy=proxyConfig;

const configuration: webpack.Configuration = {
  devtool: 'inline-source-map',

  mode: 'development',

  target: ['web'],

  entry: [
    `webpack-dev-server/client?http://localhost:${port}/dist`,
    'webpack/hot/only-dev-server',
    path.join(webpackPaths.srcPath, 'index.tsx'),
  ],

  output: {
    path: webpackPaths.distPath,
    publicPath: '/',
    filename: 'index.dev.js',
    library: {
      type: 'umd',
    },
  },

  module: {
    rules: [
      {
        test: /\.s?css$/,
        use: [
          'style-loader',
          {
            loader: 'css-loader',
            options: {
              module: true,
              sourceMap: true,
              importLoaders: 1,
            },
          },
          'sass-loader',
        ],
        include: /\.module\.s?(c|a)ss$/,
      },
      {
        test: /\.s?css$/,
        use: ['style-loader', 'css-loader', 'sass-loader'],
        exclude: /\.module\.s?(c|a)ss$/,
      },
      {
        test: /\.(woff|woff2|eot|ttf|otf)$/i,
        type: 'asset/resource',
      },
      {
        test: /\.(png|svg|jpg|jpeg|gif)$/,
        type: 'asset/resource',
      },
    ],
  },
  plugins: [
    new webpack.NoEmitOnErrorsPlugin(),
    new webpack.EnvironmentPlugin({
      NODE_ENV: 'development',
    }),
    new webpack.LoaderOptionsPlugin({
      debug: true,
    }),
    new ReactRefreshWebpackPlugin(),
    new HtmlWebpackPlugin({
      filename: path.join('index.html'),
      template: path.join(webpackPaths.rootPath, '/public/index.html'),
      minify: {
        collapseWhitespace: true,
        removeAttributeQuotes: true,
        removeComments: true,
      },
      isBrowser: false,
      env: process.env.NODE_ENV,
      isDevelopment: process.env.NODE_ENV !== 'production',
      nodeModules: path.join(__dirname, '../dist/node_modules'),
    }),
  ],

  node: {
    __dirname: false,
    __filename: false,
  },

  devServer: {
    port,
    headers:{
      'Access-Control-Allow-Origin':'*',
      'Access-Control-Allow-Methods':'*',
      'Access-Control-Allow-Headers':'*'
    },
    compress: true,
    hot: true,
    static: {
      directory:paths.appPublic,
      publicPath: paths.publicUrlOrPath,
    },
    historyApiFallback: {
      verbose: true,
      disableDotRule: true,
    },
    proxy:proxyConfig,
    setupMiddlewares(middlewares) {
      console.log(
        chalk.blueBright.bold(
          `Starting webpack dev server at http://localhost:${port}...`
        )
      );
      return middlewares;
    },
  },
};

export default merge(baseConfig, configuration);
