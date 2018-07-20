namespace Bemo
open System
open System.Drawing
open System.IO
open System.Windows.Forms
open Bemo.Win32
open Bemo.Win32.Forms


type HotKeyView() =
    let settingsProperty name =
        {
            new IProperty<'a> with
                member x.value
                    with get() = unbox<'a>(Services.settings.getValue(name))
                    and set(value) = Services.settings.setValue(name, box(value))
        }

    let switchTabs =
        let hotKeys = List2([
            ("nextTab", "Next Tab")
            ("prevTab", "Previous Tab")
        ])

        let editors = hotKeys.enumerate.fold (Map2()) <| fun editors (i,(key, text)) ->
            let label = UIHelper.label text
            let editor = HotKeyEditor() :> IPropEditor
            editor.control.Margin <- Padding(0,5,0,5)
            label.Margin <- Padding(0,5,0,5)
            editors.add key editor

        hotKeys.iter <| fun (key,_) ->
            let editor = editors.find key
            editor.value <- Services.program.getHotKey(key)
            editor.changed.Add <| fun() ->
                Services.program.setHotKey key (unbox<int>(editor.value))
        
        let checkBox (prop:IProperty<bool>) = 
            let checkbox = BoolEditor() :> IPropEditor
            checkbox.value <- box(prop.value)
            checkbox.changed.Add <| fun() -> prop.value <- unbox<bool>(checkbox.value)
            checkbox.control

        let settingsCheckbox key = checkBox(settingsProperty(key))

        let fields = hotKeys.map <| fun(key,text) ->
            let editor = editors.find key
            text, editor.control

        let fields = fields.prependList(List2([
            ("Run WindowTabs at startup", settingsCheckbox "runAtStartup")
            ("Fade out tabs on inactive windows", settingsCheckbox "hideInactiveTabs")
            ("Enable tabs for all programs by default", checkBox(prop<IFilterService, bool>(Services.filter, "isTabbingEnabledForAllProcessesByDefault")))
            ("Combine icons in taskbar by default", settingsCheckbox "combineIconsInTaskbar")
            ("Replace ALT+TAB with WindowTabs task switcher.", settingsCheckbox "replaceAltTab")
            ("Group windows in task switcher.", settingsCheckbox "groupWindowsInSwitcher")
        ]))

        "Switch Tabs", UIHelper.form fields

    let sections = List2([
        switchTabs
        ])

    let table = 
        let controls = sections.map <| fun(text,control) ->
            control.Dock <- DockStyle.Fill
            let group = GroupBox()
            group.Dock <- DockStyle.Fill
            group.AutoSize <- true
            group.Text <- text
            group.Controls.Add(control)
            group :> Control
        let table = UIHelper.hbox controls
        table.Dock <- DockStyle.Fill
        table

    interface ISettingsView with
        member x.key = SettingsViewType.HotKeySettings
        member x.title = "Behavior"
        member x.control = table :> Control

