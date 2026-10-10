using MusicPlayerApp;
using LibVLCSharp.Shared;

namespace Playback
{
    public record PlaybackState(
        Track? CurrentTrack,
        bool IsPlaying,
        float Position,
        int Volume
    );
    
    public static class AudioPlayer
    {
        private static LibVLC vlcInstance = new LibVLC();
        private static MediaPlayer mediaPlayer = new MediaPlayer(vlcInstance);

        private static Track? currentTrack = null;

        public static void InitilizeAudioPlayer()
        {
            mediaPlayer.EncounteredError += OnEncounteredError;
            mediaPlayer.EndReached += OnEndReached;
            mediaPlayer.MediaChanged += OnMediaChanged;
            mediaPlayer.Paused += OnPaused;
            mediaPlayer.Playing += OnPlaying;
            mediaPlayer.VolumeChanged += OnVolumeChanged;
        }

        #region Event Handlers
        
        private static void OnEncounteredError(object? sender, EventArgs eventArgs)
        {
            Console.WriteLine($"Playback error");
        }

        private static void OnEndReached(object? sender, EventArgs eventArgs)
        {
            Console.WriteLine($"Track ended");

            ThreadPool.QueueUserWorkItem(_ =>
            {
                PlaybackResult nextPlayback = QueueManager.Next();
                PlayTrackFromPlaybackResult(nextPlayback);
            });
        }
        
        private static void OnMediaChanged(object? sender, EventArgs eventArgs)
        {
            Console.WriteLine("Media changed");
        }

        private static void OnPaused(object? sender, EventArgs eventArgs)
        {
            Console.WriteLine("Paused");
        }

        private static void OnPlaying(object? sender, EventArgs eventArgs)
        {
            Console.WriteLine("Playing");
        }

        private static void OnVolumeChanged(object? sender, EventArgs eventArgs)
        {
           
        }

        #endregion
        
        public static PlaybackState GetPlaybackState()
        {
            return new PlaybackState(
                currentTrack, 
                IsPlaying(), 
                GetCurrentPosition(), 
                GetVolume()
            );
        }

        public static void SetTrack(Track track)
        {
            if (IsPlaying()) 
                mediaPlayer.Stop();

            currentTrack = track;

            Media media = new Media(vlcInstance, new Uri(track.FilePath));
            mediaPlayer.Media = media;

        }
        
        public static void PlayTrack(Track track)
        {
            SetTrack(track);
            Resume();
        }

        public static void PlayTrackFromPlaybackResult(PlaybackResult playbackResult)
        {
            Track? track = playbackResult.Track;
            if(track == null)
                return;
            
            SetTrack(track);

            if (playbackResult.ShouldAutoPlay)
            {
                Resume();
            }
        }

        public static bool IsPlaying()
        {
            return mediaPlayer.IsPlaying;
        }

        public static void Pause()
        {
            mediaPlayer.Pause();
        }

        public static void Resume()
        {
            if (mediaPlayer.Media != null)
                mediaPlayer.Play();
        }

        public static void SetVolume(int value)
        {
            mediaPlayer.Volume = value;
        }

        public static int GetVolume()
        {
            return mediaPlayer.Volume;
        }

        public static void Seek(int positionInSeconds)
        {
            mediaPlayer.SeekTo(TimeSpan.FromMilliseconds(positionInSeconds * 1000));
        }

        public static float GetCurrentPosition()
        {
            return mediaPlayer.Position * mediaPlayer.Length/1000;
        }
    }
}