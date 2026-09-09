import Sidebar from "./Sidebar";
import Topbar from "./Topbar";
import "./Layout.css";

// Application shell layout component wrapping pages with a responsive sidebar and topbar
export default function AppLayout({ children }) {
  return (
    <div className="app-layout">
      <Sidebar />
      <div className="main-content">
        <Topbar />
        <div className="content">
          {children}
        </div>
      </div>
    </div>
  );
}
