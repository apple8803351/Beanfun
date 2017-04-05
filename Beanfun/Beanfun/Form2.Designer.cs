namespace Beanfun
{
    partial class Form2
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.username1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.username2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.otp_button = new System.Windows.Forms.Button();
            this.otp_textBox = new System.Windows.Forms.TextBox();
            this.pingWorker = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.SignOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.getOtpWorker = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.username1,
            this.username2});
            this.listView1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(11, 24);
            this.listView1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 4);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(254, 194);
            this.listView1.TabIndex = 10;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // username1
            // 
            this.username1.Text = "遊戲名稱";
            this.username1.Width = 80;
            // 
            // username2
            // 
            this.username2.Text = "遊戲帳號 (Double Copy)";
            this.username2.Width = 180;
            // 
            // otp_button
            // 
            this.otp_button.Location = new System.Drawing.Point(11, 225);
            this.otp_button.Name = "otp_button";
            this.otp_button.Size = new System.Drawing.Size(66, 30);
            this.otp_button.TabIndex = 11;
            this.otp_button.Text = "獲取密碼";
            this.otp_button.UseVisualStyleBackColor = true;
            this.otp_button.Click += new System.EventHandler(this.otp_button_Click);
            // 
            // otp_textBox
            // 
            this.otp_textBox.BackColor = System.Drawing.SystemColors.Control;
            this.otp_textBox.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.otp_textBox.Location = new System.Drawing.Point(83, 229);
            this.otp_textBox.Name = "otp_textBox";
            this.otp_textBox.Size = new System.Drawing.Size(182, 23);
            this.otp_textBox.TabIndex = 12;
            this.otp_textBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pingWorker
            // 
            this.pingWorker.WorkerSupportsCancellation = true;
            this.pingWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.pingWorker_DoWork);
            this.pingWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.pingWorker_RunWorkerCompleted);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SignOutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(278, 24);
            this.menuStrip1.TabIndex = 13;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // SignOutToolStripMenuItem
            // 
            this.SignOutToolStripMenuItem.Name = "SignOutToolStripMenuItem";
            this.SignOutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.SignOutToolStripMenuItem.Text = "登出";
            this.SignOutToolStripMenuItem.Click += new System.EventHandler(this.SignOutToolStripMenuItem_Click);
            // 
            // getOtpWorker
            // 
            this.getOtpWorker.WorkerSupportsCancellation = true;
            this.getOtpWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.getOtpWorker_DoWork);
            this.getOtpWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.getOtpWorker_RunWorkerCompleted);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 263);
            this.Controls.Add(this.otp_textBox);
            this.Controls.Add(this.otp_button);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader username1;
        private System.Windows.Forms.ColumnHeader username2;
        private System.Windows.Forms.Button otp_button;
        private System.Windows.Forms.TextBox otp_textBox;
        private System.ComponentModel.BackgroundWorker pingWorker;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem SignOutToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker getOtpWorker;
    }
}