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
            Console.WriteLine(addToFront);
            
            List<QueueItem> queueItems = tracks.Select(track => new QueueItem(track, isPersistent)).ToList();
            if (!addToFront || queue.Count == 0)
            {
                Console.WriteLine("Adding to back of queue");
                queue.AddRange(queueItems);
                return;
            }

            Console.WriteLine("Adding to front of queue");
            queue.InsertRange(currentIndex, queueItems);
        }
    }
}