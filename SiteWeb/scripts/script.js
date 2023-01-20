// Interface entre Elm et le CAPTCHA

let app;

/**
 * Le CAPTCHA est validé
 * 
 * @param {string} token le jeton de validation
 */
function captchaFilled(token) {
    app.ports.captchaFilled.send(token);
}

/**
 * Le CAPTCHA a expiré
 */
function captchaExpired() {
    app.ports.captchaExpired.send();
}

/**
 * Le CAPTCHA a rencontré une erreur
 */
function captchaError() {
    app.ports.captchaError.send();
}

/**
 * Le CAPTCHA a besoin d'être rechargé
 */
function reloadCaptcha() {
    grecaptcha.render("recaptcha", {
        "sitekey": "6LfnKFwiAAAAAPd-9GjoxlDOI36qmaFu8o-Fkuy8",
        "callback": "captchaFilled",
        "expired-callback": "captchaExpired",
        "error-callback": "captchaError",
    });
}

window.onload = () => {
    app = Elm.Main.init();

    app.ports.reloadCaptcha.subscribe(reloadCaptcha);
}