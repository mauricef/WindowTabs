namespace Bemo
open System
open System.Drawing
open System.IO
open System.Windows.Forms
open Bemo.Win32.Forms

type LicenseView() as this =
    let panel = ref None
    let desktop = Services.program
    let statusLabel = 
        let label = Label()
        label.Dock <- DockStyle.Fill
        label.AutoEllipsis <- true
        label
    let licenseKeyLabel = 
        let label = Label()
        label.Text <- "License Key:"
        label.Dock <- DockStyle.Fill
        label
    let licenseKeyBox = 
        let box = TextBox()
        box.Multiline <- true
        box.WordWrap <- true
        box.Dock <- DockStyle.Fill
        box
    let activateButton = 
        let btn = Button()
        btn.Text <- "Activate"
        btn.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
        btn.Click.Add <| fun _ ->
            this.tryUnlock()
        btn
    let offlineActivate =
        let btn = Button()
        btn.Text <- "Offline"
        btn.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
        btn.Click.Add <| fun _ ->
            let form = Form()
            form.Text <- "Offline Activation"
            let label =
                let l = Label()
                l.Dock <- DockStyle.Fill
                l.Text <- "If you are unable to activate over the internet, contact support@windowtabs.com and we will supply an activation code to paste below"
                l
            let keyBox = 
                let box = TextBox()
                box.Multiline <- true
                box.WordWrap <- true
                box.ScrollBars <- ScrollBars.Both
                box.Dock <- DockStyle.Fill
                box
            let okBtn =
                let btn = Button()
                btn.Text <- "OK"
                btn.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
                btn.Click.Add <| fun _ ->
                    form.DialogResult <- DialogResult.OK
                btn
            let layout = UIHelper.vbox(List2([label :>Control; keyBox:>Control; okBtn:>Control]))
            layout.Padding <- Padding(10)
            layout.Dock <- DockStyle.Fill
            let style = layout.RowStyles.Item(0)
            style.SizeType <- SizeType.Absolute
            style.Height <- 50.0f
            let style = layout.RowStyles.Item(1)
            style.SizeType <- SizeType.Absolute
            style.Height <- 300.0f
            layout.Dock <- DockStyle.Fill
            form.Controls.Add(layout)
            form.StartPosition <- FormStartPosition.CenterParent
            form.Size <- Size(500, 500)
            let result = form.ShowDialog(panel.Value.Value)
            if result = DialogResult.OK then
                let bytes = System.Convert.FromBase64String(keyBox.Text);
                let s = System.Text.Encoding.Unicode.GetString(bytes)
                let s2 = System.Text.Encoding.ASCII.GetString(bytes)
                Services.lm.setTicketString s2
                this.updateLockedStatus()
        btn
    do
        this.updateLockedStatus()
        licenseKeyBox.Text <- Services.lm.licenseKey
        let children = List2<Control>([
            statusLabel 
            licenseKeyLabel
            licenseKeyBox
            activateButton
            offlineActivate
            ])
        
        panel := Some(UIHelper.vbox children)
        panel.Value.Value.Dock <- DockStyle.Fill
        panel.Value.Value.Padding <- Padding(10)
        let setLicenseTextBoxRowStyle = 
            let style = panel.Value.Value.RowStyles.Item(2)
            style.SizeType <- SizeType.Absolute
            style.Height <- 100.0f
        ()

    member this.updateLockedStatus() =
        let isLocked = Services.lm.isLicensed.not
        if isLocked then
            statusLabel.Text <- "Locked Trial - Three tab per group limit."
            statusLabel.Font <- Font(statusLabel.Font, FontStyle.Bold)
            statusLabel.ForeColor <- Color.Black
        else
            statusLabel.Text <- "Activated! Enjoy WindowTabs!"
            statusLabel.Font <- Font(statusLabel.Font, FontStyle.Bold)
            statusLabel.ForeColor <- Color.Green

    member this.tryUnlock() = 
        let licenseKey = licenseKeyBox.Text
        Services.lm.licenseKey <- licenseKey
        if Services.lm.isLicensed then
            this.updateLockedStatus()
            MessageBox.Show("WindowTabs has been successfully activated!", "WindowTabs Activated").ignore
        else
            this.updateLockedStatus()
            MessageBox.Show("Please ensure your computer has internet access and that your license key is correct. Email support@windowtabs.com if you are having trouble activating WindowTabs.", "Activation Failed").ignore

    interface ISettingsView with
        member x.key = SettingsViewType.LicenseSettings
        member x.title = "License"
        member x.control = panel.Value.Value :> Control