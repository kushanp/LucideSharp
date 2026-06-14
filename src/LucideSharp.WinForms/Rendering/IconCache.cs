using System.Collections.Concurrent;

namespace LucideSharp.WinForms.Rendering;

internal static class IconCache
{
    private static readonly ConcurrentDictionary<IconRenderOptions, Bitmap> Cache = new();
    private const int MaxCacheEntries = 2048;

    public static Bitmap GetOrAdd(IconRenderOptions options, Func<IconRenderOptions, Bitmap> factory)
    {
        if (Cache.TryGetValue(options, out var cached))
        {
            return cached;
        }

        var bitmap = factory(options);
        Cache[options] = bitmap;
        TrimIfNeeded();
        return bitmap;
    }

    public static void Clear() => Cache.Clear();

    private static void TrimIfNeeded()
    {
        if (Cache.Count <= MaxCacheEntries)
        {
            return;
        }

        foreach (var key in Cache.Keys.Take(Cache.Count - MaxCacheEntries))
        {
            if (Cache.TryRemove(key, out var bitmap))
            {
                bitmap.Dispose();
            }
        }
    }
}