let emailTemplatesTable;
let currentTemplateId = null;
let isEditMode = false;

function onI18nReady(callback) {
    if (window.i18next && window.i18next.isInitialized) {
        callback();
        return;
    }
    if (window.i18next && typeof window.i18next.on === 'function') {
        const handler = function() { callback(); };
        window.i18next.on('initialized', handler);
        return;
    }
    callback();
}

$(document).ready(function() {
    onI18nReady(() => initializeEmailTemplates());
    bindEventHandlers();
    
    if (window.i18next) {
        window.i18next.on('languageChanged', function() {
            if (emailTemplatesTable) {
                emailTemplatesTable.destroy();
                initializeEmailTemplates();
            }
        });
    }
    
    window.addEventListener('languageChanged', function() {
        if (emailTemplatesTable) {
            emailTemplatesTable.destroy();
            initializeEmailTemplates();
        }
    });
});

function initializeEmailTemplates() {
    const $table = $('.datatables-email-templates');
    if ($.fn.DataTable.isDataTable($table)) {
        $table.DataTable().destroy();
        $table.find('tbody').empty();
    }
    emailTemplatesTable = $table.DataTable({
        destroy: true,
        ajax: {
            url: '/api/Email',
            type: 'GET',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            },
            dataSrc: '',
            error: function(xhr) {
                if (xhr.status === 401) {
                    window.location.href = '/auth/login.html';
                }
            }
        },
        columns: [
            { 
                data: 'name',
                render: function(data, type, row) {
                    return `<span class="fw-semibold cursor-pointer" onclick="previewEmailTemplate(${row.id})">${data}</span>`;
                }
            },
            { 
                data: 'language',
                render: function(data, type, row) {
                    const languageMap = {
                        'en': '<span class="badge bg-primary cursor-pointer" onclick="previewEmailTemplate(' + row.id + ')">English</span>',
                        'vi': '<span class="badge bg-success cursor-pointer" onclick="previewEmailTemplate(' + row.id + ')">Tiếng Việt</span>',
                        'ja': '<span class="badge bg-info cursor-pointer" onclick="previewEmailTemplate(' + row.id + ')">日本語</span>'
                    };
                    return languageMap[data] || data;
                }
            },
            { 
                data: 'isActive',
                render: function(data, type, row) {
                    const statusClass = data ? 'bg-success' : 'bg-danger';
                    const statusText = data ? 'Active' : 'Inactive';
                    return `<span class="badge ${statusClass} cursor-pointer" onclick="previewEmailTemplate(${row.id})">${statusText}</span>`;
                }
            },
            { 
                data: 'createdAt',
                render: function(data, type, row) {
                    return `<span class="cursor-pointer" onclick="previewEmailTemplate(${row.id})">${moment(data).format('DD/MM/YYYY HH:mm')}</span>`;
                }
            },
            { 
                data: 'updatedAt',
                render: function(data, type, row) {
                    return data ? `<span class="cursor-pointer" onclick="previewEmailTemplate(${row.id})">${moment(data).format('DD/MM/YYYY HH:mm')}</span>` : '-';
                }
            },
            {
                data: 'deletedAt',
                render: function(data, type, row) {
                    return data ? `<span class="text-danger">${moment(data).format('DD/MM/YYYY HH:mm')}</span>` : '-';
                }
            },
            { 
                data: null,
                orderable: false,
                render: function(data, type, row) {
                    const isDeleted = !!row.deletedAt || row.isActive === false;
                    const previewBtn = `
                        <button class="btn btn-sm btn-icon btn-outline-primary preview-template-btn" 
                                data-id="${row.id}" 
                                title="${getLocalizedText('previewEmailTemplate')}" data-bs-toggle="tooltip" data-bs-placement="top">
                            <i class="ti ti-eye"></i>
                        </button>`;
                    const editBtn = `
                        <button class="btn btn-sm btn-icon btn-outline-warning edit-template-btn" 
                                data-id="${row.id}" 
                                title="${getLocalizedText('edit')}" data-bs-toggle="tooltip" data-bs-placement="top">
                            <i class="ti ti-edit"></i>
                        </button>`;
                    const deleteBtn = `
                        <button class="btn btn-sm btn-icon btn-outline-danger delete-template-btn" 
                                data-id="${row.id}" data-name="${row.name}" data-language="${row.language}"
                                title="${getLocalizedText('deactivate')}" data-bs-toggle="tooltip" data-bs-placement="top">
                            <i class="ti ti-trash"></i>
                        </button>`;
                    const restoreBtn = `
                        <button class="btn btn-sm btn-icon btn-outline-success restore-template-btn" 
                                data-id="${row.id}" data-name="${row.name}" data-language="${row.language}" data-deletedat="${row.deletedAt || ''}"
                                title="${getLocalizedText('restore')}" data-bs-toggle="tooltip" data-bs-placement="top">
                            <i class="ti ti-refresh"></i>
                        </button>`;
                    return `<div class="d-flex gap-2">${previewBtn}${editBtn}${isDeleted ? restoreBtn : deleteBtn}</div>`;
                }
            }
        ],
        order: [[0, 'asc'], [1, 'asc']],
        pageLength: 10,
        responsive: true,
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
             '<"row"<"col-sm-12"tr>>' +
             '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
        language: getDataTablesLanguage(),
        drawCallback: function() {
            if (window.bootstrap && document.querySelectorAll) {
                const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                tooltipTriggerList.forEach(function (el) {
                    if (!el._tooltip) {
                        el._tooltip = new bootstrap.Tooltip(el);
                    }
                });
            }
        }
    });
}

