namespace MusicPlayerApp
{
    public record CommandDef(
        Command command,
        CommandAttribute Metadata
    )

    [AttributeUsage]


    public interface ICommand : CommandAttribute
    {
        public Task Execute(string[] arguments);
    }
}