// Import/Export JavaScript Interop Functions

/**
 * Triggers a click on a file input element
 */
window.clickFileInput = () => {
    const fileInput = document.querySelector('input[type="file"]');
    if (fileInput) {
        fileInput.click();
    }
};

/**
 * Downloads a file from base64 data
 * @param {string} fileName - The name for the downloaded file
 * @param {string} base64Data - The file content as base64
 * @param {string} mimeType - The MIME type of the file
 */
window.downloadFileFromBase64 = (fileName, base64Data, mimeType) => {
    // Convert base64 to blob
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    // Create download link
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;

    // Trigger download
    document.body.appendChild(link);
    link.click();

    // Cleanup
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

/**
 * Downloads a file from byte array
 * @param {string} fileName - The name for the downloaded file
 * @param {Uint8Array} data - The file content as byte array
 * @param {string} mimeType - The MIME type of the file
 */
window.downloadFile = (fileName, data, mimeType) => {
    const blob = new Blob([data], { type: mimeType || 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;

    document.body.appendChild(link);
    link.click();

    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};
