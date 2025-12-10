namespace ApplicationA
{
    partial class ApplicationA
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
            this.groupBox_Config = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtApiUrl = new System.Windows.Forms.TextBox();
            this.groupBox_Sensor = new System.Windows.Forms.GroupBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSaida = new System.Windows.Forms.Button();
            this.btnEntrada = new System.Windows.Forms.Button();
            this.groupBox_Config.SuspendLayout();
            this.groupBox_Sensor.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_Config
            // 
            this.groupBox_Config.Controls.Add(this.btnConnect);
            this.groupBox_Config.Controls.Add(this.txtApiUrl);
            this.groupBox_Config.Location = new System.Drawing.Point(30, 44);
            this.groupBox_Config.Name = "groupBox_Config";
            this.groupBox_Config.Size = new System.Drawing.Size(624, 100);
            this.groupBox_Config.TabIndex = 0;
            this.groupBox_Config.TabStop = false;
            this.groupBox_Config.Text = "Configuração:";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(358, 36);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(122, 29);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Ligar Sensor";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtApiUrl
            // 
            this.txtApiUrl.Location = new System.Drawing.Point(7, 36);
            this.txtApiUrl.Name = "txtApiUrl";
            this.txtApiUrl.Size = new System.Drawing.Size(318, 26);
            this.txtApiUrl.TabIndex = 0;
            this.txtApiUrl.Text = "http://localhost:51364/api/somiod/";
            this.txtApiUrl.TextChanged += new System.EventHandler(this.txtApiUrl_TextChanged);
            // 
            // groupBox_Sensor
            // 
            this.groupBox_Sensor.Controls.Add(this.lblStatus);
            this.groupBox_Sensor.Controls.Add(this.btnSaida);
            this.groupBox_Sensor.Controls.Add(this.btnEntrada);
            this.groupBox_Sensor.Location = new System.Drawing.Point(30, 188);
            this.groupBox_Sensor.Name = "groupBox_Sensor";
            this.groupBox_Sensor.Size = new System.Drawing.Size(624, 100);
            this.groupBox_Sensor.TabIndex = 1;
            this.groupBox_Sensor.TabStop = false;
            this.groupBox_Sensor.Text = "GroupBox (Sensor Lugar A1):";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(358, 34);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(166, 20);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "Status: Desconhecido";
            // 
            // btnSaida
            // 
            this.btnSaida.Location = new System.Drawing.Point(7, 61);
            this.btnSaida.Name = "btnSaida";
            this.btnSaida.Size = new System.Drawing.Size(167, 33);
            this.btnSaida.TabIndex = 1;
            this.btnSaida.Text = "Liberar Lugar";
            this.btnSaida.UseVisualStyleBackColor = true;
            this.btnSaida.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // btnEntrada
            // 
            this.btnEntrada.Location = new System.Drawing.Point(7, 26);
            this.btnEntrada.Name = "btnEntrada";
            this.btnEntrada.Size = new System.Drawing.Size(167, 29);
            this.btnEntrada.TabIndex = 0;
            this.btnEntrada.Text = "Ocupar Lugar";
            this.btnEntrada.UseVisualStyleBackColor = true;
            this.btnEntrada.Click += new System.EventHandler(this.btnEntrar_Click);
            // 
            // ApplicationA
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox_Sensor);
            this.Controls.Add(this.groupBox_Config);
            this.Name = "ApplicationA";
            this.Text = "Producer Application";
            this.Load += new System.EventHandler(this.ApplicationA_Load);
            this.groupBox_Config.ResumeLayout(false);
            this.groupBox_Config.PerformLayout();
            this.groupBox_Sensor.ResumeLayout(false);
            this.groupBox_Sensor.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_Config;
        private System.Windows.Forms.TextBox txtApiUrl;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox groupBox_Sensor;
        private System.Windows.Forms.Button btnEntrada;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnSaida;
    }
}

