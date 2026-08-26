import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import InventoryDialog from "../../components/inventory/InventoryDialog";
import { getInventoryById, deleteInventory } from "../../services/inventoryService";

const FIELD_LABEL = {
  margin: "0 0 4px 0",
  fontSize: "12px",
  color: "#6b7280",
  fontWeight: 500,
  textTransform: "uppercase",
  letterSpacing: "0.05em",
};

const FIELD_VALUE = {
  margin: 0,
  fontSize: "15px",
  color: "#111827",
  fontWeight: 600,
};

export default function InventoryDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [inventory, setInventory] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editVisible, setEditVisible] = useState(false);

  useEffect(() => {
    loadInventory();
  }, [id]);

  const loadInventory = async () => {
    setLoading(true);
    try {
      const data = await getInventoryById(id);
      setInventory(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = () => {
    confirmDialog({
      message: `Are you sure you want to delete Inventory #${id}?`,
      header: "Delete Inventory",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteInventory(id);
        navigate("/inventory");
      },
    });
  };

  if (loading) {
    return (
      <AppLayout>
        <LoadingSpinner />
      </AppLayout>
    );
  }

  if (!inventory) {
    return (
      <AppLayout>
        <p>Inventory not found.</p>
        <Button label="Back" onClick={() => navigate("/inventory")} />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <ConfirmDialog />

      <InventoryDialog
        visible={editVisible}
        onHide={() => setEditVisible(false)}
        onSuccess={loadInventory}
        mode="edit"
        inventory={inventory}
      />

      <DetailPageHeader
        backPath="/inventory"
        backLabel="Inventory"
        title={`#${inventory.inventoryId} — ${inventory.filmTitle}`}
        actions={[
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ]}
      />

      <Card>
        {/* Header */}
        <div style={{ display: "flex", alignItems: "center", gap: "16px", marginBottom: "32px", flexWrap: "wrap" }}>
          <div style={{ width: "64px", height: "64px", borderRadius: "50%", background: "#ede9fe", display: "flex", alignItems: "center", justifyContent: "center" }}>
            <i className="pi pi-box" style={{ fontSize: "1.8rem", color: "#6366f1" }} />
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: "22px", fontWeight: 600 }}>
              {inventory.filmTitle}
            </h2>
            <span style={{ color: "#6b7280", fontSize: "14px" }}>
              Inventory #{inventory.inventoryId}
            </span>
          </div>
        </div>

        {/* Info Grid */}
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "24px" }}>
          <div>
            <p style={FIELD_LABEL}>Inventory ID</p>
            <p style={FIELD_VALUE}>{inventory.inventoryId}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Film</p>
            <p style={FIELD_VALUE}>{inventory.filmTitle}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Store</p>
            <p style={FIELD_VALUE}>Store {inventory.storeId}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Status</p>
            <Tag
              value={inventory.isAvailable ? "Available" : "Rented"}
              severity={inventory.isAvailable ? "success" : "warning"}
            />
          </div>
          <div>
            <p style={FIELD_LABEL}>Total Rentals</p>
            <p style={FIELD_VALUE}>{inventory.totalRentals}</p>
          </div>
          <div>
            <p style={FIELD_LABEL}>Last Updated</p>
            <p style={FIELD_VALUE}>
              {inventory.lastUpdate ? new Date(inventory.lastUpdate).toLocaleDateString() : "—"}
            </p>
          </div>
        </div>
      </Card>
    </AppLayout>
  );
}
