// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Windows.Forms;
using System.Drawing;

namespace Open.Diagramming
{
    //CommandFactory follows the Factory design pattern
	public class CommandFactory
	{
		//Events
		public event EventHandler CreateCommand;

        //Property variables
        private Controller _controller;

        //Constructors
        public CommandFactory(Controller controller)
        {
            if (controller == null) throw new ArgumentNullException();
            _controller = controller;
        }

        //Properties
        public Controller Controller
        {
            get
            {
                return _controller;
            }
        }

		public virtual TranslateCommand CreateTranslateCommand()
		{
			TranslateCommand command = new TranslateCommand(Controller);

			OnCreateCommand(command);
			return command;
		}

        public virtual ScaleCommand CreateScaleCommand()
        {
            ScaleCommand command = new ScaleCommand(Controller);

            OnCreateCommand(command);
            return command;
        }

        public virtual RotateCommand CreateRotateCommand()
        {
            RotateCommand command = new RotateCommand(Controller);

            OnCreateCommand(command);
            return command;
        }

        public virtual PropertyCommand CreatePropertyCommand(Element element)
        {
            PropertyCommand command = new PropertyCommand(element);

            OnCreateCommand(command);
            return command;
        }

        public virtual PropertyCommand CreatePropertyCommand(ElementList elements)
        {
            PropertyCommand command = new PropertyCommand(elements);

            OnCreateCommand(command);
            return command;
        }

        public virtual TextCommand CreateTextCommand()
        {
            TextCommand command = new TextCommand(Controller);

            OnCreateCommand(command);
            return command;
        }

        public virtual MouseDownCommand CreateMouseDownCommand(IMouseEvents element)
        {
            MouseDownCommand command = new MouseDownCommand(element);

            OnCreateCommand(command);
            return command;
        }

        public virtual MouseMoveCommand CreateMouseMoveCommand(IMouseEvents element)
        {
            MouseMoveCommand command = new MouseMoveCommand(element);

            OnCreateCommand(command);
            return command;
        }

        public virtual MouseUpCommand CreateMouseUpCommand(IMouseEvents element)
        {
            MouseUpCommand command = new MouseUpCommand(element);

            OnCreateCommand(command);
            return command;
        }

		//Raises the create element event
		protected virtual void OnCreateCommand(object sender)
		{
			if (CreateCommand != null) CreateCommand(sender,EventArgs.Empty);
		}
	}
}
