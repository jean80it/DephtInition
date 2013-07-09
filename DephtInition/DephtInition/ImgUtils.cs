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

    class MapUtils
    {
        // Computes some sort of resolution independant contrast defined as follows
        // (don't know yet if it's ok or not)
        // compute contrast map on input map, then
        // downscales the image, gets the contrast map again, scales up contrast map to 
        // original size and accumulates in original input map. Repeat several times.
        // Arguably, it could be computed more efficiently, but i don't care 
        // optimizing code that will still have to be changed several times.
        public static FloatMap GetMultiResContrastEvaluation(FloatMap imgfIn, int subSamples)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            float k = 1;

            FloatMap contr = new FloatMap(w, h);
            var img = imgfIn;
            for (int i = 0; i < subSamples; ++i)
            {
                var rc = ResizeMap(BlurMap(GetContrastMap(img)), w, h);
                //var rc = DoubleImg(BlurImg(GetContrImg(img)), i); // <-- multi step reduction would still be first choice with openCl
                Accumulate(contr, rc, k);
                k *= 0.5f;
                img = HalfMap(img);
            }
            return contr;
        }

        // Resizes map to arbitrary size
        private static FloatMap ResizeMap(FloatMap imgfIn, int dstW, int dstH)
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

        // Sets each pixel P in accumulation map to PValue + QValue, 
        // where Q is corresponding pixel in the other provided map
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

        // Returns maximum value in a map;
        // used to show normalized bitmaps
        public static float GetMapMax(FloatMap imgfIn) 
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            float max = float.MinValue;
            float min = float.MaxValue;
            
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
                    
                    // debug only
                    if ((v > 0) && (v < min))
                    {
                        min = v;
                    }

                    i += 1;
                }
                lineStart += stride;
            }

            Console.WriteLine("\n\nmin: {0}\nmax: {1}\n\n", min, max);

            return max;
        }

        // Returns a map of focus ranks, defined as follows:
        //  for each pixel C of input image (but borders) 
        //      for each pixel N of center's neighborhood
        //          accumulate |C-P| * W where W is a weight depending on distance between C and N
        public static FloatMap GetContrastMap(FloatMap imgfIn)
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

        // Returns an image which sizes are each 1/2^times the original value;
        // will be probably replaced with reduction/expansion-style OpenCl operation
        public static FloatMap HalfMap(FloatMap imgfIn, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                imgfIn = HalfMap(imgfIn);
            }
            return imgfIn;
        }

        // Returns an image which sizes are each half the original value;
        // will be probably replaced with reduction/expansion-style OpenCl operation
        public static FloatMap HalfMap(FloatMap imgfIn)
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

        // Returns an image which sizes are each 2^times the original value;
        // will be probably replaced with reduction/expansion-style
        // OpenCl operation
        public static FloatMap DoubleMap(FloatMap imgfIn, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                imgfIn = DoubleMap(imgfIn);
            }
            return imgfIn;
        }

        // returns an image which sizes are each double the original value;
        // will be probably replaced with reduction/expansion-style
        // OpenCl operation
        public static FloatMap DoubleMap(FloatMap imgfIn)
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

        // Blurs image; this has been written quickly and without reference;
        // should be modified in order to use a true gaussian kernel;
        // is left as is because shortly all convolution-like functions
        // will be handled by a single method (possibly with OpenCL)
        public static FloatMap BlurMap(FloatMap imgfIn)
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

        // Converts from bitmap 32bpp ARGB to float map
        public static FloatMap Bmp2Map(Bitmap bmp)
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
                        // considers Y component; in future, it would be nice to let user choose between single channels, luma, average, ...
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

        // Converts from float map to bitmap 32bpp ARGB
        public static Bitmap Map2Bmp(FloatMap imgf, float k)
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

        // Creates the blue-red depht map
        public static Bitmap Map2BmpDephtMap(FloatMap imgf, float k, int count)
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
                        v = v < 0 ? -1 : 255 - v * 255 / count;

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

        // Returns Y component
        static float getLuminosity(byte r, int g, int b)
        {
            return 0.299f * r + 0.587f * g + 0.114f * b;
        }

        // This is some sort of median filter, with the difference that
        // "outlier" values are just invalidated and left
        // for another step to be replaced with appropriate value
        // (this additional interpolation step still doesn't exist, though)
        public static FloatMap SpikesFilter(FloatMap imgfIn, float treshold)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            var imgfOut = new FloatMap(w, h);

            const float k = 0.70710678118654752440084436210485f; // w = 1/sqrt(2); lazy me, i just compied result of w/wtot from calc... i know we don't have that much detail in singles

            // TODO: Should handle -1s correctly here, sooner or later XD

            // copy borders directly from src to dst
            int yLin = (h-1) * stride;
            for (int x = 0; x < w; ++x)
            {
                imgfOut[x] = imgfIn[x];
                imgfOut[x + yLin] = imgfIn[x + yLin];
            }

            yLin = 0;
            for (int y = 0; y < h; ++y)
            {
                imgfOut[yLin] = imgfIn[yLin];
                imgfOut[yLin + stride - 1] = imgfIn[yLin + stride - 1];
                yLin += stride;
            }

            // visit each pixel not belonging to borders;
            // for each one, consider its value and the average value
            // of its neighborhood (weighted proportionally to distance
            // from center pixel): if |value-average|>treshold
            // pixel is invalidated (=-1)
            float neighborhoodWeight;
            float neighborhoodAccu;
            float v;
            int lineStart = stride;
            for (int y = 1; y < h - 1; ++y)
            {
                int i = lineStart + 1; ;
                for (int x = 1; x < w - 1; ++x)
                {
                    neighborhoodWeight = 0; 
                    neighborhoodAccu = 0;

                    // considering neighborhood pixels separately to correctly handle -1s

                    v = imgfIn[i + stride];
                    if (v > 0)
                    {
                        neighborhoodAccu += v;
                        neighborhoodWeight += 1;
                    }

                    v = imgfIn[i - stride];
                    if (v > 0)
                    {
                        neighborhoodAccu += v;
                        neighborhoodWeight += 1;
                    }

                    v = imgfIn[i + 1];
                    if (v > 0)
                    {
                        neighborhoodAccu += v;
                        neighborhoodWeight += 1;
                    }

                    v = imgfIn[i -1];
                    if (v > 0)
                    {
                        neighborhoodAccu += v;
                        neighborhoodWeight += 1;
                    }

                    v = imgfIn[i + stride + 1];
                    if (v > 0)
                    {
                        neighborhoodAccu += v * k;
                        neighborhoodWeight += k;
                    }
                    
                    v = imgfIn[i + stride -1];
                    if (v > 0)
                    {
                        neighborhoodAccu += v * k;
                        neighborhoodWeight += k;
                    }
                    
                    v = imgfIn[i - stride + 1];
                    if (v > 0)
                    {
                        neighborhoodAccu += v * k;
                        neighborhoodWeight += k;
                    }

                    v = imgfIn[i - stride - 1];
                    if (v > 0)
                    {
                        neighborhoodAccu += v * k;
                        neighborhoodWeight += k;
                    }

                    var d = Math.Abs(imgfIn[i] - (neighborhoodAccu / neighborhoodWeight));

                    imgfOut[i] = ((d > treshold) ? -1 : imgfIn[i]); // pixel value is just invalidated. A further step will take care of interpolation for missing value

                    ++i;
                }
                lineStart += stride;
            }
            return imgfOut;
        }

        // Code has been just replicated from SpikesFilter... 
        // this stuff is just helping me experiment
        // and won't be mantained "nice"
        // SpikesFilter doesn't just call GetSpike
        // for performance reasons, too
        public static float GetSpikeHeight(FloatMap imgfIn, int x, int y)
        {
            const float k = 0.70710678118654752440084436210485f; // w = 1/sqrt(2); lazy me, i just compied result of w/wtot from calc... i know we don't have that much detail in singles

            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            int lineStart = y * stride;

            int i = y * stride + x;

            
            float neighborhoodWeight = 0;
            float neighborhoodAccu = 0;
            float v;
            // considering neighborhood pixels separately to correctly handle -1s

            v = imgfIn[i + stride];
            if (v > 0)
            {
                neighborhoodAccu += v;
                neighborhoodWeight += 1;
            }

            v = imgfIn[i - stride];
            if (v > 0)
            {
                neighborhoodAccu += v;
                neighborhoodWeight += 1;
            }

            v = imgfIn[i + 1];
            if (v > 0)
            {
                neighborhoodAccu += v;
                neighborhoodWeight += 1;
            }

            v = imgfIn[i - 1];
            if (v > 0)
            {
                neighborhoodAccu += v;
                neighborhoodWeight += 1;
            }

            v = imgfIn[i + stride + 1];
            if (v > 0)
            {
                neighborhoodAccu += v * k;
                neighborhoodWeight += k;
            }

            v = imgfIn[i + stride - 1];
            if (v > 0)
            {
                neighborhoodAccu += v * k;
                neighborhoodWeight += k;
            }

            v = imgfIn[i - stride + 1];
            if (v > 0)
            {
                neighborhoodAccu += v * k;
                neighborhoodWeight += k;
            }

            v = imgfIn[i - stride - 1];
            if (v > 0)
            {
                neighborhoodAccu += v * k;
                neighborhoodWeight += k;
            }

            var d = Math.Abs(imgfIn[i] - (neighborhoodAccu / neighborhoodWeight));

            return d;
        }

        // Caps holes taking values (weighted by distance) from neighborhood
        // for interpolation; filter can cap holes as large as filterHalfSize*2;
        // multiple passes could be needed.
        public static FloatMap CapHoles(FloatMap imgfIn, int filterHalfSize)
        {
            bool thereAreStillHoles = true;
            while (thereAreStillHoles)
            {
                imgfIn = MapUtils.capHoles(imgfIn, filterHalfSize, out thereAreStillHoles);
            }
            return imgfIn;
        }

        static FloatMap capHoles(FloatMap imgfIn, int filterHalfSize, out bool thereAreStillHoles)
        {
            thereAreStillHoles = false;
            FloatMap filter = getDistanceWeightMap(filterHalfSize);
            int filterSize = filterHalfSize * 2 +1; 

            int h = imgfIn.H;
            int w = imgfIn.W;

            var imgfOut = new FloatMap(w, h);

            for (int y = 0; y < h ; ++y)
            {
                for (int x = 0; x < w; ++x)
                {
                    // if point in [x,y] == -1 then cap that:
                    // foreach a,b from x-filtersize to x+filtersize (same for y)
                    // if that point is != -1 accumulate that * weight[a,b], accumulate weight[a,b]

                    if (imgfIn[x, y] < 0) // --> need to cap
                    {
                        float accu = 0;
                        float wAccu = 0;
                        int cMinX = x - filterHalfSize;
                        int cMinY = y - filterHalfSize;
                        int minX = Math.Max(0, cMinX);
                        int minY = Math.Max(0, cMinY);
                        int xOffs = minX - cMinX; 
                        int yOffs = minY - cMinY;
                        int maxX = Math.Min(w, x + filterHalfSize);
                        int maxY = Math.Min(h, y + filterHalfSize);
                        for (int b = minY, fb = yOffs; b < maxY; ++b, ++fb)
                        {
                            for (int a = minX, fa = xOffs; a < maxX; ++a, ++fa)
                            {
                                float v = imgfIn[a, b];
                                if (v >= 0)
                                {
                                    float weight = filter[fa, fb];
                                    wAccu += weight;
                                    accu += v * weight;
                                }
                            }
                        }

                        if (wAccu != 0)
                        {
                            imgfOut[x, y] = accu / wAccu;
                        }
                        else
                        {
                            imgfOut[x, y] = -1;
                            thereAreStillHoles = true;
                        }
                    }
                    else
                    {
                        imgfOut[x, y] = imgfIn[x,y];
                    }
                    
                }
            }

            return imgfOut;
        }

        private static FloatMap getDistanceWeightMap(int filterHalfSize)
        {
            int size = filterHalfSize * 2 + 1;
            int sup = size - 1;
            FloatMap wMap = new FloatMap(size, size);
            for (int y = 0; y < filterHalfSize; ++y)
            {
                for (int x = 0; x <= filterHalfSize; ++x)
                {
                    float dx = (filterHalfSize - x);
                    float dy = (filterHalfSize - y);
                    wMap[x, y] = wMap[y, sup - x] = wMap[sup - y, x] = wMap[sup - x, sup - y] = (float)(1.0/Math.Sqrt(dx * dx + dy * dy));
                }
            }

            wMap[filterHalfSize, filterHalfSize] = 0;

            return wMap;
        }
    }
}
