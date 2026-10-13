using System.Security.Cryptography;
using Database;

namespace MusicPlayerApp
{
    public enum PlaylistType
    {
        Manual,
        Smart
    }

    public class Playlist
    {
        private List<Track> _tracks = new List<Track>();
        
        public int? PlaylistId { get; internal set; }
        public string Title { get; internal set; }
        public string? Description { get; internal set; }
        public string? ImageFilePath { get; internal set; }
        public PlaylistType PlaylistType { get; }
        public List<Track> Tracks => _tracks;
        
        public Playlist(int playlistId, string title, string description, string imageFilePath, PlaylistType playlistType)
        {
            PlaylistId = playlistId;
            Title = title;
            Description = description;
            ImageFilePath = imageFilePath;
            PlaylistType = playlistType;
        }
        
        public void InitilizePlaylistId(int id)
        {
            if(PlaylistId.HasValue)
                throw new InvalidOperationException("The playlistId has already been initilized");
            
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            
            PlaylistId = id;
        }
        
        public void UpdateMetadata(string? title, string? description, string? imageFilePath)
        {
            // Use the new value if it exists, else keep using value already assigned
            Title = title ?? Title;
            Description = description ?? Description;
            ImageFilePath = ImageFilePath ?? ImageFilePath;

            DatabaseManager.UpdatePlaylistMetadata(this);
        }
        
        public void AddTracks(List<Track> tracks)
        {
            tracks.Select(track => track.TrackId > 0);
            
            _tracks.AddRange(tracks);
            DatabaseManager.UpdatePlaylistTracks(this);

            Console.WriteLine($"Added {tracks.Count} to Playlist {Title}");
        }

        public void RemoveTracks(int[] indexes)
        {
            int removedTracks = 0;

            foreach(int index in indexes)
            {
                if (index > 0 && index < _tracks.Count)
                {
                    _tracks.RemoveAt(index);
                    removedTracks++;
                }
            }

            DatabaseManager.UpdatePlaylistTracks(this);

            Console.WriteLine($"Removed {removedTracks} from Playlist {Title}");
        }

        public void MoveTrack(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
                return;
            
            if(fromIndex < 0 || fromIndex > _tracks.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            
            if (toIndex < 0 || toIndex > _tracks.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));

            Track track = _tracks[fromIndex];
            _tracks.RemoveAt(fromIndex);
            _tracks.Insert(toIndex, track);
            
            DatabaseManager.UpdatePlaylistTracks(this);

            Console.WriteLine($"Moved {track.Metadata.Title} from {fromIndex} to {toIndex} in Playlist {Title}");
        }
    }
}