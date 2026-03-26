let sessionTimer;

function iniciarContadorSesion(minutos, logoutUrl) {
    if (sessionTimer) {
        clearInterval(sessionTimer);
    }

    sessionTimer = setInterval(function () {
        // Mostrar mensaje de sesión expirada después de 5 minutos
        const mensaje = document.createElement('div');
        mensaje.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.9);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 9999;
        `;

        mensaje.innerHTML = `
            <div style="
                background: #2b0811;
                border: 2px solid #c5a367;
                border-radius: 12px;
                padding: 35px;
                text-align: center;
                max-width: 380px;
                box-shadow: 0 0 25px rgba(197,163,103,0.3);
            ">
                <i class="fas fa-clock" style="font-size: 55px; color: #c5a367; margin-bottom: 20px;"></i>
                <h3 style="color: white; margin-bottom: 15px; font-family: 'Cinzel', serif;">Sesión Expirada</h3>
                <p style="color: rgba(255,255,255,0.8); margin-bottom: 25px; font-size: 15px;">
                    Su sesión ha expirado por inactividad.
                </p>
                <button onclick="window.location.href='${logoutUrl}?expired=true'" 
                        style="
                            background: linear-gradient(135deg, #c5a367 0%, #8e6d31 100%);
                            border: none;
                            padding: 12px 30px;
                            border-radius: 6px;
                            color: #1a050a;
                            font-weight: bold;
                            cursor: pointer;
                            font-size: 16px;
                            width: 100%;
                        ">
                    Volver a iniciar sesión
                </button>
            </div>
        `;

        document.body.appendChild(mensaje);

        clearInterval(sessionTimer);
    }, minutos * 60 * 1000);
}