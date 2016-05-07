#if WINDOWS_UWP
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Windows.UI.Xaml

#elif ANDROID
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;

namespace Android.Views

#elif iOS
using System.Linq;
using Foundation;

namespace UIKit
#endif
{
    static class BindingExtensions
    {
#if ANDROID
        public static ViewStates ToVisible(this bool value)
        {
            return value ? ViewStates.Visible : ViewStates.Gone;
        }

        public static ViewStates ToCollapsed(this bool value)
        {
            return value ? ViewStates.Gone : ViewStates.Visible;
        }

        public static ViewStates ToVisible(this string value)
        {
            return string.IsNullOrEmpty(value) ? ViewStates.Gone : ViewStates.Visible;
        }

        public static ViewStates ToCollapsed(this string value)
        {
            return string.IsNullOrEmpty(value) ? ViewStates.Visible : ViewStates.Gone;
        }

        public static void SetSource(this ImageView image, string value, bool small = false)
        {
            var existingValue = image.GetDataContext<string>();
            if (existingValue == value)
            {
                return;
            }

            image.SetDataContext(value);

            image.SetImageBitmap(null);
            var bitmapManager = small ? BitmapManager.Small : BitmapManager.Large;
            bitmapManager.Release(existingValue);
            image.SetImageBitmap(bitmapManager.Get(value));
        }
#elif WINDOWS_UWP
        public static Visibility ToVisible(this bool value) => value ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility ToCollapsed(this bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility ToVisible(this string value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility ToCollapsed(this string value) => string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;

        public static SolidColorBrush ToBrush(this uint color)
        {
            SolidColorBrush result;
            if (!_brushes.TryGetValue(color, out result))
                _brushes[color] = result = new SolidColorBrush(Color.FromArgb(
                    (byte)(color >> 24 & 0xFF),
                    (byte)(color >> 16 & 0xFF),
                    (byte)(color >> 8 & 0xFF),
                    (byte)(color >> 0 & 0xFF)));
            return result;
        }

        private static Dictionary<uint, SolidColorBrush> _brushes = new Dictionary<uint, SolidColorBrush>();

        public static void SetSource(this Image image, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                image.Source = null;
                return;
            }

            try
            {
                var uri = new Uri(value);
                var bitmap = image.Source as BitmapImage;
                if (bitmap != null && bitmap.UriSource == uri) return;
                image.Source = new BitmapImage(new Uri(value));
            }
            catch
            {
                image.Source = null;
            }
        }
#endif
    }
}