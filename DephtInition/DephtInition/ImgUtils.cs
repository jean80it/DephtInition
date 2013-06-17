//Copyright 2013 Giancarlo Todone

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace DephtInition
{
    class FloatMap
    {
        int _w = -1, _h = -1, _stride = 0;
        float[] _buf = null;

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
            _buf = new float[_stride*_h];
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
                return _buf[x + (y *_stride)];
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
    }

    class FloatMap2Aligned
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
    }

    class ImgUtils
    {
        //public static FloatMap GetMultiResContrastEvaluation(FloatMap imgfIn, int subSamples)
        //{
        //    int h = imgfIn.H;
        //    int w = imgfIn.W;
        //    float k = 1;

        //    FloatMap contr = new float[h, w];
        //    for (int i = 0; i < subSamples; ++i)
        //    {
        //        var img = halfImg(imgfIn, i);
        //        var rc = doubleImg(blurImg(getContrImg(img)), i);
        //        Accumulate(contr, rc, k);
        //        k *= 0.5f;
        //    }
        //    return contr;
        //}

        public static FloatMap GetMultiResContrastEvaluation(FloatMap imgfIn, int subSamples)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            float k = 1;

            FloatMap contr = new FloatMap(w, h);
            var img = imgfIn;
            for (int i = 0; i < subSamples; ++i)
            {
                var rc = ResizeImg(BlurImg(GetContrImg(img)), w, h);
                //var rc = DoubleImg(BlurImg(GetContrImg(img)), i); // <-- multi step reduction would still be first choice with openCl
                Accumulate(contr, rc, k);
                k *= 0.5f;
                img = HalfImg(img);
            }
            return contr;
        }

        private static FloatMap ResizeImg(FloatMap imgfIn, int dstW, int dstH)
        {
            int srcH = imgfIn.H;
            int srcW = imgfIn.W;
            int stride = imgfIn.Stride;

            float xk = srcW / dstW;
            float yk = srcH / dstH;

            FloatMap imgOut = new FloatMap(dstW, dstH);
            float dy = 0;
            int lineStart = 0;
            for (int y = 0; y < dstH; ++y)
            {
                float dx = 0;
                int i = lineStart;
                for (int x = 0; x < dstW; ++x)
                {
                    imgOut[i] = imgfIn[dx, dy];
                    dx += xk;
                    ++i;
                }
                lineStart += stride;
                dy += yk;
            }

            return imgOut;
        }

        public static void Accumulate(FloatMap imgfInAccu, FloatMap imgfIn, float k)
        {
            int h = imgfInAccu.H;
            int w = imgfInAccu.W;
            int stride = imgfInAccu.Stride;

            if ((imgfIn.H != h) || (imgfIn.W != w))
            {
                throw new Exception("Images must have same size!");
            }

            int lineStart = 0;
            for (int y = 0; y < h; ++y)
            {
                var i = lineStart;
                for (int x = 0; x < w; ++x)
                {
                    imgfInAccu[i] = imgfInAccu[i] + imgfIn[i] * k; 
                    i += 1;
                }
                lineStart += stride;
            }
        }

        public static float GetMax(FloatMap imgfIn) 
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            float max = float.MinValue;

            int lineStart = 0;
            for (int y = 0; y < h; ++y)
            {
                var i = lineStart;
                for (int x = 0; x < w; ++x)
                {
                    var v = imgfIn[i];
                    if (v > max)
                    {
                        max = v;
                    }
                    i += 1;
                }
                lineStart += stride;
            }

            return max;
        }

        public static FloatMap GetContrImg(FloatMap imgfIn)
        {
            const float k1 = 0.104167f;
            const float k2 = 0.145833f;

            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            var contrImgfs = new FloatMap(w, h);

            int lineStart = stride;
            for (int y = 1; y < h - 1; ++y)
            {
                var i = lineStart + 1;
                for (int x = 1; x < w - 2; ++x) // -2 ?????
                {
                    var c = imgfIn[i];

                    // TODO: Optimize with scanlines
                    contrImgfs[i] = (Math.Abs(c - imgfIn[i + stride]) + Math.Abs(c - imgfIn[i - stride]) + Math.Abs(c - imgfIn[i + 1]) + Math.Abs(c - imgfIn[i - 1])) * k2 +
                                    (Math.Abs(c - imgfIn[i + stride + 1]) + Math.Abs(c - imgfIn[i + stride - 1]) + Math.Abs(c - imgfIn[i - stride + 1]) + Math.Abs(c - imgfIn[i - stride - 1])) * k1;
                    i += 1;
                }
                lineStart += stride;
            }
            return contrImgfs;
        }

        public static FloatMap HalfImg(FloatMap imgfIn, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                imgfIn = HalfImg(imgfIn);
            }
            return imgfIn;
        }

        public static FloatMap HalfImg(FloatMap imgfIn)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            int hh = h >> 1;
            int hw = w >> 1;
            
            var imgfOut = new FloatMap(hw, hh);

            int hStride = imgfOut.Stride;

            int lineStart = 0;
            int hLineStart = 0;
            for (int y = 0; y < hh; ++y)
            {
                int i = lineStart;
                int hi = hLineStart;
                for (int x = 0; x < hw; ++x)
                {
                    imgfOut[hi] = (imgfIn[i] + imgfIn[i + stride] + imgfIn[i + 1] + imgfIn[i + stride + 1]) * 0.25f;
                    i += 2;
                    ++hi;
                }
                lineStart += stride << 1;
                hLineStart += hStride;
            }
            return imgfOut;
        }

        public static FloatMap DoubleImg(FloatMap imgfIn, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                imgfIn = DoubleImg(imgfIn);
            }
            return imgfIn;
        }

        public static FloatMap DoubleImg(FloatMap imgfIn)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            
            int dh = h * 2;
            int dw = w * 2;

            var imgfOut = new FloatMap(dw, dh);
            int dstStride = imgfOut.Stride;

            float dx = 0, dy = 0;

            int dLineStart = 0;
            for (int y = 0; y < dh - 2; ++y)
            {
                dx = 0;
                int i = dLineStart;
                for (int x = 0; x < dw - 2; ++x)
                {
                    imgfOut[i] = imgfIn[dx, dy];
                    dx += 0.5f;
                    ++i;
                }
                dy += 0.5f;
                dLineStart += dstStride;
            }
            return imgfOut;
        }

        public static FloatMap BlurImg(FloatMap imgfIn)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            var imgfOut = new FloatMap(w, h);

            const float k1 = 0.1715728f; // w = 2
            const float k2 = 0.0857864f; // w = 1
            const float k3 = 0.0606601f; // w = 1/1.4 = 0.7

            int lineStart = stride;
            for (int y = 1; y < h - 1; ++y)
            {
                int i = lineStart + 1; ;
                for (int x = 1; x < w - 1; ++x)
                {
                    imgfOut[i] = (imgfIn[i]) * k1 +
                                    (imgfIn[i + stride] + imgfIn[i - stride] + imgfIn[i+1] + imgfIn[i-1]) * k2 +
                                    (imgfIn[i + stride + 1] + imgfIn[i + stride - 1] + imgfIn[i - stride + 1] + imgfIn[i - stride - 1]) * k3;
                    ++i;
                }
                lineStart += stride;
            }
            return imgfOut;
        }

        public static FloatMap Bmp2floatIntensity(Bitmap bmp)
        {
            var w = bmp.Width;
            var h = bmp.Height;

            Bitmap tmp = null;

            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                bmp = bmp.Clone(new Rectangle(0, 0, w, h), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                tmp = bmp;
            }

            var imgf = new FloatMap(w, h);
            int stride = imgf.Stride;

            int pixelSize = 4;

            BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* srcRow = (byte*)srcData.Scan0;
                int srcStride = srcData.Stride;

                int dstLineStart = 0;
                for (int y = 0; y < h; ++y)
                {
                    int dstIdx = dstLineStart;
                    int wb = w * pixelSize;
                    for (int x = 0; x < wb; x+=pixelSize)
                    {
                        imgf[dstIdx] = getLuminosity(srcRow[x + 0], srcRow[x + 1], srcRow[x + 2]); // +3 is alpha
                        ++dstIdx;
                    }
                    dstLineStart += stride;
                    srcRow += srcStride;
                }
            }

            bmp.UnlockBits(srcData);

            if (tmp == null)
            {
                tmp.Dispose(); // disposing our cloned copy... caller is responsible to dispose original bmp
            }

            return imgf;
        }

        public static Bitmap FloatIntensity2Bmp(FloatMap imgf, float k)
        {
            int h = imgf.H;
            int w = imgf.W;
            int stride = imgf.Stride;

            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            BitmapData dstData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int pixelSize = 4;

            unsafe
            {
                var dstStride = dstData.Stride;
                byte* dstRow = (byte*)dstData.Scan0;
                int srcLineStart = 0;
                for (int y = 0; y < h; ++y)
                {
                    int srcIdx = srcLineStart;
                    int wb = w * pixelSize;
                    for (int x = 0; x < wb; x+=pixelSize)
                    {
                        byte b = (byte)(imgf[srcIdx] * k);
                        dstRow[x] = b;
                        dstRow[x + 1] = b;
                        dstRow[x + 2] = b;
                        dstRow[x + 3] = 255;
                        ++srcIdx;
                    }
                    srcLineStart += stride;
                    dstRow += dstStride;
                }
            }

            bmp.UnlockBits(dstData);
            return bmp;
        }

        // creates the blue-red depht map
        public static Bitmap FloatIntensity2Bmp_(FloatMap imgf, float k)
        {
            int h = imgf.H;
            int w = imgf.W;
            int stride = imgf.Stride;

            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            BitmapData dstData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int pixelSize = 4;

            unsafe
            {
                var dstStride = dstData.Stride;
                byte* dstRow = (byte*)dstData.Scan0;
                int srcLineStart = 0;
                for (int y = 0; y < h; ++y)
                {
                    int srcIdx = srcLineStart;
                    int wb = w * pixelSize;
                    for (int x = 0; x < wb; x+=4)
                    {
                        float v = imgf[srcIdx];
                        if (v >= 0)
                        {
                            byte b = (byte)Math.Min(255, Math.Max((v * k), 0));
                            dstRow[x] = b;
                            dstRow[x + 1] = 0;
                            dstRow[x + 2] = (byte)(255 - b);
                            dstRow[x + 3] = 255;
                        }
                        else
                        {
                            dstRow[x] = 0;
                            dstRow[x + 1] = 0;
                            dstRow[x + 2] = 0;
                            dstRow[x + 3] = 255;
                        }
                        ++srcIdx;
                    }
                    srcLineStart += stride;
                    dstRow += dstStride;
                }
            }

            bmp.UnlockBits(dstData);
            return bmp;
        }

        static  float getLuminosity(byte r, int g, int b)
        {
            return 0.299f * r + 0.587f * g + 0.114f * b;
            //return (r + g + b) / 3.0f;
        }
    }
}
