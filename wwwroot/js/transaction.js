/*
 * Transaction JavaScript for Cüzdan Uygulaması (Wallet Application)
 * Contains all functionality specific to transaction creation and editing
 */

// ========================================
// Transaction Type Management
// ========================================

function selectTransactionType(type) {
    const options = document.querySelectorAll('.type-option');
    const typeInput = document.getElementById('Type');
    
    if (!options.length || !typeInput) return;
    
    // Remove selected class from all options
    options.forEach(option => option.classList.remove('selected'));
    
    // Set the selected type
    if (type === 0) { // Income
        options[0].classList.add('selected');
        typeInput.value = 0;
    } else { // Expense
        options[1].classList.add('selected');
        typeInput.value = 1;
    }
    
    // Trigger category update after setting the type
    if (window.updateCategoryOptionsFromType) {
        window.updateCategoryOptionsFromType();
    }
    
    // Add visual feedback
    const selectedOption = type === 0 ? options[0] : options[1];
    if (selectedOption) {
        selectedOption.style.transform = 'scale(1.02)';
        setTimeout(() => {
            selectedOption.style.transform = '';
        }, 150);
    }
}

// ========================================
// Recurring Transaction Management
// ========================================

function toggleRecurringSection() {
    const checkbox = document.querySelector('input[name="IsRecurring"]');
    const section = document.getElementById('recurringSection');
    
    if (!checkbox || !section) return;
    
    if (checkbox.checked) {
        section.classList.add('show');
        // Focus on recurrence type select
        setTimeout(() => {
            const recurrenceSelect = section.querySelector('select[name="RecurrenceType"]');
            if (recurrenceSelect) {
                recurrenceSelect.focus();
            }
        }, 300);
    } else {
        section.classList.remove('show');
        // Reset recurrence type
        const recurrenceSelect = document.querySelector('select[name="RecurrenceType"]');
        if (recurrenceSelect) {
            recurrenceSelect.value = '';
        }
    }
}

// ========================================
// Date and Time Management
// ========================================

function setDefaultTransactionDate() {
    const transactionDateInput = document.querySelector('input[name="TransactionDate"]');
    if (transactionDateInput && !transactionDateInput.value) {
        // Set default transaction date to current date and time
        const now = new Date();
        const offsetDate = new Date(now.getTime() - (now.getTimezoneOffset() * 60000));
        transactionDateInput.value = offsetDate.toISOString().slice(0, 16);
    }
}

// ========================================
// Form Focus Management
// ========================================

function setInitialFocus() {
    const descriptionInput = document.querySelector('input[name="Description"]');
    if (descriptionInput) {
        // Small delay to ensure the page is fully rendered
        setTimeout(() => {
            descriptionInput.focus();
        }, 100);
    }
}

// ========================================
// Category Filtering (Dynamic)
// ========================================

function initializeCategoryFiltering() {
    const categorySelect = document.getElementById('CategoryId');
    const typeInput = document.getElementById('Type');
    
    if (!categorySelect || !typeInput) return;
    
    // Store original options
    if (!window.originalCategoryOptions) {
        window.originalCategoryOptions = Array.from(categorySelect.options).map(option => ({
            value: option.value,
            text: option.text,
            selected: option.selected
        }));
    }
    
    // Set up the update function
    window.updateCategoryOptionsFromType = function() {
        const selectedType = parseInt(typeInput.value);
        
        // For now, show all categories regardless of type
        // This can be enhanced later to filter categories based on transaction type
        if (window.originalCategoryOptions) {
            categorySelect.innerHTML = '';
            window.originalCategoryOptions.forEach(optionData => {
                const option = document.createElement('option');
                option.value = optionData.value;
                option.textContent = optionData.text;
                option.selected = optionData.selected;
                categorySelect.appendChild(option);
            });
        }
    };
    
    // Initial call
    window.updateCategoryOptionsFromType();
}

// ========================================
// Form Validation Enhancements
// ========================================

function enhanceFormValidation() {
    const form = document.querySelector('form');
    if (!form) return;
    
    // Add real-time validation
    const requiredInputs = form.querySelectorAll('input[required], select[required]');
    
    requiredInputs.forEach(input => {
        input.addEventListener('blur', function() {
            validateField(this);
        });
        
        input.addEventListener('input', function() {
            if (this.classList.contains('is-invalid')) {
                validateField(this);
            }
        });
    });
    
    // Form submission validation
    form.addEventListener('submit', function(e) {
        let isValid = true;
        
        requiredInputs.forEach(input => {
            if (!validateField(input)) {
                isValid = false;
            }
        });
        
        // Validate transaction type selection
        const typeInput = document.getElementById('Type');
        if (typeInput && (typeInput.value === '' || typeInput.value === null)) {
            showFieldError(typeInput, 'Lütfen bir işlem türü seçin');
            isValid = false;
        }
        
        if (!isValid) {
            e.preventDefault();
            // Focus on first invalid field
            const firstInvalidField = form.querySelector('.is-invalid');
            if (firstInvalidField) {
                firstInvalidField.focus();
            }
        }
    });
}

