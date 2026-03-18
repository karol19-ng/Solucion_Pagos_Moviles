namespace Pegasos.WEB.Admin.Controllers

open System
open System.Collections.Generic
open System.Security.Claims
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.Extensions.Logging
open Pegasos.WEB.Admin.Models

open Pegasos.Web.Admin.Services

type AuthController(authService: IAuthService, logger: ILogger<AuthController>) =
    inherit Controller()

    // Simulación de bloqueo estática
    static let loginAttempts = Dictionary<string, int * DateTime option>()

    // Métodos privados de ayuda para el bloqueo
    let isUserLockedOut (username: string) =
        match loginAttempts.TryGetValue(username.ToLower()) with
        | true, (attempts, Some lockoutEnd) when lockoutEnd > DateTime.Now -> true
        | true, (_, Some lockoutEnd) when lockoutEnd <= DateTime.Now ->
            loginAttempts.Remove(username.ToLower()) |> ignore
            false
        | _ -> false

    let registerFailedAttempt (username: string) =
        let user = username.ToLower()
        match loginAttempts.TryGetValue(user) with
        | true, (attempts, lockout) -> loginAttempts.[user] <- (attempts + 1, lockout)
        | _ -> loginAttempts.[user] <- (1, None)

    let getFailedAttempts (username: string) =
        match loginAttempts.TryGetValue(username.ToLower()) with
        | true, (attempts, _) -> attempts
        | _ -> 0

    let lockUser (username: string) =
        loginAttempts.[username.ToLower()] <- (3, Some (DateTime.Now.AddMinutes(15.0)))

    let clearAttempts (username: string) =
        loginAttempts.Remove(username.ToLower()) |> ignore

    [<HttpGet>]
    [<AllowAnonymous>]
    member this.Login(returnUrl: string) =
        if this.User.Identity.IsAuthenticated then
            this.RedirectToAction("Index", "Home") :> IActionResult
        else
            if this.TempData.ContainsKey("SessionExpired") then
                this.ViewData.["SessionExpired"] <- true
            
            this.ViewData.["ReturnUrl"] <- returnUrl
            this.View(LoginViewModel()) :> IActionResult

    [<HttpPost>]
    [<AllowAnonymous>]
    [<ValidateAntiForgeryToken>]
    member this.Login(model: LoginViewModel, returnUrl: string) =
        async {
            if not this.ModelState.IsValid then
                return this.View(model) :> IActionResult
            else
                let username = model.Username.ToLower()

                if isUserLockedOut username then
                    model.ErrorMessage <- "Usuario bloqueado. Intente más tarde."
                    logger.LogWarning("Intento en usuario bloqueado: {Username}", username)
                    return this.View(model) :> IActionResult
                else
                    // Llamada al servicio
                    let! resultOpt = authService.LoginAsync(model.Username, model.Password) |> Async.AwaitTask

                    match resultOpt with
                    | None ->
                        registerFailedAttempt username
                        let attempts = getFailedAttempts username
                        let remaining = 3 - attempts

                        if remaining <= 0 then
                            lockUser username
                            model.ErrorMessage <- "Bloqueado por seguridad tras 3 intentos."
                        else
                            model.ErrorMessage <- sprintf "Credenciales incorrectas. Intentos restantes: %i" remaining

                        logger.LogWarning("Login fallido: {Username} (Intento {Attempt}/3)", username, attempts)
                        return this.View(model) :> IActionResult

                    | Some result ->
                        clearAttempts username
                        
                        let claims = [
                            Claim(ClaimTypes.Name, model.Username)
                            Claim(ClaimTypes.NameIdentifier, result.UsuarioId.ToString())
                            Claim("nombreCompleto", result.NombreCompleto)
                            Claim("access_token", result.AccessToken)
                            Claim("refresh_token", result.RefreshToken)
                            Claim(ClaimTypes.Role, "Administrador")
                        ]

                        let identity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                        let principal = ClaimsPrincipal(identity)

                        let authProperties = AuthenticationProperties(
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = Nullable(DateTimeOffset(result.ExpiresIn)),
                            AllowRefresh = false
                        )

                        do! this.HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            authProperties) |> Async.AwaitTask

                        logger.LogInformation("Login exitoso: {Username}", username)

                        if not (String.IsNullOrEmpty(returnUrl)) && this.Url.IsLocalUrl(returnUrl) then
                            return this.Redirect(returnUrl) :> IActionResult
                        else
                            return this.RedirectToAction("Index", "Home") :> IActionResult
        } |> Async.StartAsTask

    [<HttpPost>]
    [<ValidateAntiForgeryToken>]
    member this.Logout([<FromQuery>] expired: bool) =
        async {
            do! this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme) |> Async.AwaitTask
            if expired then this.TempData.["SessionExpired"] <- true
            logger.LogInformation("Logout exitoso")
            return this.RedirectToAction("Login") :> IActionResult
        } |> Async.StartAsTask

    [<HttpGet>]
    member this.AccessDenied() = this.View() :> IActionResult