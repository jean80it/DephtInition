using System;
using System.Drawing;

namespace DepthInition
{
    public interface IDIMapComputer : IDisposable
    {
        FloatMap GetMultiResContrastEvaluation(FloatMap imgfIn, int subSamples);
        FloatMap ResizeMap(FloatMap imgfIn, int dstW, int dstH);
        void Accumulate(FloatMap imgfInAccu, FloatMap imgfIn, float k);
        float GetMapMax(FloatMap imgfIn);
        FloatMap GetContrastMap(FloatMap imgfIn);
        FloatMap HalfMap(FloatMap imgfIn, int times);
        FloatMap HalfMap(FloatMap imgfIn);
        //FloatMap DoubleMap(FloatMap imgfIn, int times);
        //FloatMap DoubleMap(FloatMap imgfIn);
        FloatMap QuickBlurMap(FloatMap imgfIn);
        FloatMap Bmp2Map(Bitmap bmp);
        Bitmap Map2Bmp(FloatMap imgf, float k);
        Bitmap Map2BmpFauxColors(FloatMap imgf, float k, int count);
        FloatMap SpikesFilter(FloatMap imgfIn, float treshold);
        FloatMap CapHoles(FloatMap imgfIn, int filterHalfSize);
        FloatMap GaussianBlur(FloatMap imgfIn, float sigma);
    }
}