
    partial class AdsSampleServerTester
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._buttonConnect = new System.Windows.Forms.Button();
            this._buttonDisconnect = new System.Windows.Forms.Button();
            this._loggerListbox = new System.Windows.Forms.ListBox();
            this._ReadDevInfoButton = new System.Windows.Forms.Button();
            this._writeControlButton = new System.Windows.Forms.Button();
            this._readButton = new System.Windows.Forms.Button();
            this._readStateButton = new System.Windows.Forms.Button();
            this._writeButton = new System.Windows.Forms.Button();
            this._readWriteButton = new System.Windows.Forms.Button();
            this._addNotButton = new System.Windows.Forms.Button();
            this._delNotButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _buttonConnect
            // 
            this._buttonConnect.Location = new System.Drawing.Point(12, 12);
            this._buttonConnect.Name = "_buttonConnect";
            this._buttonConnect.Size = new System.Drawing.Size(160, 40);
            this._buttonConnect.TabIndex = 0;
            this._buttonConnect.Text = "Connect";
            this._buttonConnect.Click += new System.EventHandler(this.OnConnectClicked);
            // 
            // _buttonDisconnect
            // 
            this._buttonDisconnect.Location = new System.Drawing.Point(12, 58);
            this._buttonDisconnect.Name = "_buttonDisconnect";
            this._buttonDisconnect.Size = new System.Drawing.Size(160, 40);
            this._buttonDisconnect.TabIndex = 2;
            this._buttonDisconnect.Text = "Disconnect";
            this._buttonDisconnect.Click += new System.EventHandler(this.OnDisconnectClicked);
            // 
            // _loggerListbox
            // 
            this._loggerListbox.ItemHeight = 25;
            this._loggerListbox.Location = new System.Drawing.Point(178, 12);
            this._loggerListbox.Name = "_loggerListbox";
            this._loggerListbox.Size = new System.Drawing.Size(851, 454);
            this._loggerListbox.TabIndex = 3;
            // 
            // _ReadDevInfoButton
            // 
            this._ReadDevInfoButton.Location = new System.Drawing.Point(12, 104);
            this._ReadDevInfoButton.Name = "_ReadDevInfoButton";
            this._ReadDevInfoButton.Size = new System.Drawing.Size(160, 40);
            this._ReadDevInfoButton.TabIndex = 4;
            this._ReadDevInfoButton.Text = "Dev Info";
            this._ReadDevInfoButton.Click += new System.EventHandler(this.OnReadDevInfoClicked);
            // 
            // _writeControlButton
            // 
            this._writeControlButton.Location = new System.Drawing.Point(12, 380);
            this._writeControlButton.Name = "_writeControlButton";
            this._writeControlButton.Size = new System.Drawing.Size(160, 40);
            this._writeControlButton.TabIndex = 5;
            this._writeControlButton.Text = "Write Control";
            this._writeControlButton.Click += new System.EventHandler(this.OnWriteControlClicked);
            // 
            // _readButton
            // 
            this._readButton.Location = new System.Drawing.Point(12, 150);
            this._readButton.Name = "_readButton";
            this._readButton.Size = new System.Drawing.Size(160, 40);
            this._readButton.TabIndex = 5;
            this._readButton.Text = "Read";
            this._readButton.Click += new System.EventHandler(this.OnReadClicked);
            // 
            // _readStateButton
            // 
            this._readStateButton.Location = new System.Drawing.Point(12, 242);
            this._readStateButton.Name = "_readStateButton";
            this._readStateButton.Size = new System.Drawing.Size(160, 40);
            this._readStateButton.TabIndex = 6;
            this._readStateButton.Text = "Read State";
            this._readStateButton.Click += new System.EventHandler(this.OnReadStateClicked);
            // 
            // _writeButton
            // 
            this._writeButton.Location = new System.Drawing.Point(12, 196);
            this._writeButton.Name = "_writeButton";
            this._writeButton.Size = new System.Drawing.Size(160, 40);
            this._writeButton.TabIndex = 6;
            this._writeButton.Text = "Write";
            this._writeButton.Click += new System.EventHandler(this.OnWriteClicked);
            // 
            // _readWriteButton
            // 
            this._readWriteButton.Location = new System.Drawing.Point(12, 426);
            this._readWriteButton.Name = "_readWriteButton";
            this._readWriteButton.Size = new System.Drawing.Size(160, 40);
            this._readWriteButton.TabIndex = 7;
            this._readWriteButton.Text = "Read Write";
            this._readWriteButton.Click += new System.EventHandler(this.OnReadWriteClicked);
            // 
            // _addNotButton
            // 
            this._addNotButton.Location = new System.Drawing.Point(12, 288);
            this._addNotButton.Name = "_addNotButton";
            this._addNotButton.Size = new System.Drawing.Size(160, 40);
            this._addNotButton.TabIndex = 8;
            this._addNotButton.Text = "Add Notification";
            this._addNotButton.Click += new System.EventHandler(this.OnAddNotificationClicked);
            // 
            // _delNotButton
            // 
            this._delNotButton.Location = new System.Drawing.Point(12, 334);
            this._delNotButton.Name = "_delNotButton";
            this._delNotButton.Size = new System.Drawing.Size(160, 40);
            this._delNotButton.TabIndex = 9;
            this._delNotButton.Text = "Del Notification";
            this._delNotButton.Click += new System.EventHandler(this.OnDeleteNotificationClicked);
            // 
            // AdsSampleServerTester
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(1041, 484);
            this.Controls.Add(this._delNotButton);
            this.Controls.Add(this._addNotButton);
            this.Controls.Add(this._readWriteButton);
            this.Controls.Add(this._writeButton);
            this.Controls.Add(this._readStateButton);
            this.Controls.Add(this._readButton);
            this.Controls.Add(this._writeControlButton);
            this.Controls.Add(this._ReadDevInfoButton);
            this.Controls.Add(this._loggerListbox);
            this.Controls.Add(this._buttonDisconnect);
            this.Controls.Add(this._buttonConnect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AdsSampleServerTester";
            this.Text = "SampleAdsServer";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AdsSampleServerTester_Closing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _buttonConnect;
        private System.Windows.Forms.Button _buttonDisconnect;
        private System.Windows.Forms.ListBox _loggerListbox;
        private System.Windows.Forms.Button _ReadDevInfoButton;
        private System.Windows.Forms.Button _writeControlButton;
        private System.Windows.Forms.Button _readButton;
        private System.Windows.Forms.Button _readStateButton;
        private System.Windows.Forms.Button _writeButton;
        private System.Windows.Forms.Button _readWriteButton;
        private System.Windows.Forms.Button _addNotButton;
        private System.Windows.Forms.Button _delNotButton;
}
