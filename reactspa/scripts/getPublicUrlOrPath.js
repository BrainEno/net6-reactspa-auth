const { URL } = require('url')

function getPublicUrlOrPath(isEnvDevelopment, homepage, envPublicUrl) {
    if (envPublicUrl) {
        envPublicUrl = envPublicUrl.endsWith('/')
            ? envPublicUrl
            : envPublicUrl + '/';

        const validPublicUrl = new URL(envPublicUrl)

        return isEnvDevelopment
            ? envPublicUrl.startsWith('.')
                ? '/'
                : validPublicUrl.pathname
            : envPublicUrl;
    }



    if (homepage) {
        // strip last slash if exists
        homepage = homepage.endsWith('/') ? homepage : homepage + '/';

        // validate if `homepage` is a URL or path like and use just pathname
        const validHomepagePathname = new URL(homepage, stubDomain).pathname;
        return isEnvDevelopment
            ? homepage.startsWith('.')
                ? '/'
                : validHomepagePathname
            : // Some apps do not use client-side routing with pushState.
            // For these, "homepage" can be set to "." to enable relative asset paths.
            homepage.startsWith('.')
                ? homepage
                : validHomepagePathname;
    }

    return '/';
}

module.exports=getPublicUrlOrPath;