namespace KeyEdit.GUI
{
    partial class EntityBrowser
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EntityBrowser));
            this.ribbonBar1 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer1 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem3 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem2 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem5 = new DevComponents.DotNetBar.ButtonItem();
            this.advTree1 = new DevComponents.AdvTree.AdvTree();
            this.nodeConnector1 = new DevComponents.AdvTree.NodeConnector();
            this.elementStyle1 = new DevComponents.DotNetBar.ElementStyle();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.advTree1)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonBar1
            // 
            this.ribbonBar1.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar1.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar1.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar1.BackgroundStyle.Class = "";
            this.ribbonBar1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar1.ContainerControlProcessDialogKey = true;
            this.ribbonBar1.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbonBar1.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer1});
            this.ribbonBar1.Location = new System.Drawing.Point(0, 0);
            this.ribbonBar1.Name = "ribbonBar1";
            this.ribbonBar1.Size = new System.Drawing.Size(285, 27);
            this.ribbonBar1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar1.TabIndex = 0;
            this.ribbonBar1.Text = "ribbonBar1";
            // 
            // 
            // 
            this.ribbonBar1.TitleStyle.Class = "";
            this.ribbonBar1.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar1.TitleStyleMouseOver.Class = "";
            this.ribbonBar1.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar1.TitleVisible = false;
            // 
            // itemContainer1
            // 
            // 
            // 
            // 
            this.itemContainer1.BackgroundStyle.Class = "";
            this.itemContainer1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer1.Name = "itemContainer1";
            this.itemContainer1.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem1,
            this.buttonItem4,
            this.buttonItem3,
            this.buttonItem2,
            this.buttonItem5});
            // 
            // buttonItem1
            // 
            this.buttonItem1.Image = global::KeyEdit.Properties.Resources.sun_icon_16;
            this.buttonItem1.Name = "buttonItem1";
            this.buttonItem1.Text = "buttonItem1";
            // 
            // buttonItem4
            // 
            this.buttonItem4.Image = global::KeyEdit.Properties.Resources.globe_2_icon_16;
            this.buttonItem4.Name = "buttonItem4";
            this.buttonItem4.Text = "buttonItem4";
            // 
            // buttonItem3
            // 
            this.buttonItem3.Image = global::KeyEdit.Properties.Resources.users_icon_161;
            this.buttonItem3.Name = "buttonItem3";
            this.buttonItem3.Text = "buttonItem3";
            // 
            // buttonItem2
            // 
            this.buttonItem2.Image = global::KeyEdit.Properties.Resources.target_icon_16;
            this.buttonItem2.Name = "buttonItem2";
            this.buttonItem2.Text = "buttonItem2";
            // 
            // buttonItem5
            // 
            this.buttonItem5.Image = global::KeyEdit.Properties.Resources.track_icon_16;
            this.buttonItem5.Name = "buttonItem5";
            this.buttonItem5.Text = "buttonItem5";
            // 
            // advTree1
            // 
            this.advTree1.AccessibleRole = System.Windows.Forms.AccessibleRole.Outline;
            this.advTree1.AllowDrop = true;
            this.advTree1.BackColor = System.Drawing.SystemColors.Window;
            // 
            // 
            // 
            this.advTree1.BackgroundStyle.Class = "TreeBorderKey";
            this.advTree1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.advTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.advTree1.ImageList = this.imageList1;
            this.advTree1.Location = new System.Drawing.Point(0, 27);
            this.advTree1.Name = "advTree1";
            this.advTree1.NodesConnector = this.nodeConnector1;
            this.advTree1.NodeStyle = this.elementStyle1;
            this.advTree1.PathSeparator = ";";
            this.advTree1.Size = new System.Drawing.Size(285, 872);
            this.advTree1.Styles.Add(this.elementStyle1);
            this.advTree1.TabIndex = 1;
            this.advTree1.Text = "advTree1";
            // 
            // nodeConnector1
            // 
            this.nodeConnector1.LineColor = System.Drawing.SystemColors.ControlText;
            // 
            // elementStyle1
            // 
            this.elementStyle1.Class = "";
            this.elementStyle1.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.elementStyle1.Name = "elementStyle1";
            this.elementStyle1.TextColor = System.Drawing.SystemColors.ControlText;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "2x2_grid_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(1, "3x3_grid_2_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(2, "3x3_grid_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(3, "air_signal_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(4, "align_center_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(5, "align_just_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(6, "align_left_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(7, "align_right_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(8, "app_window&16 - Copy.png");
            this.imageList1.Images.SetKeyName(9, "app_window_black&16 - Copy.png");
            this.imageList1.Images.SetKeyName(10, "app_window_black_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(11, "app_window_cross&16 - Copy.png");
            this.imageList1.Images.SetKeyName(12, "app_window_cross_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(13, "app_window_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(14, "app_window_shell&16 - Copy.png");
            this.imageList1.Images.SetKeyName(15, "app_window_shell_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(16, "arrow_bottom_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(17, "arrow_bottom_left_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(18, "arrow_bottom_rigth_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(19, "arrow_l_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(20, "arrow_left_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(21, "arrow_r_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(22, "arrow_right_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(23, "arrow_top_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(24, "arrow_top_left_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(25, "arrow_top_right_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(26, "arrow_two_head_2_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(27, "arrow_two_head_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(28, "attention_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(29, "balance_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(30, "book_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(31, "bookmark_2_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(32, "br_next_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(33, "brackets_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(34, "bug_icon&16 - Copy.png");
            this.imageList1.Images.SetKeyName(35, "cancel_icon&16.png");
            this.imageList1.Images.SetKeyName(36, "case_icon&16.png");
            this.imageList1.Images.SetKeyName(37, "cassette_icon&16.png");
            this.imageList1.Images.SetKeyName(38, "cc_icon&16.png");
            this.imageList1.Images.SetKeyName(39, "cert_icon&16.png");
            this.imageList1.Images.SetKeyName(40, "chart_bar_icon&16.png");
            this.imageList1.Images.SetKeyName(41, "chart_line_2_icon&16.png");
            this.imageList1.Images.SetKeyName(42, "chart_line_icon&16.png");
            this.imageList1.Images.SetKeyName(43, "chart_pie_icon&16.png");
            this.imageList1.Images.SetKeyName(44, "chat_bubble_message_square_icon&16.png");
            this.imageList1.Images.SetKeyName(45, "checkbox_checked_icon&16.png");
            this.imageList1.Images.SetKeyName(46, "checkbox_unchecked_icon&16.png");
            this.imageList1.Images.SetKeyName(47, "checkmark_icon&16.png");
            this.imageList1.Images.SetKeyName(48, "clip_icon&16.png");
            this.imageList1.Images.SetKeyName(49, "clipboard_copy_icon&16.png");
            this.imageList1.Images.SetKeyName(50, "clipboard_cut_icon&16.png");
            this.imageList1.Images.SetKeyName(51, "clipboard_past_icon&16.png");
            this.imageList1.Images.SetKeyName(52, "clock_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(53, "clock_icon&16.png");
            this.imageList1.Images.SetKeyName(54, "cloud_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(55, "coffe_cup_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(56, "cog_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(57, "comp_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(58, "compass_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(59, "contact_card_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(60, "contact_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(61, "cur_bp_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(62, "cur_dollar_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(63, "cur_yen_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(64, "cursor_arrow_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(65, "cursor_arrow_icon&16 - Copy (2).png");
            this.imageList1.Images.SetKeyName(66, "cursor_drag_arrow_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(67, "cursor_hand_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(68, "db_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(69, "disconnected_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(70, "doc_delete_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(71, "doc_edit_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(72, "doc_empty_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(73, "doc_export_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(74, "doc_import_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(75, "doc_lines_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(76, "doc_lines_stright_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(77, "doc_minus_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(78, "doc_new_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(79, "doc_plus_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(80, "document_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(81, "download_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(82, "eject_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(83, "emotion_sad_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(84, "emotion_smile_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(85, "expand_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(86, "export_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(87, "eye_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(88, "eye_inv_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(89, "facebook_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(90, "fastforward_next_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(91, "fill_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(92, "filter_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(93, "fire_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(94, "flag_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(95, "flag_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(96, "folder_arrow_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(97, "folder_delete_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(98, "folder_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(99, "folder_minus_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(100, "folder_open_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(101, "folder_plus_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(102, "font_bold_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(103, "font_italic_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(104, "font_size_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(105, "font_strokethrough_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(106, "font_underline_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(107, "game_pad_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(108, "glasses_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(109, "globe_1_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(110, "globe_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(111, "globe_3_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(112, "google_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(113, "hand_1_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(114, "hand_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(115, "hand_contra_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(116, "hand_pro_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(117, "hanger_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(118, "headphones_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(119, "heart_empty_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(120, "heart_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(121, "home_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(122, "image_text_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(123, "import_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(124, "inbox_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(125, "indent_decrease_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(126, "indent_increase_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(127, "info_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(128, "inject_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(129, "invisible_light_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(130, "invisible_revert_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(131, "iphone_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(132, "key_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(133, "layers_1_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(134, "layers_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(135, "lightbulb_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(136, "lighting_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(137, "link_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(138, "list_bullets_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(139, "list_num_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(140, "loading_throbber_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(141, "lock_open_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(142, "magic_wand_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(143, "magic_wand_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(144, "mail_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(145, "mail_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(146, "message_attention_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(147, "mic_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(148, "microphone_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(149, "money_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(150, "monitor_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(151, "movie_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(152, "music_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(153, "music_square_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(154, "net_comp_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(155, "network_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(156, "not_connected_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(157, "notepad_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(158, "notepad_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(159, "off_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(160, "on_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(161, "on-off_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(162, "openid_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(163, "padlock_closed_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(164, "padlock_open_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(165, "page_layout_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(166, "paper_airplane_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(167, "paragraph_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(168, "pencil_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(169, "phone_1_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(170, "phone_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(171, "phone_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(172, "phone_touch_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(173, "photo_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(174, "picture_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(175, "pin_2_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(176, "pin_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(177, "pin_map_down_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(178, "pin_map_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(179, "pin_map_left_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(180, "pin_map_right_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(181, "pin_map_top_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(182, "pin_sq_down_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(183, "pin_sq_left_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(184, "pin_sq_right_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(185, "pin_sq_top_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(186, "playback_ff_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(187, "playback_next_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(188, "playback_pause_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(189, "playback_play_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(190, "playback_prev_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(191, "playback_rec_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(192, "playback_reload_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(193, "playback_rew_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(194, "playback_stop_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(195, "podcast_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(196, "preso_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(197, "print_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(198, "push_pin_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(199, "redo_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(200, "refresh_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(201, "reload_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(202, "rewind_previous_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(203, "rnd_br_down_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(204, "rnd_br_first_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(205, "rnd_br_last_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(206, "rnd_br_next_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(207, "rnd_br_prev_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(208, "rnd_br_up_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(209, "round_and_up_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(210, "round_arrow_left_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(211, "round_arrow_right_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(212, "round_checkmark_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(213, "round_delete_icon&16 - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(214, "round_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(215, "round_minus_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(216, "rss_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(217, "rss_sq_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(218, "rss_sq_icon&16 - Copy (2) - Copy.png");
            this.imageList1.Images.SetKeyName(219, "sand_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(220, "sat_dish_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(221, "save_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(222, "server_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(223, "shapes_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(224, "share_2_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(225, "share_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(226, "shield_2_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(227, "shield_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(228, "shop_cart_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(229, "shopping_bag_dollar_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(230, "shopping_bag_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(231, "sound_high_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(232, "sound_low_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(233, "sound_mute_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(234, "spechbubble_2_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(235, "spechbubble_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(236, "spechbubble_sq_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(237, "spechbubble_sq_line_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(238, "sq_br_down_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(239, "sq_br_first_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(240, "sq_br_last_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(241, "sq_br_next_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(242, "sq_br_prev_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(243, "sq_br_up_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(244, "sq_down_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(245, "sq_minus_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(246, "sq_next_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(247, "sq_plus_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(248, "sq_prev_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(249, "sq_up_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(250, "square_shape_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(251, "stairs_down_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(252, "stairs_up_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(253, "star_fav_empty_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(254, "star_fav_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(255, "star_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(256, "stop_watch_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(257, "sun_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(258, "tag_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(259, "tape_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(260, "target_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(261, "text_curstor_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(262, "text_letter_t_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(263, "top_right_expand_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(264, "track_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(265, "trash_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(266, "twitter_2_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(267, "twitter_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(268, "undo_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(269, "user_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(270, "users_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(271, "vault_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(272, "wallet_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(273, "wifi_router_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(274, "wireless_signal_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(275, "wrench_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(276, "wrench_plus_2_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(277, "wrench_plus_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(278, "youtube_icon&16 - Copy - Copy - Copy.png");
            this.imageList1.Images.SetKeyName(279, "zoom_icon&16 - Copy - Copy - Copy.png");
            // 
            // EntityBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.advTree1);
            this.Controls.Add(this.ribbonBar1);
            this.Name = "EntityBrowser";
            this.Size = new System.Drawing.Size(285, 899);
            ((System.ComponentModel.ISupportInitialize)(this.advTree1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.RibbonBar ribbonBar1;
        private DevComponents.DotNetBar.ItemContainer itemContainer1;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.ButtonItem buttonItem2;
        private DevComponents.DotNetBar.ButtonItem buttonItem3;
        private DevComponents.DotNetBar.ButtonItem buttonItem4;
        private DevComponents.DotNetBar.ButtonItem buttonItem5;
        private DevComponents.AdvTree.AdvTree advTree1;
        private DevComponents.AdvTree.NodeConnector nodeConnector1;
        private DevComponents.DotNetBar.ElementStyle elementStyle1;
        private System.Windows.Forms.ImageList imageList1;
    }
}
