using System;
using System.Collections.Generic;
using System.IO;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Shaders
{
    /// <summary>
    /// The Gaussian blur is a type of image-blurring filter that uses a normal distribution (also called "Gaussian distribution", 
    /// thus the name "Gaussian blur") for calculating the transformation to apply to each pixel in the image.
    /// It is typically used to reduce image noise and reduce detail levels. The visual effect of this blurring technique is a 
    /// smooth blur resembling that of viewing the image through a translucent screen.
    /// Mathematically speaking, applying a Gaussian blur to an image is the same as convolving the image with a Gaussian or
    /// normal distribution. 
    /// Each pixel's value is set to a weighted average of that pixel's neighborhood. The original pixel's value receives the 
    /// heaviest weight (having the highest Gaussian value), and neighboring pixels receive smaller weights as their distance 
    /// to the original pixel increases. This results in a blur that preserves boundaries and edges better than other, more uniform blurring filters
    /// src Wikipedia: http://en.wikipedia.org/wiki/Gaussian_blur
    /// </summary>
    public class GaussianDistribution : Shader
    {
        private TV_2DVECTOR _texelSize;
        private float _standardDeviation;
        private int _numberofTaps;
        private float[] _weights;

        // TODO: why arent i using the base class to create the shader from string and to assign the defines?
        public GaussianDistribution(string id, string resourcePath, int numberOfTaps)
            : base(id)
        {
            _tvShader = CoreClient._CoreClient.Scene.CreateShader(id);
            string effectString = string.Format("#define TAPS {0}", numberOfTaps) +
                                  File.ReadAllText(resourcePath);
            _tvShader.CreateFromEffectString(effectString);
            if (_tvShader == null)
                throw new Exception("GaussianDistribution:New() Could not create shader instance.");


            _numberofTaps = numberOfTaps;
            // standard deviation must be computed prior to finding weights
            switch (_numberofTaps)
            {
                case 5:
                    _standardDeviation = 1;
                    break;
                case 7:
                    _standardDeviation = 1.745f;
                    break;
                case 9:
                    _standardDeviation = 2.7f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Taps must be 5, 7 or 9");
            }

            // calculate and pass the weights to the shader
            _weights = GetWeightArray(_numberofTaps);
            _tvShader.SetEffectParamFloat("centerTapWeight", _weights[0]);

            if (_numberofTaps == 5)
                // note that for 5, we explicilty provide the weights for both sides of the center
                _tvShader.SetEffectParamVector4("tapWeights",
                                                new TV_4DVECTOR(_weights[2], _weights[1], _weights[1], _weights[2]));

            else if (_numberofTaps == 7)
                // note that for 7 and 9, Zak's shader is written to know how to place these 3 or 4 on both sides of the center weight
                _tvShader.SetEffectParamVector3("tapWeights", new TV_3DVECTOR(_weights[1], _weights[2], _weights[3]));
            else
                _tvShader.SetEffectParamVector4("tapWeights",
                                                new TV_4DVECTOR(_weights[1], _weights[2], _weights[3], _weights[4]));
        }

        public override object Traverse(Traversers.ITraverser target, object data)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        internal override Traversers.ChildSetter GetChildSetter()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public TV_2DVECTOR TexelSize
        {
            get { return _texelSize; }
            set
            {
                _tvShader.SetEffectParamVector2("texelSize", value);
                _texelSize = value;
            }
        }

        public float[] Weights
        {
            get { return _weights; }
        }

        private float[] GetWeightArray(int numberOfTaps)
        {
            numberOfTaps = (numberOfTaps - 1)/2 + 1;
            float[] weights = new float[numberOfTaps];

            double[] tempWeights = new double[numberOfTaps];
            double sum = 0;
            for (int i = 0; i < numberOfTaps; i++)
            {
                tempWeights[i] = Gaussian1D(i);
                sum += tempWeights[i]*2;
            }
            sum -= tempWeights[0];

            double augmentationFactor = 1/sum;

            for (int i = 0; i < numberOfTaps; i++)
                weights[i] = (float) (tempWeights[i]*augmentationFactor);

            return weights;
        }

        private double Gaussian1D(int distance)
        {
            return 1/(Math.Sqrt(2*Math.PI)*_standardDeviation)*
                   Math.Exp(-Math.Pow(distance, 2)/(2*Math.Pow(_standardDeviation, 2)));
        }

        // aavLaptop's main function for his alternative gaussian distribution calculation
        //int main(int argc, char* argv[])
        //{
        //    std::vector<float> curve;
        //    int decimations;

        //    if( argc == 1 )
        //        return 0;

        //    float sigma = atof(argv[1]);

        //    GenGaussian( sigma, curve, decimations );

        //    for( size_t i = curve.size(); i < 13; ++i )
        //        curve.push_back(0.0f);

        //    printf( "image scale = 1/%d\n\n", decimations );
        //    printf( "Nearest filtering:\n" );
        //    for( size_t i = 0; i < curve.size(); ++i )
        //    {
        //        printf( "fetch %d: offset = %f; w = %f;\n", i, (float)i, curve[i] );
        //    }

        //    printf( "\nLinear filtering:\n" );
        //    printf( "fetch %d: offset = %f; w = %f;\n", 0, 0.0f, curve[0] );
        //    for( size_t i = 1; i < curve.size(); i += 2 )
        //    {
        //        float w1 = curve[i+0];
        //        float w2 = curve[i+1];
        //        float w12 = w1 + w2;
        //        float k = w12 > 0.0f ? w2/w12 : 0.0f;

        //        printf( "fetch %d: offset = %f; w = %f; (reconstructed w0 = %f; w1 = %f)\n", 
        //                i/2+1,
        //                i+k,
        //                w12,
        //                w12 * (1.0f-k),
        //                w12 * (k)
        //                );
        //    }

        //    return 0;
        //}
        // an alternative written (presumably) by aav from #gamedev who pasted it to pastebin
        //        <aavLaptop> sigma is related to the filter radius, but
        //<aavLaptop> 13 is the nr of samples my shader uses
        //<aavLaptop> or rather, 7 bilinear ones
        //<aavLaptop> sorry i mean 13 bilinear ones, since this is only the positive side
        //<aavLaptop> so it's 6+1+6 with the 1 being in the middle
        //<aavLaptop> ah, I see
        //<aavLaptop> with nearest filtering it would be 12+1+12
        //<aavLaptop> so, two passes (horz/vert) with 13 samples each gives you a 25x25 kernel gaussian blur
        //<aavLaptop> however, i also scale down the image if the sigma is too high
        public float[] GetWeightArray(float sigma, List<float> curve, int decimations)
        {
            for (decimations = 1; decimations < 4; decimations++)
            {
                curve.Clear();

                float x = 0.0f;
                float g;
                do
                {
                    g = (float) Math.Exp(-x*x/(2.0*sigma*sigma));
                    curve.Add(g);

                    x += 1.0f*decimations;
                } while (g >= 1.0f/255.0f);

                if (curve.Count <= 13)
                    break;
            }

            float sum = 0.0f;
            for (int i = 0; i < curve.Count; ++i)
            {
                sum += curve[i]*(i > 0 ? 2.0f : 1.0f);
            }

            for (int i = 0; i < curve.Count; ++i)
            {
                curve[i] /= sum;
            }

            return curve.ToArray();
        }

        public float StandardDeviation
        {
            get { return _standardDeviation; }
            set { _standardDeviation = value; }
        }
    }
}