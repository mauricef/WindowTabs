using System;
using System.Collections;
using System.Windows.Forms;

//
// Hotkey selection control, written by serenity@exscape.org, 2006-08-03
// Please mail me if you find a bug.
//

namespace Bemo.Win32
{
    /// <summary>
    /// A simple control that allows the user to select pretty much any valid hotkey combination
    /// </summary>
    [System.ComponentModel.DesignerCategory("CODE")]
    public class HotKeyControl2 : TextBox
    {
        // These variables store the current hotkey and modifier(s)
        private Keys _hotkey = Keys.None;
        private Keys _modifiers = Keys.None;

        // ArrayLists used to enforce the use of proper modifiers.
        // Shift+A isn't a valid hotkey, for instance, as it would screw up when the user is typing.
        private ArrayList needNonShiftModifier = null;
        private ArrayList needNonAltGrModifier = null;

        private ContextMenu dummy = new ContextMenu();

        /// <summary>
        /// Used to make sure that there is no right-click menu available
        /// </summary>
        public override ContextMenu ContextMenu
        {
            get
            {
                return dummy;
            }
            set
            {
                base.ContextMenu = dummy;
            }
        }

        /// <summary>
        /// Forces the control to be non-multiline
        /// </summary>
        public override bool Multiline
        {
            get
            {
                return base.Multiline;
            }
            set
            {
                // Ignore what the user wants; force Multiline to false
                base.Multiline = false;
            }
        }

        /// <summary>
        /// Creates a new HotkeyControl
        /// </summary>
        public HotKeyControl2()
        {
            this.ContextMenu = dummy; // Disable right-clicking
            this.Text = "None";

            // Handle events that occurs when keys are pressed
            this.KeyPress += new KeyPressEventHandler(HotkeyControl_KeyPress);
            this.KeyUp += new KeyEventHandler(HotkeyControl_KeyUp);
            this.KeyDown += new KeyEventHandler(HotkeyControl_KeyDown);

            // Fill the ArrayLists that contain all invalid hotkey combinations
            needNonShiftModifier = new ArrayList();
            needNonAltGrModifier = new ArrayList();
            PopulateModifierLists();
        }

        /// <summary>
        /// Populates the ArrayLists specifying disallowed hotkeys
        /// such as Shift+A, Ctrl+Alt+4 (would produce a dollar sign) etc
        /// </summary>
        private void PopulateModifierLists()
        {
            //// Shift + 0 - 9, A - Z
            //for (Keys k = Keys.D0; k <= Keys.Z; k++)
            //    needNonShiftModifier.Add((int)k);

            //// Shift + Numpad keys
            //for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++)
            //    needNonShiftModifier.Add((int)k);

            //// Shift + Misc (,;<./ etc)
            //for (Keys k = Keys.Oem1; k <= Keys.OemBackslash; k++)
            //    needNonShiftModifier.Add((int)k);

            //// Shift + Space, PgUp, PgDn, End, Home
            //for (Keys k = Keys.Space; k <= Keys.Home; k++)
            //    needNonShiftModifier.Add((int)k);

            //// Misc keys that we can't loop through
            //needNonShiftModifier.Add((int)Keys.Insert);
            //needNonShiftModifier.Add((int)Keys.Help);
            //needNonShiftModifier.Add((int)Keys.Multiply);
            //needNonShiftModifier.Add((int)Keys.Add);
            //needNonShiftModifier.Add((int)Keys.Subtract);
            //needNonShiftModifier.Add((int)Keys.Divide);
            //needNonShiftModifier.Add((int)Keys.Decimal);
            //needNonShiftModifier.Add((int)Keys.Return);
            //needNonShiftModifier.Add((int)Keys.Escape);
            //needNonShiftModifier.Add((int)Keys.NumLock);
            //needNonShiftModifier.Add((int)Keys.Scroll);
            //needNonShiftModifier.Add((int)Keys.Pause);

            //// Ctrl+Alt + 0 - 9
            //for (Keys k = Keys.D0; k <= Keys.D9; k++)
            //    needNonAltGrModifier.Add((int)k);
        }

        /// <summary>
        /// Resets this hotkey control to None
        /// </summary>
        public new void Clear()
        {
            this.Hotkey = Keys.None;
            this.HotkeyModifiers = Keys.None;
        }

