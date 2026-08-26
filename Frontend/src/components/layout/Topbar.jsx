import { useContext, useRef } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Menu } from "primereact/menu";
import { AuthContext } from "../../context/AuthContext";
import "./Layout.css";

const PAGE_TITLES = {
  "/dashboard": "Dashboard",
  "/movies": "Movies",
  "/actors": "Actors",
  "/categories": "Categories",
  "/languages": "Languages",
  "/inventory": "Inventory",
  "/rentals": "Rentals",
  "/payments": "Payments",
  "/customers": "Customers",
  "/staff": "Staff",
  "/users": "Users",
  "/roles": "Roles",
  "/stores": "Stores",
  "/countries": "Countries",
  "/cities": "Cities",
  "/addresses": "Addresses",
};

export default function Topbar() {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useContext(AuthContext);
  const menuRef = useRef(null);

  const base = "/" + location.pathname.split("/")[1];
  const title = PAGE_TITLES[base] ?? "Movie Rental";

  const menuItems = [
    {
      label: user?.fullName ?? "User",
      items: [
        {
          label: "Logout",
          icon: "pi pi-sign-out",
          command: () => {
            logout();
            navigate("/login");
          },
        },
      ],
    },
  ];

  return (
    <div className="topbar">
      <span className="topbar-title">{title}</span>

      <Menu model={menuItems} popup ref={menuRef} />

      <div
        onClick={(e) => menuRef.current.toggle(e)}
        style={{
          width: "34px",
          height: "34px",
          borderRadius: "50%",
          background: "#ede9fe",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          cursor: "pointer",
        }}
      >
        <i
          className="pi pi-user"
          style={{ fontSize: "14px", color: "#6366f1" }}
        />
      </div>
    </div>
  );
}
