﻿using System.Windows.Forms;

namespace SimpleWizard
{
    public interface IWizardPage
    {
        UserControl Content { get; }
        void Load();
        void Save();
        void Cancel();
        bool IsBusy { get; }

        bool PageValid { get; }
        string ValidationMessage { get; }
    }
}