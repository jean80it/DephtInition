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

    class CurveFunctions
    {
        // The function.
        public static double F(List<double> coeffs, double x)
        {
            double total = 0;
            double x_factor = 1;
            for (int i = 0; i < coeffs.Count; i++)
            {
                total += x_factor * coeffs[i];
                x_factor *= x;
            }
            return total;
        }

        // Return the error squared.
        public static double ErrorSquared(List<PointF> points, List<double> coeffs)
        {
            double total = 0;
            foreach (PointF pt in points)
            {
                double dy = pt.Y - F(coeffs, pt.X);
                total += dy * dy;
            }
            return total;
        }

        // Find the least squares linear fit.
        public static List<double> FindPolynomialLeastSquaresFit(List<PointF> points, int degree)
        {
            // Allocate space for (degree + 1) equations with 
            // (degree + 2) terms each (including the constant term).
            double[,] coeffs = new double[degree + 1, degree + 2];

            // Calculate the coefficients for the equations.
            for (int j = 0; j <= degree; j++)
            {
                // Calculate the coefficients for the jth equation.

                // Calculate the constant term for this equation.
                coeffs[j, degree + 1] = 0;
                foreach (PointF pt in points)
                {
                    coeffs[j, degree + 1] -= Math.Pow(pt.X, j) * pt.Y;
                }

                // Calculate the other coefficients.
                for (int a_sub = 0; a_sub <= degree; a_sub++)
                {
                    // Calculate the dth coefficient.
                    coeffs[j, a_sub] = 0;
                    foreach (PointF pt in points)
                    {
                        coeffs[j, a_sub] -= Math.Pow(pt.X, a_sub + j);
                    }
                }
            }

            // Solve the equations.
            double[] answer = GaussianElimination(coeffs);

            // Return the result converted into a List<double>.
            return answer.ToList<double>();
        }

        // Perform Gaussian elimination on these coefficients.
        // Return the array of values that gives the solution.
        private static double[] GaussianElimination(double[,] coeffs)
        {
            int max_equation = coeffs.GetUpperBound(0);
            int max_coeff = coeffs.GetUpperBound(1);
            for (int i = 0; i <= max_equation; i++)
            {
                // Use equation_coeffs[i, i] to eliminate the ith
                // coefficient in all of the other equations.

                // Find a row with non-zero ith coefficient.
                if (coeffs[i, i] == 0)
                {
                    for (int j = i + 1; j <= max_equation; j++)
                    {
                        // See if this one works.
                        if (coeffs[j, i] != 0)
                        {
                            // This one works. Swap equations i and j.
                            // This starts at k = i because all
                            // coefficients to the left are 0.
                            for (int k = i; k <= max_coeff; k++)
                            {
                                double temp = coeffs[i, k];
                                coeffs[i, k] = coeffs[j, k];
                                coeffs[j, k] = temp;
                            }
                            break;
                        }
                    }
                }

                // Make sure we found an equation with
                // a non-zero ith coefficient.
                double coeff_i_i = coeffs[i, i];
                if (coeff_i_i == 0)
                {
                    throw new ArithmeticException(String.Format(
                        "There is no unique solution for these points.",
                        coeffs.GetUpperBound(0) - 1));
                }

                // Normalize the ith equation.
                for (int j = i; j <= max_coeff; j++)
                {
                    coeffs[i, j] /= coeff_i_i;
                }

                // Use this equation value to zero out
                // the other equations' ith coefficients.
                for (int j = 0; j <= max_equation; j++)
                {
                    // Skip the ith equation.
                    if (j != i)
                    {
                        // Zero the jth equation's ith coefficient.
                        double coef_j_i = coeffs[j, i];
                        for (int d = 0; d <= max_coeff; d++)
                        {
                            coeffs[j, d] -= coeffs[i, d] * coef_j_i;
                        }
                    }
                }
            }

            // At this point, the ith equation contains
            // 2 non-zero entries:
            //      The ith entry which is 1
            //      The last entry coeffs[max_coeff]
            // This means Ai = equation_coef[max_coeff].
            double[] solution = new double[max_equation + 1];
            for (int i = 0; i <= max_equation; i++)
            {
                solution[i] = coeffs[i, max_coeff];
            }

            // Return the solution values.
            return solution;
        }
    }

}

