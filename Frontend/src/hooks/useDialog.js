// Custom hook providing open and close state for modal dialogs and popup forms
import { useState } from "react";

export default function useDialog() {
  // Boolean indicating whether the modal dialog is currently open
  const [visible, setVisible] = useState(false);

  // Opens the dialog
  const open = () => setVisible(true);

  // Closes the dialog
  const close = () => setVisible(false);

  // Return dialog visibility state and control functions
  return { visible, open, close };
}
