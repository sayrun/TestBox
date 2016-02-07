namespace JpegExifTest
{
    partial class PreViewForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.mapsTab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.currentMap = new System.Windows.Forms.WebBrowser();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.newMap = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.mapsTab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.mapsTab);
            this.splitContainer1.Size = new System.Drawing.Size(1146, 427);
            this.splitContainer1.SplitterDistance = 740;
            this.splitContainer1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(740, 427);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // mapsTab
            // 
            this.mapsTab.Controls.Add(this.tabPage1);
            this.mapsTab.Controls.Add(this.tabPage2);
            this.mapsTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapsTab.Location = new System.Drawing.Point(0, 0);
            this.mapsTab.Name = "mapsTab";
            this.mapsTab.SelectedIndex = 0;
            this.mapsTab.Size = new System.Drawing.Size(402, 427);
            this.mapsTab.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.currentMap);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(394, 401);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "current";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // currentMap
            // 
            this.currentMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.currentMap.Location = new System.Drawing.Point(3, 3);
            this.currentMap.MinimumSize = new System.Drawing.Size(20, 20);
            this.currentMap.Name = "currentMap";
            this.currentMap.ScriptErrorsSuppressed = true;
            this.currentMap.ScrollBarsEnabled = false;
            this.currentMap.Size = new System.Drawing.Size(388, 395);
            this.currentMap.TabIndex = 0;
            this.currentMap.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.currentMap_DocumentCompleted);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.newMap);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(394, 401);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "new";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // newMap
            // 
            this.newMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.newMap.Location = new System.Drawing.Point(3, 3);
            this.newMap.MinimumSize = new System.Drawing.Size(20, 20);
            this.newMap.Name = "newMap";
            this.newMap.ScriptErrorsSuppressed = true;
            this.newMap.ScrollBarsEnabled = false;
            this.newMap.Size = new System.Drawing.Size(388, 395);
            this.newMap.TabIndex = 0;
            this.newMap.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.newMap_DocumentCompleted);
            // 
            // PreViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1146, 427);
            this.Controls.Add(this.splitContainer1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreViewForm";
            this.Text = "PreViewForm";
            this.Load += new System.EventHandler(this.PreViewForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.mapsTab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TabControl mapsTab;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.WebBrowser currentMap;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.WebBrowser newMap;
    }
}