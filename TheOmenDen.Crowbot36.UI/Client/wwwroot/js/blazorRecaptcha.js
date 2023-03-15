function isRecaptchaLoaded(key) {
    try {
            grecaptcha.execute(key, {
                action: 'homepage'
            }).then(function (token) {
                return true;
            });
        return true;
    } catch (e) {
        return false;
    }
}

async function generateCaptchaToken(key, action) {
    return await grecaptcha.execute(key, {action: action})
    .then(function (token) {
        return token;
    });
}