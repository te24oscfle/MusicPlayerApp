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
            // Found issue: Seemingly when media gets disposed and is set to null, the volume updates to -1, which would be undesirable to send to the client. The fix is to simply only send the client a volume update if the volume is between 0 and 100.
            // If I even send these volume updates to the client, after all the client itself will be updating the volume, surely the client would know itself.
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
            
            DisposeMedia();

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
            {
                DisposeMedia();
                currentTrack = null;
                return;
            }
            
            SetTrack(track);

            if (playbackResult.ShouldAutoPlay)
            {
                Resume();
            }
        }

        public static void DisposeMedia()
        {
            if (IsPlaying())
                mediaPlayer.Stop();
            
            mediaPlayer.Media?.Dispose();
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
            else
            {
                // TODO: This could potentially create a recursion.
                // PlayTrackFromPlaybackResult will eventuelly call Resume(). If the Media is not set at that point, it will return back to this point.
                PlaybackResult playbackResult = QueueManager.GetCurrentPlayback();
                PlayTrackFromPlaybackResult(playbackResult);
            }
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