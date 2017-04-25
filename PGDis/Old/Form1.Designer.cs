namespace Data
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.axLicenseControl1 = new ESRI.ArcGIS.Controls.AxLicenseControl();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtIncomeFCN = new System.Windows.Forms.TextBox();
            this.txtIncomeGDBPath = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtObjectID = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFCN2 = new System.Windows.Forms.TextBox();
            this.txtGDB2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStart = new System.Windows.Forms.TextBox();
            this.txtEnd = new System.Windows.Forms.TextBox();
            this.txtFCN = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGDB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // axLicenseControl1
            // 
            this.axLicenseControl1.Enabled = true;
            this.axLicenseControl1.Location = new System.Drawing.Point(412, 206);
            this.axLicenseControl1.Name = "axLicenseControl1";
            this.axLicenseControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axLicenseControl1.OcxState")));
            this.axLicenseControl1.Size = new System.Drawing.Size(32, 32);
            this.axLicenseControl1.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(481, 41);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(137, 12);
            this.label9.TabIndex = 43;
            this.label9.Text = "inComeFeatureClassName";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(481, 15);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 12);
            this.label10.TabIndex = 42;
            this.label10.Text = "inComeGDB";
            // 
            // txtIncomeFCN
            // 
            this.txtIncomeFCN.Location = new System.Drawing.Point(624, 38);
            this.txtIncomeFCN.Name = "txtIncomeFCN";
            this.txtIncomeFCN.Size = new System.Drawing.Size(327, 21);
            this.txtIncomeFCN.TabIndex = 41;
            this.txtIncomeFCN.Text = "EDGES";
            // 
            // txtIncomeGDBPath
            // 
            this.txtIncomeGDBPath.Location = new System.Drawing.Point(624, 9);
            this.txtIncomeGDBPath.Name = "txtIncomeGDBPath";
            this.txtIncomeGDBPath.Size = new System.Drawing.Size(327, 21);
            this.txtIncomeGDBPath.TabIndex = 40;
            this.txtIncomeGDBPath.Text = "D:\\Data\\data_2.gdb";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(481, 137);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 12);
            this.label8.TabIndex = 39;
            this.label8.Text = "要素ObjectID";
            // 
            // txtObjectID
            // 
            this.txtObjectID.Location = new System.Drawing.Point(624, 134);
            this.txtObjectID.Name = "txtObjectID";
            this.txtObjectID.Size = new System.Drawing.Size(327, 21);
            this.txtObjectID.TabIndex = 38;
            this.txtObjectID.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(481, 108);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(101, 12);
            this.label6.TabIndex = 37;
            this.label6.Text = "FeatureClassName";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(481, 79);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(23, 12);
            this.label7.TabIndex = 36;
            this.label7.Text = "GDB";
            // 
            // txtFCN2
            // 
            this.txtFCN2.Location = new System.Drawing.Point(624, 105);
            this.txtFCN2.Name = "txtFCN2";
            this.txtFCN2.Size = new System.Drawing.Size(327, 21);
            this.txtFCN2.TabIndex = 35;
            this.txtFCN2.Text = "IntersectLine";
            // 
            // txtGDB2
            // 
            this.txtGDB2.Location = new System.Drawing.Point(624, 76);
            this.txtGDB2.Name = "txtGDB2";
            this.txtGDB2.Size = new System.Drawing.Size(327, 21);
            this.txtGDB2.TabIndex = 34;
            this.txtGDB2.Text = "D:\\Data\\IntersectGDB.gdb";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(876, 183);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 33;
            this.button2.Text = "相交";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 101);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 12);
            this.label4.TabIndex = 32;
            this.label4.Text = "end";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 12);
            this.label3.TabIndex = 31;
            this.label3.Text = "start";
            // 
            // txtStart
            // 
            this.txtStart.Location = new System.Drawing.Point(117, 76);
            this.txtStart.Name = "txtStart";
            this.txtStart.Size = new System.Drawing.Size(327, 21);
            this.txtStart.TabIndex = 30;
            this.txtStart.Text = "0";
            // 
            // txtEnd
            // 
            this.txtEnd.Location = new System.Drawing.Point(117, 98);
            this.txtEnd.Name = "txtEnd";
            this.txtEnd.Size = new System.Drawing.Size(327, 21);
            this.txtEnd.TabIndex = 29;
            this.txtEnd.Text = "-1";
            // 
            // txtFCN
            // 
            this.txtFCN.Location = new System.Drawing.Point(117, 41);
            this.txtFCN.Name = "txtFCN";
            this.txtFCN.Size = new System.Drawing.Size(327, 21);
            this.txtFCN.TabIndex = 28;
            this.txtFCN.Text = "EDGES";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 27;
            this.label2.Text = "FeatureClassName";
            // 
            // txtGDB
            // 
            this.txtGDB.Location = new System.Drawing.Point(117, 12);
            this.txtGDB.Name = "txtGDB";
            this.txtGDB.Size = new System.Drawing.Size(327, 21);
            this.txtGDB.TabIndex = 26;
            this.txtGDB.Text = "D:\\Data\\data_2.gdb";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 25;
            this.label1.Text = "GDB";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(369, 125);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 24;
            this.button1.Text = "导数据";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(45, 275);
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(350, 21);
            this.txtLog.TabIndex = 45;
            this.txtLog.Text = "D:\\Data\\log_2.txt";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 278);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 12);
            this.label5.TabIndex = 44;
            this.label5.Text = "Log";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 392);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtIncomeFCN);
            this.Controls.Add(this.txtIncomeGDBPath);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtObjectID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtFCN2);
            this.Controls.Add(this.txtGDB2);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtStart);
            this.Controls.Add(this.txtEnd);
            this.Controls.Add(this.txtFCN);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtGDB);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.axLicenseControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ESRI.ArcGIS.Controls.AxLicenseControl axLicenseControl1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtIncomeFCN;
        private System.Windows.Forms.TextBox txtIncomeGDBPath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtObjectID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFCN2;
        private System.Windows.Forms.TextBox txtGDB2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtStart;
        private System.Windows.Forms.TextBox txtEnd;
        private System.Windows.Forms.TextBox txtFCN;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGDB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label5;
    }
}

