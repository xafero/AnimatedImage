using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif.Formats;

namespace WpfAnimatedGif
{
    static class AnimationCache
    {
        private struct CacheKey
        {
            private readonly ImageSource _source;

            public CacheKey(ImageSource source)
            {
                _source = source;
            }

            private bool Equals(CacheKey other)
            {
                return ImageEquals(_source, other._source);
            }

            public override bool Equals(object? obj)
            {
                if (obj is CacheKey key)
                {
                    return Equals(key);
                }
                else return false;
            }

            public override int GetHashCode()
            {
                return ImageGetHashCode(_source);
            }

            private static int ImageGetHashCode(ImageSource image)
            {
                if (image != null)
                {
                    var uri = GetUri(image);
                    if (uri != null)
                        return uri.GetHashCode();
                }
                return 0;
            }

            private static bool ImageEquals(ImageSource? x, ImageSource? y)
            {
                if (x is null && y is null)
                    return true;

                if (x is null || y is null)
                    return false;

                if (Equals(x, y))
                    return true;

                if (x.GetType() != y.GetType())
                    return false;

                var xUri = GetUri(x);
                var yUri = GetUri(y);
                return xUri != null && xUri == yUri;
            }

            private static Uri? GetUri(ImageSource image)
            {
                if (image is BitmapImage bmp && bmp.UriSource is not null)
                {
                    if (bmp.UriSource.IsAbsoluteUri)
                        return bmp.UriSource;
                    if (bmp.BaseUri is not null)
                        return new Uri(bmp.BaseUri, bmp.UriSource);
                }

                if (image is BitmapFrame frame)
                {
                    string s = frame.ToString();
                    if (s != frame.GetType().FullName
                     && Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out var fUri))
                    {
                        if (fUri.IsAbsoluteUri)
                            return fUri;
                        if (frame.BaseUri != null)
                            return new Uri(frame.BaseUri, fUri);
                    }
                }
                return null;
            }
        }

        private static readonly Dictionary<CacheKey, AnimationCacheEntry> s_animationCache = new Dictionary<CacheKey, AnimationCacheEntry>();
        private static readonly Dictionary<CacheKey, HashSet<Image>> s_imageControls = new Dictionary<CacheKey, HashSet<Image>>();

        public static void AddControlForSource(ImageSource source, Image imageControl)
        {
            var cacheKey = new CacheKey(source);
            if (!s_imageControls.TryGetValue(cacheKey, out var controls))
            {
                s_imageControls[cacheKey] = controls = new HashSet<Image>();
            }

            controls.Add(imageControl);
        }

        public static void RemoveControlForSource(ImageSource source, Image imageControl)
        {
            var cacheKey = new CacheKey(source);
            if (s_imageControls.TryGetValue(cacheKey, out var controls))
            {
                if (controls.Remove(imageControl))
                {
                    if (controls.Count == 0)
                    {
                        s_animationCache.Remove(cacheKey);
                        s_imageControls.Remove(cacheKey);
                    }
                }
            }
        }

        public static void Add(ImageSource source, AnimationCacheEntry entry)
        {
            var key = new CacheKey(source);
            s_animationCache[key] = entry;
        }

        public static void Remove(ImageSource source)
        {
            var key = new CacheKey(source);
            s_animationCache.Remove(key);
        }

        public static AnimationCacheEntry? Get(ImageSource source)
        {
            var key = new CacheKey(source);
            s_animationCache.TryGetValue(key, out var entry);
            return entry;
        }
    }

    internal class AnimationCacheEntry
    {
        public AnimationCacheEntry(FrameRenderer renderer)
        {
            Renderer = renderer;
        }

        public FrameRenderer Renderer { get; }
    }
}