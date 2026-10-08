using System.Runtime.Serialization;
using MusicPlayerApp;

namespace Playback
{
    public record QueueItem(
        Track Track,
        bool IsPersistent
    );
    
    public static class QueueManager
    {
        private static List<QueueItem> queue = new List<QueueItem>();
        private static int currentIndex = 0;
        public static List<QueueItem> Queue => queue;

        public static void AddTracksToQueue(List<Track> tracks, bool isPersistent, bool addToFront=false)
        {
            List<QueueItem> queueItems = tracks.Select(track => new QueueItem(track, isPersistent)).ToList();
            if (!addToFront || queue.Count == 0)
                queue.AddRange(queueItems);
            else
                queue.InsertRange(currentIndex, queueItems);

            Console.WriteLine($"Added {tracks.Count} tracks");
        }

        public static void ClearQueue(bool clearPersistentTracks=false)
        {
            int tracksCleared = queue.Count;
            if (clearPersistentTracks)
                queue.Clear();
            else
                tracksCleared = queue.RemoveAll(queueItem => queueItem.IsPersistent == false);
            
            Console.WriteLine($"Cleared {tracksCleared} tracks");
        }

        public static void RemoveTrackFromQueue(int index)
        {
            
        }
    }
}