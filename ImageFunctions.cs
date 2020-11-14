using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManipulator
{
    class ImageFunctions
    {
        static public ShortARGB Contrast(ShortARGB color, short value)
        {
            if (color.R + color.G + color.B > 382.5)
                return new ShortARGB(color.A, color.R + value, color.G + value, color.B + value);
            return new ShortARGB(color.A, color.R - value, color.G - value, color.B - value);
        }

        static public ShortARGB Brightness(ShortARGB color, short value)
            => new ShortARGB(color.A, color.R + value, color.G + value, color.B + value);

        static public ShortARGB GrayScale2(ShortARGB colorInput)
        {
            var R = (int)Math.Round(colorInput.R * 0.299);
            var G = (int)Math.Round(colorInput.G * 0.587);
            var B = (int)Math.Round(colorInput.G * 0.114);
            var gray = R + G + B;

            return new ShortARGB(colorInput.A, gray, gray, gray);
        }

        static public ShortARGB GrayScale1(ShortARGB colorInput)
        {
            var gray = (int)Math.Round((colorInput.R + colorInput.G + colorInput.B) / 3f);

            return new ShortARGB(colorInput.A, gray, gray, gray);
        }

        static public ShortARGB InvertColor(ShortARGB colorInput)
        {

            return new ShortARGB(colorInput.A,
                255 - ImagePixels.ShortToByte(colorInput.R),
                255 - ImagePixels.ShortToByte(colorInput.G),
                255 - ImagePixels.ShortToByte(colorInput.B));
        }
    }
}
