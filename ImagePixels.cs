﻿using System;
using System.Linq;
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
        readonly public PixelFormat Format;
        readonly public BitmapPalette Palette;

        // Konstruktor z BitmapySource
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


        // Konstruktor Kopiujący
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


        // Metoda do zmiany short na byte z ucinaniem wartosci poza zakresem 
        static public byte ShortToByte(short x)
        {
            if (x < 0)
                return 0;
            if (x > 255)
                return 255;
            return (byte)x;

        }

        // ForEach tylko get
        public void ForEachOnPixel(Action<ShortARGB> func)
        {
            ForEach(func);
        }

        // ForEach set, zwraca klon poprzedniego obiektu
        public ImagePixels ForEachOnPixel(Func<ShortARGB, ShortARGB> func)
        {
            var clone = new ImagePixels(this);

            clone.ForEachAndSet(func);

            return clone;
        }

        // Konwersja do BitmapSource
        public BitmapSource ToBitmapSource()
        {
            return ToBitmapSource(ShortToByte);
        }


        // Konwersja do bitmap source z własną funkcją do konwersj short na byte
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
        private void ForEach(Action<ShortARGB> func)
        {
            for (long y = 0; y < Height; y++)
                for (long x = 0; x < Width; x++)
                    func(GetPixel(x, y));
        }

        private void ForEachAndSet(Func<ShortARGB, ShortARGB> func)
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
            if(Pixels != null)
                return new ShortARGB(Pixels[step+3], Pixels[step+2], Pixels[step+1], Pixels[step]);
            return new ShortARGB(Bits[step+3], Bits[step+2], Bits[step+1], Bits[step]);
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
