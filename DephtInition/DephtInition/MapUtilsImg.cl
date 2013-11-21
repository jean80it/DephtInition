__kernel void getLumaImg(__read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST; // linear filtereing not even supported for imageui

    const int2 coord = (int2)(get_global_id(0), get_global_id(1));

    write_imagef(dstImg, coord, dot((float4)(0.299f, 0.587f, 0.114f, 0.0f),  convert_float4(read_imageui(srcImg, smp, coord))));
}

__kernel void mapToGreyscaleBmp(__read_only image2d_t srcImg,  __write_only image2d_t dstImg, __private float k)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;

    const int2 coord = (int2)(get_global_id(0), get_global_id(1));
    float v = read_imagef(srcImg, smp, coord).x * k; // should i clamp here?
    write_imageui(dstImg, coord, (uint4)(v, v, v, 255));
}

__kernel void mapToFauxColorsBmp(__read_only image2d_t srcImg,  __write_only image2d_t dstImg, __private float k)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;

    const int2 coord = (int2)(get_global_id(0), get_global_id(1));
    float v = read_imagef(srcImg, smp, coord).x * k; // should i clamp here?
    write_imageui(dstImg, coord, v<0?(uint4)(0,0,0,0):(uint4)(v, 0, 255-v, 255));
}

__kernel void quickBlurImgH( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
	const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
	const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 

	float4 v = read_imagef(srcImg, smp, coord); 

	float4 v1 = read_imagef(srcImg, smp, coord+(int2)(1, 0)); 
	float4 v2 = read_imagef(srcImg, smp, coord-(int2)(1, 0)); 
    
	write_imagef(dstImg, coord, (v*0.5f+(v1+v2)*0.25f)); // only float4.x is considered, since image format is Luminosity/float on host code
}

__kernel void quickBlurImgV( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
	const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
	const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 

	float4 v = read_imagef(srcImg, smp, coord); 

	float4 v1 = read_imagef(srcImg, smp, coord+(int2)(0, 1)); 
	float4 v2 = read_imagef(srcImg, smp, coord-(int2)(0, 1)); 
    
	write_imagef(dstImg, coord, (v*0.5f+(v1+v2)*0.25f)); // only float4.x is considered, since image format is Luminosity/float on host code
}

__kernel void upsizeImg( __read_only image2d_t srcImg,  __write_only image2d_t dstImg, __private float xk, __private float yk)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_LINEAR;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const float2 inCoord = (float2)(xk * coord.x, yk * coord.y);
    
    write_imagef(dstImg, coord, read_imagef(srcImg, smp, inCoord)); 
}

__kernel void halfSizeImgH( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const int2 inCoord = (int2)(coord.x * 2, coord.y); 

    float4 v = (read_imagef(srcImg, smp, inCoord) + read_imagef(srcImg, smp, inCoord + (int2)(1,0)))*0.5f;
    
    write_imagef(dstImg, coord, v); 
}

__kernel void halfSizeImgV( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const int2 inCoord = (int2)(coord.x, coord.y * 2); 

    float4 v = (read_imagef(srcImg, smp, inCoord) + read_imagef(srcImg, smp, inCoord + (int2)(0,1)))*0.5f;
    
    write_imagef(dstImg, coord, v); 
}

__kernel void getContrastImg( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
	const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;

    const float k1 = 0.145833f;
    const float k2 = 0.104167f;
    
	const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    
    const float e11 = read_imagef(srcImg, smp, coord+(int2)(-1, -1)).x;
    const float e21 = read_imagef(srcImg, smp, coord+(int2)( 0, -1)).x;
    const float e31 = read_imagef(srcImg, smp, coord+(int2)( 1, -1)).x;

    const float e12 = read_imagef(srcImg, smp, coord+(int2)(-1,  0)).x;
    const float e22 = read_imagef(srcImg, smp, coord).x;
    const float e32 = read_imagef(srcImg, smp, coord+(int2)( 1,  0)).x;

    const float e13 = read_imagef(srcImg, smp, coord+(int2)(-1,  1)).x;
    const float e23 = read_imagef(srcImg, smp, coord+(int2)( 0,  1)).x;
    const float e33 = read_imagef(srcImg, smp, coord+(int2)( 1,  1)).x;

	float v = ((fabs(e21 - e22)+fabs(e12 - e22)+fabs(e32 - e22)+fabs(e23 - e22))*k1 + 
              (fabs(e11 - e22)+fabs(e31 - e22)+fabs(e13 - e22)+fabs(e33 - e22))*k2);

	write_imagef(dstImg, coord, v);
}

__kernel void minReduceImgH( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const int2 inCoord = (int2)(coord.x * 2, coord.y); 

    float v = min(read_imagef(srcImg, smp, inCoord).x, read_imagef(srcImg, smp, inCoord + (int2)(1,0)).x);
    
    write_imagef(dstImg, coord, v); 
}

__kernel void minReduceImgV( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const int2 inCoord = (int2)(coord.x, coord.y * 2); 

    float v = min(read_imagef(srcImg, smp, inCoord).x, read_imagef(srcImg, smp, inCoord + (int2)(0,1)).x);
    
    write_imagef(dstImg, coord, v); 
}

