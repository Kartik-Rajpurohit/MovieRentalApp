export default function LoadingSpinner() {
  return (
    <div
      style={{
        display: "flex",
        justifyContent: "center",
        paddingTop: "60px",
      }}
    >
      <i
        className="pi pi-spin pi-spinner"
        style={{ fontSize: "2rem", color: "#6366f1" }}
      />
    </div>
  );
}
