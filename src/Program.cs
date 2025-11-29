using System;
using System.IO;
using CommandLine;
using ImageMagick;

namespace RayTracer
{
    /// <summary>
    /// Main program. Modify this file **AT YOUR OWN RISK**. Doing so may break how
    /// our automated testing system checks your solution, since the command line
    /// arguments need to exactly match the specification. Note we have already
    /// parsed these for you here, and they are passed to the Scene class. If you feel
    /// the need to modify this file, you are probably doing something wrong.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Command line arguments configuration
        /// </summary>
        public class OptionsConf
        {
            [Option('f', "file", Required = true, HelpText = "Input file path (txt).")]
            public string InputFilePath { get; set; }

            [Option('o', "output", Default = null, HelpText = "Output file path (PNG/GIF).")]
            public string OutputFilePath { get; set; }

            [Option('w', "width", Default = (int)400, HelpText = "Output image width in pixels.")]
            public int OutputImageWidth { get; set; }

            [Option('h', "height", Default = (int)400, HelpText = "Output image height in pixels.")]
            public int OutputImageHeight { get; set; }

            [Option('x', "aa-mult", Default = (int)1, HelpText = "Anti-aliasing sampling multiplier.")]
            public int AAMultiplier { get; set; }

            [Option('l', "ambient", Default = (bool)false, HelpText = "Enable ambient lighting.")]
            public bool AmbientLightingEnabled { get; set; }

            [Option('r', "aperture-radius", Default = (double)0, HelpText = "Aperture radius of the camera.")]
            public double ApertureRadius { get; set; }

            [Option('t', "focal-length", Default = (double)1, HelpText = "Focal length of the camera.")]
            public double FocalLength { get; set; }

            [Option('m', "frames", Default = (int)1, HelpText = "Number of frames to render.")]
            public int FrameCount { get; set; }

            [Option('s', "frames-per-second", Default = (int)30, HelpText = "Frames per second for the animation.")]
            public int FramesPerSecond { get; set; }

            [Option('q', "quality", Default = 0, HelpText = "Quality of the render.")]
            public int Quality { get; set; }
        }

        /// <summary>
        /// Main program entry point for the ray tracer.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args)
        {
            // Assume failure as a default (override later if successful).
            Environment.ExitCode = 1;

            Parser.Default
                .ParseArguments<OptionsConf>(args)
                .WithParsed(options =>
                {
                    try
                    {
                        // Read/parse the scene specification file
                        var sceneReader = new SceneReader(options.InputFilePath);

                        if (string.IsNullOrEmpty(options.OutputFilePath))
                        {
                            options.OutputFilePath =
                                Path.GetFileNameWithoutExtension(options.InputFilePath) + ".png";
                        }

                        // Construct the scene
                        // - Pass options based on command line arguments
                        // - Populate the scene based on the parsed file
                        var scene = new Scene(new SceneOptions(
                            options.AAMultiplier,
                            options.AmbientLightingEnabled,
                            options.ApertureRadius,
                            options.FocalLength,
                            options.Quality,
                            options.FrameCount,
                            options.FramesPerSecond
                        ));
                        sceneReader.PopulateScene(scene);

                        CleanOutputFilePath(options.OutputFilePath);

                        if (options.FrameCount <= 1)
                        {
                            Console.WriteLine("Rendering single static image...");
                            RenderImage(
                                scene,
                                0,
                                options.OutputImageWidth,
                                options.OutputImageHeight,
                                options.OutputFilePath
                            );
                        }
                        else
                        {
                            string outputName = Path.GetFileNameWithoutExtension(options.OutputFilePath);
                            for (int i = 0; i < options.FrameCount; i++)
                            {
                                double time = (double)i / options.FramesPerSecond;
                                Console.WriteLine($"Rendering frame {i + 1} of {options.FrameCount} (time={time:F2})...");
                                RenderImage(
                                    scene,
                                    time,
                                    options.OutputImageWidth,
                                    options.OutputImageHeight,
                                    options.OutputFilePath.Replace(".png", $"_{i + 1}.png")
                                );
                            }
                            GenerateGif(
                                options.FrameCount,
                                options.FramesPerSecond,
                                options.OutputFilePath.Replace(".png", ".gif")
                            );
                        }

                        // Got to this point? Success!
                        Environment.ExitCode = 0;
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("Input file not found.");
                    }
                    catch (SceneReader.ParseException e)
                    {
                        Console.WriteLine(
                            $"Input file invalid on line {e.Line}: {e.Message}");
                    }
                    catch (ArgumentException e)
                    {
                        if (e.ParamName == "cmdLineParam")
                            Console.WriteLine(e.Message);
                        else
                            // Still allow other argument exceptions through
                            throw;
                    }
                });
        }

        /// <summary>
        /// Render the scene to an output image.
        /// </summary>
        /// <param name="scene">Scene to render</param>
        /// <param name="time">Time in seconds for animations</param>
        /// <param name="width">Width of the output image</param>
        /// <param name="height">Height of the output image</param>
        /// <param name="outputPath">File path to save the output image</param>
        private static void RenderImage(
            Scene scene,
            double time,
            int width,
            int height,
            string outputPath
        )
        {
            // Create an image with the specified dimensions
            var outputImage = new Image(width, height);

            // Render the scene into the image
            scene.Render(outputImage, time);

            // Write the output image to the specified file path
            outputImage.WritePNG(outputPath);

        }

        /// <summary>
        /// Clean up any existing output files in the specified directory.
        /// This is useful to avoid cluttering the output directory with old files.
        /// </summary>
        /// <param name="outputPath">The output file path to clean</param>
        private static void CleanOutputFilePath(string outputPath)
        {
            string outputDir = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputDir))
            {
                outputDir = Directory.GetCurrentDirectory();
            }
            string outputFileName = Path.GetFileNameWithoutExtension(outputPath);
            string outputFilePattern = $"{outputFileName}*.png";

            if (Directory.Exists(outputDir))
            {
                foreach (var file in Directory.GetFiles(outputDir, outputFilePattern))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete file {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Output directory {outputDir} does not exist.");
            }
        }

        /// <summary>
        /// Generate a GIF from the rendered frames.
        /// This will create a GIF file from the individual frame images.
        /// </summary>
        /// <param name="frameCount">Number of frames in the animation</param>
        /// <param name="framesPerSecond">Frames per second for the GIF</param>
        /// <param name="outputPath">Output file path for the GIF</param>
        private static void GenerateGif(int frameCount, int framesPerSecond, string outputPath)
        {
            int delay = 100 / framesPerSecond; 

            using (var collection = new MagickImageCollection())
            {
                for (int i = 0; i < frameCount; i++)
                {
                    string frameFileName = outputPath.Replace(".gif", $"_{i + 1}.png");
                    if (File.Exists(frameFileName))
                    {
                        var frame = new MagickImage(frameFileName);
                        frame.AnimationDelay = (uint)delay;
                        frame.Format = MagickFormat.Gif;
                        collection.Add(frame);
                    }
                    else
                    {
                        Console.WriteLine($"Error: Frame file {frameFileName} does not exist.");
                    }
                }

                collection.Optimize();
                collection.Write(outputPath);
            }
        }
    }
}
