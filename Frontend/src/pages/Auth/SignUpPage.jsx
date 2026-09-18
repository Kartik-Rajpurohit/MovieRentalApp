import { useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { AutoComplete } from "primereact/autocomplete";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { AuthContext } from "../../context/AuthContext";
import { signUpUser, getAddressAutocomplete } from "../../services/authService";
import { getErrorMessage } from "../../utils/errorUtils";

const labelStyle = { display: "block", marginBottom: "6px", fontWeight: 500 };

// Provides user registration with personal info, global Geoapify address autocomplete, and editable address inputs
export default function SignUpPage() {
  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  // User credentials and identity state
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  // Address search and autocomplete state
  const [addressQuery, setAddressQuery] = useState("");
  const [addressSuggestions, setAddressSuggestions] = useState([]);
  const [isSearchingAddress, setIsSearchingAddress] = useState(false);

  // Auto-populated and editable address fields
  const [street, setStreet] = useState("");
  const [city, setCity] = useState("");
  const [country, setCountry] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [phone, setPhone] = useState("");

  // Form states
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  // ─── Address Autocomplete Handlers ──────────────────────────────────────────

  const searchAddress = async (event) => {
    const query = event.query?.trim();
    if (!query || query.length < 3) {
      setAddressSuggestions([]);
      return;
    }

    setIsSearchingAddress(true);
    try {
      const results = await getAddressAutocomplete(query);
      setAddressSuggestions(results || []);
    } catch (err) {
      console.error("Address autocomplete error:", err);
      setAddressSuggestions([]);
    } finally {
      setIsSearchingAddress(false);
    }
  };

  const handleSelectAddress = (selected) => {
    if (!selected) return;

    // Combine houseNumber and street (e.g. "12 Residency Road")
    const streetValue = selected.houseNumber
      ? `${selected.houseNumber} ${selected.street || ""}`.trim()
      : selected.street || "";

    setStreet(streetValue.slice(0, 50));
    setCity((selected.city || "").slice(0, 50));
    setCountry((selected.country || "").slice(0, 50));
    setPostalCode((selected.postalCode || "").slice(0, 10));

    // Display formatted address in search bar
    setAddressQuery(selected.formattedAddress || streetValue);

    // Clear field-level validation errors upon auto-fill
    setErrors((prev) => ({
      ...prev,
      street: undefined,
      city: undefined,
      country: undefined,
      postalCode: undefined,
    }));
  };

  const addressItemTemplate = (item) => {
    const title = item.houseNumber
      ? `${item.houseNumber} ${item.street || ""}`.trim()
      : item.street || item.city || item.country || item.formattedAddress;

    return (
      <div style={{ padding: "4px 0", maxWidth: "420px" }}>
        <div style={{ fontWeight: 600, fontSize: "14px", color: "#1f2937" }}>
          {title}
        </div>
        {item.formattedAddress && (
          <div
            style={{
              fontSize: "12px",
              color: "#6b7280",
              marginTop: "2px",
              whiteSpace: "normal",
              wordBreak: "break-word",
            }}
          >
            {item.formattedAddress}
          </div>
        )}
      </div>
    );
  };

  // ─── Validation ────────────────────────────────────────────────────────────

  const validate = () => {
    const e = {};
    if (!firstName.trim()) {
      e.firstName = "First name is required";
    } else if (firstName.trim().length > 50) {
      e.firstName = "First name must not exceed 50 characters";
    }

    if (!lastName.trim()) {
      e.lastName = "Last name is required";
    } else if (lastName.trim().length > 50) {
      e.lastName = "Last name must not exceed 50 characters";
    }

    if (!email.trim()) {
      e.email = "Email is required";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      e.email = "Invalid email format";
    } else if (email.trim().length > 255) {
      e.email = "Email must not exceed 255 characters";
    }

    if (!password.trim()) {
      e.password = "Password is required";
    } else if (password.length < 8) {
      e.password = "Password must be at least 8 characters";
    } else if (password.length > 100) {
      e.password = "Password must not exceed 100 characters";
    } else if (!/[A-Z]/.test(password)) {
      e.password = "Password must contain at least one uppercase letter";
    } else if (!/[0-9]/.test(password)) {
      e.password = "Password must contain at least one number";
    } else if (!/[^A-Za-z0-9]/.test(password)) {
      e.password = "Password must contain at least one special character";
    }

    if (!country.trim()) {
      e.country = "Country is required";
    } else if (country.trim().length > 50) {
      e.country = "Country must not exceed 50 characters";
    }

    if (!city.trim()) {
      e.city = "City is required";
    } else if (city.trim().length > 50) {
      e.city = "City must not exceed 50 characters";
    }

    if (!street.trim()) {
      e.street = "Street address is required";
    } else if (street.trim().length > 50) {
      e.street = "Street address must not exceed 50 characters";
    }

    if (postalCode && postalCode.trim().length > 10) {
      e.postalCode = "Postal code must not exceed 10 characters";
    }

    if (!phone.trim()) {
      e.phone = "Phone number is required";
    } else if (phone.trim().length > 20) {
      e.phone = "Phone number must not exceed 20 characters";
    }

    return e;
  };

  // ─── Submit ────────────────────────────────────────────────────────────────

  const handleSignUp = async () => {
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);
    try {
      const payload = {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        password,
        country: country.trim(),
        city: city.trim(),
        street: street.trim(),
        postalCode: postalCode.trim() || null,
        phone: phone.trim(),
      };

      const response = await signUpUser(payload);
      login(response);
      const hasRole = response?.role && response.role !== "Unassigned";
      navigate(hasRole ? "/dashboard" : "/home");
    } catch (err) {
      setErrors({ submit: getErrorMessage(err, "Sign up failed") });
    } finally {
      setLoading(false);
    }
  };

  // ─── Render ────────────────────────────────────────────────────────────────

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "100vh",
        background: "#f5f6fa",
        padding: "24px 0",
      }}
    >
      <Card
        style={{
          width: "480px",
          maxWidth: "95vw",
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
        }}
      >
        {/* Header */}
        <div style={{ textAlign: "center", marginBottom: "24px" }}>
          <i
            className="pi pi-video"
            style={{ fontSize: "2.5rem", color: "#6366f1" }}
          />
          <h1
            style={{
              margin: "12px 0 0 0",
              color: "#111827",
              fontSize: "1.75rem",
            }}
          >
            Movie Rental
          </h1>
          <p style={{ margin: "4px 0 0 0", color: "#6b7280" }}>
            Create Account
          </p>
        </div>

        {errors.submit && (
          <Message
            severity="error"
            text={errors.submit}
            style={{ marginBottom: "16px", width: "100%" }}
          />
        )}

        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          {/* First Name */}
          <div>
            <label htmlFor="signup-firstname" style={labelStyle}>
              First Name
            </label>
            <InputText
              id="signup-firstname"
              value={firstName}
              onChange={(e) => {
                setFirstName(e.target.value);
                setErrors((prev) => ({ ...prev, firstName: undefined }));
              }}
              placeholder="First name"
              maxLength={50}
              style={{ width: "100%" }}
              className={errors.firstName ? "p-invalid" : ""}
            />
            {errors.firstName && (
              <small className="p-error">{errors.firstName}</small>
            )}
          </div>

          {/* Last Name */}
          <div>
            <label htmlFor="signup-lastname" style={labelStyle}>
              Last Name
            </label>
            <InputText
              id="signup-lastname"
              value={lastName}
              onChange={(e) => {
                setLastName(e.target.value);
                setErrors((prev) => ({ ...prev, lastName: undefined }));
              }}
              placeholder="Last name"
              maxLength={50}
              style={{ width: "100%" }}
              className={errors.lastName ? "p-invalid" : ""}
            />
            {errors.lastName && (
              <small className="p-error">{errors.lastName}</small>
            )}
          </div>

          {/* Email */}
          <div>
            <label htmlFor="signup-email" style={labelStyle}>
              Email
            </label>
            <InputText
              id="signup-email"
              value={email}
              onChange={(e) => {
                setEmail(e.target.value);
                setErrors((prev) => ({ ...prev, email: undefined }));
              }}
              placeholder="Enter your email"
              type="email"
              maxLength={255}
              style={{ width: "100%" }}
              className={errors.email ? "p-invalid" : ""}
            />
            {errors.email && <small className="p-error">{errors.email}</small>}
          </div>

          {/* Password */}
          <div>
            <label htmlFor="signup-password" style={labelStyle}>
              Password
            </label>
            <Password
              inputId="signup-password"
              value={password}
              onChange={(e) => {
                setPassword(e.target.value);
                setErrors((prev) => ({ ...prev, password: undefined }));
              }}
              placeholder="Create a password"
              toggleMask
              feedback={false}
              maxLength={100}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.password ? "p-invalid" : ""}
            />
            {errors.password ? (
              <small className="p-error">{errors.password}</small>
            ) : (
              <small style={{ color: "#6b7280" }}>
                Min 8 characters, with at least 1 uppercase, 1 number, and 1
                special character
              </small>
            )}
          </div>

          {/* Divider */}
          <div style={{ borderTop: "1px solid #e5e7eb", paddingTop: "8px" }}>
            <p
              style={{
                margin: "0 0 4px 0",
                fontSize: "14px",
                fontWeight: 600,
                color: "#374151",
              }}
            >
              Address Details
            </p>
            <small style={{ color: "#6b7280" }}>
              Search for your address to automatically fill the fields below.
            </small>
          </div>

          {/* Address Autocomplete Search */}
          <div>
            <label htmlFor="signup-address-search" style={labelStyle}>
              Search Address
            </label>
            <AutoComplete
              id="signup-address-search"
              value={addressQuery}
              suggestions={addressSuggestions}
              completeMethod={searchAddress}
              field="formattedAddress"
              itemTemplate={addressItemTemplate}
              delay={400}
              minLength={3}
              placeholder="Enter 3 character minimum (e.g. 123 Main St, London)..."
              emptyMessage={
                isSearchingAddress ? "Searching..." : "No addresses found"
              }
              onChange={(e) => {
                const val =
                  typeof e.value === "string"
                    ? e.value
                    : e.value?.formattedAddress || "";
                setAddressQuery(val);
              }}
              onSelect={(e) => handleSelectAddress(e.value)}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
            />
          </div>

          {/* Street Address */}
          <div>
            <label htmlFor="signup-street" style={labelStyle}>
              Street Address
            </label>
            <InputText
              id="signup-street"
              value={street}
              onChange={(e) => {
                setStreet(e.target.value);
                setErrors((prev) => ({ ...prev, street: undefined }));
              }}
              placeholder="Street address"
              maxLength={50}
              style={{ width: "100%" }}
              className={errors.street ? "p-invalid" : ""}
            />
            {errors.street && (
              <small className="p-error">{errors.street}</small>
            )}
          </div>

          {/* City */}
          <div>
            <label htmlFor="signup-city" style={labelStyle}>
              City
            </label>
            <InputText
              id="signup-city"
              value={city}
              onChange={(e) => {
                setCity(e.target.value);
                setErrors((prev) => ({ ...prev, city: undefined }));
              }}
              placeholder="City"
              maxLength={50}
              style={{ width: "100%" }}
              className={errors.city ? "p-invalid" : ""}
            />
            {errors.city && <small className="p-error">{errors.city}</small>}
          </div>

          {/* Country */}
          <div>
            <label htmlFor="signup-country" style={labelStyle}>
              Country
            </label>
            <InputText
              id="signup-country"
              value={country}
              onChange={(e) => {
                setCountry(e.target.value);
                setErrors((prev) => ({ ...prev, country: undefined }));
              }}
              placeholder="Country"
              maxLength={50}
              style={{ width: "100%" }}
              className={errors.country ? "p-invalid" : ""}
            />
            {errors.country && (
              <small className="p-error">{errors.country}</small>
            )}
          </div>

          {/* Postal Code */}
          <div>
            <label htmlFor="signup-postalcode" style={labelStyle}>
              Postal Code{" "}
              <span style={{ color: "#9ca3af", fontWeight: 400 }}>
                (optional)
              </span>
            </label>
            <InputText
              id="signup-postalcode"
              value={postalCode}
              onChange={(e) => {
                setPostalCode(e.target.value);
                setErrors((prev) => ({ ...prev, postalCode: undefined }));
              }}
              placeholder="Postal code"
              maxLength={10}
              style={{ width: "100%" }}
              className={errors.postalCode ? "p-invalid" : ""}
            />
            {errors.postalCode && (
              <small className="p-error">{errors.postalCode}</small>
            )}
          </div>

          {/* Phone */}
          <div>
            <label htmlFor="signup-phone" style={labelStyle}>
              Phone
            </label>
            <InputText
              id="signup-phone"
              value={phone}
              onChange={(e) => {
                setPhone(e.target.value);
                setErrors((prev) => ({ ...prev, phone: undefined }));
              }}
              placeholder="Phone number"
              maxLength={20}
              style={{ width: "100%" }}
              className={errors.phone ? "p-invalid" : ""}
            />
            {errors.phone && <small className="p-error">{errors.phone}</small>}
          </div>

          <Button
            id="signup-submit-btn"
            label="Sign Up"
            onClick={handleSignUp}
            loading={loading}
            style={{ width: "100%", marginTop: "8px" }}
          />

          <div
            style={{ textAlign: "center", fontSize: "14px", color: "#6b7280" }}
          >
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
