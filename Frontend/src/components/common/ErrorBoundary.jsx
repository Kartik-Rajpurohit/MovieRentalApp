import React from "react";
import { Button } from "primereact/button";
import { Card } from "primereact/card";

export default class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error) {
    return { hasError: true, error };
  }

  componentDidCatch(error, errorInfo) {
    console.error("ErrorBoundary caught an error:", error, errorInfo);
  }

  handleReload = () => {
    window.location.reload();
  };

  handleHome = () => {
    window.location.href = "/";
  };

  render() {
    if (this.state.hasError) {
      return (
        <div
          style={{
            minHeight: "100vh",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            background: "#f8fafc",
            padding: "24px",
          }}
        >
          <Card style={{ width: "100%", maxWidth: "500px", textAlign: "center", padding: "20px" }}>
            <div
              style={{
                width: "64px",
                height: "64px",
                borderRadius: "50%",
                background: "#fee2e2",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                margin: "0 auto 16px",
              }}
            >
              <i className="pi pi-exclamation-triangle" style={{ fontSize: "2rem", color: "#dc2626" }} />
            </div>

            <h2 style={{ margin: "0 0 8px 0", color: "#1e293b", fontSize: "22px", fontWeight: 600 }}>
              Something went wrong
            </h2>
            <p style={{ margin: "0 0 24px 0", color: "#64748b", fontSize: "14px" }}>
              An unexpected error occurred while rendering the page.
            </p>

            <div style={{ display: "flex", gap: "12px", justifyContent: "center" }}>
              <Button label="Reload Page" icon="pi pi-refresh" onClick={this.handleReload} />
              <Button label="Go to Home" icon="pi pi-home" severity="secondary" outlined onClick={this.handleHome} />
            </div>
          </Card>
        </div>
      );
    }

    return this.props.children;
  }
}
