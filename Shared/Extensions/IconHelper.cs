using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace DMXCore.DMXCore100.Extensions;

public static class IconHelper
{
    public static string? GetIconName(ILogger log, string? iconName, string imageFolder, int width, int height, string? internalFilename = null)
    {
        if ((string.IsNullOrEmpty(iconName) && string.IsNullOrEmpty(internalFilename)) || (width == 0 && height == 0))
            return null;

        // Custom icon
        // Check if it exists in the assets folder
        string baseFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        string builtInIconFolder = Path.Combine(baseFolder!, "Assets", "Icons");

        string? finalName;
        if (CheckIconName(builtInIconFolder, false, iconName, width, height, out finalName, out _))
        {
            log.LogTrace("Icon: {IconName}   Built-in Final: {FinalIconName}", iconName, finalName);

            return finalName;
        }

        // Check our cache
        string cacheImageFolder = Path.Combine(Path.GetTempPath(), "img_cache");
        Directory.CreateDirectory(cacheImageFolder);

        if (CheckIconName(cacheImageFolder, true, iconName, width, height, out finalName, out _))
        {
            log.LogTrace("Icon: {IconName}   Cache Final: {FinalIconName}", iconName, Path.GetFileName(finalName));

            return $"file://{finalName}";
        }

        bool found = CheckIconName(imageFolder, false, iconName, width, height, out finalName, out bool hasWidthHeight);
        if (!found)
        {
            imageFolder = Path.Combine(baseFolder!, "Assets", "Images");
            found = CheckIconName(imageFolder, false, internalFilename ?? iconName, width, height, out finalName, out hasWidthHeight);

            if (found && internalFilename != null)
            {
                // Check cache as well
                if (CheckIconName(cacheImageFolder, true, internalFilename, width, height, out string cacheFinalName, out _))
                {
                    log.LogTrace("Icon: {IconName}   Cache Final: {FinalIconName}", iconName, Path.GetFileName(cacheFinalName));

                    return $"file://{cacheFinalName}";
                }
            }
        }

        if (found)
        {
            log.LogTrace("Icon {IconName} found in image folder {ImageFolder}, final name {FinalName}", iconName, imageFolder, finalName);

            if (hasWidthHeight)
            {
                string destinationFilename = Path.Combine(cacheImageFolder, finalName);
                File.Copy(Path.Combine(imageFolder, finalName), destinationFilename, true);

                log.LogTrace("Icon: {IconName}   AddedCache Final: {FinalIconName}", iconName, finalName);

                return $"file://{destinationFilename}";
            }

            try
            {
                // Load image
                using var image = Image.Load<Rgba32>(Path.Combine(imageFolder, finalName));

                string destinationFilename = Path.Combine(cacheImageFolder, $"{Path.GetFileNameWithoutExtension(finalName)}_{width}x{height}.png");

                if (image.Width == width && image.Height == height)
                {
                    // No need to scale, just copy to cache
                    File.Copy(Path.Combine(imageFolder, finalName), destinationFilename, true);

                    log.LogTrace("Icon: {IconName}   AddedCacheNoScale Final: {FinalIconName}", iconName, finalName);
                }
                else
                {
                    var scaledImage = ImageExtensions.ResizeImage(image, width, height, mode: KnownResamplers.Lanczos3, maintainAspectRatio: true);
                    // Save to cache
                    scaledImage.SaveAsPng(destinationFilename);

                    log.LogTrace("Icon: {IconName}   AddedCacheScale Final: {FinalIconName}, saved to {DestName}", iconName, finalName, Path.GetFileName(destinationFilename));
                }

                return $"file://{destinationFilename}";
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to scale image: {Message}", ex.Message);
                // Ignore
            }
        }

        log.LogTrace("Icon: {IconName} not found", iconName);

        return null;
    }

