namespace PGDis
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
            this.axMapControl = new ESRI.ArcGIS.Controls.AxMapControl();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDrawPolygon = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.delF = new System.Windows.Forms.Button();
            this.btnDrawPolyline = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.btnClip = new System.Windows.Forms.Button();
            this.btnIntersect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.C = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CARCGIS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CPG = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.btnRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axMapControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // axLicenseControl1
            // 
            this.axLicenseControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axLicenseControl1.Enabled = true;
            this.axLicenseControl1.Location = new System.Drawing.Point(0, 0);
            this.axLicenseControl1.Name = "axLicenseControl1";
            this.axLicenseControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axLicenseControl1.OcxState")));
            this.axLicenseControl1.Size = new System.Drawing.Size(32, 32);
            this.axLicenseControl1.TabIndex = 0;
            // 
            // axMapControl
            // 
            this.axMapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axMapControl.Location = new System.Drawing.Point(0, 0);
            this.axMapControl.Name = "axMapControl";
            this.axMapControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMapControl.OcxState")));
            this.axMapControl.Size = new System.Drawing.Size(466, 452);
            this.axMapControl.TabIndex = 1;
            this.axMapControl.OnMouseDown += new ESRI.ArcGIS.Controls.IMapControlEvents2_Ax_OnMouseDownEventHandler(this.axMapControl_OnMouseDown);
            // 
            // btnEdit
            // 
            this.btnEdit.Location = new System.Drawing.Point(3, 9);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "开始编辑";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnDrawPolygon
            // 
            this.btnDrawPolygon.Enabled = false;
            this.btnDrawPolygon.Location = new System.Drawing.Point(3, 54);
            this.btnDrawPolygon.Name = "btnDrawPolygon";
            this.btnDrawPolygon.Size = new System.Drawing.Size(75, 23);
            this.btnDrawPolygon.TabIndex = 3;
            this.btnDrawPolygon.Text = "面";
            this.btnDrawPolygon.UseVisualStyleBackColor = true;
            this.btnDrawPolygon.Click += new System.EventHandler(this.btnDrawPolygon_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(3, 125);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 4;
            this.btnSelect.Text = "选择";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // delF
            // 
            this.delF.Location = new System.Drawing.Point(3, 154);
            this.delF.Name = "delF";
            this.delF.Size = new System.Drawing.Size(75, 23);
            this.delF.TabIndex = 6;
            this.delF.Text = "删除";
            this.delF.UseVisualStyleBackColor = true;
            this.delF.Click += new System.EventHandler(this.delF_Click);
            // 
            // btnDrawPolyline
            // 
            this.btnDrawPolyline.Enabled = false;
            this.btnDrawPolyline.Location = new System.Drawing.Point(3, 83);
            this.btnDrawPolyline.Name = "btnDrawPolyline";
            this.btnDrawPolyline.Size = new System.Drawing.Size(75, 23);
            this.btnDrawPolyline.TabIndex = 7;
            this.btnDrawPolyline.Text = "线";
            this.btnDrawPolyline.UseVisualStyleBackColor = true;
            this.btnDrawPolyline.Click += new System.EventHandler(this.btnDrawPolyline_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.axMapControl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(862, 452);
            this.splitContainer1.SplitterDistance = 466;
            this.splitContainer1.TabIndex = 10;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.btnRefresh);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.btnClip);
            this.splitContainer2.Panel1.Controls.Add(this.btnEdit);
            this.splitContainer2.Panel1.Controls.Add(this.btnDrawPolyline);
            this.splitContainer2.Panel1.Controls.Add(this.btnIntersect);
            this.splitContainer2.Panel1.Controls.Add(this.delF);
            this.splitContainer2.Panel1.Controls.Add(this.btnDrawPolygon);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            this.splitContainer2.Panel1.Controls.Add(this.btnSelect);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.dgv);
            this.splitContainer2.Size = new System.Drawing.Size(392, 452);
            this.splitContainer2.SplitterDistance = 84;
            this.splitContainer2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 269);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 29);
            this.label2.TabIndex = 11;
            this.label2.Text = "选中要素作为裁剪要素";
            // 
            // btnClip
            // 
            this.btnClip.Location = new System.Drawing.Point(3, 301);
            this.btnClip.Name = "btnClip";
            this.btnClip.Size = new System.Drawing.Size(75, 23);
            this.btnClip.TabIndex = 10;
            this.btnClip.Text = "裁剪";
            this.btnClip.UseVisualStyleBackColor = true;
            this.btnClip.Click += new System.EventHandler(this.btnClip_Click);
            // 
            // btnIntersect
            // 
            this.btnIntersect.Location = new System.Drawing.Point(3, 231);
            this.btnIntersect.Name = "btnIntersect";
            this.btnIntersect.Size = new System.Drawing.Size(75, 23);
            this.btnIntersect.TabIndex = 8;
            this.btnIntersect.Text = "空间相交";
            this.btnIntersect.UseVisualStyleBackColor = true;
            this.btnIntersect.Click += new System.EventHandler(this.btnIntersect_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 204);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 29);
            this.label1.TabIndex = 9;
            this.label1.Text = "与选中的要素相交";
            // 
            // dgv
            // 
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.C,
            this.CARCGIS,
            this.CPG});
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 0);
            this.dgv.Name = "dgv";
            this.dgv.ReadOnly = true;
            this.dgv.RowHeadersVisible = false;
            this.dgv.RowTemplate.Height = 23;
            this.dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgv.Size = new System.Drawing.Size(304, 452);
            this.dgv.TabIndex = 0;
            // 
            // C
            // 
            this.C.HeaderText = "功能";
            this.C.Name = "C";
            this.C.ReadOnly = true;
            this.C.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // CARCGIS
            // 
            this.CARCGIS.HeaderText = "ArcGIS";
            this.CARCGIS.Name = "CARCGIS";
            this.CARCGIS.ReadOnly = true;
            this.CARCGIS.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // CPG
            // 
            this.CPG.HeaderText = "PG";
            this.CPG.Name = "CPG";
            this.CPG.ReadOnly = true;
            this.CPG.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(862, 25);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer3.Size = new System.Drawing.Size(862, 478);
            this.splitContainer3.SplitterDistance = 452;
            this.splitContainer3.SplitterWidth = 1;
            this.splitContainer3.TabIndex = 12;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(726, 20);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(19, 20);
            this.toolStripStatusLabel2.Text = "    ";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 19);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(3, 426);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 12;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(862, 478);
            this.Controls.Add(this.splitContainer3);
            this.Controls.Add(this.axLicenseControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axMapControl)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ESRI.ArcGIS.Controls.AxLicenseControl axLicenseControl1;
        private ESRI.ArcGIS.Controls.AxMapControl axMapControl;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDrawPolygon;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button delF;
        private System.Windows.Forms.Button btnDrawPolyline;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnIntersect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnClip;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridViewTextBoxColumn C;
        private System.Windows.Forms.DataGridViewTextBoxColumn CARCGIS;
        private System.Windows.Forms.DataGridViewTextBoxColumn CPG;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button btnRefresh;
    }
}