        /// <summary>
        /// Fires when a key is pushed down. Here, we'll want to update the text in the box
        /// to notify the user what combination is currently pressed.
        /// </summary>
        void HotkeyControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Clear the current hotkey
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                ResetHotkey();
                return;
            }
            else
            {
                this._modifiers = e.Modifiers;
                this._hotkey = e.KeyCode;
                Redraw();
            }
        }

        /// <summary>
        /// Fires when all keys are released. If the current hotkey isn't valid, reset it.
        /// Otherwise, do nothing and keep the text and hotkey as it was.
        /// </summary>
        void HotkeyControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (this._hotkey == Keys.None && Control.ModifierKeys == Keys.None)
            {
                ResetHotkey();
                return;
            }
        }

        /// <summary>
        /// Prevents the letter/whatever entered to show up in the TextBox
        /// Without this, a "A" key press would appear as "aControl, Alt + A"
        /// </summary>
        void HotkeyControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Handles some misc keys, such as Ctrl+Delete and Shift+Insert
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Delete || keyData == (Keys.Control | Keys.Delete))
            {
                ResetHotkey();
                return true;
            }

            if (keyData == (Keys.Shift | Keys.Insert)) // Paste
                return true; // Don't allow

            // Allow the rest
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Clears the current hotkey and resets the TextBox
        /// </summary>
        public void ResetHotkey()
        {
            this._hotkey = Keys.None;
            this._modifiers = Keys.None;
            Redraw();
        }

        /// <summary>
        /// Used to get/set the hotkey (e.g. Keys.A)
        /// </summary>
        public Keys Hotkey
        {
            get
            {
                return this._hotkey;
            }
            set
            {
                this._hotkey = value;
                Redraw(true);
            }
        }

        /// <summary>
        /// Used to get/set the modifier keys (e.g. Keys.Alt | Keys.Control)
        /// </summary>
        public Keys HotkeyModifiers
        {
            get
            {
                return this._modifiers;
            }
            set
            {
                this._modifiers = value;
                Redraw(true);
            }
        }

        /// <summary>
        /// Helper function
        /// </summary>
        private void Redraw()
        {
            Redraw(false);
        }

        /// <summary>
        /// Redraws the TextBox when necessary.
        /// </summary>
        /// <param name="bCalledProgramatically">Specifies whether this function was called by the Hotkey/HotkeyModifiers properties or by the user.</param>
        private void Redraw(bool bCalledProgramatically)
        {
            // No hotkey set
            if (this._hotkey == Keys.None)
            {
                this.Text = "None";
                return;
            }

            // LWin/RWin doesn't work as hotkeys (neither do they work as modifier keys in .NET 2.0)
            if (this._hotkey == Keys.LWin || this._hotkey == Keys.RWin)
            {
                this.Text = "None";
                return;
            }

            // Only validate input if it comes from the user
            if (bCalledProgramatically == false)
            {
                // No modifier or shift only, AND a hotkey that needs another modifier
                if ((this._modifiers == Keys.Shift || this._modifiers == Keys.None) &&
                this.needNonShiftModifier.Contains((int)this._hotkey))
                {
                    if (this._modifiers == Keys.None)
                    {
                        // Set Ctrl+Alt as the modifier unless Ctrl+Alt+<key> won't work...
                        if (needNonAltGrModifier.Contains((int)this._hotkey) == false)
                            this._modifiers = Keys.Alt | Keys.Control;
                        else // ... in that case, use Shift+Alt instead.
                            this._modifiers = Keys.Alt | Keys.Shift;
                    }
                    else
                    {
                        // User pressed Shift and an invalid key (e.g. a letter or a number),
                        // that needs another set of modifier keys
                        this._hotkey = Keys.None;
                        this.Text = this._modifiers.ToString() + " + Invalid key";
                        return;
                    }
                }
                // Check all Ctrl+Alt keys
                if ((this._modifiers == (Keys.Alt | Keys.Control)) &&
                    this.needNonAltGrModifier.Contains((int)this._hotkey))
                {
                    // Ctrl+Alt+4 etc won't work; reset hotkey and tell the user
                    this._hotkey = Keys.None;
                    this.Text = this._modifiers.ToString() + " + Invalid key";
                    return;
                }
            }

            if (this._modifiers == Keys.None)
            {
                if (this._hotkey == Keys.None)
                {
                    this.Text = "None";
                    return;
                }
                else
                {
                    // We get here if we've got a hotkey that is valid without a modifier,
                    // like F1-F12, Media-keys etc.
                    this.Text = this._hotkey.ToString();
                    return;
                }
            }

            // I have no idea why this is needed, but it is. Without this code, pressing only Ctrl
            // will show up as "Control + ControlKey", etc.
            if (this._hotkey == Keys.Menu /* Alt */ ||
                this._hotkey == Keys.ShiftKey || 
                this._hotkey == Keys.ControlKey)
                this._hotkey = Keys.None;

            this.Text = this._modifiers.ToString() + " + " + this._hotkey.ToString();
        }
    }
}
