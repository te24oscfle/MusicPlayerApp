using MusicPlayerApp;
using Database;

namespace CommandSystem
{
    [Command(
        "getPlaylists", 
        "Lists all playlists", 
        "getPlaylists"
    )]
    public class GetPlaylists : ICommand
    {
        private void PrintTrack(int index, Track track)
        {
            Console.WriteLine($"\t{index}. {track.Metadata.Artist} - {track.Metadata.Title}");
        }
        
        private void PrintPlaylist(Playlist playlist)
        {
            Console.WriteLine("===========================");
            Console.WriteLine($"{playlist.PlaylistId}. {playlist.Title}");
            Console.WriteLine($"\t{playlist.Description}");
            Console.Write("\n");
            
            for(int i = 0; i < playlist.Tracks.Count; i++)
            {
                Track track = playlist.Tracks[i];
                PrintTrack(i, track);
            }
            Console.Write("\n");
        }
        
        public Task Execute(string[] arguments)
        {
            List<Playlist> playlists = DatabaseManager.GetPlaylists();

            if (playlists.Count == 0)
            {
                Console.WriteLine("No playlists");
                return Task.CompletedTask;
            }
            
            foreach(Playlist playlist in playlists)
                PrintPlaylist(playlist);

            return Task.CompletedTask;
        }
    }
}