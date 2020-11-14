namespace ImageManipulator
{
    class ImageStatistic
    {
        public BitmapHistogram Histogram { get; }

        public ImageStatistic(ImagePixels img)
        {
            Histogram = new BitmapHistogram();

            img.ForEachOnPixel(DoHistogram);
        }

        private void DoHistogram(ShortARGB color)
        {
            
            Histogram.A[_histIdx(color.A)]++;
            Histogram.R[_histIdx(color.R)]++;
            Histogram.G[_histIdx(color.G)]++;
            Histogram.B[_histIdx(color.B)]++;
        }

        private int _histIdx(short v)
        {
            if (v < 0)
                return 0;
            if (v > 255)
                return 255;
            return v;
        }
    }
}
