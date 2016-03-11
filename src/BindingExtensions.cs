namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
#if WINDOWS_UWP
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Media.Imaging;
#elif ANDROID
    using Android.Views;
    using Android.Widget;
    using Android.Graphics.Drawables;
#elif iOS
    using System.Linq;
    using UIKit;
    using Foundation;
#endif

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
        public static Visibility ToVisible(this bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility ToCollapsed(this bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility ToVisible(this string value)
        {
            return string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility ToCollapsed(this string value)
        {
            return string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;
        }

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
                Debug.WriteLine($"Error convert '{ value }' to BitmapImage");
            }
        }

        /// <summary>
        /// Creates an ReadOnlyObservableCollection wrapper around this collection.
        /// On WinRT, listview requires the collection to derive from ObservableCollection rather then
        /// implement INotifyPropertyChanged.
        /// </summary>
        public static ReadOnlyObservableCollection<T> AsReadOnlyObservableCollection<T>(this IReadOnlyList<T> collection)
        {
            return new ReadOnlyObservableCollection<T>(AsObservableCollection(collection));
        }

        /// <summary>
        /// Creates an ReadOnlyObservableCollection wrapper around this collection.
        /// On WinRT, listview requires the collection to derive from ObservableCollection rather then
        /// implement INotifyPropertyChanged.
        /// </summary>
        public static ObservableCollection<T> AsObservableCollection<T>(this IReadOnlyList<T> collection, bool smoothFastRemoveAdd = false)
        {
            var list = new ObservableCollection<T>(collection);

            var incc = collection as INotifyCollectionChanged;
            if (incc != null)
            {
                var lastRemoveIndex = -1;
                var queue = new List<Action>();

                incc.CollectionChanged += async (sender, e) =>
                {
                    var action = new Action(() =>
                    {
                        if (e.Action == NotifyCollectionChangedAction.Reset)
                        {
                            list.Clear();
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Add)
                        {
                            list.Insert(e.NewStartingIndex, (T)e.NewItems[0]);
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Remove)
                        {
                            list.RemoveAt(e.OldStartingIndex);
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Replace)
                        {
                            list[e.NewStartingIndex] = (T)e.NewItems[0];
                        }
                        else if (e.Action == NotifyCollectionChangedAction.Move)
                        {
                            list.Move(e.OldStartingIndex, e.NewStartingIndex);
                        }
                    });
                    
                    if (!smoothFastRemoveAdd)
                    {
                        action();
                        return;
                    }

                    queue.Add(action);

                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        lastRemoveIndex = e.OldStartingIndex;

                        await Task.Delay(200);
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex == lastRemoveIndex && lastRemoveIndex >= 0)
                    {
                        var capturedIndex = lastRemoveIndex;
                        queue.RemoveAt(queue.Count - 1);
                        queue[queue.Count - 1] = () => list[capturedIndex] = (T)e.NewItems[0];
                        lastRemoveIndex = -1;

                        await Task.Delay(800);
                    }

                    lastRemoveIndex = -1;

                    foreach (var item in queue)
                    {
                        item();
                    }
                    queue.Clear();
                };
            }
            return list;
        }

        private static readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        public static void BindDataContext<T>(this FrameworkElement element, Action<T> update) where T : class
        {
            var safeAction = new Action(() =>
            {
                try
                {
                    var data = element.DataContext as T;
                    if (data != null)
                    {
                        update(data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Binding Error:");
                    Debug.WriteLine(ex);
                }
            });

            var propertyChangedHandler = new PropertyChangedEventHandler((sender, e) =>
            {
                if (syncContext != null)
                {
                    syncContext.Post(x => ((Action)x)(), safeAction);
                }
                else
                {
                    safeAction();
                }
            });

            element.DataContextChanged += (a, b) =>
            {
                var data = element.DataContext as T;
                if (data != null)
                {
                    update(data);

                    var notifyPropertyChanged = data as INotifyPropertyChanged;
                    if (notifyPropertyChanged != null)
                    {
                        notifyPropertyChanged.PropertyChanged -= propertyChangedHandler;
                        notifyPropertyChanged.PropertyChanged += propertyChangedHandler;
                    }
                }
            };
        }
#endif
    }
}