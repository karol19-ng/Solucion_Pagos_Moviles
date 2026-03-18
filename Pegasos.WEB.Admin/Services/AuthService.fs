namespace Pegasos.Web.Admin.Services

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Microsoft.Extensions.Logging

type AuthService(httpClient: HttpClient, logger: ILogger<AuthService>) =
    
    interface IAuthService with
        member this.LoginAsync(username, password) =
            async {
                try
                    let request = {| Username = username; Password = password |}
                    let json = JsonSerializer.Serialize(request)
                    let content = new StringContent(json, Encoding.UTF8, "application/json")

                    // Llamada al Gateway
                    let! response = httpClient.PostAsync("auth/login", content) |> Async.AwaitTask

                    if response.StatusCode = System.Net.HttpStatusCode.Unauthorized then
                        return None
                    
                    elif response.StatusCode = System.Net.HttpStatusCode.Created then
                        let! responseJson = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
                        let result = JsonSerializer.Deserialize<AuthResult>(responseJson, options)
                        return Some result
                    
                    else
                        logger.LogWarning("Login inesperado: {Status}", response.StatusCode)
                        return None
                with
                | ex ->
                    logger.LogError(ex, "Error en login")
                    return None
            } |> Async.StartAsTask

        member this.ExtendSessionAsync(accessToken) =
            async {
                try
                    httpClient.DefaultRequestHeaders.Authorization <- 
                        new AuthenticationHeaderValue("Bearer", accessToken)

                    let! response = httpClient.PostAsync("auth/refresh", new StringContent("{}")) |> Async.AwaitTask
                    return response.IsSuccessStatusCode
                with
                | _ -> return false
            } |> Async.StartAsTask