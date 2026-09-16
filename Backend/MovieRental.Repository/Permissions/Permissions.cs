namespace MovieRental.Repository.Permissions
{
    /// <summary>
    /// Centralized permission definitions for the MovieRental application.
    /// Defined at the Repository layer as the canonical source of authorization permissions.
    /// </summary>
    public static class Permissions
    {
        // ── Domain Permission Constants ───────────────────────────────

        public static class Movies
        {
            public const string Read = "Permissions.Movies.Read";
            public const string Create = "Permissions.Movies.Create";
            public const string Update = "Permissions.Movies.Update";
            public const string Delete = "Permissions.Movies.Delete";
        }

        public static class Actors
        {
            public const string Read = "Permissions.Actors.Read";
            public const string Create = "Permissions.Actors.Create";
            public const string Update = "Permissions.Actors.Update";
            public const string Delete = "Permissions.Actors.Delete";
        }

        public static class Categories
        {
            public const string Read = "Permissions.Categories.Read";
            public const string Create = "Permissions.Categories.Create";
            public const string Update = "Permissions.Categories.Update";
            public const string Delete = "Permissions.Categories.Delete";
        }

        public static class Languages
        {
            public const string Read = "Permissions.Languages.Read";
            public const string Create = "Permissions.Languages.Create";
            public const string Update = "Permissions.Languages.Update";
            public const string Delete = "Permissions.Languages.Delete";
        }

        public static class Inventory
        {
            public const string Read = "Permissions.Inventory.Read";
            public const string Create = "Permissions.Inventory.Create";
            public const string Update = "Permissions.Inventory.Update";
            public const string Delete = "Permissions.Inventory.Delete";
        }

        public static class Rentals
        {
            public const string Read = "Permissions.Rentals.Read";
            public const string Create = "Permissions.Rentals.Create";
            public const string Return = "Permissions.Rentals.Return";
        }

        public static class Payments
        {
            public const string Read = "Permissions.Payments.Read";
            public const string Create = "Permissions.Payments.Create";
        }

        public static class Customers
        {
            public const string Read = "Permissions.Customers.Read";
        }

        public static class Staff
        {
            public const string Read = "Permissions.Staff.Read";
        }

        public static class Stores
        {
            public const string Read = "Permissions.Stores.Read";
            public const string Create = "Permissions.Stores.Create";
        }

        public static class Users
        {
            public const string Read = "Permissions.Users.Read";
            public const string Create = "Permissions.Users.Create";
            public const string Update = "Permissions.Users.Update";
            public const string ManageStores = "Permissions.Users.ManageStores";
        }

        public static class Roles
        {
            public const string Read = "Permissions.Roles.Read";
            public const string Create = "Permissions.Roles.Create";
        }

        public static class Countries
        {
            public const string Read = "Permissions.Countries.Read";
            public const string Create = "Permissions.Countries.Create";
            public const string Update = "Permissions.Countries.Update";
            public const string Delete = "Permissions.Countries.Delete";
        }

        public static class Cities
        {
            public const string Read = "Permissions.Cities.Read";
            public const string Create = "Permissions.Cities.Create";
            public const string Update = "Permissions.Cities.Update";
            public const string Delete = "Permissions.Cities.Delete";
        }

        public static class Addresses
        {
            public const string Read = "Permissions.Addresses.Read";
            public const string Create = "Permissions.Addresses.Create";
            public const string Update = "Permissions.Addresses.Update";
            public const string Delete = "Permissions.Addresses.Delete";
        }

        public static class Dashboard
        {
            public const string Read = "Permissions.Dashboard.Read";
        }

        /// <summary>
        /// Maps each permission to the roles that possess it by default.
        /// Consumed by the API layer to configure authorization policies.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string[]> RoleMap = new Dictionary<string, string[]>
        {
            // Movies
            [Movies.Read] = ["Admin", "Staff", "Customer"],
            [Movies.Create] = ["Admin"],
            [Movies.Update] = ["Admin"],
            [Movies.Delete] = ["Admin"],

            // Actors
            [Actors.Read] = ["Admin", "Staff", "Customer"],
            [Actors.Create] = ["Admin", "Staff"],
            [Actors.Update] = ["Admin", "Staff"],
            [Actors.Delete] = ["Admin", "Staff"],

            // Categories
            [Categories.Read] = ["Admin", "Staff", "Customer"],
            [Categories.Create] = ["Admin", "Staff"],
            [Categories.Update] = ["Admin", "Staff"],
            [Categories.Delete] = ["Admin", "Staff"],

            // Languages
            [Languages.Read] = ["Admin", "Staff", "Customer"],
            [Languages.Create] = ["Admin"],
            [Languages.Update] = ["Admin"],
            [Languages.Delete] = ["Admin"],

            // Inventory
            [Inventory.Read] = ["Admin", "Staff"],
            [Inventory.Create] = ["Admin", "Staff"],
            [Inventory.Update] = ["Admin", "Staff"],
            [Inventory.Delete] = ["Admin", "Staff"],

            // Rentals
            [Rentals.Read] = ["Admin", "Staff", "Customer"],
            [Rentals.Create] = ["Admin", "Staff"],
            [Rentals.Return] = ["Admin", "Staff"],

            // Payments
            [Payments.Read] = ["Admin", "Staff", "Customer"],
            [Payments.Create] = ["Admin", "Staff"],

            // Customers
            [Customers.Read] = ["Admin", "Staff"],

            // Staff
            [Staff.Read] = ["Admin", "Staff"],

            // Stores
            [Stores.Read] = ["Admin", "Staff"],
            [Stores.Create] = ["Admin"],

            // Users
            [Users.Read] = ["Admin"],
            [Users.Create] = ["Admin"],
            [Users.Update] = ["Admin"],
            [Users.ManageStores] = ["Admin", "Staff"],

            // Roles
            [Roles.Read] = ["Admin"],
            [Roles.Create] = ["Admin"],

            // Countries
            [Countries.Read] = ["Admin", "Staff"],
            [Countries.Create] = ["Admin"],
            [Countries.Update] = ["Admin"],
            [Countries.Delete] = ["Admin"],

            // Cities
            [Cities.Read] = ["Admin", "Staff"],
            [Cities.Create] = ["Admin"],
            [Cities.Update] = ["Admin"],
            [Cities.Delete] = ["Admin"],

            // Addresses
            [Addresses.Read] = ["Admin", "Staff"],
            [Addresses.Create] = ["Admin", "Staff"],
            [Addresses.Update] = ["Admin", "Staff"],
            [Addresses.Delete] = ["Admin"],

            // Dashboard
            [Dashboard.Read] = ["Admin", "Staff", "Customer"],
        };
    }
}
