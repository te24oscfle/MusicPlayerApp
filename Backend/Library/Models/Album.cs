namespace MusicPlayerApp
{
    public class Album
    {
        public string Title;
        public string Artist;
        public Dictionary<int, List<Track>> Discs;

        public Album(string title, string artist, Dictionary<int, List<Track>> discs)
        {
            Title = title;
            Artist = artist;
            Discs = discs;
        }

        public void AddTracks(List<Track> tracks)
        {
            // Add tracks
            foreach(Track track in tracks)
            {
                // Check if disc exists
                if(!Discs.ContainsKey(track.Metadata.DiscNumber))
                {
                    // It doesn't, create it
                    Discs.Add(track.Metadata.DiscNumber, new List<Track>());
                }

                // Prevent duplicate tracks
                List<Track> disc = Discs[track.Metadata.DiscNumber];
                if (disc.Contains(track))
                    continue;
                
                // Add track to disc
                disc.Add(track);
            }

            // Sort each disc
            foreach (var pair in Discs)
            {
                pair.Value.Sort((a, b) =>
                    a.Metadata.TrackNumber.CompareTo(b.Metadata.TrackNumber)
                );
            }
        }
    }
}