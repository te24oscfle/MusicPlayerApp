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
        static async Task Main(string[] args)
        {
            Dictionary<string, CommandDef> commands = CommandRegistry.DiscoverCommands(true);

            DatabaseManager.InitilizeDatabase();

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

                await commandDef.command.Execute(arguments);
            }
        }
    }
}