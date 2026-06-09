namespace Asciify;
using Spectre.Console;
using System.Text;
using SkiaSharp;

public static class Download
{
    public static void HTMLDownload(string asciiArt)
    {
        var fileName = AnsiConsole.Ask<string>("Enter a file name for the HTML download:");
        if (string.IsNullOrWhiteSpace(fileName)) fileName = "ascii-art.html";
        if (!fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            fileName += ".html";

        var outputPath = Path.Combine(Environment.CurrentDirectory, fileName);
        var htmlBody = asciiArt.Contains("[rgb(", StringComparison.OrdinalIgnoreCase)
            ? ConvertMarkupToHtml(asciiArt)
            : HtmlEncode(asciiArt);

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
                   "  <pre>" + htmlBody + "</pre>\n" +
                   "</body>\n" +
                   "</html>\n";

        File.WriteAllText(outputPath, html, Encoding.UTF8);
        AnsiConsole.MarkupLine($"[green]Saved HTML to:[/] {outputPath}");
    }

    private static string ConvertMarkupToHtml(string input)
    {
        var sb = new StringBuilder();
        var spanOpen = false;

        for (int i = 0; i < input.Length; i++)
        {
            if (IsAt(input, i, "[["))
            {
                sb.Append('[');
                i++;
                continue;
            }

            if (IsAt(input, i, "[rgb("))
            {
                var end = input.IndexOf(")]", i, StringComparison.OrdinalIgnoreCase);
                if (end < 0)
                    end = input.IndexOf(']', i);

                if (end > i)
                {
                    var rgb = input.Substring(i + 5, end - (i + 5));
                    if (spanOpen) sb.Append("</span>");
                    sb.Append("<span style=\"color: rgb(" + HtmlEncode(rgb) + ")\">" );
                    spanOpen = true;
                    i = end + 1;
                    continue;
                }
            }

            if (IsAt(input, i, "[/]"))
            {
                if (spanOpen)
                {
                    sb.Append("</span>");
                    spanOpen = false;
                }
                i += 2;
                continue;
            }

            sb.Append(HtmlEncode(input[i].ToString()));
        }

        if (spanOpen) sb.Append("</span>");
        return sb.ToString();
    }

    private static bool IsAt(string input, int index, string value)
    {
        if (index + value.Length > input.Length) return false;
        return string.Compare(input, index, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0;
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
        var fileName = AnsiConsole.Ask<string>("Enter a file name for the PNG download:");
        if (string.IsNullOrWhiteSpace(fileName)) fileName = "ascii-art.png";
        if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            fileName += ".png";

        var outputPath = Path.Combine(Environment.CurrentDirectory, fileName);
        var plainText = StripMarkup(asciiArt).Replace("\r\n", "\n");
        var lines = plainText.Split('\n');
        var maxColumns = lines.Length == 0 ? 0 : lines.Max(line => line.Length);

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
        var height = Math.Max(1, (int)Math.Ceiling((lines.Length * lineStep) + (padding * 2)));

        using var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        for (int y = 0; y < lines.Length; y++)
        {
            var line = lines[y];
            var baseline = padding - metrics.Ascent + (y * lineStep);

            for (int x = 0; x < line.Length; x++)
            {
                var character = line[x].ToString();
                var drawX = padding + (x * cellWidth);
                    canvas.DrawText(character, drawX, baseline, SKTextAlign.Left, font, paint);
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var output = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        data.SaveTo(output);

        AnsiConsole.MarkupLine($"[green]Saved PNG to:[/] {outputPath}");
    }

    private static string StripMarkup(string input)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            if (MatchesAt(input, i, "[["))
            {
                sb.Append('[');
                i++;
                continue;
            }

            if (MatchesAt(input, i, "[rgb("))
            {
                var end = input.IndexOf("]", i, StringComparison.OrdinalIgnoreCase);
                if (end >= 0)
                {
                    i = end;
                    continue;
                }
            }

            if (MatchesAt(input, i, "[/]"))
            {
                i += 2;
                continue;
            }

            sb.Append(input[i]);
        }

        return sb.ToString();
    }

    private static bool MatchesAt(string input, int index, string value)
    {
        if (index + value.Length > input.Length) return false;
        return string.Compare(input, index, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0;
    }
}