#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <assert.h>

#include "wavefront.h"
#include "hull.h"


void main(int argc,char **argv)
{
	if ( argc < 2 )
	{
		printf("Usage: StanHull <fname>  (options)\r\n");
		printf("\r\n");
		printf("<fname> must be a valid Wavefront .OBJ file\r\n");
		printf("\r\n");
		printf("-t  output as triangles instead of polgons.\r\n");
		printf("-s(n) skin width\r\n");
		printf("-v(n) maximum number of vertices\r\n");
		printf("-n(n) normal epsilon\r\n");
	}
	else
	{
		char *fname = argv[1];

    HullDesc desc;

    for (int i=2; i<argc; i++)
    {
    	char *option = argv[i];

    	if ( stricmp(option,"-t") == 0 )
    	{
    		desc.SetHullFlag(QF_TRIANGLES);
    		printf("Saving as triangles.\r\n");
    	}
    	else if ( strnicmp(option,"-s",2) == 0 )
    	{
    		float swidth = (float) atof( &option[2] );
    		desc.mSkinWidth = swidth;
    		desc.SetHullFlag(QF_SKIN_WIDTH);
    		printf("Skin Width: %0.4f\r\n", swidth );
    	}
    	else if ( strnicmp(option,"-v",2) == 0 )
    	{
    		int maxv = atoi( &option[2] );
    		desc.mMaxVertices = maxv;
    		printf("Maximum Vertices: %d\r\n", maxv );
    	}
    	else if ( strnicmp(option,"-n",2) == 0 )
    	{
    		desc.mNormalEpsilon = (float) atof( &option[2] );
    		printf("Normal epsilon: %0.4f\r\n", desc.mNormalEpsilon );
    	}

    }

		WavefrontObj w;

    int t = w.loadObj( fname );
    if ( t )
    {

    	desc.mVcount       = w.mVertexCount;
    	desc.mVertices     = w.mVertices;
    	desc.mVertexStride = sizeof(float)*3;

      printf("Read Wavefront OBJ %s with %d vertices and %d triangles.\r\n", fname, w.mVertexCount, w.mTriCount );
      printf("Now generating a convex hull.\r\n");

      HullResult result;
      HullLibrary hl;

      HullError ret = hl.CreateConvexHull(desc,result);

      if ( ret == QE_OK )
      {

      	printf("Successfully created convex hull.\r\n");

        if ( result.mPolygons )
        	printf("Hull contains %d poygons.\r\n", result.mNumFaces );
        else
        	printf("Hull contains %d triangles.\r\n", result.mNumFaces );

        printf("Output hull contains %d vertices.\r\n ", result.mNumOutputVertices );

				FILE *fph = fopen("hull.obj", "wb");
				if ( fph )
				{
  				printf("Saving hull as 'hull.obj'\r\n");

					for (unsigned int i=0; i<result.mNumOutputVertices; i++)
					{
						const float *v = &result.mOutputVertices[i*3];
						fprintf(fph,"v %0.9f %0.9f %0.9f\r\n", v[0], v[1], v[2] );
					}

  				if ( result.mPolygons )
	  			{
	  				const unsigned int *idx = result.mIndices;
				  	for (unsigned int i=0; i<result.mNumFaces; i++)
				  	{
				  		unsigned int pcount = *idx++;
				  		fprintf(fph,"f ");
				  		for (unsigned int j=0; j<pcount; j++)
				  		{
				  			fprintf(fph,"%4d ", idx[0]+1 );
				  			idx++;
				  		}
				  		fprintf(fph,"\r\n");
				  	}
		  		}
			  	else
				  {
				  	for (unsigned int i=0; i<result.mNumFaces; i++)
				  	{
				  		unsigned int *idx = &result.mIndices[i*3];
				  		fprintf(fph,"f %4d %4d %4d\r\n", idx[0]+1, idx[1]+1, idx[2]+1 );
				  	}

  				}
  				fclose(fph);
  			}

      	hl.ReleaseResult(result);
      }
      else
      {
      	printf("Failed to create convex hull.\r\n");
      }

    }
    else
    {
    	printf("Failed to load '%s'\r\n", fname );
    }

	}
}
