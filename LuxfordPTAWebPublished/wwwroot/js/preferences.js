// Cookie consent management functions

export function getConsent(key) {
    // Console log for debugging
    console.log(`Retrieving consent for key: ${key}`);  
    return localStorage.getItem(key);
}

export function setConsent(key, value, expiryDays) {
    // Console log for debugging
    console.log(`Setting consent for key: ${key} with value: ${value} and expiry in days: ${expiryDays}`);
    localStorage.setItem(key, value);
    
    // Also set a timestamp for expiry tracking
    const expiryDate = new Date();
    expiryDate.setDate(expiryDate.getDate() + expiryDays);
    localStorage.setItem(key + '_expiry', expiryDate.toISOString());
}

export function removeConsent(key) {
    // Console log for debugging
    console.log(`Removing consent for key: ${key}`);
    localStorage.removeItem(key);
    localStorage.removeItem(key + '_expiry');
}

export function isConsentExpired(key) {
    // Console log for debugging
    console.log(`Checking if consent for key: ${key} is expired`);
    const expiryStr = localStorage.getItem(key + '_expiry');
    if (!expiryStr) return true;
    
    const expiry = new Date(expiryStr);
    return new Date() > expiry;
}