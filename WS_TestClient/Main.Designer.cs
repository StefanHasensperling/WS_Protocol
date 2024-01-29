namespace WS_TestClient
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.buttonDisconnect = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.groupBoxRequest = new System.Windows.Forms.GroupBox();
            this.buttonExecute = new System.Windows.Forms.Button();
            this.comboBoxDataType = new System.Windows.Forms.ComboBox();
            this.textBoxTagValue = new System.Windows.Forms.TextBox();
            this.textBoxTagId = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxCmd = new System.Windows.Forms.ComboBox();
            this.groupBoxResponse = new System.Windows.Forms.GroupBox();
            this.textBoxResponse = new System.Windows.Forms.TextBox();
            this.backgroundWorkerRunCommand = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorkerConnect = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBoxConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).BeginInit();
            this.groupBoxRequest.SuspendLayout();
            this.groupBoxResponse.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(367, 257);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(180, 65);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBoxConnection
            // 
            this.groupBoxConnection.Controls.Add(this.buttonDisconnect);
            this.groupBoxConnection.Controls.Add(this.buttonConnect);
            this.groupBoxConnection.Controls.Add(this.label2);
            this.groupBoxConnection.Controls.Add(this.numericUpDownPort);
            this.groupBoxConnection.Controls.Add(this.label1);
            this.groupBoxConnection.Controls.Add(this.textBoxIP);
            this.groupBoxConnection.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxConnection.Location = new System.Drawing.Point(0, 0);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(559, 75);
            this.groupBoxConnection.TabIndex = 1;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "Connection";
            // 
            // buttonDisconnect
            // 
            this.buttonDisconnect.Location = new System.Drawing.Point(311, 17);
            this.buttonDisconnect.Name = "buttonDisconnect";
            this.buttonDisconnect.Size = new System.Drawing.Size(75, 48);
            this.buttonDisconnect.TabIndex = 5;
            this.buttonDisconnect.Text = "Disconnect";
            this.buttonDisconnect.UseVisualStyleBackColor = true;
            this.buttonDisconnect.Click += new System.EventHandler(this.buttonDisconnect_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(230, 17);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 48);
            this.buttonConnect.TabIndex = 4;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Port";
            // 
            // numericUpDownPort
            // 
            this.numericUpDownPort.Location = new System.Drawing.Point(104, 45);
            this.numericUpDownPort.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownPort.Name = "numericUpDownPort";
            this.numericUpDownPort.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownPort.TabIndex = 2;
            this.numericUpDownPort.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP address";
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(104, 19);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(120, 20);
            this.textBoxIP.TabIndex = 0;
            this.textBoxIP.Text = "127.0.0.1";
            // 
            // groupBoxRequest
            // 
            this.groupBoxRequest.Controls.Add(this.buttonExecute);
            this.groupBoxRequest.Controls.Add(this.comboBoxDataType);
            this.groupBoxRequest.Controls.Add(this.textBoxTagValue);
            this.groupBoxRequest.Controls.Add(this.textBoxTagId);
            this.groupBoxRequest.Controls.Add(this.label4);
            this.groupBoxRequest.Controls.Add(this.label3);
            this.groupBoxRequest.Controls.Add(this.comboBoxCmd);
            this.groupBoxRequest.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxRequest.Location = new System.Drawing.Point(0, 75);
            this.groupBoxRequest.Name = "groupBoxRequest";
            this.groupBoxRequest.Size = new System.Drawing.Size(559, 98);
            this.groupBoxRequest.TabIndex = 2;
            this.groupBoxRequest.TabStop = false;
            this.groupBoxRequest.Text = "Request";
            // 
            // buttonExecute
            // 
            this.buttonExecute.Location = new System.Drawing.Point(12, 72);
            this.buttonExecute.Name = "buttonExecute";
            this.buttonExecute.Size = new System.Drawing.Size(193, 20);
            this.buttonExecute.TabIndex = 6;
            this.buttonExecute.Text = "Execute";
            this.buttonExecute.UseVisualStyleBackColor = true;
            this.buttonExecute.Click += new System.EventHandler(this.buttonExecute_Click);
            // 
            // comboBoxDataType
            // 
            this.comboBoxDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDataType.FormattingEnabled = true;
            this.comboBoxDataType.Items.AddRange(new object[] {
            "Integer",
            "Floating Point"});
            this.comboBoxDataType.Location = new System.Drawing.Point(12, 45);
            this.comboBoxDataType.Name = "comboBoxDataType";
            this.comboBoxDataType.Size = new System.Drawing.Size(193, 21);
            this.comboBoxDataType.TabIndex = 5;
            // 
            // textBoxTagValue
            // 
            this.textBoxTagValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTagValue.Location = new System.Drawing.Point(401, 42);
            this.textBoxTagValue.Name = "textBoxTagValue";
            this.textBoxTagValue.Size = new System.Drawing.Size(146, 20);
            this.textBoxTagValue.TabIndex = 4;
            this.textBoxTagValue.Text = "1324";
            // 
            // textBoxTagId
            // 
            this.textBoxTagId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTagId.Location = new System.Drawing.Point(401, 19);
            this.textBoxTagId.Name = "textBoxTagId";
            this.textBoxTagId.Size = new System.Drawing.Size(146, 20);
            this.textBoxTagId.TabIndex = 3;
            this.textBoxTagId.Text = "30";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(249, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Value (only for Writes)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(249, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "TagId";
            // 
            // comboBoxCmd
            // 
            this.comboBoxCmd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCmd.FormattingEnabled = true;
            this.comboBoxCmd.Items.AddRange(new object[] {
            "Read Single Value",
            "Read String Value",
            "Write Single Value",
            "Write String Value"});
            this.comboBoxCmd.Location = new System.Drawing.Point(12, 19);
            this.comboBoxCmd.Name = "comboBoxCmd";
            this.comboBoxCmd.Size = new System.Drawing.Size(193, 21);
            this.comboBoxCmd.TabIndex = 0;
            // 
            // groupBoxResponse
            // 
            this.groupBoxResponse.Controls.Add(this.textBoxResponse);
            this.groupBoxResponse.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxResponse.Location = new System.Drawing.Point(0, 173);
            this.groupBoxResponse.Name = "groupBoxResponse";
            this.groupBoxResponse.Size = new System.Drawing.Size(559, 75);
            this.groupBoxResponse.TabIndex = 3;
            this.groupBoxResponse.TabStop = false;
            this.groupBoxResponse.Text = "Response";
            // 
            // textBoxResponse
            // 
            this.textBoxResponse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxResponse.Location = new System.Drawing.Point(3, 16);
            this.textBoxResponse.Multiline = true;
            this.textBoxResponse.Name = "textBoxResponse";
            this.textBoxResponse.ReadOnly = true;
            this.textBoxResponse.Size = new System.Drawing.Size(553, 56);
            this.textBoxResponse.TabIndex = 7;
            // 
            // backgroundWorkerRunCommand
            // 
            this.backgroundWorkerRunCommand.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerRunCommand_DoWork);
            this.backgroundWorkerRunCommand.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerRunCommand_RunWorkerCompleted);
            // 
            // backgroundWorkerConnect
            // 
            this.backgroundWorkerConnect.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerConnect_DoWork);
            this.backgroundWorkerConnect.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerConnect_RunWorkerCompleted);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 334);
            this.Controls.Add(this.groupBoxResponse);
            this.Controls.Add(this.groupBoxRequest);
            this.Controls.Add(this.groupBoxConnection);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Main";
            this.Text = "MLogics Chile Ltda.    Weihenstephaner Protocol Test Client";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBoxConnection.ResumeLayout(false);
            this.groupBoxConnection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPort)).EndInit();
            this.groupBoxRequest.ResumeLayout(false);
            this.groupBoxRequest.PerformLayout();
            this.groupBoxResponse.ResumeLayout(false);
            this.groupBoxResponse.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.GroupBox groupBoxRequest;
        private System.Windows.Forms.GroupBox groupBoxResponse;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Button buttonDisconnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxCmd;
        private System.Windows.Forms.TextBox textBoxTagValue;
        private System.Windows.Forms.TextBox textBoxTagId;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxDataType;
        private System.Windows.Forms.TextBox textBoxResponse;
        private System.Windows.Forms.Button buttonExecute;
        private System.ComponentModel.BackgroundWorker backgroundWorkerRunCommand;
        private System.ComponentModel.BackgroundWorker backgroundWorkerConnect;
    }
}

