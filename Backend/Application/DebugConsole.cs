using CommandSystem;

namespace Application;

public class DebugConsole
{
    private Dictionary<string, CommandDef> Commands { get; }
    
    public DebugConsole()
    {
        Commands = CommandRegistry.DiscoverCommands(true);
    }

    public async Task Start()
    {
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
            
            if(!Commands.TryGetValue(commandName, out CommandDef? commandDef))
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