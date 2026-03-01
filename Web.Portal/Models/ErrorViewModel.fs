namespace Web.Portal.Models

open System

type ErrorViewModel =
    { RequestId: string }

    member this.ShowRequestId =
        not (String.IsNullOrEmpty(this.RequestId))
