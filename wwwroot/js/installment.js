/*
 * Installment JavaScript for Cüzdan Uygulaması (Wallet Application)
 * Contains all functionality specific to installment creation and management
 */

// ========================================
// Smart Interest Rate Management System
// ========================================

class InterestRateManager {
    constructor() {
        this.currentMarketRate = null;
        this.isUsingMarketRate = false;
        this.currentCategoryData = null;
    }

    setRate(rate, isMarketRate = false, categoryData = null) {
        const input = document.getElementById('InterestRateInput');
        if (input) {
            input.value = rate.toFixed(2);
            this.isUsingMarketRate = isMarketRate;
            this.currentCategoryData = categoryData;

            this.updateUI();
            if (typeof calculateInstallment === 'function') {
                calculateInstallment();
            }
        }
    }

    updateUI() {
        const marketControls = document.getElementById('marketRateControls');
        const manualControls = document.getElementById('manualRateControls');
        const rateIndicator = document.getElementById('rateSourceIndicator');
        const marketRateInfo = document.getElementById('marketRateInfo');

        if (!marketControls || !manualControls || !rateIndicator) return;

        if (this.isUsingMarketRate) {
            marketControls.style.display = 'block';
            manualControls.style.display = 'none';
            rateIndicator.style.display = 'inline';

            if (this.currentCategoryData && marketRateInfo) {
                marketRateInfo.style.display = 'block';
                const marketRateText = document.getElementById('marketRateText');
                if (marketRateText) {
                    marketRateText.innerHTML = `
                        <strong>${this.currentCategoryData.categoryName}</strong> için piyasa oranı: <strong>%${this.currentCategoryData.rate}</strong>
                        <br><small class="text-muted">${this.currentCategoryData.explanation}</small>
                    `;
                }
            }
        } else {
            marketControls.style.display = 'none';
            manualControls.style.display = 'block';
            rateIndicator.style.display = 'none';
            if (marketRateInfo) marketRateInfo.style.display = 'none';
        }

        // Hide other indicators
        const rateComparisonInfo = document.getElementById('rateComparisonInfo');
        const manualOverrideNotice = document.getElementById('manualOverrideNotice');
        if (rateComparisonInfo) rateComparisonInfo.style.display = 'none';
        if (manualOverrideNotice) manualOverrideNotice.style.display = 'none';
    }

    reset() {
        this.currentMarketRate = null;
        this.isUsingMarketRate = false;
        this.currentCategoryData = null;
        
        const elements = ['marketRateControls', 'manualRateControls', 'marketRateInfo', 'rateSourceIndicator'];
        elements.forEach(id => {
            const element = document.getElementById(id);
            if (element) element.style.display = 'none';
        });
    }
}

// Initialize the rate manager
const rateManager = new InterestRateManager();

// ========================================
// Category Change Handler
// ========================================

async function onCategoryChange() {
    const categorySelect = document.getElementById('CategoryId');
    if (!categorySelect) return;
    
    const categoryId = parseInt(categorySelect.value);

    // Reset rate manager
    rateManager.reset();

    if (!categoryId) {
        // No category selected - show default rate
        rateManager.setRate(0, false);
        return;
    }

    try {
        // Show loading
        const rateIndicator = document.getElementById('rateSourceIndicator');
        if (rateIndicator) {
            rateIndicator.textContent = 'Yükleniyor...';
            rateIndicator.className = 'badge bg-secondary ms-2';
            rateIndicator.style.display = 'inline';
        }

        // Fetch market rate
        const response = await fetch(`/api/InterestRateApi/category/${categoryId}`);
        const result = await response.json();

        if (result.success && result.data.isInstallmentCategory) {
            rateManager.currentMarketRate = result.data.rate;
            rateManager.setRate(result.data.rate, true, result.data);

            // Reset loading indicator
            if (rateIndicator) {
                rateIndicator.textContent = 'Piyasa Oranı';
                rateIndicator.className = 'badge bg-info ms-2';
            }
        } else {
            throw new Error(result.error || 'Invalid category');
        }
    } catch (error) {
        console.error('Error fetching rate:', error);
        rateManager.setRate(1.99, false); // Fallback
        rateManager.reset(); // Hide indicators
    }
}

// ========================================
// Rate Control Functions
// ========================================

function enableManualRate() {
    rateManager.isUsingMarketRate = false;
    rateManager.updateUI();
}

function useMarketRate() {
    if (rateManager.currentMarketRate !== null) {
        rateManager.setRate(rateManager.currentMarketRate, true, rateManager.currentCategoryData);
    }
}

function setManualRate(rate) {
    rateManager.setRate(rate, false);
}

function onInterestRateChange() {
    // If user manually types a rate, switch to manual mode
    if (rateManager.isUsingMarketRate) {
        enableManualRate();
    }
    if (typeof calculateInstallment === 'function') {
        calculateInstallment();
    }
}

// ========================================
// Installment Calculation
// ========================================

