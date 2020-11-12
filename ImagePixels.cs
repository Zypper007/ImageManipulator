using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageManipulator
{
    class ImagePixels
    {
        readonly private int Width, Height, Stride, BPS;
        readonly private double DpiY, DpiX;
        readonly private short[] Pixels;
        private byte[] Bits;
        readonly private PixelFormat Format;
        readonly private BitmapPalette Palette;


        public ImagePixels(BitmapSource imgSrc)
        {
            Width = imgSrc.PixelWidth;
            Height = imgSrc.PixelHeight;
            DpiY = imgSrc.DpiY;
            DpiX = imgSrc.DpiX;
            Format = imgSrc.Format;
            Palette = imgSrc.Palette;
            Stride = (Width * Format.BitsPerPixel + 7) / 8;
            BPS = Format.BitsPerPixel / 8;
            Bits = new byte[Stride * Height];
            imgSrc.CopyPixels(Bits, Stride, 0);
        }

        public ImagePixels(ImagePixels source)
        {
            Width = source.Width;
            Height = source.Height;
            DpiX = source.DpiX;
            DpiY = source.DpiY;
            BPS = source.BPS;
            Stride = source.Stride;
            Format = source.Format;
            Palette = source.Palette;
            Pixels = new short[Stride * Height];
            source.Bits.CopyTo(Pixels, 0);
        }

        static public byte ShortToByte(short x)
        {
            if (x >= 0 && x <= 255)
                return (byte)x;
            if (x < 0)
                return 0;
            else return 255;
        }

        public ImagePixels ForEachOnPixel(Func<ShortARGB, ShortARGB> func)
        {
            var clone = new ImagePixels(this);

            clone.ForEach(func);

            return clone;
        }


        public BitmapSource ToBitmapSource()
        {
            return ToBitmapSource(ShortToByte);
        }

        public BitmapSource ToBitmapSource(Func<short, byte> FromIntToByteConventer)
        {
            if (Bits == null)
            {
                var linq = from p in Pixels select FromIntToByteConventer(p);
                Bits = linq.ToArray();
            }

            var img = BitmapSource.Create(Width, Height, DpiX, DpiY, Format, Palette, Bits, Stride);
            img.Freeze();

            return img;
        }

        private void ForEach(Func<ShortARGB, ShortARGB> func)
        {
            for (long y = 0; y < Height; y++)
                for (long x = 0; x < Width; x++)
                    SetPixel(x, y, func(GetPixel(x, y)));
        }

        private long CalcualteStep(long x, long y )
        {
            // tutaj powinnien być switch z wyborem formatu ale ja się ograniczyłem tylko do ARGB
            // ile jest bajtów na pixel
            /*
                W każdym wierszu jest Width pixeli a kazdy pixel to BPS bitów więc w każdym wierszu może być y*Width*BPS bajtów;
                Do tego trzeba dodać przesunięcie o x ale każdy pixel to BPS bajtów więc dodać x*BPS
             */
            
            return y * Width * BPS + x * BPS;
        }


        private ShortARGB GetPixel(long x, long y)
        {
            var step = CalcualteStep(x,y);
            return new ShortARGB(Pixels[step+3],Pixels[step+2],Pixels[step+1],Pixels[step]);
        }

        private void SetPixel(long x, long y, ShortARGB color)
        {
            var step = CalcualteStep(x, y);
    
            Pixels[step] = color.B;
            Pixels[step + 1] = color.G;
            Pixels[step + 2] = color.R;
            Pixels[step + 3] = color.A;
        }

      
    }

    struct ShortARGB
    {
        public ShortARGB(short A, short R, short G, short B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public ShortARGB(int A, int R, int G, int B)
        {
            this.A = (short)A;
            this.R = (short)R;
            this.G = (short)G;
            this.B = (short)B;
        }
        public short A { get; set; }
        public short R { get; set; }
        public short G { get; set; }
        public short B { get; set; }
    }
}
