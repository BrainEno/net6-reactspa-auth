import fs from 'fs';
import path from "path";
import getPublicUrlOrPath from '../scripts/getPublicUrlOrPath';

const appDirectory=fs.realpathSync(process.cwd());
const resolveApp=(relativePath:string)=>path.resolve(appDirectory,relativePath);

const publicUrlOrPath=getPublicUrlOrPath(
  process.env.NODE_ENV==='development',
  require(resolveApp('package.json')).homepage,
  process.env.PUBLIC_URL
)

const rootPath=appDirectory;

const appPublic=resolveApp('public');
const srcPath = path.join(rootPath, "src");
const packagePath = path.join(rootPath, "package.json");

const distPath = path.join(rootPath, "dist");
const buildPath = path.join(rootPath, "build");
const nodeModulesPath = path.join(rootPath, "node_modules");
export default {
  rootPath,
  srcPath,
  packagePath,
  appPublic,
  distPath,
  buildPath,
  nodeModulesPath,
  publicUrlOrPath
};
