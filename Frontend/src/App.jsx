import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./routes/ProtectedRoute";

import LoginPage from "./pages/Auth/LoginPage";
import SignUpPage from "./pages/Auth/SignUpPage";
import DashboardPage from "./pages/Dashboard/DashboardPage";
import UsersPage from "./pages/Users/UsersPage";
import UserDetailPage from "./pages/Users/UserDetailPage";
import StaffPage from "./pages/Staff/StaffPage";
import StaffDetailPage from "./pages/Staff/StaffDetailPage";
import CustomersPage from "./pages/Customers/CustomersPage";
import CustomerDetailPage from "./pages/Customers/CustomerDetailPage";
import RolesPage from "./pages/Roles/RolesPage";
import RoleDetailPage from "./pages/Roles/RoleDetailPage";
import MoviesPage from "./pages/Movies/MoviesPage";
import MovieDetailPage from "./pages/Movies/MovieDetailPage";
import ActorsPage from "./pages/Actors/ActorsPage";
import ActorDetailPage from "./pages/Actors/ActorDetailPage";
import CategoriesPage from "./pages/Categories/CategoriesPage";
import CategoryDetailPage from "./pages/Categories/CategoryDetailPage";
import LanguagesPage from "./pages/Languages/LanguagesPage";
import LanguageDetailPage from "./pages/Languages/LanguageDetailPage";
import InventoryPage from "./pages/Inventory/InventoryPage";
import InventoryDetailPage from "./pages/Inventory/InventoryDetailPage";
import RentalsPage from "./pages/Rentals/RentalsPage";
import RentalDetailPage from "./pages/Rentals/RentalDetailPage";
import PaymentsPage from "./pages/Payments/PaymentsPage";
import PaymentDetailPage from "./pages/Payments/PaymentDetailPage";
import StoresPage from "./pages/Stores/StoresPage";
import StoreDetailPage from "./pages/Stores/StoreDetailPage";
import CountriesPage from "./pages/Locations/CountriesPage";
import CountryDetailPage from "./pages/Locations/CountryDetailPage";
import CitiesPage from "./pages/Locations/CitiesPage";
import CityDetailPage from "./pages/Locations/CityDetailPage";
import AddressesPage from "./pages/Locations/AddressesPage";
import AddressDetailPage from "./pages/Locations/AddressDetailPage";

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public routes */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/signup" element={<SignUpPage />} />

          {/* Dashboard — all logged in users */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />

          {/* Users — Admin only */}
          <Route
            path="/users"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <UsersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/users/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <UserDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Roles — Admin only */}
          <Route
            path="/roles"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <RolesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/roles/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin"]}>
                <RoleDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Staff — Admin and Staff */}
          <Route
            path="/staff"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <StaffPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/staff/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <StaffDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Customers — Admin, Staff, Customer */}
          <Route
            path="/customers"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}>
                <CustomersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/customers/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}>
                <CustomerDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Movies — Admin and Staff */}
          <Route
            path="/movies"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <MoviesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/movies/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <MovieDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/actors"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <ActorsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/actors/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <ActorDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/categories"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <CategoriesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/categories/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <CategoryDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/languages"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <LanguagesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/languages/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <LanguageDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/inventory"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <InventoryPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/inventory/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <InventoryDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/rentals"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <RentalsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/rentals/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <RentalDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Payments — Admin and Staff */}
          <Route
            path="/payments"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <PaymentsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/payments/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <PaymentDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Stores — Admin and Staff */}
          <Route
            path="/stores"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <StoresPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/stores/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <StoreDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/countries"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <CountriesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/countries/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <CountryDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/cities"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <CitiesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/cities/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <CityDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Addresses — Admin and Staff */}
          <Route
            path="/addresses"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <AddressesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/addresses/:id"
            element={
              <ProtectedRoute allowedRoles={["Admin", "Staff"]}>
                <AddressDetailPage />
              </ProtectedRoute>
            }
          />

          {/* Default redirects */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
