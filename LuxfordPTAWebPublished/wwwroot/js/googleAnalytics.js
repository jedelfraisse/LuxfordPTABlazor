// Google Analytics 4 integration with consent management

let isInitialized = false;
let measurementId = '';

export function initializeGA(measurementIdParam, debug = false) {
    if (isInitialized) return;
    
    measurementId = measurementIdParam;
    
    // Load gtag script
    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${measurementId}`;
    document.head.appendChild(script);
    
    // Initialize gtag
    window.dataLayer = window.dataLayer || [];
    function gtag() {
        dataLayer.push(arguments);
    }
    window.gtag = gtag;
    window.GA_MEASUREMENT_ID = measurementId; // Store globally for reference
    
    gtag('js', new Date());
    
    // Set default consent state (GRANTED by default for opt-out approach)
    gtag('consent', 'default', {
        'analytics_storage': 'granted', // Changed to granted for opt-out approach
        'ad_storage': 'denied',
        'functionality_storage': 'denied',
        'personalization_storage': 'denied',
        'security_storage': 'granted' // Required for basic functionality
    });
    
    // Configure GA4 with privacy settings
    gtag('config', measurementId, {
        // Privacy settings
        'anonymize_ip': true,
        'allow_google_signals': false,
        'allow_ad_personalization_signals': false,
        // Debug mode
        'debug_mode': debug,
        // Respect consent mode
        'ads_data_redaction': true
    });
    
    isInitialized = true;
    
    if (debug) {
        console.log('Google Analytics 4 initialized with measurement ID:', measurementId);
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
    
    window.gtag('event', 'page_view', {
        'page_title': pageTitle || pageName,
        'page_location': window.location.href,
        'page_path': pageName
    });
}

export function trackEvent(eventName, eventData = {}) {
    if (!isInitialized || !window.gtag) return;
    
    window.gtag('event', eventName, eventData);
}

export function setUserId(userId) {
    if (!isInitialized || !window.gtag || !measurementId) return;
    
    window.gtag('config', measurementId, {
        'user_id': userId
    });
}

export function disableGA(measurementIdParam) {
    if (!isInitialized) return;
    
    // Revoke consent
    revokeAnalyticsConsent();
    
    // Disable Google Analytics
    window['ga-disable-' + measurementIdParam] = true;
    
    console.log('Google Analytics disabled');
}