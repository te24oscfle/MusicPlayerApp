using MusicPlayerApp;
using LibVLCSharp.Shared;

namespace Playback
{
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
            Console.WriteLine("Volume changed");
        }

        #endregion
        
        public static void PlayTrack(Track track)
        {
            if (IsPlaying()) 
                mediaPlayer.Stop();

            currentTrack = track;

            Media media = new Media(vlcInstance, new Uri(track.FilePath));
            mediaPlayer.Play(media);
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
    }
}