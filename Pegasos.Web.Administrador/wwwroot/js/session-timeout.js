/**
 * SA4: Control de sesión por inactividad - PEGASOS BANK
 * Configuración: 5 minutos = 300,000 ms
 */

(function () {
    'use strict';

    // 1. CONFIGURACIÓN
    const SESSION_DURATION = 5 * 60 * 1000; // 5 minutos totales
    const WARNING_BEFORE = 30 * 1000;      // Mostrar aviso 30 segundos antes
    const CHECK_INTERVAL = 1000;           // Revisar cada 1 segundo

    // 2. ELEMENTOS DEL DOM (Asegúrate de que existan en tu _Layout)
    const timeoutModal = document.getElementById('sessionTimeoutModal');
    const countdownEl = document.getElementById('countdownSeconds');
    const timerEl = document.getElementById('sessionTimer');
    const indicatorEl = document.getElementById('sessionIndicator');

    // 3. ESTADO
    let lastActivity = Date.now();
    let warningShown = false;
    let checkIntervalId = null;

    /**
     * Inicializar control de sesión
     */
    function init() {
        // Solo activamos el contador si el usuario está logueado 
        // (Buscamos tu sidebar de perfil que creamos antes)
        if (!document.querySelector('.user-profile-sidebar')) {
            return;
        }

        // Eventos que resetean el timer (Movimiento, teclado, scroll)
        const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];
        events.forEach(event => {
            document.addEventListener(event, resetTimer, true);
        });

        // Iniciar la revisión cada segundo
        checkIntervalId = setInterval(checkSession, CHECK_INTERVAL);

        // Mostrar tiempo inicial en la barra superior
        updateTimerDisplay(SESSION_DURATION);
    }

    /**
     * Resetear timer por actividad del usuario
     */
    function resetTimer() {
        lastActivity = Date.now();

        if (warningShown) {
            hideWarning();
        }
    }

    /**
     * Verificar cuánto tiempo queda
     */
    function checkSession() {
        const elapsed = Date.now() - lastActivity;
        const remaining = SESSION_DURATION - elapsed;

        // Actualizar el numerito en el Header
        updateTimerDisplay(Math.max(0, remaining));

        // ¿Es hora de mostrar la advertencia? (Faltan 30 seg)
        if (remaining <= WARNING_BEFORE && remaining > 0 && !warningShown) {
            showWarning(Math.ceil(remaining / 1000));
        }

        // ¿Se acabó el tiempo? (0 seg)
        if (remaining <= 0) {
            sessionExpired();
        }
    }

    /**
     * Mostrar el modal de advertencia
     */
    function showWarning(seconds) {
        warningShown = true;

        if (timeoutModal) {
            timeoutModal.style.display = 'flex';
            updateCountdown(seconds);
        }
    }

    /**
     * Ocultar advertencia
     */
    function hideWarning() {
        warningShown = false;
        if (timeoutModal) {
            timeoutModal.style.display = 'none';
        }
    }

    /**
     * Acción cuando la sesión expira
     */
    function sessionExpired() {
        clearInterval(checkIntervalId);
        hideWarning();

        // Redirección inmediata al Logout con parámetro de expiración
        window.location.href = '/Auth/Logout?expired=true';
    }

    /**
     * Función global para el botón "Continuar trabajando"
     */
    window.extendSession = function () {
        hideWarning();
        resetTimer();

        // Opcional: Llamada al servidor para mantener la cookie viva
        fetch('/Auth/ExtendSession', { method: 'POST' })
            .then(res => console.log("Sesión extendida en servidor"))
            .catch(err => console.error("Error al extender sesión"));
    };

    /**
     * Actualiza el texto del reloj (05:00)
     */
    function updateTimerDisplay(ms) {
        if (!timerEl) return;

        const totalSeconds = Math.floor(ms / 1000);
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        timerEl.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        // Poner el texto en rojo si queda menos de 1 minuto
        if (totalSeconds < 60) {
            indicatorEl?.style.setProperty('color', '#ff4444', 'important');
        } else {
            indicatorEl?.style.setProperty('color', 'var(--dorado-primario)', 'important');
        }
    }

    /**
     * Actualizar el segundero del Modal
     */
    function updateCountdown(seconds) {
        if (countdownEl) {
            countdownEl.textContent = seconds;
        }
    }

    // Arrancar cuando el DOM esté listo
    document.addEventListener('DOMContentLoaded', init);

})();