//using System;
//using System.Collections.Generic;
//using Lidgren.Network;
//using KeyCommon.Commands;
//using KeyCommon.Entities;
//using System.Diagnostics;
//using DevComponents.DotNetBar;

//namespace KeyEdit.Workspaces
//{
//    /// <summary>
//    /// Like all classes that implement IVIew, the LobbyView is responsible for tracking the state of all aspects of this view
//    /// so that switching to from this view to any other will always result in the view appearing just as it did when the user
//    /// left it.  
//    /// One key point about how views works to accomodate the above is that a view is only beholden to itself.  Thus it will never
//    /// be affected by the manipulation of a dockpanel or floating window that was moved.  So in this respect
//    /// a view doesnt share any docks or floating windows with any other view.  It may however share a particular control instance
//    /// that sits on a view so that for instance, switching from a tactical combat view to some other 3d view will always share
//    /// the primary viewport.
//    /// </summary>
//    public class LobbyView : BaseView 
//    {

//        // controls
//        LobbyControl mLobby;  // could rename this LobbyDisplay 
//        //Messenger mMessenger; // IM client
//        // TableDisplay 


//// ViewManager
//        //      LobbyView : AppView <-- manages the docking and position restoration and state tracking so switching back/forth always returns to last state
//        //          Lobby : UserControl 
//        //          Buddy\User list : UserControl
//        //          Table : UserControl  <-- has the chat in it
//        //      GameView 
//        //      EditView


//        // we're going to alter things.  In my game you will only get join/left messages from users
//        // in your buddy list.  Otherwise it'll potentially be too many.  Now, perhaps
//        // we can have it so initially everyone is always in everyone's buddy list, but otherwise passing
//        // these huge lists to everybody who joins is not practical.  We'll also impose a buddy limit max of perhaps 256 or 1024.

//        // AND ->  The Buddy List feature is more like a real IM in that it doesnt matter what you're doing in the game
//        //  you are connected to your buddy list no matter what.  
//        //  For now we will just make the lobby that buddy list maintainer as well... but we might spin that off to a seperate app.

//        private Dictionary<long, string> mUsers;
//        private Dictionary<long, KeyCommon.Entities.Table> mTables;
//        private KeyCommon.Entities.Table mRegisteredAtTable;
//        KeyEdit.Network.LobbyNetManager mLobbyManager;


//        public LobbyView(string name, string definitionpath, string layoutfilepath, KeyEdit.Network.LobbyNetManager lobbyManager, DotNetBarManager barManager, ViewManager vm)
//            : base(name, definitionpath, layoutfilepath, barManager,vm)
//        {
//            if (lobbyManager == null) throw new ArgumentNullException();

//            mLobbyManager = lobbyManager;
//            mLobbyManager.UserJoined += OnUserJoined;
//            mLobbyManager.UserLeft += OnUserLeft ;
//            mLobbyManager.TableAdded += OnTableStatusChanged;
//            mLobbyManager.TableRemoved += OnTableStatusChanged;
//            //mLobbyManager.UnRegisterTable += 
//            //mLobbyManager.RegisterTable += 

//            mLobby = new LobbyControl();
//            mHideDocumentTabs = false;
//            mHideRibbonTabItems = false;
//            mHideRibbonTabGroups = true;
//        }


//        #region IView Members
//        // so i know how Bar's work... and they can host DockContainerItem which then in turn
//        // can host a particular control such as our Lobby control
//        // all we have to do 
//        public override string DefaultLayout { get { return KeyEdit.Properties.Resources.lobby_layout; } }
//        public override string DefaultDefinition { get { return KeyEdit.Properties.Resources.lobby_definition; } }

//        // mBarManager layout loading handles the bars and DockContainerItems, but we're responsible
//        // for finding the right DockContainerItem and removing the correct controls
//        public override void Hide()
//        {
//            // remove this control from the DockContainerItem.Control 
//            DockContainerItem container = FindDockContainer("docDock1");
//            Debug.Assert(container.Control == mLobby);
//            container.Control = null;
//            mLobby.Visible = false;
//        }

//        public override void Show(bool loadLayout)
//        {
//            base.Show(loadLayout);
//            // our UserControls/panelDockContents are always visible
//            // but our bars and DockContainerItems are not.  So
//            // here before showing, we recreate the bars and dockContents 
//            // Further, we have to track the events when a user drags/drops/tears/creates new bar/etc
//            // so that we can always have those arrays of bars and dockcontents
//            // All we ever have to do is track these bars and make sure they have the same names
//            // used in the layout.config files.  Then during Show we just recreate them with those names
//            // and then re-load the layoutconfig
//            // So this is what we will test.  For our expieriment
//            // We need to create a new Layout file that only has the Lobby control in the document
//            // with none of the side windows.

//            DockContainerItem container = FindDockContainer("docDock1");
//            Debug.Assert(container.Control != mLobby);
//            container.Control = mLobby;
//            mLobby.Visible = true;
//        }
//        #endregion

