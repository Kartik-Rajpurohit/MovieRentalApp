// Manages the JWT Access Token strictly in application memory
let accessToken = null;
const listeners = new Set();

// Updates the in-memory access token and notifies active subscribers
export const setAccessToken = (token) => {
  accessToken = token;
  listeners.forEach((listener) => listener(token));
};

// Returns the current in-memory access token
export const getAccessToken = () => {
  return accessToken;
};

// Clears the in-memory access token and notifies active subscribers
export const clearAccessToken = () => {
  accessToken = null;
  listeners.forEach((listener) => listener(null));
};

// Registers a subscriber callback for token updates and returns an unsubscribe function
export const onAccessTokenChange = (listener) => {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
};
