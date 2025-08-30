// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ========== Common Functions ==========

// Delete Category Function
function deleteCategory(id) {
    if (typeof Swal === 'undefined') {
        if (confirm('Bu kategoriyi silmek istediğinize emin misiniz?')) {
            submitDeleteForm('/Category/Delete/' + id);
        }
        return;
    }
    
    Swal.fire({
        title: 'Kategori Silinecek',
        text: 'Bu işlem geri alınamaz!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal'
    }).then((result) => {
        if (result.isConfirmed) {
            submitDeleteForm('/Category/Delete/' + id);
        }
    });
}

// Delete Account Function
function deleteAccount(id) {
    if (typeof Swal === 'undefined') {
        if (confirm('Bu hesabı silmek istediğinize emin misiniz?')) {
            submitDeleteForm('/Wallet/DeleteAccount/' + id);
        }
        return;
    }
    
    Swal.fire({
        title: 'Hesap Silinecek',
        text: 'Bu işlem geri alınamaz!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal'
    }).then((result) => {
        if (result.isConfirmed) {
            submitDeleteForm('/Wallet/DeleteAccount/' + id);
        }
    });
}

// Delete Installment Function
function deleteInstallment(id) {
    if (typeof Swal === 'undefined') {
        if (confirm('Bu taksiti silmek istediğinize emin misiniz?')) {
            submitDeleteForm('/Installment/Delete/' + id);
        }
        return;
    }
    
    Swal.fire({
        title: 'Taksit Silinecek',
        text: 'Bu işlem geri alınamaz!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal'
    }).then((result) => {
        if (result.isConfirmed) {
            submitDeleteForm('/Installment/Delete/' + id);
        }
    });
}

// Delete Transaction Function
function deleteTransaction(id) {
    if (typeof Swal === 'undefined') {
        if (confirm('Bu işlemi silmek istediğinize emin misiniz? Bu işlem geri alınamaz ve hesap bakiyesi güncellenecek!')) {
            submitDeleteForm('/Transaction/Delete/' + id);
        }
        return;
    }
    
    Swal.fire({
        title: 'İşlem Silinecek',
        text: 'Bu işlem geri alınamaz ve hesap bakiyesi güncellenecek!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: 'Evet, Sil',
        cancelButtonText: 'İptal'
    }).then((result) => {
        if (result.isConfirmed) {
            submitDeleteForm('/Transaction/Delete/' + id);
        }
    });
}

// Helper function to submit delete forms
function submitDeleteForm(action) {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = action;
    
    // Add anti-forgery token if available
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        const token = document.createElement('input');
        token.type = 'hidden';
        token.name = '__RequestVerificationToken';
        token.value = tokenInput.value;
        form.appendChild(token);
    }
    
    document.body.appendChild(form);
    form.submit();
}

// ========== Success/Error Message Display ==========
// This function should be called when the page loads to display TempData messages

function displayMessages() {
    // Check if SweetAlert is available
    if (typeof Swal === 'undefined') {
        console.warn('SweetAlert2 is not loaded. Messages will not be displayed.');
        return;
    }
    
    // Success messages (this would be populated by server-side code)
    const successMessage = window.tempDataSuccess;
    if (successMessage) {
        Swal.fire({
            icon: 'success',
            title: 'Başarılı!',
            text: successMessage,
            timer: 3000,
            showConfirmButton: false
        });
        window.tempDataSuccess = null; // Clear the message
    }
    
    // Error messages (this would be populated by server-side code)
    const errorMessage = window.tempDataError;
    if (errorMessage) {
        Swal.fire({
            icon: 'error',
            title: 'Hata!',
            text: errorMessage
        });
        window.tempDataError = null; // Clear the message
    }
}

// ========== Document Ready Functions ==========
document.addEventListener('DOMContentLoaded', function() {
    // Display any messages when the page loads
    displayMessages();
    
    // Initialize any other JavaScript functionality here
    initializeFormValidation();
    initializeTooltips();
});

// ========== Form Validation Enhancement ==========
function initializeFormValidation() {
    // Add custom validation styling to forms
    const forms = document.querySelectorAll('form[novalidate]');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
    
    // Add real-time validation feedback
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('blur', function() {
            if (this.checkValidity()) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else {
                this.classList.remove('is-valid');
                this.classList.add('is-invalid');
            }
        });
    });
}

