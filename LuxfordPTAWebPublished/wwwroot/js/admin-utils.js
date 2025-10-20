// PTA Tech Note: JavaScript utilities for file downloads and other admin functions
// This file supports the backup administration interface

window.downloadFile = (filePath, fileName) => {
    const link = document.createElement('a');
    link.href = `/backups/${fileName}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Additional utility functions for PTA admin features
window.ptaUtils = {
    // Confirm deletion with custom message
    confirmDelete: (itemName) => {
        return confirm(`Are you sure you want to delete "${itemName}"? This action cannot be undone.`);
    },
    
    // Show success toast notification
    showSuccess: (message) => {
        // You can integrate this with Bootstrap toast or other notification systems
        console.log('Success:', message);
    },
    
    // Show error toast notification
    showError: (message) => {
        // You can integrate this with Bootstrap toast or other notification systems
        console.error('Error:', message);
    }
};