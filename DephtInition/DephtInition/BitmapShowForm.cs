using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DephtInition
{
    public partial class BitmapShowForm : Form
    {
        Bitmap _displayedBitmap = null;
        bool _dontClose = true;

        public Bitmap DisplayedBitmap
        {
            get 
            {
                return _displayedBitmap;
            }

            set 
            {
                if (_displayedBitmap != null)
                {
                    _displayedBitmap.Dispose();
                }
                _displayedBitmap = value;
                pnlDisplayBitmap.Invalidate();
            }
        }

        public BitmapShowForm()
            : this(true)
        { }

        public BitmapShowForm(bool dontClose)
        {
            _dontClose = dontClose;
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (_displayedBitmap != null)
            {
                e.Graphics.DrawImage(_displayedBitmap, 0, 0, pnlDisplayBitmap.Width, pnlDisplayBitmap.Height);
            }
        }

        private void pnlDisplayBitmap_Resize(object sender, EventArgs e)
        {
            pnlDisplayBitmap.Invalidate();
        }

        private void BitmapShowForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dontClose && (e.CloseReason == CloseReason.UserClosing))
            {
                e.Cancel = true;
                this.Hide();
            }
        }

    }
}
