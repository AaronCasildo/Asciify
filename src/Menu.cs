using Spectre.Console;
using SkiaSharp;
namespace Asciify;

public static class Menu
{
    public static void Run()
    {   
        //Ensure the console can display UTF-8 characters for better ASCII art representation.
        Console.OutputEncoding = System.Text.Encoding.UTF8; 

        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Asciify").Centered().Color(Color.Green));
        Console.WriteLine("Welcome to Asciify!");
        
        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices(new[] { "Convert Image to ASCII", "Exit" }));

            if (choice == "Convert Image to ASCII") Configure();

            else if (choice == "Exit")
            {
                Console.Clear();
                
                if (Random.Shared.Next(0, 1000) == 0) //Easter egg: 1 in 1000 chance to show a dark souls style message on exit.
                    AnsiConsole.Write(new FigletText("YOU DIED").Centered().Color(Color.Red));
                else
                {
                    AnsiConsole.Write(new FigletText("Goodbye!").Centered().Color(Color.Yellow));    
                }
            } break;
        }
    }


    public static void Configure()
    {
        var filePath = AnsiConsole.Ask<string>("Enter the image path or drag and drop the image here:");
        if (!File.Exists(filePath))        
        {
            AnsiConsole.MarkupLine("[red]File not found. Please try again.[/]");
            return;
        }

        var useroptions = new UserOptions();
        //Get user options for ASCII conversion.
        useroptions.With = AnsiConsole.Ask<int>("Enter the desired width of the ASCII art (default is 100):", defaultValue: 100);
        useroptions.Invert = AnsiConsole.Confirm("Invert brightness (dark areas become light and vice versa)?", defaultValue: false);
        useroptions.Color = AnsiConsole.Confirm("Colorize the ASCII art?", defaultValue: false);

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .Start("Processing image...", ctx =>
            {
                Renderize(useroptions, filePath);
            });
    }

    public static void Renderize(UserOptions options, string filePath)
    {
        //Load the image using SkiaSharp.
        using var bitmap = SKBitmap.Decode(filePath);

        //Redimention the iamge based on the user specified width while maintaining aspect ratio.
        int newHeight =  (int)((float)bitmap.Height / bitmap.Width * options.With / 2);
        var resized = new SKBitmap(new SKImageInfo(options.With, newHeight));
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
                    sb.Append(chars[index]);
                }
                sb.AppendLine();
            }
        AnsiConsole.Write(sb.ToString());
    }
}