// Admin Authentication Handler - Common for all admin pages
class AdminAuth {
  constructor() {
    this.isInitialized = false;
    this.init();
  }

  init() {
    if (this.isInitialized) return;
    this.checkAuthentication();
    this.setupLogoutHandler();
    this.setupGlobalErrorHandling();
    this.isInitialized = true;
  }

  checkAuthentication() {
    const token = localStorage.getItem("authToken");
    const currentPath = window.location.pathname;
    const isAdminPage =
      currentPath.includes("/admin/") || currentPath.includes("index.html");

    if (!isAdminPage) {
      return true;
    }

    if (!token) {
      this.redirectToLogin();
      return false;
    }

    this.validateToken(token).catch((error) => {
      if (error && error.message && error.message.includes("invalid")) {
        this.redirectToLogin();
      }
    });

    return true;
  }

  async validateToken(token) {
    try {
              const response = await fetch("/api/Auth/validate", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ token: token }),
      });
      if (!response.ok) {
        return false;
      }
      const data = await response.json();
      if (!data.isValid) {
        localStorage.removeItem("authToken");
        this.redirectToLogin();
        return false;
      }
      return true;
    } catch (error) {
      return false;
    }
  }

  setupLogoutHandler() {
    $(document).off("click", "#logout-btn");
    $(document).on("click", "#logout-btn", (e) => {
      e.preventDefault();
      e.stopPropagation();
      this.logout();
    });
  }

  async logout() {
    const token = localStorage.getItem("authToken");
    try {
      if (token) {
        await fetch("/api/Auth/logout", {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        });
      }
    } catch (error) {
      // Only log real errors
      console.error("Logout API error:", error);
    }
    localStorage.removeItem("authToken");
    sessionStorage.clear();
    safeToastrMessage("success", window.i18next.t("loggedOutSuccessfully"));
    setTimeout(() => {
      this.redirectToLogin();
    }, 500);
  }

  redirectToLogin() {
    window.location.href = "/auth/login.html";
  }

  setupGlobalErrorHandling() {
    $(document).ajaxError((event, xhr, settings) => {
      if (xhr.status === 401) {
        localStorage.removeItem("authToken");
        this.redirectToLogin();
      }
    });
  }

  getAuthToken() {
    return localStorage.getItem("authToken");
  }

  isAuthenticated() {
    return !!localStorage.getItem("authToken");
  }

  getAuthHeaders() {
    const token = this.getAuthToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  getCurrentUserInfo() {
    const token = this.getAuthToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(
        atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")),
      );
      return {
        id: payload.sub,
        email: payload.email,
        fullName: payload.name,
      };
    } catch (e) {
      console.error("Failed to parse JWT token:", e);
      return null;
    }
  }

  async updateUserProfileDisplay() {
    const userInfo = this.getCurrentUserInfo();
    $(".avatar-initial").remove();
    $(".user-avatar").attr("src", "").hide();
    $(".user-name").text("");
    $(".user-role").text("");
    
    function showFallbackAvatar(letter, seed = "default") {
      let hash = 0;
      for (let i = 0; i < seed.length; i++) {
        const char = seed.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash;
      }
      
      const hue = Math.abs(hash) % 360;
      const saturation = 70 + (Math.abs(hash) % 20);
      const lightness = 45 + (Math.abs(hash) % 15);
      
      const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
      const svg = `<svg width='40' height='40' xmlns='http://www.w3.org/2000/svg'><circle cx='20' cy='20' r='20' fill='${color}'/><text x='50%' y='50%' text-anchor='middle' dy='.35em' font-family='Arial' font-size='20' fill='#fff'>${letter}</text></svg>`;
      const dataUrl =
        "data:image/svg+xml;base64," + btoa(unescape(encodeURIComponent(svg)));
      $(".user-avatar").attr("src", dataUrl).show();
    }
    
    if (!userInfo) {
      showFallbackAvatar("A", "default");
      return;
    }
    
    let fallbackLetter = (
      userInfo.username ||
      userInfo.email ||
      userInfo.fullName ||
      "A"
    )
      .charAt(0)
      .toUpperCase();
    const seed = userInfo.email || userInfo.username || userInfo.fullName || "default";
    showFallbackAvatar(fallbackLetter, seed);
    $(".user-name").text(userInfo.fullName || userInfo.email || "Admin");
    $(".user-role").text("Admin");
    
    try {
      const response = await fetch(
        `/api/User/${userInfo.id}`,
        {
          headers: this.getAuthHeaders(),
        },
      );
      if (response.ok) {
        const data = await response.json();
        if (data.success && data.data) {
          const user = data.data;
          if (user.profilePicture && user.profilePicture.trim() !== "") {
            const isGooglePicture = user.profilePicture.includes('googleusercontent.com');
            
            const img = new Image();
            img.onload = function() {
              $(".user-avatar").attr("src", user.profilePicture).show();
            };
            img.onerror = function() {
              showFallbackAvatar(fallbackLetter, seed);
            };
            
            if (isGooglePicture) {
              const canvas = document.createElement('canvas');
              const ctx = canvas.getContext('2d');
              const tempImg = new Image();
              tempImg.crossOrigin = 'anonymous';
              tempImg.onload = function() {
                canvas.width = tempImg.width;
                canvas.height = tempImg.height;
                ctx.drawImage(tempImg, 0, 0);
                const dataUrl = canvas.toDataURL('image/png');
                $(".user-avatar").attr("src", dataUrl).show();
              };
              tempImg.onerror = function() {
                showFallbackAvatar(fallbackLetter, seed);
              };
              tempImg.src = user.profilePicture;
            } else {
              img.src = user.profilePicture;
            }
            
            setTimeout(() => {
              if (img.complete === false || img.naturalWidth === 0) {
                showFallbackAvatar(fallbackLetter, seed);
              }
            }, 2000);
          } else {
            showFallbackAvatar(fallbackLetter, seed);
          }
          if (user.fullName && user.fullName.trim() !== "") {
            $(".user-name").text(user.fullName);
          } else if (user.email) {
            $(".user-name").text(user.email);
          }
        } else {
          showFallbackAvatar(fallbackLetter, seed);
        }
      } else {
        showFallbackAvatar(fallbackLetter, seed);
      }
    } catch (error) {
      showFallbackAvatar(fallbackLetter, seed);
    }
  }
}

$(document).ready(async () => {
  try {
    window.adminAuth = new AdminAuth();
    if (window.adminAuth) {
      window.adminAuth.init();
      await window.adminAuth.updateUserProfileDisplay();
    }
  } catch (error) {}
});

if (typeof module !== "undefined" && module.exports) {
  module.exports = AdminAuth;
}

function loadActiveUsersTable() {
  const token = localStorage.getItem("authToken");
  if (!token) {
    window.location.href = "/auth/login.html";
    return;
  }
  const dt_user_table = $(".datatables-users");
  if (dt_user_table.length) {
    if ($.fn.DataTable.isDataTable(dt_user_table)) {
      dt_user_table.DataTable().destroy();
    }
    dt_user_table.DataTable({
      serverSide: true,
      processing: true,
      ajax: {
        url: "/api/User",
        type: "GET",
        data: function (d) {
          return {
            page: Math.floor(d.start / d.length) + 1,
            pageSize: d.length,
            search: d.search.value || null,
            sortBy: d.columns[d.order0?.column]?.data || null,
            sortOrder: d.order0 || "asc",
          };
        },
        dataSrc: function (json) {
          if (!json || !Array.isArray(json.data)) return [];

          if (json.pagination) {
            const totalCount = json.pagination.totalCount;
            updateUserStatsDashboard();
          }

          return json.data;
        },
        dataFilter: function (data) {
          var json = JSON.parse(data);
          if (json.pagination) {
            json.recordsTotal = json.pagination.totalCount;
            json.recordsFiltered = json.pagination.totalCount;
          }
          return JSON.stringify(json);
        },
        beforeSend: function (xhr) {
          if (token) xhr.setRequestHeader("Authorization", "Bearer " + token);
        },
      },
      columns: [
        { data: null, defaultContent: "" }, // Avatar
        { data: "username" },
        { data: "fullName" },
        { data: "email" },
        { data: "status" },
        { data: "lastLoginAt" },
        { data: null, defaultContent: "" }, // Actions
      ],
      columnDefs: getUserTableColumnDefs(),
      order: [[1, "asc"]],
      dom: getUserTableDom(),
      language: getUserTableLanguage(),
      buttons: getUserTableButtons(),
      rowCallback: function (row, data) {
        $(row).attr("data-userid", data.id);
      },
      initComplete: function () {
        translateUserTableHeaders();
      },
    });
    if (window.i18next) {
      window.i18next.on && window.i18next.on("languageChanged", function () {
        translateUserTableHeaders();
      });
    }
  }
}

function loadAllUsersTable() {
  const token = localStorage.getItem("authToken");
  if (!token) {
    window.location.href = "/auth/login.html";
    return;
  }
  const dt_user_table = $(".datatables-users");
  if (dt_user_table.length) {
    if ($.fn.DataTable.isDataTable(dt_user_table)) {
      dt_user_table.DataTable().destroy();
    }
    dt_user_table.DataTable({
      serverSide: true,
      processing: true,
      ajax: {
        url: "/api/User?includeDeleted=true",
        type: "GET",
        data: function (d) {
          return {
            page: Math.floor(d.start / d.length) + 1,
            pageSize: d.length,
            search: d.search.value || null,
            sortBy: d.columns[d.order0?.column]?.data || null,
            sortOrder: d.order0 || "asc",
          };
        },
        dataSrc: function (json) {
          if (!json || !Array.isArray(json.data)) {
            return [];
          }
          const users = json.data;
          let total = users.length,
            active = 0,
            inactive = 0,
            suspended = 0,
            banned = 0;
          users.forEach((u) => {
            if (u.status === 1 || u.status === "Active") active++;
            else if (u.status === 2 || u.status === "Inactive") inactive++;
            else if (u.status === 3 || u.status === "Suspended") suspended++;
            else if (u.status === 4 || u.status === "Banned") banned++;
          });
          $("#total-users").text(total);
          $("#active-users").text(active);
          $("#inactive-users").text(inactive);
          $("#banned-users").text(banned);
          return users;
        },
        dataFilter: function (data) {
          var json = JSON.parse(data);
          if (json.pagination) {
            json.recordsTotal = json.pagination.totalCount;
            json.recordsFiltered = json.pagination.totalCount;
          }
          return JSON.stringify(json);
        },
        beforeSend: function (xhr) {
          const token = localStorage.getItem("authToken");
          if (token) xhr.setRequestHeader("Authorization", "Bearer " + token);
        },
      },
      columns: [
        { data: null },
        { data: "username" },
        { data: "fullName" },
        { data: "email" },
        // { data: "phoneNumber" },
        { data: "status" },
        { data: null },
        { data: null },
        { data: null },
      ],
      columnDefs: getUserTableColumnDefs(),
      createdRow: function (row, data) {
        $(row).attr("data-userid", data.id);
      },
      order: [[1, "asc"]],
      dom: getUserTableDom(),
      language: getUserTableLanguage(),
      buttons: getUserTableButtons(),
    });
  }
}

function loadDeactiveUsersTable() {
  const token = localStorage.getItem("authToken");
  if (!token) {
    window.location.href = "/auth/login.html";
    return;
  }
  const dt_user_table = $(".datatables-users");
  if (dt_user_table.length) {
    if ($.fn.DataTable.isDataTable(dt_user_table)) {
      dt_user_table.DataTable().destroy();
    }
    dt_user_table.DataTable({
      serverSide: true,
      processing: true,
      ajax: {
        url: "/api/User?includeDeleted=true",
        type: "GET",
        data: function (d) {
          return {
            page: Math.floor(d.start / d.length) + 1,
            pageSize: d.length,
            search: d.search.value || null,
            sortBy: d.columns[d.order0?.column]?.data || null,
            sortOrder: d.order0 || "asc",
            status: 4,
            deletedAt: !null,
            includeDeleted: true,
          };
        },
        dataSrc: function (json) {
          if (!json || !Array.isArray(json.data)) return [];

          if (json.pagination) {
            const totalCount = json.pagination.totalCount;
            updateUserStatsDashboard();
          }

          return json.data;
        },
        dataFilter: function (data) {
          var json = JSON.parse(data);
          if (json.pagination) {
            json.recordsTotal = json.pagination.totalCount;
            json.recordsFiltered = json.pagination.totalCount;
          }
          return JSON.stringify(json);
        },
        beforeSend: function (xhr) {
          if (token) xhr.setRequestHeader("Authorization", "Bearer " + token);
        },
      },
      columns: [
        { data: null, defaultContent: "" }, // Avatar
        { data: "username" },
        { data: "fullName" },
        { data: "email" },
        { data: "status" },
        { data: "deletedAt" },
        { data: null, defaultContent: "" }, // Actions
      ],
      columnDefs: getUserTableColumnDefs(true),
      order: [[1, "asc"]],
      dom: getUserTableDom(),
      language: getUserTableLanguage(),
      buttons: getUserTableButtons(),
      rowCallback: function (row, data) {
        $(row).attr("data-userid", data.id);
      },
      initComplete: function () {
        translateUserTableHeaders();
      },
    });
    if (window.i18next) {
      window.i18next.on && window.i18next.on("languageChanged", function () {
        translateUserTableHeaders();
      });
    }
  }
}

