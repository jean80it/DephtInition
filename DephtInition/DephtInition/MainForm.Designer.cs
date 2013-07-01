namespace DephtInition
{
    partial class MainForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Liberare le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contrastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pointDephtGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dephtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gaugeProgressBar1 = new DephtInition.GaugeProgressBar();
            this.updStackInterDistance = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.updSpikeFilterTreshold = new System.Windows.Forms.NumericUpDown();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updStackInterDistance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updSpikeFilterTreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Left;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 40);
            this.button1.TabIndex = 0;
            this.button1.Text = "go";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(388, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rGBToolStripMenuItem,
            this.contrastToolStripMenuItem,
            this.pointDephtGraphToolStripMenuItem,
            this.dephtToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // rGBToolStripMenuItem
            // 
            this.rGBToolStripMenuItem.Name = "rGBToolStripMenuItem";
            this.rGBToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.rGBToolStripMenuItem.Text = "RGB";
            this.rGBToolStripMenuItem.Click += new System.EventHandler(this.rGBToolStripMenuItem_Click);
            // 
            // contrastToolStripMenuItem
            // 
            this.contrastToolStripMenuItem.Name = "contrastToolStripMenuItem";
            this.contrastToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.contrastToolStripMenuItem.Text = "Contrast";
            this.contrastToolStripMenuItem.Click += new System.EventHandler(this.contrastToolStripMenuItem_Click);
            // 
            // pointDephtGraphToolStripMenuItem
            // 
            this.pointDephtGraphToolStripMenuItem.Name = "pointDephtGraphToolStripMenuItem";
            this.pointDephtGraphToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.pointDephtGraphToolStripMenuItem.Text = "Point Depht Graph";
            this.pointDephtGraphToolStripMenuItem.Click += new System.EventHandler(this.pointDephtGraphToolStripMenuItem_Click);
            // 
            // dephtToolStripMenuItem
            // 
            this.dephtToolStripMenuItem.Name = "dephtToolStripMenuItem";
            this.dephtToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.dephtToolStripMenuItem.Text = "Depht";
            this.dephtToolStripMenuItem.Click += new System.EventHandler(this.dephtToolStripMenuItem_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gaugeProgressBar1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 124);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(388, 40);
            this.panel1.TabIndex = 3;
            // 
            // gaugeProgressBar1
            // 
            this.gaugeProgressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gaugeProgressBar1.Label = "waiting";
            this.gaugeProgressBar1.Location = new System.Drawing.Point(75, 0);
            this.gaugeProgressBar1.Name = "gaugeProgressBar1";
            this.gaugeProgressBar1.Size = new System.Drawing.Size(313, 40);
            this.gaugeProgressBar1.TabIndex = 2;
            // 
            // updStackInterDistance
            // 
            this.updStackInterDistance.DecimalPlaces = 1;
            this.updStackInterDistance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updStackInterDistance.Location = new System.Drawing.Point(174, 27);
            this.updStackInterDistance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updStackInterDistance.Name = "updStackInterDistance";
            this.updStackInterDistance.Size = new System.Drawing.Size(64, 20);
            this.updStackInterDistance.TabIndex = 4;
            this.updStackInterDistance.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "distance between pictures (mm)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "spikes filter treshold (0-255)";
            // 
            // updSpikeFilterTreshold
            // 
            this.updSpikeFilterTreshold.DecimalPlaces = 1;
            this.updSpikeFilterTreshold.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updSpikeFilterTreshold.Location = new System.Drawing.Point(174, 53);
            this.updSpikeFilterTreshold.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.updSpikeFilterTreshold.Name = "updSpikeFilterTreshold";
            this.updSpikeFilterTreshold.Size = new System.Drawing.Size(64, 20);
            this.updSpikeFilterTreshold.TabIndex = 6;
            this.updSpikeFilterTreshold.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(388, 164);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.updSpikeFilterTreshold);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.updStackInterDistance);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "DephtInition";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updStackInterDistance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updSpikeFilterTreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rGBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contrastToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pointDephtGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dephtToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DephtInition.GaugeProgressBar gaugeProgressBar1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown updStackInterDistance;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown updSpikeFilterTreshold;
    }
}

