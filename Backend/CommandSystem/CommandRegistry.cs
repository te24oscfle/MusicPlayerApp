using System.Reflection;

namespace MusicPlayerApp
{
    public static class CommandRegistry
    {
        public static Dictionary<string, CommandDef> DiscoverCommands(bool getAliases)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Discover all commands in project and create command definitions.
            var commandDefs = assembly
                .GetTypes()
                .Where(type =>
                    typeof(ICommand).IsAssignableFrom(type) &&
                    type is { IsClass: true, IsAbstract: false})
                .Select(type => new
                {
                    Type = type,
                    Attribute = type.GetCustomAttribute<CommandAttribute>()
                })
                .Where(x => x.Attribute != null)
                .ToDictionary(
                    x => x.Attribute!.Name,
                    x => new CommandDef(
                        (ICommand)Activator.CreateInstance(x.Type)!,
                        x.Attribute!));
            
            if (getAliases)
                return commandDefs;

            // Add aliases
            var allCommands = new Dictionary<string, CommandDef>();

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