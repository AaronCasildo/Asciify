using Spectre.Console;
using SkiaSharp;
namespace Asciify;

public static class Menu
{
    private static string SanitizeFilePath(string input)
    {
        //Cleanses the file path input by removing common artifacts from drag-and-drop in PowerShell and Command Prompt.
        if (input.StartsWith("& "))
            input = input.Substring(2);
               
        input = input.Trim('"', '\'');
        input = input.Trim();
        
        return input;
    }

    public static void Run()
    {   
        //Ensure the console can display UTF-8 characters for better ASCII art representation.
        Console.OutputEncoding = System.Text.Encoding.UTF8; 
        
        while (true)
        {
            AnsiConsole.Clear();
            var banner = new Panel(Align.Center(new FigletText("Asciify").Color(Color.Green)))
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green),
                Padding = new Padding(2, 1)
            };

            AnsiConsole.Write(banner);
            AnsiConsole.Write(Align.Center(new Markup("[grey]Image → ASCII converter[/]")));
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to do?")
                    .AddChoices(new[] { "Convert Image to ASCII", "Exit" }));

            if (choice == "Convert Image to ASCII") Configure();

            else if (choice == "Exit")
            {
                AnsiConsole.Clear();

                if (Random.Shared.Next(0, 1000) == 0) //Easter egg: 1 in 1000 chance to show a dark souls style message on exit.
                {
                    var deathBanner = new Panel(Align.Center(new FigletText("YOU DIED").Color(Color.Red)))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(Color.Red),
                        Padding = new Padding(2, 1)
                    };

                    AnsiConsole.Write(deathBanner);
                    AnsiConsole.Write(Align.Center(new Markup("[grey]Asciify has fallen[/]")));
                }
                else
                {
                    var goodbyeBanner = new Panel(Align.Center(new FigletText("Goodbye!").Color(Color.Yellow)))
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(Color.Yellow),
                        Padding = new Padding(2, 1)
                    };

                    AnsiConsole.Write(goodbyeBanner);
                    AnsiConsole.Write(Align.Center(new Markup("[grey]Thanks for using Asciify[/]")));
                }

                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[green]Press any key to exit...[/]");
                Console.ReadKey(true);
                break;
            }
        }
    }


    public static void Configure()
    {
        AnsiConsole.Clear();
        while (true)
        {
            var filePath = AnsiConsole.Ask<string>("Enter the image path or drag and drop the image here (q to quit):");
            filePath = SanitizeFilePath(filePath);
            
            if (!File.Exists(filePath))        
            {
                if (filePath.ToLower() == "q" || filePath.ToLower() == "quit")
                {
                    AnsiConsole.MarkupLine("[yellow]Exiting to main menu...[/]");
                    return;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]File not found. Please try again.[/]");
                    continue;
                }
            }

            var useroptions = new UserOptions();
            //Get user options for ASCII conversion.
            useroptions.Width = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter the desired width of the ASCII art (default is 100):")
                    .DefaultValue(100)
                    .Validate(w=> w > 0
                        ? ValidationResult.Success() 
                        : ValidationResult.Error("[red]Width must be a positive integer.[/]")));
            useroptions.Invert = AnsiConsole.Confirm("Invert brightness (dark areas become light and vice versa)?", defaultValue: false);
            useroptions.Color = AnsiConsole.Confirm("Colorize the ASCII art?", defaultValue: true);

            Render.Renderize(useroptions, filePath);
            
            AnsiConsole.MarkupLine("[green]Done! Press any key to return to the main menu...[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
            return;
        }
    }
}