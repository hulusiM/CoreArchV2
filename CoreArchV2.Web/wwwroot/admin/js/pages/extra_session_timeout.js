/* ------------------------------------------------------------------------------
*
*  # Idle timeout
*
*  Specific JS code additions for extra_idle_timeout.html page
*
*  Version: 1.0
*  Latest update: Aug 1, 2015
*
* ---------------------------------------------------------------------------- */

$(function() {

    // Session timeout
    $.sessionTimeout({
        heading: 'h5',
        title: 'Oturum Zaman Aþýmý Bildirimi',
        message: 'Oturumunuzun süresi dolmak üzere. Baðlý kalmak istiyor musunuz?',
        ignoreUserActivity: true,
        warnAfter: 5000,
        redirAfter: 8000,
        keepAliveUrl: '/',
        redirUrl: 'login',
        logoutUrl: 'login'
    });

});
