import { lazy, Suspense } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import { ToastProvider } from "./context/ToastContext";
import ProtectedRoute from "./routes/ProtectedRoute";
import HomeRoute from "./routes/HomeRoute";
import LoadingSpinner from "./components/common/LoadingSpinner";
import ErrorBoundary from "./components/common/ErrorBoundary";

// Lazy-loaded page components for route code-splitting
const LoginPage = lazy(() => import("./pages/Auth/LoginPage"));
const SignUpPage = lazy(() => import("./pages/Auth/SignUpPage"));
const HomePage = lazy(() => import("./pages/Home/HomePage"));
const DashboardPage = lazy(() => import("./pages/Dashboard/DashboardPage"));
const UsersPage = lazy(() => import("./pages/Users/UsersPage"));
const UserDetailPage = lazy(() => import("./pages/Users/UserDetailPage"));
const StaffPage = lazy(() => import("./pages/Staff/StaffPage"));
const StaffDetailPage = lazy(() => import("./pages/Staff/StaffDetailPage"));
const CustomersPage = lazy(() => import("./pages/Customers/CustomersPage"));
const CustomerDetailPage = lazy(() => import("./pages/Customers/CustomerDetailPage"));
const RolesPage = lazy(() => import("./pages/Roles/RolesPage"));
const RoleDetailPage = lazy(() => import("./pages/Roles/RoleDetailPage"));
const MoviesPage = lazy(() => import("./pages/Movies/MoviesPage"));
const MovieDetailPage = lazy(() => import("./pages/Movies/MovieDetailPage"));
const ActorsPage = lazy(() => import("./pages/Actors/ActorsPage"));
const ActorDetailPage = lazy(() => import("./pages/Actors/ActorDetailPage"));
const CategoriesPage = lazy(() => import("./pages/Categories/CategoriesPage"));
const CategoryDetailPage = lazy(() => import("./pages/Categories/CategoryDetailPage"));
const LanguagesPage = lazy(() => import("./pages/Languages/LanguagesPage"));
const LanguageDetailPage = lazy(() => import("./pages/Languages/LanguageDetailPage"));
const InventoryPage = lazy(() => import("./pages/Inventory/InventoryPage"));
const InventoryDetailPage = lazy(() => import("./pages/Inventory/InventoryDetailPage"));
const RentalsPage = lazy(() => import("./pages/Rentals/RentalsPage"));
const RentalDetailPage = lazy(() => import("./pages/Rentals/RentalDetailPage"));
const PaymentsPage = lazy(() => import("./pages/Payments/PaymentsPage"));
const PaymentDetailPage = lazy(() => import("./pages/Payments/PaymentDetailPage"));
const StoresPage = lazy(() => import("./pages/Stores/StoresPage"));
const StoreDetailPage = lazy(() => import("./pages/Stores/StoreDetailPage"));
const CountriesPage = lazy(() => import("./pages/Locations/CountriesPage"));
const CountryDetailPage = lazy(() => import("./pages/Locations/CountryDetailPage"));
const CitiesPage = lazy(() => import("./pages/Locations/CitiesPage"));
const CityDetailPage = lazy(() => import("./pages/Locations/CityDetailPage"));
const AddressesPage = lazy(() => import("./pages/Locations/AddressesPage"));
const AddressDetailPage = lazy(() => import("./pages/Locations/AddressDetailPage"));

