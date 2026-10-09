using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using MusicPlayerApp;

namespace Playback
{
    public record QueueItem(
        Track Track,
        bool IsPersistent
    );

    public record PlaybackResult(
        Track? Track,
        bool ShouldAutoPlay
    );
    
    public static class QueueManager
    {
        private static List<QueueItem> queue = new List<QueueItem>();
        private static int currentIndex = 0;
        private static bool loopQueue = true;
        private static bool shuffleQueue = false;

        public static List<QueueItem> Queue => queue;
        public static bool LoopQueue => loopQueue;
        public static bool ShuffleQueue => shuffleQueue;


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
                if (index > queue.Count || index < 0)
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
            
            if (fromIndex >= queue.Count || toIndex >= queue.Count || fromIndex < 0 || toIndex < 0)
            {
                Console.WriteLine("Both fromIndex and toIndex must be smaller than queue.Count and greater or equal to zero");
                return;
            }

            QueueItem item = queue[fromIndex];
            queue.RemoveAt(fromIndex);
            queue.Insert(toIndex, item);

            Console.WriteLine($"Successfully moved {item.Track.Metadata.Title} from {fromIndex} to {toIndex}");
        }

        // This method is very buggy. I'm scared.
        // Need to properly handle if tracks get deleted from the queue while the queue is playing.
        // If the current track gets deleted things get messy.
        // I really don't want to do this right now.
        public static PlaybackResult AdvanceQueue()
        {
            bool shouldAutoPlay = true;
            
            if (queue.Count < 0)
                return new PlaybackResult(null, false);

            if (currentIndex > queue.Count)
            {
                currentIndex = 0;
            }

            // If current track is persistent, remove it from queue
            // We only want to increment currentIndex if we dont remove a track. If we remove a track, following tracks will be shifted down and currentIndex will already point to the correct track.
            QueueItem? currentItem = queue.ElementAtOrDefault(currentIndex);
            if (currentItem is not null && currentItem.IsPersistent)
                queue.RemoveAt(currentIndex);
            else
                currentIndex++;

            // The queue has finished. We only want the AudioPlayer to start the queue again if looping is on.
            if (currentIndex >= queue.Count)
            {
                currentIndex = 0;
                shouldAutoPlay = loopQueue;
            }

            QueueItem? nextItem = queue.ElementAtOrDefault(currentIndex);
            Track? nextTrack = nextItem is not null ? nextItem.Track : null;
            return new PlaybackResult(nextTrack, shouldAutoPlay);
        }
    }
}