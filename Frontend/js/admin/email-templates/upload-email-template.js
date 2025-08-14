$(document).ready(function() {
    setTimeout(() => {
        initializeUploadPage();
        bindEventHandlers();
    }, 200);
});

function initializeUploadPage() {
    setTimeout(() => {
        setupDragAndDrop();
        setupFormValidation();
    }, 100);
}

function setupDragAndDrop() {
    const uploadArea = $('#uploadArea');
    const fileInput = $('#htmlFile');

    if (uploadArea.length === 0 || fileInput.length === 0) {
        console.error('Upload area or file input not found');
        return;
    }

    uploadArea.off('click').on('click', function(e) {
        if ($(e.target).closest('#browseFileBtn').length) {
            return;
        }
        e.preventDefault();
        e.stopPropagation();
        fileInput.trigger('click');
    });

    fileInput.off('click').on('click', function(e) {
        e.stopPropagation();
    });

    uploadArea.on('dragover', function(e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('drag-over');
    });

    uploadArea.on('dragleave', function(e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag-over');
    });

    uploadArea.on('drop', function(e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('drag-over');
        
        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            handleFileSelection(files[0]);
        }
    });

    fileInput.off('change').on('change', function(e) {
        if (this.files.length > 0) {
            handleFileSelection(this.files[0]);
        }
    });
}

function handleFileSelection(file) {
    if (!file.name.toLowerCase().endsWith('.html') && !file.name.toLowerCase().endsWith('.htm')) {
        showErrorNotification(getLocalizedText('pleaseSelectValidHtmlFile'));
        return;
    }

    if (file.size > 5 * 1024 * 1024) {
        showErrorNotification(getLocalizedText('fileTooLarge'));
        return;
    }

    displayFileInfo(file);

    readAndAnalyzeFile(file);
}