// ========== Tooltip Initialization ==========
function initializeTooltips() {
    // Initialize Bootstrap tooltips if available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

// ========== Utility Functions ==========

// Format currency for display
function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

// Format date for display
function formatDate(date) {
    return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(date));
}

// Debounce function for search inputs
function debounce(func, wait, immediate) {
    var timeout;
    return function() {
        var context = this, args = arguments;
        var later = function() {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
}

// ========== Navigation Enhancement ==========
function initializeNavigation() {
    // Add active class to current nav item
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-link');
    
    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        }
    });
}

// Call navigation initialization when DOM is loaded
document.addEventListener('DOMContentLoaded', initializeNavigation);

// ========== Dynamic Category Filtering ==========
function initializeCategoryFiltering() {
    const transactionTypeSelect = document.getElementById('Type');
    const categorySelect = document.getElementById('CategoryId');
    
    if (!transactionTypeSelect || !categorySelect) {
        return;
    }

    // Cache all categories by type
    let categoriesByType = null;
    
    // Function to load all categories
    async function loadCategoriesByType() {
        if (categoriesByType) {
            return categoriesByType;
        }
        
        try {
            const response = await fetch('/Transaction/GetAllCategoriesByType');
            if (response.ok) {
                categoriesByType = await response.json();
                return categoriesByType;
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
        return null;
    }
    
    // Function to update category options
    async function updateCategoryOptions() {
        const selectedType = transactionTypeSelect.value;
        
        if (!selectedType && selectedType !== '0') {
            // Clear category options if no type selected
            categorySelect.innerHTML = '<option value="">Kategori Seçin</option>';
            return;
        }
        
        const categories = await loadCategoriesByType();
        if (!categories) {
            return;
        }
        
        // Store current selected value
        const currentValue = categorySelect.value;
        
        // Clear current options
        categorySelect.innerHTML = '<option value="">Kategori Seçin</option>';
        
        // Determine which categories to show based on transaction type
        let categoriesToShow = [];
        
        switch(selectedType) {
            case '0': // Income
                categoriesToShow = categories.income || [];
                break;
            case '1': // Expense
                categoriesToShow = categories.expense || [];
                break;
            default:
                // For installments or other types, show expense categories
                categoriesToShow = categories.expense || [];
                break;
        }
        
        // Add filtered options
        categoriesToShow.forEach(category => {
            const option = document.createElement('option');
            option.value = category.value;
            option.textContent = category.text;
            categorySelect.appendChild(option);
        });
        
        // Restore selected value if it still exists in the filtered list
        if (currentValue) {
            const optionExists = Array.from(categorySelect.options).some(opt => opt.value === currentValue);
            if (optionExists) {
                categorySelect.value = currentValue;
            }
        }
    }
    
    // Add event listener to transaction type change
    transactionTypeSelect.addEventListener('change', updateCategoryOptions);
    
    // Expose the update function globally for external calls
    window.updateCategoryOptionsFromType = updateCategoryOptions;
    
    // Initialize categories on page load if type is already selected
    if (transactionTypeSelect.value || transactionTypeSelect.value === '0') {
        updateCategoryOptions();
    }
}

// Initialize installment category filtering
function initializeInstallmentCategoryFiltering() {
    const categorySelect = document.getElementById('CategoryId');
    
    if (!categorySelect) {
        return;
    }
    
    // For installment pages, always show installment categories
    async function loadInstallmentCategories() {
        try {
            const response = await fetch('/Transaction/GetAllCategoriesByType');
            if (response.ok) {
                const categoriesByType = await response.json();
                const installmentCategories = categoriesByType.installment || [];
                
                // Clear current options
                categorySelect.innerHTML = '<option value="">Kategori Seçin</option>';
                
                // Add installment categories
                installmentCategories.forEach(category => {
                    const option = document.createElement('option');
                    option.value = category.value;
                    option.textContent = category.text;
                    categorySelect.appendChild(option);
                });
            }
        } catch (error) {
            console.error('Error loading installment categories:', error);
        }
    }
    
    // Load installment categories on page load
    loadInstallmentCategories();
}

// ========== Export functions for global use ==========
window.deleteCategory = deleteCategory;
window.deleteAccount = deleteAccount;
window.deleteInstallment = deleteInstallment;
window.deleteTransaction = deleteTransaction;
window.formatCurrency = formatCurrency;
window.formatDate = formatDate;
window.debounce = debounce;
window.initializeCategoryFiltering = initializeCategoryFiltering;
window.initializeInstallmentCategoryFiltering = initializeInstallmentCategoryFiltering;