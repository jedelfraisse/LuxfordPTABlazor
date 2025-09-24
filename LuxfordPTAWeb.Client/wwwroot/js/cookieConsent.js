// Cookie consent management functions

export function getConsent(key) {
    return localStorage.getItem(key);
}

export function setConsent(key, value, expiryDays) {
    localStorage.setItem(key, value);
    
    // Also set a timestamp for expiry tracking
    const expiryDate = new Date();
    expiryDate.setDate(expiryDate.getDate() + expiryDays);
    localStorage.setItem(key + '_expiry', expiryDate.toISOString());
}

export function removeConsent(key) {
    localStorage.removeItem(key);
    localStorage.removeItem(key + '_expiry');
}

export function isConsentExpired(key) {
    const expiryStr = localStorage.getItem(key + '_expiry');
    if (!expiryStr) return true;
    
    const expiry = new Date(expiryStr);
    return new Date() > expiry;
}