# MovieRentalApp: Quality Assurance & Acceptance Test Plan

This document outlines the complete manual and regression test scenarios for **MovieRentalApp**. It covers functional workflows, security boundaries, role-based access controls, and data integrity rules across both the React frontend and ASP.NET Core / PostgreSQL backend.

---

## 1. Authentication, Sessions & Token Security

### Test Case 1.1: Multi-Role Authentication
- **Objective:** Verify that users with different roles are correctly authenticated and routed.
- **Preconditions:** Active accounts for `Admin`, `Staff` (assigned to Store 1), and `Customer`.
- **Steps:**
  1. Navigate to `/login`.
  2. Sign in as Admin. Verify landing on `/dashboard` with full administrative KPIs (Revenue, Total Customers, Overdue Rentals).
  3. Sign out. Sign in as Staff. Verify landing on `/dashboard` with store-specific metrics (Store Active Rentals, Store Inventory).
  4. Sign out. Sign in as Customer. Verify landing on `/dashboard` displaying Customer personal cards (Active Rentals, Recent Rentals, Total Spent).
- **Expected Result:** Proper role-based redirection, navigation menus match role permissions, and user profile badge indicates active role.

### Test Case 1.2: Inactive Account Rejection
- **Objective:** Ensure deactivated users cannot access the application.
- **Preconditions:** A user account with `is_active = false`.
- **Steps:**
  1. Attempt to sign in with the inactive user's credentials.
- **Expected Result:** Login is rejected with HTTP 401 and an explicit message: *"Account is inactive. Please contact administrator."*

### Test Case 1.3: Silent Token Refresh via Interceptor
- **Objective:** Validate that token expiry does not interrupt user workflows.
- **Preconditions:** User is logged in; access token lifetime is short (or manually expired in DevTools).
- **Steps:**
  1. Wait for access token expiry.
  2. Navigate between tables or trigger a pagination event.
- **Expected Result:**
  - Axios response interceptor intercepts the 401 response.
  - Automatically issues a POST to `/api/Auth/refresh` sending the secure HttpOnly cookie.
  - Receives a fresh JWT, updates `localStorage`, and replays the original request seamlessly without user redirection.

### Test Case 1.4: Secure Logout & Refresh Token Revocation
- **Objective:** Guarantee that logging out revokes the session on both client and database.
- **Preconditions:** User is logged in.
- **Steps:**
  1. Click **Logout** from the top-right profile menu.
  2. Inspect database: `SELECT refresh_token, refresh_token_expiry FROM "user" WHERE user_id = <id>;`.
  3. Attempt to hit `/api/Auth/refresh` directly via Postman or browser console.
  4. Navigate to `http://localhost:5173/dashboard`.
- **Expected Result:**
  - Database `refresh_token` is set to `NULL`.
  - Refresh cookie is cleared.
  - Client is redirected to `/login`, and protected routes bounce back to login.

---

## 2. Rental Lifecycle & Inventory Availability