//        // for lists, internally i could iterate thru the list and just call this single event
//        private void OnUserJoined(IRemotableType type)
//        {
//            UserStatusChanged status = (UserStatusChanged)type;

//            ConsoleMessage("* " + status.UserName + " has joined the lobby.");
//         //   lstLobbyUsers.Items.Add(status.UserName);
//            mUsers.Add(status.UserID, status.UserName);
                        
//            //// TODO: below is actually for UserList
//            //switch (userList.Scope)
//            //{
//            //    case KeyCommon.Messages.Enumerations.Scope.Global:
//            //    case KeyCommon.Messages.Enumerations.Scope.Local:
//            //        if (userList.Users != null)
//            //        {
//            //            foreach (int key in userList.Users.Keys)
//            //            {
//            //                if (userList.Scope == KeyCommon.Messages.Enumerations.Scope.Global)
//            //                {
//            //                    lstLobbyUsers.Items.Add(userList.Users[key]);
//            //                    mUsers.Add(key, userList.Users[key]);
//            //                    if (userList.Users[key] == UserName)
//            //                    {
//            //                        //mClient.UserID = key;
//            //                    }
//            //                }
//            //                else if (userList.Scope == KeyCommon.Messages.Enumerations.Scope.Local)
//            //                {
//            //                    //  checkListTableUsers.Items.Add(userList.Users[key]);
//            //                }
//            //            }
//            //        }
//            //        break;
//            //    default:
//            //        Debug.Assert(false, "Unexpected scope");
//            //        break;
//            //}
//            //ConsoleMessage("Received user list...");
//        }

//        private void OnUserLeft(IRemotableType type)
//        {
//            UserStatusChanged status = (UserStatusChanged)type;
//            ConsoleMessage("* " + status.UserName + " has left the lobby.");
//      //      int index = lstLobbyUsers.FindStringExact(status.UserName);
//      //      lstLobbyUsers.Items.RemoveAt(index);
//       //     lstLobbyUsers.Items.Remove(status.UserName);
//            mUsers.Remove(status.UserID);
//        }


//        private void OnTableStatusChanged(IRemotableType type)
//        {
//            TableStatusChanged status = (TableStatusChanged)type;

//            // added
//           // mLobby.Add(status.Table.PrimaryKey.ToString(), status.Table.Name, 0); 
//            // listviewGames.Items.Add(status.Table.PrimaryKey.ToString(), status.Table.Name, 0);
//            mTables.Add(status.Table.PrimaryKey, status.Table);


//            // removed
//          //  mLobby.Remove(status.Table.PrimaryKey.ToString()); 
//          //  listviewGames.Items.RemoveByKey(status.Table.PrimaryKey.ToString());
//            mTables.Remove(status.Table.PrimaryKey);

//            // table parameter changed
//            ConsoleMessage("Table parameter changed...");

//        }

//        // rather than call this, i could iterate thru the received list and simply call the above event instead
//        private void OnTableListReceived(IRemotableType type)
//        { 
//        }

//        private void OnChatReceived(IRemotableType type)
//        {
//            ChatMessage message = (ChatMessage)type;

//            string userName = mUsers[message.SenderID];
//            if (message.Scope == KeyCommon.Messages.Enumerations.Scope.Global)
//            {
//                ConsoleMessage("<" + userName + " >" + message.Content);
//            }
//            else if (message.Scope == KeyCommon.Messages.Enumerations.Scope.Local)
//            {
//                TableChatMessage("<" + userName + " >" + message.Content);
//            }
//            //  note that we respond to server events as far as client side actions and NOT by our
//            //  own initiation of those actions.  The reason is because we're using a dumb client model 
//            //  now we "could" throw up a tiny "Creating Table..."" dialog and have it time out on it's own
//            //  or be dismissed when we get the event from the server in order to provide feedback to the user
//        }

//        private void OnRegisterTable(IRemotableType type)
//        {
//            CreateTable registration = (CreateTable)type;

//            if (mRegisteredAtTable == null)
//            {
//                //  this must mean that the first userID of this registration must be our own since this has to be a confirmation "RegisterTable" notice
//                Debug.Assert(registration.UserID == mLobbyManager.UserID);
//                mRegisteredAtTable = mTables[registration.TableID];
//                ////  this table should already exist
//                //tabCtrlMain.TabPages.Add(tabTable);
//                //tabTable.Text = ("Table - " + registration.TableID.ToString());
//                //checkListTableUsers.Items.Clear();
//            }
//            else
//            {
//                //checkListTableUsers.Items.Add(mUsers[register.UserID]);
//            }
//            mRegisteredAtTable.AddUser(registration.UserID, mUsers[registration.UserID]);
//            ConsoleMessage("User has registered to our table...");
//        }