function getUserTableColumnDefs(isDeactive) {
  return [
    {
      targets: 0, // Avatar
      render: function (data, type, full) {
        const letter = (full.email || "").charAt(0).toUpperCase();
        const seed = full.email || full.username || full.fullName || "default";
        
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
          const char = seed.charCodeAt(i);
          hash = ((hash << 5) - hash) + char;
          hash = hash & hash;
        }
        
        const hue = Math.abs(hash) % 360;
        const saturation = 70 + (Math.abs(hash) % 20);
        const lightness = 45 + (Math.abs(hash) % 15);
        
        const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
        
        const fallbackAvatar = `<div class="avatar-initial rounded-circle" style="width:36px;height:36px;background:${color};color:#fff;display:flex;align-items:center;justify-content:center;font-weight:bold;font-size:18px;">${letter}</div>`;
        
        if (full.profilePicture && full.profilePicture.trim() !== "") {
          const isGooglePicture = full.profilePicture.includes('googleusercontent.com');
          
          if (isGooglePicture) {
            return `<div style="position:relative;width:36px;height:36px;">
              ${fallbackAvatar}
              <img src="${full.profilePicture}" alt="User Avatar" class="rounded-circle" style="width:36px;height:36px;object-fit:cover;position:absolute;top:0;left:0;opacity:0;transition:opacity 0.3s;" onerror="this.style.opacity='0';" onload="this.style.opacity='1'; this.previousElementSibling.style.display='none';">
            </div>`;
          } else {
            return `<div style="position:relative;width:36px;height:36px;">
              ${fallbackAvatar}
              <img src="${full.profilePicture}" alt="User Avatar" class="rounded-circle" style="width:36px;height:36px;object-fit:cover;position:absolute;top:0;left:0;opacity:0;transition:opacity 0.3s;" onerror="this.style.opacity='0';" onload="this.style.opacity='1'; this.previousElementSibling.style.display='none';">
            </div>`;
          }
        } else {
          return fallbackAvatar;
        }
      },
    },
    {
      targets: 1, // Username
      render: function (data, type, full) {
        return full.username || "";
      },
    },
    {
      targets: 2, // Full Name
      render: function (data, type, full) {
        return full.fullName || "";
      },
    },
    {
      targets: 3, // Email
      render: function (data, type, full) {
        return full.email || "";
      },
    },
    {
      targets: 4, // Status
      render: function (data, type, full) {
        var $status = full.status;
        var statusObj = {
          1: { title: window.i18next.t("active"), class: "bg-label-success" },
          2: {
            title: window.i18next.t("inactive"),
            class: "bg-label-secondary",
          },
          3: {
            title: window.i18next.t("suspended"),
            class: "bg-label-warning",
          },
          4: { title: window.i18next.t("banned"), class: "bg-label-danger" },
        };
        var obj = statusObj[$status] || {
          title: window.i18next.t("unknown"),
          class: "bg-label-secondary",
        };
        return `<span class="badge ${obj.class}" text-capitalized>${obj.title}</span>`;
      },
    },
    {
      targets: isDeactive ? 5 : 5,
      render: function (data, type, full) {
        if (isDeactive) {
          if (full.deletedAt || full.DeletedAt) {
            return new Date(full.deletedAt || full.DeletedAt).toLocaleString(
              "en-GB",
              {
                year: "numeric",
                month: "2-digit",
                day: "2-digit",
                hour: "2-digit",
                minute: "2-digit",
                second: "2-digit",
              },
            );
          } else {
            return '<span class="text-muted">N/A</span>';
          }
        } else {
          if (full.lastLoginAt) {
            return new Date(full.lastLoginAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            });
          } else {
            return '<span class="text-muted">N/A</span>';
          }
        }
      },
    },
    {
      targets: isDeactive ? 6 : 6, // Actions
      title: window.i18next.t("actions"),
      searchable: false,
      orderable: false,
      render: function (data, type, row, meta) {
        let html = "";
        html += `<a href="javascript:;" class="text-body view-user" title="${window.i18next.t("viewUser")}" data-bs-toggle="tooltip"><i class="ti ti-eye text-primary me-1"></i></a>`;
        if (isDeactive) {
          html += `<a href="javascript:;" class="text-body restore-user" title="${window.i18next.t("restoreUser")}" data-bs-toggle="tooltip"><i class="ti ti-refresh text-success me-1"></i></a>`;
        } else {
          html += `<a href="javascript:;" class="text-body edit-user" title="${window.i18next.t("editUser")}" data-bs-toggle="tooltip"><i class="ti ti-edit text-primary me-1"></i></a>`;
          html += `<a href="javascript:;" class="text-body delete-user" title="${window.i18next.t("deleteUser")}" data-bs-toggle="tooltip"><i class="ti ti-trash text-danger me-1"></i></a>`;
        }
        return html;
      },
    },
  ];
}

function getUserTableDom() {
  return '<"row me-2"<"col-md-2"<"me-3"l>><"col-md-10"<"dt-action-buttons text-xl-end text-lg-start text-md-end text-start d-flex align-items-center justify-content-end flex-md-row flex-column mb-3 mb-md-0"fB>>>t<"row mx-2"<"col-sm-12 col-md-6"i><"col-sm-12 col-md-6"p>>';
}

function getUserTableLanguage() {
  const getTranslation = (key) => {
    if (window.i18next && window.i18next.isInitialized) {
      return window.i18next.t(key);
    }
    const fallbacks = {
      searchPlaceholder: "Search...",
      export: "Export",
      previous: "Previous",
      next: "Next",
      showing: "Showing",
      to: "to",
      of: "of",
      entries: "entries",
      loading: "Loading...",
      noData: "No data available",
      info: "Showing _START_ to _END_ of _TOTAL_ entries",
      infoEmpty: "Showing 0 to 0 of 0 entries",
      infoFiltered: "(filtered from _MAX_ total entries)",
      lengthMenu: "Show _MENU_ entries",
      zeroRecords: "No matching records found",
    };
    return fallbacks[key] || key;
  };

  return {
    searchPlaceholder: getTranslation("searchPlaceholder"),
    sLengthMenu: getTranslation("lengthMenu"),
    search: "",
    paginate: {
      previous: getTranslation("previous"),
      next: getTranslation("next"),
    },
    info: getTranslation("info"),
    infoEmpty: getTranslation("infoEmpty"),
    infoFiltered: getTranslation("infoFiltered"),
    zeroRecords: getTranslation("zeroRecords"),
    processing: getTranslation("loading"),
    emptyTable: getTranslation("noData"),
  };
}

