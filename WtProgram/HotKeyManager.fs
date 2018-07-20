namespace Bemo
open System
open System.Drawing
open System.Runtime.InteropServices
open System.Windows.Forms
open Bemo.Win32

type HotKeyManager() =
    let Cell = CellScope()
    let os = OS()
    let hotKeyIds = Cell.create(Map2())
    let handlers = Cell.create(Map2())
    let nextId = Cell.create(1)
    let hotKeyWindow =
        let wndProc m =
            match m.msg with
            | WindowMessages.WM_HOTKEY ->
                let id = m.wParam.ToInt32()
                handlers.value.tryFind(id).iter <| fun handler ->
                    handler()
            | _ -> ()
            m.def()
        let window = os.createWindow wndProc 0 0
        let window = os.windowFromHwnd(window.hwnd)
        window

    member private this.findOrAllocateId name =
        match hotKeyIds.value.tryFind name with
        | Some(id) -> id
        | None ->
            let id = nextId.value
            nextId.map((+)1)
            hotKeyIds.map(fun m -> m.add name id)
            id

    member this.register name (modifiers,key) handler =
        let id = this.findOrAllocateId name
        handlers.map(fun m -> m.add id handler)
        hotKeyWindow.unregisterHotKey(id).ignore
        hotKeyWindow.registerHotKey(id,modifiers,key,true)

    member this.unregister name =
        let id = this.findOrAllocateId name
        hotKeyWindow.unregisterHotKey(id).ignore
        handlers.map(fun m -> m.remove id)
