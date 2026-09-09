// Utility functions to extract readable, user-friendly error messages from API and network errors
// Safely parses error formats (RFC 7807 ProblemDetails, ASP.NET validation error dictionaries, strings, Error objects)
export function getErrorMessage(err, fallback = "An unexpected error occurred.") {
  if (!err) return fallback;
  if (typeof err === "string") return err;

  const data = err.response?.data;
  if (typeof data === "string") return data;

  if (data && typeof data === "object") {
    // 1. Validation error dictionary: { errors: { Password: ["..."], Email: ["..."] } }
    if (data.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors)
        .flat()
        .filter((msg) => typeof msg === "string" && msg.trim().length > 0);
      if (messages.length > 0) return messages.join(". ");
    }

    // 2. RFC 7807 detail property: { detail: "Email already registered" }
    if (data.detail && typeof data.detail === "string") {
      return data.detail;
    }

    // 3. Simple message property: { message: "..." }
    if (data.message && typeof data.message === "string") {
      return data.message;
    }

    // 4. RFC 7807 title property: { title: "Conflict" }
    if (data.title && typeof data.title === "string") {
      return data.title;
    }
  }

  // Fallback to standard JavaScript error message or provided default message
  return err.message || fallback;
}
