namespace Bemo

type ManagerViewService() =
    interface IManagerView with
        member x.show() =
            let form = new DesktopManagerForm()
            form.show()

        member x.show(view) =
            let form = new DesktopManagerForm()
            form.showView(view)