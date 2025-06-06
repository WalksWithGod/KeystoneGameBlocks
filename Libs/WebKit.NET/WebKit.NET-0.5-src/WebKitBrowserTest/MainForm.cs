﻿/*
 * Copyright (c) 2009, Peter Nelson (charn.opcode@gmail.com)
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * * Redistributions of source code must retain the above copyright notice, 
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice, 
 *   this list of conditions and the following disclaimer in the documentation 
 *   and/or other materials provided with the distribution.
 *   
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using WebKit;
using WebKit.DOM;
using System.Threading;

namespace WebKitBrowserTest
{
    public partial class MainForm : Form
    {
        WebBrowserTabPage currentPage;

        public MainForm()
        {
            InitializeComponent();

            // an an initial tab
            WebBrowserTabPage page = new WebBrowserTabPage();
            tabControl.TabPages.Add(page);
            currentPage = page;

            RegisterBrowserEvents();

            // tabcontrol events
            tabControl.SelectedIndexChanged += (s, e) => 
            {
                if (currentPage != null)
                    UnregisterBrowserEvents();
                currentPage = (WebBrowserTabPage)tabControl.SelectedTab;
                if (currentPage != null)
                {
                    RegisterBrowserEvents();
                    if (currentPage.browser.Url != null)
                        navigationBar.UrlText = currentPage.browser.Url.ToString();
                    else
                        navigationBar.UrlText = "";

                    this.Text = "WebKit Browser Example - " + currentPage.browser.DocumentTitle;

                    currentPage.browser.Focus();
                }
            };

            // navigation bar events
            navigationBar.Back += () => { currentPage.browser.GoBack(); ActivateBrowser(); };
            navigationBar.Forward += () => { currentPage.browser.GoForward(); ActivateBrowser(); };
            navigationBar.Go += () => { currentPage.browser.Navigate(navigationBar.UrlText); ActivateBrowser(); };
            navigationBar.Refresh += () => { currentPage.browser.Reload(); ActivateBrowser(); };
            navigationBar.Stop += () => { currentPage.Stop(); ActivateBrowser(); };
        }

        private void RegisterBrowserEvents()
        {
            currentPage.browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(browser_DocumentCompleted);
            currentPage.browser.Navigated += new WebBrowserNavigatedEventHandler(browser_Navigated);
            currentPage.browser.DocumentTitleChanged += new EventHandler(browser_DocumentTitleChanged);
            currentPage.browser.Error += new WebKit.WebKitBrowserErrorEventHandler(browser_Error);
            currentPage.browser.DownloadBegin += new FileDownloadBeginEventHandler(browser_DownloadBegin);
            currentPage.browser.NewWindowRequest += new NewWindowRequestEventHandler(browser_NewWindowRequest);
            currentPage.browser.NewWindowCreated += new NewWindowCreatedEventHandler(browser_NewWindowCreated);
        }

        void browser_NewWindowCreated(object sender, NewWindowCreatedEventArgs args)
        {
            tabControl.TabPages.Add(new WebBrowserTabPage(args.WebKitBrowser, false));
        }

        void browser_NewWindowRequest(object sender, NewWindowRequestEventArgs args)
        {
            args.Cancel = (MessageBox.Show(args.Url, "Open new window?", MessageBoxButtons.YesNo) == DialogResult.No);
        }

        void browser_DownloadBegin(object sender, FileDownloadBeginEventArgs args)
        {
            DownloadForm frm = new DownloadForm(args.Download);
        }

        void browser_Error(object sender, WebKitBrowserErrorEventArgs args)
        {
            if (currentPage != null)
                currentPage.browser.DocumentText = "<html><head><title>Error</title></head><center><p>" + args.Description + "</p></center></html>";
        }

        void browser_DocumentTitleChanged(object sender, EventArgs e)
        {
            this.Text = "WebKit Browser Example - " + currentPage.browser.DocumentTitle;
        }

        void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            navigationBar.UrlText = currentPage.browser.Url.ToString();            
        }

        void browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            navigationBar.UrlText = currentPage.browser.Url.ToString();

            navigationBar.CanGoBack = currentPage.browser.CanGoBack;
            navigationBar.CanGoForward = currentPage.browser.CanGoForward;
        }

        private void UnregisterBrowserEvents()
        {
            currentPage.browser.DocumentCompleted -= browser_DocumentCompleted;
            currentPage.browser.Navigated -= browser_Navigated;
            currentPage.browser.DocumentTitleChanged -= browser_DocumentTitleChanged;
            currentPage.browser.NewWindowCreated -= browser_NewWindowCreated;
            currentPage.browser.NewWindowRequest -= browser_NewWindowRequest;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WebBrowserTabPage page = new WebBrowserTabPage();
            tabControl.TabPages.Add(page);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("www.sourceforge.net/projects/webkitdotnet\n\nWebKitBrowser version " + currentPage.browser.Version, "About WebKit.NET");
        }

        private void pageSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new SourceViewForm(currentPage.browser.DocumentText, currentPage.browser)).Show();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPage.browser.SelectedText != null)
                    Clipboard.SetText(currentPage.browser.SelectedText);
            }
            catch (Exception)
            {
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            ActivateBrowser();
        }

        private void ActivateBrowser()
        {
            if (currentPage.browser.CanFocus)
                currentPage.browser.Focus();
        }

        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage page = currentPage;
            tabControl.Controls.Remove(page);
            page.Dispose();

            if (tabControl.Controls.Count == 0)
                Application.Exit();
        }

        private void testPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPage.browser.DocumentText =
                "<html><head><title>Test Page</title></head><body>" +
                "<p id=\"testelement\" style=\"color: red\">Hello, World!</p>" +
                "<div><p>A</p><p>B</p><p>C</p></div>" +
                "<script type=\"text/javascript\">" + 
                "function f() { window.open('http://www.google.com', 'myWindow'); }</script>" + 
                "</body></html>";
        }

        private void tToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPage.browser.StringByEvaluatingJavaScriptFromString("f()");
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPage.browser.ShowPrintDialog();
        }

        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPage.browser.ShowPageSetupDialog();
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This is likely to cause a crash. Continue?",
                "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var thread = new Thread(new ThreadStart(MyThread));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        private void MyThread()
        {
            Application.Run(new MainForm());
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPage.browser.ShowPrintPreviewDialog();
        }

        private void printImmediatelyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPage.browser.Print();
        }
    }
}
