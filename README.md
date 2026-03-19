# Solucion_Pagos_Moviles

Resumen
-------
Solución .NET 8 que implementa un portal y microservicios para pagos móviles. Incluye:
- `API_Gateway` — Gateway con Ocelot y middleware de validación JWT.
- `Pegasos.WEB.Portal` — Portal de clientes (front-end web).
- `Pegasos.Web.Administrador` — Portal administrativo.
- Servicios y microservicios varios (transacciones, auth, inscripciones, etc.).
- DTOs, entidades y utilidades compartidas.

Requisitos
---------
- .NET 8 SDK
- Visual Studio 2022/2026 o VS Code
- Certificados de desarrollo (HTTPS local)
- Puertos usados (valores de ejemplo en desarrollo):
  - API Gateway: `https://localhost:7096` (y `http://localhost:5077`)
  - Microservicio Auth: `https://localhost:7258`
  - Otros microservicios: ver `ocelot.json` y archivos de configuración

Proyectos importantes
---------------------
- `API_Gateway`
  - Archivo de rutas: `API_Gateway/Configuration/ocelot.json`
  - Middleware de autenticación: `API_Gateway/Middleware/GatewayAuthenticationMiddleware.cs`
  - Configura `AddAuthentication().AddJwtBearer(...)` en `Program.cs`.

- `Pegasos.WEB.Portal`
  - Servicio de consumo HTTP: `Pegasos.WEB.Portal/Services/PagosService.cs`
  - Envía `Authorization: Bearer <token>` hacia el gateway.

- `Pegasos.Web.Administrador`
  - Login y manejo de sesión: `Pegasos.Web.Administrador/Controllers/AuthController.cs`
  - Servicio de login: `Pegasos.Web.Administrador/Services/AuthService.cs`
  - DTOs: `Pegasos.Web.Administrador/DTOs/AuthResponseDto.cs`

Instalación y ejecución
-----------------------
1. Clona el repositorio:
   - `git clone https://github.com/karol19-ng/Solucion_Pagos_Moviles.git`
2. Abrir la solución en Visual Studio (suele detectar proyectos .NET 8).
3. Restaurar paquetes y compilar:
   - Visual Studio: Build Solution
   - CLI: `dotnet restore` y `dotnet build`
4. Ejecutar los proyectos necesarios en el orden recomendado:
   - Microservicios (incluyendo el servicio de `auth`) → `API_Gateway` → portales (`Pegasos.WEB.Portal`, `Pegasos.Web.Administrador`).
5. Asegúrate de que los `appsettings` y variables de entorno contengan:
   - `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` (mismo secreto/issuer/audience para validación local si aplicable).
   - `Ocelot` `BaseUrl` (en `API_Gateway/Configuration/ocelot.json`).

Autenticación y Ocelot (resumen funcional)
-----------------------------------------
- El gateway valida tokens JWT:
  - Primera opción: validación local con `JwtSecurityTokenHandler` usando `Jwt:Key`/`Issuer`/`Audience`.
  - Si la validación local falla, el middleware llama al microservicio `auth/validate`.
- Ocelot puede proteger rutas usando `AuthenticationOptions` en `ocelot.json` (campo `"AuthenticationProviderKey": "GatewayAuth"`).
  - Si Ocelot exige autenticación por esquema, el middleware debe registrar la autenticación con:
    - `await context.SignInAsync("GatewayAuth", principal);`
  - Alternativa (menos seguro para pruebas): quitar `AuthenticationOptions` de las rutas para dejar que el middleware próprio controle la validación (ya se usó como prueba en esta solución).
- Archivos clave:
  - `API_Gateway/Program.cs` — configuración de `AddAuthentication`, `UseAuthentication`, `UseAuthorization`.
  - `API_Gateway/Middleware/GatewayAuthenticationMiddleware.cs` — validación local y por microservicio; asigna `context.User`.

Configuración recomendada (producción)
-------------------------------------
- Nunca quitar `AuthenticationOptions` en producción: mantener Ocelot validando por esquema o delegar seguridad de forma explícita.
- Usar secretos fuertes y rotación de claves JWT.
- Configurar HTTPS/TLS, políticas CORS y límites de rate-limiting si procede.
- Habilitar logs estructurados y monitorización.

Desarrollo y pruebas
--------------------
- Ejecutar los proyectos en modo `Development` para ver logs detallados.
- Revisar logs de:
  - `API_Gateway` (console logs)
  - Portales (`Pegasos.WEB.Portal`, `Pegasos.Web.Administrador`)
  - Microservicio `auth`
- Para pruebas unitarias (si existen): `dotnet test`

Notas sobre merge y correcciones recientes
-----------------------------------------
- Se corrigieron merges en controladores y middleware relacionados con autenticación (evitar variables duplicadas y referencias fuera de alcance).
- Se dejaron dos opciones para la interoperabilidad entre Ocelot y el middleware de gateway:

Estructura de directorios (resumen)
----------------------------------
- `/API_Gateway` — gateway Ocelot, `Program.cs`, `Middleware/`
- `/Pegasos.WEB.Portal` — portal clientes
- `/Pegasos.Web.Administrador` — portal administrador
- `/Services`, `/Entities`, `/DTOs` — capas de dominio y comunicación

Contribuir
----------
- Testear localmente antes de abrir PR.
- Documentar cambios en `README` si afectan configuración o puertos.
