namespace Pegasos.Web.Admin.Services

open System
open System.Threading.Tasks

// Interfaz en F#
type IAuthService =
    abstract member LoginAsync: username:string * password:string -> Task<AuthResult option>
    abstract member ExtendSessionAsync: accessToken:string -> Task<bool>

// Clase de resultado en F#
and AuthResult() =
    member val AccessToken = String.Empty with get, set
    member val RefreshToken = String.Empty with get, set
    member val ExpiresIn = DateTime.MinValue with get, set
    member val UsuarioId = 0 with get, set
    member val NombreCompleto = String.Empty with get, set