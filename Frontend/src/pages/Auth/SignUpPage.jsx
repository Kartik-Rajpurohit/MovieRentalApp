import { useState, useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Dropdown } from "primereact/dropdown";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { AuthContext } from "../../context/AuthContext";
import {
  signUpUser,
  getLookupCountries,
  getLookupCities,
} from "../../services/authService";
import { getErrorMessage } from "../../utils/errorUtils";

const labelStyle = { display: "block", marginBottom: "6px", fontWeight: 500 };

// Provides user registration with personal info, CountriesNow reference country/city selection, and address inputs
export default function SignUpPage() {
  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  // User credentials and identity state
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  // Location reference dropdown states
  const [countries, setCountries] = useState([]);
  const [countriesLoading, setCountriesLoading] = useState(false);
  const [selectedCountry, setSelectedCountry] = useState("");

  const [cities, setCities] = useState([]);
  const [citiesLoading, setCitiesLoading] = useState(false);
  const [selectedCity, setSelectedCity] = useState("");

  // Address text inputs
  const [street, setStreet] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [phone, setPhone] = useState("");

  // Form states
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});
  const [lookupError, setLookupError] = useState(null);

  // Load country reference list on component mount
  useEffect(() => {
    fetchCountries();
  }, []);

  const fetchCountries = async () => {
    setCountriesLoading(true);
    setLookupError(null);
    try {
      const data = await getLookupCountries();
      const mapped = data.map((c) => ({ label: c.name, value: c.name }));
      setCountries(mapped);
    } catch (err) {
      console.error("Failed to load countries:", err);
      setLookupError("Unable to load countries. Please try again.");
    } finally {
      setCountriesLoading(false);
    }
  };

  const fetchCities = async (countryName) => {
    if (!countryName) {
      setCities([]);
      return;
    }
    setCitiesLoading(true);
    setLookupError(null);
    try {
      const data = await getLookupCities(countryName);
      const mapped = data.map((c) => ({ label: c.name, value: c.name }));
      setCities(mapped);
    } catch (err) {
      console.error(`Failed to load cities for ${countryName}:`, err);
      setLookupError("Unable to load cities. Please try again.");
      setCities([]);
    } finally {
      setCitiesLoading(false);
    }
  };

  // When country changes, immediately reset selected city, clear city options, and fetch cities for new country
  const handleCountryChange = (countryName) => {
    setSelectedCountry(countryName || "");
    setSelectedCity("");
    setCities([]);
    setErrors((prev) => ({ ...prev, country: undefined, city: undefined }));

    if (countryName) {
      fetchCities(countryName);
    }
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

    if (!selectedCountry.trim()) {
      e.country = "Country is required";
    } else if (selectedCountry.trim().length > 50) {
      e.country = "Country must not exceed 50 characters";
    }

    if (!selectedCity.trim()) {
      e.city = "City is required";
    } else if (selectedCity.trim().length > 50) {
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
        country: selectedCountry.trim(),
        city: selectedCity.trim(),
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

        {lookupError && (
          <Message
            severity="warn"
            text={lookupError}
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
                Min 8 characters, with at least 1 uppercase, 1 number, and 1 special character
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
          </div>

          {/* Country Dropdown */}
          <div>
            <label htmlFor="signup-country" style={labelStyle}>
              Country
            </label>
            <Dropdown
              inputId="signup-country"
              value={selectedCountry}
              options={countries}
              onChange={(e) => handleCountryChange(e.value)}
              placeholder={countriesLoading ? "Loading countries..." : "Select Country"}
              filter
              showClear={!!selectedCountry}
              disabled={countriesLoading}
              style={{ width: "100%" }}
              className={errors.country ? "p-invalid" : ""}
            />
            {errors.country && (
              <small className="p-error">{errors.country}</small>
            )}
          </div>

          {/* City Dropdown */}
          <div>
            <label htmlFor="signup-city" style={labelStyle}>
              City
            </label>
            <Dropdown
              inputId="signup-city"
              value={selectedCity}
              options={cities}
              onChange={(e) => {
                setSelectedCity(e.value || "");
                setErrors((prev) => ({ ...prev, city: undefined }));
              }}
              placeholder={
                !selectedCountry
                  ? "Select country first"
                  : citiesLoading
                    ? "Loading cities..."
                    : "Select City"
              }
              filter
              showClear={!!selectedCity}
              disabled={!selectedCountry || citiesLoading}
              style={{ width: "100%" }}
              className={errors.city ? "p-invalid" : ""}
            />
            {errors.city && <small className="p-error">{errors.city}</small>}
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
