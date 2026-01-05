// order-calculator-6products.js
document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Order calculator para 6 productos cargado');

    // Productos y precios
    const products = {
        'QuantityMedioMolido250g': {
            price: 2500,
            name: 'Tueste Medio Molido 250g',
            summaryId: 'item-medio-molido-250',
            qtyId: 'summary-qty-medio-molido-250',
            priceId: 'summary-price-medio-molido-250'
        },
        'QuantityMedioMolido500g': {
            price: 4500,
            name: 'Tueste Medio Molido 500g',
            summaryId: 'item-medio-molido-500',
            qtyId: 'summary-qty-medio-molido-500',
            priceId: 'summary-price-medio-molido-500'
        },
        'QuantityOscuroMolido250g': {
            price: 2500,
            name: 'Tueste Oscuro Molido 250g',
            summaryId: 'item-oscuro-molido-250',
            qtyId: 'summary-qty-oscuro-molido-250',
            priceId: 'summary-price-oscuro-molido-250'
        },
        'QuantityOscuroMolido500g': {
            price: 4500,
            name: 'Tueste Oscuro Molido 500g',
            summaryId: 'item-oscuro-molido-500',
            qtyId: 'summary-qty-oscuro-molido-500',
            priceId: 'summary-price-oscuro-molido-500'
        },
        'QuantityMedioGrano250g': {
            price: 2500,
            name: 'Tueste Medio Grano 250g',
            summaryId: 'item-medio-grano-250',
            qtyId: 'summary-qty-medio-grano-250',
            priceId: 'summary-price-medio-grano-250'
        },
        'QuantityMedioGrano500g': {
            price: 4500,
            name: 'Tueste Medio Grano 500g',
            summaryId: 'item-medio-grano-500',
            qtyId: 'summary-qty-medio-grano-500',
            priceId: 'summary-price-medio-grano-500'
        }
    };

    // Agregar event listeners a todos los inputs de cantidad
    Object.keys(products).forEach(inputId => {
        const input = document.getElementById(inputId);
        if (input) {
            input.addEventListener('input', updateOrderSummary);
            console.log(`✅ Event listener agregado a ${inputId}`);
        } else {
            console.log(`❌ No se encontró input: ${inputId}`);
        }
    });

    // Listener para provincia (para calcular envío)
    const provinceSelect = document.getElementById('Province');
    if (provinceSelect) {
        provinceSelect.addEventListener('change', updateOrderSummary);
        console.log('✅ Event listener agregado a Province');
    }

    function updateOrderSummary() {
        console.log('🔄 Actualizando resumen del pedido...');

        let subtotal = 0;
        let hasProducts = false;
        const orderSummary = document.getElementById('order-summary');

        // Calcular subtotal y mostrar productos
        Object.keys(products).forEach(inputId => {
            const input = document.getElementById(inputId);
            const quantity = parseInt(input?.value) || 0;
            const product = products[inputId];

            console.log(`📦 ${inputId}: ${quantity} unidades`);

            if (quantity > 0) {
                hasProducts = true;
                const itemTotal = quantity * product.price;
                subtotal += itemTotal;

                // Mostrar item en resumen
                showSummaryItem(product, quantity, itemTotal);
            } else {
                // Ocultar item si cantidad es 0
                hideSummaryItem(product);
            }
        });

        // Calcular envío
        const province = document.getElementById('Province')?.value || '';
        const shippingCost = province === 'Alajuela' ? 0 : 3200;

        // Calcular total
        const total = subtotal + shippingCost;

        console.log(`💰 Subtotal: ₡${subtotal}, Envío: ₡${shippingCost}, Total: ₡${total}`);

        // Actualizar resumen
        updateSummaryTotals(subtotal, shippingCost, total, province);

        // Mostrar/ocultar resumen
        if (orderSummary) {
            orderSummary.style.display = hasProducts ? 'block' : 'none';
            console.log(`📋 Resumen ${hasProducts ? 'mostrado' : 'ocultado'}`);
        }
    }

    function showSummaryItem(product, quantity, itemTotal) {
        const itemElement = document.getElementById(product.summaryId);

        if (itemElement) {
            itemElement.style.display = 'block';

            // Actualizar cantidad y precio
            const qtyElement = document.getElementById(product.qtyId);
            const priceElement = document.getElementById(product.priceId);

            if (qtyElement) qtyElement.textContent = quantity;
            if (priceElement) priceElement.textContent = `₡${itemTotal.toLocaleString()}`;

            console.log(`✅ Mostrado: ${product.name} x${quantity} = ₡${itemTotal.toLocaleString()}`);
        } else {
            console.log(`❌ No se encontró elemento: ${product.summaryId}`);
        }
    }

    function hideSummaryItem(product) {
        const itemElement = document.getElementById(product.summaryId);

        if (itemElement) {
            itemElement.style.display = 'none';
        }
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
            } else if (province) {
                shippingInfo.textContent = `📦 Envío a ${province}`;
                shippingInfo.className = 'small text-info mb-3';
            } else {
                shippingInfo.textContent = '📦 Selecciona provincia para calcular envío';
                shippingInfo.className = 'small text-muted mb-3';
            }
        }

        // Actualizar total
        const totalElement = document.getElementById('summary-total');
        if (totalElement) {
            totalElement.textContent = `₡${total.toLocaleString()}`;
        }
    }

    // Inicializar
    console.log('🎯 Inicializando resumen...');
    updateOrderSummary();
});

// Función global para los botones de cantidad
function changeQuantity(inputId, change) {
    const input = document.getElementById(inputId);
    if (input) {
        const currentValue = parseInt(input.value) || 0;
        const newValue = Math.max(0, Math.min(50, currentValue + change));
        input.value = newValue;

        // Trigger change event to update summary
        input.dispatchEvent(new Event('input'));
        console.log(`🔄 Cantidad cambiada: ${inputId} = ${newValue}`);
    }
}