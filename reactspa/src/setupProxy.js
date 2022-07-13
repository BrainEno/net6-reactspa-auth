const {createProxyMiddleware}=require('http-proxy-middleware');
const {env}=require('process');

const target=env.ASPNETCORE_HTTPS_PORT?`https://localhost:${env.ASPNETCORE_HTTPS_PORT}`:
    env.ASPNETCORE_URLS?env.ASPNETCORE_URLS.split(';')[0]:'https://localhost:5002';

const context=[
    "/accounts"
];

const onError=(err,req,res,target)=>{
    console.error(`${err.message}`);
}

module.exports=function(app){
    console.log(app);
    const appProxy=createProxyMiddleware(context,{
        target,
        onError,
        secure:false,
        headers:{
            Connection:'Keep-Alive'
        },
        changeOrigin:true,
    })

    app.use(appProxy)
}