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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace DephtInition
{
    public partial class MainForm : Form
    {
        BitmapShowForm _showContrForm;
        BitmapShowForm _showRGBForm;
        BitmapShowForm _showDepthForm;
        ValuesGraphForm _graphForm;

        FloatMap _maxMap = null; // TODO: introduce middle step "raw map"

        // TODO: databind these:
        float _spikeFilterTreshold = 2.0f;
        int _multiResSteps = 3;
        float _curveReliabilityTreshold = 0.2f;
        int _preShrinkTimes = 1;
        int _capHolesFilterEmisize = 3;
        int _capHolesFilterIterations = 3;

        public MainForm()
        {
            InitializeComponent();
            _showContrForm = new BitmapShowForm() { Text = "Contrast" };
            _showContrForm.Show();

            _showRGBForm = new BitmapShowForm() { Text = "RGB" };
            _showRGBForm.Show();

            _showDepthForm = new BitmapShowForm() { Text = "Depht (approx)" };
            _showDepthForm.Show();

            _graphForm = new ValuesGraphForm() { Text = "graph"};
            _graphForm.Show();

            _showRGBForm.pnlDisplayBitmap.MouseDown += new MouseEventHandler(pnlDisplayBitmap_MouseDown);
            _showContrForm.pnlDisplayBitmap.MouseDown += new MouseEventHandler(pnlDisplayBitmap_MouseDown);

            _showDepthForm.pnlDisplayBitmap.MouseDown += new MouseEventHandler(checkSpikes);

            btnGo.Tag = false;
        }

        void checkSpikes(object sender, MouseEventArgs e)
        {
            var typedSender = sender as Control;

            int w = _imgfs[0].W;
            int h = _imgfs[0].H;

            float xProp = (float)w / (float)typedSender.Width;
            float yProp = (float)h / (float)typedSender.Height;

            int x = (int)(e.X * xProp);
            int y = (int)(e.Y * yProp);

            Console.WriteLine("[{0},{1}] -> [{2},{3}]", e.X, e.Y, x, y);

            Console.WriteLine("spike: {0}",MapUtils.GetSpikeHeight(_maxMap, x, y));
        }

        void pnlDisplayBitmap_MouseDown(object sender, MouseEventArgs e)
        {
            var typedSender = sender as Control;

            int w = _imgfs[0].W;
            int h = _imgfs[0].H;
            
            float xProp = (float)w / (float)typedSender.Width;
            float yProp = (float)h / (float)typedSender.Height;

            int x = (int)(e.X * xProp);
            int y = (int)(e.Y * yProp);

            Console.WriteLine("[{0},{1}] -> [{2},{3}]", e.X, e.Y, x, y);


            float[] vs = new float[_imgfs.Count];

            int i = 0;
            foreach (var im in _imgfs)
            {
                vs[i] = im[x, y];
                Console.WriteLine("{1} {0:0.0000}", im[x, y], i++);
            }


            _graphForm.Values = vs;
        }

        string[] _fileNames = null;

        int _displayedBmpIdx = -1;

        public int DisplayedBmpIdx
        {
            get 
            {
                if ((_displayedBmpIdx < 0) && (_fileNames.Length > 0))
                {
                    _displayedBmpIdx = 0;
                }
                
                return _displayedBmpIdx;
            }

            set
            {
                if ((_fileNames == null) || (_fileNames.Length <= 0))
                {
                    return;
                }

                if (value >= _fileNames.Length)
                {
                    _displayedBmpIdx = _fileNames.Length -1 ;
                }
                else
                {
                    if (value < 0)
                    {
                        _displayedBmpIdx = (_fileNames.Length > 0) ? 0 : -1;
                    }
                    else
                    {
                        _displayedBmpIdx = value;
                    }
                }
                                
                _showRGBForm.DisplayedBitmap = new Bitmap(_fileNames[_displayedBmpIdx]);
                this.Text = string.Format("displaying image {0}/{1}", _displayedBmpIdx, _fileNames.Length - 1);

                float max = MapUtils.GetMapMax(_imgfs[_displayedBmpIdx]);
                _showContrForm.DisplayedBitmap = MapUtils.Map2Bmp(_imgfs[_displayedBmpIdx], 255.0f / max);
            }
        }

        List<FloatMap> _imgfs = new List<FloatMap>();

        int _w = -1;
        int _h = -1;

        float _stackInterDistance = 8;

        private void btnGo_Click(object sender, EventArgs e)
        {
            if ((bool)(btnGo.Tag) == false)
            {
                _preShrinkTimes = (int)updShrinkTimes.Value;

                _stackInterDistance = (float)updStackInterDistance.Value;
                _multiResSteps = (int)updMultiResSteps.Value;
                _curveReliabilityTreshold = (float)updCurveReliabilityTreshold.Value;

                _spikeFilterTreshold = (float)updSpikeFilterTreshold.Value;

                _capHolesFilterEmisize = (int)updCapHolesSize.Value;
                _capHolesFilterIterations = (int)updCapHolesIterations.Value;

                btnGo.Text = "cancel";
                btnGo.Tag = true;
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                btnGo.Text = "go";
                btnGo.Tag = false;
                backgroundWorker1.CancelAsync();
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case '+':
                    ++DisplayedBmpIdx;
                    break;
                case '-':
                    --DisplayedBmpIdx;
                    break;
            }
        }

        string pickName(string name, string ext)
        {
            int i = 0;
            string result;
            while (File.Exists(result = string.Format("{0}{1}.{2}", name, i, ext)))
            {
                ++i;
            }
            return result;
        }

        float getMaxIdx(int x, int y)
        {
            int l = _imgfs.Count;
            float[] vals = new float[l];
            for (int i = 0; i < l; ++i )
            {
                vals[i] = _imgfs[i][x, y];
            }

            return getDist(vals);
        }

        FloatMap getMaxMap()
        {
            int h = _imgfs[0].H;
            int w = _imgfs[0].W;

            FloatMap imgfOut = new FloatMap(w, h);

            for (int y =0; y<h; ++y)
            {
                for (int x = 0; x < w; ++x )
                {
                    float v = getMaxIdx(x, y);
                    imgfOut[x, y] = v;// < 0 ? -1 : 255 - v * 255 / _imgfs.Count; // MOVED into map2BmpDepht
                }            
            }

            return imgfOut;
        }

        void smoothDepht()
        {
            int h = _imgfs[0].H;
            int w = _imgfs[0].W;
            int stride = _imgfs[0].Stride;
            
            int l = _imgfs.Count;

            for (int imgIdx = 1; imgIdx < l-1; ++imgIdx)
            {
                int lineStart = 0;
                for (int y = 0; y < h; ++y)
                {
                    int i = lineStart;
                    for (int x = 0; x < w; ++x)
                    {
                        _imgfs[imgIdx][i] = _imgfs[imgIdx][i] * 0.5f + (_imgfs[imgIdx + 1][i] + _imgfs[imgIdx - 1][i]) * 0.25f;
                        ++i;
                    }
                    lineStart += stride;
                }
            }


        }

        float getDist(float[] values)
        {
            // this is basically trying to fit a parable
            // to the points which focus rank is higher than average
            // THIS IS A CHEAP TRICK soon to be substituted
            // with RANSAC or RANSAC-like technique...

            try
            {
                float min = 1000000;
                int minPos = -1;

                float max = 0;
                int maxPos = -1;

                int l = values.Length;
                float v = 0;
                float accu = 0;

                for (int i = 0; i < l; ++i)
                {
                    v = values[i];
                    accu += v;

                    if (v < min)
                    {
                        min = v;
                        minPos = i;
                    }

                    if (v > max)
                    {
                        max = v;
                        maxPos = i;
                    }
                }

                float average = accu / l;

                List<PointF> ps = new List<PointF>(l);

                for (int i = 0; i < l; ++i)
                {
                    v = values[i];
                    if (v > average)
                    {
                        ps.Add(new PointF(i, values[i]));
                    }
                }

                var fittedParamsTemp = CurveFunctions.FindPolynomialLeastSquaresFit(ps, 2);

                if (fittedParamsTemp[2] > 0)
                {
                    return -1;
                }

                var errorTemp = Math.Sqrt(CurveFunctions.ErrorSquared(ps, fittedParamsTemp));

                //if (errorTemp > 0.5f)
                //{
                //    return -1;
                //}

                if (max - average < _curveReliabilityTreshold)
                {
                    return -1;
                }

                // -b / 2a 
                float res = (float)(-fittedParamsTemp[1] / (2 * fittedParamsTemp[2]));
                return (res >= l) || (res <= 0) ? -1 : res;
            }
            catch { return -1; }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _w = -1;
            _h = -1;
            _fileNames = openFileDialog1.FileNames;
        }

        private void rGBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showRGBForm.Show();
            _showContrForm.Focus();
        }

        private void contrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showContrForm.Show();
            _showContrForm.Focus();
        }

        private void pointDephtGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _graphForm.Show();
            _graphForm.Focus();
        }

        private void dephtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showDepthForm.Show();
            _showDepthForm.Focus();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // TODO: correct the bell-distorsion

            if ((_fileNames == null) || (_fileNames.Length <= 3))
            {
                return;
            }

            _imgfs.Clear();
            
            int fileCount = _fileNames.Length;

            float progressStep = 100.0f / (fileCount * 2.0f);
            float progress = 0;

            backgroundWorker1.ReportProgress((int)progress,"converting");

            // for each selected file 
            for (int fileIdx = 0; fileIdx < fileCount; ++fileIdx)
            {
                string fileName = _fileNames[fileIdx];

                // load bitmap
                using (var _bmp = new Bitmap(fileName))
                {
                    if (fileIdx == 0)
                    {
                        _h = _bmp.Height;
                        _w = _bmp.Width;
                    }
                    else
                    {

                        if ((_h != _bmp.Height) || (_w != _bmp.Width))
                        {
                            MessageBox.Show("Images must have same size!");
                            return;
                        }
                    }

                    FloatMap imgf;

                    // get luminance map
                    imgf = MapUtils.HalfMap(MapUtils.Bmp2Map(_bmp), _preShrinkTimes);
                    
                    _imgfs.Add(imgf);
                }

                // update and report progress
                progress += progressStep;
                backgroundWorker1.ReportProgress((int)progress);

                // check for cancellation
                if (backgroundWorker1.CancellationPending)
                {
                    return;
                }
            }

            List<FloatMap> newImgfs = new List<FloatMap>();

            backgroundWorker1.ReportProgress((int)progress, "getting contrast");

            // for each luminance map
            foreach (var imgf in _imgfs)
            {
                // get contrast, then shrink result (averaging pixels)
                FloatMap newImgf = MapUtils.HalfMap(MapUtils.GetMultiResContrastEvaluation(imgf, 2), 4);

                newImgfs.Add(newImgf);

                // update and report progress
                progress += progressStep;
                backgroundWorker1.ReportProgress((int)progress);

                // check for cancellation
                if (backgroundWorker1.CancellationPending)
                {
                    return;
                }
            }

            _imgfs = newImgfs;

            smoothDepht(); smoothDepht();

            _maxMap = getMaxMap();

            // filter out spikes
            _maxMap = MapUtils.SpikesFilter(_maxMap, _spikeFilterTreshold);

            // cap holes
            bool thereAreStillHoles = false;
            for (int i = 0; i < _capHolesFilterIterations; ++i )
            {
                _maxMap = MapUtils.CapHoles(_maxMap, _capHolesFilterEmisize, out thereAreStillHoles);
                if (!thereAreStillHoles)
                {
                    break;
                }
            }

            // SAVE PLY 

            int rw = _maxMap.W;
            int rh = _maxMap.H;

            int count = 0;

            for (int y = 0; y < rh; ++y)
            {
                for (int x = 0; x < rw; ++x)
                {
                    if (_maxMap[x, y] > 0)
                    {
                        ++count;
                    }
                }
            }

            var xk = _w / rw;
            var yk = _h / rh;

            // load last bitmap
            using (var bmp = new Bitmap(_fileNames[_fileNames.Length - 1]))
            {
                BitmapData dstData = bmp.LockBits(new Rectangle(0, 0, _w, _h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int pixelSize = 4;

                // open ply file
                using (var sw = new StreamWriter(pickName("shot", "ply")))
                {
                    // write ply header
                    sw.WriteLine("ply");
                    sw.WriteLine("format ascii 1.0");
                    sw.WriteLine("comment PLY generated with DephtInition by Giancarlo Todone");
                    sw.WriteLine("element vertex " + count);
                    sw.WriteLine("property float x");
                    sw.WriteLine("property float y");
                    sw.WriteLine("property float z");
                    sw.WriteLine("property uchar red");
                    sw.WriteLine("property uchar green");
                    sw.WriteLine("property uchar blue");
                    sw.WriteLine("end_header");

                    float s = (float)Math.Max(rw, rh);
                    float xOffs = -0.5f * s / (float)rw;
                    float yOffs = -0.5f * s / (float)rh;
                    float zk = -_stackInterDistance;
                    float zOffs = _stackInterDistance * (float)(_imgfs.Count) * 0.5f;

                    unsafe
                    {
                        // access bitmap data
                        int dstStride = dstData.Stride;
                        for (int y = 0; y < rh; ++y)
                        {
                            int by = y * yk;
                            byte* dstRow = (byte*)dstData.Scan0 + dstStride * by;
                            for (int x = 0; x < rw; ++x)
                            {
                                var v = _maxMap[x, y];
                                if (v >= 0)
                                {
                                    int i = x * xk * pixelSize;
                                    byte b = dstRow[i];
                                    byte g = dstRow[i + 1];
                                    byte r = dstRow[i + 2];

                                    float pz = v * zk + zOffs;
                                    float px = (((float)x / s + xOffs) * 400.0f); // TODO: fix once an for all conversions between virtual units and real world ones
                                    float py = (((float)y / s + yOffs) * 400.0f);

                                    // write point
                                    sw.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000} {1:0.000} {2:0.000} {3} {4} {5}", px, py, pz, r, g, b));
                                }
                            }
                            dstRow += dstStride;
                        }
                    }
                }

                bmp.UnlockBits(dstData);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            gaugeProgressBar1.Value = e.ProgressPercentage;
            string s = e.UserState as string;
            if (s != null)
            {
                gaugeProgressBar1.Label = s;    
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnGo.Enabled = true;
            btnGo.Text = "go";
            btnGo.Tag = false;
            
            if (gaugeProgressBar1.Value == 0)
            {
                if ((_fileNames == null) || (_fileNames.Length <= 3))
                {
                    MessageBox.Show("Nothing to do (select more files)");
                    return;
                }
                return;
            }

            gaugeProgressBar1.Value = 0;

            if (!e.Cancelled)
            {
                try
                {
                    DisplayedBmpIdx = 0;
                    _showDepthForm.DisplayedBitmap = MapUtils.Map2BmpDephtMap(_maxMap, 1, _imgfs.Count);
                }
                catch { }
                gaugeProgressBar1.Label = "done";
            }
            else
            {
                gaugeProgressBar1.Label = "canceled";
            }
        }
    }
}