function bindEventHandlers() {
    $('#add-template-btn').on('click', function() {
        openAddTemplateModal();
    });

    $('#upload-html-btn').on('click', function() {
        $('#uploadHtmlModal').modal('show');
    });

    $('#emailTemplateForm').on('submit', function(e) {
        e.preventDefault();
        saveEmailTemplate();
    });

    $('#uploadHtmlForm').on('submit', function(e) {
        e.preventDefault();
        uploadHtmlFile();
    });

    $('.datatables-email-templates').on('click', '.preview-template-btn', function() {
        const templateId = $(this).data('id');
        previewEmailTemplate(templateId);
    });

    $('.datatables-email-templates').on('click', '.edit-template-btn', function() {
        const templateId = $(this).data('id');
        editEmailTemplate(templateId);
    });

    $('.datatables-email-templates').on('click', '.delete-template-btn', function() {
        const templateId = $(this).data('id');
        const name = $(this).data('name');
        const language = $(this).data('language');
        $('#delete-template-id').val(templateId);
        $('#delete-template-name').text(name);
        $('#delete-template-language').text(getLanguageDisplayName(language));
        const modal = new bootstrap.Modal(document.getElementById('deleteTemplateModal'));
        modal.show();
    });

    $('.datatables-email-templates').on('click', '.restore-template-btn', function() {
        const templateId = $(this).data('id');
        const name = $(this).data('name');
        const language = $(this).data('language');
        const deletedAt = $(this).data('deletedat');
        $('#restore-template-id').val(templateId);
        $('#restore-template-name').text(name);
        $('#restore-template-language').text(getLanguageDisplayName(language));
        $('#restore-template-deletedat').text(deletedAt ? moment(deletedAt).format('DD/MM/YYYY HH:mm') : '-');
        const modal = new bootstrap.Modal(document.getElementById('restoreTemplateModal'));
        modal.show();
    });

    $('#confirmDeleteTemplate').on('click', async function() {
        const templateId = $('#delete-template-id').val();
        try {
            const response = await fetch(`/api/Email/${templateId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('authToken')}`
                }
            });
            if (!response.ok) {
                throw new Error('Failed to delete template');
            }
            showSuccessNotification(getLocalizedText('templateDeactivatedSuccessfully'));
            $('#deleteTemplateModal').modal('hide');
            emailTemplatesTable.ajax.reload();
        } catch (err) {
            console.error('Delete error:', err);
            showErrorNotification(getLocalizedText('errorDeactivatingTemplate'));
        }
    });

    $('#confirmRestoreTemplate').on('click', async function() {
        const templateId = $('#restore-template-id').val();
        try {
            const response = await fetch(`/api/Email/${templateId}/restore`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('authToken')}`
                }
            });
            if (!response.ok) {
                throw new Error('Failed to restore template');
            }
            showSuccessNotification(getLocalizedText('templateRestoredSuccessfully'));
            $('#restoreTemplateModal').modal('hide');
            emailTemplatesTable.ajax.reload();
        } catch (err) {
            console.error('Restore error:', err);
            showErrorNotification(getLocalizedText('errorRestoringTemplate'));
        }
    });

    $('.datatables-email-templates').on('click', '.preview-template-btn', function() {
        const templateId = $(this).data('id');
        previewEmailTemplate(templateId);
    });
}

function openAddTemplateModal() {
    isEditMode = false;
    currentTemplateId = null;
    
    $('#emailTemplateForm')[0].reset();
    $('#templateIsActive').prop('checked', true);
    
    $('#emailTemplateModalTitle').text(getLocalizedText('addEmailTemplate'));
    $('#saveTemplateBtn').text(getLocalizedText('save'));
    
    $('#emailTemplateModal').modal('show');
}

async function editEmailTemplate(templateId) {
    try {
        const response = await fetch(`/api/Email/${templateId}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch template');
        }

        const template = await response.json();
        
        $('#templateName').val(template.name);
        $('#templateLanguage').val(template.language);
        $('#templateSubject').val(template.subject);
        $('#templateDescription').val(template.description);
        $('#templateHtmlContent').val(template.body);
        $('#templateIsActive').prop('checked', template.isActive);
        
        isEditMode = true;
        currentTemplateId = templateId;
        
        $('#emailTemplateModalTitle').text(getLocalizedText('editEmailTemplate'));
        $('#saveTemplateBtn').text(getLocalizedText('update'));
        
        $('#emailTemplateModal').modal('show');
        
    } catch (error) {
        console.error('Error fetching template:', error);
        showErrorNotification(getLocalizedText('errorLoadingTemplate'));
    }
}