    public static void ClearImageCache(string? filename)
    {
        try
        {
            string cacheImageFolder = Path.Combine(Path.GetTempPath(), "img_cache");
            Directory.CreateDirectory(cacheImageFolder);

            if (string.IsNullOrEmpty(filename))
            {
                // Clear the entire cache
                Directory.GetFiles(cacheImageFolder, "*.*", SearchOption.AllDirectories).ToList().ForEach(File.Delete);
            }
            else
            {
                filename = Path.GetFileName(filename);
                if (File.Exists(Path.Combine(cacheImageFolder, filename)))
                    File.Delete(Path.Combine(cacheImageFolder, filename));

                string searchPattern = $"{Path.GetFileNameWithoutExtension(filename)}*.*";
                foreach (string file in Directory.GetFiles(cacheImageFolder, searchPattern))
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Ignore
        }
    }

    private static bool CheckIconName(string folder, bool addPath, string iconName, int width, int height, out string? finalName, out bool hasWidthHeight)
    {
        hasWidthHeight = false;

        if (string.IsNullOrEmpty(iconName))
        {
            finalName = null;
            return false;
        }

        finalName = Path.Combine(folder, iconName);
        if (File.Exists(finalName))
        {
            if (!addPath)
                finalName = iconName;
            return true;
        }

        // Check with ext added
        string ext = Path.GetExtension(iconName);
        if (string.IsNullOrEmpty(ext))
            ext = ".png";

        if (CheckIconNameExt(folder, addPath, iconName, ext, width, height, out finalName, out hasWidthHeight))
            return true;

        ext = ".jpg";
        if (CheckIconNameExt(folder, addPath, iconName, ext, width, height, out finalName, out hasWidthHeight))
            return true;

        ext = ".jpeg";
        if (CheckIconNameExt(folder, addPath, iconName, ext, width, height, out finalName, out hasWidthHeight))
            return true;

        finalName = null;
        return false;
    }

    private static bool CheckIconNameExt(string folder, bool addPath, string iconName, string ext, int width, int height, out string? finalName, out bool hasWidthHeight)
    {
        hasWidthHeight = false;

        // Check with WxH added
        string testName = $"{Path.GetFileNameWithoutExtension(iconName)}_{width}x{height}{ext}";
        finalName = Path.Combine(folder, testName);
        if (File.Exists(finalName))
        {
            hasWidthHeight = true;
            if (!addPath)
                finalName = testName;
            return true;
        }

        testName = $"{Path.GetFileNameWithoutExtension(iconName)}{width}x{height}{ext}";
        finalName = Path.Combine(folder, testName);
        if (File.Exists(finalName))
        {
            hasWidthHeight = true;
            if (!addPath)
                finalName = testName;
            return true;
        }

        testName = $"{Path.GetFileNameWithoutExtension(iconName)}{ext}";
        finalName = Path.Combine(folder, testName);
        if (File.Exists(finalName))
        {
            if (!addPath)
                finalName = testName;
            return true;
        }

        finalName = null;
        return false;
    }

    public static bool FileExistsFromUri(Uri uri)
    {
        switch (uri.Scheme)
        {
            case "ms-appx":
                // Handle ms-appx:// URIs
                string iconFile = Path.Combine(Path.GetDirectoryName(System.AppContext.BaseDirectory)!, uri.AbsolutePath[1..]);
                return File.Exists(iconFile);

            case "file":
                // Handle file:// URIs
                string filePath = uri.LocalPath; // Convert URI to local file path
                return File.Exists(filePath);

            default:
                throw new NotSupportedException($"Unsupported URI scheme: {uri.Scheme}");
        }
    }

    public static Stream GetFileStreamFromUri(string uriString)
    {
        var uri = new Uri(uriString);

        switch (uri.Scheme)
        {
            case "ms-appx":
                // Handle ms-appx:// URIs
                string storageFile = Path.Combine(Path.GetDirectoryName(System.AppContext.BaseDirectory)!, uri.AbsolutePath[1..]);
                return new FileStream(storageFile, FileMode.Open, FileAccess.Read);

            case "file":
                // Handle file:// URIs
                var filePath = uri.LocalPath; // Convert URI to local file path
                return new FileStream(filePath, FileMode.Open, FileAccess.Read);

            default:
                throw new NotSupportedException($"Unsupported URI scheme: {uri.Scheme}");
        }
    }

    public static string? GetThemeBasedIcon(string? input, bool isDarkMode)
    {
        if (input == null)
            return null;

        if (!Uri.TryCreate(input, UriKind.Absolute, out Uri? iconUri))
            iconUri = new Uri($"ms-appx:///Assets/Icons/{input}");

        string themeSuffix = isDarkMode ? "_dark" : "_light";

        // See if a theme version is available
        string pathWithoutFilename = string.Join("", iconUri.Segments.Take(iconUri.Segments.Length - 1));
        string filename = iconUri.Segments.Last();
        string themeFilename = $"{Path.GetFileNameWithoutExtension(filename)}{themeSuffix}{Path.GetExtension(filename)}";
        Uri themeIconUri = new Uri($"{iconUri.Scheme}://{pathWithoutFilename}{themeFilename}");

        if (FileExistsFromUri(themeIconUri))
            return themeIconUri.ToString();

        return iconUri.ToString();
    }

    public static void SaveImageAsIco(Image image, string filePath)
    {
        using var ms = new MemoryStream();

        image.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder()); // Save as PNG (ICO uses PNG compression)
        File.WriteAllBytes(filePath, ms.ToArray());
    }

    public static Stream ConvertToIco(string fileUri, int width, int height)
    {
        using (Image image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height))
        {
            image.Mutate(x => x.BackgroundColor(SixLabors.ImageSharp.Color.Blue));
            //SaveImageAsIco(image, outputPath);
            throw new NotImplementedException();
        }

    }
}
