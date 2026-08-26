import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { useContext } from "react";
import { AuthContext } from "../../context/AuthContext";
import { loginUser } from "../../services/authService";

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleLogin = async () => {
    setError("");
    setLoading(true);

    try {
      const response = await loginUser(email, password);
      login(response);
      navigate("/dashboard");
    } catch (err) {
      setError(err.response?.data || "Login failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        background: "#f5f6fa",
      }}
    >
      <Card style={{ width: "400px", boxShadow: "0 4px 6px rgba(0,0,0,0.1)" }}>
        <div style={{ textAlign: "center", marginBottom: "24px" }}>
          <i
            className="pi pi-video"
            style={{ fontSize: "2.5rem", color: "#6366f1" }}
          />
          <h1 style={{ margin: "12px 0 0 0", color: "#111827" }}>
            Movie Rental
          </h1>
          <p style={{ margin: "4px 0 0 0", color: "#6b7280" }}>Sign In</p>
        </div>

        {error && (
          <Message
            severity="error"
            text={error}
            style={{ marginBottom: "16px" }}
          />
        )}

        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          <div>
            <label
              style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}
            >
              Email
            </label>
            <InputText
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Enter your email"
              type="email"
              style={{ width: "100%" }}
              onKeyPress={(e) => e.key === "Enter" && handleLogin()}
            />
          </div>

          <div>
            <label
              style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}
            >
              Password
            </label>
            <Password
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter your password"
              toggleMask
              style={{ width: "100%" }}
              feedback={false}
              onKeyPress={(e) => e.key === "Enter" && handleLogin()}
            />
          </div>

          <Button
            label="Sign In"
            onClick={handleLogin}
            loading={loading}
            style={{ width: "100%" }}
          />

          <div
            style={{ textAlign: "center", fontSize: "14px", color: "#6b7280" }}
          >
            Don't have an account?{" "}
            <span
              onClick={() => navigate("/signup")}
              style={{ cursor: "pointer", color: "#6366f1", fontWeight: 600 }}
            >
              Sign Up
            </span>
          </div>
        </div>
      </Card>
    </div>
  );
}
