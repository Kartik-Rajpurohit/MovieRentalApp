import { useEffect, useRef } from "react";
import { Dialog } from "primereact/dialog";

// Focusable selector according to WCAG 2.1 criteria
const FOCUSABLE_ELEMENTS_SELECTOR =
  'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';

// Reusable, WCAG-compliant modal wrapper with focus-trapping and Escape keyboard listener
export default function Modal({
  visible,
  onHide,
  header,
  footer,
  style = { width: "460px", maxWidth: "95vw" },
  children,
  className = "",
  ...props
}) {
  const dialogRef = useRef(null);
  const previousActiveElement = useRef(null);

  // 1. Listen for Escape key press when modal is open
  useEffect(() => {
    if (!visible) return;

    previousActiveElement.current = document.activeElement;

    const handleKeyDown = (e) => {
      if (e.key === "Escape") {
        e.stopPropagation();
        onHide();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => {
      window.removeEventListener("keydown", handleKeyDown);
      if (previousActiveElement.current && typeof previousActiveElement.current.focus === "function") {
        previousActiveElement.current.focus();
      }
    };
  }, [visible, onHide]);

  // 2. Trap focus inside modal container
  useEffect(() => {
    if (!visible) return;

    const trapFocus = (e) => {
      if (e.key !== "Tab") return;

      const container = document.querySelector(".p-dialog");
      if (!container) return;

      const focusable = container.querySelectorAll(FOCUSABLE_ELEMENTS_SELECTOR);
      if (focusable.length === 0) return;

      const firstElement = focusable[0];
      const lastElement = focusable[focusable.length - 1];

      if (e.shiftKey) {
        // Shift + Tab: moving backwards
        if (document.activeElement === firstElement) {
          e.preventDefault();
          lastElement.focus();
        }
      } else {
        // Tab: moving forwards
        if (document.activeElement === lastElement) {
          e.preventDefault();
          firstElement.focus();
        }
      }
    };

    window.addEventListener("keydown", trapFocus);
    return () => window.removeEventListener("keydown", trapFocus);
  }, [visible]);

  return (
    <Dialog
      ref={dialogRef}
      header={header}
      visible={visible}
      onHide={onHide}
      footer={footer}
      style={style}
      modal
      closeOnEscape
      focusOnShow
      dismissableMask
      className={className}
      {...props}
    >
      {children}
    </Dialog>
  );
}
