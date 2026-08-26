import { useEffect, useState } from "react";
import { Dialog } from "primereact/dialog";
import { InputText } from "primereact/inputtext";
import { Password } from "primereact/password";
import { Dropdown } from "primereact/dropdown";
import { Button } from "primereact/button";
import {
  createUser,
  updateUser,
  getCountries,
  getCitiesByCountry,
  getAddressesByCity,
  getRoles,
  getStores,
} from "../../services/userService";

const emptyForm = {
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  roleId: null,
  storeId: null,
  countryId: null,
  cityId: null,
  addressId: null,
};

const labelStyle = {
  display: "block",
  marginBottom: "6px",
  fontWeight: "500",
  fontSize: "14px",
  color: "#374151",
};

// Roles that require a store assignment
const STORE_ROLES = ["Staff", "Customer"];

export default function UserDialog({
  visible,
  onHide,
  onSuccess,
  mode = "add",
  user = null,
}) {
  const isEdit = mode === "edit";

  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);

  // Roles dropdown
  const [roles, setRoles] = useState([]);

  // Stores dropdown
  const [stores, setStores] = useState([]);

  // Countries
  const [countries, setCountries] = useState([]);
  const [countriesPage, setCountriesPage] = useState(1);
  const [countriesHasMore, setCountriesHasMore] = useState(true);

  // Cities
  const [cities, setCities] = useState([]);
  const [citiesPage, setCitiesPage] = useState(1);
  const [citiesHasMore, setCitiesHasMore] = useState(true);
  const [citiesLoading, setCitiesLoading] = useState(false);

  // Addresses
  const [addresses, setAddresses] = useState([]);
  const [addressesPage, setAddressesPage] = useState(1);
  const [addressesHasMore, setAddressesHasMore] = useState(true);
  const [addressesLoading, setAddressesLoading] = useState(false);

  // Selected role name — used to show/hide store dropdown
  const [selectedRoleName, setSelectedRoleName] = useState(null);

  // ─── Dialog open hone par init ─────────────────────────────────────────────
  useEffect(() => {
    if (!visible) return;

    if (isEdit && user) {
      setForm({
        userId: user.userId,
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        password: "",
        roleId: user.roleId,
        storeId: null,
        countryId: null,
        cityId: null,
        addressId: user.addressId,
      });
    } else {
      setForm(emptyForm);
      setCities([]);
      setAddresses([]);
      setSelectedRoleName(null);
    }

    setErrors({});
    fetchRoles();
    fetchStores();
    fetchCountries();
  }, [visible]);

  // ─── Fetchers ──────────────────────────────────────────────────────────────

  const fetchRoles = async () => {
    const data = await getRoles(1, 100);
    setRoles(data.map((r) => ({ label: r.name, value: r.id, name: r.name })));
  };

  const fetchStores = async () => {
    const data = await getStores(1, 100);
    setStores(data.map((s) => ({ label: s.name, value: s.id })));
  };

  const fetchCountries = async (page = 1) => {
    const data = await getCountries(page, 10);
    setCountries((prev) => {
      const newItems = data.map((c) => ({ label: c.name, value: c.id }));
      if (page === 1) return newItems;
      const existingIds = new Set(prev.map((c) => c.value));
      return [...prev, ...newItems.filter((c) => !existingIds.has(c.value))];
    });
    setCountriesHasMore(data.length === 10);
    setCountriesPage(page);
  };

  const fetchCities = async (countryId, page = 1) => {
    setCitiesLoading(true);
    try {
      const data = await getCitiesByCountry(countryId, page, 10);
      setCities((prev) => {
        const newItems = data.map((c) => ({ label: c.name, value: c.id }));
        if (page === 1) return newItems;
        const existingIds = new Set(prev.map((c) => c.value));
        return [...prev, ...newItems.filter((c) => !existingIds.has(c.value))];
      });
      setCitiesHasMore(data.length === 10);
      setCitiesPage(page);
    } finally {
      setCitiesLoading(false);
    }
  };

  const fetchAddresses = async (cityId, page = 1) => {
    setAddressesLoading(true);
    try {
      const data = await getAddressesByCity(cityId, page, 100);
      setAddresses((prev) => {
        const newItems = data.map((a) => ({ label: a.name, value: a.id }));
        if (page === 1) return newItems;
        const existingIds = new Set(prev.map((a) => a.value));
        return [...prev, ...newItems.filter((a) => !existingIds.has(a.value))];
      });
      setAddressesHasMore(data.length === 100);
      setAddressesPage(page);
    } finally {
      setAddressesLoading(false);
    }
  };

  // ─── Handlers ──────────────────────────────────────────────────────────────

  const handleRoleChange = (roleId) => {
    const selected = roles.find((r) => r.value === roleId);
    setSelectedRoleName(selected?.name ?? null);
    setForm((prev) => ({ ...prev, roleId, storeId: null }));
    setErrors((prev) => ({ ...prev, roleId: undefined, storeId: undefined }));
  };

  const handleCountryChange = (countryId) => {
    setForm((prev) => ({ ...prev, countryId, cityId: null, addressId: null }));
    setErrors((prev) => ({
      ...prev,
      country: undefined,
      city: undefined,
      address: undefined,
    }));
    setCities([]);
    setAddresses([]);
    if (countryId) fetchCities(countryId, 1);
  };

  const handleCityChange = (cityId) => {
    setForm((prev) => ({ ...prev, cityId, addressId: null }));
    setErrors((prev) => ({ ...prev, city: undefined, address: undefined }));
    setAddresses([]);
    if (cityId) fetchAddresses(cityId, 1);
  };

  const handleChange = (field) => (e) => {
    const value = e.target !== undefined ? e.target.value : e.value;
    setForm((prev) => ({ ...prev, [field]: value }));
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  // ─── Validation ────────────────────────────────────────────────────────────
  const validate = () => {
    const e = {};
    if (!form.firstName?.trim()) e.firstName = "First name is required";
    if (!form.lastName?.trim()) e.lastName = "Last name is required";
    if (!isEdit) {
      if (!form.email?.trim()) e.email = "Email is required";
      if (!form.password?.trim()) e.password = "Password is required";
    }
    if (!form.roleId) e.roleId = "Role is required";
    if (STORE_ROLES.includes(selectedRoleName) && !form.storeId)
      e.storeId = "Store is required for this role";
    if (!form.countryId) e.country = "Country is required";
    if (!form.cityId) e.city = "City is required";
    if (!form.addressId) e.address = "Address is required";
    return e;
  };

  // ─── Submit ────────────────────────────────────────────────────────────────
  const handleSubmit = async () => {
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setLoading(true);
    try {
      if (isEdit) {
        await updateUser({
          userId: form.userId,
          firstName: form.firstName,
          lastName: form.lastName,
          roleId: form.roleId,
          addressId: form.addressId,
        });
      } else {
        await createUser({
          firstName: form.firstName,
          lastName: form.lastName,
          email: form.email,
          password: form.password,
          roleId: form.roleId,
          storeId: form.storeId,
          addressId: form.addressId,
        });
      }

      setForm(emptyForm);
      setErrors({});
      onSuccess();
      onHide();
    } catch (error) {
      console.error(error);
      const msg =
        error?.response?.data ??
        (isEdit ? "Failed to update user." : "Failed to create user.");
      setErrors({
        submit: typeof msg === "string" ? msg : "Something went wrong.",
      });
    } finally {
      setLoading(false);
    }
  };

  const handleHide = () => {
    setForm(emptyForm);
    setErrors({});
    onHide();
  };

  const needsStore = STORE_ROLES.includes(selectedRoleName);

  // ─── Footer ────────────────────────────────────────────────────────────────
  const footer = (
    <div style={{ display: "flex", gap: "8px", justifyContent: "flex-end" }}>
      <Button
        label="Cancel"
        icon="pi pi-times"
        severity="secondary"
        outlined
        onClick={handleHide}
        disabled={loading}
      />
      <Button
        label={isEdit ? "Save Changes" : "Create User"}
        icon="pi pi-check"
        onClick={handleSubmit}
        loading={loading}
      />
    </div>
  );

  // ─── Render ────────────────────────────────────────────────────────────────
  return (
    <Dialog
      header={isEdit ? "Edit User" : "Add New User"}
      visible={visible}
      onHide={handleHide}
      footer={footer}
      style={{ width: "440px" }}
      modal
    >
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          gap: "16px",
          paddingTop: "8px",
        }}
      >
        {/* First Name */}
        <div>
          <label style={labelStyle}>First Name</label>
          <InputText
            value={form.firstName}
            onChange={handleChange("firstName")}
            placeholder="Enter first name"
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
            value={form.lastName}
            onChange={handleChange("lastName")}
            placeholder="Enter last name"
            style={{ width: "100%" }}
            className={errors.lastName ? "p-invalid" : ""}
          />
          {errors.lastName && (
            <small className="p-error">{errors.lastName}</small>
          )}
        </div>

        {/* Email — only in add mode */}
        {!isEdit && (
          <div>
            <label style={labelStyle}>Email</label>
            <InputText
              value={form.email}
              onChange={handleChange("email")}
              placeholder="Enter email"
              type="email"
              style={{ width: "100%" }}
              className={errors.email ? "p-invalid" : ""}
            />
            {errors.email && (
              <small className="p-error">{errors.email}</small>
            )}
          </div>
        )}

        {/* Password — only in add mode */}
        {!isEdit && (
          <div>
            <label style={labelStyle}>Password</label>
            <Password
              value={form.password}
              onChange={handleChange("password")}
              placeholder="Enter password"
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
        )}

        {/* Role */}
        <div>
          <label style={labelStyle}>Role</label>
          <Dropdown
            value={form.roleId}
            options={roles}
            onChange={(e) => handleRoleChange(e.value)}
            placeholder="Select role"
            style={{ width: "100%" }}
            appendTo="self"
            className={errors.roleId ? "p-invalid" : ""}
          />
          {errors.roleId && (
            <small className="p-error">{errors.roleId}</small>
          )}
        </div>

        {/* Store — only visible when role is Staff or Customer */}
        {needsStore && (
          <div>
            <label style={labelStyle}>Store</label>
            <Dropdown
              value={form.storeId}
              options={stores}
              onChange={handleChange("storeId")}
              placeholder="Select store"
              style={{ width: "100%" }}
              appendTo="self"
              className={errors.storeId ? "p-invalid" : ""}
            />
            {errors.storeId && (
              <small className="p-error">{errors.storeId}</small>
            )}
          </div>
        )}

        {/* Country */}
        <div>
          <label style={labelStyle}>Country</label>
          <Dropdown
            value={form.countryId}
            options={countries}
            onChange={(e) => handleCountryChange(e.value)}
            placeholder="Select country"
            style={{ width: "100%" }}
            appendTo="self"
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
            value={form.cityId}
            options={cities}
            onChange={(e) => handleCityChange(e.value)}
            placeholder={
              !form.countryId
                ? "Select country first"
                : citiesLoading
                  ? "Loading..."
                  : "Select city"
            }
            style={{ width: "100%" }}
            appendTo="self"
            disabled={!form.countryId || citiesLoading}
            filter
            className={errors.city ? "p-invalid" : ""}
            virtualScrollerOptions={{
              lazy: true,
              itemSize: 38,
              onLazyLoad: (e) => {
                if (citiesHasMore && e.last >= cities.length - 2)
                  fetchCities(form.countryId, citiesPage + 1);
              },
            }}
          />
          {errors.city && <small className="p-error">{errors.city}</small>}
        </div>

        {/* Address */}
        <div>
          <label style={labelStyle}>Address</label>
          <Dropdown
            value={form.addressId}
            options={addresses}
            onChange={handleChange("addressId")}
            placeholder={
              !form.cityId
                ? "Select city first"
                : addressesLoading
                  ? "Loading..."
                  : "Select address"
            }
            style={{ width: "100%" }}
            appendTo="self"
            disabled={!form.cityId || addressesLoading}
            filter
            className={errors.address ? "p-invalid" : ""}
          />
          {errors.address && (
            <small className="p-error">{errors.address}</small>
          )}
        </div>

        {/* Submit Error */}
        {errors.submit && (
          <small className="p-error" style={{ textAlign: "center" }}>
            {errors.submit}
          </small>
        )}
      </div>
    </Dialog>
  );
}
