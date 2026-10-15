import { useNavigate } from "react-router-dom";
import { confirmDialog } from "primereact/confirmdialog";
import { useToastContext } from "../context/ToastContext";
import { getErrorMessage } from "../utils/errorUtils";

// Custom hook providing standardized entity deletion confirmation, error boundary, toast alerts, and navigation
export default function useDeleteEntity({
  deleteFn,
  redirectPath,
  entityName = "Item",
  successMessage,
}) {
  const navigate = useNavigate();
  const { showSuccess, showError } = useToastContext();

  const confirmDelete = (id, displayName, customMessage) => {
    const name = displayName ?? id;
    confirmDialog({
      header: `Delete ${entityName}`,
      message:
        customMessage ??
        `Are you sure you want to delete ${entityName.toLowerCase()} "${name}"?`,
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      acceptLabel: "Delete",
      rejectLabel: "Cancel",
      accept: async () => {
        try {
          await deleteFn(id);
          showSuccess(
            successMessage ?? `${entityName} deleted successfully.`
          );
          if (redirectPath) {
            navigate(redirectPath);
          }
        } catch (err) {
          showError(
            getErrorMessage(err, `Failed to delete ${entityName.toLowerCase()}.`)
          );
        }
      },
    });
  };

  return { confirmDelete };
}
