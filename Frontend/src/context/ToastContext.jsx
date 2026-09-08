import { createContext, useContext, useRef, useEffect } from "react";
import { Toast } from "primereact/toast";
import { registerToastNotifier } from "../services/api";

export const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const toastRef = useRef(null);

  useEffect(() => {
    // Register toast handler with Axios API interceptor for automatic global error toasts
    const unregister = registerToastNotifier((config) => {
      toastRef.current?.show(config);
    });
    return unregister;
  }, []);

  const showToast = ({ severity = "info", summary = "Notification", detail = "", life = 4000 }) => {
    toastRef.current?.show({ severity, summary, detail, life });
  };

  const showSuccess = (detail, summary = "Success") => {
    showToast({ severity: "success", summary, detail });
  };

  const showError = (detail, summary = "Error") => {
    showToast({ severity: "error", summary, detail });
  };

  const showWarn = (detail, summary = "Warning") => {
    showToast({ severity: "warn", summary, detail });
  };

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

export function useToastContext() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToastContext must be used within a ToastProvider");
  }
  return context;
}
