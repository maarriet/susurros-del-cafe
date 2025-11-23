document.addEventListener('DOMContentLoaded', function () {
    const quantity250Input = document.getElementById('Quantity250g');
    const quantity500Input = document.getElementById('Quantity500g');
    const provinceSelect = document.getElementById('Province');

    // Price constants
    const PRICE_250G = 2500;
    const PRICE_500G = 4500;
    const SHIPPING_COST = 3200;

    function updateOrderSummary() {
        const qty250 = parseInt(quantity250Input.value) || 0;
        const qty500 = parseInt(quantity500Input.value) || 0;
        const province = provinceSelect.value;

        // Calculate subtotal
        const subtotal = (qty250 * PRICE_250G) + (qty500 * PRICE_500G);

        // Calculate shipping
        const isAlajuela = province.toLowerCase() === 'alajuela';
        const shipping = isAlajuela ? 0 : SHIPPING_COST;

        // Calculate total
        const total = subtotal + shipping;

        // Update display
        updateSummaryDisplay(qty250, qty500, subtotal, shipping, total, isAlajuela);

        // Show/hide summary and individual items
        const summaryDiv = document.getElementById('order-summary');
        const item250 = document.getElementById('item-250');
        const item500 = document.getElementById('item-500');

        if (qty250 > 0 || qty500 > 0) {
            summaryDiv.style.display = 'block';
            summaryDiv.style.animation = 'fadeInUp 0.5s ease-out';
        } else {
            summaryDiv.style.display = 'none';
        }

        // Show/hide individual items
        item250.style.display = qty250 > 0 ? 'block' : 'none';
        item500.style.display = qty500 > 0 ? 'block' : 'none';
    }

    function updateSummaryDisplay(qty250, qty500, subtotal, shipping, total, isAlajuela) {
        // Update quantities
        document.getElementById('summary-qty-250').textContent = qty250;
        document.getElementById('summary-qty-500').textContent = qty500;

        // Update prices
        document.getElementById('summary-price-250').textContent =
            formatCurrency(qty250 * PRICE_250G);
        document.getElementById('summary-price-500').textContent =
            formatCurrency(qty500 * PRICE_500G);

        // Update totals
        document.getElementById('summary-subtotal').textContent = formatCurrency(subtotal);
        document.getElementById('summary-shipping').textContent = shipping === 0 ? 'Gratis' : formatCurrency(shipping);
        document.getElementById('summary-total').textContent = formatCurrency(total);

        // Update shipping info
        const shippingInfo = document.getElementById('shipping-info');
        if (isAlajuela) {
            shippingInfo.textContent = '🚚 ¡Envío gratis a Alajuela!';
            shippingInfo.className = 'small text-success mb-3';
        } else if (province) {
            shippingInfo.textContent = '📦 Envío por Correos de Costa Rica';
            shippingInfo.className = 'small text-info mb-3';
        } else {
            shippingInfo.textContent = '';
        }
    }

    function formatCurrency(amount) {
        return '₡' + amount.toLocaleString('es-CR');
    }

    // Event listeners
    if (quantity250Input) quantity250Input.addEventListener('input', updateOrderSummary);
    if (quantity500Input) quantity500Input.addEventListener('input', updateOrderSummary);
    if (provinceSelect) provinceSelect.addEventListener('change', updateOrderSummary);

    // Initial calculation
    updateOrderSummary();
});