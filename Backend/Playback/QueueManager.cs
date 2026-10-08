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

        public static void RemoveTracksFromQueue(int[] indexes)
        {
            int removedTracks = 0;
            
            // We remove in the reverse order to make sure deleting an index won't offset other indexes 
            // and thus deleting the wrong tracks
            Array.Sort(indexes);
            Array.Reverse(indexes);

            foreach(int index in indexes)
            {
                if (index > queue.Count)
                    continue;
                queue.RemoveAt(index);
                removedTracks++;
            }

            Console.WriteLine($"Removed {removedTracks} tracks from queue");
        }

        public static void MoveTrack(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return;
            }
            
            if (fromIndex >= queue.Count || toIndex >= queue.Count)
            {
                Console.WriteLine("Both fromIndex and toIndex must be smaller than queue.Count");
                return;
            }

            QueueItem item = queue[fromIndex];
            queue.RemoveAt(fromIndex);
            queue.Insert(toIndex, item);

            Console.WriteLine($"Successfully moved {item.Track.Metadata.Title} from {fromIndex} to {toIndex}");
        }
    }
}