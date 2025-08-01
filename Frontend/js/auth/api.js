import { getToken, logout } from "./utils.js";

const API_BASE_URL = "/api";

export async function apiRequest(path, options = {}) {
  const token = getToken();
  const headers = options.headers || {};
  if (token) headers["Authorization"] = "Bearer " + token;
  headers["Content-Type"] = "application/json";
  headers["Accept-Language"] =
    window.i18next?.language || localStorage.getItem("i18nextLng") || "en";

  const res = await fetch(API_BASE_URL + path, {
    ...options,
    headers,
  });

  let data;
  try {
    data = await res.json();
  } catch {
    data = null;
  }

  if (!res.ok) {
    if (window.errorHandler && data) {
      window.errorHandler.handleApiError(data);
    } else {
      let message = data?.message || `HTTP ${res.status}: ${res.statusText}`;
      if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
        message = window.i18next.t(message);
      }
      showToastrMessage(message, "error");
    }

    if (res.status === 401) {
      logout();
    }

    throw new Error(data?.message || "API error");
  }

  return data;
}

export async function login(credentials) {
  return apiRequest("/Auth/login", {
    method: "POST",
    body: JSON.stringify(credentials),
  });
}

export async function register(userData) {
  return apiRequest("/Auth/register", {
    method: "POST",
    body: JSON.stringify(userData),
  });
}

export async function forgotPassword(email) {
  return apiRequest("/Auth/forgot-password", {
    method: "POST",
    body: JSON.stringify({ email }),
  });
}

export async function resetPassword(token, newPassword) {
  return apiRequest("/Auth/reset-password", {
    method: "POST",
    body: JSON.stringify({ token, newPassword }),
  });
}

export async function changePassword(currentPassword, newPassword) {
  return apiRequest("/Auth/change-password", {
    method: "POST",
    body: JSON.stringify({ currentPassword, newPassword }),
  });
}

export async function validateToken() {
  return apiRequest("/Auth/validate", {
    method: "GET",
  });
}

function showToastrMessage(msg, type = "success") {
  if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
    if (typeof msg === "string" && (!msg.trim().includes(" ") || msg === msg.toUpperCase())) {
      msg = window.i18next.t(msg);
    }
  }
  
  if (typeof toastr !== "undefined") {
    toastr[type](msg);
  }
} 