function getUserTableButtons() {
  const getTranslation = (key) => {
    if (window.i18next && window.i18next.isInitialized) {
      return window.i18next.t(key);
    }
    const fallbacks = {
      export: "Export",
      print: "Print",
      csv: "CSV",
      excel: "Excel",
      pdf: "PDF",
      copy: "Copy",
    };
    return fallbacks[key] || key;
  };

  async function getUserDataForExport(dt) {
    try {
      const token = localStorage.getItem("authToken");

      const countResponse = await fetch(
        "/api/User?includeDeleted=true&page=1&pageSize=1",
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        },
      );

      if (!countResponse.ok) {
        safeToastrMessage("error", window.i18next.t("failedToLoadData"));
        return null;
      }

      const countResult = await countResponse.json();
      const totalCount =
        countResult.pagination?.totalCount || countResult.totalCount || 0;

      if (totalCount === 0) {
        safeToastrMessage("warning", window.i18next.t("noDataToExport"));
        return null;
      }

      const response = await fetch(
        `/api/User?includeDeleted=true&pageSize=${totalCount}`,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        },
      );

      if (!response.ok) {
        safeToastrMessage("error", window.i18next.t("failedToLoadData"));
        return null;
      }

      const result = await response.json();
      let data = result.data || result || [];

      const currentPath = window.location.pathname;
      if (currentPath.includes("deactive-users")) {
        data = data.filter(
          (u) => (u.status === 4 || u.status === "Banned") && u.deletedAt,
        );
      } else if (currentPath.includes("active-users")) {
        data = data.filter((u) => u.status === 1 || u.status === "Active");
      }

      if (!data || !data.length) {
        safeToastrMessage("warning", window.i18next.t("noDataToExport"));
        return null;
      }

      const allFields = [
        "id",
        "username",
        "fullName",
        "email",
        "phoneNumber",
        "status",
        "isVerified",
        "lastLoginAt",
        "deletedAt",
        "dateOfBirth",
        "address",
        "bio",
        "loginProvider",
        "profilePicture",
        "createdAt",
      ];

      const headers = allFields.map((key) => {
        switch (key) {
          case "id":
            return window.i18next.t("id");
          case "username":
            return window.i18next.t("username");
          case "fullName":
            return window.i18next.t("fullName");
          case "email":
            return window.i18next.t("email");
          case "phoneNumber":
            return window.i18next.t("phone");
          case "status":
            return window.i18next.t("status");
          case "isVerified":
            return window.i18next.t("verified");
          case "lastLoginAt":
            return window.i18next.t("lastLogin");
          case "deletedAt":
            return window.i18next.t("deletedAt");
          case "dateOfBirth":
            return window.i18next.t("dateOfBirth");
          case "address":
            return window.i18next.t("address");
          case "bio":
            return window.i18next.t("bio");
          case "loginProvider":
            return window.i18next.t("provider");
          case "profilePicture":
            return window.i18next.t("profilePicture");
          case "createdAt":
            return window.i18next.t("createdAt");
          default:
            return key;
        }
      });

      const rows = data.map((u) =>
        allFields.map((key) => {
          let val = u[key];
          if (key === "status") {
            switch (val) {
              case 1:
                return window.i18next.t("active");
              case 2:
                return window.i18next.t("inactive");
              case 3:
                return window.i18next.t("suspended");
              case 4:
                return window.i18next.t("banned");
              default:
                return window.i18next.t("unknown");
            }
          }
          if (key === "isVerified")
            return val ? window.i18next.t("yes") : window.i18next.t("no");
          if (
            key === "phoneNumber" &&
            (window.location.pathname.includes("active-users") ||
              window.location.pathname.includes("all-users"))
          ) {
            if (typeof val === "string" && val.trim() !== "") {
              let phone = val.trim();
              if (phone.startsWith("+84")) return phone;
              if (phone.startsWith("84")) return "+" + phone;
              if (phone.startsWith("0")) return "+84" + phone.slice(1);
              return phone;
            }
            return val || "";
          }
          if (val === undefined || val === null) return "";
          if (val instanceof Date) return val.toLocaleString("en-GB");
          if (typeof val === "string" && val.match(/^\d{4}-\d{2}-\d{2}T/)) {
            try {
              return new Date(val).toLocaleString("en-GB");
            } catch {
              return val;
            }
          }
          if (key === "profilePicture" && val)
            return val.length > 30 ? val.slice(0, 30) + "..." : val;
          return val;
        }),
      );

      return { headers, rows, allFields };
    } catch (error) {
      console.error("Export error:", error);
      safeToastrMessage("error", window.i18next.t("exportFailed"));
      return null;
    }
  }

  async function exportCSVAllFields(e, dt, button, config) {
    const exportData = await getUserDataForExport(dt);
    if (!exportData) return;

    const { headers, rows } = exportData;

    function escapeCsvValue(val) {
      if (val == null || val === undefined) return "";
      val = String(val);
      val = val.replace(/"/g, '""');
      return `"${val}"`;
    }

    const delimiter = ";";
    const headersCsv = headers.map(escapeCsvValue).join(delimiter);
    const rowsCsv = rows
      .map((r) => r.map(escapeCsvValue).join(delimiter))
      .join("\n");
    const BOM = "\uFEFF";
    const csv = BOM + headersCsv + "\n" + rowsCsv;

    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = getExportFileName("csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  async function exportExcelAllFields(e, dt, button, config) {
    const exportData = await getUserDataForExport(dt);
    if (!exportData) return;
    const { headers, rows } = exportData;
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.aoa_to_sheet([headers, ...rows]);
    const colWidths = headers.map((header, idx) => {
      const maxLength = Math.max(
        header.length,
        ...rows.map((row) => (row[idx] ? String(row[idx]).length : 0)),
      );
      return { wch: Math.min(Math.max(maxLength + 2, 10), 40) };
    });
    ws["!cols"] = colWidths;
    const range = XLSX.utils.decode_range(ws["!ref"]);
    for (let R = range.s.r; R <= range.e.r; ++R) {
      for (let C = range.s.c; C <= range.e.c; ++C) {
        const cell = ws[XLSX.utils.encode_cell({ r: R, c: C })];
        if (cell) {
          cell.s = cell.s || {};
          cell.s.border = {
            top: { style: "thin", color: { rgb: "CCCCCC" } },
            bottom: { style: "thin", color: { rgb: "CCCCCC" } },
            left: { style: "thin", color: { rgb: "CCCCCC" } },
            right: { style: "thin", color: { rgb: "CCCCCC" } },
          };
        }
      }
    }
    XLSX.utils.book_append_sheet(wb, ws, "Users");
    XLSX.writeFile(wb, getExportFileName("xlsx"));
  }

  async function exportPDFAllFields(e, dt, button, config) {
    const exportData = await getUserDataForExport(dt);
    if (!exportData) return;
    const { headers, rows, allFields } = exportData;
    const body = [headers, ...rows];

    const colWidths = allFields.map((field, idx) => {
      const headerLength = headers[idx] ? headers[idx].length : 10;
      const maxDataLength = Math.max(
        ...rows.map((row) => (row[idx] ? String(row[idx]).length : 0)),
      );
      const maxLength = Math.max(headerLength, maxDataLength);
      return Math.max(8, Math.min(15, maxLength * 0.8)) + "%";
    });

    const docDefinition = {
      pageSize: "A4",
      pageOrientation: "landscape",
      pageMargins: [10, 15, 10, 15],
      defaultStyle: {
        font: "Roboto",
        fontSize: 7,
      },
      header: {
        text: window.i18next.t("userList"),
        style: {
          alignment: "center",
          fontSize: 12,
          bold: true,
          margin: [0, 5, 0, 10],
        },
      },
      footer: function (currentPage, pageCount) {
        return {
          text:
            window.i18next.t("page") +
            " " +
            currentPage.toString() +
            " / " +
            pageCount,
          alignment: "center",
          fontSize: 8,
          margin: [0, 10, 0, 5],
        };
      },
      content: [
        {
          table: {
            headerRows: 1,
            widths: colWidths,
            body: body,
          },
          layout: {
            hLineWidth: function () {
              return 0.3;
            },
            vLineWidth: function () {
              return 0.3;
            },
            hLineColor: function () {
              return "#ddd";
            },
            vLineColor: function () {
              return "#ddd";
            },
            paddingLeft: function () {
              return 3;
            },
            paddingRight: function () {
              return 3;
            },
            paddingTop: function () {
              return 2;
            },
            paddingBottom: function () {
              return 2;
            },
          },
        },
      ],
      styles: {
        header: {
          fontSize: 8,
          bold: true,
          fillColor: "#f8f9fa",
          color: "#333",
        },
      },
    };
    if (typeof pdfMake !== "undefined") {
      pdfMake.createPdf(docDefinition).download(getExportFileName("pdf"));
    } else {
      safeToastrMessage("error", window.i18next.t("pdfLibraryNotLoaded"));
    }
  }

  return [
    {
      extend: "collection",
      className: "btn btn-label-secondary dropdown-toggle mx-3",
      text:
        '<i class="ti ti-screen-share me-1 ti-xs"></i>' +
        getTranslation("export"),
      buttons: [
        {
          extend: "copyHtml5",
          text: '<i class="ti ti-copy me-2" ></i>' + getTranslation("copy"),
          exportOptions: { columns: ":visible" },
        },
        {
          text: '<i class="ti ti-file-text me-2"></i>' + getTranslation("csv"),
          action: exportCSVAllFields,
        },
        {
          text:
            '<i class="ti ti-file-spreadsheet me-2"></i>' +
            getTranslation("excel"),
          action: exportExcelAllFields,
        },
        {
          text:
            '<i class="ti ti-file-code-2 me-2"></i>' + getTranslation("pdf"),
          action: exportPDFAllFields,
        },
        {
          extend: "print",
          text: '<i class="ti ti-printer me-2" ></i>' + getTranslation("print"),
          exportOptions: { columns: ":visible" },
        },
      ],
    },
  ];
}

function isValidEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function getCurrentLanguage() {
  return window.i18next?.language || localStorage.getItem("i18nextLng") || "en";
}

function showUsernameSuggestionModal(original, suggested, onAccept, onReject) {
  const msg = window.i18next.t("usernameSuggestionMessage").replace("{original}", original).replace("{suggested}", suggested);
  const modalHtml = `
    <div class="modal fade" id="usernameSuggestionModal" tabindex="-1" aria-labelledby="usernameSuggestionModalLabel" aria-hidden="true">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="usernameSuggestionModalLabel">${window.i18next.t("usernameSuggestionTitle")}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">${msg}</div>
          <div class="modal-footer">
            <button type="button" class="btn btn-primary" id="acceptSuggestedUsernameBtn">${window.i18next.t("yes")}</button>
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${window.i18next.t("no")}</button>
          </div>
        </div>
      </div>
    </div>`;
  $("body").append(modalHtml);
  const modal = new bootstrap.Modal(document.getElementById("usernameSuggestionModal"));
  modal.show();
  $("#acceptSuggestedUsernameBtn").on("click", function () {
    modal.hide();
    setTimeout(() => {
      $("#usernameSuggestionModal").remove();
      onAccept();
    }, 150);
  });
  $("#usernameSuggestionModal").on("hidden.bs.modal", function () {
    $("#usernameSuggestionModal").remove();
    if (onReject) onReject();
  });
}

function handleAddUser() {
  const form = document.getElementById("addNewUserForm");
  if (!form) return;
  const formData = new FormData(form);
  const username = formData.get("username")?.trim();
  const fullName = formData.get("fullName")?.trim();
  const email = formData.get("email")?.trim();
  const phoneNumber = formData.get("phoneNumber")?.trim();
  const password = formData.get("password")?.trim();

  if (!username) {
    safeToastrMessage("error", window.i18next.t("usernameRequired"));
    return;
  }
  if (!/^[a-zA-Z0-9]+$/.test(username)) {
    safeToastrMessage("error", window.i18next.t("usernameInvalidFormat"));
    return;
  }
  if (!fullName) {
    safeToastrMessage("error", window.i18next.t("fullNameRequired"));
    return;
  }
  if (!email || !isValidEmail(email)) {
    safeToastrMessage("error", window.i18next.t("validEmailRequired"));
    return;
  }
  if (!password || password.length < 6) {
    safeToastrMessage("error", window.i18next.t("passwordRequired"));
    return;
  }
  const data = { username, fullName, email, password };
  let phone = phoneNumber;
  if (phone && phone.startsWith("+84")) phone = "0" + phone.slice(3);
  else if (phone && phone.startsWith("84")) phone = "0" + phone.slice(2);
  if (phone) data.phoneNumber = phone;
  const token = localStorage.getItem("authToken");

    fetch("/api/Auth/register", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  })
    .then((res) => {
      if (!res.ok) {
        return res.json().then((errorData) => {
          throw { status: res.status, ...errorData };
        });
      }
      return res.json();
    })
    .then((res) => {
      if (res.success || res.id) {
        safeToastrMessage("clear");
        let backendUsername = res.username || username;
        if (backendUsername !== username) {
          safeToastrMessage(
            "success",
            window.i18next
              .t("userAddedSuccessfullyWithNewUsername")
              .replace("{old}", username)
              .replace("{new}", backendUsername),
          );
        } else {
          safeToastrMessage("success", window.i18next.t("userAddedSuccessfully"));
        }
        form.reset();
        $("#offcanvasAddUser").offcanvas("hide");
        safeToastrMessage("clear");
        const dt_user_table = $(".datatables-users");
        if (dt_user_table.length && $.fn.DataTable.isDataTable(dt_user_table)) {
          dt_user_table.DataTable().ajax.reload(null, false);
        } else {
          setTimeout(() => window.location.reload(), 1000);
        }
      } else {
        safeToastrMessage("clear");
        safeToastrMessage("error", res.message || window.i18next.t("addUserFailed"));
      }
    })
    .catch((error) => {
      console.error("Add user error:", error);
      if (error.status === 400) {
        if (error.message && error.message.toLowerCase().includes("email") && error.message.toLowerCase().includes("already exists")) {
          safeToastrMessage(
            "error",
            window.i18next.t("userAlreadyExists").replace("{email}", email),
          );
        } else if (error.errorCode === "USERNAME_ALREADY_EXISTS" && error.suggestedUsername) {
          showUsernameSuggestionModal(username, error.suggestedUsername, async () => {
            const updatedData = {
              username: error.suggestedUsername,
              fullName: fullName,
              email: email,
              phoneNumber: phoneNumber || null,
              password: password,
              language: getCurrentLanguage(),
              acceptSuggestedUsername: true
            };
            
            try {
              const res = await fetch("/api/Auth/register", {
                method: "POST",
                headers: {
                  Authorization: `Bearer ${token}`,
                  "Content-Type": "application/json",
                },
                body: JSON.stringify(updatedData),
              });
                if (!res.ok) {
                const errorData = await res.json();
                throw { status: res.status, ...errorData };
              }
              
              const result = await res.json();
              if (result && Object.keys(result).length > 0) {
                form.reset();
                $("#offcanvasAddUser").offcanvas("hide");
                
                const actualUsername = result.username || error.suggestedUsername;
                const successMessage = window.i18next
                  .t("userAddedSuccessfullyWithNewUsername")
                  .replace("{old}", username)
                  .replace("{new}", actualUsername);
                
                setTimeout(() => {
                  if (typeof toastr !== "undefined") {
                    toastr.success(successMessage);
                  } else if (typeof window.safeToastr === "function") {
                    window.safeToastr("success", successMessage);
                  } else {
                    alert(successMessage);
                  }
                }, 500);
                
                setTimeout(() => {
                  const dt_user_table = $(".datatables-users");
                  if (dt_user_table.length && $.fn.DataTable.isDataTable(dt_user_table)) {
                    dt_user_table.DataTable().ajax.reload(null, false);
                  } else {
                    window.location.reload();
                  }
                }, 2000);
              }
            } catch (retryError) {
              console.error("Retry add user error:", retryError);
              safeToastrMessage("error", window.i18next.t("addUserFailed"));
            }
          }, () => {
            safeToastrMessage("info", window.i18next.t("userCancelledUsernameSuggestion"));
          });
        } else if (error.message) {
          safeToastrMessage("error", error.message);
        } else {
          safeToastrMessage("error", window.i18next.t("addUserFailed"));
        }
      } else {
        safeToastrMessage("error", window.i18next.t("addUserFailed"));
      }
    });
}

let cropper = null;
let selectedImageFile = null;
let windowCropper = null;
let currentZoom = 1;
let cropperReady = false;
let initialCropBoxWidth = null;

$(document).on(
  "change",
  "#edit-profilePicture, #profile-picture-input",
  function (e) {
    const file = this.files && this.files[0];
    if (file) {
      if (!file.type.startsWith("image/")) {
        safeToastrMessage("error", window.i18next.t("pleaseSelectValidImageFile"));
        this.value = "";
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        safeToastrMessage("error", window.i18next.t("imageSizeMustBeLessThan5MB"));
        this.value = "";
        return;
      }
      if (file.size < 10 * 1024) {
        safeToastrMessage("warning", window.i18next.t("imageSizeIsVerySmall"));
      }
      selectedImageFile = file;
      const reader = new FileReader();
      reader.onload = function (ev) {
        const $img = $("#cropper-image");
        $img.attr("src", ev.target.result);
        $img.off("load").on("load", function () {
          if (cropper) {
            cropper.destroy();
            cropper = null;
          }
          $("#cropImageModal").modal("show");
          $("#cropImageModal").one("shown.bs.modal", function () {
            setTimeout(() => {
              initializeCropper($img[0], ev.target.result);
            }, 100);
          });
        });
        if ($img[0].complete) {
          $img.trigger("load");
        }
      };
      reader.readAsDataURL(file);
    } else {
      $("#edit-profilePicture-container, #profile-picture-preview").hide();
      window._editProfilePictureBase64 = null;
    }
  },
);

