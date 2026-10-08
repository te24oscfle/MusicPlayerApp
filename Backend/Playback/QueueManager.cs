using System.Runtime.Serialization;
using MusicPlayerApp;

namespace Playback
{
    public record QueueItem(
        Track track,
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
            if (!addToFront)
            {
                queue.AddRange(queueItems);
                return;
            }

            for (int i = 1; i <= queueItems.Count; i++)
            {
                queue.Insert(currentIndex + i, queueItems[i]);
            }
        }
    }
}