function App() {
  return (
    <ErrorBoundary>
      <ToastProvider>
        <AuthProvider>
          <BrowserRouter>
            <Suspense fallback={<LoadingSpinner />}>
              <Routes>
                {/* Public routes */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/signup" element={<SignUpPage />} />

                {/* Home — logged in but no role assigned */}
                <Route path="/home" element={<HomeRoute><HomePage /></HomeRoute>} />

                {/* Dashboard — all roles */}
                <Route path="/dashboard" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />

                {/* Users — Admin only */}
                <Route path="/users" element={<ProtectedRoute allowedRoles={["Admin"]}><UsersPage /></ProtectedRoute>} />
                <Route path="/users/:id" element={<ProtectedRoute allowedRoles={["Admin"]}><UserDetailPage /></ProtectedRoute>} />

                {/* Roles — Admin only */}
                <Route path="/roles" element={<ProtectedRoute allowedRoles={["Admin"]}><RolesPage /></ProtectedRoute>} />
                <Route path="/roles/:id" element={<ProtectedRoute allowedRoles={["Admin"]}><RoleDetailPage /></ProtectedRoute>} />

                {/* Staff — Admin and Staff */}
                <Route path="/staff" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><StaffPage /></ProtectedRoute>} />
                <Route path="/staff/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><StaffDetailPage /></ProtectedRoute>} />

                {/* Customers — Admin and Staff */}
                <Route path="/customers" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><CustomersPage /></ProtectedRoute>} />
                <Route path="/customers/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><CustomerDetailPage /></ProtectedRoute>} />

                {/* Movies — Admin, Staff, Customer */}
                <Route path="/movies" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><MoviesPage /></ProtectedRoute>} />
                <Route path="/movies/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><MovieDetailPage /></ProtectedRoute>} />

                {/* Actors — Admin, Staff, Customer */}
                <Route path="/actors" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><ActorsPage /></ProtectedRoute>} />
                <Route path="/actors/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><ActorDetailPage /></ProtectedRoute>} />

                {/* Categories — Admin, Staff, Customer */}
                <Route path="/categories" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><CategoriesPage /></ProtectedRoute>} />
                <Route path="/categories/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><CategoryDetailPage /></ProtectedRoute>} />

                {/* Languages — Admin, Staff, Customer */}
                <Route path="/languages" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><LanguagesPage /></ProtectedRoute>} />
                <Route path="/languages/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><LanguageDetailPage /></ProtectedRoute>} />

                {/* Inventory — Admin and Staff */}
                <Route path="/inventory" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><InventoryPage /></ProtectedRoute>} />
                <Route path="/inventory/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><InventoryDetailPage /></ProtectedRoute>} />

                {/* Rentals — Admin, Staff, Customer */}
                <Route path="/rentals" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><RentalsPage /></ProtectedRoute>} />
                <Route path="/rentals/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><RentalDetailPage /></ProtectedRoute>} />

                {/* Payments — Admin, Staff, Customer */}
                <Route path="/payments" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><PaymentsPage /></ProtectedRoute>} />
                <Route path="/payments/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff", "Customer"]}><PaymentDetailPage /></ProtectedRoute>} />

                {/* Stores — Admin and Staff */}
                <Route path="/stores" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><StoresPage /></ProtectedRoute>} />
                <Route path="/stores/:id" element={<ProtectedRoute allowedRoles={["Admin", "Staff"]}><StoreDetailPage /></ProtectedRoute>} />

                {/* Locations — Admin only */}
                <Route path="/countries" element={<ProtectedRoute allowedRoles={["Admin"]}><CountriesPage /></ProtectedRoute>} />
                <Route path="/countries/:id" element={<ProtectedRoute allowedRoles={["Admin"]}><CountryDetailPage /></ProtectedRoute>} />
                <Route path="/cities" element={<ProtectedRoute allowedRoles={["Admin"]}><CitiesPage /></ProtectedRoute>} />
                <Route path="/cities/:id" element={<ProtectedRoute allowedRoles={["Admin"]}><CityDetailPage /></ProtectedRoute>} />
                <Route path="/addresses" element={<ProtectedRoute allowedRoles={["Admin"]}><AddressesPage /></ProtectedRoute>} />
                <Route path="/addresses/:id" element={<ProtectedRoute allowedRoles={["Admin"]}><AddressDetailPage /></ProtectedRoute>} />

                {/* Default redirects */}
                <Route path="/" element={<Navigate to="/dashboard" replace />} />
                <Route path="*" element={<Navigate to="/dashboard" replace />} />
              </Routes>
            </Suspense>
          </BrowserRouter>
        </AuthProvider>
      </ToastProvider>
    </ErrorBoundary>
  );
}

export default App;