function initializeCropper(imageElement, imageUrl) {
  $(".drag-drop-zone").addClass("hidden");
  currentZoom = 1;
  cropperReady = false;
  initialCropBoxWidth = null;
  cropper = new Cropper(imageElement, {
    aspectRatio: 1,
    viewMode: 1,
    dragMode: "crop",
    autoCropArea: 0.8,
    background: false,
    responsive: true,
    movable: false,
    rotatable: true,
    scalable: false,
    zoomable: true,
    zoomOnWheel: true,
    wheelZoomRatio: 0.1,
    cropBoxMovable: true,
    cropBoxResizable: true,
    toggleDragModeOnDblclick: false,
    ready: function () {
      let triedResize = false;
      let tryCount = 0;
      function waitForCropBox() {
        tryCount++;
        const cropBox = cropper.getCropBoxData();
        const imageData = cropper.getImageData();
        if (!imageData || !cropBox) return setTimeout(waitForCropBox, 30);
        if (tryCount === 1) {
          const boxSize = Math.floor(
            Math.min(imageData.naturalWidth, imageData.naturalHeight) * 0.8,
          );
          cropper.setCropBoxData({
            width: boxSize,
            height: boxSize,
            left: imageData.left + (imageData.naturalWidth - boxSize) / 2,
            top: imageData.top + (imageData.naturalHeight - boxSize) / 2,
          });
          setTimeout(waitForCropBox, 30);
          return;
        }
        let adjusted = false;
        let newLeft = cropBox.left;
        let newTop = cropBox.top;
        if (cropBox.left < imageData.left) {
          newLeft = imageData.left;
          adjusted = true;
        }
        if (cropBox.top < imageData.top) {
          newTop = imageData.top;
          adjusted = true;
        }
        if (
          cropBox.left + cropBox.width >
          imageData.left + imageData.naturalWidth
        ) {
          newLeft = imageData.left + imageData.naturalWidth - cropBox.width;
          adjusted = true;
        }
        if (
          cropBox.top + cropBox.height >
          imageData.top + imageData.naturalHeight
        ) {
          newTop = imageData.top + imageData.naturalHeight - cropBox.height;
          adjusted = true;
        }
        if (adjusted) {
          cropper.setCropBoxData({
            width: cropBox.width,
            height: cropBox.height,
            left: newLeft,
            top: newTop,
          });
          setTimeout(waitForCropBox, 30);
          return;
        }
        if (imageData.naturalWidth < 100 || imageData.naturalHeight < 100) {
          safeToastrMessage("error", window.i18next.t("imageIsTooSmall"));
          $("#cropImageModal").modal("hide");
          return;
        }
        if (
          tryCount > 10 &&
          (!cropBox || cropBox.width <= 0 || cropBox.height <= 0)
        ) {
          safeToastrMessage("error", window.i18next.t("failedToInitializeCropper"));
          $("#cropImageModal").modal("hide");
          return;
        }
        if (!initialCropBoxWidth) initialCropBoxWidth = cropBox.width;
        let maxZoom = Math.min(
          Math.floor((initialCropBoxWidth / 10) * 100) / 100,
          imageData.naturalWidth / initialCropBoxWidth,
          3,
        );
        maxZoom = Math.max(1, maxZoom);
        cropperReady = true;
        updateZoomSlider(maxZoom);
        updateCircleOverlay();
        updateAvatarPreview();
        const zoomSlider = document.getElementById("zoom-slider");
        if (zoomSlider) {
          zoomSlider.disabled = maxZoom === 1;
          zoomSlider.max = maxZoom.toFixed(2);
        }
        $("#zoom-in-btn, #zoom-out-btn").prop("disabled", maxZoom === 1);
      }
      waitForCropBox();
      const zoomSlider = document.getElementById("zoom-slider");
      if (zoomSlider) {
        zoomSlider.min = 1;
        zoomSlider.max = 3;
        zoomSlider.step = 0.01;
        zoomSlider.value = 1;
        zoomSlider.oninput = function () {
          let val = parseFloat(this.value);
          if (val < 1) val = 1;
          if (val > parseFloat(this.max)) val = parseFloat(this.max);
          if (val === currentZoom) return;
          this.value = val;
          currentZoom = val;
          if (initialCropBoxWidth) {
            const newWidth = initialCropBoxWidth / currentZoom;
            const cropBox = cropper.getCropBoxData();
            cropper.setCropBoxData({
              width: newWidth,
              height: newWidth,
              left: cropBox.left,
              top: cropBox.top,
            });
          } else {
            cropper.zoomTo(currentZoom);
          }
          updateZoomDisplay(currentZoom);
        };
      }
      updateZoomSlider();
      updateCircleOverlay();
      updateAvatarPreview();
      $("#zoom-in-btn")
        .off("click")
        .on("click", function () {
          const zoomSlider = document.getElementById("zoom-slider");
          let maxZoom = zoomSlider ? parseFloat(zoomSlider.max) : 3;
          if (currentZoom >= maxZoom) return;
          let newZoom = Math.min(currentZoom + 0.1, maxZoom);
          if (newZoom === currentZoom) return;
          currentZoom = newZoom;
          if (initialCropBoxWidth) {
            const newWidth = initialCropBoxWidth / currentZoom;
            const cropBox = cropper.getCropBoxData();
            cropper.setCropBoxData({
              width: newWidth,
              height: newWidth,
              left: cropBox.left,
              top: cropBox.top,
            });
          } else {
            cropper.zoomTo(currentZoom);
          }
          updateZoomSlider(maxZoom);
          updateZoomDisplay(currentZoom);
        });
      $("#zoom-out-btn")
        .off("click")
        .on("click", function () {
          if (currentZoom <= 1) return;
          let newZoom = Math.max(currentZoom - 0.1, 1);
          if (newZoom === currentZoom) return;
          currentZoom = newZoom;
          if (initialCropBoxWidth) {
            const newWidth = initialCropBoxWidth / currentZoom;
            const cropBox = cropper.getCropBoxData();
            cropper.setCropBoxData({
              width: newWidth,
              height: newWidth,
              left: cropBox.left,
              top: cropBox.top,
            });
          } else {
            cropper.zoomTo(currentZoom);
          }
          updateZoomSlider();
          updateZoomDisplay(currentZoom);
        });
      $("#rotate-left-btn")
        .off("click")
        .on("click", function () {
          cropper.rotate(-90);
        });
      $("#rotate-right-btn")
        .off("click")
        .on("click", function () {
          cropper.rotate(90);
        });
      $("#reset-btn")
        .off("click")
        .on("click", function () {
          cropper.reset();
          currentZoom = 1;
          updateZoomSlider();
          updateZoomDisplay(currentZoom);
        });
    },
    zoom: function (event) {
      let ratio = event.detail.ratio;
      const zoomSlider = document.getElementById("zoom-slider");
      let maxZoom = zoomSlider ? parseFloat(zoomSlider.max) : 3;
      if (ratio < 1) {
        event.preventDefault();
        currentZoom = 1;
      } else if (ratio > maxZoom) {
        event.preventDefault();
        currentZoom = maxZoom;
      } else {
        currentZoom = ratio;
      }
      if (zoomSlider) {
        zoomSlider.value = currentZoom;
        updateZoomDisplay(currentZoom);
      }
      updateCircleOverlay();
    },
    crop: function (event) {
      updateCircleOverlay();
      if (cropperReady) updateAvatarPreview();
      updateZoomSlider();
    },
    cropmove: function () {
      updateCircleOverlay();
      if (cropperReady) updateAvatarPreview();
    },
    error: function () {
      safeToastrMessage("error", window.i18next.t("failedToLoadImage"));
      $("#cropImageModal").modal("hide");
    },
  });
  window._cropper = cropper;
}
function updateZoomSlider(maxZoom) {
  const zoomSlider = document.getElementById("zoom-slider");
  if (zoomSlider) {
    if (maxZoom) zoomSlider.max = maxZoom;
    zoomSlider.value = currentZoom;
    updateZoomDisplay(currentZoom);
    const minLabel = zoomSlider.parentElement?.previousElementSibling;
    const maxLabel = document.getElementById("zoom-max-label");
    if (minLabel) minLabel.textContent = "100%";
    if (maxLabel && maxZoom)
      maxLabel.textContent = `${Math.round(maxZoom * 100)}%`;
  }
}

function updateZoomDisplay(zoom) {
  const percentage = Math.round((zoom || 1) * 100);
  const zoomDisplay = document.querySelector(".zoom-percentage");
  if (zoomDisplay) {
    zoomDisplay.textContent = `${percentage}%`;
  }
}

$(document).on("dragover", ".drag-drop-zone", function (e) {
  e.preventDefault();
  $(this).addClass("dragover");
});

$(document).on("dragleave", ".drag-drop-zone", function (e) {
  e.preventDefault();
  $(this).removeClass("dragover");
});

$(document).on("drop", ".drag-drop-zone", function (e) {
  e.preventDefault();
  $(this).removeClass("dragover");

  const files = e.originalEvent.dataTransfer.files;
  if (files.length > 0) {
    const file = files[0];
    if (file.type.startsWith("image/")) {
      handleImageFile(file);
    } else {
      safeToastrMessage("error", window.i18next.t("pleaseSelectValidImageFile"));
    }
  }
});

$(document).on("click", ".drag-drop-zone", function () {
  $("#edit-profilePicture, #profile-picture-input").click();
});

function handleImageFile(file) {
  if (file.size > 5 * 1024 * 1024) {
    safeToastrMessage("error", window.i18next.t("imageSizeMustBeLessThan5MB"));
    return;
  }
  if (file.size < 10 * 1024) {
    safeToastrMessage("warning", window.i18next.t("imageSizeIsVerySmall"));
  }

  selectedImageFile = file;
  const reader = new FileReader();
  reader.onload = function (ev) {
    const $img = $("#cropper-image");
    if (cropper) {
      cropper.destroy();
      cropper = null;
    }
    $img.attr("src", ev.target.result);
    $img.off("load").on("load", function () {
      initializeCropper($img[0], ev.target.result);
    });
    if ($img[0].complete) {
      $img.trigger("load");
    }
  };
  reader.readAsDataURL(file);
}

function openCropperModal(imageUrl) {
  const $modal = $("#cropImageModal");
  const $img = $("#cropper-image");
  if (cropper) {
    cropper.destroy();
    cropper = null;
  }
  $img.hide();
  $img.attr("src", imageUrl);
  $modal.modal("show");
  $img.off("load").on("load", function () {
    $img.show();
    initializeCropper($img[0], imageUrl);
  });
}

function updateCircleOverlay() {
  if (!cropper) return;
  const overlay = document.querySelector(".crop-circle-overlay");
  if (!overlay) return;
  const cropBox = cropper.getCropBoxData();
  overlay.style.width = `${cropBox.width}px`;
  overlay.style.height = `${cropBox.height}px`;
  overlay.style.left = `${cropBox.left}px`;
  overlay.style.top = `${cropBox.top}px`;
  overlay.style.display = "block";
}

function updateAvatarPreview() {
  if (!cropper || !cropperReady) return;
  let cropBox;
  try {
    cropBox = cropper.getCropBoxData();
    const imageData = cropper.getImageData();
    if (
      cropBox.left < imageData.left ||
      cropBox.top < imageData.top ||
      cropBox.left + cropBox.width > imageData.left + imageData.naturalWidth ||
      cropBox.top + cropBox.height > imageData.top + imageData.naturalHeight
    ) {
      ensureCropBoxInBounds();
      cropBox = cropper.getCropBoxData();
    }
    if (!cropBox || cropBox.width <= 0 || cropBox.height <= 0) return;
    if (
      imageData &&
      (cropBox.width > imageData.naturalWidth ||
        cropBox.height > imageData.naturalHeight)
    ) {
      const preview = document.getElementById("crop-avatar-preview");
      if (preview) {
        preview.src = "";
        preview.style.display = "none";
      }
      return;
    }
    const canvas = cropper.getCroppedCanvas({
      width: 200,
      height: 200,
      imageSmoothingQuality: "high",
      fillColor: "#fff",
    });
    if (!canvas) return;
    const preview = document.getElementById("crop-avatar-preview");
    if (canvas && preview) {
      const circleCanvas = document.createElement("canvas");
      circleCanvas.width = 200;
      circleCanvas.height = 200;
      const ctx = circleCanvas.getContext("2d");
      ctx.save();
      ctx.beginPath();
      ctx.arc(100, 100, 100, 0, 2 * Math.PI);
      ctx.closePath();
      ctx.clip();
      ctx.drawImage(canvas, 0, 0, 200, 200);
      ctx.restore();
      preview.src = circleCanvas.toDataURL("image/png");
      preview.style.display = "block";
    } else if (preview) {
      preview.src = "";
      preview.style.display = "none";
    }
  } catch (error) {
    const preview = document.getElementById("crop-avatar-preview");
    if (preview) {
      preview.src = "";
      preview.style.display = "none";
    }
  }
}

$("#cropImageModal").on("hidden.bs.modal", function () {
  if (cropper) {
    cropper.destroy();
    cropper = null;
  }
  if (!window._editProfilePictureBase64) {
    $("#edit-profilePicture, #profile-picture-input").val("");
    selectedImageFile = null;
  }
  $(".drag-drop-zone").removeClass("hidden");
});

$(document).on("click", "#cropImageBtn", function () {
  if (!cropper || !cropperReady) {
    safeToastrMessage("error", window.i18next.t("cropperNotReady"));
    return;
  }
  let cropBox = cropper.getCropBoxData();
  const imageData = cropper.getImageData();
  if (!cropBox || cropBox.width <= 0 || cropBox.height <= 0) {
    safeToastrMessage("error", window.i18next.t("cropAreaInvalid"));
    return;
  }
  if (
    imageData &&
    (cropBox.width > imageData.naturalWidth ||
      cropBox.height > imageData.naturalHeight)
  ) {
    safeToastrMessage("error", window.i18next.t("cropAreaLargerThanImage"));
    return;
  }
  try {
    const canvas = cropper.getCroppedCanvas({
      width: 200,
      height: 200,
      imageSmoothingEnabled: true,
      imageSmoothingQuality: "high",
      fillColor: "#fff",
    });
    if (!canvas) throw new Error(window.i18next.t("canvasIsNull"));
    const size = 200;
    const circleCanvas = document.createElement("canvas");
    circleCanvas.width = size;
    circleCanvas.height = size;
    const ctx = circleCanvas.getContext("2d");
    ctx.save();
    ctx.beginPath();
    ctx.arc(size / 2, size / 2, size / 2, 0, 2 * Math.PI);
    ctx.closePath();
    ctx.clip();
    ctx.drawImage(canvas, 0, 0, size, size);
    ctx.restore();
    window._editProfilePictureBase64 = circleCanvas.toDataURL("image/png");
    $("#crop-avatar-preview, #edit-profilePicture-preview").attr(
      "src",
      window._editProfilePictureBase64,
    );
    $("#edit-profilePicture-container, #profile-picture-preview").show();
    setTimeout(() => {
      $("#cropImageModal").modal("hide");
      safeToastrMessage(
        "success",
        window.i18next.t("profilePictureCroppedSuccessfully"),
      );
    }, 200);
  } catch (error) {
    window._editProfilePictureBase64 = null;
    $("#crop-avatar-preview, #edit-profilePicture-preview").attr("src", "");
    $("#edit-profilePicture-container, #profile-picture-preview").hide();
    safeToastrMessage("error", error.message || window.i18next.t("failedToCropImage"));
  }
});

$(document).on("click", "#remove-profile-picture", function () {
  $("#crop-avatar-preview, #edit-profilePicture-preview").attr("src", "");
  $("#edit-profilePicture-container, #profile-picture-preview").hide();
  $("#edit-profilePicture, #profile-picture-input").val("");
  window._editProfilePictureBase64 = null;
  selectedImageFile = null;
  window._profilePictureRemoved = true;
  if (cropper) {
    cropper.destroy();
    cropper = null;
  }
  safeToastrMessage("info", window.i18next.t("profilePictureRemoved"));
});

