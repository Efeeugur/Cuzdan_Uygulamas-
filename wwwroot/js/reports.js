/*
 * Reports JavaScript for Cüzdan Uygulaması (Wallet Application)
 * Contains all interactive functionality specific to the Reports page
 */

// ========================================
// Global Variables and Chart Instances
// ========================================
let trendChart;
// Note: categoryChart and accountChart are commented out for future implementation

// ========================================
// Initialization and Setup Functions
// ========================================
document.addEventListener('DOMContentLoaded', function() {
    initializeCharts();
    setupAjaxReporting();
    initializeTransactionTypeDropdown();
    setupDateChangeListeners();
    
    // Check if we have report data on page load and enable PDF export
    if (typeof hasReportData !== 'undefined' && hasReportData) {
        enablePdfExport();
    }
});

/**
 * Sets up AJAX-based real-time reporting functionality
 */
function setupAjaxReporting() {
    const form = document.querySelector('form');
    const realTimeCheckbox = document.getElementById('realTimeUpdates');
    const generateBtn = document.getElementById('generateReportBtn');
    
    // Handle real-time updates
    if (realTimeCheckbox) {
        const formInputs = form.querySelectorAll('input, select');
        
        formInputs.forEach(input => {
            input.addEventListener('change', function() {
                if (realTimeCheckbox.checked) {
                    debounce(generateReportAjax, 500)();
                }
            });
        });
    }
    
    // Handle form submission via AJAX when real-time is enabled
    form.addEventListener('submit', function(e) {
        if (realTimeCheckbox && realTimeCheckbox.checked) {
            e.preventDefault();
            generateReportAjax();
        }
    });
}

/**
 * Sets up event listeners for date input changes
 */
function setupDateChangeListeners() {
    const startDateInput = document.getElementById('startDate');
    const endDateInput = document.getElementById('endDate');
    
    if (startDateInput && endDateInput) {
        [startDateInput, endDateInput].forEach(input => {
            input.addEventListener('change', function() {
                // Remove active state from preset buttons when manually changing dates
                document.querySelectorAll('.btn-group .btn-outline-primary.active')
                        .forEach(btn => btn.classList.remove('active'));
            });
        });
    }
}

// ========================================
// AJAX Reporting Functions
// ========================================

/**
 * Generates report via AJAX call
 */
function generateReportAjax() {
    const form = document.querySelector('form');
    const formData = new FormData(form);
    const loadingSpinner = document.getElementById('loadingSpinner');
    const reportResults = document.getElementById('reportResults');
    const generateBtn = document.getElementById('generateReportBtn');
    
    // Show loading state
    showLoadingState(loadingSpinner, reportResults, generateBtn);
    
    // Convert FormData to JSON
    const data = {};
    for (let [key, value] of formData.entries()) {
        data[key] = value;
    }
    
    fetch('/Reports/GenerateReportAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(result => {
        if (result.success) {
            updateReportDisplay(result.data);
        } else {
            showError(result.error || 'Rapor oluşturulurken bir hata oluştu.');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showError('Bağlantı hatası oluştu. Lütfen tekrar deneyin.');
    })
    .finally(() => {
        hideLoadingState(loadingSpinner, reportResults, generateBtn);
    });
}

/**
 * Shows loading state for report generation
 */
function showLoadingState(loadingSpinner, reportResults, generateBtn) {
    loadingSpinner.style.display = 'block';
    reportResults.style.display = 'none';
    generateBtn.disabled = true;
    generateBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Oluşturuluyor...';
}

/**
 * Hides loading state and restores normal UI
 */
function hideLoadingState(loadingSpinner, reportResults, generateBtn) {
    loadingSpinner.style.display = 'none';
    reportResults.style.display = 'block';
    generateBtn.disabled = false;
    generateBtn.innerHTML = '<i class="fas fa-search me-2"></i>Rapor Oluştur';
}

/**
 * Updates the entire report display with new data
 */
function updateReportDisplay(data) {
    updateSummaryCards(data.summary);
    updateCharts(data);
    updateTransactionTable(data.transactions, data.dateRange);
    enablePdfExport();
    showSuccess(`Rapor başarıyla oluşturuldu! ${data.totalTransactions} işlem bulundu.`);
}

/**
 * Updates summary cards with new financial data
 */
