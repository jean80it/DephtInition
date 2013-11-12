__kernel void getY_alt(__read_only image2d_t srcImg, __global __write_only float *dstImg, __private int outStride)
{
  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_LINEAR;

  const int2 coord = (int2)(get_global_id(0), get_global_id(1));

  uint4 bgra = read_imageui(srcImg, smp, coord); // byte order is BGRA

  float4 bgrafloat = convert_float4(bgra) / 255.0f; //Convert to normalized [0..1] float

  //Convert RGB to luminance (make the image grayscale).
  float luminance =  sqrt(0.241f * bgrafloat.z * bgrafloat.z + 0.691f * 
                      bgrafloat.y * bgrafloat.y + 0.068f * bgrafloat.x * bgrafloat.x);

  dstImg[coord.x+coord.y*outStride] = luminance;
}

	__kernel void getY(__read_only image2d_t srcImg, __global __write_only float *dstImg, __private int outStride)
	{
	  const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;

	  const int2 coord = (int2)(get_global_id(0), get_global_id(1));

	  uint4 bgra = read_imageui(srcImg, smp, coord); // byte order is BGRA

	  float4 bgrafloat = convert_float4(bgra);

	  dstImg[coord.x+coord.y*outStride] = 0.299f * bgrafloat.z + 0.587f * bgrafloat.y + 0.114f * bgrafloat.x;
	}

// this is separable
__kernel void halfSizeMap(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride)
{
    // x,y of the output (i.e. the halved one)
    const int outX = get_global_id(0);
    const int outY = get_global_id(1);
    
    const int inX = outX * 2;
    const int inY = outY * 2;

	// this would be free with a sampler
    // four sampling points for input    
    // A B 
    // C D
    const int inIdxA = inX+inY*inStride;
    const int inIdxB = inIdxA + 1;
    const int inIdxC = inIdxA + inStride;
    const int inIdxD = inIdxC + 1;

	//dstImg[outX+outY*outStride] = srcImg[inIdxA]; // nearest neighbour
    dstImg[outX+outY*outStride] = (srcImg[inIdxA] + srcImg[inIdxB] + srcImg[inIdxC] + srcImg[inIdxD]) * 0.25f; // simple average

}

// this is separable
__kernel void doubleSizeMap(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride)
{
    // x,y of the output (i.e. the doubled one)
    const int outX = get_global_id(0);
    const int outY = get_global_id(1);
    
	// this would be free with a sampler
    const float inX = (float)outX / 2.0f;
    const float inY = (float)outY / 2.0f;
    const int intInX = (int)inX;
    const int intInY = (int)inY;
    const float fracInX = inX - intInX;
    const float fracInY = inY - intInY;

    // four sampling points for input    
    // A B 
    // C D
    const int inIdxA = intInX+intInY*inStride;
    const int inIdxB = inIdxA + 1;
    const int inIdxC = inIdxA + inStride;
    const int inIdxD = inIdxC + 1;

	//dstImg[outX+outY*outStride] = srcImg[inIdxA]; // nearest neighbour
    dstImg[outX+outY*outStride] = mix(mix(srcImg[inIdxA], srcImg[inIdxB], fracInX), mix(srcImg[inIdxC], srcImg[inIdxD], fracInX), fracInY); // simple lerp
}

// this is separable
__kernel void upsizel(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride, __private float xk, __private float yk)
{
	// x,y of the output (i.e. the upsized one)
    const int outX = get_global_id(0);
    const int outY = get_global_id(1);
    
	// this would be free with a sampler
    const float inX = (float)outX * xk;
    const float inY = (float)outY * yk;
    const int intInX = (int)inX;
    const int intInY = (int)inY;
    const float fracInX = inX - intInX;
    const float fracInY = inY - intInY;

    // four sampling points for input    
    // A B 
    // C D
    const int inIdxA = intInX+intInY*inStride;
    const int inIdxB = inIdxA + 1;
    const int inIdxC = inIdxA + inStride;
    const int inIdxD = inIdxC + 1;

	//dstImg[outX+outY*outStride] = srcImg[inIdxA]; // nearest neighbour
    dstImg[outX+outY*outStride] = mix(mix(srcImg[inIdxA], srcImg[inIdxB], fracInX), mix(srcImg[inIdxC], srcImg[inIdxD], fracInX), fracInY); // simple lerp
}

