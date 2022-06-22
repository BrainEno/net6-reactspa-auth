import path from "path";
import webpack from "webpack";
import HtmlWebpackPlugin from "html-webpack-plugin";
import MiniCssExtractPlugin from "mini-css-extract-plugin";
import { BundleAnalyzerPlugin } from "webpack-bundle-analyzer";
import CssMinimizerPlugin from "css-minimizer-webpack-plugin";
import { merge } from "webpack-merge";
import TerserPlugin from "terser-webpack-plugin";
import baseConfig from "./webpack.config.base";
import deleteSourceMaps from "../scripts/delete-source-maps";
import checkNodeEnv from "../scripts/check-node-env";
import webpackPaths from "./webpack.paths";

checkNodeEnv("production");
deleteSourceMaps();

const devtoolsConfig =
  process.env.DEBUG_PROD === "true"
    ? {
        devtool: "source-map",
      }
    : {};

const configuration: webpack.Configuration = {
  ...devtoolsConfig,

  mode: "production",

  target: ["web"],

  entry: [path.join(webpackPaths.srcPath, "index.tsx")],

  output: {
    path: webpackPaths.buildPath,
    publicPath: "/",
    filename: "[name].bundle.js",
    library: {
      type: "umd",
    },
    clean: true,
  },

  module: {
    rules: [
      {
        test: /\.s?(a|c)ss$/,
        use: [
          MiniCssExtractPlugin.loader,
          {
            loader: "css-loader",
            options: {
              modules: true,
              sourceMap: true,
              importLoaders: 1,
            },
          },
          "sass-loader",
        ],
        include: /\.module\.s?(c|a)ss$/,
      },
      {
        test: /\.s?(a|c)ss$/,
        use: [MiniCssExtractPlugin.loader, "css-loader", "sass-loader"],
        exclude: /\.module\.s?(c|a)ss$/,
      },
      // Fonts
      {
        test: /\.(woff|woff2|eot|ttf|otf)$/i,
        type: "asset/resource",
      },
      // Images
      {
        test: /\.(png|svg|jpg|jpeg|gif)$/i,
        type: "asset/resource",
      },
    ],
  },

  optimization: {
    minimize: true,
    minimizer: [
      new TerserPlugin({
        parallel: true,
      }),
      new CssMinimizerPlugin(),
    ],
  },

  plugins: [
    /**
     * Create global constants which can be configured at compile time.
     *
     * Useful for allowing different behaviour between development builds and
     * release builds
     *
     * NODE_ENV should be production so that modules do not perform certain
     * development checks
     */
    new webpack.EnvironmentPlugin({
      NODE_ENV: "production",
      DEBUG_PROD: false,
      ANALYZE: "true",
    }),

    new MiniCssExtractPlugin({
      filename: "style.css",
    }),

    new BundleAnalyzerPlugin({
      analyzerMode: process.env.ANALYZE === "true" ? "server" : "disabled",
    }),

    new HtmlWebpackPlugin({
      filename: "index.html",
      template: path.join(webpackPaths.rootPath, "/public/index.html"),
      minify: {
        collapseWhitespace: true,
        removeAttributeQuotes: true,
        removeComments: true,
      },
      isBrowser: false,
      isDevelopment: process.env.NODE_ENV !== "production",
    }),
  ],
};

export default merge(baseConfig, configuration);
