open Bemo
open System
open System.Drawing
open System.Windows.Forms

let tabAppearance = {
    tabHeight = 25
    tabMaxWidth = 200
    tabOverlap = 20
    tabNormalBgColor = Color.FromRGB(0x9FC4F0)
    tabHighlightBgColor = Color.FromRGB(0xBDD5F4)
    tabActiveBgColor = Color.FromRGB(0xFAFCFE)
    tabBorderColor = Color.FromRGB(0x3A70B1)
    tabFlashBgColor = Color.FromRGB(0xFFBBBB)
    tabHeightOffset = 1
    tabIndentFlipped = 80
    tabIndentNormal = 3 
}

let dragDrop = {
    new IDragDrop with
        member this.beginDrag((hwnd, getImage, pt1, pt2, data)) = ()
        member this.registerTarget((hwnd, target)) = ()
        member this.unregisterTarget(hwnd) = ()
}

let group = WindowGroup(
    tabAppearance, 
    dragDrop
)
group.addWindow(IntPtr(0x4033A), false)
group.addWindow(IntPtr(0x90312), false)
Application.Run()