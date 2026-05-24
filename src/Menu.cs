using Spectre.Console;

namespace Asciify;

public static class Menu
{
    public static void Run()
    {
        Console.WriteLine("Welcome to Asciify!");
        Console.WriteLine("Please enter the path to the image you want to convert:");
        string? imagePath = Console.ReadLine();

        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        {
            Console.WriteLine("Invalid file path. Please try again.");
            return;
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