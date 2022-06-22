import path from "path";

const rootPath = path.join(__dirname, "../");

const srcPath = path.join(rootPath, "src");
const packagePath = path.join(rootPath, "package.json");

const distPath = path.join(rootPath, "dist");
const buildPath = path.join(rootPath, "build");
const nodeModulesPath = path.join(rootPath, "node_modules");

export default {
  rootPath,
  srcPath,
  packagePath,
  distPath,
  buildPath,
  nodeModulesPath,
};