function displayFileInfo(file) {
    $('#selectedFileName').text(file.name);
    $('#fileSize').text(formatFileSize(file.size));
    $('#fileInfoSection').show();
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

async function readAndAnalyzeFile(file) {
    try {
        const content = await readFileContent(file);
        const analysis = analyzeHtmlFile(file, content);
        
        displayAnalysisResults(analysis);
        
        $('#analysisSection').show();
        $('#variablesSection').show();
        $('#saveTemplateBtn').show();
        $('#resetFormBtn').show();
        
        $('#analysisSection')[0].scrollIntoView({ behavior: 'smooth' });
        
    } catch (error) {
        console.error('Error analyzing file:', error);
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
    } else if (content.match(/[あ-んア-ン]/)) {
        analysis.language = 'ja';
    }

    return analysis;
}

function displayAnalysisResults(analysis) {
    $('#templateName').val(analysis.name);
    $('#templateLanguage').val(analysis.language);
    $('#templateSubject').val(analysis.subject);
    $('#templateDescription').val(analysis.description);
    $('#templateHtmlContent').val(analysis.htmlContent);

    const variables = extractTemplateVariables(analysis.htmlContent);
    displayTemplateVariables(variables);
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
    const container = $('#templateVariables');
    
    if (variables.length === 0) {
        container.html(`<span class="badge bg-secondary">${getLocalizedText('noVariablesDetected')}</span>`);
        return;
    }

    let html = '';
    variables.forEach(variable => {
        html += `<span class="badge bg-primary me-1 mb-1">${variable}</span>`;
    });
    
    container.html(html);
}

function setupFormValidation() {
    $(document).off('submit', '#uploadHtmlForm').on('submit', '#uploadHtmlForm', function(e) {
        e.preventDefault();
        
        if (validateForm()) {
            saveEmailTemplate();
        }
    });
}

function validateForm() {
    const name = $('#templateName').val().trim();
    const subject = $('#templateSubject').val().trim();
    const htmlContent = $('#templateHtmlContent').val().trim();

    if (!name) {
        showErrorNotification(getLocalizedText('pleaseEnterTemplateName'));
        $('#templateName').focus();
        return false;
    }

    if (!subject) {
        showErrorNotification(getLocalizedText('pleaseEnterTemplateSubject'));
        $('#templateSubject').focus();
        return false;
    }

    if (!htmlContent) {
        showErrorNotification(getLocalizedText('pleaseEnterHtmlContent'));
        $('#templateHtmlContent').focus();
        return false;
    }

    return true;
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

        const response = await fetch('/api/Email', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('authToken')}`
            },
            body: JSON.stringify(formData)
        });

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || 'Failed to save template');
        }

        showSuccessNotification(getLocalizedText('templateCreatedSuccessfully'));
        
        setTimeout(() => {
            window.location.href = '/admin/email-templates/email-templates.html';
        }, 1500);

    } catch (error) {
        console.error('Error saving template:', error);
        showErrorNotification(error.message || getLocalizedText('errorSavingTemplate'));
    }
}

function bindEventHandlers() {
    // Sử dụng event delegation để tránh xung đột
    $(document).off('click', '#browseFileBtn').on('click', '#browseFileBtn', function(e) {
        e.preventDefault();
        e.stopPropagation();
        $('#htmlFile').trigger('click');
    });

    $(document).off('click', '#resetFormBtn').on('click', '#resetFormBtn', function(e) {
        e.preventDefault();
        resetForm();
    });

    $(document).off('dblclick', '#templateHtmlContent').on('dblclick', '#templateHtmlContent', function(e) {
        e.preventDefault();
        if ($(this).prop('readonly')) {
            $(this).prop('readonly', false);
            $(this).removeClass('form-control-plaintext');
            $(this).addClass('form-control');
        }
    });
}

function resetForm() {
    $('#uploadHtmlForm')[0].reset();
    $('#fileInfoSection').hide();
    $('#analysisSection').hide();
    $('#variablesSection').hide();
    $('#saveTemplateBtn').hide();
    $('#resetFormBtn').hide();
    
    $('#uploadArea').removeClass('drag-over');
}

function getLocalizedText(key) {
    const fallbackTexts = {
        'uploadEmailTemplateTitle': 'Upload Email Template',
        'uploadEmailTemplateDescription': 'Upload an HTML file to create a new email template. The system will automatically analyze the file and extract template information.',
        'dragAndDropFile': 'Drag and drop your HTML file here',
        'orClickToBrowse': 'or click to browse',
        'browseFiles': 'Browse Files',
        'selectedFile': 'Selected File',
        'templateAnalysis': 'Template Analysis',
        'templateNameHelp': 'Name will be extracted from filename or you can modify it',
        'languageHelp': 'Language will be auto-detected from content',
        'subjectHelp': 'Subject will be extracted from title tag or meta tags',
        'descriptionHelp': 'Description will be extracted from meta description tag',
        'htmlContentHelp': 'HTML content from your file. You can edit this content if needed.',
        'detectedVariables': 'Detected Variables',
        'variablesInfo': 'The following variables were detected in your HTML template:',
        'variablesHelp': 'These variables can be used for dynamic content. Example: {{Username}}, {{VerifyLink}}',
        'saveTemplate': 'Save Template',
        'resetForm': 'Reset Form',
        'backToList': 'Back to List',
        'pleaseSelectValidHtmlFile': 'Please select a valid HTML file',
        'fileTooLarge': 'File size is too large. Maximum size is 5MB',
        'errorProcessingHtmlFile': 'Error processing HTML file',
        'pleaseEnterTemplateName': 'Please enter template name',
        'pleaseEnterTemplateSubject': 'Please enter template subject',
        'pleaseEnterHtmlContent': 'Please enter HTML content',
        'templateCreatedSuccessfully': 'Email template created successfully',
        'errorSavingTemplate': 'Error saving template',
        'noVariablesDetected': 'No variables detected'
    };

    if (window.i18next && window.i18next.t) {
        const translated = window.i18next.t(key);
        if (translated !== key) {
            return translated;
        }
    }

    return fallbackTexts[key] || key;
}


