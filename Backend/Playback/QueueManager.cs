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

    public enum LoopState
    {
        Disabled,
        LoopQueue,
        LoopSingleTrack
    };

    public enum ShuffleState
    {
        Disabled,
        Enabled
    };
    
    public static class QueueManager
    {
        private static readonly object lockObject = new();
        
        private static List<QueueItem> _queue = new List<QueueItem>();
        private static List<QueueItem> _shuffledQueue = new List<QueueItem>();
        private static int currentIndex = 0;
        private static LoopState _loopState = LoopState.LoopQueue;
        private static ShuffleState _shuffleState = ShuffleState.Disabled;

        public static List<QueueItem> Queue => _shuffleState == ShuffleState.Disabled ? _queue : _shuffledQueue;
        public static int CurrentIndex => currentIndex;
        public static LoopState LoopState => _loopState;
        public static ShuffleState ShuffleState => _shuffleState;
        
        private static void EnableShuffle()
        {
            _shuffledQueue = _queue.ToList();
            
            QueueItem currentItem = _shuffledQueue[currentIndex];
            _shuffledQueue.RemoveAt(currentIndex);
            _shuffledQueue.Shuffle();
            _shuffledQueue.Insert(0, currentItem);
            currentIndex = 0;

            _shuffleState = ShuffleState.Enabled;
        }

        private static void DisableShuffle()
        {
            // TODO: There may be a bug here if tracks in the queue are deleted before shuffle is disabled
            // It may have been solved now by checking if the FindIndex called returned -1, in which case currentIndex is set to zero.
            // It's not the greatest solution, but for now, I'll leave it as is.
            if(currentIndex >= 0 && currentIndex < _shuffledQueue.Count)
            {
                QueueItem currentItem = _shuffledQueue[currentIndex];
                
                int index = _queue.FindIndex(queueItem => queueItem.Track == currentItem.Track); 
                if (index < 0)
                {
                    // Track could not be found in the Queue. Must have been deleted.
                    currentIndex = 0;
                }
                
                currentIndex = index;
            }

            _shuffleState = ShuffleState.Disabled;
        }
        
        public static void SetLoopState(LoopState loopState)
        {
            _loopState = loopState;
        }

        public static void SetShuffleState(ShuffleState shuffleState)
        {
            _shuffleState = shuffleState;
            
            if(_shuffleState == ShuffleState.Enabled)
                EnableShuffle();
            else
                DisableShuffle();
        }
        
        public static void AddTracksToQueue(List<Track> tracks, bool isPersistent, bool addToFront=false)
        {
            List<QueueItem> queueItems = tracks.Select(track => new QueueItem(track, isPersistent)).ToList();
            if (!addToFront || Queue.Count == 0)
                Queue.AddRange(queueItems);
            else
                Queue.InsertRange(currentIndex + 1, queueItems);

            Console.WriteLine($"Added {tracks.Count} tracks");
        }

        public static void ClearQueue(bool clearPersistentTracks=false)
        {
            lock(lockObject)
            {
                int tracksCleared = Queue.Count;
                if (clearPersistentTracks)
                {
                    _queue.Clear();
                    _shuffledQueue.Clear();
                    currentIndex = 0;
                }
                else
                {
                    tracksCleared = Queue.RemoveAll(queueItem => queueItem.IsPersistent == false && queueItem != _queue[currentIndex]);
                    
                    // This is cursed. We call RemoveAll on both queues even though one of these has already been cleared from the statement above.
                    // This is all to avoid doing a weird if statement.
                    _queue.RemoveAll(queueItem => queueItem.IsPersistent == false && queueItem != _queue[currentIndex]);
                    _shuffledQueue.RemoveAll(queueItem => queueItem.IsPersistent == false && queueItem != _queue[currentIndex]);
                }
                    
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
                    if (index < 0 || index > Queue.Count || index <= currentIndex)
                        continue;
                    Queue.RemoveAt(index);
                    removedTracks++;
                }

                Console.WriteLine($"Removed {removedTracks} tracks from queue");   
            }
        }

        public static void MoveTracks(int[] positions, int toIndex)
        {
            lock(lockObject)
            {
                if (toIndex < 0 || toIndex >= Queue.Count)
                    throw new ArgumentOutOfRangeException(nameof(toIndex));
                
                if (toIndex <= currentIndex)
                {
                    Console.WriteLine("Cannot move tracks behind current track");
                    return;
                }
                
                int[] validatedPositions = positions
                    .Distinct()
                    .Where(position => position >= 0 && position < Queue.Count)
                    .Where(position => position > currentIndex)
                    .OrderBy(position => position)
                    .ToArray();

                if (validatedPositions.Length == 0)
                {
                    Console.WriteLine("No valid positions to move");
                    return;
                }

                List<QueueItem> items = validatedPositions
                    .Select(position => Queue[position])
                    .ToList();
            
                foreach(int position in validatedPositions.Reverse())
                    Queue.RemoveAt(position);
                
                Queue.InsertRange(Math.Min(toIndex, Queue.Count), items);
                Console.WriteLine($"Moved {validatedPositions.Length} tracks to {toIndex} in Queue");
            }
        }

        public static PlaybackResult Next()
        {
            lock(lockObject)
            {
                // Check for empty queue
                if (Queue.Count == 0)
                    return new PlaybackResult(null, false);

                // Restart playback of current track is LoopSingleTrack is enabled.
                if(_loopState == LoopState.LoopSingleTrack)
                    return new PlaybackResult(Queue[currentIndex].Track, true);

                // Delete current track if it is not persistent
                QueueItem? currentItem = Queue.ElementAtOrDefault(currentIndex);
                Console.WriteLine(nameof(Queue));
                if(currentItem is not null && !currentItem.IsPersistent)
                {
                    Queue.RemoveAt(currentIndex);
                }
                else
                {
                    // We only want to increment currentIndex if we do not delete a track.
                    // When we delete the current track, the entire queue after currentIndex will be shifted down by one, meaning currentIndex will already be pointing at the correct track
                    currentIndex++;
                }

                if(currentIndex >= Queue.Count)
                {
                    // We reached the end of the queue.
                    currentIndex = 0;
                }

                // currentIndex is now pointing at the next track
                QueueItem? nextItem = Queue.ElementAtOrDefault(currentIndex);
                Track? nextTrack = nextItem?.Track;

                // If the next track is the start of the queue, it means we are starting the queue.
                // We only want to auto play the start of the queue again if looping is enabled.
                return new PlaybackResult(nextTrack, currentIndex == 0 ? _loopState == LoopState.LoopQueue : true);
            }
        }

        public static PlaybackResult Previous()
        {
            lock(lockObject)
            {
                // Check for empty queue
                if (Queue.Count == 0)
                {
                    return new PlaybackResult(null, false);
                }

                // Restart playback of current track is LoopSingleTrack is enabled.
                if(_loopState == LoopState.LoopSingleTrack)
                    return new PlaybackResult(Queue[currentIndex].Track, true);

                currentIndex--;
                if(currentIndex < 0)
                {
                    // We reached the end of the queue.
                    currentIndex = _loopState == LoopState.LoopQueue ? Queue.Count - 1 : 0;
                }

                QueueItem? previousItem = Queue.ElementAtOrDefault(currentIndex);
                Track? previousTrack = previousItem is not null ? previousItem.Track : null;
                return new PlaybackResult(previousTrack, true); // TODO: Create better ShouldAutoPlay condition depending on if loopQueue is enabled.
            }
        }

        public static PlaybackResult GetCurrentPlayback()
        {
            QueueItem? item = Queue.ElementAtOrDefault(currentIndex);
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