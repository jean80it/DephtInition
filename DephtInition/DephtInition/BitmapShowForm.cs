//   Copyright 2013 Giancarlo Todone

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

//   for info: http://www.stareat.it/sp.aspx?g=3ce7bc36fb334b8d85e6900b0bdf11c3

using System;
using System.Drawing;
using System.Windows.Forms;

namespace DepthInition
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
