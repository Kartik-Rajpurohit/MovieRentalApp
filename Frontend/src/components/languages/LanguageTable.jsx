import { useEffect, useState } from "react";
import { DataTable } from "primereact/datatable";
import { Column } from "primereact/column";
import { useNavigate } from "react-router-dom";
import PageHeader from "../common/PageHeader";
import SearchBar from "../common/SearchBar";
import LanguageDialog from "./LanguageDialog";
import { getLanguages } from "../../services/languageService";

export default function LanguageTable() {
  const navigate = useNavigate();

  const [languages, setLanguages] = useState([]);
  const [filtered, setFiltered] = useState([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState("");
  const [dialogVisible, setDialogVisible] = useState(false);

  useEffect(() => { loadLanguages(); }, []);

  useEffect(() => {
    // Client-side search — backend returns full list
    if (!search.trim()) { setFiltered(languages); return; }
    const s = search.toLowerCase();
    setFiltered(languages.filter(l => l.name.toLowerCase().includes(s)));
  }, [search, languages]);

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
        onAdd={() => setDialogVisible(true)}
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
