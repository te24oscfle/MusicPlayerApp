namespace MusicPlayerApp
{
    [Command(
        "help", 
        "Lists all commands and their usage and aliases", 
        "help <commandName?>",
        "h"
    )]
    public class Help : ICommand
    {
        private void PrintCommandDef(CommandDef commandDef)
        {
            var metadata = commandDef.Metadata;
            
            Console.WriteLine($"\n{metadata.Name}");
            Console.WriteLine($"\t{metadata.Description}");

            if (!string.IsNullOrWhiteSpace(metadata.Usage))
                Console.WriteLine($"\tUsage: {metadata.Usage}");
            
            if (metadata.Aliases != null)
                Console.WriteLine($"\tAliases: {string.Join(", ", metadata.Aliases)}");
        }
        public Task Execute(string[] arguments)
        {
            Dictionary<string, CommandDef> commandDefs = CommandRegistry.DiscoverCommands(false);
            
            if (arguments.Length > 0)
            {
                // User has requested help on a specific command
                string commandName = arguments[0];
                if (!commandDefs.TryGetValue(commandName, out CommandDef? commandDef))
                {
                    Console.WriteLine($"Could not find command named {commandName}");
                    return Task.CompletedTask;
                };
                PrintCommandDef(commandDef);
                return Task.CompletedTask;
            }

            foreach(var pair in commandDefs)
                PrintCommandDef(pair.Value);

            return Task.CompletedTask;
        }
    }
}