//        private void OnUnRegisterTable(IRemotableType type)
//        {
//            TableRegistration registration = (TableRegistration)type;
//            mTables[registration.TableID].RemoveUser(registration.UserID);
//            //  if mRegisteredAtTable is nothing, the user Unregistering is probably us in which case we already nulled mRegisteredAtTable
//            if (mRegisteredAtTable == null)
//            {
//                Debug.Assert(mRegisteredAtTable.PrimaryKey == registration.TableID);
//                return;
//            }
//            //checkListTableUsers.Items.Remove(mUsers[registration.UserID]);
//            ConsoleMessage("User has unRegistered from our table...");
//        }

//        private void OnReadyStatusChange(IRemotableType type)
//        {
//            //int userIndex = checkListTableUsers.Items.IndexOf(mUsers[readystatus.UserID]);
//            //checkListTableUsers.ClearSelected();
//            // checkListTableUsers.SetItemChecked(userIndex, readystatus.IsReady);
//            ConsoleMessage("User Ready Status changed...");
//        }

//        private void OnCreateGame(IRemotableType type)
//        {
//            CreateGame create = (CreateGame)type;

//            //  the CreateGame packet contains both the Game object and our AuthenticatedLogin

//            ////  store this game as our profile's current game
//            //mCurrentProfile.Current.CurrentGame = create.Game.mID;
//            //mCurrentProfile.Current.HostEndpoint = create.Game.mHost.EndPoints(0);
//            //// mCurrentProfile.Current.HostAddress = create.mGame.mHost.IP
//            //// mCurrentProfile.Current.HostPort = create.mGame.mHost.Port
//            //mCurrentProfile.Current.HostName = create.Game.mHost.Name;
//            //mCurrentProfile.Current.StartDate = DateTime.Now();
//            //mCurrentProfile.Current.SetTicketBytes(createGame.Data);
//            //mINI.Save();

//            // TODO: this should invoke the Lobby.cs callback for creating a game which then should create a 
//            // GameManager instance in AppMain.GameManager
//        //    Connect(create.Game.PrimaryKey, create.Game.mHost, create.Data);
//        }


//        //private void ListViewTables_Click(object sender, System.EventArgs e)
//        //{
//        //    //  if we're already at a table, ignore
//        //    if (mRegisteredAtTable != null)
//        //    {
//        //        ListView listview = (ListView)sender;
//        //        if (listview.SelectedItems != null)
//        //        {
//        //            Debug.Assert(listview.SelectedItems.Count == 1);
//        //            //  we dont want multi-select working and hopefully > 0 is not possible here
//        //            ListViewItem item = listview.SelectedItems[0];
//        //            //  send registerTable request
//        //            KeyCommon.Commands.TableRegistration register = new KeyCommon.Commands.TableRegistration((int)KeyCommon.Messages.Enumerations.Commands.RegisterTable);
//        //            register.UserID = mClient.UserID;
//        //            register.TableID = int.Parse(item.Name);
//        //            mLobbyManager.SendCommand(register);
//        //            ConsoleMessage(string.Format("listview table id {0} named {1} clicked.", item.Name, item.Text));
//        //        }
//        //    }
//        //}

//        //void LeaveTable()
//        //{
//        //    //  send unregisterTable 
//        //    KeyCommon.Commands.TableRegistration unregister = new KeyCommon.Commands.TableRegistration((int)KeyCommon.Messages.Enumerations.Commands.UnRegisterTable);
//        //    unregister.TableID = mRegisteredAtTable.TableID;
//        //    unregister.UserID = mClient.UserID;
//        //    mRegisteredAtTable = null;
//        //    mLobbyManager.SendCommand(unregister);
//        //}

//        ////case KeyCommon.Messages.Enumerations.Commands.Enumerations.MapSpec:
//        ////            KeyCommon.Commands.MapSpec tileSpec = new KeyCommon.Commands.MapSpec();
//        ////            tileSpec.Read(message.Buffer);
//        ////            ProcessNewTiles(tileSpec);
//        ////            break;
//        ////        case KeyCommon.Messages.Enumerations.Commands.FileDownloadAuthorization:
//        ////            KeyCommon.Commands.FileDownloadAuthorization fileauth = new KeyCommon.Commands.FileDownloadAuthorization();
//        ////            fileauth.Read(message.Buffer);
//        ////            DownloadAuthorizedFiles(fileauth);
//        ////            break;

//        private void ConsoleMessage(string msg)
//        {
//            //string NewContent;
//            //string timeStamp = DateTime.Now.ToString("hh:mm ss tt");
//            //NewContent = "[" + timeStamp + "]  " + msg + Environment.NewLine;
//            //rtbConsole.Text = rtbConsole.Text + NewContent;
//            //rtbConsole.SelectionStart = rtbConsole.Text.Length;
//        }

//        private void TableChatMessage(string msg)
//        {
//            //string NewContent;
//            //string timeStamp = DateTime.Now.ToString("hh:mm ss tt");
//            //NewContent = "[" + timeStamp + "]  " + msg + Environment.NewLine;
//            //rtbTableChat.Text = rtbTableChat.Text + NewContent;
//            //rtbTableChat.SelectionStart = rtbTableChat.Text.Length;
//        }
//    }
//}
