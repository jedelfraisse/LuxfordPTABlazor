// Google Analytics 4 integration with consent management

let isInitialized = false;
let measurementId = '';

export function initializeGA(measurementIdParam, debug = false) {
    if (isInitialized) return;
    
    measurementId = measurementIdParam;
    
    // Use Google's standard installation format
    // Load gtag script - Google's recommended way
    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${measurementId}`;
    document.head.appendChild(script);
    
    // Standard Google Analytics initialization - exactly as Google recommends
    window.dataLayer = window.dataLayer || [];
    function gtag() {
        dataLayer.push(arguments);
    }
    window.gtag = gtag;
    window.GA_MEASUREMENT_ID = measurementId; // Store globally for reference
    
    gtag('js', new Date());
    
    // For opt-out approach: Set consent to granted by default, then configure GA
    gtag('consent', 'default', {
        'analytics_storage': 'granted', // Opt-out approach - granted by default
        'ad_storage': 'denied',
        'functionality_storage': 'denied',
        'personalization_storage': 'denied',
        'security_storage': 'granted'
    });
    
    // Standard Google Analytics config - Google's recommended format
    gtag('config', measurementId, {
        'anonymize_ip': true,
        'allow_google_signals': false,
        'allow_ad_personalization_signals': false,
        'debug_mode': debug,
        'ads_data_redaction': true
    });
    
    isInitialized = true;
    
    if (debug) {
        console.log('Google Analytics 4 initialized with measurement ID:', measurementId);
        console.log('Standard Google Tag format used for maximum compatibility');
    }
}

export function grantAnalyticsConsent() {
    if (!isInitialized || !window.gtag) return;
    
    window.gtag('consent', 'update', {
        'analytics_storage': 'granted'
    });
    
    console.log('Analytics consent granted');
}

export function revokeAnalyticsConsent() {
    if (!isInitialized || !window.gtag) return;
    
    window.gtag('consent', 'update', {
        'analytics_storage': 'denied'
    });
    
    console.log('Analytics consent revoked');
}

export function trackPageView(pageName, pageTitle = '') {
    if (!isInitialized || !window.gtag) return;
    
    // Standard Google Analytics page_view event
    window.gtag('event', 'page_view', {
        'page_title': pageTitle || pageName,
        'page_location': window.location.href,
        'page_path': pageName
    });
}

export function trackEvent(eventName, eventData = {}) {
    if (!isInitialized || !window.gtag) return;
    
    // Standard Google Analytics custom event
    window.gtag('event', eventName, eventData);
}

export function setUserId(userId) {
    if (!isInitialized || !window.gtag || !measurementId) return;
    
    // Standard Google Analytics user ID setting
    window.gtag('config', measurementId, {
        'user_id': userId
    });
}