function calculateInstallment() {
    const totalAmountInput = document.querySelector('input[name="TotalAmount"]');
    const installmentCountSelect = document.querySelector('select[name="TotalInstallments"]');
    const interestRateInput = document.querySelector('input[name="InterestRate"]');
    const firstPaymentDateInput = document.querySelector('input[name="FirstPaymentDate"]');
    
    if (!totalAmountInput || !installmentCountSelect || !interestRateInput) return;

    const totalAmount = parseFloat(totalAmountInput.value) || 0;
    const installmentCount = parseInt(installmentCountSelect.value) || 0;
    const interestRate = parseFloat(interestRateInput.value) || 0;
    const firstPaymentDate = firstPaymentDateInput ? firstPaymentDateInput.value : '';

    const previewDiv = document.getElementById('calculationPreview');
    const interestRow = document.getElementById('interestRow');
    const totalCostRow = document.getElementById('totalCostRow');

    if (!previewDiv) return;

    if (totalAmount > 0 && installmentCount > 0) {
        let monthlyPayment, totalInterest, totalCost;

        if (interestRate === 0) {
            // Simple installment without interest
            monthlyPayment = totalAmount / installmentCount;
            totalInterest = 0;
            totalCost = totalAmount;
            if (interestRow) interestRow.style.display = 'none';
            if (totalCostRow) totalCostRow.style.display = 'none';
        } else {
            // Compound interest calculation - same formula as backend
            const monthlyRate = interestRate / 100;
            const denominator = Math.pow(1 + monthlyRate, installmentCount) - 1;

            if (denominator === 0) {
                monthlyPayment = totalAmount / installmentCount;
            } else {
                monthlyPayment = totalAmount * (monthlyRate * Math.pow(1 + monthlyRate, installmentCount)) / denominator;
            }

            totalCost = monthlyPayment * installmentCount;
            totalInterest = totalCost - totalAmount;

            if (interestRow) interestRow.style.display = 'flex';
            if (totalCostRow) totalCostRow.style.display = 'flex';
        }

        // Calculate last payment date
        let lastPaymentDate = '';
        if (firstPaymentDate) {
            const firstDate = new Date(firstPaymentDate);
            const lastDate = new Date(firstDate);
            lastDate.setMonth(lastDate.getMonth() + installmentCount - 1);
            lastPaymentDate = lastDate.toLocaleDateString('tr-TR', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        }

        // Update preview with proper Turkish number formatting
        updatePreviewElement('previewPrincipal', '₺' + totalAmount.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        updatePreviewElement('previewRate', '%' + interestRate.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' (aylık)');
        updatePreviewElement('previewCount', installmentCount + ' ay');
        updatePreviewElement('previewMonthly', '₺' + monthlyPayment.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));

        if (interestRate > 0) {
            updatePreviewElement('previewInterest', '₺' + totalInterest.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
            updatePreviewElement('previewTotalCost', '₺' + totalCost.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
        }

        updatePreviewElement('previewEndDate', lastPaymentDate || 'Hesaplanıyor...');

        previewDiv.style.display = 'block';
    } else {
        previewDiv.style.display = 'none';
    }
}

// ========================================
// Utility Functions
// ========================================

function updatePreviewElement(id, value) {
    const element = document.getElementById(id);
    if (element) {
        element.textContent = value;
    }
}

// ========================================
// Date Utilities
// ========================================

function setDefaultFirstPaymentDate() {
    const firstPaymentInput = document.querySelector('input[name="FirstPaymentDate"]');
    if (firstPaymentInput && !firstPaymentInput.value) {
        // Set default first payment date to next month
        const nextMonth = new Date();
        nextMonth.setMonth(nextMonth.getMonth() + 1);
        nextMonth.setDate(1);
        firstPaymentInput.value = nextMonth.toISOString().split('T')[0];
    }
}

// ========================================
// Alert Auto-Hide
// ========================================

function initializeAlertAutoHide() {
    // Auto-hide success/error messages after 5 seconds
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(function(alert) {
            if (alert.classList.contains('alert-success') || alert.classList.contains('alert-danger')) {
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 500);
            }
        });
    }, 5000);
}

// ========================================
// Focus Management
// ========================================

function setInitialFocus() {
    const descriptionInput = document.querySelector('input[name="Description"]');
    if (descriptionInput) {
        descriptionInput.focus();
    }
}

// ========================================
// DOM Content Loaded Event
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    // Initialize default values and focus
    setDefaultFirstPaymentDate();
    setInitialFocus();
    
    // Initialize rate manager
    rateManager.reset();
    
    // Initialize alert auto-hide (for details page)
    initializeAlertAutoHide();
    
    // Add event listeners for form inputs
    const totalAmountInput = document.querySelector('input[name="TotalAmount"]');
    const installmentCountSelect = document.querySelector('select[name="TotalInstallments"]');
    const interestRateInput = document.querySelector('input[name="InterestRate"]');
    const categorySelect = document.getElementById('CategoryId');
    
    if (totalAmountInput) {
        totalAmountInput.addEventListener('change', calculateInstallment);
        totalAmountInput.addEventListener('input', calculateInstallment);
    }
    
    if (installmentCountSelect) {
        installmentCountSelect.addEventListener('change', calculateInstallment);
    }
    
    if (interestRateInput) {
        interestRateInput.addEventListener('change', onInterestRateChange);
        interestRateInput.addEventListener('input', onInterestRateChange);
    }
    
    if (categorySelect) {
        categorySelect.addEventListener('change', onCategoryChange);
    }
});

// ========================================
// Global Functions (for inline event handlers)
// ========================================

// Make functions globally available for inline event handlers
window.calculateInstallment = calculateInstallment;
window.onCategoryChange = onCategoryChange;
window.onInterestRateChange = onInterestRateChange;
window.enableManualRate = enableManualRate;
window.useMarketRate = useMarketRate;
window.setManualRate = setManualRate;