async function saveEmailTemplate() {
    try {
        const formData = {
            name: $('#templateName').val().trim(),
            language: $('#templateLanguage').val(),
            subject: $('#templateSubject').val().trim(),
            description: $('#templateDescription').val().trim(),
            body: $('#templateHtmlContent').val().trim(),
            isActive: $('#templateIsActive').is(':checked')
        };

        if (!formData.name || !formData.subject || !formData.body) {
            showErrorNotification(getLocalizedText('pleaseFillRequiredFields'));
            return;
        }

        const url = isEditMode ? 
            `/api/Email/${currentTemplateId}` : 
            '/api/Email';
        
        const method = isEditMode ? 'PUT' : 'POST';

        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Failed to save template');
        }

        const successMessage = isEditMode ? 
            getLocalizedText('templateUpdatedSuccessfully') : 
            getLocalizedText('templateCreatedSuccessfully');
        showSuccessNotification(successMessage);

        $('#emailTemplateModal').modal('hide');
        emailTemplatesTable.ajax.reload();

    } catch (error) {
        console.error('Error saving template:', error);
        showErrorNotification(error.message || getLocalizedText('errorSavingTemplate'));
    }
}

async function deleteEmailTemplate(templateId) {
    if (!confirm(getLocalizedText('confirmDeleteTemplate'))) {
        return;
    }

    try {
        const response = await fetch(`/api/Email/${templateId}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to delete template');
        }

        showSuccessNotification(getLocalizedText('templateDeactivatedSuccessfully'));
        emailTemplatesTable.ajax.reload();

    } catch (error) {
        console.error('Error deleting template:', error);
        showErrorNotification(getLocalizedText('errorDeactivatingTemplate'));
    }
}

async function previewEmailTemplate(templateId) {
    try {
        const response = await fetch(`/api/Email/${templateId}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            }
        });

        if (!response.ok) {
            throw new Error('Failed to fetch template');
        }

        const template = await response.json();
        
        $('#previewTemplateName').text(template.name);
        $('#previewTemplateLanguage').text(getLanguageDisplayName(template.language));
        $('#previewTemplateSubject').text(template.subject);
        $('#previewTemplateDescription').text(template.description || '-');
        
        $('#previewTemplateHtml').html(template.body);
        
        const variables = extractTemplateVariables(template.body);
        displayTemplateVariables(variables);
        
        $('#previewTemplateModal').modal('show');
        
    } catch (error) {
        console.error('Error fetching template:', error);
        showErrorNotification(getLocalizedText('errorLoadingTemplate'));
    }
}

