import { NavLink } from "react-router-dom";
import { useContext } from "react";
import { AuthContext } from "../../context/AuthContext";
import "./Layout.css";

const sections = [
  {
    label: "People",
    items: [
      { label: "Users", icon: "pi pi-lock", to: "/users", roles: ["Admin"] },
      { label: "Customers", icon: "pi pi-users", to: "/customers", roles: ["Admin", "Staff", "Customer"] },
      { label: "Staff", icon: "pi pi-user", to: "/staff", roles: ["Admin", "Staff"] },
      { label: "Roles", icon: "pi pi-shield", to: "/roles", roles: ["Admin"] },
    ],
  },
  {
    label: "Movie Management",
    items: [
      { label: "Movies", icon: "pi pi-video", to: "/movies", roles: ["Admin", "Staff"] },
      { label: "Actors", icon: "pi pi-star", to: "/actors", roles: ["Admin", "Staff"] },
      { label: "Categories", icon: "pi pi-tag", to: "/categories", roles: ["Admin", "Staff"] },
      { label: "Languages", icon: "pi pi-globe", to: "/languages", roles: ["Admin", "Staff"] },
    ],
  },
  {
    label: "Rental Management",
    items: [
      { label: "Inventory", icon: "pi pi-box", to: "/inventory", roles: ["Admin", "Staff"] },
      { label: "Rentals", icon: "pi pi-sync", to: "/rentals", roles: ["Admin", "Staff"] },
      { label: "Payments", icon: "pi pi-credit-card", to: "/payments", roles: ["Admin", "Staff"] },
    ],
  },
  {
    label: "Locations",
    items: [
      { label: "Stores", icon: "pi pi-building", to: "/stores", roles: ["Admin"] },
      { label: "Countries", icon: "pi pi-globe", to: "/countries", roles: ["Admin"] },
      { label: "Cities", icon: "pi pi-map-marker", to: "/cities", roles: ["Admin"] },
      { label: "Addresses", icon: "pi pi-map", to: "/addresses", roles: ["Admin"] },
    ],
  },
];

export default function Sidebar() {
  const { user } = useContext(AuthContext);
  const userRole = user?.role ?? "";
  const canAccess = (roles) => !roles || roles.includes(userRole);

  return (
    <div className="sidebar">
      {/* Logo */}
      <div className="sidebar-logo">
        <i className="pi pi-video" style={{ fontSize: "16px", color: "#6366f1" }} />
        Movie Rental
      </div>

      {/* Dashboard — visible to all logged in users */}
      <div style={{ padding: "10px 8px 0" }}>
        <NavLink
          to="/dashboard"
          className={({ isActive }) => "sidebar-link" + (isActive ? " active" : "")}
        >
          <i className="pi pi-chart-bar" style={{ fontSize: "14px", width: "16px" }} />
          Dashboard
        </NavLink>
      </div>

      {/* Role-filtered sections */}
      {sections.map((section) => {
        const visibleItems = section.items.filter((item) => canAccess(item.roles));
        if (visibleItems.length === 0) return null;

        return (
          <div key={section.label}>
            <div className="sidebar-section-label">{section.label}</div>
            <nav className="sidebar-nav">
              {visibleItems.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  className={({ isActive }) => "sidebar-link" + (isActive ? " active" : "")}
                >
                  <i className={item.icon} style={{ fontSize: "14px", width: "16px" }} />
                  {item.label}
                </NavLink>
              ))}
            </nav>
          </div>
        );
      })}
    </div>
  );
}
