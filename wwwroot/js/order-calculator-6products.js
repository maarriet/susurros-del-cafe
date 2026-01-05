// order-calculator-6products.js
document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Order calculator para 6 productos cargado');

    // Productos y precios
    const products = {
        'QuantityMedioMolido250g': { price: 2500, name: 'Tueste Medio Molido 250g' },
        'QuantityMedioMolido500g': { price: 4500, name: 'Tueste Medio Molido 500g' },
        'QuantityOscuroMolido250g': { price: 2500, name: 'Tueste Oscuro Molido 250g' },
        'QuantityOscuroMolido500g': { price: 4500, name: 'Tueste Oscuro Molido 500g' },
        'QuantityMedioGrano250g': { price: 2500, name: 'Tueste Medio Grano 250g' },
        'QuantityMedioGrano500g': { price: 4500, name: 'Tueste Medio Grano 500g' }
    };

    // Agregar event listeners a todos los inputs de cantidad
    Object.keys(products).forEach(inputId => {
        const input = document.getElementById(inputId);
        if (input) {
            input.addEventListener('input', updateOrderSummary);
        }
    });

    // Listener para provincia (para calcular envío)
    const provinceSelect = document.getElementById('Province');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', updateOrderSummary);
    }

    function updateOrderSummary() {
        let subtotal = 0;
        let hasProducts = false;
        const orderSummary = document.getElementById('order-summary');

        // Calcular subtotal y mostrar productos
        Object.keys(products).forEach(inputId => {
            const input = document.getElementById(inputId);
            const quantity = parseInt(input?.value) || 0;

            if (quantity > 0) {
                hasProducts = true;
                const productPrice = products[inputId].price;
                const itemTotal = quantity * productPrice;
                subtotal += itemTotal;

                // Mostrar item en resumen
                showSummaryItem(inputId, quantity, itemTotal, products[inputId].name);
            } else {
                // Ocultar item si cantidad es 0
                hideSummaryItem(inputId);
            }
        });

        // Calcular envío
        const province = document.getElementById('Province')?.value;
        const shippingCost = province === 'Alajuela' ? 0 : 3200;

        // Calcular total
        const total = subtotal + shippingCost;

        // Actualizar resumen
        updateSummaryTotals(subtotal, shippingCost, total, province);

        // Mostrar/ocultar resumen
        if (orderSummary) {
            orderSummary.style.display = hasProducts ? 'block' : 'none';
        }
    }

    function showSummaryItem(inputId, quantity, itemTotal, productName) {
        const itemId = getItemId(inputId);
        const itemElement = document.getElementById(itemId);

        if (itemElement) {
            itemElement.style.display = 'block';

            // Actualizar cantidad y precio
            const qtyElement = document.getElementById(`summary-qty-${getProductKey(inputId)}`);
            const priceElement = document.getElementById(`summary-price-${getProductKey(inputId)}`);

            if (qtyElement) qtyElement.textContent = quantity;
            if (priceElement) priceElement.textContent = `₡${itemTotal.toLocaleString()}`;
        }
    }

    function hideSummaryItem(inputId) {
        const itemId = getItemId(inputId);
        const itemElement = document.getElementById(itemId);

        if (itemElement) {
            itemElement.style.display = 'none';
        }
    }

    function getItemId(inputId) {
        const mapping = {
            'QuantityMedioMolido250g': 'item-medio-molido-250',
            'QuantityMedioMolido500g': 'item-medio-molido-500',
            'QuantityOscuroMolido250g': 'item-oscuro-molido-250',
            'QuantityOscuroMolido500g': 'item-oscuro-molido-500',
            'QuantityMedioGrano250g': 'item-medio-grano-250',
            'QuantityMedioGrano500g': 'item-medio-grano-500'
        };
        return mapping[inputId];
    }

    function getProductKey(inputId) {
        const mapping = {
            'QuantityMedioMolido250g': 'medio-molido-250',
            'QuantityMedioMolido500g': 'medio-molido-500',
            'QuantityOscuroMolido250g': 'oscuro-molido-250',
            'QuantityOscuroMolido500g': 'oscuro-molido-500',
            'QuantityMedioGrano250g': 'medio-grano-250',
            'QuantityMedioGrano500g': 'medio-grano-500'
        };
        return mapping[inputId];
    }

    function updateSummaryTotals(subtotal, shippingCost, total, province) {
        // Actualizar subtotal
        const subtotalElement = document.getElementById('summary-subtotal');
        if (subtotalElement) {
            subtotalElement.textContent = `₡${subtotal.toLocaleString()}`;
        }

        // Actualizar envío
        const shippingElement = document.getElementById('summary-shipping');
        if (shippingElement) {
            shippingElement.textContent = shippingCost === 0 ? '¡Gratis!' : `₡${shippingCost.toLocaleString()}`;
        }

        // Actualizar info de envío
        const shippingInfo = document.getElementById('shipping-info');
        if (shippingInfo) {
            if (shippingCost === 0) {
                shippingInfo.textContent = '✅ Envío gratis a Alajuela';
                shippingInfo.className = 'small text-success mb-3';
            } else {
                shippingInfo.textContent = `📦 Envío a ${province || 'provincia seleccionada'}`;
                shippingInfo.className = 'small text-info mb-3';
            }
        }

        // Actualizar total
        const totalElement = document.getElementById('summary-total');
        if (totalElement) {
            totalElement.textContent = `₡${total.toLocaleString()}`;
        }
    }

    // Inicializar
    updateOrderSummary();
});