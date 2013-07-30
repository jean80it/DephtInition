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
    // sorry for bad naming, but this has been stolen from another project of mine ^__^
    public class GaugeProgressBar : ProgressBar
    {
        public String Label { get; set; }

        public GaugeProgressBar()
        {
            // Modify the ControlStyles flags
            //http://msdn.microsoft.com/en-us/library/system.windows.forms.controlstyles.aspx
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                Rectangle rect = ClientRectangle;
                Graphics g = e.Graphics;

                ProgressBarRenderer.DrawHorizontalBar(g, rect);
                rect.Inflate(-3, -3);
                if (Value > 0)
                {
                    // As we doing this ourselves we need to draw the chunks on the progress bar
                    Rectangle clip = new Rectangle(rect.X, rect.Y, (int)Math.Round(((float)Value / Maximum) * rect.Width), rect.Height);
                    ProgressBarRenderer.DrawHorizontalChunks(g, clip);
                }

                string text = string.Format("{0}: {1}/{2}", Label, Value, Maximum);

                using (Font f = new Font(FontFamily.GenericSerif, 10))
                {

                    SizeF len = g.MeasureString(text, f);
                    // Calculate the location of the text (the middle of progress bar)
                    Point location = new Point(Convert.ToInt32((rect.Width / 2) - (len.Width / 2)), Convert.ToInt32((rect.Height / 2) - (len.Height / 2)));
                    // Draw the custom text
                    g.DrawString(text, f, Brushes.Black, location);
                }
            }
            catch { } // temporary workaround (I hope) for execution on other platforms with Mono
        }
    }
}