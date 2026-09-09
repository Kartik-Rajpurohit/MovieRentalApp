// Provides global toast notification alerts (success, error, warning, info) across the application
import { createContext, useContext, useRef, useEffect } from "react";
import { Toast } from "primereact/toast";
import { registerToastNotifier } from "../services/api";

// Context for accessing toast notification triggers
export const ToastContext = createContext(null);

// Provider component that manages the PrimeReact Toast instance and exposes toast helpers
export function ToastProvider({ children }) {
  const toastRef = useRef(null);

  // Connects the toast component to the Axios client interceptor to show automatic API error toasts
  useEffect(() => {
    const unregister = registerToastNotifier((config) => {
      toastRef.current?.show(config);
    });
    return unregister;
  }, []);

  // Generic function to display a customizable toast message
  const showToast = ({ severity = "info", summary = "Notification", detail = "", life = 4000 }) => {
    toastRef.current?.show({ severity, summary, detail, life });
  };

  // Displays a green success alert toast
  const showSuccess = (detail, summary = "Success") => {
    showToast({ severity: "success", summary, detail });
  };

  // Displays a red error alert toast
  const showError = (detail, summary = "Error") => {
    showToast({ severity: "error", summary, detail });
  };

  // Displays a yellow warning alert toast
  const showWarn = (detail, summary = "Warning") => {
    showToast({ severity: "warn", summary, detail });
  };

  // Displays a blue informational alert toast
  const showInfo = (detail, summary = "Information") => {
    showToast({ severity: "info", summary, detail });
  };

  return (
    <ToastContext.Provider value={{ showToast, showSuccess, showError, showWarn, showInfo }}>
      <Toast ref={toastRef} position="top-right" />
      {children}
    </ToastContext.Provider>
  );
}

// Custom hook allowing any component to easily trigger toast notifications
export function useToastContext() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToastContext must be used within a ToastProvider");
  }
  return context;
}
