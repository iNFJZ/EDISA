(function () {
  function parseDeviceInfo(userAgent) {
    const ua = (userAgent || "").toLowerCase();
    let browser = "Unknown";
    if (ua.includes("edg/")) browser = "Microsoft Edge";
    else if (ua.includes("chrome")) browser = "Chrome";
    else if (ua.includes("safari") && !ua.includes("chrome")) browser = "Safari";
    else if (ua.includes("firefox")) browser = "Firefox";
    else if (ua.includes("msie") || ua.includes("trident")) browser = "Internet Explorer";

    let deviceName = "Unknown Device";
    let iconHtml = '<i class="ti ti-device-laptop me-2 ti-sm"></i>';

    const isMac = ua.includes("mac os x");
    const isIphone = ua.includes("iphone");
    const isAndroid = ua.includes("android");

    if (isMac) {
      const m1Hint = ua.includes("applewebkit") && ua.includes("version/");
      deviceName = m1Hint ? "MacBook Pro M1" : "MacBook Pro";
      iconHtml = '<i class="ti ti-brand-apple me-2 ti-sm"></i>';
    } else if (isIphone) {
      if (ua.match(/iphone os 18/)) {
        deviceName = "iPhone 16 Pro Max";
      } else if (ua.match(/iphone os 17/)) {
        deviceName = "iPhone 15 Pro Max";
      } else if (ua.match(/iphone os 16/)) {
        deviceName = "iPhone 14 Pro Max";
      } else {
        deviceName = "iPhone";
      }
      iconHtml = '<i class="ti ti-device-mobile text-danger me-2 ti-sm"></i>';
    } else if (isAndroid) {
      deviceName = "Android Phone";
      iconHtml = '<i class="ti ti-brand-android text-success me-2 ti-sm"></i>';
    } else if (ua.includes("windows")) {
      deviceName = "Windows PC";
      iconHtml = '<i class="ti ti-brand-windows text-info me-2 ti-sm"></i>';
    }

    return { browser, deviceName, iconHtml };
  }

  async function fetchUserAuditLogs(userId, token, page = 1, pageSize = 50) {
    const url = `/api/Audit/logs/user/${userId}?page=${page}&pageSize=${pageSize}`;
    const res = await fetch(url, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    if (!res.ok) return { data: [], pagination: null };
    const json = await res.json();
    return { data: json.data || [], pagination: json.pagination };
  }

  function formatDateUtcToLocal(iso) {
    try {
      const d = new Date(iso);
      if (isNaN(d.getTime())) return iso || "";
      return d.toLocaleString();
    } catch {
      return iso || "";
    }
  }

  async function renderRecentDevices() {
    const tbody = document.querySelector(
      "table.table.border-top tbody.table-border-bottom-0"
    );
    if (!tbody) return;

    tbody.innerHTML = "";

    const token = localStorage.getItem("authToken");
    if (!token || !window.adminAuth) return;
    const userInfo = window.adminAuth.getCurrentUserInfo();
    if (!userInfo || !userInfo.id) return;

    const { data } = await fetchUserAuditLogs(userInfo.id, token, 1, 100);
    const loginEvents = data
      .filter((e) => (e.action || "").toUpperCase() === "LOGIN")
      .sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));

    if (!loginEvents.length) {
      const tr = document.createElement("tr");
      tr.innerHTML = `<td colspan="4" class="text-center text-muted">No recent devices</td>`;
      tbody.appendChild(tr);
      return;
    }

    loginEvents.slice(0, 10).forEach((e) => {
      const meta = e.metadata || {};
      const ua = e.userAgent || meta.UserAgent || "";
      let { browser, deviceName, iconHtml } = parseDeviceInfo(ua);

      const brand = meta.DeviceBrand || meta.brand || meta.deviceBrand;
      const model = meta.DeviceModel || meta.model || meta.deviceModel;
      const ddName = meta.DeviceName || meta.deviceName;
      if (brand || model || ddName) {
        const brandStr = (brand || "").toString().trim();
        const modelStr = (model || ddName || "").toString().trim();
        const composed = [brandStr, modelStr].filter(Boolean).join(" ");
        if (composed) deviceName = composed;
        if (!iconHtml) iconHtml = '<i class="ti ti-device-laptop me-2 ti-sm"></i>';
        const low = composed.toLowerCase();
        if (low.includes("iphone") || low.includes("ipad") || low.includes("apple")) {
          iconHtml = '<i class="ti ti-device-mobile text-danger me-2 ti-sm"></i>';
          browser = browser.includes("Safari") ? browser : "Safari on iPhone";
        }
        if (low.includes("android") || low.includes("samsung") || low.includes("xiaomi") || low.includes("pixel")) {
          iconHtml = '<i class="ti ti-brand-android text-success me-2 ti-sm"></i>';
          if (!browser.toLowerCase().includes("chrome")) browser = "Chrome on Android";
        }
        if (low.includes("mac") || low.includes("macbook") || low.includes("apple mac")) {
          iconHtml = '<i class="ti ti-brand-apple me-2 ti-sm"></i>';
          browser = browser.includes("MacOS") ? browser : "Safari on macOS";
        }
      }

      const locationText = e.ipAddress || meta.IpAddress || "Unknown";
      const when = formatDateUtcToLocal(e.createdAt);

      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td class="text-truncate">${iconHtml}<span class="fw-medium">${browser}</span></td>
        <td class="text-truncate">${deviceName}</td>
        <td class="text-truncate">${locationText}</td>
        <td class="text-truncate">${when}</td>
      `;
      tbody.appendChild(tr);
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    setTimeout(renderRecentDevices, 0);
    if (window.i18next && window.i18next.on) {
      window.i18next.on("languageChanged", function () {
        setTimeout(renderRecentDevices, 0);
      });
    }
  });
})();