// this is separable
__kernel void getBorder(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride)
{
    const float k1 = 0.145833f;
    const float k2 = 0.104167f;
    
    const int x = get_global_id(0);
    const int y = get_global_id(1);

    const int xyIdx0 = x-1+(y-1)*inStride;
    const int xyIdx1 = xyIdx0 + inStride;
    const int xyIdx2 = xyIdx1 + inStride;
    
    //if ((x==0)||(x==width-1)||(y==0)||(y==height-1)) return;

    // elementXY is the element of a 3x3 matrix centered around current sample
    // TODO: learn about matrix operations
    const float e11 = srcImg[xyIdx0];
    const float e21 = srcImg[xyIdx0 + 1];
    const float e31 = srcImg[xyIdx0 + 2];

    const float e12 = srcImg[xyIdx1];
    const float e22 = srcImg[xyIdx1 + 1]; // this is center
    const float e32 = srcImg[xyIdx1 + 2];

    const float e13 = srcImg[xyIdx2];
    const float e23 = srcImg[xyIdx2 + 1];
    const float e33 = srcImg[xyIdx2 + 2];

    dstImg[x+y*outStride] = ((fabs(e21 - e22)+fabs(e12 - e22)+fabs(e32 - e22)+fabs(e23 - e22))*k1 + 
                             (fabs(e11 - e22)+fabs(e31 - e22)+fabs(e13 - e22)+fabs(e33 - e22))*k2);
}

// this is separable
__kernel void quickBlur(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride)
{
    const float k1 = 0.1715728f; // w = 2
    const float k2 = 0.0857864f; // w = 1
    const float k3 = 0.0606601f; // w = 1/1.4 = 0.7
    
    const int x = get_global_id(0);
    const int y = get_global_id(1);

    const int xyIdx0 = x-1+(y-1)*inStride;
    const int xyIdx1 = xyIdx0 + inStride;
    const int xyIdx2 = xyIdx1 + inStride;
    
    // elementXY is the element of a 3x3 matrix centered around current sample
    // TODO: learn about matrix operations
    const float e11 = srcImg[xyIdx0];
    const float e21 = srcImg[xyIdx0 + 1];
    const float e31 = srcImg[xyIdx0 + 2];

    const float e12 = srcImg[xyIdx1];
    const float e22 = srcImg[xyIdx1 + 1]; // this is center
    const float e32 = srcImg[xyIdx1 + 2];

    const float e13 = srcImg[xyIdx2];
    const float e23 = srcImg[xyIdx2 + 1];
    const float e33 = srcImg[xyIdx2 + 2];

    dstImg[x+y*outStride] = (    e22 * k1 +
                                (e21 + e12 + e32 + e23) * k2 + 
                                (e11 + e31 + e13 + e33) * k3
                            );
}

__kernel void capHoles(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride, __constant float * mask, __private int maskHalfSize )
{
    const int x = get_global_id(0);
    const int y = get_global_id(1);
    const int maskStride = maskHalfSize + 1;

    const float value = srcImg[x + y * inStride];

    if (value>=0)
    {
        dstImg[x + y * outStride] = value;
    }
    else
    {
        // Collect neighbor values and multiply with mask
        float accu = 0.0f;
        float wAccu = 0.0f;
        for(int a = -maskHalfSize; a < maskHalfSize+1; a++) {
            for(int b = -maskHalfSize; b < maskHalfSize+1; b++) {
                const float v = srcImg[a + x + (b + y) * inStride];
                if (v>=0)
                {
                    const float w = mask[a+maskHalfSize+(b+maskHalfSize)*maskStride];
                    wAccu += w;
                    accu += w * v; // TODO: use 'mad' or something like that, maybe?
                }
            }
        }

        if (wAccu != 0)
        {
            dstImg[x + y * outStride] = accu / wAccu;
        }
        else
        {
            dstImg[x + y * outStride] = -1;
        }

        
    }
}

__kernel void convolve(__global __read_only float *srcImg, __global __write_only float *dstImg, __private int inStride, __private int outStride, __constant float * mask, __private int maskHalfSize )
{
    const int x = get_global_id(0);
    const int y = get_global_id(1);
    const int maskStride = maskHalfSize*2 + 1;

    const float value = srcImg[x + y * inStride];

   
    // Collect neighbor values and multiply with mask
    float accu = 0.0f;
    float wAccu = 0.0f;
    for(int a = -maskHalfSize; a < maskHalfSize+1; a++) {
        for(int b = -maskHalfSize; b < maskHalfSize+1; b++) {
            const float v = srcImg[a + x + (b + y) * inStride];
            if (v>=0)
            {
                const float w = mask[a+maskHalfSize+(b+maskHalfSize)*maskStride];
                wAccu += w;
                accu += w * v; // TODO: use 'mad' or something like that, maybe?
            }
        }
    }

    if (wAccu != 0)
    {
        dstImg[x + y * outStride] = accu / wAccu;
    }
    else
    {
        dstImg[x + y * outStride] = -1;
    }
}

__kernel void accumulate(__global float *accuImg, __global __read_only float *srcImg, __private int accuStride, __private int srcStride, __private float k)
{
    const int x = get_global_id(0);
    const int y = get_global_id(1);
	const int idx = x + y * accuStride;

    accuImg[idx] = accuImg[idx] + srcImg[x+y*srcStride] * k;
}