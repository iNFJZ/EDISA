function getLocalizedText(key) {
    if (window.i18next && window.i18next.t) {
        const translated = window.i18next.t(key);
        if (translated !== key) {
            return translated;
        }
    }
    
    const fallbackTexts = {
        'unknown': 'Unknown',
        'failedToFetchFileList': 'Failed to fetch file list',
        'failedToLoadFiles': 'Failed to load files',
        'noDataFound': 'No data found',
        'preview': 'Preview',
        'delete': 'Delete',
        'failedToDeleteFile': 'Failed to delete file',
        'fileDeletedSuccessfully': 'File deleted successfully'
    };

    return fallbackTexts[key] || key;
}

function getFileExtension(filename) {
  if (!filename) return getLocalizedText('unknown');
  const ext = filename.split(".").pop();
  if (!ext || ext === filename) return getLocalizedText('unknown');
  return ext.toUpperCase();
}

function getCurrentLanguage() {
    const storedLang = localStorage.getItem('language') || localStorage.getItem('i18nextLng');
    if (storedLang) {
        return storedLang;
    }
    
    const htmlLang = document.documentElement.lang;
    if (htmlLang && ['en', 'vi', 'ja'].includes(htmlLang)) {
        return htmlLang;
    }
    
    return 'en';
}

function getDataTablesLanguage() {
    const currentLang = getCurrentLanguage();
    
    if (window.i18next && window.i18next.t) {
        return {
            "search": window.i18next.t('search') + ":",
            "lengthMenu": window.i18next.t('lengthMenu'),
            "info": window.i18next.t('info'),
            "infoEmpty": window.i18next.t('infoEmpty'),
            "infoFiltered": window.i18next.t('infoFiltered'),
            "paginate": {
                "first": window.i18next.t('first'),
                "last": window.i18next.t('last'),
                "next": window.i18next.t('next'),
                "previous": window.i18next.t('previous')
            },
            "emptyTable": window.i18next.t('emptyTable'),
            "zeroRecords": window.i18next.t('zeroRecords')
        };
    }
      
    return {
        "search": "Search:",
        "lengthMenu": "Show _MENU_ entries",
        "info": "Showing _START_ to _END_ of _TOTAL_ entries",
        "infoEmpty": "Showing 0 to 0 of 0 entries",
        "infoFiltered": "(filtered from _MAX_ total entries)",
        "paginate": {
            "first": "First",
            "last": "Last",
            "next": "Next",
            "previous": "Previous"
        },
        "emptyTable": "No data available in table",
        "zeroRecords": "No matching records found"
    };
}

function formatFileSize(size) {
  if (size >= 1024 * 1024) return (size / (1024 * 1024)).toFixed(2) + " MB";
  if (size >= 1024) return (size / 1024).toFixed(2) + " KB";
  return size + " B";
}