__kernel void maxReduceImgH( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const int2 inCoord = (int2)(coord.x * 2, coord.y); 

    float v = max(read_imagef(srcImg, smp, inCoord).x, read_imagef(srcImg, smp, inCoord + (int2)(1,0)).x);
    
    write_imagef(dstImg, coord, v); 
}

__kernel void maxReduceImgV( __read_only image2d_t srcImg,  __write_only image2d_t dstImg)
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    const int2 inCoord = (int2)(coord.x, coord.y * 2); 

    float v = max(read_imagef(srcImg, smp, inCoord).x, read_imagef(srcImg, smp, inCoord + (int2)(0,1)).x);
    
    write_imagef(dstImg, coord, v); 
}

__kernel void capHolesImg( __read_only image2d_t srcImg,  __write_only image2d_t dstImg, __constant float * mask, __private int maskHalfSize )
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    
    const int maskStride = maskHalfSize * 2 + 1;

    const float value = read_imagef(srcImg, smp, coord).x;

    if (value>=0)
    {
        write_imagef(dstImg, coord, value);
    }
    else
    {
        // Collect neighbor values and multiply with mask
        float accu = 0.0f;
        float wAccu = 0.0f;
        for(int a = -maskHalfSize; a < maskHalfSize+1; a++) {
            for(int b = -maskHalfSize; b < maskHalfSize+1; b++) {
                const float v = read_imagef(srcImg, smp, coord + (int2)(a,b)).x;
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
            write_imagef(dstImg, coord, accu / wAccu);
        }
        else
        {
            write_imagef(dstImg, coord, -1.0f);
        }
    }
}

__kernel void accumulate(__global float *accuImg, __global __read_only float *srcImg, __private int accuStride, __private int srcStride, __private float k)
{
    const int x = get_global_id(0);
    const int y = get_global_id(1);
	const int idx = x + y * accuStride;

    accuImg[idx] = accuImg[idx] + srcImg[x+y*srcStride] * k;
}

__kernel void quickSpikesFilterImg( __read_only image2d_t srcImg,  __write_only image2d_t dstImg, __private float treshold )
{
    // not vectorized... should get rid of some ifs
	const float k1 = 0.145833f;
    const float k2 = 0.104167f;

	const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 

	float neighborhoodWeight = 0;
	float neighborhoodAccu = 0;

	float v = read_imagef(srcImg, smp, coord + (int2)(0,1)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k1;
		neighborhoodWeight += k1;
	}

    v = read_imagef(srcImg, smp, coord + (int2)(0,-1)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k1;
		neighborhoodWeight += k1;
	}

	v = read_imagef(srcImg, smp, coord + (int2)(1,0)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k1;
		neighborhoodWeight += k1;
	}

	v = read_imagef(srcImg, smp, coord + (int2)(-1,0)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k1;
		neighborhoodWeight += k1;
	}

	v = read_imagef(srcImg, smp, coord + (int2)(1,1)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k2;
		neighborhoodWeight += k2;
	}

	v = read_imagef(srcImg, smp, coord + (int2)(-1,-1)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k2;
		neighborhoodWeight += k2;
	}

	v = read_imagef(srcImg, smp, coord + (int2)(1,-1)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k2;
		neighborhoodWeight += k2;
	}

	v = read_imagef(srcImg, smp, coord + (int2)(-1,1)).x;
	if (v > 0)
	{
		neighborhoodAccu += v * k2;
		neighborhoodWeight += k2;
	}

	float d;
    const float sv = read_imagef(srcImg, smp, coord).x;
       
	if (neighborhoodWeight>0)
	{
        d = fabs(sv - (neighborhoodAccu / neighborhoodWeight));
        write_imagef(dstImg, coord, ((d > treshold) ? -1 : sv)); // pixel value is just invalidated. A further step will take care of interpolation for missing value
	}
	else
	{
		write_imagef(dstImg, coord, -1); // let's filter isolated values, too
	}
} 

// TODO: create separated version
__kernel void convolveImg( __read_only image2d_t srcImg,  __write_only image2d_t dstImg, __constant float * mask, __private int maskHalfSize )
{
    const sampler_t smp = CLK_NORMALIZED_COORDS_FALSE | CLK_ADDRESS_CLAMP_TO_EDGE | CLK_FILTER_NEAREST;
    
    const int2 coord = (int2)(get_global_id(0), get_global_id(1)); 
    
    const int maskStride = maskHalfSize * 2 + 1;

    const float value = read_imagef(srcImg, smp, coord).x;
   
    // Collect neighbor values and multiply with mask
    float accu = 0.0f;
    float wAccu = 0.0f;
    for(int a = -maskHalfSize; a < maskHalfSize+1; a++) {
        for(int b = -maskHalfSize; b < maskHalfSize+1; b++) {
            const float v = read_imagef(srcImg, smp, coord + (int2)(a,b)).x;
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
        write_imagef(dstImg, coord, accu / wAccu);
    }
    else
    {
        write_imagef(dstImg, coord, -1);
    }
}