function handleUpdateUser(userId) {
  const form = document.getElementById("editUserForm");
  if (!form) return;
  const formData = new FormData(form);

  let dateOfBirth = formData.get("dateOfBirth");
  const data = {
    fullName: formData.get("fullName")?.trim(),
    phoneNumber: formData.get("phoneNumber")?.trim(),
    address: formData.get("address")?.trim(),
    bio: formData.get("bio")?.trim(),
    status: parseInt(formData.get("status")) || 1,
    isVerified: formData.get("isVerified") === "on",
  };

  if (dateOfBirth && dateOfBirth.trim() !== "") {
    const today = new Date();
    if (dateOfBirth > today.toISOString().split("T")[0]) {
      safeToastrMessage("error", window.i18next.t("dateOfBirthCannotBeInFuture"));
      return;
    }
    data.dateOfBirth = new Date(dateOfBirth).toISOString();
  }
  if (window._editProfilePictureBase64) {
    data.profilePicture = window._editProfilePictureBase64;
  } else if (selectedImageFile) {
    safeToastrMessage(
      "warning",
      window.i18next.t("pleaseCropYourProfilePictureBeforeSaving"),
    );
    return;
  } else if (window._profilePictureRemoved) {
    data.profilePicture = null;
  }

  Object.keys(data).forEach((key) => {
    if (data[key] === null || data[key] === undefined || data[key] === "") {
      delete data[key];
    }
  });

  if (data.isVerified !== undefined) {
    data.isVerified = Boolean(data.isVerified);
  }

  const original = $("#editUserForm").data("original") || {};
  let changed = false;

  if (
    window._editProfilePictureBase64 ||
    selectedImageFile ||
    window._profilePictureRemoved
  ) {
    changed = true;
  }

  for (const key of Object.keys(data)) {
    if (key === "profilePicture") continue;

    let oldVal = original[key] || "";
    let newVal = data[key] || "";

    if (key === "dateOfBirth") {
      if (oldVal) {
        oldVal = oldVal.split("T")[0];
      }
      if (newVal) {
        newVal = newVal.split("T")[0];
      }
    }

    if (String(oldVal) !== String(newVal)) {
      changed = true;
      break;
    }
  }
  if (!changed) {
    safeToastrMessage("warning", window.i18next.t("noInformationChanged"));
    $("#editUserModal").modal("hide");
    return;
  }
  if (data.phoneNumber !== original.phoneNumber) {
    if (data.phoneNumber && !/^[0-9]{10,11}$/.test(data.phoneNumber)) {
      safeToastrMessage(
        "error",
        window.i18next.t("phoneNumberMustBe10To11DigitsAndOnlyNumbers"),
      );
      return;
    }
  }
  if (!data.fullName) {
    safeToastrMessage("error", window.i18next.t("fullNameRequired"));
    return;
  }
  if (!/^[a-zA-ZÀ-ỹ\s]+$/.test(data.fullName)) {
    safeToastrMessage("error", window.i18next.t("fullNameInvalidCharacters"));
    return;
  }
  const token = localStorage.getItem("authToken");
  if (!token) {
    safeToastrMessage("error", window.i18next.t("authenticationRequired"));
    return;
  }
  fetch(`/api/User/${userId}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  })
    .then(async (res) => {
      let responseData;
      try {
        responseData = await res.json();
      } catch (e) {
        responseData = {};
      }
      if (!res.ok) {
        if (responseData.errors) {
          Object.keys(responseData.errors).forEach((field) => {
            const messages = responseData.errors[field];
            if (Array.isArray(messages)) {
              messages.forEach((msg) => safeToastrMessage(`${field}: ${msg}`));
            } else {
              safeToastrMessage(`${field}: ${messages}`);
            }
          });
        } else if (responseData.message) {
          safeToastrMessage("error", responseData.message);
        } else {
          safeToastrMessage("error", window.i18next.t("failedToUpdateUser"));
        }
        throw new Error(
          responseData.message || responseData.error || `HTTP ${res.status}`,
        );
      }
      return responseData;
    })
    .then((res) => {
      safeToastrMessage("success", window.i18next.t("userUpdatedSuccessfully"));
      $("#editUserModal").modal("hide");

      reloadCurrentPageData();

      setTimeout(() => {
        const dt_user_table = $(".datatables-users");
        if (dt_user_table.length && $.fn.DataTable.isDataTable(dt_user_table)) {
          dt_user_table.DataTable().ajax.reload(null, false);
        }
      }, 500);

      window._editProfilePictureBase64 = null;
      selectedImageFile = null;
      window._profilePictureRemoved = false;
      if (cropper) {
        cropper.destroy();
        cropper = null;
      }
    })
    .catch(() => safeToastrMessage("error", window.i18next.t("failedToUpdateUser")));
}

function deleteUser(userId) {
  const token = localStorage.getItem("authToken");
  if (!userId || !token) {
    safeToastrMessage("error", window.i18next.t("invalidRequest"));
    return;
  }

  let currentUserInfo = null;
  try {
    const payload = JSON.parse(
      atob(token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/")),
    );
    currentUserInfo = {
      id: payload.nameid || payload.sub,
      email: payload.email,
      username: payload.username,
    };
  } catch (e) {
    console.error("Failed to parse JWT token:", e);
  }

  fetch(`/api/User/${userId}`, {
    method: "DELETE",
    headers: { Authorization: `Bearer ${token}` },
  })
    .then(async (res) => {
      let responseData = {};
      try {
        responseData = await res.json();
      } catch {}
      if (!res.ok) {
        throw new Error(responseData.message || `HTTP ${res.status}`);
      }
      return responseData;
    })
    .then((res) => {
      safeToastrMessage("success", window.i18next.t("userDeletedSuccessfully"));
      $("#deleteUserModal").modal("hide");
      const userInfo = res.data || res;

      if (userInfo && currentUserInfo && userInfo.id == currentUserInfo.id) {
        localStorage.removeItem("authToken");
        sessionStorage.clear();
        safeToastrMessage("info", window.i18next.t("yourAccountHasBeenDeleted"));
        setTimeout(() => {
          window.location.href = "/admin/index.html";
        }, 1000);
        return;
      }
      reloadCurrentPageData();
    })
    .catch((error) => {
      console.error("Delete user error:", error);
      safeToastrMessage(
        "error",
        error.message || window.i18next.t("failedToDeleteUser"),
      );
    });
}

function restoreUser(userId) {
  const token = localStorage.getItem("authToken");
  if (!userId || !token) {
    safeToastrMessage("error", window.i18next.t("invalidRequest"));
    return;
  }

  fetch(`/api/User/${userId}/restore`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${token}` },
  })
    .then(async (res) => {
      const responseData = await res.json();
      if (!res.ok) {
        throw new Error(responseData.message || `HTTP ${res.status}`);
      }
      return responseData;
    })
    .then((res) => {
      safeToastrMessage("success", window.i18next.t("userRestoredSuccessfully"));

      $("#restoreUserModal").modal("hide");

      reloadCurrentPageData();
    })
    .catch((error) => {
      console.error("Restore user error:", error);
      safeToastrMessage(
        "error",
        error.message || window.i18next.t("failedToRestoreUser"),
      );
    });
}

function reloadCurrentPageData() {
  const currentPath = window.location.pathname;
  const currentPage = currentPath.split("/").pop();

  safeToastrMessage("clear");

  setTimeout(() => {
    if (currentPage === "index.html" || currentPage === "") {
      if (typeof updateUserStatsDashboard === "function") {
        updateUserStatsDashboard();
      }
    } else if (currentPage === "active-users.html") {
      if (typeof loadActiveUsersTable === "function") {
        loadActiveUsersTable();
      }
    } else if (currentPage === "deactive-users.html") {
      if (typeof loadDeactiveUsersTable === "function") {
        loadDeactiveUsersTable();
      }
    } else {
      const dt_user_table = $(".datatables-users");
      if (dt_user_table.length && $.fn.DataTable.isDataTable(dt_user_table)) {
        dt_user_table.DataTable().destroy();
      }
      if (typeof loadAllUsersTable === "function") {
        loadAllUsersTable();
      }
    }
  }, 100);
}

function openEditUserModal(userId) {
  const token = localStorage.getItem("authToken");
  if (!userId || !token) return;
  fetch(`/api/User/${userId}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
    .then((res) => res.json())
    .then((res) => {
      const user = res.data || res;
      if (!user || !user.id) {
        safeToastrMessage("error", window.i18next.t("userNotFound"));
        return;
      }
      $("#editUserForm").data("userid", user.id);
      $("#editUserForm").data("original", {
        fullName: user.fullName || "",
        phoneNumber: user.phoneNumber || "",
        dateOfBirth: user.dateOfBirth ? user.dateOfBirth.split("T")[0] : "",
        address: user.address || "",
        bio: user.bio || "",
        status: user.status ? String(user.status) : "1",
        isVerified: !!user.isVerified,
      });
      $("#edit-username")
        .val(user.username || "")
        .prop("disabled", true);
      $("#edit-email")
        .val(user.email || "")
        .prop("disabled", true);
      $("#edit-fullName").val(user.fullName || "");
      $("#edit-phoneNumber").val(user.phoneNumber || "");
      $("#edit-dateOfBirth").val(
        user.dateOfBirth ? user.dateOfBirth.split("T")[0] : "",
      );
      $("#edit-address").val(user.address || "");
      $("#edit-bio").val(user.bio || "");
      $("#edit-status").val(user.status ? String(user.status) : "1");
      $("#edit-isVerified").prop("checked", !!user.isVerified);
      if (user.profilePicture && user.profilePicture.trim() !== "") {
        const $preview = $("#edit-profilePicture-preview");
        const $container = $("#edit-profilePicture-container");
        
        const letter = (user.username || user.email || "").charAt(0).toUpperCase();
        const seed = user.email || user.username || user.fullName || "default";
        
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
          const char = seed.charCodeAt(i);
          hash = ((hash << 5) - hash) + char;
          hash = hash & hash;
        }
        
        const hue = Math.abs(hash) % 360;
        const saturation = 70 + (Math.abs(hash) % 20);
        const lightness = 45 + (Math.abs(hash) % 15);
        
        const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
        const fallbackSvg = `<svg width='80' height='80' xmlns='http://www.w3.org/2000/svg'><circle cx='40' cy='40' r='40' fill='${color}'/><text x='50%' y='50%' text-anchor='middle' dy='.35em' font-family='Arial' font-size='32' fill='#fff'>${letter}</text></svg>`;
        const fallbackDataUrl = "data:image/svg+xml;base64," + btoa(unescape(encodeURIComponent(fallbackSvg)));
        
        $preview.attr("src", user.profilePicture)
          .on("error", function() {
            $(this).attr("src", fallbackDataUrl);
          })
          .on("load", function() {
          });
        
        $container.show();
      } else {
        $("#edit-profilePicture-container").hide();
      }
      $("#edit-profilePicture").val("");
      if (cropper) {
        cropper.destroy();
        cropper = null;
      }
      window._editProfilePictureBase64 = null;
      selectedImageFile = null;
      window._profilePictureRemoved = false;
      $("#editUserModal").modal("show");
    })
    .catch(() =>
      safeToastrMessage("error", window.i18next.t("failedToLoadUserInformation")),
    );

  $("#editUserModal").on("hidden.bs.modal", function () {
    if (cropper) {
      cropper.destroy();
      cropper = null;
    }
    window._editProfilePictureBase64 = null;
    selectedImageFile = null;
    window._profilePictureRemoved = false;
  });
}

function openViewUserModal(userId) {
  const token = localStorage.getItem("authToken");
  if (!userId || !token) return;
  fetch(`/api/User/${userId}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
    .then((res) => res.json())
    .then((res) => {
      const user = res.data || res;
      if (!user || !user.id) {
        safeToastrMessage("error", window.i18next.t("userNotFound"));
        return;
      }
      $("#viewUserModal").data("userid", user.id);
      $(".user-username").text(
        user.username || window.i18next.t("notAvailable"),
      );
      $(".user-fullName").text(
        user.fullName || window.i18next.t("notAvailable"),
      );
      $(".user-email").text(user.email || window.i18next.t("notAvailable"));
      $(".user-phone").text(
        user.phoneNumber || window.i18next.t("notAvailable"),
      );
      $(".user-lastlogin").text(
        user.lastLoginAt
          ? new Date(user.lastLoginAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            })
          : window.i18next.t("never"),
      );
      $(".user-deletedat").text(
        user.deletedAt
          ? new Date(user.deletedAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            })
          : window.i18next.t("notAvailable"),
      );
      $(".user-address").text(user.address || window.i18next.t("notAvailable"));
      $(".user-dob").text(
        user.dateOfBirth
          ? new Date(user.dateOfBirth).toLocaleDateString("en-GB")
          : window.i18next.t("notAvailable"),
      );
      $(".user-verified").text(
        user.isVerified ? window.i18next.t("yes") : window.i18next.t("no"),
      );
      $(".user-provider").text(user.loginProvider || window.i18next.t("local"));
      $(".user-bio").text(user.bio || window.i18next.t("noBioAvailable"));
      
      const $avatar = $("#view-user-avatar");
      const $fallback = $("#avatar-fallback");
      
      if (user.profilePicture && user.profilePicture.trim() !== "") {
        const letter = (user.username || user.email || "").charAt(0).toUpperCase();
        const seed = user.email || user.username || user.fullName || "default";
        
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
          const char = seed.charCodeAt(i);
          hash = ((hash << 5) - hash) + char;
          hash = hash & hash;
        }
        
        const hue = Math.abs(hash) % 360;
        const saturation = 70 + (Math.abs(hash) % 20);
        const lightness = 45 + (Math.abs(hash) % 15);
        
        const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
        $fallback
          .text(letter)
          .css({ background: color, color: "#fff", display: "flex" });
        
        // Thêm onerror handler cho ảnh
        $avatar.attr("src", user.profilePicture)
          .on("error", function() {
            $(this).hide();
            $fallback.show();
          })
          .on("load", function() {
            $fallback.hide();
            $(this).show();
          })
          .show();
      } else {
        $avatar.hide();
        const letter = (user.username || user.email || "").charAt(0).toUpperCase();
        const seed = user.email || user.username || user.fullName || "default";
        
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
          const char = seed.charCodeAt(i);
          hash = ((hash << 5) - hash) + char;
          hash = hash & hash;
        }
        
        const hue = Math.abs(hash) % 360;
        const saturation = 70 + (Math.abs(hash) % 20);
        const lightness = 45 + (Math.abs(hash) % 15);
        
        const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
        $fallback
          .text(letter)
          .css({ background: color, color: "#fff", display: "flex" });
        $fallback.show();
      }
      const statusBadge = $(".user-status-badge");
      statusBadge.html(getStatusBadge(user.status));
      $("#viewUserModal").modal("show");
    })
    .catch((err) => {
      safeToastrMessage("error", window.i18next.t("failedToLoadUserInformation"));
    });
}
window.openViewUserModal = openViewUserModal;