### Test Case 2.1: Renting an Available Copy
- **Objective:** Verify rental creation and immediate status synchronization.
- **Preconditions:** Inventory copy (e.g., Copy #1) has all previous rentals returned.
- **Steps:**
  1. Go to **Rentals** &rarr; click **Add Rental**.
  2. Select Inventory Copy #1, a valid Customer, and Staff. Submit the form.
- **Expected Result:**
  - Rental is created with `rental_date = UtcNow` and `return_date = null`.
  - Rental appears at the top of the Rentals table with a yellow/orange **Rented** tag.

### Test Case 2.2: Real-Time Inventory Status Toggle
- **Objective:** Verify that the Inventory table dynamically reflects active rentals.
- **Preconditions:** Test Case 2.1 completed.
- **Steps:**
  1. Navigate to **Inventory** table.
  2. Locate Copy #1.
  3. Open Filters &rarr; select Status = *"Available"* &rarr; Apply.
  4. Change filter to Status = *"Rented"* &rarr; Apply.
- **Expected Result:**
  - Copy #1 displays status **Rented** (warning tag).
  - Copy #1 is excluded when filtered by *"Available"*, and included when filtered by *"Rented"*.

### Test Case 2.3: Double-Rental Prevention Guard
- **Objective:** Ensure an already rented copy cannot be checked out simultaneously.
- **Preconditions:** Copy #1 is currently rented out (`return_date is null`).
- **Steps:**
  1. Click **Add Rental**.
  2. Attempt to select Copy #1 and submit.
- **Expected Result:**
  - Action is rejected with a validation toast/alert: *"Inventory item #1 is currently rented out and cannot be rented again until returned."*
  - No duplicate rental record is created in the database.

### Test Case 2.4: Return Process & Idempotency
- **Objective:** Confirm returning a rental stamps the date and prevents repeated returns.
- **Preconditions:** Active rental exists.
- **Steps:**
  1. Open Rental Details (`/rentals/{id}`) as Staff or Admin.
  2. Click **Mark as Returned** &rarr; Confirm the dialog.
  3. Note the newly populated `Returned On` date and green **Returned** badge.
  4. Refresh the page. Verify the *"Mark as Returned"* button is no longer present.
- **Expected Result:**
  - `return_date` is stamped with current UTC timestamp.
  - Backend idempotency check prevents duplicate return operations or overwriting historical return dates.

### Test Case 2.5: Immediate Inventory Re-Availability
- **Objective:** Validate that returned copies immediately show as available.
- **Preconditions:** Test Case 2.4 completed for Copy #1.
- **Steps:**
  1. Navigate back to **Inventory** and locate Copy #1.
- **Expected Result:**
  - Status immediately reflects green **Available**.
  - Copy #1 appears in the *"Available"* filter list.

---

## 3. Role-Based Access Control (RBAC) & IDOR Boundaries

### Test Case 3.1: Customer Self-Service Boundary (IDOR Check)
- **Objective:** Ensure customer accounts cannot view or tamper with other customers' rentals.
- **Preconditions:** Customer A (ID: 1) and Customer B (ID: 2) each have rental records.
- **Steps:**
  1. Log in as Customer A.
  2. Navigate to `/rentals`. Observe the list.
  3. Attempt direct navigation to `/rentals/{Rental_ID_belonging_to_Customer_B}`.
- **Expected Result:**
  - `/rentals` only displays rentals where `customer_id = 1`.
  - Direct navigation returns 404 or *"Rental not found"*; customer cannot see Customer B's records.

### Test Case 3.2: Customer UI Action Restrictions
- **Objective:** Ensure mutating action buttons are hidden from Customer view.
- **Preconditions:** Logged in as Customer.
- **Steps:**
  1. Inspect `/rentals`, `/movies`, `/inventory`, `/payments`.
- **Expected Result:**
  - No *"Add Rental"*, *"Add Movie"*, *"Add Copy"*, *"Add Payment"*, *"Edit"*, or *"Delete"* buttons are visible.
  - Rental detail page displays no *"Mark as Returned"* action.

### Test Case 3.3: Staff Store Boundary Enforcement
- **Objective:** Prevent staff from managing or returning rentals outside their store.
- **Preconditions:** Staff user assigned to Store 1.
- **Steps:**
  1. Log in as Store 1 Staff.
  2. Attempt to create a rental for an inventory copy belonging to Store 2.
  3. Attempt to return a rental that originated at Store 2.
- **Expected Result:**
  - Rental creation is rejected: *"Staff can only create rentals for inventory belonging to their assigned store."*
  - Rental return is rejected: *"Staff can only process returns for rentals belonging to their assigned store."*

---

## 4. User Role Transitions & Historical Integrity

### Test Case 4.1: Promoting Customer with Existing History to Staff
- **Objective:** Verify role transition does not violate PostgreSQL foreign key constraints.
- **Preconditions:** A Customer user who has past rentals or payment records in the database.
- **Steps:**
  1. Log in as Admin &rarr; navigate to **Users** (`/users`).
  2. Edit the Customer user &rarr; change Role from `Customer` to `Staff`.
  3. Select an assigned Store for the staff member and save.
- **Expected Result:**
  - The update succeeds with HTTP 200 without PostgreSQL exception `23503` (foreign key violation).
  - The old `customer` record's `user_id` is safely unlinked (`NULL`), preserving historical financial and rental relations.
  - A new `staff` record is created for the user.

### Test Case 4.2: Role Transition for New Customer Without History
- **Objective:** Verify role change cleans up unneeded empty profiles.
- **Preconditions:** Newly created Customer user with zero rentals/payments.
- **Steps:**
  1. Change role from `Customer` to `Staff`.
- **Expected Result:**
  - Empty `customer` row is deleted cleanly and staff profile is created.

---

## 5. UI Consistency, Sorting & Detail Navigation

### Test Case 5.1: Terminology Uniformity ("Movie" vs "Film")
- **Objective:** Ensure user-facing UI consistently uses "Movie".
- **Steps:**
  1. Verify Table Headers across `/movies`, `/inventory`, `/rentals`, `/actors`, `/categories`.
  2. Verify Dialog titles (e.g., *"Add Movie"*), buttons, and search placeholders (*"Search by movie title..."*).
- **Expected Result:**
  - All public facing UI displays **Movie**. No occurrences of "Film" in buttons, headers, or messages.

### Test Case 5.2: Actor Sorting
- **Objective:** Confirm server-side sorting works for all name permutations.
- **Steps:**
  1. Go to `/actors`.
  2. Click sort on **First Name** (Ascending / Descending).
  3. Click sort on **Last Name** (Ascending / Descending).
  4. Click sort on **Full Name** (Ascending / Descending).
- **Expected Result:**
  - All sort transitions complete smoothly with 0 backend exceptions.

### Test Case 5.3: Detail Page Deep Linking
- **Objective:** Ensure relational navigation connects entities smoothly.
- **Steps:**
  1. Open a Movie detail page (`/movies/:id`). Click on an Actor chip &rarr; routes to `/actors/:id`.
  2. In Actor detail page, click on a Movie title in the filmography list &rarr; routes back to `/movies/:id`.
  3. In Movie detail page, click on a Category chip &rarr; routes to `/categories/:id`.
- **Expected Result:**
  - All entity relationships link seamlessly without broken routes or missing parameter errors.

---

## 6. Payments & Revenue Accounting

### Test Case 6.1: Record Payment for Rental
- **Objective:** Validate payment entry and link to rental.
- **Steps:**
  1. In a rental detail page or via Payments dialog, create a payment of $4.99 for a rental.
  2. Navigate to `/payments` and find the newly logged transaction.
- **Expected Result:**
  - Payment record appears with correct amount, customer name, and staff name.

### Test Case 6.2: Payment Range & Date Filtering
- **Objective:** Verify server-side payment parameter filtering.
- **Steps:**
  1. Open Payments filter dialog.
  2. Set Min Amount = $3.00, Max Amount = $5.00.
  3. Apply filters.
- **Expected Result:**
  - Only records within the $3.00 - $5.00 range are rendered.
  - Filter count badge updates to show active filter count.

### Test Case 6.3: Dashboard Financial KPI Reflection
- **Objective:** Ensure financial actions update analytical dashboard cards.
- **Steps:**
  1. Note current **Total Revenue** on Admin Dashboard.
  2. Add a payment of $10.00.
  3. Return to Dashboard.
- **Expected Result:**
  - Total Revenue KPI increases by exactly $10.00.
