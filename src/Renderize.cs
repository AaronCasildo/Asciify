namespace Asciify;
using SkiaSharp;
using Spectre.Console;


public static class Render
{
    public static void Renderize(UserOptions options, string filePath)
    {
        //Load the image using SkiaSharp.
        using var bitmap = SKBitmap.Decode(filePath);
        const float redpixel = 0.299f, greenpixel = 0.587f, bluepixel = 0.114f;

        //Redimention the iamge based on the user specified width while maintaining aspect ratio.
        int newHeight =  (int)((float)bitmap.Height / bitmap.Width * options.Width / 2);
        var resized = new SKBitmap(new SKImageInfo(options.Width, newHeight));
        bitmap.ScalePixels(resized, SKSamplingOptions.Default);
        
        const string chars = " .,:;i1tfLCG08@";
            var sb = new System.Text.StringBuilder();

            for (int y = 0; y < resized.Height; y++)
            {
                for (int x = 0; x < resized.Width; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    float brightness = (pixel.Red * redpixel + pixel.Green * greenpixel + pixel.Blue * bluepixel) / 255f;
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

        if(AnsiConsole.Confirm("Do you want to download the ASCII art?"))
        {
            if (options.Download) Download.HTMLDownload(sb.ToString());
        }
    }
}