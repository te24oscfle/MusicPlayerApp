using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using LibVLCSharp.Shared;
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
        private static readonly object lockObject = new();
        
        private static List<QueueItem> queue = new List<QueueItem>();
        private static int currentIndex = 0;
        private static bool loopQueue = true;
        private static bool shuffleQueue = false;

        public static List<QueueItem> Queue => queue;
        public static int CurrentIndex => currentIndex;
        public static bool LoopQueue => loopQueue;
        public static bool ShuffleQueue => shuffleQueue;

        public static void AddTracksToQueue(List<Track> tracks, bool isPersistent, bool addToFront=false)
        {
            List<QueueItem> queueItems = tracks.Select(track => new QueueItem(track, isPersistent)).ToList();
            if (!addToFront || queue.Count == 0)
                queue.AddRange(queueItems);
            else
                queue.InsertRange(currentIndex + 1, queueItems);

            Console.WriteLine($"Added {tracks.Count} tracks");
        }

        public static void ClearQueue(bool clearPersistentTracks=false)
        {
            lock(lockObject)
            {
                int tracksCleared = queue.Count;
                if (clearPersistentTracks)
                {
                    queue.Clear();
                    currentIndex = 0;
                }
                else
                    tracksCleared = queue.RemoveAll(queueItem => queueItem.IsPersistent == false && queueItem != queue[currentIndex]);
                
                Console.WriteLine($"Cleared {tracksCleared} tracks");
            }
        }

        public static void RemoveTracksFromQueue(int[] indexes)
        {
            lock(lockObject)
            {
                int removedTracks = 0;
                
                // We remove in the reverse order to make sure deleting an index won't offset other indexes 
                // and thus deleting the wrong tracks
                Array.Sort(indexes);
                Array.Reverse(indexes);

                foreach(int index in indexes)
                {
                    // Index must be greater or equal to zero to prevent Index out of range
                    // Index must be smaller than queue length to prevent Index out of range
                    // Index must be greater than currentIndex to prevent currentIndex from shifting
                    if (index < 0 || index > queue.Count || index <= currentIndex)
                        continue;
                    queue.RemoveAt(index);
                    removedTracks++;
                }

                Console.WriteLine($"Removed {removedTracks} tracks from queue");   
            }
        }

        public static void MoveTracks(int[] positions, int toIndex)
        {
            lock(lockObject)
            {
                if (toIndex < 0 || toIndex >= queue.Count)
                    throw new ArgumentOutOfRangeException(nameof(toIndex));
                
                if (toIndex <= currentIndex)
                {
                    Console.WriteLine("Cannot move tracks behind current track");
                    return;
                }
                
                int[] validatedPositions = positions
                    .Distinct()
                    .Where(position => position >= 0 && position < queue.Count)
                    .Where(position => position > currentIndex)
                    .OrderBy(position => position)
                    .ToArray();
                
                foreach(int position in validatedPositions)
                {
                    Console.WriteLine(position);
                }

                if (validatedPositions.Length == 0)
                {
                    Console.WriteLine("No valid positions to move");
                    return;
                }

                List<QueueItem> items = validatedPositions
                    .Select(position => queue[position])
                    .ToList();
            
                foreach(int position in validatedPositions.Reverse())
                {
                    queue.RemoveAt(position);
                }
                
                queue.InsertRange(Math.Min(toIndex, queue.Count), items);
                Console.WriteLine($"Moved {validatedPositions.Length} tracks to {toIndex} in Queue");
            }
        }

        public static PlaybackResult Next()
        {
            lock(lockObject)
            {
                // Check for empty queue
                if (queue.Count == 0)
                {
                    return new PlaybackResult(null, false);
                }

                // Delete current track if it is not persistent
                QueueItem? currentItem = queue.ElementAtOrDefault(currentIndex);
                if(currentItem is not null && !currentItem.IsPersistent)
                {
                    queue.RemoveAt(currentIndex);
                }
                else
                {
                    // We only want to increment currentIndex if we do not delete a track.
                    // When we delete the current track, the entire queue after currentIndex will be shifted down by one, meaning currentIndex will already be pointing at the correct track
                    currentIndex++;
                }

                if(currentIndex >= queue.Count)
                {
                    // We reached the end of the queue.
                    currentIndex = 0;
                }

                // currentIndex is now pointing at the next track
                QueueItem? nextItem = queue.ElementAtOrDefault(currentIndex);
                Track? nextTrack = nextItem is not null ? nextItem.Track : null;

                // If the next track is the start of the queue, it means we are starting the queue.
                // We only want to auto play the start of the queue again if looping is enabled.
                return new PlaybackResult(nextTrack, currentIndex == 0 ? loopQueue : true);
            }
        }

        public static PlaybackResult Previous()
        {
            lock(lockObject)
            {
                // Check for empty queue
                if (queue.Count == 0)
                {
                    return new PlaybackResult(null, false);
                }

                currentIndex--;
                if(currentIndex < 0)
                {
                    // We reached the end of the queue.
                    currentIndex = loopQueue ? queue.Count - 1 : 0;
                }

                QueueItem? previousItem = queue.ElementAtOrDefault(currentIndex);
                Track? previousTrack = previousItem is not null ? previousItem.Track : null;
                return new PlaybackResult(previousTrack, true); // TODO: Create better ShouldAutoPlay condition depending on if loopQueue is enabled.
            }
        }

        public static PlaybackResult GetCurrentPlayback()
        {
            QueueItem? item = queue.ElementAtOrDefault(currentIndex);
            Track? track = item is not null ? item.Track : null;
            return new PlaybackResult(track, true); // TODO: Create ShouldAutoPlay condition? Unsure.
        }

        public static void PlayAlbum(Album album)
        {
            List<Track> tracks = album.GetTracks();
            
            ClearQueue(true);
            AddTracksToQueue(tracks, true, false);
            
            PlaybackResult newPlayback = GetCurrentPlayback();
            AudioPlayer.PlayTrackFromPlaybackResult(newPlayback);
        }

        public static void PlayPlaylist(Playlist playlist)
        {
            List<Track> tracks = playlist.Tracks;
            
            ClearQueue(true);
            AddTracksToQueue(tracks, true, false);
            
            PlaybackResult newPlayback = GetCurrentPlayback();
            AudioPlayer.PlayTrackFromPlaybackResult(newPlayback);
        }
    }
}