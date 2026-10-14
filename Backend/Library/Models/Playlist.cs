using System.Security.Cryptography;
using Database;

namespace MusicPlayerApp
{
    public enum PlaylistType
    {
        Manual,
        Smart
    }

    public record PlaylistEntry(
        int PlaylistId,
        int Position,
        int TrackId
    );
    
    public class Playlist
    {
        private List<Track> _tracks = new List<Track>();
        
        public int? PlaylistId { get; internal set; }
        public string Title { get; internal set; }
        public string? Description { get; internal set; }
        public string? ImageFilePath { get; internal set; }
        public PlaylistType PlaylistType { get; }
        public List<Track> Tracks => _tracks;
        
        public Playlist(int playlistId, string title, string? description, string? imageFilePath, PlaylistType playlistType)
        {
            PlaylistId = playlistId;
            Title = title;
            Description = description;
            ImageFilePath = imageFilePath;
            PlaylistType = playlistType;
        }
        public Playlist(string title, PlaylistType playlistType)
        {
            Title = title;
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
        
        public void AddTracks(List<Track> tracks, bool silencePrints=false)
        {
            tracks.Select(track => track.TrackId > 0);
            
            _tracks.AddRange(tracks);
            DatabaseManager.UpdatePlaylistTracks(this);

            if (!silencePrints)
                Console.WriteLine($"Added {tracks.Count} tracks to Playlist {Title}");
        }

        public void RemoveTracks(int[] indexes)
        {
            int removedTracks = 0;

            foreach(int index in indexes)
            {
                if (index >= 0 && index < _tracks.Count)
                {
                    _tracks.RemoveAt(index);
                    removedTracks++;
                }
            }

            DatabaseManager.UpdatePlaylistTracks(this);

            Console.WriteLine($"Removed {removedTracks} from Playlist {Title}");
        }

        public void MoveTracks(int[] positions, int toIndex)
        {
            if (toIndex < 0 || toIndex >= _tracks.Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));

            int[] validatedPositions = positions
                .Distinct()
                .Where(position => position >= 0 && position < _tracks.Count)
                .OrderBy(position => position)
                .ToArray();

            if (validatedPositions.Length == 0)
            {
                Console.WriteLine("No valid positions to move");
                return;
            }
                
            List<Track> tracks = validatedPositions
                .Select(position => _tracks[position])
                .ToList();
            
            foreach(int position in validatedPositions.Reverse())
            {
                _tracks.RemoveAt(position);
            }
            
            _tracks.InsertRange(Math.Min(toIndex, _tracks.Count), tracks);
            
            DatabaseManager.UpdatePlaylistTracks(this);
            Console.WriteLine($"Moved {validatedPositions.Length} tracks to {toIndex} in Playlist {Title}");
        }
    }
}