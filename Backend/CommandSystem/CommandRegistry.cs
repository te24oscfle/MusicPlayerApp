using System.Reflection;

namespace CommandSystem
{
    public static class CommandRegistry
    {
        public static Dictionary<string, CommandDef> DiscoverCommands(bool getAliases)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Discover all commands in project and create command definitions.
            Dictionary<string, CommandDef> commandDefs = assembly
                .GetTypes() // Get all types, types being Classes, Methods, and so on.
                .Where(type => // Filter for non abstract classes
                    typeof(ICommand).IsAssignableFrom(type) &&
                    type is { IsClass: true, IsAbstract: false})
                .Select(type => new 
                {
                    // If the Class has the CommandAttribute, Attribute will be equal to CommandAttribute.
                    // Otherwise, Attribute will be equal to null.
                    Type = type,
                    Attribute = type.GetCustomAttribute<CommandAttribute>()
                })
                .Where(x => x.Attribute != null) // Filter out Classes withouth a CommandAttribute
                .ToDictionary( // Map to Dictionary
                    x => x.Attribute!.Name,
                    x => new CommandDef(
                        (ICommand)Activator.CreateInstance(x.Type)!,
                        x.Attribute!));
            
            // Caller did not request aliases, we can send the commandDefs early. 
            if (!getAliases)
                return commandDefs;

            // Add aliases
            Dictionary<string, CommandDef> allCommands = new Dictionary<string, CommandDef>();

            foreach (var pair in commandDefs)
            {
                CommandDef commandDef = pair.Value;
                
                allCommands.Add(commandDef.Metadata.Name, commandDef);
                
                if (commandDef.Metadata.Aliases == null)
                    continue;
                
                foreach (string alias in commandDef.Metadata.Aliases)
                {
                    allCommands.Add(alias, commandDef);    
                }
            }

            return allCommands;
        }
    }
}