namespace DephtInition
{
    partial class BitmapShowForm
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
            this.pnlDisplayBitmap = new System.Windows.Forms.DblBufPanel();
            this.SuspendLayout();
            // 
            // pnlDisplayBitmap
            // 
            this.pnlDisplayBitmap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDisplayBitmap.Location = new System.Drawing.Point(0, 0);
            this.pnlDisplayBitmap.Name = "pnlDisplayBitmap";
            this.pnlDisplayBitmap.Size = new System.Drawing.Size(284, 264);
            this.pnlDisplayBitmap.TabIndex = 0;
            this.pnlDisplayBitmap.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.pnlDisplayBitmap.Resize += new System.EventHandler(this.pnlDisplayBitmap_Resize);
            // 
            // BitmapShowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.Controls.Add(this.pnlDisplayBitmap);
            this.Name = "BitmapShowForm";
            this.Text = "Form2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BitmapShowForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.DblBufPanel pnlDisplayBitmap;

    }
}