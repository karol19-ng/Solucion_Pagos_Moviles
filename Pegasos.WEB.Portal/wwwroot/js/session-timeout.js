/**
 * SA4: Control de sesión por inactividad
 * Configuración: 5 minutos = 300,000 ms
 */

(function () {
    'use strict';

    // Configuración
    const SESSION_DURATION = 5 * 60 * 1000; // 5 minutos
    const WARNING_BEFORE = 30 * 1000; // Advertir 30 seg antes
    const CHECK_INTERVAL = 1000; // Revisar cada segundo

    // Elementos DOM
    const timeoutModal = document.getElementById('sessionTimeoutModal');
    const expiredModal = document.getElementById('sessionExpiredModal');
    const countdownEl = document.getElementById('countdownSeconds');
    const timerEl = document.getElementById('sessionTimer');
    const indicatorEl = document.getElementById('sessionIndicator');

    // Estado
    let lastActivity = Date.now();
    let warningShown = false;
    let checkIntervalId = null;

    /**
     * Inicializar control de sesión
     */
    function init() {
        // Solo si hay sesión activa
        if (!document.querySelector('.nexus-user-menu')) {
            return;
        }

        // Eventos que resetean el timer
        const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click'];
        events.forEach(event => {
            document.addEventListener(event, resetTimer, true);
        });

        // Iniciar revisión periódica
        checkIntervalId = setInterval(checkSession, CHECK_INTERVAL);

        // Mostrar tiempo inicial
        updateTimerDisplay(SESSION_DURATION);
    }

    /**
     * Resetear timer por actividad
     */
    function resetTimer() {
        lastActivity = Date.now();

        if (warningShown) {
            hideWarning();
        }
    }

    /**
     * Verificar estado de sesión
     */
    function checkSession() {
        const elapsed = Date.now() - lastActivity;
        const remaining = SESSION_DURATION - elapsed;

        // Actualizar display
        updateTimerDisplay(Math.max(0, remaining));

        // Mostrar advertencia
        if (remaining <= WARNING_BEFORE && remaining > 0 && !warningShown) {
            showWarning(Math.ceil(remaining / 1000));
        }

        // Sesión expirada
        if (remaining <= 0) {
            sessionExpired();
        }
    }

    /**
     * Mostrar modal de advertencia
     */
    function showWarning(seconds) {
        warningShown = true;

        if (timeoutModal) {
            timeoutModal.style.display = 'flex';
            updateCountdown(seconds);

            // Contador regresivo en el modal
            const countdownInterval = setInterval(() => {
                seconds--;
                updateCountdown(seconds);

                if (seconds <= 0 || !warningShown) {
                    clearInterval(countdownInterval);
                }
            }, 1000);
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
     * Sesión expirada - logout forzado
     */
    function sessionExpired() {
        clearInterval(checkIntervalId);
        hideWarning();

        if (expiredModal) {
            expiredModal.style.display = 'flex';
        }

        // Llamar logout después de mostrar modal
        setTimeout(() => {
            window.location.href = '/Auth/Logout?expired=true';
        }, 3000);
    }

    /**
     * Extender sesión (llamada desde botón)
     */
    window.extendSession = function () {
        hideWarning();
        resetTimer();

        // Llamada AJAX para renovar cookie en servidor
        fetch('/Auth/ExtendSession', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '',
                'Content-Type': 'application/json'
            },
            credentials: 'same-origin'
        })
            .then(response => {
                if (!response.ok) {
                    console.warn('No se pudo extender la sesión en servidor');
                }
            })
            .catch(err => {
                console.error('Error extendiendo sesión:', err);
            });
    };

    /**
     * Actualizar display del timer
     */
    function updateTimerDisplay(ms) {
        if (!timerEl) return;

        const totalSeconds = Math.floor(ms / 1000);
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        timerEl.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        // Cambiar color si queda poco tiempo
        if (totalSeconds < 60) {
            indicatorEl?.classList.add('nexus-session-warning');
        } else {
            indicatorEl?.classList.remove('nexus-session-warning');
        }
    }

    /**
     * Actualizar countdown del modal
     */
    function updateCountdown(seconds) {
        if (countdownEl) {
            countdownEl.textContent = seconds;
        }
    }

    // Iniciar cuando DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();