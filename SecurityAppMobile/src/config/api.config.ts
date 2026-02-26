// API Configuration
// Configure your development environment settings here

/** Default app timezone (sent as X-Timezone to API). Use IANA id e.g. Asia/Kolkata for India. */
export const DEFAULT_APP_TIMEZONE = 'Asia/Kolkata';

export const API_CONFIG = {
  // Development settings
  DEVELOPMENT: {
    // Replace with your actual machine IP address
    // To find your IP: 
    // - Windows: ipconfig in command prompt
    // - Mac/Linux: ifconfig or ip addr in terminal
    // - Or use: http://localhost:5014 in your browser and check network tab
    BASE_URL: 'http://192.168.68.126:5286', // TODO: Replace with your actual IP
    TIMEOUT: 10000, // 10 seconds
  },
  
  // Production settings
  PRODUCTION: {
    BASE_URL: 'https://your-production-api.com', // TODO: Update with your production URL
    TIMEOUT: 15000, // 15 seconds
  },
  
  // Common headers for all requests
  HEADERS: {
    'Content-Type': 'application/json',
  },
};

// Helper function to get current environment config
export const getApiConfig = () => {
  if (__DEV__) {
    return API_CONFIG.DEVELOPMENT;
  }
  return API_CONFIG.PRODUCTION;
};

// Helper to get current base URL
export const getBaseUrl = () => {
  const config = getApiConfig();
  return config.BASE_URL;
};

// Environment info for debugging
export const getEnvironmentInfo = () => {
  return {
    environment: __DEV__ ? 'development' : 'production',
    baseUrl: getBaseUrl(),
    timestamp: new Date().toISOString(),
  };
};
