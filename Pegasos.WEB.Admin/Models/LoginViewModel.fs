namespace Pegasos.WEB.Admin.Models

open System.ComponentModel.DataAnnotations

type LoginViewModel() =
    [<Required(ErrorMessage = "El usuario es requerido")>]
    member val Usuario = "" with get, set

    [<Required(ErrorMessage = "La contraseña es requerida")>]
    [<DataType(DataType.Password)>]
    member val Password = "" with get, set

    member val Username = "" with get, set
    member val RememberMe = false with get, set
    member val ErrorMessage = "" with get, set



type ErrorViewModel() =
    member val RequestId = "" with get, set
    member this.ShowRequestId = not (System.String.IsNullOrEmpty(this.RequestId))