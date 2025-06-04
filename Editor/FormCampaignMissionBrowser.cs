using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormCampaignMissionBrowser : Form
    {
        public string CampaignsPath;
        public string SelectedCampaign;
        public string SelectedMission;
        private string[] CampaignsFolderPaths;
        private string[] MissionFullPaths;

        public FormCampaignMissionBrowser()
        {
            InitializeComponent();


        }

        private void FormCampaignMissionBrowser_Load(object sender, EventArgs e)
        {

        }

        private void FormCampaignMissionBrowser_Shown(object sender, EventArgs e)
        {
            // todo: i think its ok to use campaign folder names stored on the client
            //       but available missions should be retreived from the server during play mode (as opposed to edit mode)
            PopulateCampaigns();
        }

        private void PopulateCampaigns()
        {
            CampaignsFolderPaths = System.IO.Directory.GetDirectories(CampaignsPath);

            if (CampaignsFolderPaths == null)
            {
                listCampaigns.Items.Add("(no campaigns found)");
                SelectedMission = null;
                return;
            }

            for (int i = 0; i < CampaignsFolderPaths.Length; i++)
            {
                string campaignFolderName = new System.IO.DirectoryInfo(CampaignsFolderPaths[i]).Name;
                listCampaigns.Items.Add(campaignFolderName);

                // select the first campaign directory and populate the missions list
                if (i == 0)
                {
                    listCampaigns.SelectedIndex = 0;
                    string missionsPath = System.IO.Path.Combine(CampaignsFolderPaths[i], "missions");
                    PopulateMissions(missionsPath);
                }
            }
        }

        private void PopulateMissions(string missionsPath)
        {
            listMissions.Items.Clear();
            SelectedMission = null;
            if (!System.IO.Directory.Exists(missionsPath))
            {
                listMissions.Items.Add("(no missions found)");
                return;
            }

            // search for all files with signature "missionNN.xml"
            MissionFullPaths = System.IO.Directory.GetFiles(missionsPath, "mission*.xml");

            if (MissionFullPaths == null)
            {
                listMissions.Items.Add("(no missions found)");
                return;
            }

            for (int i = 0; i < MissionFullPaths.Length; i++)
            {
                // open each mission and retreive the friendly mission name
                Settings.Initialization missionData = Settings.Initialization.Load(MissionFullPaths[i]);
                string friendlyName = missionData.settingRead("info", "name");
                listMissions.Items.Add(i + 1 + " - " + friendlyName);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listMissions_SelectedIndexChanged(object sender, EventArgs e)
        {
            int missionIndex = listMissions.SelectedIndex;
            SelectedMission =  new System.IO.DirectoryInfo(MissionFullPaths[missionIndex]).Name; 
        }

        private void listCampaigns_SelectedIndexChanged(object sender, EventArgs e)
        {
            int campaignIndex = listCampaigns.SelectedIndex;
            SelectedCampaign = new System.IO.DirectoryInfo(CampaignsFolderPaths[campaignIndex]).Name; 
            string missionsPath = System.IO.Path.Combine(SelectedCampaign, "missions");
            PopulateMissions(missionsPath);
        }
    }
}
