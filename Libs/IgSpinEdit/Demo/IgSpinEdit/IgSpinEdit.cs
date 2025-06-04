//=================================================================//
// Copyright (C) 2008. Igor Voynovskyy. (igor.voynovskyy@gmail.com)
//=================================================================//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IgNS.Controls
{
    /// <summary>
    /// SpinEdit class with some extra features such as extra up/down for instant increment 
    /// adjustment; late update of the value thru external code, support for power 2 increment;
    /// adjustable format including hex and more.
    /// </summary>
    /// <remarks>
    /// Please pay attention for properties adjustment because some of them related and
    /// for flawless operation they should be set with understanding. Just apply common sense.
    /// </remarks>
    [DefaultEvent("")]
    [DefaultProperty("Value")]
    public partial class IgSpinEdit : UserControl, ISupportInitialize
    {
        public IgSpinEdit()
        {
            InitializeComponent();

            valAsInt = true;
            hint.SetToolTip(btnIncr, string.Format("Increment: {0}", incr));
        }

        private void edVal_SizeChanged(object sender, EventArgs e)
        {
            if (BorderStyle == BorderStyle.None)
                Height = edVal.Height + 4;
            else if (BorderStyle == BorderStyle.FixedSingle)
                Height = edVal.Height + 6;
            else Height = edVal.Height + 8;
        }

        #region ISupportInitialize Members
        bool initializing;
        public void BeginInit()
        {
            initializing = true;
        }
        public void EndInit()
        {
            initializing = false;
            Value = val; // to correct possibly erroneous display of value caused by ValAsInt set during construction
        }
        #endregion

        /// <summary>
        /// Allows to set tooptip message to be shown when mouse hover over control.
        /// </summary>
        /// <param name="caption">String to be shown in tooltip.</param>
        public void SetToolTip(string caption)
        {
            hint.SetToolTip(edVal, caption);
        }

        //== Format string =====
        string formatStr = "{0}";
        /// <summary>
        /// Format string adherent to the same conventions as used for standard Format().
        /// For displaying value as hex ValueAsHex should be set and hex format string used,
        /// e.g. "0x{0:X4}".
        /// </summary>
        [Description("Format string which controls how value is displayed.")]
        [Category("Appearance")]
        public string FormatString
        {
            get { return formatStr; }
            set {
                try
                {
                    if (valAsInt) 
                        edVal.Text = string.Format(value, (long)val);
                    else edVal.Text = string.Format(value, val);
                    formatStr = value;
                }
                catch { 
                    formatStr = "{0}";
                    edVal.Text = string.Format(formatStr, val);
                }
            }
        }
        /// <summary>
        /// Controls if extra increment up/down is shown. 
        /// If Power2Increament is set IncreamentVisible is reset.
        /// </summary>
        [Description("Controls if extra increment up/down is shown.")]
        [Category("Appearance")]
        public bool IncrementVisible
        {
            get { return btnIncr.Visible; }
            set { btnIncr.Visible = value; }
        }
        /// <summary>
        /// Back color of the edit portion of the control.
        /// </summary>
        [Description("Sets back color of the edit portion of the control.")]
        [Category("Appearance")]
        public Color ValueBackColor
        {
            get { return edVal.BackColor; }
            set { edVal.BackColor = value; }
        }
        protected override void OnForeColorChanged(EventArgs e)
        {
            edVal.ForeColor = this.ForeColor;
            base.OnForeColorChanged(e);
        } 
        /// <summary>
        /// Indicates how value should be aligned inside control.
        /// </summary>
        [Description("Indicates how value should be aligned inside control.")]
        [Category("Appearance")]
        public HorizontalAlignment TextAlign
        {
            get { return edVal.TextAlign; }
            set { edVal.TextAlign = value; }
        }

        bool extUpdate = false;
        bool pow2Incr = false;
        bool valAsHex = false;
        bool valAsInt = false;

        /// <summary>
        /// Enables to implement late uprate of the value. If this property is set and ValueChanging
        /// is assigned, the new value is passed to event handler, but Value is not updated until it is
        /// done by explicitly assigning Value from outside of the control. This is useful when new value is 
        /// send to remote destination, but it may be set or not or set to different value and this actual
        /// value returned from remote point is then assigned to the Value property of the control. 
        /// </summary>
        [Description("Enables to implement late (external) uprate of the value.")]
        [Category("Behavior")]
        public bool ExternalUpdate
        {
            get { return extUpdate; }
            set { extUpdate = value; }
        }
        /// <summary>
        /// Allows to implement increment scheme where next value is twice as higher or twice as lower 
        /// as previous. Most useful to generate 1, 2, 4, 8, ..., 2^n sequence. But not only.
        /// </summary>
        [Description("Allows to implement increment scheme where next value is twice as higher or twice as lower as previous.")]
        [Category("Behavior")]
        public bool Pow2Increment
        {
            get { return pow2Incr; }
            set { 
                pow2Incr = value;

                if (pow2Incr && val == 0) Value = 1.0;

                IncrementVisible = !value; 
            }
        }
        /// <summary>
        /// Allows to display value as integer. Does not influence internal value format which is double.
        /// </summary>
        [Description("Allows to display value as integer.")]
        [Category("Behavior")]
        public bool ValueAsInt
        {
            get { return valAsInt; }
            set { valAsInt = value; }
        }
        /// <summary>
        /// Along with proper hex format string allows to display Value as hex. Also sets the increment 
        /// increase/decrease multiplier to 16.
        /// </summary>
        [Description("Along with proper hex format string allows to display Value as hex. Also sets the increment increase/decrease multiplier to 16.")]
        [Category("Behavior")]
        public bool ValueAsHex
        {
            get { return valAsHex; }
            set {
                ValueAsInt = true;
                valAsHex = value; 
            }
        }

        //== Value change =====
        double val = 0.0;
        double valMax = 100.0;
        double valMin = 0.0;

        double incr = 1.0;
        double incrMax = 10.0;
        double incrMin = 0.1;

        /// <summary>
        /// Called before new value has been changed. Allows to cancel the value change or implement 
        /// late update along with ExternalUpdate property.
        /// </summary>
        [Description("The event is raised before new value has been changed. Allows to cancel the value change or implement "+ 
        "late update along with ExternalUpdate property.")]
        [Category("Behavior")]
        public event IgSpinEditChanged ValueChanging;
        /// <summary>
        /// Called when new value has been set.
        /// </summary>
        [Description("The event is raised when new value has been set.")]
        [Category("Behavior")]
        public event EventHandler ValueChanged;

        IgSpinEditChangedArgs arg = new IgSpinEditChangedArgs(0);
        decimal lastUpDownVal = 0;
        //-------------------------
        void btnVal_ValueChanged(object sender, EventArgs e)
        {
            double locVal = val;
            if (btnVal.Value > lastUpDownVal)
            {
                if (pow2Incr) locVal *= 2;
                else locVal += incr;
            }
            else
            {
                if (pow2Incr) locVal /= 2;
                else locVal -= incr;
            }
            lastUpDownVal = btnVal.Value;

            if (ValueChanging != null)
            {
                if (locVal < valMin) locVal = valMin;
                if (locVal > valMax) locVal = valMax;

                arg.Value = locVal;
                ValueChanging(this, arg);

                if (arg.Cancel)
                    return;
                if (extUpdate) return;
            }
            Value = locVal;
        }
        //-------------------------
        /// <summary>
        /// Maximum for the value.
        /// </summary>
        [Description("Maximum for the Value.")]
        [Category("Behavior")]
        public double ValueMax
        {
            get { return valMax; }
            set
            {
                if (initializing)
                {
                    valMax = value;
                    return;
                }
                if (value == valMax) return;
                if (value < valMin) value = valMin;
                valMax = value;
                if (val > value) Value = value;
            }
        }
        //-------------------------
        /// <summary>
        /// Minimum for the Value.
        /// </summary>
        [Description("Minimum for the Value.")]
        [Category("Behavior")]
        public double ValueMin
        {
            get { return valMin; }
            set
            {
                if (initializing)
                {
                    valMin = value;
                    return;
                }
                if (value == valMin) return;
                if (value > valMax) value = valMax;
                valMin = value;
                if (val < value) Value = value;
            }
        }
        //-------------------------
        /// <summary>
        /// Represent value controlled by this SpinEdit.
        /// </summary>
        [Description("Represent value controlled by this SpinEdit.")]
        [Category("Behavior")]
        public double Value
        {
            get { return val; }
            set
            {
                if ( ! initializing)
                {
                    //if (value = val) return;
                    if (value < valMin) value = valMin;
                    if (value > valMax) value = valMax;

                    if (pow2Incr && value == 0) value = 1;
                }
                val = value;

                try
                {
                    if (valAsInt)
                        edVal.Text = string.Format(formatStr, (long)val);
                    else edVal.Text = string.Format(formatStr, val);
                }
                catch
                {
                    formatStr = "{0}";
                    edVal.Text = string.Format(formatStr, val);
                }

                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }
        //-------------------------

        //== Increament change =====
        decimal lastIncr = 0;
        private void btnIncr_ValueChanged(object sender, EventArgs e)
        {
            if (btnIncr.Value > lastIncr)
            {
                if (valAsHex) Increament *= 16;
                else Increament *= 10;
            }
            else
            {
                if (valAsHex) Increament /= 16;
                else Increament /= 10;
            }
            lastIncr = btnIncr.Value;
        }
        /// <summary>
        /// Increment of the value for every mode except when Pow2Increment is set.
        /// </summary>
        [Description("Increment of the value for every mode except when Pow2Increment is set.")]
        [Category("Behavior")]
        public double Increament
        {
            get { return incr; }
            set
            {
                if ( ! initializing)
                {
                    if (value == incr) return;
                    if (value == 0) value = 1.0;
                    if (value < incrMin) value = incrMin;
                    if (value > incrMax) value = incrMax;
                }                
                incr = value;
                try
                {
                    if (valAsHex)
                        hint.SetToolTip(btnIncr, string.Format("Increment: 0x{0:X}", (long)value));
                    else if (!valAsInt && (value < 1e-4 || value > 1e4))
                        hint.SetToolTip(btnIncr, string.Format("Increment: {0:0.0##e0}", value));
                    else hint.SetToolTip(btnIncr, string.Format("Increment: {0}", value));
                }
                catch
                {
                    hint.SetToolTip(btnIncr, string.Format("Increment: {0}", value));
                }
            }
        }

        /// <summary>
        /// Increment maximum value.
        /// </summary>
        [Description("Increment maximum value.")]
        [Category("Behavior")]
        public double IncrementMax
        {
            get { return incrMax; }
            set {
                if (initializing)
                {
                    incrMax = value;
                    return;
                }
                if (value < incrMin) value = incrMin;
                incrMax = value;
            }
        }
        /// <summary>
        /// Increment minimum value.
        /// </summary>
        [Description("Increment minimum value.")]
        [Category("Behavior")]
        public double IncrementMin
        {
            get { return incrMin; }
            set
            {
                if (initializing)
                {
                    incrMin = value;
                    return;
                }
                if (value > incrMax) value = incrMax;
                incrMin = value;
            }
        }
        
        //== Some last adjustments =====
        private void btnVal_Enter(object sender, EventArgs e)
        {
            // Draw selection rectangle only if control is not selected by mouse (no point to select)
            if (! btnVal.RectangleToScreen(btnVal.ClientRectangle).Contains(MousePosition))
            { 
                Graphics gr = CreateGraphics();
                gr.DrawRectangle(new Pen(Color.Black/*ValueBackColor*/), btnVal.Left, 1, 
                                                   btnVal.Width - 3, Height - 3);
            }            
        }
        private void btnIncr_Enter(object sender, EventArgs e)
        {
            // Draw selection rectangle only if control is not selected by mouse
            if (!btnIncr.RectangleToScreen(btnIncr.ClientRectangle).Contains(MousePosition))
            {
                Graphics gr = CreateGraphics();
                gr.DrawRectangle(new Pen(Color.Black/*ValueBackColor*/), btnIncr.Left, 1,
                                                   btnIncr.Width - 3, Height - 3);
            }  
        }
        private void btnVal_Leave(object sender, EventArgs e)
        {
            Refresh();
        }

        private void edVal_Click(object sender, EventArgs e)
        {
            // Relay text box Click event to user
            OnClick(e);
        }

        private void edVal_Enter(object sender, EventArgs e)
        {
            // Value field is readonly, so should not appear as if editable
            btnVal.Focus();
        }
    }
    
    /// <summary>
    /// Argument object passed with ValueCanging event.
    /// </summary>
    public class IgSpinEditChangedArgs : EventArgs
    {
        private bool cancel;
        /// <summary>
        /// Setting Cancel to true in event handler prevent value update.
        /// </summary>
        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }
        private double value;
        /// <summary>
        /// Read only new value which has not been set yet.
        /// </summary>
        public double Value
        {
            get { return this.value; }
            internal set { this.value = value; }
        }
        public IgSpinEditChangedArgs(double value)
            : base()
        {
            cancel = false;
            this.value = value;
        }
    }
    /// <summary>
    /// ValueChanging event handler signature.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void IgSpinEditChanged(object sender, IgSpinEditChangedArgs e);

}