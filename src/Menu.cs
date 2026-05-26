using Spectre.Console;
using SkiaSharp;
namespace Asciify;

public static class Menu
{
    public static void Run()
    {   
        //Ensure the console can display UTF-8 characters for better ASCII art representation.
        Console.OutputEncoding = System.Text.Encoding.UTF8; 
        
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Asciify").Centered().Color(Color.Green));
            Console.WriteLine("Welcome to Asciify!");

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
            if (!File.Exists(filePath))        
            {
                if (filePath.ToLower() == "q" || filePath.ToLower() == "quit")
                {
                    AnsiConsole.MarkupLine("[yellow]Exiting to main menu...[/]");
                    return;
                }
                if (!File.Exists(filePath))
                {
                    AnsiConsole.MarkupLine("[red]File not found. Please try again.[/]");
                    continue;
                }
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
                    Render.Renderize(useroptions, filePath);
                });
            AnsiConsole.MarkupLine("[green]Done! Press any key to return to the main menu...[/]");
            Console.ReadKey(true);
            AnsiConsole.Clear();
            return;
        }
    }
}