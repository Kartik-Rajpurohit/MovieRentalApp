import Sidebar from "./Sidebar";
import Topbar from "./Topbar";
import "./Layout.css";

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
