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
using System.Drawing.Imaging;

namespace DepthInition
{
    public class FloatMap
    {
        int _w = -1, _h = -1, _stride = 0;
        public float[] _buf = null;

        public int W { get { return _w; } }
        public int H { get { return _h; } }
        public int Stride { get { return _stride; } }

        public int LineStart(int y)
        {
            return y * _stride;
        }

        public FloatMap(int w, int h)
        {
            _w = w;
            _h = h;
            _stride = _w;
            _buf = new float[_stride * _h];
        }

        public float this[int linearX]
        {
            get { return _buf[linearX]; }
            set { _buf[linearX] = value; }
        }

        public float this[int x, int y]
        {
            get
            {
                return _buf[x + (y * _stride)];
            }

            set
            {
                _buf[x + (y * _stride)] = value;
            }
        }

        public float this[float x, float y]
        {
            get
            {
                // linear interpolation

                int intX = (int)x;
                int intY = (int)y;
                int nextX = intX + 1;
                int nextY = intY + 1;

                int wl = _w - 1;
                int hl = _h - 1;

                if (nextX > wl) nextX = wl;
                if (nextY > hl) nextY = hl;

                float fracX = x - intX;
                float fracY = y - intY;
                float nFracX = 1 - fracX;

                return (this[intX, intY] * nFracX + this[nextX, intY] * fracX) * (1 - fracY) +
                        (this[intX, nextY] * nFracX + this[nextX, nextY] * fracX) * fracY;
            }
        }

        public int Size
        {
            get { return _buf.Length; }
        }

        public int ByteSize
        {
            get { return Size * sizeof(float); }
        }
    }

    public class FloatMap2Aligned
    {
        int _w = -1, _h = -1, _stride = 0, _strideBits = 0;
        float[] _buf = null;

        public int W { get { return _w; } }
        public int H { get { return _h; } }
        public int Stride { get { return _stride; } }
        public int StrideBits { get { return _strideBits; } }

        public int LineStart(int y)
        {
            return y << _strideBits;
        }

        public FloatMap2Aligned(int w, int h)
        {
            _w = w;
            _h = h;
            _strideBits = lowestGreater2Pow(_w);
            _stride = 1 << _strideBits;
            _buf = new float[_stride * _h];
        }

        private int lowestGreater2Pow(int x)
        {
            int i = 0;
            while (x > 0)
            {
                x >>= 1;
                ++i;
            }

            return i;
        }

        public float this[int linearX]
        {
            get { return _buf[linearX]; }
            set { _buf[linearX] = value; }
        }

        public float this[int x, int y]
        {
            get
            {
                return _buf[x + (y << _strideBits)];
            }

            set
            {
                _buf[x + (y << _strideBits)] = value;
            }
        }

        public float this[float x, float y]
        {
            get
            {
                // linear interpolation

                int intX = (int)x;
                int intY = (int)y;
                float fracX = x - intX;
                float fracY = y - intY;
                float nFracX = 1 - fracX;

                return (this[intX, intY] * nFracX + this[intX + 1, intY] * fracX) * (1 - fracY) +
                        (this[intX, intY + 1] * nFracX + this[intX + 1, intY + 1] * fracX) * fracY;
            }
        }

        public int Size
        {
            get { return _buf.Length; }
        }

        public int ByteSize
        {
            get { return Size * sizeof(float); }
        }
    }

}
