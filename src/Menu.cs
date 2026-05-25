using Spectre.Console;

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

            if (choice == "Convert Image to ASCII") Convert();

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
        try
        {
            // Tmr we continue with the image processing and ASCII conversion logic here.
            Console.WriteLine("The image path is valid. {0}", imagePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}