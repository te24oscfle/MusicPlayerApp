namespace CommandSystem
{
    public record CommandDef(
        ICommand command,
        CommandAttribute Metadata
    );

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description {get; }
        public string? Usage { get; }
        public string[]? Aliases { get; }

        public CommandAttribute(
            string name, 
            string description, 
            string usage, 
            params string[] aliases)
        {
            Name = name;
            Description = description;
            Usage = usage;
            Aliases = aliases;
        }
    }

    public interface ICommand
    {
        public Task Execute(string[] arguments);
    }
}