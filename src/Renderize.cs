namespace Asciify;
using SkiaSharp;
using Spectre.Console;


public static class Render
{
    public static void Renderize(UserOptions options, string filePath)
{
    //Load the image using SkiaSharp.
    using var bitmap = SKBitmap.Decode(filePath);

    //Redimention the iamge based on the user specified width while maintaining aspect ratio.
    int newHeight =  (int)((float)bitmap.Height / bitmap.Width * options.Width / 2);
    var resized = new SKBitmap(new SKImageInfo(options.Width, newHeight));
    bitmap.ScalePixels(resized, SKSamplingOptions.Default);

    // Defo need upgrading this to use color if the user wants it, but for now just render the ASCII art in grayscale.
    // Map brightness to ASCII characters. The string of characters is ordered from darkest to lightest.
    // The brightness calculation uses the luminosity method, which weights the RGB channels according to human perception.
    // Its very basic and ugly for now but it works, and I can always improve it later (tomorrow lol)
    
    const string chars = " .,:;i1tfLCG08@";
        var sb = new System.Text.StringBuilder();

        for (int y = 0; y < resized.Height; y++)
        {
            for (int x = 0; x < resized.Width; x++)
            {
                var pixel = resized.GetPixel(x, y);
                float brightness = (pixel.Red * 0.299f + pixel.Green * 0.587f + pixel.Blue * 0.114f) / 255f;
                if (options.Invert) brightness = 1f - brightness;
                int index = (int)(brightness * (chars.Length - 1));
                if (options.Color)
                {
                    string ch = chars[index] == '[' ? "[[" : chars[index].ToString();
                    sb.Append($"[rgb({pixel.Red},{pixel.Green},{pixel.Blue})]{ch}[/]");
                }
                else
                    sb.Append(chars[index]);
            }
            sb.AppendLine();
        }
    if (options.Color) AnsiConsole.Markup(sb.ToString());
    else AnsiConsole.Write(sb.ToString());
    }
}