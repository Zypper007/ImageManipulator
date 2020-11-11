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
        readonly private int[] Pixels;
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
            Pixels = new int[Stride * Height];
            source.Bits.CopyTo(Pixels, 0);
        }

        static public byte IntToByte(int x)
        {
            if (x >= 0 && x <= 255)
                return (byte)x;
            if (x < 0)
                return 0;
            else return 255;
        }

        public ImagePixels ForEachOnPixel(Func<IntARGB, IntARGB> func)
        {
            var clone = new ImagePixels(this);

            clone.ForEach(func);

            return clone;
        }


        public BitmapSource ToBitmapSource()
        {
            return ToBitmapSource(IntToByte);
        }

        public BitmapSource ToBitmapSource(Func<int, byte> FromIntToByteConventer)
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

        private void ForEach(Func<IntARGB, IntARGB> func)
        {
            for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    SetPixel(x, y, func(GetPixel(x, y)));
        }

        private int CalcualteStep(int x, int y )
        {
            // tutaj powinnien być switch z wyborem formatu ale ja się ograniczyłem tylko do ARGB
            // ile jest bajtów na pixel
            /*
                W każdym wierszu jest Width pixeli a kazdy pixel to BPS bitów więc w każdym wierszu może być y*Width*BPS bajtów;
                Do tego trzeba dodać przesunięcie o x ale każdy pixel to BPS bajtów więc dodać x*BPS
             */
            
            return y * Width * BPS + x * BPS;
        }


        private IntARGB GetPixel(int x, int y)
        {
            var step = CalcualteStep(x,y);
            return new IntARGB(Pixels[step+3],Pixels[step+2],Pixels[step+1],Pixels[step]);
        }

        private void SetPixel(int x, int y, IntARGB color)
        {
            var step = CalcualteStep(x, y);
    
            Pixels[step] = color.B;
            Pixels[step + 1] = color.G;
            Pixels[step + 2] = color.R;
            Pixels[step + 3] = color.A;
        }

      
    }

    struct IntARGB
    {
        public IntARGB(int A, int R, int G, int B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public int A { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}