$(document).on("click", "#viewUserModal .btn-edit-user", function () {
  const userId = $("#viewUserModal").data("userid");
  if (userId) {
    $("#viewUserModal").modal("hide");
    setTimeout(() => openEditUserModal(userId), 300);
  }
});

$(document).on("click", "#viewUserModal .btn-delete-user", function () {
  const userId = $("#viewUserModal").data("userid");
  if (userId) {
    $("#viewUserModal").modal("hide");
    setTimeout(() => openDeleteUserModal(userId), 300);
  }
});

$(document).on("click", "#viewUserModal .btn-restore-user", function () {
  const userId = $("#viewUserModal").data("userid");
  if (userId) {
    $("#viewUserModal").modal("hide");
    setTimeout(() => openRestoreUserModal(userId), 300);
  }
});

$(document).on("click", "#saveEditUserBtn", function () {
  const userId = $("#editUserForm").data("userid");
  if (userId) {
    handleUpdateUser(userId);
  }
});

$(document).on("click", "#confirmDeleteUser", function () {
  const userId = $("#deleteUserModal").data("userid");
  if (userId) {
    deleteUser(userId);
    $("#deleteUserModal").modal("hide");
  }
});

$(document).on("click", "#confirmRestoreUser", function () {
  const userId = $("#restoreUserModal").data("userid");
  if (userId) {
    restoreUser(userId);
    $("#restoreUserModal").modal("hide");
  }
});

$(document).on("submit", "#editUserForm", function (e) {
  e.preventDefault();
  const userId = $(this).data("userid");
  if (userId) {
    handleUpdateUser(userId);
  }
});

$(document)
  .off("click", ".view-user")
  .on("click", ".view-user", function (e) {
    e.stopPropagation();
    const userId = $(this).closest("tr").attr("data-userid");
    if (userId) openViewUserModal(userId);
  });

$(document)
  .off("click", ".edit-user")
  .on("click", ".edit-user", function (e) {
    e.stopPropagation();
    const userId = $(this).closest("tr").attr("data-userid");
    if (userId) openEditUserModal(userId);
  });

$(document)
  .off("click", ".delete-user")
  .on("click", ".delete-user", function (e) {
    e.stopPropagation();
    const userId = $(this).closest("tr").attr("data-userid");
    if (userId) openDeleteUserModal(userId);
  });

$(document)
  .off("click", ".restore-user")
  .on("click", ".restore-user", function (e) {
    e.stopPropagation();
    const userId = $(this).closest("tr").attr("data-userid");
    if (userId) openRestoreUserModal(userId);
  });

$(document)
  .off("click", ".datatables-users tbody tr")
  .on("click", ".datatables-users tbody tr", function (e) {
    if (
      $(e.target).closest(
        ".edit-user, .delete-user, .restore-user, .view-user, .btn",
      ).length
    )
      return;
    const userId = $(this).attr("data-userid");
    if (userId) openViewUserModal(userId);
  });

$(document).on("click", "#add-user-btn", function () {
  $("#offcanvasAddUser").offcanvas("show");
});

$(document).on("submit", "#addNewUserForm", function (e) {
  e.preventDefault();
  handleAddUser();
});
$(document).on("click", '.breadcrumb-item a[href="index.html"]', function (e) {
  e.preventDefault();
  window.location.href = "/admin/index.html";
});

function updateUserStatsDashboard() {
  const token = localStorage.getItem("authToken");
  if (!token) return;

  // First, get total count to determine optimal pageSize
  fetch(
    "/api/User?includeDeleted=true&page=1&pageSize=1",
    {
      headers: { Authorization: `Bearer ${token}` },
    },
  )
    .then((res) => res.json())
    .then((countResult) => {
      const totalCount =
        countResult.pagination?.totalCount || countResult.totalCount || 0;

      if (totalCount === 0) {
        $("#total-users").text(0);
        $("#active-users").text(0);
        $("#inactive-users").text(0);
        $("#banned-users").text(0);
        return;
      }

      // Now fetch all data with the actual total count as pageSize
      return fetch(
        `/api/User?includeDeleted=true&pageSize=${totalCount}`,
        {
          headers: { Authorization: `Bearer ${token}` },
        },
      );
    })
    .then((res) => res.json())
    .then((json) => {
      if (!json || !Array.isArray(json.data)) {
        $("#total-users").text(0);
        $("#active-users").text(0);
        $("#inactive-users").text(0);
        $("#banned-users").text(0);
        return;
      }
      const users = json.data;
      let total = users.length,
        active = 0,
        inactive = 0,
        banned = 0;
      users.forEach((u) => {
        if (u.status === 1 || u.status === "Active") active++;
        else if (u.status === 2 || u.status === "Inactive") inactive++;
        else if (u.status === 4 || u.status === "Banned") banned++;
      });
      $("#total-users").text(total);
      $("#active-users").text(active);
      $("#inactive-users").text(inactive);
      $("#banned-users").text(banned);
    })
    .catch((error) => {
      console.error("Failed to update dashboard stats:", error);
      $("#total-users").text(0);
      $("#active-users").text(0);
      $("#inactive-users").text(0);
      $("#banned-users").text(0);
    });
}

function getStatusBadge(status) {
  switch (status) {
    case 1:
    case "Active":
      return (
        '<span class="badge bg-label-success">' +
        window.i18next.t("active") +
        "</span>"
      );
    case 2:
    case "Inactive":
      return (
        '<span class="badge bg-label-secondary">' +
        window.i18next.t("inactive") +
        "</span>"
      );
    case 3:
    case "Suspended":
      return (
        '<span class="badge bg-label-warning">' +
        window.i18next.t("suspended") +
        "</span>"
      );
    case 4:
    case "Banned":
      return (
        '<span class="badge bg-label-danger">' +
        window.i18next.t("banned") +
        "</span>"
      );
    default:
      return (
        '<span class="badge bg-label-secondary">' +
        window.i18next.t("unknown") +
        "</span>"
      );
  }
}

window.openDeleteUserModal = function (userId) {
  const token = localStorage.getItem("authToken");
  if (!userId || !token) return;
  fetch(`/api/User/${userId}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
    .then((res) => res.json())
    .then((res) => {
      const user = res.data || res;
      if (!user || !user.id) {
        safeToastrMessage("error", window.i18next.t("userNotFound"));
        return;
      }
      $("#deleteUserModal").data("userid", user.id);
      $(".delete-user-username").text(
        user.username || window.i18next.t("notAvailable"),
      );
      $(".delete-user-fullName").text(
        user.fullName || window.i18next.t("notAvailable"),
      );
      $(".delete-user-email").text(
        user.email || window.i18next.t("notAvailable"),
      );
      $(".delete-user-phone").text(
        user.phoneNumber || window.i18next.t("notAvailable"),
      );
      $(".delete-user-status").html(getStatusBadge(user.status));
      $(".delete-user-lastlogin").text(
        user.lastLoginAt
          ? new Date(user.lastLoginAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            })
          : window.i18next.t("never"),
      );
      $(".delete-user-address").text(
        user.address || window.i18next.t("notAvailable"),
      );
      $(".delete-user-createdAt").text(
        user.createdAt
          ? new Date(user.createdAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            })
          : window.i18next.t("notAvailable"),
      );
      $("#deleteUserModal").modal("show");
    })
    .catch(() =>
      safeToastrMessage("error", window.i18next.t("failedToLoadUserInformation")),
    );
};

window.openRestoreUserModal = function (userId) {
  const token = localStorage.getItem("authToken");
  if (!userId || !token) return;
  fetch(`/api/User/${userId}`, {
    headers: { Authorization: `Bearer ${token}` },
  })
    .then((res) => res.json())
    .then((res) => {
      const user = res.data || res;
      if (!user || !user.id) {
        safeToastrMessage("error", window.i18next.t("userNotFound"));
        return;
      }
      $("#restoreUserModal").data("userid", user.id);
      $(".restore-user-username").text(
        user.username || window.i18next.t("notAvailable"),
      );
      $(".restore-user-fullName").text(
        user.fullName || window.i18next.t("notAvailable"),
      );
      $(".restore-user-email").text(
        user.email || window.i18next.t("notAvailable"),
      );
      $(".restore-user-phone").text(
        user.phoneNumber || window.i18next.t("notAvailable"),
      );
      $(".restore-user-status").html(getStatusBadge(user.status));
      $(".restore-user-deletedat").text(
        user.deletedAt
          ? new Date(user.deletedAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            })
          : window.i18next.t("notAvailable"),
      );
      $(".restore-user-address").text(
        user.address || window.i18next.t("notAvailable"),
      );
      $(".restore-user-createdAt").text(
        user.createdAt
          ? new Date(user.createdAt).toLocaleString("en-GB", {
              year: "numeric",
              month: "2-digit",
              day: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
              second: "2-digit",
            })
          : window.i18next.t("notAvailable"),
      );
      $("#restoreUserModal").modal("show");
    })
    .catch(() =>
      safeToastrMessage("error", window.i18next.t("failedToLoadUserInformation")),
    );
};

function generateLetterAvatarFromUser(user) {
  let letter = "U";
  let seed = "default";
  
  if (user && user.email && user.email.trim() !== "") {
    letter = user.email.trim().charAt(0).toUpperCase();
    seed = user.email.trim();
  } else if (user && user.username && user.username.trim() !== "") {
    letter = user.username.trim().charAt(0).toUpperCase();
    seed = user.username.trim();
  } else if (user && user.fullName && user.fullName.trim() !== "") {
    letter = user.fullName.trim().charAt(0).toUpperCase();
    seed = user.fullName.trim();
  }
  
  let hash = 0;
  for (let i = 0; i < seed.length; i++) {
    const char = seed.charCodeAt(i);
    hash = ((hash << 5) - hash) + char;
    hash = hash & hash;
  }
  
  const hue = Math.abs(hash) % 360;
  const saturation = 70 + (Math.abs(hash) % 20);
  const lightness = 45 + (Math.abs(hash) % 15);
  
  const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`; 
  const svg = `<svg width='40' height='40' xmlns='http://www.w3.org/2000/svg'><circle cx='20' cy='20' r='20' fill='${color}'/><text x='50%' y='50%' text-anchor='middle' dy='.35em' font-family='Arial' font-size='20' fill='#fff'>${letter}</text></svg>`;
  return "data:image/svg+xml;base64," + btoa(unescape(encodeURIComponent(svg)));
}

if (typeof window.getUserTableColumnDefs === "function") {
  const oldDefs = window.getUserTableColumnDefs;
  window.getUserTableColumnDefs = function (isDeactive) {
    const defs = oldDefs(isDeactive);
    defs.forEach((def) => {
      if (def.targets === 0) {
        def.render = function (data, type, full) {
          if (full.profilePicture && full.profilePicture.trim() !== "") {
            return `<img src="${full.profilePicture}" alt="User Avatar" class="rounded-circle" style="width:36px;height:36px;object-fit:cover;">`;
          } else {
            const svg = generateLetterAvatarFromUser(full);
            return `<img src="${svg}" alt="User Avatar" class="rounded-circle" style="width:36px;height:36px;object-fit:cover;">`;
          }
        };
      }
    });
    return defs;
  };
}

async function loadUserProfile() {
  try {
    const userInfo = window.adminAuth.getCurrentUserInfo();
    if (!userInfo) {
      safeToastrMessage("error", window.i18next.t("userInformationNotFound"));
      return;
    }

    const response = await fetch(
      `/api/User/${userInfo.id}`,
      {
        headers: window.adminAuth.getAuthHeaders(),
      },
    );

    if (response.ok) {
      const data = await response.json();
      const user = data.data;

      $("#profile-fullName").val(user.fullName || "");
      $("#profile-email").val(user.email || "");
      $("#profile-phone").val(user.phone || "");

      if (user.profilePicture && user.profilePicture.trim() !== "") {
        const $preview = $("#profile-picture-preview");
        const letter = (user.fullName || user.username || "U")
          .charAt(0)
          .toUpperCase();
        const seed = user.email || user.username || user.fullName || "default";
        
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
          const char = seed.charCodeAt(i);
          hash = ((hash << 5) - hash) + char;
          hash = hash & hash;
        }
        
        const hue = Math.abs(hash) % 360;
        const saturation = 70 + (Math.abs(hash) % 20);
        const lightness = 45 + (Math.abs(hash) % 15);
        
        const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
        const svg = `<svg width='100' height='100' xmlns='http://www.w3.org/2000/svg'><circle cx='50' cy='50' r='50' fill='${color}'/><text x='50%' y='50%' text-anchor='middle' dy='.35em' font-family='Arial' font-size='40' fill='#fff'>${letter}</text></svg>`;
        const fallbackDataUrl = "data:image/svg+xml;base64," + btoa(unescape(encodeURIComponent(svg)));
        
        $preview.attr("src", user.profilePicture)
          .on("error", function() {
            $(this).attr("src", fallbackDataUrl);
          })
          .on("load", function() {
          })
          .show();
      } else {
        const letter = (user.fullName || user.username || "U")
          .charAt(0)
          .toUpperCase();
        const seed = user.email || user.username || user.fullName || "default";
        
        let hash = 0;
        for (let i = 0; i < seed.length; i++) {
          const char = seed.charCodeAt(i);
          hash = ((hash << 5) - hash) + char;
          hash = hash & hash;
        }
        
        const hue = Math.abs(hash) % 360;
        const saturation = 70 + (Math.abs(hash) % 20);
        const lightness = 45 + (Math.abs(hash) % 15);
        
        const color = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
        const svg = `<svg width='100' height='100' xmlns='http://www.w3.org/2000/svg'><circle cx='50' cy='50' r='50' fill='${color}'/><text x='50%' y='50%' text-anchor='middle' dy='.35em' font-family='Arial' font-size='40' fill='#fff'>${letter}</text></svg>`;
        const dataUrl =
          "data:image/svg+xml;base64," +
          btoa(unescape(encodeURIComponent(svg)));
        $("#profile-picture-preview").attr("src", dataUrl).show();
      }
    } else {
      safeToastrMessage("error", window.i18next.t("failedToLoadUserProfile"));
    }
  } catch (error) {
    console.error("Error loading user profile:", error);
    safeToastrMessage("error", window.i18next.t("errorLoadingUserProfile"));
  }
}

