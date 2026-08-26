import { Tag } from "primereact/tag";

export default function StatusTag({ isActive }) {
  return (
    <Tag
      value={isActive ? "Active" : "Inactive"}
      severity={isActive ? "success" : "danger"}
    />
  );
}
