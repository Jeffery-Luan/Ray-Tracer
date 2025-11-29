using System.IO;
using System;
using StbImageSharp;

namespace RayTracer
{
    /// <summary>
    /// Class to represent an image in memory and allow for I/O operations 
    /// relating to that image.
    /// </summary>
    public class Image
    {
        private byte[] data;
        private int width;
        private int height;
        private int numPixels;

        /// <summary>
        /// Construct a new image with given dimensions.
        /// </summary>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        public Image(int width, int height)
        {
            this.data = new byte[width * height * 4]; // RBGA format
            this.width = width;
            this.height = height;
            this.numPixels = width * height;

            ClearImage(new Color(0, 0, 0));
        }

        /// <summary>
        /// Load an image from a file path.
        /// </summary>
        /// <param name="path">Path to the image file</param>
        /// <returns>An Image object containing the image data</returns>
        public static Image LoadFromFile(string path)
        {
            using (Stream stream = File.OpenRead(path))
            {
                ImageResult imageResult = ImageResult.FromStream(
                    stream,
                    ColorComponents.RedGreenBlueAlpha
                );

                Image img = new Image(imageResult.Width, imageResult.Height);
                img.data = imageResult.Data;
                return img;
            }
        }

        /// <summary>
        /// Get a specific pixel coordinate and return its color.
        /// </summary>
        /// <param name="x">X-coordinate of the pixel</param>
        /// <param name="y">Y-coordinate of the pixel</param>
        /// <param name="color">Color at the specified pixel</param>
        public Color GetPixel(int x, int y)
        {
            return GetPixel(x + y * this.width);
        }

        /// <summary>
        /// Get a specific pixel ID and return its color.
        /// </summary>
        /// <param name="pid">Pixel ID</param>
        /// <param name="color">Color at the specified pixel</param>
        public Color GetPixel(int pid)
        {
            return new Color(
                ByteToColorFrac(this.data[pid * 4]),     // R
                ByteToColorFrac(this.data[pid * 4 + 1]), // G
                ByteToColorFrac(this.data[pid * 4 + 2])  // B
            ); // Skip A (assume always 1)
        }

        /// <summary>
        /// Set a specific pixel coordinate to a given color.
        /// </summary>
        /// <param name="x">X-coordinate of the pixel</param>
        /// <param name="y">Y-coordinate of the pixel</param>
        /// <param name="color">Color to set pixel to</param>
        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x + y * this.width, color);
        }

        /// <summary>
        /// Set a specific pixel ID to a given color.
        /// </summary>
        /// <param name="pid">Pixel ID</param>
        /// <param name="color">Color to set pixel to</param>
        public void SetPixel(int pid, Color color)
        {
            this.data[pid * 4] = ColorFracToByte(color.R);
            this.data[pid * 4 + 1] = ColorFracToByte(color.G);
            this.data[pid * 4 + 2] = ColorFracToByte(color.B);
            this.data[pid * 4 + 3] = ColorFracToByte(1); // Assume alpha always 1
        }

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width { get { return this.width; } }

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height { get { return this.height; } }

        /// <summary>
        /// Write image to disk as a PNG file.
        /// </summary>
        /// <param name="path">Path to save file</param>
        public void WritePNG(string path)
        {
            using (Stream stream = File.OpenWrite(path))
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                writer.WritePng(
                    data,
                    width,
                    height,
                    StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha,
                    stream
                );
            }
        }

        /// <summary>
        /// Clear image by replacing each pixel with a specified color.
        /// </summary>
        /// <param name="color">Replacement color</param>
        private void ClearImage(Color color)
        {
            for (int pid = 0; pid < this.numPixels; pid++)
            {
                SetPixel(pid, color);
            }
        }

        /// <summary>
        /// Helper to convert color component range to byte range.
        /// Caps component range to prevent overflows.
        /// </summary>
        /// <param name="v">Component value (0-1)</param>
        /// <returns>Byte value (0-255)</returns>
        private byte ColorFracToByte(double v)
        {
            return (byte)(Math.Min(v, 1) * 255);
        }

        /// <summary>
        /// Helper to convert byte value to color component range.
        /// </summary>
        /// <param name="v">Byte value (0-255)</param>
        /// <returns>Component value (0-1)</returns>
        private double ByteToColorFrac(byte v)
        {
            return v / 255.0;
        }
    }
}