async function saveUserProfile(formData) {
  try {
    const userInfo = window.adminAuth.getCurrentUserInfo();
    if (!userInfo) {
      safeToastrMessage("error", window.i18next.t("userInformationNotFound"));
      return false;
    }

    const response = await fetch(
      `/api/User/${userInfo.id}`,
      {
        method: "PUT",
        headers: {
          ...window.adminAuth.getAuthHeaders(),
          "Content-Type": "application/json",
        },
        body: JSON.stringify(formData),
      },
    );

    if (response.ok) {
      safeToastrMessage("success", window.i18next.t("profileUpdatedSuccessfully"));
      return true;
    } else {
      const errorData = await response.json();
      safeToastrMessage(
        "error",
        errorData.message || window.i18next.t("failedToUpdateProfile"),
      );
      return false;
    }
  } catch (error) {
    console.error("Error saving user profile:", error);
    safeToastrMessage("error", window.i18next.t("errorSavingProfile"));
    return false;
  }
}

function loadUserSettings() {
  try {
    const settings = JSON.parse(localStorage.getItem("userSettings") || "{}");
    $("#settings-notifications").prop(
      "checked",
      settings.notifications !== false,
    );
    if (settings.language) {
      window.i18next.changeLanguage(settings.language);
    }
    $("#settings-darkmode").prop("checked", settings.darkMode === true);
    safeToastrMessage("success", window.i18next.t("settingsLoaded"));
  } catch (error) {
    console.error("Error loading settings:", error);
    safeToastrMessage("error", window.i18next.t("errorLoadingSettings"));
  }
}

function saveUserSettings() {
  try {
    const lang = $("#settings-language").val();
    const settings = {
      notifications: $("#settings-notifications").is(":checked"),
      darkMode: $("#settings-darkmode").is(":checked"),
    };
    localStorage.setItem("userSettings", JSON.stringify(settings));
    safeToastrMessage("success", window.i18next.t("settingsSavedSuccessfully"));
    if (lang) {
      window.i18next.changeLanguage(lang);
      const token = localStorage.getItem("authToken");
      if (token) {
        fetch("/api/User/update-language", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
            "Accept-Language": lang,
          },
          body: JSON.stringify({ language: lang }),
        });
      }
    }
    if (settings.darkMode) {
      $("html").addClass("dark-style");
    } else {
      $("html").removeClass("dark-style");
    }
    return true;
  } catch (error) {
    console.error("Error saving settings:", error);
    safeToastrMessage("error", window.i18next.t("errorSavingSettings"));
    return false;
  }
}

function initializePageFunctionality() {
  const currentPage = window.location.pathname.split("/").pop();

  switch (currentPage) {
    case "pages-profile-user.html":
      loadUserProfile();

      $("#profile-form").on("submit", async function (e) {
        e.preventDefault();

        const formData = {
          fullName: $("#profile-fullName").val(),
          phone: $("#profile-phone").val(),
        };

        const fileInput = $("#profile-picture-input")[0];
        if (fileInput.files.length > 0) {
          const file = fileInput.files[0];
          const reader = new FileReader();
          reader.onload = async function (e) {
            formData.profilePicture = e.target.result;
            await saveUserProfile(formData);
          };
          reader.readAsDataURL(file);
        } else {
          await saveUserProfile(formData);
        }
      });

      $("#profile-picture-input").on("change", function () {
        const file = this.files[0];
        if (file) {
          const reader = new FileReader();
          reader.onload = function (e) {
            $("#profile-picture-preview").attr("src", e.target.result);
          };
          reader.readAsDataURL(file);
        }
      });
      break;

    case "my-profile.html":
      loadUserProfile();

      $("#profile-form").on("submit", async function (e) {
        e.preventDefault();

        const formData = {
          fullName: $("#profile-fullName").val(),
          phone: $("#profile-phone").val(),
        };

        const fileInput = $("#profile-picture-input")[0];
        if (fileInput.files.length > 0) {
          const file = fileInput.files[0];
          const reader = new FileReader();
          reader.onload = async function (e) {
            formData.profilePicture = e.target.result;
            await saveUserProfile(formData);
          };
          reader.readAsDataURL(file);
        } else {
          await saveUserProfile(formData);
        }
      });

      $("#profile-picture-input").on("change", function () {
        const file = this.files[0];
        if (file) {
          const reader = new FileReader();
          reader.onload = function (e) {
            $("#profile-picture-preview").attr("src", e.target.result);
          };
          reader.readAsDataURL(file);
        }
      });
      break;

    case "notifications.html":
      loadUserSettings();

      $("#settings-form").on("submit", function (e) {
        e.preventDefault();
        saveUserSettings();
      });
      break;

    case "faq.html":
      break;
  }
}

$(document).on("shown.bs.modal", "#cropImageModal", function () {
  setTimeout(() => {
    updateCircleOverlay();
  }, 150);
});

document.addEventListener("DOMContentLoaded", function () {
  const form = document.getElementById("security-form");
  if (form) {
    form.addEventListener("submit", async function (e) {
      e.preventDefault();
      const oldPwd = document.getElementById("old-password").value.trim();
      const newPwd = document.getElementById("new-password").value.trim();
      const confirmPwd = document
        .getElementById("confirm-password")
        .value.trim();
      if (!oldPwd || !newPwd || !confirmPwd) {
        safeToastrMessage("error", window.i18next.t("allFieldsRequired"));
        return;
      }
      if (newPwd.length < 6) {
        safeToastrMessage(
          "error",
          window.i18next.t("newPasswordMustBeAtLeast6Characters"),
        );
        return;
      }
      if (newPwd !== confirmPwd) {
        safeToastrMessage("error", window.i18next.t("passwordsDoNotMatch"));
        return;
      }
      try {
        const token = localStorage.getItem("authToken");
        if (!token) {
          safeToastrMessage("error", window.i18next.t("authenticationRequired"));
          return;
        }
        const res = await fetch(
          "/api/Auth/change-password",
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: "Bearer " + token,
            },
            body: JSON.stringify({
              currentPassword: oldPwd,
              newPassword: newPwd,
              confirmPassword: confirmPwd,
              language: getCurrentLanguage(),
            }),
          },
        );
        const data = await res.json();
        if (res.ok && data.success !== false) {
          safeToastrMessage(
            "success",
            window.i18next.t("passwordChangedSuccessfully"),
          );
          form.reset();
        } else {
          if (
            data.message &&
            (data.message.toLowerCase().includes("old password is incorrect") ||
              data.message
                .toLowerCase()
                .includes("current password is incorrect") ||
              data.message.toLowerCase().includes("mật khẩu cũ không đúng") ||
              data.message
                .toLowerCase()
                .includes("古いパスワードが正しくありません"))
          ) {
            safeToastrMessage("error", window.i18next.t("oldPasswordIncorrect"));
          } else {
            safeToastrMessage(
              "error",
              data.message || window.i18next.t("changePasswordFailed"),
            );
          }
        }
      } catch (err) {
        safeToastrMessage("error", window.i18next.t("changePasswordFailed"));
      }
    });
  }
}); 

if (typeof window.i18next !== "undefined") {
  window.i18next.on("languageChanged", function () {
    setTimeout(() => {
      reloadCurrentPageData();
    }, 100);
  });
}

window.generateLetterAvatarFromUser = generateLetterAvatarFromUser;

function setExportButtonLoading(buttonElement, isLoading) {
  if (!buttonElement) return;
  if (isLoading) {
    buttonElement.disabled = true;
    buttonElement.innerHTML =
      '<i class="ti ti-loader ti-spin me-2"></i>' +
      (window.i18next ? window.i18next.t("exporting") : "Exporting...");
  } else {
    buttonElement.disabled = false;
    buttonElement.innerHTML =
      '<i class="ti ti-download me-2"></i>' +
      (window.i18next
        ? window.i18next.t("exportAllUsers")
        : "Export All Users");
  }
}

