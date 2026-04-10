using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppCafebookApi.Services
{
    public static class HinhAnhHelper
    {
        public static BitmapImage LoadImage(string? imageSource, string defaultImagePath)
        {
            if (string.IsNullOrEmpty(imageSource))
            {
                return LoadImageFromPackUri(defaultImagePath);
            }

            if (imageSource.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(imageSource, UriKind.Absolute);
                    image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();

                    return image;
                }
                catch (Exception)
                {
                    return LoadImageFromPackUri(defaultImagePath);
                }
            }

            try
            {
                if (!File.Exists(imageSource))
                {
                    return LoadImageFromPackUri(defaultImagePath);
                }

                var fileImage = new BitmapImage();
                fileImage.BeginInit();
                fileImage.UriSource = new Uri(imageSource, UriKind.Absolute);
                fileImage.CacheOption = BitmapCacheOption.OnLoad;
                fileImage.EndInit();
                fileImage.Freeze();
                return fileImage;
            }
            catch (Exception)
            {
                return LoadImageFromPackUri(defaultImagePath);
            }
        }

        private static BitmapImage LoadImageFromPackUri(string uriPath)
        {
            try
            {
                var image = new BitmapImage();
                var uri = new Uri($"pack://application:,,,/AppCafebookApi;component{uriPath}", UriKind.Absolute);

                image.BeginInit();
                image.UriSource = uri;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception)
            {
                var image = new BitmapImage();
                var writeableBitmap = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);
                using (var stream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                    encoder.Save(stream);
                    stream.Position = 0;
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
        }
    }
}