function updateSummaryCards(summary) {
    const summaryContainer = document.querySelector('.summary-cards');
    if (!summaryContainer) return;
    
    summaryContainer.innerHTML = `
        <div class="summary-card income">
            <div class="summary-value text-success">₺${summary.totalIncome.toLocaleString('tr-TR', {minimumFractionDigits: 2})}</div>
            <div class="summary-label">Toplam Gelir</div>
        </div>
        <div class="summary-card expense">
            <div class="summary-value text-danger">₺${summary.totalExpense.toLocaleString('tr-TR', {minimumFractionDigits: 2})}</div>
            <div class="summary-label">Toplam Gider</div>
        </div>
        <div class="summary-card net">
            <div class="summary-value ${summary.netAmount >= 0 ? 'text-success' : 'text-danger'}">
                ₺${summary.netAmount.toLocaleString('tr-TR', {minimumFractionDigits: 2})}
            </div>
            <div class="summary-label">Net Durum</div>
        </div>
        <div class="summary-card count">
            <div class="summary-value text-info">${data.totalTransactions}</div>
            <div class="summary-label">İşlem Sayısı</div>
        </div>
    `;
}

/**
 * Updates transaction table with new data
 */
function updateTransactionTable(transactions, dateRange) {
    console.log('Updating transaction table with', transactions.length, 'transactions');
    // Implementation depends on specific table structure needed
    // This can be expanded based on requirements
}

// ========================================
// Chart Management Functions
// ========================================

/**
 * Initializes all charts with current data
 */
function initializeCharts() {
    // Future: Category and Account charts can be uncommented when needed
    /*
    if (categoryData && categoryData.length > 0) {
        createCategoryChart();
    }
    
    if (accountData && accountData.length > 0) {
        createAccountChart();
    }
    */
    
    if (window.trendData && window.trendData.length > 0) {
        createTrendChart();
    }
}

/**
 * Updates all charts with new data
 */
function updateCharts(data) {
    // Destroy existing charts
    if (trendChart) trendChart.destroy();
    
    // Update global data
    window.trendData = data.monthlyTrends || [];
    
    // Recreate charts
    initializeCharts();
}

/**
 * Creates monthly trend chart showing income/expense/net over time
 */
function createTrendChart() {
    const ctx = document.getElementById('trendChart');
    if (!ctx) return;
    
    trendChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: window.trendData.map(t => t.monthYear),
            datasets: [{
                label: 'Gelir',
                data: window.trendData.map(t => t.incomeTotal),
                borderColor: 'rgba(40, 167, 69, 1)',
                backgroundColor: 'rgba(40, 167, 69, 0.1)',
                fill: false,
                tension: 0.4,
                borderWidth: 3
            }, {
                label: 'Gider',
                data: window.trendData.map(t => Math.abs(t.expenseTotal)),
                borderColor: 'rgba(220, 53, 69, 1)',
                backgroundColor: 'rgba(220, 53, 69, 0.1)',
                fill: false,
                tension: 0.4,
                borderWidth: 3
            }, {
                label: 'Net',
                data: window.trendData.map(t => t.netAmount),
                borderColor: 'rgba(0, 122, 255, 1)',
                backgroundColor: 'rgba(0, 122, 255, 0.1)',
                fill: false,
                tension: 0.4,
                borderWidth: 2,
                borderDash: [5, 5]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return new Intl.NumberFormat('tr-TR', {
                                style: 'currency',
                                currency: 'TRY',
                                minimumFractionDigits: 0
                            }).format(value);
                        }
                    }
                }
            },
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return `${context.dataset.label}: ${new Intl.NumberFormat('tr-TR', {
                                style: 'currency',
                                currency: 'TRY'
                            }).format(context.parsed.y)}`;
                        }
                    }
                }
            }
        }
    });
}

// ========================================
// Filter Management Functions
// ========================================

/**
 * Resets all form filters to default state
 */
