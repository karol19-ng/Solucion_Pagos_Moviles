namespace Pegasos.WEB.Admin.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics
open Pegasos.WEB.Admin.Models
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging


type HomeController (logger : ILogger<HomeController>) =
    inherit Controller()

    member this.Index () =
        this.View()

    member this.Privacy () =
        this.View()

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View(ErrorViewModel(RequestId = reqId)) :> IActionResult
