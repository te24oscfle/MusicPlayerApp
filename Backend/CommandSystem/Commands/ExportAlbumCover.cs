using Database;
using Playback;

namespace CommandSystem
{
    [Command(
        "exportAlbumCover", 
        "Exports provided album cover", 
        "exportAlbumCover <albumCoverId>"
    )]
    public class ExportAlbumCover : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"No album cover id was provided");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int albumCoverId);
            DatabaseManager.ExportAlbumCover(albumCoverId);

            return Task.CompletedTask;
        }
    }
}