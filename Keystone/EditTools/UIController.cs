using System;

namespace Keystone.Controllers
{
    // TODO: this class and it's derived are fundamentally wrong...
    // We DO want functions that we can bind too, but we don't need to host them in a class like this
    // Control.cs already has that responsibility as being a type of Entity that can have functions
    // bound to it.  The question is really about how to bind these functions especially
    // if say we want to design a button in the game world that will invoke some function... even if
    // it's a custom scripted function...
    //  So it might invoke an intrinsic function (maybe going through our InputController.cs or it
    // might invoke a scripted function)
    public abstract class UIController : IDisposable 
    {
        protected Controls.Control _control;
        protected Controls.Control _activeChildControl;

        public Controls.Control ActiveControl
        {
            get { return _activeChildControl; }
            set { _activeChildControl = value; }
        }

        public Controls.Control Control
        {
            get { return _control; }
            set { _control = value; }
        }

        #region IDisposable Members

        public abstract void Dispose();

        #endregion
    }
}
