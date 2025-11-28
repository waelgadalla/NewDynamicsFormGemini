/*!
 * DynamicForms JavaScript Library
 * Version: 1.0.0
 * Provides client-side functionality for dynamic form handling
 */

(function(window, document, $) {
    'use strict';

    // DynamicForms namespace
    const DynamicForms = {
        version: '1.0.0',
        config: {
            autoSave: {
                enabled: false,
                interval: 30000,
                url: null
            },
            validation: {
                enabled: true,
                showErrorSummary: true,
                highlightErrors: true
            },
            language: 'EN',
            formId: null
        },
        
        // Initialize the library
        initialize: function(options) {
            this.config = Object.assign(this.config, options || {});
            this.setupEventHandlers();
            this.initializeComponents();
            
            if (this.config.autoSave.enabled) {
                this.setupAutoSave();
            }
            
            console.log('DynamicForms initialized with config:', this.config);
        },
        
        // Setup global event handlers
        setupEventHandlers: function() {
            const form = this.getForm();
            if (!form) return;
            
            // Form submission handler
            form.addEventListener('submit', this.handleFormSubmit.bind(this));
            
            // Field change handlers
            form.addEventListener('change', this.handleFieldChange.bind(this));
            form.addEventListener('input', this.handleFieldInput.bind(this));
            
            // Modal handlers
            document.addEventListener('click', this.handleModalActions.bind(this));
            
            // File upload handlers
            form.addEventListener('change', this.handleFileUpload.bind(this));
        },
        
        // Initialize form components
        initializeComponents: function() {
            this.initializeConditionalFields();
            this.initializeValidation();
            this.initializeFileUploads();
            this.initializeSpeciesAutoComplete();
            this.initializeTableFields();
        },
        
        // Get the main form element
        getForm: function() {
            if (this.config.formId) {
                return document.getElementById(this.config.formId);
            }
            return document.querySelector('form[data-dynamic-form]');
        },
        
        // Handle form submission
        handleFormSubmit: function(e) {
            const form = e.target;
            
            // Validate if enabled
            if (this.config.validation.enabled) {
                const isValid = this.validateForm(form);
                if (!isValid) {
                    e.preventDefault();
                    this.showValidationSummary();
                    return false;
                }
            }
            
            // Show loading state
            this.showFormLoading(true);
            
            // Allow form to submit normally or handle with AJAX
            console.log('Form submitted');
        },
        
        // Handle field changes
        handleFieldChange: function(e) {
            const field = e.target;
            const fieldContainer = field.closest('[data-field-id]');
            
            if (fieldContainer) {
                const fieldId = fieldContainer.getAttribute('data-field-id');
                
                // Clear previous validation errors
                this.clearFieldValidation(fieldContainer);
                
                // Validate field
                this.validateField(field);
                
                // Handle conditional logic
                this.handleConditionalLogic(fieldId, field.value);
                
                // Auto-save if enabled
                if (this.config.autoSave.enabled) {
                    this.saveFieldData(fieldId, field.value);
                }
            }
        },
        
        // Handle field input (for real-time validation)
        handleFieldInput: function(e) {
            const field = e.target;
            
            // Debounce input validation
            clearTimeout(field.validationTimeout);
            field.validationTimeout = setTimeout(() => {
                this.validateField(field);
            }, 500);
        },
        
        // Handle modal actions
        handleModalActions: function(e) {
            const target = e.target.closest('button');
            if (!target) return;
            
            const action = target.getAttribute('data-action');
            const modalId = target.getAttribute('data-modal-id');
            const recordId = target.getAttribute('data-record-id');
            
            switch (action) {
                case 'add-modal-record':
                    this.openModalForAdd(modalId);
                    break;
                case 'edit-modal-record':
                    this.openModalForEdit(modalId, recordId);
                    break;
                case 'delete-modal-record':
                    this.deleteModalRecord(modalId, recordId);
                    break;
            }
        },
        
        // Handle file uploads
        handleFileUpload: function(e) {
            const input = e.target;
            if (input.type !== 'file') return;
            
            const files = input.files;
            if (files.length === 0) return;
            
            const fieldContainer = input.closest('[data-field-id]');
            const fieldId = fieldContainer?.getAttribute('data-field-id');
            
            // Validate file(s)
            for (let file of files) {
                if (!this.validateFile(file, input)) {
                    input.value = '';
                    return;
                }
            }
            
            // Show upload progress
            this.showFileUploadProgress(fieldContainer, true);
            
            // Upload file(s)
            this.uploadFiles(fieldId, files);
        },
        
        // Initialize conditional field logic
        initializeConditionalFields: function() {
            const conditionalFields = document.querySelectorAll('[data-conditional-rules]');
            
            conditionalFields.forEach(field => {
                const rules = JSON.parse(field.getAttribute('data-conditional-rules'));
                this.applyConditionalRules(field, rules);
            });
        },
        
        // Handle conditional logic
        handleConditionalLogic: function(changedFieldId, value) {
            const dependentFields = document.querySelectorAll(`[data-depends-on="${changedFieldId}"]`);
            
            dependentFields.forEach(field => {
                const condition = field.getAttribute('data-condition');
                const expectedValue = field.getAttribute('data-expected-value');
                
                const shouldShow = this.evaluateCondition(value, condition, expectedValue);
                this.toggleFieldVisibility(field, shouldShow);
            });
        },
        
        // Evaluate conditional expression
        evaluateCondition: function(actualValue, condition, expectedValue) {
            switch (condition) {
                case 'equals':
                    return actualValue === expectedValue;
                case 'not-equals':
                    return actualValue !== expectedValue;
                case 'contains':
                    return actualValue.includes(expectedValue);
                case 'greater-than':
                    return parseFloat(actualValue) > parseFloat(expectedValue);
                case 'less-than':
                    return parseFloat(actualValue) < parseFloat(expectedValue);
                case 'is-empty':
                    return !actualValue || actualValue.trim() === '';
                case 'is-not-empty':
                    return actualValue && actualValue.trim() !== '';
                default:
                    return true;
            }
        },
        
        // Toggle field visibility
        toggleFieldVisibility: function(field, visible) {
            const container = field.closest('[data-field-id]') || field;
            
            if (visible) {
                container.style.display = '';
                container.classList.remove('d-none');
            } else {
                container.style.display = 'none';
                container.classList.add('d-none');
                
                // Clear field value when hidden
                const inputs = container.querySelectorAll('input, select, textarea');
                inputs.forEach(input => {
                    if (input.type === 'checkbox' || input.type === 'radio') {
                        input.checked = false;
                    } else {
                        input.value = '';
                    }
                });
            }
        },
        
        // Initialize form validation
        initializeValidation: function() {
            if (!this.config.validation.enabled) return;
            
            const form = this.getForm();
            if (form) {
                form.setAttribute('novalidate', '');
            }
        },
        
        // Validate entire form
        validateForm: function(form) {
            let isValid = true;
            const fields = form.querySelectorAll('input, select, textarea');
            
            fields.forEach(field => {
                if (!this.validateField(field)) {
                    isValid = false;
                }
            });
            
            return isValid;
        },
        
        // Validate individual field
        validateField: function(field) {
            const fieldContainer = field.closest('[data-field-id]');
            if (!fieldContainer) return true;
            
            let isValid = true;
            const errors = [];
            
            // Required validation
            if (field.hasAttribute('required') && !this.hasValue(field)) {
                errors.push(this.getValidationMessage('required', field));
                isValid = false;
            }
            
            // Type-specific validation
            if (this.hasValue(field)) {
                switch (field.type) {
                    case 'email':
                        if (!this.isValidEmail(field.value)) {
                            errors.push(this.getValidationMessage('email', field));
                            isValid = false;
                        }
                        break;
                    case 'url':
                        if (!this.isValidUrl(field.value)) {
                            errors.push(this.getValidationMessage('url', field));
                            isValid = false;
                        }
                        break;
                    case 'number':
                        if (!this.isValidNumber(field.value)) {
                            errors.push(this.getValidationMessage('number', field));
                            isValid = false;
                        }
                        break;
                }
            }
            
            // Length validation
            const maxLength = field.getAttribute('maxlength');
            if (maxLength && field.value.length > parseInt(maxLength)) {
                errors.push(this.getValidationMessage('maxlength', field, maxLength));
                isValid = false;
            }
            
            // Show/hide validation errors
            this.showFieldValidation(fieldContainer, errors);
            
            return isValid;
        },
        
        // Check if field has value
        hasValue: function(field) {
            if (field.type === 'checkbox' || field.type === 'radio') {
                return field.checked;
            }
            return field.value && field.value.trim() !== '';
        },
        
        // Validation helper functions
        isValidEmail: function(email) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(email);
        },
        
        isValidUrl: function(url) {
            try {
                new URL(url);
                return true;
            } catch {
                return false;
            }
        },
        
        isValidNumber: function(value) {
            return !isNaN(value) && !isNaN(parseFloat(value));
        },
        
        // Get validation message
        getValidationMessage: function(type, field, param) {
            const messages = {
                EN: {
                    required: 'This field is required.',
                    email: 'Please enter a valid email address.',
                    url: 'Please enter a valid URL.',
                    number: 'Please enter a valid number.',
                    maxlength: `Maximum ${param} characters allowed.`
                },
                FR: {
                    required: 'Ce champ est obligatoire.',
                    email: 'Veuillez entrer une adresse e-mail valide.',
                    url: 'Veuillez entrer une URL valide.',
                    number: 'Veuillez entrer un nombre valide.',
                    maxlength: `Maximum ${param} caractères autorisés.`
                }
            };
            
            return messages[this.config.language][type] || messages.EN[type];
        },
        
        // Show field validation errors
        showFieldValidation: function(fieldContainer, errors) {
            this.clearFieldValidation(fieldContainer);
            
            if (errors.length > 0) {
                // Add error class to field
                const field = fieldContainer.querySelector('input, select, textarea');
                if (field) {
                    field.classList.add('is-invalid');
                }
                
                // Show error message
                const errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback d-block';
                errorDiv.textContent = errors[0];
                fieldContainer.appendChild(errorDiv);
                
                // Highlight field container if configured
                if (this.config.validation.highlightErrors) {
                    fieldContainer.classList.add('has-error');
                }
            }
        },
        
        // Clear field validation
        clearFieldValidation: function(fieldContainer) {
            const field = fieldContainer.querySelector('input, select, textarea');
            if (field) {
                field.classList.remove('is-invalid');
            }
            
            const errorDiv = fieldContainer.querySelector('.invalid-feedback');
            if (errorDiv) {
                errorDiv.remove();
            }
            
            fieldContainer.classList.remove('has-error');
        },
        
        // Show validation summary
        showValidationSummary: function() {
            if (!this.config.validation.showErrorSummary) return;
            
            const form = this.getForm();
            const invalidFields = form.querySelectorAll('.is-invalid');
            
            if (invalidFields.length === 0) return;
            
            // Remove existing summary
            const existingSummary = form.querySelector('.validation-summary-dynamic');
            if (existingSummary) {
                existingSummary.remove();
            }
            
            // Create new summary
            const summary = document.createElement('div');
            summary.className = 'alert alert-danger validation-summary-dynamic';
            summary.innerHTML = `
                <h5>${this.config.language === 'FR' ? 'Erreurs de validation :' : 'Validation errors:'}</h5>
                <ul class="mb-0">
                    ${Array.from(invalidFields).map(field => {
                        const container = field.closest('[data-field-id]');
                        const fieldName = container?.querySelector('label')?.textContent || 'Field';
                        const errorMsg = container?.querySelector('.invalid-feedback')?.textContent || 'Invalid value';
                        return `<li>${fieldName}: ${errorMsg}</li>`;
                    }).join('')}
                </ul>
            `;
            
            // Insert at top of form
            form.insertBefore(summary, form.firstChild);
            
            // Scroll to summary
            summary.scrollIntoView({ behavior: 'smooth', block: 'center' });
        },
        
        // Auto-save functionality
        setupAutoSave: function() {
            if (!this.config.autoSave.url) {
                console.warn('Auto-save enabled but no URL configured');
                return;
            }
            
            setInterval(() => {
                this.autoSave();
            }, this.config.autoSave.interval);
        },
        
        // Perform auto-save
        autoSave: function() {
            const form = this.getForm();
            if (!form) return;
            
            const formData = new FormData(form);
            
            return fetch(this.config.autoSave.url, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    this.showAutoSaveStatus('saved');
                } else {
                    this.showAutoSaveStatus('error');
                }
            })
            .catch(error => {
                console.error('Auto-save failed:', error);
                this.showAutoSaveStatus('error');
            });
        },
        
        // Show auto-save status
        showAutoSaveStatus: function(status) {
            let message, className;
            
            switch (status) {
                case 'saved':
                    message = this.config.language === 'FR' ? 'Sauvegardé automatiquement' : 'Auto-saved';
                    className = 'alert-success';
                    break;
                case 'error':
                    message = this.config.language === 'FR' ? 'Erreur de sauvegarde' : 'Save error';
                    className = 'alert-danger';
                    break;
                default:
                    return;
            }
            
            // Show temporary status message
            const statusEl = document.createElement('div');
            statusEl.className = `alert ${className} auto-save-status`;
            statusEl.textContent = message;
            statusEl.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 200px;';
            
            document.body.appendChild(statusEl);
            
            setTimeout(() => {
                statusEl.remove();
            }, 3000);
        },
        
        // Save draft functionality
        saveDraft: function() {
            const form = this.getForm();
            if (!form) return Promise.reject('No form found');
            
            const formData = new FormData(form);
            formData.append('Action', 'SaveDraft');
            
            return fetch(this.config.autoSave.url || window.location.href, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .then(response => response.json());
        },
        
        // Show loading state
        showFormLoading: function(loading) {
            const form = this.getForm();
            if (!form) return;
            
            const submitBtns = form.querySelectorAll('button[type="submit"]');
            
            submitBtns.forEach(btn => {
                if (loading) {
                    btn.disabled = true;
                    btn.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status"></span>${btn.textContent}`;
                } else {
                    btn.disabled = false;
                    btn.innerHTML = btn.textContent.replace(/^.*?<\/span>/, '');
                }
            });
        },
        
        // File upload functions
        initializeFileUploads: function() {
            const fileInputs = document.querySelectorAll('input[type="file"]');
            fileInputs.forEach(input => {
                this.setupFileUploadPreview(input);
            });
        },
        
        validateFile: function(file, input) {
            const maxSize = input.getAttribute('data-max-size') || 10485760; // 10MB default
            const allowedTypes = input.getAttribute('data-allowed-types');
            
            if (file.size > maxSize) {
                alert(this.config.language === 'FR' 
                    ? `Le fichier est trop volumineux. Taille maximale: ${this.formatFileSize(maxSize)}`
                    : `File is too large. Maximum size: ${this.formatFileSize(maxSize)}`);
                return false;
            }
            
            if (allowedTypes) {
                const types = allowedTypes.split(',').map(t => t.trim().toLowerCase());
                const fileType = file.type.toLowerCase();
                const fileExt = '.' + file.name.split('.').pop().toLowerCase();
                
                if (!types.includes(fileType) && !types.includes(fileExt)) {
                    alert(this.config.language === 'FR'
                        ? `Type de fichier non autorisé. Types autorisés: ${allowedTypes}`
                        : `File type not allowed. Allowed types: ${allowedTypes}`);
                    return false;
                }
            }
            
            return true;
        },
        
        formatFileSize: function(bytes) {
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        },
        
        // Initialize other components (simplified)
        initializeSpeciesAutoComplete: function() {
            // Implementation for species autocomplete
        },
        
        initializeTableFields: function() {
            // Implementation for table fields
        }
    };
    
    // Export to global scope
    window.DynamicForms = DynamicForms;
    
    // Auto-initialize if jQuery is available and DOM is ready
    if (typeof $ !== 'undefined') {
        $(document).ready(function() {
            // Auto-initialize if data-auto-init is present
            if (document.querySelector('[data-dynamic-forms-auto-init]')) {
                DynamicForms.initialize();
            }
        });
    }
    
})(window, document, window.jQuery);