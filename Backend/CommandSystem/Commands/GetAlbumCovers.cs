using Database;
using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "getAlbumCovers", 
        "Lists out all all album covers", 
        "getAlbumCovers"
    )]
    public class GetAlbumCovers : ICommand
    {
        public Task Execute(string[] arguments)
        {
            int[] albumCoverIds = DatabaseHelper.ReadToList(
                """
                SELECT * FROM album_covers
                """,
                reader => reader.GetInt32(reader.GetOrdinal("album_cover_id"))
                ).ToArray();
            
            foreach(int id in albumCoverIds)
                Console.WriteLine(id);
            
            return Task.CompletedTask;
        }
    }
}