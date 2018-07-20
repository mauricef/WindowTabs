namespace Bemo
open System

type ExceptionHandlerPlugin() as this =

    member this.onException(e:UnhandledExceptionEventArgs) =
        ()

    interface IPlugin with
        member x.init() =
            AppDomain.CurrentDomain.UnhandledException.Add this.onException