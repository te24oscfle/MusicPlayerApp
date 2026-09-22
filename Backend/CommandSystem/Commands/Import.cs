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
            try
            {
                Library.ImportFromPath(directoryPath);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return Task.CompletedTask;
        }
    }
}