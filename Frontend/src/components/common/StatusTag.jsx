import { Tag } from "primereact/tag";

// Reusable status badge that displays green "Active" or red "Inactive" tag based on boolean flag
export default function StatusTag({ isActive }) {
  return (
    <Tag
      value={isActive ? "Active" : "Inactive"}
      severity={isActive ? "success" : "danger"}
    />
  );
}
