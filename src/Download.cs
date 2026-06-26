namespace Asciify;
using Spectre.Console;
using System.Text;
using SkiaSharp;

public static class Download
{
    public static void HTMLDownload(List<List<Glyph>> glyphs)
    {
        var fileName = AnsiConsole.Ask<string>("Enter a file name for the HTML download (q to cancel):");
        
        if (fileName.ToLower() == "q")
        {
            AnsiConsole.MarkupLine("[yellow]HTML download cancelled.[/]");
            return;
        }

        if (string.IsNullOrWhiteSpace(fileName)) fileName = "ascii-art.html";
        if (!fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            fileName += ".html";
        
        var outputPath = Path.Combine(Environment.CurrentDirectory, fileName);
        var sb = new StringBuilder();
        foreach (var line in glyphs)
        {
            foreach (var glyph in line)
            {
                var ch = HtmlEncode(glyph.Character.ToString());
                if (glyph.Color != SKColor.Empty)
                {
                    sb.Append($"<span style=\"color: rgb({glyph.Color.Red},{glyph.Color.Green},{glyph.Color.Blue})\">{ch}</span>");
                }
                else
                {
                    sb.Append(ch);
                }
            }
            sb.Append("\n");
        }

        var html = $"<!doctype html>\n" +
                   "<html lang=\"en\">\n" +
                   "<head>\n" +
                   "  <meta charset=\"utf-8\">\n" +
                   "  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n" +
                   "  <title>Asciify Output</title>\n" +
                   "  <style>\n" +
                   "    body { margin: 24px; background: #0f1115; color: #e6e6e6; }\n" +
                   "    pre { font: 14px/1.15 'Consolas', 'Courier New', monospace; white-space: pre; }\n" +
                   "  </style>\n" +
                   "</head>\n" +
                   "<body>\n" +
                   "  <pre>" + sb + "</pre>\n" +
                   "</body>\n" +
                   "</html>\n";

        File.WriteAllText(outputPath, html, Encoding.UTF8);
        AnsiConsole.MarkupLine($"[green]Saved HTML to:[/] {outputPath}");
    }

    private static string HtmlEncode(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    public static void PNGDownload(string asciiArt)
    {
        var fileName = AnsiConsole.Ask<string>("Enter a file name for the PNG download (q to cancel):");

        if (fileName.ToLower() == "q")
        {
            AnsiConsole.MarkupLine("[yellow]PNG download cancelled.[/]");
            return;
        }

        if (string.IsNullOrWhiteSpace(fileName)) fileName = "ascii-art.png";
        if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            fileName += ".png";

        // For now, we'll just do a white background. I'll add more background colors in this session.
        var backgroundColor = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a background color for the PNG:")
                .AddChoices(new[] { "White", "Black", "Gray"}));

        var outputPath = Path.Combine(Environment.CurrentDirectory, fileName);
        var lines = ParsePngLines(asciiArt);
        var maxColumns = lines.Count == 0 ? 0 : lines.Max(line => line.Count);

        const float textSize = 24f;
        const float padding = 24f;

        using var typeface = SKTypeface.FromFamilyName("Consolas") ?? SKTypeface.Default;
        using var paint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Black,
            IsStroke = false
        };

        using var font = new SKFont(typeface, textSize)
        {
            Edging = SKFontEdging.Antialias,
            Subpixel = true,
            Hinting = SKFontHinting.Full
        };

        font.GetFontMetrics(out var metrics);
        var cellWidth = Math.Max(font.MeasureText("M"), 1f);
        var cellHeight = Math.Max(metrics.Descent - metrics.Ascent, 1f);
        var lineStep = cellHeight;

        var width = Math.Max(1, (int)Math.Ceiling((maxColumns * cellWidth) + (padding * 2)));
        var height = Math.Max(1, (int)Math.Ceiling((lines.Count * lineStep) + (padding * 2)));

        using var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(backgroundColor switch
        {
            "White" => SKColors.White,
            "Black" => SKColors.Black,
            "Gray" => SKColors.Gray,
            _ => SKColors.White
        });

        for (int y = 0; y < lines.Count; y++)
        {
            var line = lines[y];
            var baseline = padding - metrics.Ascent + (y * lineStep);

            for (int x = 0; x < line.Count; x++)
            {
                var glyph = line[x];
                paint.Color = glyph.Color;
                var drawX = padding + (x * cellWidth);
                canvas.DrawText(glyph.Character.ToString(), drawX, baseline, SKTextAlign.Left, font, paint);
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var output = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(output);

        AnsiConsole.MarkupLine($"[green]Saved PNG to:[/] {outputPath}");
    }
}