function resetFilters() {
    // Clear all form inputs
    const form = document.querySelector('form[action="/Reports/GenerateReport"]');
    if (form) {
        form.reset();
    }
    
    // Reset custom dropdown and hidden fields
    const transactionTypeSelect = document.getElementById('transactionTypeSelect');
    const hiddenTransactionType = document.getElementById('hiddenTransactionType');
    const hiddenOnlyInstallments = document.getElementById('hiddenOnlyInstallments');
    
    if (transactionTypeSelect) transactionTypeSelect.value = '';
    if (hiddenTransactionType) hiddenTransactionType.value = '';
    if (hiddenOnlyInstallments) hiddenOnlyInstallments.value = 'false';
    
    // Clear date fields specifically
    clearAllDates();
    
    // Remove active classes from date preset buttons
    document.querySelectorAll('.btn-group .btn').forEach(btn => btn.classList.remove('active'));
    
    // Clear any other specific fields that might not reset properly
    const searchField = document.querySelector('input[name="SearchTerm"]');
    if (searchField) searchField.value = '';
    
    console.log('All filters have been reset');
}

/**
 * Handles transaction type dropdown changes
 */
function handleTransactionTypeChange() {
    const select = document.getElementById('transactionTypeSelect');
    const hiddenTransactionType = document.getElementById('hiddenTransactionType');
    const hiddenOnlyInstallments = document.getElementById('hiddenOnlyInstallments');
    
    if (select.value === '2') {
        // Installments selected
        hiddenTransactionType.value = '';
        hiddenOnlyInstallments.value = 'true';
    } else {
        // Regular transaction type or all
        hiddenTransactionType.value = select.value;
        hiddenOnlyInstallments.value = 'false';
    }
    
    // Trigger real-time update if enabled
    const realTimeCheckbox = document.getElementById('realTimeUpdates');
    if (realTimeCheckbox && realTimeCheckbox.checked) {
        debounce(generateReportAjax, 500)();
    }
}

/**
 * Initializes transaction type dropdown based on current values
 */
function initializeTransactionTypeDropdown() {
    const select = document.getElementById('transactionTypeSelect');
    const hiddenOnlyInstallments = document.getElementById('hiddenOnlyInstallments');
    const hiddenTransactionType = document.getElementById('hiddenTransactionType');
    
    if (!select || !hiddenOnlyInstallments || !hiddenTransactionType) return;
    
    // Check current values from server
    const onlyInstallments = hiddenOnlyInstallments.value === 'true';
    const transactionType = hiddenTransactionType.value;
    
    if (onlyInstallments) {
        select.value = '2';
    } else {
        select.value = transactionType || '';
    }
}

// ========================================
// Date Management Functions
// ========================================

/**
 * Clears start date input with visual feedback
 */
function clearStartDate() {
    const startInput = document.getElementById('startDate');
    if (startInput) {
        startInput.value = '';
        // Add visual feedback
        startInput.classList.add('border-success');
        setTimeout(() => startInput.classList.remove('border-success'), 500);
    }
    // Remove active state from preset buttons
    document.querySelectorAll('.btn-group .btn-outline-primary, .btn-group .btn.active')
            .forEach(btn => btn.classList.remove('active'));
}

/**
 * Clears end date input with visual feedback
 */
function clearEndDate() {
    const endInput = document.getElementById('endDate');
    if (endInput) {
        endInput.value = '';
        // Add visual feedback
        endInput.classList.add('border-success');
        setTimeout(() => endInput.classList.remove('border-success'), 500);
    }
    // Remove active state from preset buttons
    document.querySelectorAll('.btn-group .btn-outline-primary, .btn-group .btn.active')
            .forEach(btn => btn.classList.remove('active'));
}

/**
 * Clears both start and end dates
 */
function clearAllDates() {
    clearStartDate();
    clearEndDate();
}

/**
 * Sets date range based on preset selection
 */
