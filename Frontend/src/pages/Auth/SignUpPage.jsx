import { useState, useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Dropdown } from "primereact/dropdown";
import { AutoComplete } from "primereact/autocomplete";
import { Button } from "primereact/button";
import { Message } from "primereact/message";
import { AuthContext } from "../../context/AuthContext";
import { signUpUser } from "../../services/authService";
import {
  getCountries,
  getCitiesByCountry,
  getAddressesByCity,
} from "../../services/userService";

const labelStyle = { display: "block", marginBottom: "6px", fontWeight: 500 };

export default function SignUpPage() {
  const navigate = useNavigate();
  const { login } = useContext(AuthContext);

  // Basic fields
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  // Location dropdowns
  const [countries, setCountries] = useState([]);
  const [countriesPage, setCountriesPage] = useState(1);
  const [countriesHasMore, setCountriesHasMore] = useState(true);
  const [selectedCountryId, setSelectedCountryId] = useState(null);

  const [cities, setCities] = useState([]);
  const [citiesPage, setCitiesPage] = useState(1);
  const [citiesHasMore, setCitiesHasMore] = useState(true);
  const [citiesLoading, setCitiesLoading] = useState(false);
  const [selectedCityId, setSelectedCityId] = useState(null);

  // Address — autocomplete from DB + free text
  const [addressSuggestions, setAddressSuggestions] = useState([]);
  const [addressInput, setAddressInput] = useState(""); // what user typed
  const [selectedAddressId, setSelectedAddressId] = useState(null); // if user picked existing
  const [district, setDistrict] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [phone, setPhone] = useState("");

  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  // Load countries on mount
  useEffect(() => {
    fetchCountries(1);
  }, []);

  // ─── Fetchers ──────────────────────────────────────────────────────────────

  const fetchCountries = async (page = 1) => {
    const data = await getCountries(page, 10);
    const mapped = data.map((c) => ({ label: c.name, value: c.id }));
    setCountries((prev) => (page === 1 ? mapped : [...prev, ...mapped]));
    setCountriesHasMore(data.length === 10);
    setCountriesPage(page);
  };

  const fetchCities = async (countryId, page = 1) => {
    setCitiesLoading(true);
    try {
      const data = await getCitiesByCountry(countryId, page, 10);
      const mapped = data.map((c) => ({ label: c.name, value: c.id }));
      setCities((prev) => (page === 1 ? mapped : [...prev, ...mapped]));
      setCitiesHasMore(data.length === 10);
      setCitiesPage(page);
    } finally {
      setCitiesLoading(false);
    }
  };

  // AutoComplete — search existing addresses for selected city
  const searchAddresses = async (event) => {
    if (!selectedCityId) {
      setAddressSuggestions([]);
      return;
    }
    const data = await getAddressesByCity(selectedCityId, 1, 50);
    const query = event.query.toLowerCase();
    const filtered = data
      .filter((a) => a.name?.toLowerCase().includes(query))
      .map((a) => ({ label: a.name, value: a.id }));
    setAddressSuggestions(filtered);
  };

  // ─── Handlers ──────────────────────────────────────────────────────────────

  const handleCountryChange = (countryId) => {
    setSelectedCountryId(countryId);
    setSelectedCityId(null);
    setCities([]);
    setAddressInput("");
    setSelectedAddressId(null);
    setErrors((prev) => ({
      ...prev,
      country: undefined,
      city: undefined,
      address: undefined,
    }));
    if (countryId) fetchCities(countryId, 1);
  };

  const handleCityChange = (cityId) => {
    setSelectedCityId(cityId);
    setAddressInput("");
    setSelectedAddressId(null);
    setErrors((prev) => ({ ...prev, city: undefined, address: undefined }));
  };

  const handleAddressSelect = (item) => {
    // User picked an existing address from suggestions
    setSelectedAddressId(item.value);
    setAddressInput(item.label);
  };

  const handleAddressChange = (val) => {
    // User is typing — clear existing selection
    setAddressInput(val);
    setSelectedAddressId(null);
    setErrors((prev) => ({ ...prev, address: undefined }));
  };

  // ─── Validation ────────────────────────────────────────────────────────────

  const validate = () => {
    const e = {};
    if (!firstName.trim()) e.firstName = "First name is required";
    if (!lastName.trim()) e.lastName = "Last name is required";
    if (!email.trim()) e.email = "Email is required";
    if (!password.trim()) e.password = "Password is required";
    if (!selectedCountryId) e.country = "Country is required";
    if (!selectedCityId) e.city = "City is required";
    if (!addressInput.trim()) e.address = "Address (street) is required";
    if (!phone.trim()) e.phone = "Phone number is required";
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
        firstName,
        lastName,
        email,
        password,
        cityId: selectedCityId,
        district,
        postalCode,
        phone,
      };

      if (selectedAddressId) {
        // User selected existing address
        payload.existingAddressId = selectedAddressId;
      } else {
        // User typed new address — backend will create it
        payload.street = addressInput;
      }

      const response = await signUpUser(payload);
      login(response);
      navigate("/dashboard");
    } catch (err) {
      setErrors({ submit: err.response?.data || "Sign up failed" });
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
      }}
    >
      <Card style={{ width: "460px", boxShadow: "0 4px 6px rgba(0,0,0,0.1)" }}>
        {/* Header */}
        <div style={{ textAlign: "center", marginBottom: "24px" }}>
          <i
            className="pi pi-video"
            style={{ fontSize: "2.5rem", color: "#6366f1" }}
          />
          <h1 style={{ margin: "12px 0 0 0", color: "#111827" }}>
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
            style={{ marginBottom: "16px" }}
          />
        )}

        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          {/* First Name */}
          <div>
            <label style={labelStyle}>First Name</label>
            <InputText
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              placeholder="First name"
              style={{ width: "100%" }}
              className={errors.firstName ? "p-invalid" : ""}
            />
            {errors.firstName && (
              <small className="p-error">{errors.firstName}</small>
            )}
          </div>

          {/* Last Name */}
          <div>
            <label style={labelStyle}>Last Name</label>
            <InputText
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              placeholder="Last name"
              style={{ width: "100%" }}
              className={errors.lastName ? "p-invalid" : ""}
            />
            {errors.lastName && (
              <small className="p-error">{errors.lastName}</small>
            )}
          </div>

          {/* Email */}
          <div>
            <label style={labelStyle}>Email</label>
            <InputText
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Enter your email"
              type="email"
              style={{ width: "100%" }}
              className={errors.email ? "p-invalid" : ""}
            />
            {errors.email && <small className="p-error">{errors.email}</small>}
          </div>

          {/* Password */}
          <div>
            <label style={labelStyle}>Password</label>
            <Password
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Create a password"
              toggleMask
              feedback={false}
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.password ? "p-invalid" : ""}
            />
            {errors.password && (
              <small className="p-error">{errors.password}</small>
            )}
          </div>

          {/* Divider */}
          <div style={{ borderTop: "1px solid #e5e7eb", paddingTop: "8px" }}>
            <p
              style={{
                margin: "0 0 4px 0",
                fontSize: "13px",
                fontWeight: 600,
                color: "#374151",
              }}
            >
              Address
            </p>
          </div>

          {/* Country */}
          <div>
            <label style={labelStyle}>Country</label>
            <Dropdown
              value={selectedCountryId}
              options={countries}
              onChange={(e) => handleCountryChange(e.value)}
              placeholder="Select country"
              style={{ width: "100%" }}
              filter
              className={errors.country ? "p-invalid" : ""}
              virtualScrollerOptions={{
                lazy: true,
                itemSize: 38,
                onLazyLoad: (e) => {
                  if (countriesHasMore && e.last >= countries.length - 2)
                    fetchCountries(countriesPage + 1);
                },
              }}
            />
            {errors.country && (
              <small className="p-error">{errors.country}</small>
            )}
          </div>

          {/* City */}
          <div>
            <label style={labelStyle}>City</label>
            <Dropdown
              value={selectedCityId}
              options={cities}
              onChange={(e) => handleCityChange(e.value)}
              placeholder={
                !selectedCountryId
                  ? "Select country first"
                  : citiesLoading
                    ? "Loading..."
                    : "Select city"
              }
              style={{ width: "100%" }}
              disabled={!selectedCountryId || citiesLoading}
              filter
              className={errors.city ? "p-invalid" : ""}
              virtualScrollerOptions={{
                lazy: true,
                itemSize: 38,
                onLazyLoad: (e) => {
                  if (citiesHasMore && e.last >= cities.length - 2)
                    fetchCities(selectedCountryId, citiesPage + 1);
                },
              }}
            />
            {errors.city && <small className="p-error">{errors.city}</small>}
          </div>

          {/* Street / Address — AutoComplete with DB suggestions */}
          <div>
            <label style={labelStyle}>Street Address</label>
            <AutoComplete
              value={addressInput}
              suggestions={addressSuggestions}
              completeMethod={searchAddresses}
              onSelect={(e) => handleAddressSelect(e.value)}
              onChange={(e) => handleAddressChange(e.value)}
              placeholder={
                !selectedCityId ? "Select city first" : "Type or select address"
              }
              disabled={!selectedCityId}
              field="label"
              style={{ width: "100%" }}
              inputStyle={{ width: "100%" }}
              className={errors.address ? "p-invalid" : ""}
              forceSelection={false}
            />
            {errors.address && (
              <small className="p-error">{errors.address}</small>
            )}
            {selectedCityId && !selectedAddressId && (
              <small style={{ color: "#6b7280" }}>
                Type a new address or select from suggestions
              </small>
            )}
          </div>

          {/* District */}
          <div>
            <label style={labelStyle}>
              District{" "}
              <span style={{ color: "#9ca3af", fontWeight: 400 }}>
                (optional)
              </span>
            </label>
            <InputText
              value={district}
              onChange={(e) => setDistrict(e.target.value)}
              placeholder="District"
              style={{ width: "100%" }}
            />
          </div>

          {/* Postal Code */}
          <div>
            <label style={labelStyle}>
              Postal Code{" "}
              <span style={{ color: "#9ca3af", fontWeight: 400 }}>
                (optional)
              </span>
            </label>
            <InputText
              value={postalCode}
              onChange={(e) => setPostalCode(e.target.value)}
              placeholder="Postal code"
              style={{ width: "100%" }}
            />
          </div>

          {/* Phone */}
          <div>
            <label style={labelStyle}>Phone</label>
            <InputText
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="Phone number"
              style={{ width: "100%" }}
            />
          </div>

          <Button
            label="Sign Up"
            onClick={handleSignUp}
            loading={loading}
            style={{ width: "100%" }}
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
