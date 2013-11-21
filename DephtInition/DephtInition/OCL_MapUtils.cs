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

using OpenCL.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace DepthInition
{
    // © by Giancarlo Todone 2013
    //
    // This is IDIMapComputer implementation that uses OpenCl to speed up computations
    // By now chances are you're not going to see that drastic speed improvement, as 
    // this is just an initial word-by-word translation to OpenCl (thankfully the code
    // is intrinsically SIMT oriented since its inception)
    //
    // There are TONS of improvement still to do:
    // - separate separable functions (that incidentally are the vast majority)
    // - use builtins and fast approximate functions
    // - optimize memory management by reducing swap between main memory and device (no shared is used ATM)
    // - use vector math to crunch more operations in the same time
    // - being generally smarter in some tasks
    // - clean and make maintainable the code (lots of copy-n-paste have been used.. ugh!)
    //
    class OCL_MapUtils : IDIMapComputer
    {
        static readonly IntPtr intPtrSize = (IntPtr)Marshal.SizeOf(typeof(IntPtr));
        static readonly IntPtr intSize = (IntPtr)Marshal.SizeOf(typeof(int));
        static readonly IntPtr floatSize = (IntPtr)Marshal.SizeOf(typeof(float));

        Device _device;
        Context _context;
        OpenCL.Net.Program _program;
        ErrorCode err;
        CommandQueue _commandsQueue;
        Dictionary<string, Kernel> _kernels = new Dictionary<string, Kernel>();

        public OCL_MapUtils()
        {
            init(@"MapUtils.cl");
        }

        public void Dispose()
        {
            finish();
        }

        #region openCL specific stuff

        private void assert(ErrorCode err, string message)
        {
            if (err != ErrorCode.Success)
            {
                throw new Exception("error:" + err + "; " + message);
            }
        }

        private void ContextNotify(string errInfo, byte[] data, IntPtr cb, IntPtr userData)
        {
            Debug.WriteLine("OpenCL Notification: " + errInfo);
        }

        private void init(string oclProgramSourcePath)
        {
            string kernelSource = File.ReadAllText(oclProgramSourcePath);

            string[] kernelNames = new string[] { "getY", "halfSizeMap", "doubleSizeMap", "getBorder", "quickBlur", "capHoles", "convolve", "accumulate", "upsizel" };

            bool gpu = true;
            //err = clGetDeviceIDs(NULL, gpu ? CL_DEVICE_TYPE_GPU : CL_DEVICE_TYPE_CPU, 1, &device_id, NULL); 
            // NVidia driver doesn't seem to support a NULL first param (properties)
            // http://stackoverflow.com/questions/19140989/how-to-remove-cl-invalid-platform-error-in-opencl-code

            // now get all the platform IDs
            Platform[] platforms = Cl.GetPlatformIDs(out err);
            assert(err, "Error: Failed to get platform ids!");

            InfoBuffer deviceInfo = Cl.GetPlatformInfo(platforms[0], PlatformInfo.Name, out err);
            assert(err, "error retrieving platform name");
            Console.WriteLine("Platform name: {0}\n", deviceInfo.ToString());


            //                                 Arbitrary, should be configurable
            Device[] devices = Cl.GetDeviceIDs(platforms[0], gpu ? DeviceType.Gpu : DeviceType.Cpu, out err);
            assert(err, "Error: Failed to create a device group!");

            _device = devices[0]; // Arbitrary, should be configurable

            deviceInfo = Cl.GetDeviceInfo(_device, DeviceInfo.Name, out err);
            assert(err, "error retrieving device name");
            Debug.WriteLine("Device name: {0}", deviceInfo.ToString());

            deviceInfo = Cl.GetDeviceInfo(_device, DeviceInfo.ImageSupport, out err);
            assert(err, "error retrieving device image capability");
            Debug.WriteLine("Device supports img: {0}", (deviceInfo.CastTo<Bool>() == Bool.True));

            // Create a compute context 
            //
            _context = Cl.CreateContext(null, 1, new[] { _device }, ContextNotify, IntPtr.Zero, out err);
            assert(err, "Error: Failed to create a compute context!");

            // Create the compute program from the source buffer
            //
            _program = Cl.CreateProgramWithSource(_context, 1, new[] { kernelSource }, new[] { (IntPtr)kernelSource.Length }, out err);
            assert(err, "Error: Failed to create compute program!");

            // Build the program executable
            //
            err = Cl.BuildProgram(_program, 1, new[] { _device }, string.Empty, null, IntPtr.Zero);
            assert(err, "Error: Failed to build program executable!");
            InfoBuffer buffer = Cl.GetProgramBuildInfo(_program, _device, ProgramBuildInfo.Log, out err);
            Debug.WriteLine("build success: {0}", buffer.CastTo<BuildStatus>() == BuildStatus.Success);

            foreach (string kernelName in kernelNames)
            {
                // Create the compute kernel in the program we wish to run
                //
                OpenCL.Net.Kernel kernel = Cl.CreateKernel(_program, kernelName, out err);
                assert(err, "Error: Failed to create compute kernel!");
                _kernels.Add(kernelName, kernel);
            }

            // Create a command queue
            //
            _commandsQueue = Cl.CreateCommandQueue(_context, _device, CommandQueueProperties.None, out err);
            assert(err, "Error: Failed to create a command commands!");
        }

        private void finish()
        {
            //Clean up memory
            foreach (KeyValuePair<string, Kernel> k in _kernels)
            {
                Cl.ReleaseKernel(k.Value);
            }

            Cl.ReleaseCommandQueue(_commandsQueue);
            Cl.ReleaseProgram(_program);
            Cl.ReleaseContext(_context);

        }

        private FloatMap invokeMapKernel(Kernel kernel, FloatMap inMap)
        {
            return invokeMapKernel(kernel, inMap, inMap.W, inMap.H, inMap.W, inMap.H, 0, 0);
        }

        private FloatMap invokeMapKernel(Kernel kernel, FloatMap inMap, int outX, int outY)
        {
            return invokeMapKernel(kernel, inMap, outX, outY, outX, outY, 0, 0);
        }

        private FloatMap invokeMapKernel(Kernel kernel, FloatMap inMap, int outX, int outY, int rangeX, int rangeY, int offsX, int offsY, params object[] additionalParams)
        {
            // Creation of on-device memory objects
            IMem<float> inputMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.ReadOnly, inMap.Size, out err);
            assert(err, "input map creation");

            FloatMap outMap = new FloatMap(outX, outY);

            IMem<float> outputMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.WriteOnly, outMap.Size, out err);
            assert(err, "output buf creation");

            // Set memory objects as parameters to kernel
            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputMapBuffer);
            assert(err, "input map setKernelArg");

            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputMapBuffer);
            assert(err, "output map setKernelArg");

            err = Cl.SetKernelArg(kernel, 2, intSize, inMap.Stride);
            assert(err, "in stride setKernelArg");

            err = Cl.SetKernelArg(kernel, 3, intSize, outMap.Stride);
            assert(err, "out stride setKernelArg");

            uint idx = 4;
            foreach (object o in additionalParams)
            {
                if (o is int)
                {
                    err = Cl.SetKernelArg(kernel, idx++, intSize, (int)o);
                    assert(err, "additional int param " + idx + " setKernelArg");
                }

                if (o is float)
                {
                    err = Cl.SetKernelArg(kernel, idx++, floatSize, (float)o);
                    assert(err, "additional float param " + idx + " setKernelArg");
                }
            }

            // write actual data into memory object
            Event clevent;
            err = Cl.EnqueueWriteBuffer<float>(_commandsQueue, inputMapBuffer, Bool.True, 0, inMap.Size, inMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "write input buffer");

            // execute
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2,
                new[] { new IntPtr(offsX), new IntPtr(offsY), (IntPtr)0 },  // offset
                new[] { new IntPtr(rangeX), new IntPtr(rangeY), (IntPtr)1 }, // range
                null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            // sync
            err = Cl.Finish(_commandsQueue);
            assert(err, "Cl.Finish");

            // read from output memory object into actual buffer
            err = Cl.EnqueueReadBuffer<float>(_commandsQueue, outputMapBuffer, Bool.True, outMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            err = Cl.ReleaseMemObject(inputMapBuffer);
            assert(err, "releasing inputMapBuffer mem object");
            err = Cl.ReleaseMemObject(outputMapBuffer);
            assert(err, "releasing outputMapBuffer mem object");

            return outMap;
        }

        public FloatMap invokeConvolve(Kernel kernel, FloatMap inMap, FloatMap mask)
        {
            int maskSize = (mask.W - 1) / 2;

            // Creation of on-device memory objects
            IMem<float> inputMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.ReadOnly, inMap.Size, out err);
            assert(err, "input map creation");

            FloatMap outMap = new FloatMap(inMap.W, inMap.H);

            IMem<float> outputMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.WriteOnly, outMap.Size, out err);
            assert(err, "output buf creation");

            IMem<float> maskBuffer = Cl.CreateBuffer<float>(_context, MemFlags.ReadOnly | MemFlags.CopyHostPtr, mask._buf, out err);
            assert(err, "convolve mask, mem object creation");

            // Set memory objects as parameters to kernel
            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputMapBuffer);
            assert(err, "input map setKernelArg");

            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputMapBuffer);
            assert(err, "output map setKernelArg");

            err = Cl.SetKernelArg(kernel, 2, intSize, inMap.Stride);
            assert(err, "in stride setKernelArg");

            err = Cl.SetKernelArg(kernel, 3, intSize, outMap.Stride);
            assert(err, "out stride setKernelArg");

            err = Cl.SetKernelArg(kernel, 4, intPtrSize, maskBuffer);
            assert(err, "convolve mask, setKernelArg");

            err = Cl.SetKernelArg(kernel, 5, intSize, maskSize);
            assert(err, "convolve mask size, setKernelArg");

            // write actual data into memory object
            Event clevent;
            err = Cl.EnqueueWriteBuffer<float>(_commandsQueue, inputMapBuffer, Bool.True, 0, inMap.Size, inMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "write input buffer");

            // execute
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2,
                new[] { new IntPtr(maskSize), new IntPtr(maskSize), (IntPtr)0 },  // offset
                new[] { new IntPtr(outMap.W - maskSize * 2), new IntPtr(outMap.H - maskSize * 2), (IntPtr)1 }, // range
                null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            // sync
            Cl.Finish(_commandsQueue);

            // read from output memory object into actual buffer
            err = Cl.EnqueueReadBuffer<float>(_commandsQueue, outputMapBuffer, Bool.True, outMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            err = Cl.ReleaseMemObject(inputMapBuffer);
            assert(err, "releasing inputMapBuffer mem object");
            err = Cl.ReleaseMemObject(outputMapBuffer);
            assert(err, "releasing outputMapBuffer mem object");

            // do I have to release maskbuffer?

            return outMap;
        }

        #endregion // openCL specific stuff

        #region mask creation stuff

        private FloatMap createBlurMask(float sigma)
        {
            int maskSize = (int)Math.Ceiling(3.0f * sigma);
            int _2maskSizePlus1 = (maskSize << 1) + 1; // stupid C# compiler gives precedence to sum
            FloatMap mask = new FloatMap(_2maskSizePlus1, _2maskSizePlus1);
            float sum = 0.0f;
            float temp = 0.0f;
            float _2sigmaSqrInvNeg = -1 / (sigma * sigma * 2);

            for (int a = -maskSize; a <= maskSize; ++a)
            {
                for (int b = -maskSize; b <= maskSize; ++b)
                {
                    temp = (float)Math.Exp(((float)(a * a + b * b) * _2sigmaSqrInvNeg));
                    sum += temp;
                    mask[a + maskSize + (b + maskSize) * _2maskSizePlus1] = temp;
                }
            }

            // Normalize the mask
            int _2maskSizePlus1Sqr = _2maskSizePlus1 * _2maskSizePlus1;
            for (int i = 0; i < _2maskSizePlus1Sqr; ++i)
            {
                mask[i] = mask[i] / sum;
            }

            return mask;
        }

        private FloatMap getDistanceWeightMap(int filterHalfSize)
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
                    wMap[x, y] = wMap[y, sup - x] = wMap[sup - y, x] = wMap[sup - x, sup - y] = (float)(1.0 / Math.Sqrt(dx * dx + dy * dy));
                }
            }

            wMap[filterHalfSize, filterHalfSize] = 0;

            return wMap;
        }

        #endregion // mask creation stuff

        public FloatMap GetMultiResContrastEvaluation(FloatMap imgfIn, int subSamples)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            float k = 1;

            FloatMap contr = new FloatMap(w, h);
            var img = imgfIn;
            for (int i = 0; i < subSamples; ++i)
            {
                var rc = ResizeMap(QuickBlurMap(GetContrastMap(img)), w, h);
                //var rc = DoubleImg(BlurImg(GetContrImg(img)), i); // <-- multi step reduction would still be first choice with openCl
                Accumulate(contr, rc, k);
                k *= 0.5f;
                img = HalfMap(img);
            }
            return contr;
        }

        public FloatMap ResizeMap(FloatMap imgfIn, int dstW, int dstH)
        {
            int srcH = imgfIn.H;
            int srcW = imgfIn.W;

            float xk = (float)srcW / (float)dstW;
            float yk = (float)srcH / (float)dstH;

            return invokeMapKernel(_kernels["upsizel"], imgfIn, dstW, dstH, dstW, dstH, 0, 0, xk, yk);
        }

        public FloatMap Bmp2Map(Bitmap bmpImage)
        {
            var kernel = _kernels["getY"];

            int w = bmpImage.Width;
            int h = bmpImage.Height;

            BitmapData bitmapData = bmpImage.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int inputImgStride = bitmapData.Stride;
            int inputImgBytesSize = bitmapData.Stride * bitmapData.Height;

            //Copy the raw bitmap data to an unmanaged byte[] array
            byte[] inputByteArray = new byte[inputImgBytesSize];
            Marshal.Copy(bitmapData.Scan0, inputByteArray, 0, inputImgBytesSize);

            var clInImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);

            //Allocate OpenCL image memory buffer
            IMem inputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, clInImageFormat,
                                                (IntPtr)bitmapData.Width, (IntPtr)bitmapData.Height,
                                                (IntPtr)0, inputByteArray, out err);

            assert(err, "input img creation");

            FloatMap outMap = new FloatMap(w, h);
            IMem<float> outputMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.WriteOnly, outMap.Size, out err);
            assert(err, "output buf creation");

            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputImage2DBuffer);
            assert(err, "input img setKernelArg");
            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputMapBuffer);
            assert(err, "output img setKernelArg");
            err = Cl.SetKernelArg(kernel, 2, intSize, outMap.Stride);
            assert(err, "output stride setKernelArg");


            Event clevent;
            //Copy input image from the host to the GPU.
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };    //x, y, z
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };    //x, y, z
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };
            err = Cl.EnqueueWriteImage(_commandsQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, inputByteArray, 0, null, out clevent);
            assert(err, "Cl.EnqueueWriteImage");

            //Execute our kernel (OpenCL code)
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            //Wait for completion of all calculations on the GPU.
            err = Cl.Finish(_commandsQueue);
            assert(err, "Cl.Finish");


            err = Cl.EnqueueReadBuffer<float>(_commandsQueue, outputMapBuffer, Bool.True, outMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            Cl.ReleaseMemObject(inputImage2DBuffer);
            Cl.ReleaseMemObject(outputMapBuffer);

            return outMap;
        }

        // Still the simple one
        public Bitmap Map2Bmp(FloatMap imgf, float k)
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
                    for (int x = 0; x < wb; x += pixelSize)
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

        // Creates the blue-red depth map; Still the simple one
        public Bitmap Map2BmpFauxColors(FloatMap imgf, float k, int count)
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
                    for (int x = 0; x < wb; x += 4)
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

        public FloatMap HalfMap(FloatMap imgfIn, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                imgfIn = HalfMap(imgfIn);
            }
            return imgfIn;
        }

        public FloatMap HalfMap(FloatMap inMap)
        {
            return invokeMapKernel(_kernels["halfSizeMap"], inMap, inMap.W / 2, inMap.H / 2);
        }

        public FloatMap DoubleMap(FloatMap imgfIn, int times)
        {
            for (int i = 0; i < times; ++i)
            {
                imgfIn = DoubleMap(imgfIn);
            }
            return imgfIn;
        }

        public FloatMap DoubleMap(FloatMap inMap)
        {
            return invokeMapKernel(_kernels["doubleSizeMap"], inMap, inMap.W * 2, inMap.H * 2);
        }

        public FloatMap GetContrastMap(FloatMap inMap)
        {
            //return invokeMapKernel(_kernels["getBorder"], inMap);
            return invokeMapKernel(_kernels["getBorder"], inMap, inMap.W, inMap.H, inMap.W - 2, inMap.H - 2, 1, 1);
        }

        public FloatMap QuickBlurMap(FloatMap inMap)
        {
            // return invokeMapKernel(_kernels["quickBlur"], inMap);
            return invokeMapKernel(_kernels["quickBlur"], inMap, inMap.W, inMap.H, inMap.W - 2, inMap.H - 2, 1, 1);
        }

        public FloatMap GaussianBlur(FloatMap inMap, float sigma)
        {
            return invokeConvolve(_kernels["convolve"], inMap, createBlurMask(sigma));
        }

        public FloatMap CapHoles(FloatMap imgfIn, int filterHalfSize)
        {
            var mask = getDistanceWeightMap(filterHalfSize);
            var kernel = _kernels["capHoles"];
            for (int i = 0; i < 5; ++i) // since it's very difficult to set a bool (result of an or for each map element) from the kernel, let's just do it an arbitrary amount of times
            {
                imgfIn = invokeConvolve(kernel, imgfIn, mask);
            }
            return imgfIn;
        }

        private FloatMap capHoles(FloatMap imgfIn, int filterHalfSize)
        {
            return invokeConvolve(_kernels["capHoles"], imgfIn, getDistanceWeightMap(filterHalfSize));
        }

        // not so tailored for SIMT...
        public void Accumulate(FloatMap imgfAccu, FloatMap imgfSrc, float k)
        {
            var kernel = _kernels["accumulate"];

            // Creation of on-device memory objects
            IMem<float> accuMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.ReadWrite, imgfAccu.Size, out err);
            assert(err, "accu buf creation");

            IMem<float> srcMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.WriteOnly, imgfSrc.Size, out err);
            assert(err, "src buf creation");

            // Set memory objects as parameters to kernel
            err = Cl.SetKernelArg(kernel, 0, intPtrSize, accuMapBuffer);
            assert(err, "accu map setKernelArg");

            err = Cl.SetKernelArg(kernel, 1, intPtrSize, srcMapBuffer);
            assert(err, "src map setKernelArg");

            err = Cl.SetKernelArg(kernel, 2, intSize, imgfAccu.Stride);
            assert(err, "in stride setKernelArg");

            err = Cl.SetKernelArg(kernel, 3, intSize, imgfSrc.Stride);
            assert(err, "out stride setKernelArg");

            err = Cl.SetKernelArg(kernel, 4, floatSize, k);
            assert(err, "out stride setKernelArg");

            // write actual data into memory object
            Event clevent;
            err = Cl.EnqueueWriteBuffer<float>(_commandsQueue, accuMapBuffer, Bool.True, 0, imgfAccu.Size, imgfAccu._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "write accu buffer");

            err = Cl.EnqueueWriteBuffer<float>(_commandsQueue, srcMapBuffer, Bool.True, 0, imgfSrc.Size, imgfSrc._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "write src buffer");

            // execute
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2,
                new[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 },  // offset
                new[] { new IntPtr(imgfAccu.W), new IntPtr(imgfAccu.H), (IntPtr)1 }, // range
                null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            // sync
            Cl.Finish(_commandsQueue);

            // read from output memory object into actual buffer
            err = Cl.EnqueueReadBuffer<float>(_commandsQueue, accuMapBuffer, Bool.True, imgfAccu._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            err = Cl.ReleaseMemObject(accuMapBuffer);
            assert(err, "releasing accuMapBuffer mem object");
            err = Cl.ReleaseMemObject(srcMapBuffer);
            assert(err, "releasing srcMapBuffer mem object");
        }

        // could do this by reduction; in the meanwhile, the old simple method s provided
        public float GetMapMax(FloatMap imgfIn)
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

            //Console.WriteLine("\n\nmin: {0}\nmax: {1}\n\n", min, max);

            return max;
        }

        // still have to implement this OCL-wise; didn't urge to do so as it's called only once
        public FloatMap SpikesFilter(FloatMap imgfIn, float treshold)
        {
            int h = imgfIn.H;
            int w = imgfIn.W;
            int stride = imgfIn.Stride;

            var imgfOut = new FloatMap(w, h);

            const float k = 0.70710678118654752440084436210485f; // w = 1/sqrt(2); lazy me, i just copied result of w/wtot from calc... i know we don't have that much detail in singles

            // TODO: Should handle -1s correctly here, sooner or later XD

            // copy borders directly from src to dst
            int yLin = (h - 1) * stride;
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

                    imgfOut[i] = ((d > treshold) ? -1 : imgfIn[i]); // pixel value is just invalidated. A further step will take care of interpolation for missing value

                    ++i;
                }
                lineStart += stride;
            }
            return imgfOut;
        }
    }
}

