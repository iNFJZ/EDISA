import { getToken, logout } from "../auth/utils.js";

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

export async function fetchUsers() {
  const res = await apiRequest("/User", { method: "GET" });
  return res.data;
}

export async function fetchDeletedUsers() {
  const res = await apiRequest("/User/deleted", { method: "GET" });
  return res.data;
}

export async function getUserById(userId) {
  return apiRequest(`/User/${userId}`, { method: "GET" });
}

export async function updateUser(userId, userData) {
  return apiRequest(`/User/${userId}`, {
    method: "PUT",
    body: JSON.stringify(userData),
  });
}

export async function deleteUser(userId) {
  return apiRequest(`/User/${userId}`, { method: "DELETE" });
}

export async function getUserByEmail(email) {
  return apiRequest(`/User/email/${email}`, { method: "GET" });
}

export async function getUserByUsername(username) {
  return apiRequest(`/User/username/${username}`, { method: "GET" });
}

export async function restoreUser(userId) {
  return apiRequest(`/User/${userId}/restore`, {
    method: "PATCH",
  });
}

export async function statistics() {
  return apiRequest("/User/statistics", { method: "GET" });
}

export async function logoutUser() {
  return apiRequest("/Auth/logout", { method: "POST" });
}

function showToastrMessage(msg, type = "success") {
  if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
    if (typeof msg === "string" && (!msg.trim().includes(" ") || msg === msg.toUpperCase())) {
      msg = window.i18next.t(msg);
    }
  }
  showToastr(msg, type);
}
