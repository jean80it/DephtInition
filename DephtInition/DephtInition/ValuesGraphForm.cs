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
using System.Drawing;
using System.Windows.Forms;

namespace DephtInition
{
    public partial class ValuesGraphForm : Form
    {
        float[] _values;
        int _count;
        float _min;
        float _max;
        int _minPos;
        int _maxPos;
        float _average;

        float _fittedMaxPos;

        int _w;
        int _h;

        float _xk;
        float _yk;

        double _error = 0;

        List<double> _fittedParams = null;

        bool _dontClose = true;

        public float[] Values
        {
            get { return _values; }
            set 
            {
                try
                {
                    _values = value;
                    computeExtremes();
                    doFit();
                }
                catch { }

                panel1.Refresh(); 
            }
        }

        private void doFit()
        {
            int l = _values.Length;
            List<PointF> ps = new List<PointF>(l);
            
            for (int i = 0; i< l; ++i)
            {
                var v = _values[i];
                if(v>_average)
                {
                    ps.Add(new PointF(i, _values[i])); 
                }
            }
             
            var fittedParamsTemp = CurveFunctions.FindPolynomialLeastSquaresFit(ps, 2);
            var errorTemp = Math.Sqrt(CurveFunctions.ErrorSquared(ps, fittedParamsTemp));
            
            _fittedParams = fittedParamsTemp;
            _error = errorTemp;

            // -b / 2a 
            _fittedMaxPos = (float)(-_fittedParams[1] / (2 * _fittedParams[2]));
                
            lblError.Text = string.Format("{0:0.0000}", _error);
            string pt = "";
            foreach(var p in _fittedParams){pt += p.ToString("0.000");}

            lblParams.Text = pt;
        }

        public ValuesGraphForm()
            : this(true)
        { }

        public ValuesGraphForm(bool dontClose)
        {
            _dontClose = dontClose;
            InitializeComponent();
            setSize();
        }

        void computeExtremes()
        {
            if (_values == null)
            {
                return;
            }

            _count = _values.Length;

            _min = 1000000;
            _minPos = -1;

            _max = 0;
            _maxPos = -1;

            int l = _values.Length;
            float v = 0;
            float accu = 0;

            
            for (int i = 0; i < l; ++i)
            {
                v = _values[i];
                accu += v;
               
                if (v < _min)
                {
                    _min = v;
                    _minPos = i;
                }

                if (v > _max)
                {
                    _max = v;
                    _maxPos = i;
                }
            }

            _average = accu / l;
            setKs();

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            bool first = true;

            Point oldPoint = new Point();

            Graphics g = e.Graphics;

            var ca = (_average - _min) * _yk;
            g.DrawLine(Pens.Green, 0, ca, _w, ca);

            for (var i = 0; i < _count; ++i)
            {
                Point p = new Point((int)(i * _xk), (int)(_h + (_min - _values[i]) * _yk));
                if (!first)
                {
                    g.DrawLine(Pens.Black, oldPoint, p);
                }

                oldPoint = p;
                first = false;
            }

            if (_fittedParams!=null)
            {
                first = true;
                
                for(var x = 0; x<_w;++x)
                {
                    Point p = new Point((int)(x * _xk), (int)(_h + (_min - CurveFunctions.F(_fittedParams, x)) * _yk));
                
                    if (!first)
                    {
                        g.DrawLine(Pens.Blue, oldPoint, p);
                    }

                    oldPoint = p;
                    first = false;
                }
                
                var fp = _fittedMaxPos * _xk;
                g.DrawLine(Pens.Red, fp, 0, fp, _h);
            }
        }

        private void setKs()
        {
            if (_count == 0)
            {
                return;
            }

            _xk = _w / _count;
            _yk = _h / (_max - _min);
        }

        private void panel1_SizeChanged(object sender, EventArgs e)
        {
            setSize();
            panel1.Refresh();
        }

        private void setSize()
        {
            _w = panel1.Width;
            _h = panel1.Height;
            setKs();
        }

        private void ValuesGraphForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dontClose && (e.CloseReason == CloseReason.UserClosing))
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}

