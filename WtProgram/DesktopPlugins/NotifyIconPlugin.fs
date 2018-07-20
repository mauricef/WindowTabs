namespace Bemo
open System
open System.Windows.Forms

type NotifyIconPlugin() as this =
    let Cell = CellScope()

    member this.icon = Cell.cacheProp this <| fun() ->
        let notifyIcon = new NotifyIcon()
        notifyIcon.Visible <- true
        notifyIcon.Text <- "WindowTabs (version " + Services.program.version + ")"
        notifyIcon.Icon <- Services.openIcon("Bemo.ico")
        notifyIcon.ContextMenu <- new ContextMenu()
        notifyIcon.DoubleClick.Add <| fun _ -> Services.managerView.show()
        notifyIcon

    member this.contextMenuItems = this.icon.ContextMenu.MenuItems

    member this.addItem(text, handler) =
        this.contextMenuItems.Add(text, EventHandler(fun obj (e:EventArgs) -> handler())) |> ignore

    member this.onNewVersion() =
        this.icon.ShowBalloonTip(
            1000,
            "A new version is available.",
            "Please visit windowtabs.com to download the latest version.",
            ToolTipIcon.Info
        )

    interface IPlugin with
        member this.init() =
            this.addItem("Settings...", fun() -> Services.managerView.show())
            this.addItem("Feedback...", Forms.openFeedback)
            this.contextMenuItems.Add("-").ignore
            this.addItem("Close WindowTabs", fun() -> Services.program.shutdown())
            Services.program.newVersion.Add this.onNewVersion

    interface IDisposable with
        member this.Dispose() = this.icon.Dispose()