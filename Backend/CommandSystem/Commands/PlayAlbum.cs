using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "playAlbum", 
        "Sets the queue to album with corresponding albumId", 
        "playAlbum <albumId>"
    )]
    public class PlayAlbum : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                Console.WriteLine("No album ID provided");
                return Task.CompletedTask;
            }

            int.TryParse(arguments[0], out int albumId);
            Album? album = Library.GetAlbumFromAlbumId(albumId);
            if (album is null)
            {
                Console.WriteLine($"Could not find album with albumId={albumId}");
                return Task.CompletedTask;
            }

            List<Track> tracks = album.GetTracks();
            
            QueueManager.ClearQueue(true);
            QueueManager.AddTracksToQueue(tracks, true, false);
            //AudioPlayer.DisposeMedia();
            
            PlaybackResult newPlayback = QueueManager.GetCurrentPlayback();
            AudioPlayer.PlayTrackFromPlaybackResult(newPlayback);

            return Task.CompletedTask;
        }
    }
}