window.exportAllUsersToCSV = async function (buttonElement) {
  try {
    setExportButtonLoading(buttonElement, true);
    const token = localStorage.getItem("authToken");
    if (!token) {
      if (window.toastr)
        safeToastrMessage(
          "error",
          window.i18next
            ? window.i18next.t("notAuthenticated")
            : "Not authenticated",
        );
      buttonElement.disabled = false;
      buttonElement.innerHTML =
        '<i class="ti ti-download me-2"></i>' +
        (window.i18next
          ? window.i18next.t("exportAllUsers")
          : "Export All Users");
      return;
    }

    const countRes = await fetch(
      "/api/User?includeDeleted=true&page=1&pageSize=1",
      {
        headers: { Authorization: `Bearer ${token}` },
      },
    );
    const countJson = await countRes.json();
    const totalCount =
      countJson.pagination?.totalCount || countJson.totalCount || 0;

    if (totalCount === 0) {
      if (window.toastr)
        safeToastrMessage(
          "warning",
          window.i18next
            ? window.i18next.t("noDataToExport")
            : "No data to export",
        );
      buttonElement.disabled = false;
      buttonElement.innerHTML =
        '<i class="ti ti-download me-2"></i>' +
        (window.i18next
          ? window.i18next.t("exportAllUsers")
          : "Export All Users");
      return;
    }

    const res = await fetch(
      `/api/User?includeDeleted=true&pageSize=${totalCount}`,
      {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      },
    );
    const json = await res.json();
    let users = Array.isArray(json.data) ? json.data : [];

    const currentPath = window.location.pathname;
    if (currentPath.includes("deactive-users")) {
      users = users.filter(
        (u) => (u.status === 4 || u.status === "Banned") && u.deletedAt,
      );
    } else if (currentPath.includes("active-users")) {
      users = users.filter((u) => u.status === 1 || u.status === "Active");
    }

    if (!users.length) {
      if (window.toastr)
        safeToastrMessage(
          "warning",
          window.i18next
            ? window.i18next.t("noDataToExport")
            : "No data to export",
        );
      buttonElement.disabled = false;
      buttonElement.innerHTML =
        '<i class="ti ti-download me-2"></i>' +
        (window.i18next
          ? window.i18next.t("exportAllUsers")
          : "Export All Users");
      return;
    }

    const allFields = users.reduce((fields, u) => {
      Object.keys(u).forEach((k) => {
        if (!fields.includes(k)) fields.push(k);
      });
      return fields;
    }, []);

    const preferredOrder = [
      "id",
      "username",
      "email",
      "fullName",
      "phoneNumber",
      "dateOfBirth",
      "address",
      "bio",
      "status",
      "isVerified",
      "googleId",
      "profilePicture",
      "loginProvider",
      "createdAt",
      "updatedAt",
      "lastLoginAt",
      "deletedAt",
      "isDeleted",
    ];
    const fields = preferredOrder
      .filter((f) => allFields.includes(f))
      .concat(allFields.filter((f) => !preferredOrder.includes(f)));

    const fieldToI18nKey = {
      id: "id",
      username: "username",
      fullName: "fullName",
      email: "email",
      phoneNumber: "phoneNumber",
      dateOfBirth: "dateOfBirth",
      address: "address",
      bio: "bio",
      status: "status",
      isVerified: "emailVerified",
      googleId: "googleId",
      profilePicture: "profilePicture",
      loginProvider: "loginProvider",
      updatedAt: "updatedAt",
      lastLoginAt: "lastLoginAt",
      deletedAt: "deletedAt",
      isDeleted: "isDeleted",
      createdAt: "createdAt",
    };

    const headers = fields.map((f) =>
      window.i18next
        ? window.i18next.t(fieldToI18nKey[f] || f)
        : fieldToI18nKey[f] || f,
    );
    const rows = users.map((u) =>
      fields.map((f) => {
        let val = u[f];
        if (f === "status") {
          switch (val) {
            case 1:
              return window.i18next ? window.i18next.t("active") : "Active";
            case 2:
              return window.i18next ? window.i18next.t("inactive") : "Inactive";
            case 3:
              return window.i18next
                ? window.i18next.t("suspended")
                : "Suspended";
            case 4:
              return window.i18next ? window.i18next.t("banned") : "Banned";
            default:
              return window.i18next ? window.i18next.t("unknown") : "Unknown";
          }
        }
        if (f === "isVerified")
          return val
            ? window.i18next
              ? window.i18next.t("yes")
              : "Yes"
            : window.i18next
              ? window.i18next.t("no")
              : "No";
        if (val === undefined || val === null) return "";
        if (val instanceof Date) return val.toLocaleString("en-GB");
        if (typeof val === "string" && val.match(/^\d{4}-\d{2}-\d{2}T/)) {
          try {
            return new Date(val).toLocaleString("en-GB");
          } catch {
            return val;
          }
        }
        return val;
      }),
    );

    function escapeCsvValue(val) {
      if (val == null) return "";
      val = String(val);
      if (val.includes('"')) val = val.replace(/"/g, '""');
      if (val.includes(",") || val.includes("\n") || val.includes('"')) {
        return `"${val}"`;
      }
      return val;
    }

    const delimiter = ";";
    const headersCsv = headers.map(escapeCsvValue).join(delimiter);
    const rowsCsv = rows
      .map((r) => r.map(escapeCsvValue).join(delimiter))
      .join("\n");
    const BOM = "\uFEFF";
    const csv = BOM + headersCsv + "\n" + rowsCsv;
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = getExportFileName("csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    if (window.toastr)
      safeToastrMessage(
        "success",
        window.i18next
          ? window.i18next.t("exportSuccess")
          : "Export successful",
      );
  } catch (err) {
    console.error("Export error:", err);
    if (window.toastr)
      safeToastrMessage(
        "error",
        window.i18next ? window.i18next.t("exportFailed") : "Export failed",
      );
  } finally {
    setExportButtonLoading(buttonElement, false);
  }
};

document.addEventListener("DOMContentLoaded", function () {
  const btn = document.getElementById("export-all-users-btn");
  if (btn) {
    btn.addEventListener("click", async function (e) {
      e.preventDefault();
      await window.exportAllUsersToCSV(this);
    });
  }
});

function getExportFileName(ext) {
  const path = window.location.pathname;
  let prefix = "users_export";
  if (path.includes("all-users")) prefix = "all_users_export";
  else if (path.includes("active-users")) prefix = "active_users_export";
  else if (path.includes("deactive-users")) prefix = "deactive_users_export";
  return (
    prefix +
    "_" +
    new Date().toISOString().replace(/[:.]/g, "-") +
    (ext ? "." + ext : "")
  );
}

function safeToastrMessage(type, msg) {
  if (typeof window.i18next !== "undefined" && typeof window.i18next.t === "function") {
    if (typeof msg === "string" && (!msg.trim().includes(" ") || msg === msg.toUpperCase())) {
      msg = window.i18next.t(msg);
    }
  }
  if (typeof window.safeToastr === "function") {
    window.safeToastr(type, msg);
  } else if (typeof toastr !== "undefined") {
    toastr.clear();
    toastr[type](msg);
  }
}

function translateUserTableHeaders() {
  $("table.datatables-users thead th").each(function () {
    var key = $(this).attr("data-i18n");
    if (key && window.i18next && window.i18next.isInitialized) {
      $(this).text(window.i18next.t(key));
    }
  });
}

$(document).ready(function () {
  if (!window.location.pathname.endsWith("security.html")) return;

  function updateDisableTOTPModalI18n() {
    $("#disableTOTPModal .modal-title").text(window.i18next.t("disableGoogleAuthenticator"));
    $("#disableTOTPInput").attr("placeholder", window.i18next.t("enterOtpCode"));
    $("#confirmDisableTOTPBtn").text(window.i18next.t("disable2fa"));
  }

  if (window.i18next) {
    window.i18next.on("languageChanged", function () {
      updateDisableTOTPModalI18n();
    });
  }

  if ($("#disableTOTPModal").length === 0) {
    $("<div class='modal fade' id='disableTOTPModal' tabindex='-1' aria-hidden='true'>"
      + "<div class='modal-dialog modal-simple modal-dialog-centered'>"
      + "<div class='modal-content'>"
      + `<div class='modal-header'><h5 class='modal-title'>${window.i18next.t("disableGoogleAuthenticator")}</h5><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div>`
      + "<div class='modal-body'>"
      + `<input type='text' class='form-control mb-2' id='disableTOTPInput' placeholder='${window.i18next.t("enterOtpCode")}'>`
      + "<div class='text-danger small' id='disableTOTPError'></div>"
      + "</div>"
      + "<div class='modal-footer'>"
      + `<button type='button' class='btn btn-danger' id='confirmDisableTOTPBtn'>${window.i18next.t("disable2fa")}</button>`
      + "</div></div></div></div>")
      .appendTo("body");
  }

  $(document).on("click", "#disableTOTPBtn", function () {
    $("#disableTOTPInput").val("");
    $("#disableTOTPError").text("");
    updateDisableTOTPModalI18n();
    $("#disableTOTPModal").modal("show");
  });

  $(document).on("click", "#confirmDisableTOTPBtn", async function () {
    const code = $("#disableTOTPInput").val().trim();
    const lang = localStorage.getItem("i18nextLng") || (window.i18next && window.i18next.language) || "en";
    if (!code) {
      $("#disableTOTPError").text(window.i18next.t("enterOtpCode"));
      return;
    }
    $("#confirmDisableTOTPBtn").prop("disabled", true);
    try {
      const token = localStorage.getItem("authToken");
      const res = await fetch("/api/Auth/disable-2fa-totp", {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ code, language: lang })
      });
      const data = await res.json();
      if (res.ok && data.success) {
        $("#disableTOTPModal").modal("hide");
        safeToastrMessage("success", data.message || window.i18next.t("2faDisabledSuccess"));
        setTimeout(() => location.reload(), 1000);
      } else {
        $("#disableTOTPError").text(data.message || window.i18next.t("invalidOtpCode"));
      }
    } catch (err) {
      $("#disableTOTPError").text(window.i18next.t("disable2faFailed"));
    } finally {
      $("#confirmDisableTOTPBtn").prop("disabled", false);
    }
  });
});

function renderTwoFAStatus(twoFactorEnabled) {
  let $twoFAStatus = $("#twoFAStatus");
  if ($twoFAStatus.length === 0) {
    $twoFAStatus = $("<div id='twoFAStatus' class='mb-2'></div>");
    $(".two-steps-verification-card .card-body").prepend($twoFAStatus);
  }
  $("#enableTOTPBtn, #disableTOTPBtn, #totpGuideSteps").remove();
  if (twoFactorEnabled) {
    $twoFAStatus.html(`<span class='badge bg-success'><i class='ti ti-shield-lock me-1'></i> ${window.i18next.t("twoFactorAuthEnabled")}</span>`);
    $("<button class='btn btn-outline-danger mt-2 ms-2' id='disableTOTPBtn'><i class='ti ti-x me-1'></i> " + window.i18next.t('disable2fa') + "</button>")
      .appendTo($(".two-steps-verification-card .card-body"));
  } else {
    $twoFAStatus.html(`<span class='badge bg-secondary'><i class='ti ti-shield-off me-1'></i> ${window.i18next.t("twoFactorAuthNotEnabled")}</span>`);
    $("<button class='btn btn-outline-primary mt-2' id='enableTOTPBtn'><i class='ti ti-qrcode me-1'></i> Google Authenticator</button>")
      .appendTo($(".two-steps-verification-card .card-body"));
  }
}

$(document).ready(async function () {
  if (!window.location.pathname.endsWith("security.html")) return;
  let twoFactorEnabled = false;
  try {
    const token = localStorage.getItem("authToken");
    const res = await fetch("/api/Auth/two-factor-status", {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.ok) {
      const data = await res.json();
      if (data && typeof data.twoFactorEnabled !== "undefined") {
        twoFactorEnabled = !!data.twoFactorEnabled;
      }
    }
  } catch {}
  renderTwoFAStatus(twoFactorEnabled);
  if (window.i18next) {
    window.i18next.on("languageChanged", function () {
      renderTwoFAStatus(twoFactorEnabled);
    });
  }
});

$(document).off("click", "#enableTOTPBtn").on("click", "#enableTOTPBtn", async function () {
  function updateTOTPModalI18n() {
    $("#totpModal .modal-title").text(window.i18next.t("googleAuthenticator"));
    $("#totpGuideStepsModal li").eq(0).text(window.i18next.t("totpStep1"));
    $("#totpGuideStepsModal li").eq(1).text(window.i18next.t("totpStep2"));
    $("#totpGuideStepsModal li").eq(2).text(window.i18next.t("totpStep3"));
    $("#totpOtpInput").attr("placeholder", window.i18next.t("enterOtpCode"));
    $("#totpVerifyBtn").text(window.i18next.t("verify"));
  }

  if (window.i18next) {
    window.i18next.on("languageChanged", function () {
      updateTOTPModalI18n();
    });
  }

  if ($("#totpModal").length === 0) {
    $("<div class='modal fade' id='totpModal' tabindex='-1' aria-hidden='true'>"
      + "<div class='modal-dialog modal-simple modal-dialog-centered'>"
      + "<div class='modal-content'>"
      + `<div class='modal-header'><h5 class='modal-title text-center w-100'>${window.i18next.t("googleAuthenticator")}</h5><button type='button' class='btn-close' data-bs-dismiss='modal' aria-label='Close'></button></div>`
      + "<div class='modal-body'>"
      + `<ol class='mb-3 px-2' id='totpGuideStepsModal'>`
      + `<li>${window.i18next.t("totpStep1")}</li>`
      + `<li>${window.i18next.t("totpStep2")}</li>`
      + `<li>${window.i18next.t("totpStep3")}</li>`
      + "</ol>"
      + "<div id='totpQrSection' class='text-center mb-3'></div>"
      + `<div class='mb-2'><input type='text' id='totpOtpInput' class='form-control' maxlength='8' autocomplete='one-time-code' placeholder='${window.i18next.t("enterOtpCode")}'></div>`
      + "<div class='text-danger small' id='totpOtpError'></div>"
      + "</div>"
      + `<div class='modal-footer'><button type='button' class='btn btn-primary w-100' id='totpVerifyBtn'>${window.i18next.t('verify')}</button></div>`
      + "</div></div></div>")
      .appendTo("body");
  }
  updateTOTPModalI18n();
  $("#totpModal").modal("show");

  const token = localStorage.getItem("authToken");
  $("#totpGuideStepsModal li").eq(0).text(window.i18next.t("totpStep1"));
  $("#totpGuideStepsModal li").eq(1).text(window.i18next.t("totpStep2"));
  $("#totpGuideStepsModal li").eq(2).text(window.i18next.t("totpStep3"));
  $("#totpModal").modal("show");

  $("#totpQrSection").html('<div class="text-muted">' + (window.i18next ? window.i18next.t("loading") : "Loading...") + '</div>');
  try {
    const lang = window.i18next?.language || localStorage.getItem("i18nextLng") || "en";
    const res = await fetch("/api/Auth/enable-2fa-totp", {
      method: "POST",
      headers: { 
        Authorization: `Bearer ${token}`,
        "Accept-Language": lang
      },
    });
    const data = await res.json();
    if (res.ok && data.qrCodeImage) {
      $("#totpQrSection").html(`<img src='${data.qrCodeImage}' alt='QR Code' style='width:180px;height:180px;margin-bottom:10px;'><div class='mt-2'><b>${window.i18next.t("secret")}:</b> <span style='user-select:all'>${data.secret || ""}</span></div>`);
    } else {
      const errorMsg = data.errorCode ? window.i18next.t(data.errorCode) : (data.message || window.i18next.t("failedToGenerateQr"));
      $("#totpQrSection").html('<div class="text-danger">' + errorMsg + '</div>');
    }
  } catch (err) {
    $("#totpQrSection").html('<div class="text-danger">' + (window.i18next ? window.i18next.t("failedToGenerateQr") : "Failed to generate QR") + '</div>');
  }

  $("#totpVerifyBtn").off("click").on("click", async function () {
    const otp = $("#totpOtpInput").val().trim();
    if (!otp || otp.length < 4) {
      $("#totpOtpError").text(window.i18next.t("otpInvalid"));
      return;
    }
    $("#totpVerifyBtn").prop("disabled", true);
    $("#totpOtpError").text("");
    try {
      const res = await fetch("/api/Auth/verify-2fa-totp", {
        method: "POST",
        headers: { "Authorization": `Bearer ${token}`, "Content-Type": "application/json" },
        body: JSON.stringify({ code: otp }),
      });
      const data = await res.json();
      if (res.ok && data.success) {
        $("#totpModal").modal("hide");
        safeToastrMessage("success", window.i18next.t("twoFactorEnabledSuccessfully"));
        setTimeout(() => location.reload(), 1000);
      } else {
        $("#totpOtpError").text(data.message || window.i18next.t("otpInvalid"));
      }
    } catch (err) {
      $("#totpOtpError").text(window.i18next.t("otpInvalid"));
    } finally {
      $("#totpVerifyBtn").prop("disabled", false);
    }
  });
});