function setDateRange(preset) {
    const today = new Date();
    const startInput = document.getElementById('startDate');
    const endInput = document.getElementById('endDate');
    
    // Remove active class from all preset buttons
    document.querySelectorAll('.btn-group .btn-outline-primary').forEach(btn => btn.classList.remove('active'));
    
    let startDate, endDate;
    
    switch(preset) {
        case 'today':
            startDate = endDate = today;
            break;
        case 'yesterday':
            startDate = endDate = new Date(today.getTime() - 24 * 60 * 60 * 1000);
            break;
        case 'thisWeek':
            const startOfWeek = new Date(today);
            startOfWeek.setDate(today.getDate() - today.getDay() + 1);
            startDate = startOfWeek;
            endDate = today;
            break;
        case 'lastWeek':
            const lastWeekEnd = new Date(today);
            lastWeekEnd.setDate(today.getDate() - today.getDay());
            const lastWeekStart = new Date(lastWeekEnd);
            lastWeekStart.setDate(lastWeekEnd.getDate() - 6);
            startDate = lastWeekStart;
            endDate = lastWeekEnd;
            break;
        case 'thisMonth':
            startDate = new Date(today.getFullYear(), today.getMonth(), 1);
            endDate = today;
            break;
        case 'lastMonth':
            startDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            endDate = new Date(today.getFullYear(), today.getMonth(), 0);
            break;
        case 'last3Months':
            startDate = new Date(today.getFullYear(), today.getMonth() - 3, 1);
            endDate = today;
            break;
        case 'last6Months':
            startDate = new Date(today.getFullYear(), today.getMonth() - 6, 1);
            endDate = today;
            break;
        case 'thisYear':
            startDate = new Date(today.getFullYear(), 0, 1);
            endDate = today;
            break;
    }
    
    if (startDate && endDate) {
        startInput.value = startDate.toISOString().split('T')[0];
        endInput.value = endDate.toISOString().split('T')[0];
        
        // Add active class to clicked button
        if (event && event.target) {
            event.target.classList.add('active');
        }
        
        // Trigger real-time update if enabled
        const realTimeCheckbox = document.getElementById('realTimeUpdates');
        if (realTimeCheckbox && realTimeCheckbox.checked) {
            debounce(generateReportAjax, 500)();
        }
    }
}

// ========================================
// PDF Export Functions
// ========================================

/**
 * Opens PDF export modal
 */
function openPdfExportModal() {
    const modal = new bootstrap.Modal(document.getElementById('pdfExportModal'));
    modal.show();
}

/**
 * Enables PDF export button when report data is available
 */
function enablePdfExport() {
    const pdfBtn = document.getElementById('pdfExportBtn');
    if (pdfBtn) {
        pdfBtn.disabled = false;
    }
}

/**
 * Previews PDF in new window
 */
function previewPdf() {
    // TODO: Implement PDF preview functionality
    alert('Önizleme özelliği henüz mevcut değil.');
}

// ========================================
// Notification Functions
// ========================================

/**
 * Shows error message in the report results area
 */
function showError(message) {
    const reportResults = document.getElementById('reportResults');
    reportResults.innerHTML = `
        <div class="report-card" style="animation: slideInUp 0.5s ease-out;">
            <div class="card-body">
                <div class="alert alert-danger alert-dismissible fade show" role="alert">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    <strong>Hata:</strong> ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>
                <div class="text-center mt-3">
                    <button type="button" class="btn btn-primary" onclick="location.reload()">
                        <i class="fas fa-refresh me-2"></i>Sayfayı Yenile
                    </button>
                </div>
            </div>
        </div>
    `;
}

/**
 * Shows success toast notification
 */
function showSuccess(message) {
    const toast = document.createElement('div');
    toast.className = 'toast align-items-center text-bg-success border-0 position-fixed top-0 end-0 m-3';
    toast.style.zIndex = '9999';
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="fas fa-check-circle me-2"></i>
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    
    document.body.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast, {
        autohide: true,
        delay: 3000
    });
    bsToast.show();
    
    // Remove toast after hiding
    toast.addEventListener('hidden.bs.toast', () => {
        document.body.removeChild(toast);
    });
}

// ========================================
// Utility Functions
// ========================================

/**
 * Debounces function calls to limit API requests
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Generates color for chart elements based on seed
 */
function generateColor(seed) {
    const colors = [
        '#007bff', '#28a745', '#dc3545', '#ffc107', '#6f42c1',
        '#fd7e14', '#20c997', '#6610f2', '#e83e8c', '#17a2b8'
    ];
    return colors[seed % colors.length];
}

// ========================================
// Global Functions for External Access
// ========================================

// Export functions that need to be accessible from HTML
window.resetFilters = resetFilters;
window.handleTransactionTypeChange = handleTransactionTypeChange;
window.clearStartDate = clearStartDate;
window.clearEndDate = clearEndDate;
window.clearAllDates = clearAllDates;
window.setDateRange = setDateRange;
window.openPdfExportModal = openPdfExportModal;
window.previewPdf = previewPdf;