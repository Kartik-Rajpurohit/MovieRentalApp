import { IconField } from "primereact/iconfield";
import { InputIcon } from "primereact/inputicon";
import { InputText } from "primereact/inputtext";

export default function SearchBar({
  value,
  onChange,
  placeholder = "Search...",
}) {
  return (
    <IconField iconPosition="left" style={{ width: "280px" }}>
      <InputIcon className="pi pi-search" />
      <InputText
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        style={{ width: "100%" }}
      />
    </IconField>
  );
}
