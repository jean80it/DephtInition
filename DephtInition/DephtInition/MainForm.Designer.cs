namespace DepthInition
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
            this.btnGo = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contrastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pointDepthGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.depthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.gaugeProgressBar1 = new DepthInition.GaugeProgressBar();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.grpPostprocess = new System.Windows.Forms.GroupBox();
            this.panel9 = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.updBlurTimes = new System.Windows.Forms.NumericUpDown();
            this.panel7 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.updBlurSigma = new System.Windows.Forms.NumericUpDown();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.updCapHolesSize = new System.Windows.Forms.NumericUpDown();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.updSpikeFilterTreshold = new System.Windows.Forms.NumericUpDown();
            this.grpProcess = new System.Windows.Forms.GroupBox();
            this.panel10 = new System.Windows.Forms.Panel();
            this.label10 = new System.Windows.Forms.Label();
            this.updShrinkContrastTimes = new System.Windows.Forms.NumericUpDown();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.updCurveReliabilityTreshold = new System.Windows.Forms.NumericUpDown();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.updMultiResSteps = new System.Windows.Forms.NumericUpDown();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.updStackInterDistance = new System.Windows.Forms.NumericUpDown();
            this.panel8 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.updCloserPictureDistance = new System.Windows.Forms.NumericUpDown();
            this.grpPreprocess = new System.Windows.Forms.GroupBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.updShrinkTimes = new System.Windows.Forms.NumericUpDown();
            this.menuStrip1.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            this.grpPostprocess.SuspendLayout();
            this.panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updBlurTimes)).BeginInit();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updBlurSigma)).BeginInit();
            this.panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updCapHolesSize)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updSpikeFilterTreshold)).BeginInit();
            this.grpProcess.SuspendLayout();
            this.panel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updShrinkContrastTimes)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updCurveReliabilityTreshold)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updMultiResSteps)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updStackInterDistance)).BeginInit();
            this.panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updCloserPictureDistance)).BeginInit();
            this.grpPreprocess.SuspendLayout();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updShrinkTimes)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // btnGo
            // 
            this.btnGo.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnGo.Location = new System.Drawing.Point(0, 0);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 27);
            this.btnGo.TabIndex = 0;
            this.btnGo.Text = "go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(511, 24);
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
            this.pointDepthGraphToolStripMenuItem,
            this.depthToolStripMenuItem});
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
            // pointDepthGraphToolStripMenuItem
            // 
            this.pointDepthGraphToolStripMenuItem.Name = "pointDepthGraphToolStripMenuItem";
            this.pointDepthGraphToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.pointDepthGraphToolStripMenuItem.Text = "Point Depth Graph";
            this.pointDepthGraphToolStripMenuItem.Click += new System.EventHandler(this.pointDepthGraphToolStripMenuItem_Click);
            // 
            // depthToolStripMenuItem
            // 
            this.depthToolStripMenuItem.Name = "depthToolStripMenuItem";
            this.depthToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.depthToolStripMenuItem.Text = "Depth";
            this.depthToolStripMenuItem.Click += new System.EventHandler(this.depthToolStripMenuItem_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.gaugeProgressBar1);
            this.pnlBottom.Controls.Add(this.btnGo);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 448);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(511, 27);
            this.pnlBottom.TabIndex = 3;
            // 
            // gaugeProgressBar1
            // 
            this.gaugeProgressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gaugeProgressBar1.Label = "waiting";
            this.gaugeProgressBar1.Location = new System.Drawing.Point(75, 0);
            this.gaugeProgressBar1.Name = "gaugeProgressBar1";
            this.gaugeProgressBar1.Size = new System.Drawing.Size(436, 27);
            this.gaugeProgressBar1.TabIndex = 2;
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.grpPostprocess);
            this.pnlOptions.Controls.Add(this.grpProcess);
            this.pnlOptions.Controls.Add(this.grpPreprocess);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOptions.Location = new System.Drawing.Point(0, 24);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Padding = new System.Windows.Forms.Padding(3);
            this.pnlOptions.Size = new System.Drawing.Size(511, 424);
            this.pnlOptions.TabIndex = 10;
            // 
            // grpPostprocess
            // 
            this.grpPostprocess.Controls.Add(this.panel9);
            this.grpPostprocess.Controls.Add(this.panel7);
            this.grpPostprocess.Controls.Add(this.panel6);
            this.grpPostprocess.Controls.Add(this.panel2);
            this.grpPostprocess.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpPostprocess.Location = new System.Drawing.Point(3, 234);
            this.grpPostprocess.Name = "grpPostprocess";
            this.grpPostprocess.Size = new System.Drawing.Size(505, 133);
            this.grpPostprocess.TabIndex = 7;
            this.grpPostprocess.TabStop = false;
            this.grpPostprocess.Text = "Post process";
            // 
            // panel9
            // 
            this.panel9.Controls.Add(this.label9);
            this.panel9.Controls.Add(this.updBlurTimes);
            this.panel9.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel9.Location = new System.Drawing.Point(3, 106);
            this.panel9.Name = "panel9";
            this.panel9.Padding = new System.Windows.Forms.Padding(3);
            this.panel9.Size = new System.Drawing.Size(499, 30);
            this.panel9.TabIndex = 6;
            // 
            // label9
            // 
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Location = new System.Drawing.Point(3, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(429, 24);
            this.label9.TabIndex = 9;
            this.label9.Text = "blur repeat times";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updBlurTimes
            // 
            this.updBlurTimes.Dock = System.Windows.Forms.DockStyle.Right;
            this.updBlurTimes.Location = new System.Drawing.Point(432, 3);
            this.updBlurTimes.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updBlurTimes.Name = "updBlurTimes";
            this.updBlurTimes.Size = new System.Drawing.Size(64, 20);
            this.updBlurTimes.TabIndex = 8;
            this.updBlurTimes.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // panel7
            // 
            this.panel7.Controls.Add(this.label7);
            this.panel7.Controls.Add(this.updBlurSigma);
            this.panel7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel7.Location = new System.Drawing.Point(3, 76);
            this.panel7.Name = "panel7";
            this.panel7.Padding = new System.Windows.Forms.Padding(3);
            this.panel7.Size = new System.Drawing.Size(499, 30);
            this.panel7.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Location = new System.Drawing.Point(3, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(429, 24);
            this.label7.TabIndex = 7;
            this.label7.Text = "blur sigma";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updBlurSigma
            // 
            this.updBlurSigma.DecimalPlaces = 1;
            this.updBlurSigma.Dock = System.Windows.Forms.DockStyle.Right;
            this.updBlurSigma.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updBlurSigma.Location = new System.Drawing.Point(432, 3);
            this.updBlurSigma.Name = "updBlurSigma";
            this.updBlurSigma.Size = new System.Drawing.Size(64, 20);
            this.updBlurSigma.TabIndex = 6;
            this.updBlurSigma.Value = new decimal(new int[] {
            18,
            0,
            0,
            65536});
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.label6);
            this.panel6.Controls.Add(this.updCapHolesSize);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(3, 46);
            this.panel6.Name = "panel6";
            this.panel6.Padding = new System.Windows.Forms.Padding(3);
            this.panel6.Size = new System.Drawing.Size(499, 30);
            this.panel6.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(3, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(429, 24);
            this.label6.TabIndex = 9;
            this.label6.Text = "cap holes filter emisize (1-10)";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updCapHolesSize
            // 
            this.updCapHolesSize.Dock = System.Windows.Forms.DockStyle.Right;
            this.updCapHolesSize.Location = new System.Drawing.Point(432, 3);
            this.updCapHolesSize.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updCapHolesSize.Name = "updCapHolesSize";
            this.updCapHolesSize.Size = new System.Drawing.Size(64, 20);
            this.updCapHolesSize.TabIndex = 8;
            this.updCapHolesSize.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.updSpikeFilterTreshold);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 16);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(3);
            this.panel2.Size = new System.Drawing.Size(499, 30);
            this.panel2.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(429, 24);
            this.label2.TabIndex = 7;
            this.label2.Text = "spikes filter treshold (0-100)";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updSpikeFilterTreshold
            // 
            this.updSpikeFilterTreshold.DecimalPlaces = 1;
            this.updSpikeFilterTreshold.Dock = System.Windows.Forms.DockStyle.Right;
            this.updSpikeFilterTreshold.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updSpikeFilterTreshold.Location = new System.Drawing.Point(432, 3);
            this.updSpikeFilterTreshold.Name = "updSpikeFilterTreshold";
            this.updSpikeFilterTreshold.Size = new System.Drawing.Size(64, 20);
            this.updSpikeFilterTreshold.TabIndex = 6;
            this.updSpikeFilterTreshold.Value = new decimal(new int[] {
            18,
            0,
            0,
            65536});
            // 
            // grpProcess
            // 
            this.grpProcess.Controls.Add(this.panel10);
            this.grpProcess.Controls.Add(this.panel4);
            this.grpProcess.Controls.Add(this.panel3);
            this.grpProcess.Controls.Add(this.panel1);
            this.grpProcess.Controls.Add(this.panel8);
            this.grpProcess.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpProcess.Location = new System.Drawing.Point(3, 57);
            this.grpProcess.Name = "grpProcess";
            this.grpProcess.Size = new System.Drawing.Size(505, 177);
            this.grpProcess.TabIndex = 6;
            this.grpProcess.TabStop = false;
            this.grpProcess.Text = "Process";
            // 
            // panel10
            // 
            this.panel10.Controls.Add(this.label10);
            this.panel10.Controls.Add(this.updShrinkContrastTimes);
            this.panel10.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel10.Location = new System.Drawing.Point(3, 136);
            this.panel10.Name = "panel10";
            this.panel10.Padding = new System.Windows.Forms.Padding(3);
            this.panel10.Size = new System.Drawing.Size(499, 30);
            this.panel10.TabIndex = 6;
            // 
            // label10
            // 
            this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label10.Location = new System.Drawing.Point(3, 3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(429, 24);
            this.label10.TabIndex = 9;
            this.label10.Text = "shrink contrast map n times";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updShrinkContrastTimes
            // 
            this.updShrinkContrastTimes.Dock = System.Windows.Forms.DockStyle.Right;
            this.updShrinkContrastTimes.Location = new System.Drawing.Point(432, 3);
            this.updShrinkContrastTimes.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updShrinkContrastTimes.Name = "updShrinkContrastTimes";
            this.updShrinkContrastTimes.Size = new System.Drawing.Size(64, 20);
            this.updShrinkContrastTimes.TabIndex = 8;
            this.updShrinkContrastTimes.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.updCurveReliabilityTreshold);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(3, 106);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(3);
            this.panel4.Size = new System.Drawing.Size(499, 30);
            this.panel4.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(429, 24);
            this.label4.TabIndex = 9;
            this.label4.Text = "curve reliability treshold (0.1-10)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updCurveReliabilityTreshold
            // 
            this.updCurveReliabilityTreshold.DecimalPlaces = 1;
            this.updCurveReliabilityTreshold.Dock = System.Windows.Forms.DockStyle.Right;
            this.updCurveReliabilityTreshold.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updCurveReliabilityTreshold.Location = new System.Drawing.Point(432, 3);
            this.updCurveReliabilityTreshold.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updCurveReliabilityTreshold.Name = "updCurveReliabilityTreshold";
            this.updCurveReliabilityTreshold.Size = new System.Drawing.Size(64, 20);
            this.updCurveReliabilityTreshold.TabIndex = 8;
            this.updCurveReliabilityTreshold.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.updMultiResSteps);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 76);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(3);
            this.panel3.Size = new System.Drawing.Size(499, 30);
            this.panel3.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(429, 24);
            this.label3.TabIndex = 9;
            this.label3.Text = "multiresolution steps (1-10)";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updMultiResSteps
            // 
            this.updMultiResSteps.Dock = System.Windows.Forms.DockStyle.Right;
            this.updMultiResSteps.Location = new System.Drawing.Point(432, 3);
            this.updMultiResSteps.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updMultiResSteps.Name = "updMultiResSteps";
            this.updMultiResSteps.Size = new System.Drawing.Size(64, 20);
            this.updMultiResSteps.TabIndex = 8;
            this.updMultiResSteps.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.updStackInterDistance);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 46);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(499, 30);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(429, 24);
            this.label1.TabIndex = 5;
            this.label1.Text = "distance between pictures (mm)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updStackInterDistance
            // 
            this.updStackInterDistance.DecimalPlaces = 1;
            this.updStackInterDistance.Dock = System.Windows.Forms.DockStyle.Right;
            this.updStackInterDistance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updStackInterDistance.Location = new System.Drawing.Point(432, 3);
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
            // panel8
            // 
            this.panel8.Controls.Add(this.label8);
            this.panel8.Controls.Add(this.updCloserPictureDistance);
            this.panel8.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel8.Location = new System.Drawing.Point(3, 16);
            this.panel8.Name = "panel8";
            this.panel8.Padding = new System.Windows.Forms.Padding(3);
            this.panel8.Size = new System.Drawing.Size(499, 30);
            this.panel8.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Location = new System.Drawing.Point(3, 3);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(429, 24);
            this.label8.TabIndex = 5;
            this.label8.Text = "closer picture distance (mm)";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updCloserPictureDistance
            // 
            this.updCloserPictureDistance.DecimalPlaces = 1;
            this.updCloserPictureDistance.Dock = System.Windows.Forms.DockStyle.Right;
            this.updCloserPictureDistance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updCloserPictureDistance.Location = new System.Drawing.Point(432, 3);
            this.updCloserPictureDistance.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.updCloserPictureDistance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.updCloserPictureDistance.Name = "updCloserPictureDistance";
            this.updCloserPictureDistance.Size = new System.Drawing.Size(64, 20);
            this.updCloserPictureDistance.TabIndex = 4;
            this.updCloserPictureDistance.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // grpPreprocess
            // 
            this.grpPreprocess.Controls.Add(this.panel5);
            this.grpPreprocess.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpPreprocess.Location = new System.Drawing.Point(3, 3);
            this.grpPreprocess.Name = "grpPreprocess";
            this.grpPreprocess.Size = new System.Drawing.Size(505, 54);
            this.grpPreprocess.TabIndex = 5;
            this.grpPreprocess.TabStop = false;
            this.grpPreprocess.Text = "Pre process";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label5);
            this.panel5.Controls.Add(this.updShrinkTimes);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(3, 16);
            this.panel5.Name = "panel5";
            this.panel5.Padding = new System.Windows.Forms.Padding(3);
            this.panel5.Size = new System.Drawing.Size(499, 30);
            this.panel5.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 3);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(429, 24);
            this.label5.TabIndex = 9;
            this.label5.Text = "shrink input image n times (1-10)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // updShrinkTimes
            // 
            this.updShrinkTimes.Dock = System.Windows.Forms.DockStyle.Right;
            this.updShrinkTimes.Location = new System.Drawing.Point(432, 3);
            this.updShrinkTimes.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.updShrinkTimes.Name = "updShrinkTimes";
            this.updShrinkTimes.Size = new System.Drawing.Size(64, 20);
            this.updShrinkTimes.TabIndex = 8;
            this.updShrinkTimes.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(511, 475);
            this.Controls.Add(this.pnlOptions);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(270, 176);
            this.Name = "MainForm";
            this.Text = "DepthInition";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.pnlOptions.ResumeLayout(false);
            this.grpPostprocess.ResumeLayout(false);
            this.panel9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updBlurTimes)).EndInit();
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updBlurSigma)).EndInit();
            this.panel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updCapHolesSize)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updSpikeFilterTreshold)).EndInit();
            this.grpProcess.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updShrinkContrastTimes)).EndInit();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updCurveReliabilityTreshold)).EndInit();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updMultiResSteps)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updStackInterDistance)).EndInit();
            this.panel8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updCloserPictureDistance)).EndInit();
            this.grpPreprocess.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updShrinkTimes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rGBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contrastToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pointDepthGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem depthToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private DepthInition.GaugeProgressBar gaugeProgressBar1;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlOptions;
        private System.Windows.Forms.GroupBox grpPostprocess;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown updSpikeFilterTreshold;
        private System.Windows.Forms.GroupBox grpProcess;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown updCurveReliabilityTreshold;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown updMultiResSteps;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown updStackInterDistance;
        private System.Windows.Forms.GroupBox grpPreprocess;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown updShrinkTimes;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown updCapHolesSize;
        private System.Windows.Forms.Panel panel8;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown updCloserPictureDistance;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown updBlurSigma;
        private System.Windows.Forms.Panel panel9;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown updBlurTimes;
        private System.Windows.Forms.Panel panel10;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown updShrinkContrastTimes;
    }
}

