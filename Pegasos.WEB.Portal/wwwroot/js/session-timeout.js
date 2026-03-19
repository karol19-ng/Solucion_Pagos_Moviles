let sessionTimer;
let timeLeft = 300;
let timerDisplay = document.getElementById('timerDisplay');

function iniciarContadorSesion(minutos, logoutUrl) {
    timeLeft = minutos * 60;
    actualizarDisplay();

    if (sessionTimer) {
        clearInterval(sessionTimer);
    }

    sessionTimer = setInterval(function () {
        timeLeft--;
        actualizarDisplay();

        if (timeLeft <= 0) {
            clearInterval(sessionTimer);

            // Mostrar mensaje simple
            const mensaje = document.createElement('div');
            mensaje.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: rgba(0,0,0,0.8);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 9999;
            `;

            mensaje.innerHTML = `
                <div style="
                    background: #2b0811;
                    border: 2px solid #c5a367;
                    border-radius: 10px;
                    padding: 30px;
                    text-align: center;
                    max-width: 350px;
                ">
                    <i class="fas fa-clock" style="font-size: 40px; color: #c5a367; margin-bottom: 15px;"></i>
                    <p style="color: white; font-size: 16px; margin-bottom: 20px;">
                        Su sesión ha expirado por inactividad.
                    </p>
                    <button onclick="window.location.href='${logoutUrl}?expired=true'" 
                            style="
                                background: linear-gradient(135deg, #c5a367 0%, #8e6d31 100%);
                                border: none;
                                padding: 10px 25px;
                                border-radius: 5px;
                                color: #1a050a;
                                font-weight: bold;
                                cursor: pointer;
                                width: 100%;
                            ">
                        Aceptar
                    </button>
                </div>
            `;

            document.body.appendChild(mensaje);
        }
    }, 1000);

    $(document).on('mousemove keydown click scroll', function () {
        timeLeft = 300;
        actualizarDisplay();
    });
}

function actualizarDisplay() {
    if (timerDisplay) {
        let minutes = Math.floor(timeLeft / 60);
        let seconds = timeLeft % 60;
        timerDisplay.textContent = minutes.toString().padStart(2, '0') + ':' +
            seconds.toString().padStart(2, '0');
    }
}