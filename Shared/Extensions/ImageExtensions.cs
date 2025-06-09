using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace DMXCore.DMXCore100.Extensions;

/// <summary>Generic helper functions related to graphics.</summary>
public static class ImageExtensions
{
    /// <summary>
    /// Resizes an image to a new width and height value.
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="newWidth">The width of the new image.</param>
    /// <param name="newHeight">The height of the new image.</param>
    /// <param name="mode">Interpolation mode.</param>
    /// <param name="maintainAspectRatio">If true, the image is centered in the middle of the returned image, maintaining the aspect ratio of the original image.</param>
    /// <returns>The new image. The old image is unaffected.</returns>
    public static Image<Rgba32> ResizeImage(Image<Rgba32> image, int newWidth, int newHeight, IResampler mode, bool maintainAspectRatio = true)
    {
        if (maintainAspectRatio)
        {
            // Adjust newWidth or newHeight if either is zero
            if (newWidth == 0 && newHeight > 0)
            {
                newWidth = (int)((double)newHeight * image.Width / image.Height);
            }
            else if (newHeight == 0 && newWidth > 0)
            {
                newHeight = (int)((double)newWidth * image.Height / image.Width);
            }
            else if (newWidth == 0 && newHeight == 0)
            {
                throw new ArgumentException("Both newWidth and newHeight cannot be zero.");
            }

            double ratioW = (double)newWidth / (double)image.Width;
            double ratioH = (double)newHeight / (double)image.Height;
            double ratio = ratioW < ratioH ? ratioW : ratioH;
            int insideWidth = (int)(image.Width * ratio);
            int insideHeight = (int)(image.Height * ratio);

            // Resize existing image
            image.Mutate(ctx => ctx.Resize(insideWidth, insideHeight, mode));

            var outputImage = new Image<Rgba32>(newWidth, newHeight);

            outputImage.Mutate(ctx => ctx.DrawImage(image, new SixLabors.ImageSharp.Point((newWidth / 2) - (insideWidth / 2), (newHeight / 2) - (insideHeight / 2)), 1));

            return outputImage;
        }
        else
        {
            image.Mutate(ctx => ctx.Resize(newWidth, newHeight, mode));

            return image;
        }

    }
}
