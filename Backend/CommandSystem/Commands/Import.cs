using MusicPlayerApp;

namespace CommandSystem
{
    [Command(
        "import", 
        "Imports supported files from provided directory and sub directories", 
        "import <directoryPath>"
    )]
    public class Import : ICommand
    {
        public Task Execute(string[] arguments)
        {
            string directoryPath = string.Join(" ", arguments);
            Library.ImportFromPath(directoryPath);

            return Task.CompletedTask;
        }
    }
}