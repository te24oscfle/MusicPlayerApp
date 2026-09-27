// Template code:
// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();

using CommandSystem;
using Database;

namespace MusicPlayerApp
{
    public static class Program
    {
        static void Initilize()
        {
            DatabaseManager.InitilizeDatabase();
            Library.LoadLibraryFromDatabase();
        }
        
        static async Task Main(string[] args)
        {
            Dictionary<string, CommandDef> commands = CommandRegistry.DiscoverCommands(true);

            Initilize();

            bool shouldExit = false;
            while(!shouldExit)
            {
                Console.Write(">> ");

                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                
                // doSomething someArgument someOtherArgument
                // =>   commandName = "doSomething";
                //      arguments = ["someArgument", "someOtherArgument"];
                string[] split = input.Split(" ");
                string commandName = split[0];
                string[] arguments = split.Skip(1).ToArray();

                if (commandName == "exit")
                {
                    shouldExit = true;
                    continue;
                }
                
                if(!commands.TryGetValue(commandName, out CommandDef? commandDef))
                {
                    Console.WriteLine("Invalid command.");
                    continue;
                }

                try
                {
                    await commandDef.command.Execute(arguments);
                } 
                catch (Exception e)
                {
                    Console.WriteLine($"Error when executing {commandDef.Metadata.Name}: {e}");
                }
            }
        }
    }
}