function validateField(field) {
    const value = field.value.trim();
    let isValid = true;
    
    // Remove existing validation classes
    field.classList.remove('is-valid', 'is-invalid');
    hideFieldError(field);
    
    // Check required fields
    if (field.hasAttribute('required') && !value) {
        showFieldError(field, 'Bu alan zorunludur');
        isValid = false;
    }
    
    // Validate amount field
    if (field.name === 'Amount' && value) {
        const amount = parseFloat(value);
        if (isNaN(amount) || amount <= 0) {
            showFieldError(field, 'Geçerli bir tutar girin');
            isValid = false;
        }
    }
    
    // Validate description length
    if (field.name === 'Description' && value && value.length > 200) {
        showFieldError(field, 'Açıklama 200 karakterden fazla olamaz');
        isValid = false;
    }
    
    // Add visual feedback
    if (isValid && value) {
        field.classList.add('is-valid');
    } else if (!isValid) {
        field.classList.add('is-invalid');
    }
    
    return isValid;
}

function showFieldError(field, message) {
    hideFieldError(field);
    
    const errorElement = document.createElement('div');
    errorElement.className = 'text-danger field-error';
    errorElement.textContent = message;
    
    const parent = field.closest('.input-group') || field.parentNode;
    parent.appendChild(errorElement);
}

function hideFieldError(field) {
    const parent = field.closest('.input-group') || field.parentNode;
    const existingError = parent.querySelector('.field-error');
    if (existingError) {
        existingError.remove();
    }
}

// ========================================
// Number Formatting
// ========================================

function formatAmountInput() {
    const amountInput = document.querySelector('input[name="Amount"]');
    if (!amountInput) return;
    
    amountInput.addEventListener('input', function(e) {
        let value = e.target.value;
        
        // Remove non-numeric characters except decimal point
        value = value.replace(/[^0-9.,]/g, '');
        
        // Replace comma with period for decimal
        value = value.replace(',', '.');
        
        // Ensure only one decimal point
        const parts = value.split('.');
        if (parts.length > 2) {
            value = parts[0] + '.' + parts.slice(1).join('');
        }
        
        // Limit decimal places to 2
        if (parts[1] && parts[1].length > 2) {
            value = parts[0] + '.' + parts[1].substring(0, 2);
        }
        
        e.target.value = value;
    });
}

// ========================================
// Loading States
// ========================================

function showLoadingState(element) {
    if (element) {
        element.classList.add('loading');
        element.disabled = true;
    }
}

function hideLoadingState(element) {
    if (element) {
        element.classList.remove('loading');
        element.disabled = false;
    }
}

// ========================================
// Alert Auto-Hide
// ========================================

function initializeAlertAutoHide() {
    const alerts = document.querySelectorAll('.alert');
    
    alerts.forEach(alert => {
        // Only auto-hide success and info alerts, keep error alerts visible
        if (!alert.classList.contains('alert-danger')) {
            setTimeout(() => {
                alert.style.transition = 'opacity 0.5s ease';
                alert.style.opacity = '0';
                setTimeout(() => {
                    if (alert.parentNode) {
                        alert.remove();
                    }
                }, 500);
            }, 5000);
        }
    });
}

// ========================================
// Keyboard Shortcuts
// ========================================

function initializeKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + S to save
        if ((e.ctrlKey || e.metaKey) && e.key === 's') {
            e.preventDefault();
            const submitButton = document.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.click();
            }
        }
        
        // Esc to cancel
        if (e.key === 'Escape') {
            const cancelButton = document.querySelector('a[href*="Index"]');
            if (cancelButton) {
                cancelButton.click();
            }
        }
        
        // Tab navigation for transaction type
        if (e.key === 'Tab' && e.target.classList.contains('type-option')) {
            e.preventDefault();
            const options = document.querySelectorAll('.type-option');
            const currentIndex = Array.from(options).indexOf(e.target);
            const nextIndex = e.shiftKey ? 
                (currentIndex - 1 + options.length) % options.length :
                (currentIndex + 1) % options.length;
            
            options[nextIndex].focus();
        }
    });
}

// ========================================
// DOM Content Loaded Event
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    // Initialize all functionality
    setDefaultTransactionDate();
    setInitialFocus();
    initializeCategoryFiltering();
    enhanceFormValidation();
    formatAmountInput();
    initializeAlertAutoHide();
    initializeKeyboardShortcuts();
    
    // Handle create vs edit page differences
    const isEditPage = document.querySelector('input[name="Id"]') !== null;
    
    if (isEditPage) {
        // Edit page: Set transaction type based on model value
        const typeInput = document.getElementById('Type');
        if (typeInput && typeInput.value !== '') {
            selectTransactionType(parseInt(typeInput.value));
        }
        
        // Show/hide recurring section based on current value
        const recurringCheckbox = document.querySelector('input[name="IsRecurring"]');
        if (recurringCheckbox && recurringCheckbox.checked) {
            const recurringSection = document.getElementById('recurringSection');
            if (recurringSection) {
                recurringSection.classList.add('show');
            }
        }
    } else {
        // Create page: Default to expense type
        selectTransactionType(1);
    }
    
    // Add click event listeners for transaction type options
    const typeOptions = document.querySelectorAll('.type-option');
    typeOptions.forEach((option, index) => {
        option.addEventListener('click', () => {
            selectTransactionType(index);
        });
        
        // Add keyboard support
        option.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                selectTransactionType(index);
            }
        });
        
        // Make focusable
        option.setAttribute('tabindex', '0');
    });
    
    // Add change listener for recurring checkbox
    const recurringCheckbox = document.querySelector('input[name="IsRecurring"]');
    if (recurringCheckbox) {
        recurringCheckbox.addEventListener('change', toggleRecurringSection);
    }
});

// ========================================
// Global Functions (for inline event handlers)
// ========================================

// Make functions globally available for inline event handlers
window.selectTransactionType = selectTransactionType;
window.toggleRecurringSection = toggleRecurringSection;