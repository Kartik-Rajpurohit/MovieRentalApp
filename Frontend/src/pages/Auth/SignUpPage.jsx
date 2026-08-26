import { useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { AuthContext } from "../../context/AuthContext";
import { signUpUser } from "../../services/authService";

export default function SignUpPage() {
  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSignUp = async () => {
    setError("");

    if (!firstName.trim() || !lastName.trim() || !email.trim() || !password.trim()) {
      setError("All fields are required");
      return;
    }

    setLoading(true);
    try {
      const response = await signUpUser(firstName, lastName, email, password);
      login(response);
      // Redirect to dashboard after successful signup
      navigate("/dashboard");
    } catch (err) {
      setError(err.response?.data || "Sign up failed");
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
          <i className="pi pi-video" style={{ fontSize: "2.5rem", color: "#6366f1" }} />
          <h1 style={{ margin: "12px 0 0 0", color: "#111827" }}>Movie Rental</h1>
          <p style={{ margin: "4px 0 0 0", color: "#6b7280" }}>Create Account</p>
        </div>

        {error && (
          <Message severity="error" text={error} style={{ marginBottom: "16px" }} />
        )}

        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          <div>
            <label style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}>
              First Name
            </label>
            <InputText
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              placeholder="First name"
              style={{ width: "100%" }}
            />
          </div>

          <div>
            <label style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}>
              Last Name
            </label>
            <InputText
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              placeholder="Last name"
              style={{ width: "100%" }}
            />
          </div>

          <div>
            <label style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}>
              Email
            </label>
            <InputText
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Enter your email"
              type="email"
              style={{ width: "100%" }}
            />
          </div>

          <div>
            <label style={{ display: "block", marginBottom: "6px", fontWeight: 500 }}>
              Password
            </label>
            <Password
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Create a password"
              toggleMask
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
          </div>

          <Button
            label="Sign Up"
            onClick={handleSignUp}
            loading={loading}
            style={{ width: "100%" }}
          />

          <div style={{ textAlign: "center", fontSize: "14px", color: "#6b7280" }}>
            Already have an account?{" "}
            <span
              onClick={() => navigate("/login")}
              style={{ cursor: "pointer", color: "#6366f1", fontWeight: 600 }}
            >
              Sign In
            </span>
          </div>
        </div>
      </Card>
    </div>
  );
}
