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
    class OCL_MapUtils_img : IDIMapComputer
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

        public OCL_MapUtils_img()
        {
            init(@"MapUtilsImg.cl");
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

            string[] kernelNames = new string[] { "accumulate", "quickBlurImgH", "quickBlurImgV", "upsizeImg", "halfSizeImgH", "halfSizeImgV", "getLumaImg", "mapToGreyscaleBmp", "getContrastImg", "capHolesImg", "maxReduceImgH", "maxReduceImgV", "mapToFauxColorsBmp", "quickSpikesFilterImg", "convolveImg" };

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

        public void singlePass(Kernel kernel, FloatMap inMap, FloatMap outMap)
        {
            var clInImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.Luminance, ChannelType.Float);

            IMem inputMapBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, clInImageFormat,
                                                (IntPtr)inMap.W, (IntPtr)inMap.H, new IntPtr(inMap.Stride * sizeof(float)),
                                                inMap._buf, out err);

            assert(err, "input img creation");


            IMem outputMapBuffer = Cl.CreateImage2D(_context, MemFlags.WriteOnly, clInImageFormat,
                                                (IntPtr)outMap.W, (IntPtr)outMap.H, new IntPtr(outMap.Stride * sizeof(float)),
                                                outMap._buf, out err);

            assert(err, "output img creation");


            // Set memory objects as parameters to kernel
            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputMapBuffer);
            assert(err, "input map setKernelArg");

            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputMapBuffer);
            assert(err, "output map setKernelArg");


            // write actual data into memory object
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };    //x, y, z
            IntPtr[] inRegionPtr = new IntPtr[] { (IntPtr)inMap.W, (IntPtr)inMap.H, (IntPtr)1 };    //x, y, z
            IntPtr[] outRegionPtr = new IntPtr[] { (IntPtr)outMap.W, (IntPtr)outMap.H, (IntPtr)1 };    //x, y, z
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)outMap.W, (IntPtr)outMap.H, (IntPtr)1 };
            Event clevent;
            //err = Cl.EnqueueWriteImage(_commandsQueue, inputMapBuffer, Bool.True, originPtr, inRegionPtr, (IntPtr)0, (IntPtr)0, inMap._buf, 0, null, out clevent);
            //clevent.Dispose();
            //assert(err, "write input img");

            // execute
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2,
                originPtr,
                workGroupSizePtr,
                null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            // sync
            Cl.Finish(_commandsQueue);

            // read from output memory object into actual buffer
            err = Cl.EnqueueReadImage(_commandsQueue, outputMapBuffer, Bool.True, originPtr, outRegionPtr, new IntPtr(outMap.Stride * sizeof(float)), (IntPtr)0, outMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            Cl.ReleaseMemObject(inputMapBuffer);
            Cl.ReleaseMemObject(outputMapBuffer);
        }

        public void doublePass(Kernel kernel1, Kernel kernel2, FloatMap inMap, FloatMap outMap)
        {
            doublePass(kernel1, kernel2, inMap, outMap, inMap.W, inMap.H);
        }

        public void doublePass(Kernel kernel1, Kernel kernel2, FloatMap inMap, FloatMap outMap, int tmpX, int tmpY)
        {
            var clInImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.Luminance, ChannelType.Float);

            IMem inputMapBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, clInImageFormat,
                                                (IntPtr)inMap.W, (IntPtr)inMap.H, new IntPtr(inMap.Stride * sizeof(float)),
                                                inMap._buf, out err);

            assert(err, "input img creation");


            IMem tmpBuffer = Cl.CreateImage2D(_context, MemFlags.AllocHostPtr | MemFlags.ReadWrite, clInImageFormat,
                                                (IntPtr)tmpX, (IntPtr)tmpY, (IntPtr)0,
                                                null, out err);

            assert(err, "temp img creation");


            IMem outputMapBuffer = Cl.CreateImage2D(_context, MemFlags.WriteOnly, clInImageFormat,
                                                (IntPtr)outMap.W, (IntPtr)outMap.H, new IntPtr(outMap.Stride * sizeof(float)),
                                                outMap._buf, out err);

            assert(err, "output img creation");


            // Set memory objects as parameters to kernel
            err = Cl.SetKernelArg(kernel1, 0, intPtrSize, inputMapBuffer);
            assert(err, "input map setKernelArg");

            err = Cl.SetKernelArg(kernel1, 1, intPtrSize, tmpBuffer);
            assert(err, "output map setKernelArg");


            // Set memory objects as parameters to kernel2
            err = Cl.SetKernelArg(kernel2, 0, intPtrSize, tmpBuffer);
            assert(err, "input map setKernelArg");

            err = Cl.SetKernelArg(kernel2, 1, intPtrSize, outputMapBuffer);
            assert(err, "output map setKernelArg");


            // write actual data into memory object
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };    //x, y, z
            IntPtr[] inRegionPtr = new IntPtr[] { (IntPtr)inMap.W, (IntPtr)inMap.H, (IntPtr)1 };    //x, y, z
            IntPtr[] outRegionPtr = new IntPtr[] { (IntPtr)outMap.W, (IntPtr)outMap.H, (IntPtr)1 };    //x, y, z
            IntPtr[] workGroupSizePtr1 = new IntPtr[] { (IntPtr)tmpX, (IntPtr)tmpY, (IntPtr)1 };
            IntPtr[] workGroupSizePtr2 = new IntPtr[] { (IntPtr)outMap.W, (IntPtr)outMap.H, (IntPtr)1 };
            Event clevent;
            //err = Cl.EnqueueWriteImage(_commandsQueue, inputMapBuffer, Bool.True, originPtr, inRegionPtr, (IntPtr)0, (IntPtr)0, inMap._buf, 0, null, out clevent);
            //clevent.Dispose();
            //assert(err, "write input img");

            // execute
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel1, 2,
                originPtr,
                workGroupSizePtr1,
                null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            // execute 2
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel2, 2,
                originPtr,
                workGroupSizePtr2,
                null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel 2");

            // sync
            Cl.Finish(_commandsQueue);

            err = Cl.EnqueueReadImage(_commandsQueue, outputMapBuffer, Bool.True, originPtr, outRegionPtr, new IntPtr(outMap.Stride * sizeof(float)), (IntPtr)0, outMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            Cl.ReleaseMemObject(inputMapBuffer);
            Cl.ReleaseMemObject(tmpBuffer);
            Cl.ReleaseMemObject(outputMapBuffer);
        }

        public FloatMap Bmp2Map(Bitmap bmpImage)
        {
            var kernel = _kernels["getLumaImg"];

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

            var clOutImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.Luminance, ChannelType.Float);
            FloatMap outMap = new FloatMap(w, h);
            IMem outputMapBuffer = Cl.CreateImage2D(_context, MemFlags.WriteOnly, clOutImageFormat,
                                                (IntPtr)outMap.W, (IntPtr)outMap.H, new IntPtr(outMap.Stride * sizeof(float)),
                                                outMap._buf, out err);

            assert(err, "output img creation");

            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputImage2DBuffer);
            assert(err, "input img setKernelArg");
            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputMapBuffer);
            assert(err, "output img setKernelArg");

            Event clevent;
            //Copy input image from the host to the GPU.
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };    //x, y, z
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };    //x, y, z
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };

            //err = Cl.EnqueueWriteImage(_commandsQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, inputByteArray, 0, null, out clevent);
            //assert(err, "Cl.EnqueueWriteImage");

            //Execute our kernel (OpenCL code)
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            //Wait for completion of all calculations on the GPU.
            err = Cl.Finish(_commandsQueue);
            assert(err, "Cl.Finish");

            err = Cl.EnqueueReadImage(_commandsQueue, outputMapBuffer, Bool.True, originPtr, regionPtr, new IntPtr(outMap.Stride * sizeof(float)), (IntPtr)0, outMap._buf, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            Cl.ReleaseMemObject(inputImage2DBuffer);
            Cl.ReleaseMemObject(outputMapBuffer);

            return outMap;
        }

        public Bitmap Map2Bmp(FloatMap inMap, float k)
        {
            var kernel = _kernels["mapToGreyscaleBmp"];

            int w = inMap.W;
            int h = inMap.H;

            var clInImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.Luminance, ChannelType.Float);

            IMem inputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, clInImageFormat,
                                                (IntPtr)w, (IntPtr)h,
                                                new IntPtr(inMap.Stride * sizeof(float)), inMap._buf, out err);

            assert(err, "input img creation");


            var clOutImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);

            IMem outputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.WriteOnly, clOutImageFormat,
                                                (IntPtr)w, (IntPtr)h,
                                                (IntPtr)0, (IntPtr)0, out err);

            assert(err, "output img creation");

            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputImage2DBuffer);
            assert(err, "input img setKernelArg");
            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputImage2DBuffer);
            assert(err, "output img setKernelArg");
            err = Cl.SetKernelArg(kernel, 2, floatSize, k);
            assert(err, "k setKernelArg");

            Event clevent;
            //x, y, z
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };

            //err = Cl.EnqueueWriteImage(_commandsQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, inputByteArray, 0, null, out clevent);
            //assert(err, "Cl.EnqueueWriteImage");

            //Execute  kernel 
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            //Wait for completion 
            err = Cl.Finish(_commandsQueue);
            assert(err, "Cl.Finish");

            // get data back
            var outputByteArray = new byte[w * h * 4];
            err = Cl.EnqueueReadImage(_commandsQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            Cl.ReleaseMemObject(inputImage2DBuffer);
            Cl.ReleaseMemObject(outputImage2DBuffer);

            GCHandle pinnedOutputArray = GCHandle.Alloc(outputByteArray, GCHandleType.Pinned);
            IntPtr outputBmpPointer = pinnedOutputArray.AddrOfPinnedObject();

            var bmp = new Bitmap(w, h, w * 4, PixelFormat.Format32bppArgb, outputBmpPointer);
            pinnedOutputArray.Free();

            return bmp;
        }

        public FloatMap GaussianBlur(FloatMap inMap, float sigma)
        {
            var k = _kernels["convolveImg"];
            FloatMap outMap = new FloatMap(inMap.W, inMap.H);

            FloatMap mask = createBlurMask(sigma);

            IMem<float> maskBuf = Cl.CreateBuffer<float>(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, mask._buf, out err);
            assert(err, "capholes mask, mem object creation");

            err = Cl.SetKernelArg(k, 2, intPtrSize, maskBuf);
            assert(err, "capholes mask, setKernelArg");

            err = Cl.SetKernelArg(k, 3, intSize, (mask.W - 1) / 2);
            assert(err, "capholes mask, setKernelArg");

            singlePass(k, inMap, outMap);

            return outMap;

        }

        public FloatMap QuickBlurMap(FloatMap inMap)
        {
            var k1 = _kernels["quickBlurImgH"];
            var k2 = _kernels["quickBlurImgV"];
            FloatMap outMap = new FloatMap(inMap.W, inMap.H);

            doublePass(k1, k2, inMap, outMap);

            return outMap;
        }

        public FloatMap ResizeMap(FloatMap inMap, int dstW, int dstH)
        {
            float xk = (float)inMap.W / (float)dstW;
            float yk = (float)inMap.H / (float)dstH;

            FloatMap outMap = new FloatMap(dstW, dstH);

            var k = _kernels["upsizeImg"];

            err = Cl.SetKernelArg(k, 2, floatSize, xk);
            assert(err, "xk setKernelArg");

            err = Cl.SetKernelArg(k, 3, floatSize, yk);
            assert(err, "yk setKernelArg");

            singlePass(k, inMap, outMap);

            return outMap;
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
            var k1 = _kernels["halfSizeImgH"];
            var k2 = _kernels["halfSizeImgV"];
            FloatMap outMap = new FloatMap(inMap.W / 2, inMap.H / 2);

            doublePass(k1, k2, inMap, outMap, inMap.W / 2, inMap.H);

            return outMap;
        }

        public FloatMap GetContrastMap(FloatMap inMap)
        {
            var k = _kernels["getContrastImg"];
            FloatMap outMap = new FloatMap(inMap.W, inMap.H);

            singlePass(k, inMap, outMap);

            return outMap;
        }

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
                Accumulate(contr, rc, k);
                k *= 0.5f;
                img = HalfMap(img);
            }
            return contr;
        }

        private FloatMap capHoles(FloatMap inMap, int filterHalfSize)
        {
            var k = _kernels["capHolesImg"];
            FloatMap outMap = new FloatMap(inMap.W, inMap.H);

            FloatMap mask = getDistanceWeightMap(filterHalfSize);

            IMem<float> maskBuf = Cl.CreateBuffer<float>(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, mask._buf, out err);
            assert(err, "capholes mask, mem object creation");

            err = Cl.SetKernelArg(k, 2, intPtrSize, maskBuf);
            assert(err, "capholes mask, setKernelArg");

            err = Cl.SetKernelArg(k, 3, intSize, filterHalfSize);
            assert(err, "capholes mask, setKernelArg");

            singlePass(k, inMap, outMap);

            Cl.ReleaseMemObject(maskBuf);

            return outMap;
        }

        // bleach!! should initialize two memory objects and ping-pong with them
        public FloatMap CapHoles(FloatMap inMap, int filterHalfSize)
        {
            for (int i = 0; i < 5; ++i) // since it's very difficult to set a bool (result of an or for each map element) from the kernel, let's just do it an arbitrary amount of times
            {
                inMap = capHoles(inMap, filterHalfSize);
            }

            return inMap;
        }

        // images cannot be read_write... so let's continue using plain buffers
        // should implement this in a way that allows imgfAccu to be loaded only once
        // should test for image size consistency
        public void Accumulate(FloatMap imgfAccu, FloatMap imgfSrc, float k)
        {
            var kernel = _kernels["accumulate"];

            // Creation of on-device memory objects
            IMem<float> accuMapBuffer = Cl.CreateBuffer<float>(_context, MemFlags.ReadWrite, imgfAccu.Size, out err); // why MemFlags.CopyHostPtr doesn't work here (and forces me to manual copy) ???
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

            Cl.ReleaseMemObject(srcMapBuffer);
            Cl.ReleaseMemObject(accuMapBuffer); // maybe i could return this without disposing; would affect non-OpenCl implementation
        }

        // again: should initialize two memory objects and ping-pong with them (but this method is not crytical, after all)
        public float GetMapMax(FloatMap inMap)
        {
            var k1 = _kernels["maxReduceImgH"];
            var k2 = _kernels["maxReduceImgV"];
            FloatMap outMap = new FloatMap((int)Math.Ceiling(inMap.W / 2.0f), (int)Math.Ceiling(inMap.H / 2.0f));

            while (true)
            {
                doublePass(k1, k2, inMap, outMap, (int)Math.Ceiling(inMap.W / 2.0f), inMap.H);
                if ((outMap.W == 1) && (outMap.W == 1)) return outMap[0, 0];
                inMap = outMap;
                outMap = new FloatMap((int)Math.Ceiling(inMap.W / 2.0f), (int)Math.Ceiling(inMap.H / 2.0f));
            }
        }

        // copypasted/modified from Map2Bmp
        public Bitmap Map2BmpFauxColors(FloatMap inMap, float k, int count) // have to deprecate use of count-->precomputed k
        {
            var kernel = _kernels["mapToFauxColorsBmp"];

            int w = inMap.W;
            int h = inMap.H;

            var clInImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.Luminance, ChannelType.Float);

            IMem inputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.CopyHostPtr | MemFlags.ReadOnly, clInImageFormat,
                                                (IntPtr)w, (IntPtr)h,
                                                new IntPtr(inMap.Stride * sizeof(float)), inMap._buf, out err);

            assert(err, "input img creation");


            var clOutImageFormat = new OpenCL.Net.ImageFormat(ChannelOrder.RGBA, ChannelType.Unsigned_Int8);

            IMem outputImage2DBuffer = Cl.CreateImage2D(_context, MemFlags.WriteOnly, clOutImageFormat,
                                                (IntPtr)w, (IntPtr)h,
                                                (IntPtr)0, (IntPtr)0, out err);

            assert(err, "output img creation");

            err = Cl.SetKernelArg(kernel, 0, intPtrSize, inputImage2DBuffer);
            assert(err, "input img setKernelArg");
            err = Cl.SetKernelArg(kernel, 1, intPtrSize, outputImage2DBuffer);
            assert(err, "output img setKernelArg");
            err = Cl.SetKernelArg(kernel, 2, floatSize, k/count);
            assert(err, "k setKernelArg");

            Event clevent;
            //x, y, z
            IntPtr[] originPtr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0 };
            IntPtr[] regionPtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };
            IntPtr[] workGroupSizePtr = new IntPtr[] { (IntPtr)w, (IntPtr)h, (IntPtr)1 };

            //err = Cl.EnqueueWriteImage(_commandsQueue, inputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, inputByteArray, 0, null, out clevent);
            //assert(err, "Cl.EnqueueWriteImage");

            //Execute  kernel 
            err = Cl.EnqueueNDRangeKernel(_commandsQueue, kernel, 2, null, workGroupSizePtr, null, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "Cl.EnqueueNDRangeKernel");

            //Wait for completion 
            err = Cl.Finish(_commandsQueue);
            assert(err, "Cl.Finish");

            // get data back
            var outputByteArray = new byte[w * h * 4];
            err = Cl.EnqueueReadImage(_commandsQueue, outputImage2DBuffer, Bool.True, originPtr, regionPtr, (IntPtr)0, (IntPtr)0, outputByteArray, 0, null, out clevent);
            clevent.Dispose();
            assert(err, "read output buffer");

            Cl.ReleaseMemObject(inputImage2DBuffer);
            Cl.ReleaseMemObject(outputImage2DBuffer);

            GCHandle pinnedOutputArray = GCHandle.Alloc(outputByteArray, GCHandleType.Pinned);
            IntPtr outputBmpPointer = pinnedOutputArray.AddrOfPinnedObject();

            var bmp = new Bitmap(w, h, w * 4, PixelFormat.Format32bppArgb, outputBmpPointer);
            pinnedOutputArray.Free();

            return bmp;
        }

        public FloatMap SpikesFilter(FloatMap inMap, float treshold)
        {
            var k = _kernels["quickSpikesFilterImg"];
            FloatMap outMap = new FloatMap(inMap.W, inMap.H);

            err = Cl.SetKernelArg(k, 2, floatSize, treshold);
            assert(err, "treshold, setKernelArg");

            singlePass(k, inMap, outMap);

            return outMap;
        }
    }
}
