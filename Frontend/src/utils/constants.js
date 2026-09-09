// Defines shared styling objects and dropdown options used across the frontend UI
export const LABEL_STYLE = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Dropdown options for filtering records by Active, Inactive, or All
export const STATUS_OPTIONS = [
  { label: "All", value: null },
  { label: "Active", value: true },
  { label: "Inactive", value: false },
];

// Styling for field labels displayed on detail inspection pages
export const FIELD_LABEL = {
  margin: "0 0 4px 0",
  fontSize: "13px",
  fontWeight: "500",
  color: "#6b7280",
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};

// Styling for field values displayed on detail inspection pages
export const FIELD_VALUE = {
  margin: 0,
  fontSize: "16px",
  color: "#111827",
  fontWeight: "400",
};
