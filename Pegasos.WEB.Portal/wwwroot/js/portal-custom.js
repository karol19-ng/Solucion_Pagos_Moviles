// Funcionalidades específicas del portal de clientes

$(document).ready(function () {
    // Marcar enlace activo en el menú
    var currentUrl = window.location.pathname.toLowerCase();

    $('.menu-item').each(function () {
        var href = $(this).attr('href');
        if (href && href.toLowerCase() === currentUrl) {
            $(this).addClass('active');
        }
    });

    // Auto-cerrar alertas después de 5 segundos
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);
});

// Función para formatear teléfonos mientras se escribe
function formatPhone(input) {
    var value = input.value.replace(/\D/g, '');
    if (value.length >= 4) {
        input.value = value.substr(0, 4) + '-' + value.substr(4, 4);
    } else {
        input.value = value;
    }
}

// Validación de monto en tiempo real
function validateMonto(input) {
    var value = parseFloat(input.value);
    if (value > 100000) {
        input.setCustomValidity('El monto no puede exceder ₡100,000');
    } else if (value < 1) {
        input.setCustomValidity('El monto mínimo es ₡1');
    } else {
        input.setCustomValidity('');
    }
}