async function uploadHtmlFile() {
    const fileInput = $('#htmlFile')[0];
    const file = fileInput.files[0];

    if (!file) {
        showErrorNotification(getLocalizedText('pleaseSelectHtmlFile'));
        return;
    }

    if (!file.name.toLowerCase().endsWith('.html') && !file.name.toLowerCase().endsWith('.htm')) {
        showErrorNotification(getLocalizedText('pleaseSelectValidHtmlFile'));
        return;
    }

    try {
        const fileContent = await readFileContent(file);
        
        const analysis = analyzeHtmlFile(file, fileContent);
        
        fillTemplateFormFromAnalysis(analysis);
        
        $('#uploadHtmlModal').modal('hide');
        $('#emailTemplateModal').modal('show');
        
        $('#uploadHtmlForm')[0].reset();
        
    } catch (error) {
        console.error('Error processing HTML file:', error);
        showErrorNotification(getLocalizedText('errorProcessingHtmlFile'));
    }
}

function readFileContent(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = function(e) {
            resolve(e.target.result);
        };
        reader.onerror = function(e) {
            reject(new Error('Failed to read file'));
        };
        reader.readAsText(file);
    });
}

function analyzeHtmlFile(file, content) {
    const analysis = {
        name: '',
        subject: '',
        description: '',
        htmlContent: content,
        language: 'en'
    };

    analysis.name = file.name.replace(/\.(html|htm)$/i, '').toLowerCase();

    const titleMatch = content.match(/<title[^>]*>([^<]+)<\/title>/i);
    if (titleMatch) {
        analysis.subject = titleMatch[1].trim();
    }

    const descMatch = content.match(/<meta[^>]*name=["']description["'][^>]*content=["']([^"']+)["']/i);
    if (descMatch) {
        analysis.description = descMatch[1].trim();
    }

    const langMatch = content.match(/lang=["']([a-z]{2})["']/i);
    if (langMatch) {
        const lang = langMatch[1].toLowerCase();
        if (['en', 'vi', 'ja'].includes(lang)) {
            analysis.language = lang;
        }
    }

    if (content.match(/[àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ]/i)) {
        analysis.language = 'vi';
    }

    return analysis;
}

function fillTemplateFormFromAnalysis(analysis) {
    $('#templateName').val(analysis.name);
    $('#templateLanguage').val(analysis.language);
    $('#templateSubject').val(analysis.subject);
    $('#templateDescription').val(analysis.description);
    $('#templateHtmlContent').val(analysis.htmlContent);
    $('#templateIsActive').prop('checked', true);
    
    isEditMode = false;
    currentTemplateId = null;
    
    $('#emailTemplateModalTitle').text(getLocalizedText('addEmailTemplate'));
    $('#saveTemplateBtn').text(getLocalizedText('save'));
}

function extractTemplateVariables(htmlContent) {
    const variableRegex = /\{\{([^}]+)\}\}/g;
    const variables = [];
    let match;

    while ((match = variableRegex.exec(htmlContent)) !== null) {
        const variableName = match[1].trim();
        if (!variables.includes(variableName)) {
            variables.push(variableName);
        }
    }

    return variables;
}

function displayTemplateVariables(variables) {
    const container = $('#previewTemplateVariables');
    
    if (variables.length === 0) {
        container.html('<p class="mb-0">' + getLocalizedText('noVariablesDetected') + '.</p>');
        return;
    }

    let html = '<p class="mb-2"><strong>' + getLocalizedText('availableVariables') + ':</strong></p><div class="d-flex flex-wrap gap-1">';
    variables.forEach(variable => {
        html += `<span class="badge bg-secondary">${variable}</span>`;
    });
    html += '</div>';
    
    container.html(html);
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

function getLanguageDisplayName(languageCode) {
    const languageMap = {
        'en': 'English',
        'vi': 'Tiếng Việt',
        'ja': '日本語'
    };
    return languageMap[languageCode] || languageCode;
}

function getLocalizedText(key) {
    if (window.i18next && window.i18next.t) {
        const translated = window.i18next.t(key);
        if (translated !== key) {
            return translated;
        }
    }
    
    const fallbackTexts = {
        'addEmailTemplate': 'Add Email Template',
        'editEmailTemplate': 'Edit Email Template',
        'save': 'Save',
        'update': 'Update',
        'cancel': 'Cancel',
        'close': 'Close',
        'previewEmailTemplate': 'Preview Email Template',
        'templateInfo': 'Template Information',
        'templateVariables': 'Template Variables',
        'htmlPreview': 'HTML Preview',
        'emailTemplateName': 'Name',
        'emailTemplateLanguage': 'Language',
        'emailTemplateSubject': 'Subject',
        'emailTemplateDescription': 'Description',
        'emailTemplateHtmlContent': 'HTML Content',
        'emailTemplateHtmlContentHelp': 'Use {{VariableName}} for dynamic content. Example: {{Username}}, {{VerifyLink}}',
        'emailTemplateActive': 'Active',
        'deactivateTemplateConfirmTitle': 'Deactivate Template Confirmation',
        'deactivateTemplateConfirmDesc': 'You are about to deactivate this email template (it will be set to inactive):',
        'deactivate': 'Deactivate',
        'uploadHtmlFile': 'Upload HTML File',
        'selectHtmlFile': 'Select HTML File',
        'htmlFileHelp': 'Upload an HTML file. The system will analyze and extract template information.',
        'htmlFileAnalysisInfo': 'The system will analyze your HTML file and attempt to extract:',
        'htmlFileAnalysisItem1': 'Template name from filename or content',
        'htmlFileAnalysisItem2': 'Subject from title tag or meta tags',
        'htmlFileAnalysisItem3': 'HTML content structure',
        'htmlFileAnalysisItem4': 'Dynamic variables ({{VariableName}})',
        'uploadAndAnalyze': 'Upload & Analyze',
        'pleaseFillRequiredFields': 'Please fill in all required fields',
        'pleaseSelectHtmlFile': 'Please select an HTML file',
        'pleaseSelectValidHtmlFile': 'Please select a valid HTML file',
        'templateCreatedSuccessfully': 'Email template created successfully',
        'templateUpdatedSuccessfully': 'Email template updated successfully',
        'templateDeletedSuccessfully': 'Email template deleted successfully',
        'templateDeactivatedSuccessfully': 'Email template deactivated successfully',
        'templateRestoredSuccessfully': 'Email template restored successfully',
        'confirmDeleteTemplate': 'Are you sure you want to delete this email template?',
        'errorLoadingTemplate': 'Error loading template',
        'errorSavingTemplate': 'Error saving template',
        'errorDeletingTemplate': 'Error deleting template',
        'errorDeactivatingTemplate': 'Error deactivating template',
        'errorRestoringTemplate': 'Error restoring template',
        'errorProcessingHtmlFile': 'Error processing HTML file'
    };

    return fallbackTexts[key] || key;
}

window.loadEmailTemplatesTable = function() {
    if (emailTemplatesTable) {
        emailTemplatesTable.ajax.reload();
    } else {
        initializeEmailTemplates();
    }
};
