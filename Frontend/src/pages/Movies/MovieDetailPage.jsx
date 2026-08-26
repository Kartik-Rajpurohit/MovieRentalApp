import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Tag } from "primereact/tag";
import { Card } from "primereact/card";
import { Button } from "primereact/button";
import { ConfirmDialog, confirmDialog } from "primereact/confirmdialog";
import AppLayout from "../../components/layout/AppLayout";
import LoadingSpinner from "../../components/common/LoadingSpinner";
import DetailPageHeader from "../../components/common/DetailPageHeader";
import MovieDialog from "../../components/movies/MovieDialog";
import { getMovieById, deleteMovie } from "../../services/movieService";

const RATING_SEVERITY = { G: "success", PG: "info", "PG-13": "warning", R: "danger", "NC-17": "danger" };
const fieldStyle = { display: "flex", flexDirection: "column", gap: "4px" };
const labelStyle = { fontSize: "12px", color: "#6b7280", textTransform: "uppercase", letterSpacing: "0.05em" };
const valueStyle = { fontSize: "15px", color: "#111827", fontWeight: 500 };

export default function MovieDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [movie, setMovie] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editVisible, setEditVisible] = useState(false);

  useEffect(() => { loadMovie(); }, [id]);

  const loadMovie = async () => {
    setLoading(true);
    try {
      const data = await getMovieById(id);
      setMovie(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = () => {
    confirmDialog({
      message: `Are you sure you want to delete "${movie?.title}"?`,
      header: "Delete Movie",
      icon: "pi pi-exclamation-triangle",
      acceptClassName: "p-button-danger",
      accept: async () => {
        await deleteMovie(id);
        navigate("/movies");
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

  if (!movie) {
    return (
      <AppLayout>
        <p>Movie not found.</p>
        <Button label="Back to Movies" text onClick={() => navigate("/movies")} />
      </AppLayout>
    );
  }

  return (
    <AppLayout>
      <ConfirmDialog />
      <MovieDialog visible={editVisible} onHide={() => setEditVisible(false)} onSuccess={loadMovie} mode="edit" movie={movie} />

      <DetailPageHeader
        backPath="/movies"
        backLabel="Movies"
        title={movie.title}
        actions={[
          { label: "Edit", icon: "pi pi-pencil", outlined: true, onClick: () => setEditVisible(true) },
          { label: "Delete", icon: "pi pi-trash", severity: "danger", outlined: true, onClick: handleDelete },
        ]}
      />

      {/* Main Info Card */}
      <Card style={{ marginBottom: "16px" }}>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "24px" }}>
          <div style={fieldStyle}><span style={labelStyle}>Release Year</span><span style={valueStyle}>{movie.releaseYear ?? "—"}</span></div>
          <div style={fieldStyle}><span style={labelStyle}>Language</span><span style={valueStyle}>{movie.languageName}</span></div>
          {movie.originalLanguageName && <div style={fieldStyle}><span style={labelStyle}>Original Language</span><span style={valueStyle}>{movie.originalLanguageName}</span></div>}
          <div style={fieldStyle}><span style={labelStyle}>Length</span><span style={valueStyle}>{movie.length ? `${movie.length} min` : "—"}</span></div>
          <div style={fieldStyle}>
            <span style={labelStyle}>Rating</span>
            {movie.rating
              ? <Tag value={movie.rating} severity={RATING_SEVERITY[movie.rating] ?? "info"} style={{ width: "fit-content" }} />
              : <span style={valueStyle}>—</span>}
          </div>
          <div style={fieldStyle}><span style={labelStyle}>Total Inventory</span><span style={valueStyle}>{movie.totalInventory} copies</span></div>
        </div>
        {movie.description && (
          <div style={{ ...fieldStyle, marginTop: "24px" }}>
            <span style={labelStyle}>Description</span>
            <p style={{ margin: 0, color: "#374151", lineHeight: "1.6" }}>{movie.description}</p>
          </div>
        )}
      </Card>

      {/* Rental Info Card */}
      <Card title="Rental Info" style={{ marginBottom: "16px" }}>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "24px" }}>
          <div style={fieldStyle}><span style={labelStyle}>Rental Duration</span><span style={valueStyle}>{movie.rentalDuration} days</span></div>
          <div style={fieldStyle}><span style={labelStyle}>Rental Rate</span><span style={valueStyle}>${movie.rentalRate?.toFixed(2)}</span></div>
          <div style={fieldStyle}><span style={labelStyle}>Replacement Cost</span><span style={valueStyle}>${movie.replacementCost?.toFixed(2)}</span></div>
        </div>
      </Card>

      {/* Categories + Actors */}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "16px" }}>
        <Card title="Categories">
          <div style={{ display: "flex", flexWrap: "wrap", gap: "8px" }}>
            {movie.categories?.length > 0
              ? movie.categories.map((c) => <Tag key={c.categoryId} value={c.name} severity="info" />)
              : <span style={{ color: "#9ca3af" }}>No categories</span>}
          </div>
        </Card>
        <Card title="Actors">
          <div style={{ display: "flex", flexWrap: "wrap", gap: "8px" }}>
            {movie.actors?.length > 0
              ? movie.actors.map((a) => <Tag key={a.actorId} value={a.fullName} severity="secondary" />)
              : <span style={{ color: "#9ca3af" }}>No actors</span>}
          </div>
        </Card>
      </div>

      {/* Special Features */}
      {movie.specialFeatures?.length > 0 && (
        <Card title="Special Features" style={{ marginTop: "16px" }}>
          <div style={{ display: "flex", flexWrap: "wrap", gap: "8px" }}>
            {movie.specialFeatures.map((sf) => <Tag key={sf} value={sf} severity="secondary" />)}
          </div>
        </Card>
      )}
    </AppLayout>
  );
}
