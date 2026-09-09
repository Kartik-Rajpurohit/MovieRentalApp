import { useEffect, useState, useContext } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import LanguageDialog from "./LanguageDialog";
import { getLanguages } from "../../services/languageService";
import { AuthContext } from "../../context/AuthContext";

// Displays the languages catalog in a DataTable with client-side search and add language action
export default function LanguageTable() {
  const { user } = useContext(AuthContext);
  const navigate = useNavigate();

  // Full languages list and client-filtered list
  const [languages, setLanguages] = useState([]);
  const [filtered, setFiltered] = useState([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  // Controls visibility of the Add Language dialog
  const [dialogVisible, setDialogVisible] = useState(false);

  // Load languages once when component mounts
  useEffect(() => { loadLanguages(); }, []);

  // Filter languages locally whenever search text changes
  useEffect(() => {
    // Client-side search — backend returns full list
    if (!search.trim()) { setFiltered(languages); return; }
    const s = search.toLowerCase();
    setFiltered(languages.filter(l => l.name.toLowerCase().includes(s)));
  }, [search, languages]);

  // Load all languages from the backend service
  const loadLanguages = async () => {
    setLoading(true);
    try {
      const data = await getLanguages();
      setLanguages(data ?? []);
      setFiltered(data ?? []);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };


  return (
    <div>
      <PageHeader
        title="Languages"
        addLabel="Add Language"
        onAdd={user?.role !== "Customer" ? () => setDialogVisible(true) : undefined}
      />

      <div style={{ display: "flex", gap: "12px", marginBottom: "16px" }}>
        <SearchBar
          value={search}
          onChange={setSearch}
          placeholder="Search languages..."
        />
      </div>

      <LanguageDialog
        visible={dialogVisible}
        onHide={() => setDialogVisible(false)}
        onSuccess={loadLanguages}
        mode="add"
      />

      <DataTable
        value={filtered}
        loading={loading}
        emptyMessage="No languages found."
        onRowClick={(e) => navigate(`/languages/${e.data.languageId}`)}
        rowClassName={() => "cursor-pointer"}
      >
        <Column field="name" header="Name" sortable />
        <Column
          field="filmCount"
          header="Movies"
          style={{ width: "100px" }}
          sortable
        />
        <Column
          field="lastUpdate"
          header="Last Updated"
          style={{ width: "160px" }}
          body={(r) => new Date(r.lastUpdate).toLocaleDateString()}
        />
      </DataTable>
    </div>
  );
}
