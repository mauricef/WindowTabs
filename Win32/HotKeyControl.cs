using System;
using System.Windows.Forms;
using Bemo.Win32;

namespace Bemo.Win32
{
    [System.ComponentModel.DesignerCategory("CODE")]
    public class HotKeyControl : TextBox
    {
        public HotKeyControl()
        {
        }
        public int HotKey
        {
            set
            {
                WinUserApi.SendMessage(Handle, HotKeyConstants.HKM_SETHOTKEY, new IntPtr(value), IntPtr.Zero); 
            }
            get
            {
                return (int)WinUserApi.SendMessage(Handle, HotKeyConstants.HKM_GETHOTKEY, IntPtr.Zero, IntPtr.Zero);
            }
        }
        public event EventHandler HotKeyChanged;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassName = CommonControlClassNames.HOTKEY_CLASS;
                cp.ExStyle = 0;
                cp.Style = WindowsStyles.WS_CHILD | WindowsStyles.WS_VISIBLE;
                return cp;
            }
        }
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (HotKeyChanged != null)
            {
                HotKeyChanged(this, EventArgs.Empty);
            }
        }
        protected override void  WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WindowMessages.WM_COMMAND:
                    if (WinDefApi.HIWORD(m.WParam) == WindowMessages.EN_CHANGE)
                    {
                        if (HotKeyChanged != null)
                        {
                            HotKeyChanged(this, EventArgs.Empty);
                        }
                    }
                    break;
            }
 	         base.WndProc(ref m);
        }
        #region Private

        #endregion
    }
}
