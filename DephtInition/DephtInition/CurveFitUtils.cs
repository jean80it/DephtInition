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

namespace DepthInition
{
    // following code slightly modified from
    // http://blog.csharphelper.com/2010/11/08/find-a-polynomial-least-squares-fit-for-a-set-of-points-in-c.aspx
    // and related pages (couldn't find any license specification)

    public class CurveFunctions
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
            return new List<double>(answer);
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
