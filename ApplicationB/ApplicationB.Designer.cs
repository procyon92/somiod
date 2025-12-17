namespace ApplicationB
{
    partial class ApplicationB
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
            this.txtApiUrl = new System.Windows.Forms.TextBox();
            this.groupBox_Config = new System.Windows.Forms.GroupBox();
            this.btnListen = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.lbContainerName = new System.Windows.Forms.Label();
            this.lbAppName = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lbBrokerDomain = new System.Windows.Forms.Label();
            this.txtBrokerIp = new System.Windows.Forms.TextBox();
            this.btnSubscribe = new System.Windows.Forms.Button();
            this.btnUnsubscribe = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox_Config.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtApiUrl
            // 
            this.txtApiUrl.Location = new System.Drawing.Point(6, 31);
            this.txtApiUrl.Name = "txtApiUrl";
            this.txtApiUrl.Size = new System.Drawing.Size(256, 22);
            this.txtApiUrl.TabIndex = 0;
            this.txtApiUrl.Text = "http://localhost:5000/api/somiod";
            // 
            // groupBox_Config
            // 
            this.groupBox_Config.Controls.Add(this.btnListen);
            this.groupBox_Config.Controls.Add(this.textBox2);
            this.groupBox_Config.Controls.Add(this.lbContainerName);
            this.groupBox_Config.Controls.Add(this.lbAppName);
            this.groupBox_Config.Controls.Add(this.textBox1);
            this.groupBox_Config.Controls.Add(this.lbBrokerDomain);
            this.groupBox_Config.Controls.Add(this.txtBrokerIp);
            this.groupBox_Config.Controls.Add(this.txtApiUrl);
            this.groupBox_Config.Location = new System.Drawing.Point(34, 37);
            this.groupBox_Config.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox_Config.Name = "groupBox_Config";
            this.groupBox_Config.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox_Config.Size = new System.Drawing.Size(555, 121);
            this.groupBox_Config.TabIndex = 1;
            this.groupBox_Config.TabStop = false;
            this.groupBox_Config.Text = "Configuração:";
            // 
            // btnListen
            // 
            this.btnListen.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnListen.Location = new System.Drawing.Point(435, 84);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(114, 23);
            this.btnListen.TabIndex = 7;
            this.btnListen.Text = "Ligar ao MQTT";
            this.btnListen.UseVisualStyleBackColor = false;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(312, 84);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 22);
            this.textBox2.TabIndex = 6;
            this.textBox2.Text = "piso-01";
            // 
            // lbContainerName
            // 
            this.lbContainerName.AutoSize = true;
            this.lbContainerName.Location = new System.Drawing.Point(309, 65);
            this.lbContainerName.Name = "lbContainerName";
            this.lbContainerName.Size = new System.Drawing.Size(104, 16);
            this.lbContainerName.TabIndex = 5;
            this.lbContainerName.Text = "Container Name";
            // 
            // lbAppName
            // 
            this.lbAppName.AutoSize = true;
            this.lbAppName.Location = new System.Drawing.Point(159, 65);
            this.lbAppName.Name = "lbAppName";
            this.lbAppName.Size = new System.Drawing.Size(72, 16);
            this.lbAppName.TabIndex = 4;
            this.lbAppName.Text = "App Name";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(162, 84);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 22);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "smart-parking";
            // 
            // lbBrokerDomain
            // 
            this.lbBrokerDomain.AutoSize = true;
            this.lbBrokerDomain.Location = new System.Drawing.Point(7, 60);
            this.lbBrokerDomain.Name = "lbBrokerDomain";
            this.lbBrokerDomain.Size = new System.Drawing.Size(95, 16);
            this.lbBrokerDomain.TabIndex = 2;
            this.lbBrokerDomain.Text = "Broker domain";
            // 
            // txtBrokerIp
            // 
            this.txtBrokerIp.Location = new System.Drawing.Point(6, 84);
            this.txtBrokerIp.Name = "txtBrokerIp";
            this.txtBrokerIp.Size = new System.Drawing.Size(100, 22);
            this.txtBrokerIp.TabIndex = 1;
            this.txtBrokerIp.Text = "127.0.0.1";
            // 
            // btnSubscribe
            // 
            this.btnSubscribe.BackColor = System.Drawing.Color.Lime;
            this.btnSubscribe.Location = new System.Drawing.Point(34, 191);
            this.btnSubscribe.Name = "btnSubscribe";
            this.btnSubscribe.Size = new System.Drawing.Size(215, 23);
            this.btnSubscribe.TabIndex = 3;
            this.btnSubscribe.Text = "Subscribe";
            this.btnSubscribe.UseVisualStyleBackColor = false;
            this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
            // 
            // btnUnsubscribe
            // 
            this.btnUnsubscribe.BackColor = System.Drawing.Color.IndianRed;
            this.btnUnsubscribe.Location = new System.Drawing.Point(374, 191);
            this.btnUnsubscribe.Name = "btnUnsubscribe";
            this.btnUnsubscribe.Size = new System.Drawing.Size(215, 23);
            this.btnUnsubscribe.TabIndex = 4;
            this.btnUnsubscribe.Text = "Unsubscribe";
            this.btnUnsubscribe.UseVisualStyleBackColor = false;
            // CORRIGIDO: Removido o _1
            this.btnUnsubscribe.Click += new System.EventHandler(this.btnUnsubscribe_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(34, 241);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(555, 218);
            this.txtLog.TabIndex = 5;
            this.txtLog.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 222);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Received Messages";
            // 
            // ApplicationB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 471);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnUnsubscribe);
            this.Controls.Add(this.btnSubscribe);
            this.Controls.Add(this.groupBox_Config);
            this.Name = "ApplicationB";
            this.Text = "ApplicationB";
            this.Load += new System.EventHandler(this.ApplicationB_Load);
            this.groupBox_Config.ResumeLayout(false);
            this.groupBox_Config.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtApiUrl;
        private System.Windows.Forms.GroupBox groupBox_Config;
        private System.Windows.Forms.TextBox txtBrokerIp;
        private System.Windows.Forms.Label lbAppName;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lbBrokerDomain;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label lbContainerName;
        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.Button btnSubscribe;
        private System.Windows.Forms.Button btnUnsubscribe;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Label label1;
    }
}