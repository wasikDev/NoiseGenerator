using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace CSLibrary
{
    public class Class1
    {

        public static int AddNoiseToPixel(int pixel, double noise)
        {
            int a = (pixel >> 24) & 0xff;
            int r = ((pixel >> 16) & 0xff) + (int)noise;
            int g = ((pixel >> 8) & 0xff) + (int)noise;
            int b = (pixel & 0xff) + (int)noise;

            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            return (a << 24) | (r << 16) | (g << 8) | b;
        }

    }
}
