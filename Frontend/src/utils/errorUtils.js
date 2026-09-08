// Utility to safely extract human-readable error messages from any API error response
// (handles RFC 7807 ProblemDetails objects, ASP.NET validation error dictionaries, strings, and standard Error objects)
export function getErrorMessage(err, fallback = "An unexpected error occurred.") {
  if (!err) return fallback;
  if (typeof err === "string") return err;

  const data = err.response?.data;
  if (typeof data === "string") return data;

  if (data && typeof data === "object") {
    // 1. Validation dictionary: { errors: { Password: ["..."], Email: ["..."] } }
    if (data.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors)
        .flat()
        .filter((msg) => typeof msg === "string" && msg.trim().length > 0);
      if (messages.length > 0) return messages.join(". ");
    }

    // 2. RFC 7807 detail: { detail: "Email already registered" }
    if (data.detail && typeof data.detail === "string") {
      return data.detail;
    }

    // 3. Message property: { message: "..." }
    if (data.message && typeof data.message === "string") {
      return data.message;
    }

    // 4. RFC 7807 title: { title: "Conflict" }
    if (data.title && typeof data.title === "string") {
      return data.title;
    }
  }

  return err.message || fallback;
}