async function fetchAndRenderFiles() {
  try {
    const token = localStorage.getItem("authToken");
            const res = await fetch("/api/File/list", {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    if (!res.ok) throw new Error(getLocalizedText('failedToFetchFileList'));
    const data = await res.json();
    window.fileListData = Array.isArray(data) ? data : [];
    window.loadFilesTable();
  } catch (err) {
    window.fileListData = [];
    window.loadFilesTable();
    if (typeof toastr !== "undefined")
      toastr.error(err.message || getLocalizedText('failedToLoadFiles'));
  }
}

function openDeleteFileModal(file) {
  document.querySelector(".delete-file-name").textContent =
    file.fileName || "-";
  document.querySelector(".delete-file-size").textContent =
    typeof file.fileSize === "number" ? formatFileSize(file.fileSize) : "-";
  document.querySelector(".delete-file-type").textContent = getFileExtension(
    file.fileName,
  );
  document.querySelector(".delete-file-created").textContent = file.uploadedAt
    ? new Date(file.uploadedAt).toLocaleString("en-GB")
    : "-";
  $("#deleteFileModal").modal("show");
  const confirmBtn = document.getElementById("confirmDeleteFile");
  confirmBtn.onclick = null;
  confirmBtn.onclick = async function () {
    try {
      const token = localStorage.getItem("authToken");
      const res = await fetch(
        `/api/File/delete/${encodeURIComponent(file.fileName)}`,
        {
          method: "DELETE",
          headers: token ? { Authorization: `Bearer ${token}` } : {},
        },
      );
      if (!res.ok) throw new Error(getLocalizedText('failedToDeleteFile'));
      if (typeof toastr !== "undefined")
        toastr.success(getLocalizedText('fileDeletedSuccessfully'));
      $("#deleteFileModal").modal("hide");
      fetchAndRenderFiles();
    } catch (err) {
      if (typeof toastr !== "undefined")
        toastr.error(err.message || getLocalizedText('failedToDeleteFile'));
    }
  };
}

document.addEventListener("DOMContentLoaded", function() {
    fetchAndRenderFiles();
    
    if (window.i18next) {
        window.i18next.on('languageChanged', function() {
            const $table = $(".datatables-files");
            if ($table.length && $.fn.DataTable.isDataTable($table)) {
                $table.DataTable().destroy();
                loadFilesTable();
            }
        });
    }
    
    window.addEventListener('languageChanged', function(event) {
        const $table = $(".datatables-files");
        if ($table.length && $.fn.DataTable.isDataTable($table)) {
            $table.DataTable().destroy();
            loadFilesTable();
        }
    });
});

window.loadFilesTable = async function () {
  const files = window.fileListData || [];
  const tbody = document.querySelector(".datatables-files tbody");
  if (!tbody) return;
  if (!files.length) {
    tbody.innerHTML = `<tr><td colspan="5">${getLocalizedText('noDataFound')}</td></tr>`;
    return;
  }
  tbody.innerHTML = files
    .map(
      (file, idx) => `
    <tr class="file-row" data-file-idx="${idx}">
      <td><span class="cursor-pointer" data-action="preview">${file.fileName || "-"}</span></td>
      <td><span class="cursor-pointer" data-action="preview">${typeof file.fileSize === "number" ? formatFileSize(file.fileSize) : "-"}</span></td>
      <td><span class="cursor-pointer" data-action="preview">${getFileExtension(file.fileName)}</span></td>
      <td><span class="cursor-pointer" data-action="preview">${file.uploadedAt ? new Date(file.uploadedAt).toLocaleString("en-GB") : "-"}</span></td>
      <td>
        <div class="d-flex gap-2">
          <button class="btn btn-sm btn-icon btn-outline-primary btn-view-file" title="${getLocalizedText('preview')}" data-bs-toggle="tooltip" data-bs-placement="top">
            <i class="ti ti-eye"></i>
          </button>
          <button class="btn btn-sm btn-icon btn-outline-danger btn-delete-file" title="${getLocalizedText('delete')}" data-bs-toggle="tooltip" data-bs-placement="top">
            <i class="ti ti-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `,
    )
    .join("");
  tbody.querySelectorAll("tr.file-row").forEach((row, idx) => {
    row.querySelectorAll('[data-action="preview"]').forEach((el) => {
      el.addEventListener("click", function () {
        showFileDetailModal(files[idx]);
      });
    });
    row.querySelector(".btn-view-file").onclick = function (e) {
      e.preventDefault();
      showFileDetailModal(files[idx]);
    };
  });

  const $table = $(".datatables-files");
  if ($table.length) {
    if ($.fn.DataTable.isDataTable($table)) {
      $table.DataTable().destroy();
    }
    $table.DataTable({
      order: [[0, "asc"]],
      columnDefs: [{ targets: -1, orderable: false, searchable: false }],
      language: getDataTablesLanguage(),
    });
  }

  const tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]'),
  );
  tooltipTriggerList.forEach((tooltipTriggerEl) => {
    try {
      new bootstrap.Tooltip(tooltipTriggerEl);
    } catch (_) {}
  });
};

window.showFileDetailModal = function (file) {
  document.querySelector(".file-name").textContent = file.fileName || "-";
  document.querySelector(".file-size").textContent =
    typeof file.fileSize === "number" ? formatFileSize(file.fileSize) : "-";
  document.querySelector(".file-type").textContent = getFileExtension(
    file.fileName,
  );
  document.querySelector(".file-created").textContent = file.uploadedAt
    ? new Date(file.uploadedAt).toLocaleString("en-GB")
    : "-";

  const descElem = document.querySelector(".file-description");
  if (descElem) {
    if (file.description && file.description.trim() !== "") {
      descElem.textContent = file.description;
      descElem.parentElement.style.display = "block";
    } else {
      descElem.textContent = "-";
      descElem.parentElement.style.display = "none";
    }
  }

  const downloadBtn = document.querySelector(".btn-download-file");
  downloadBtn.onclick = async function () {
    if (!file.fileName) return;
    
    try {
      const token = localStorage.getItem("authToken");
      const response = await fetch(`/api/File/download/${encodeURIComponent(file.fileName)}`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      
      if (!response.ok) {
        throw new Error(getLocalizedText('downloadFailed'));
      }
      
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = file.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      showNotification("success", getLocalizedText('fileDownloadedSuccessfully'));
    } catch (error) {
      showNotification("error", getLocalizedText('downloadFailed'));
    }
  };

  const deleteBtn = document.querySelector("#viewFileModal .btn-delete-file");
  deleteBtn.onclick = function () {
    $("#viewFileModal").off("hidden.bs.modal");
    $("#viewFileModal").one("hidden.bs.modal", function () {
      openDeleteFileModal(file);
    });
    $("#viewFileModal").modal("hide");
  };

  $("#viewFileModal").modal("show");
};
