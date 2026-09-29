using MusicPlayerApp;
using LibVLCSharp.Shared;

namespace Playback
{
    public static class AudioPlayer
    {
        private static LibVLC vlcInstance = new LibVLC();
        private static MediaPlayer mediaPlayer = new MediaPlayer(vlcInstance);

        private static Track? currentTrack = null;

        public static void PlayTrack(Track track)
        {
            if (IsPlaying()) 
                mediaPlayer.Stop();

            Media media = new Media(vlcInstance, new Uri(track.FilePath));
            mediaPlayer.Play(media);

            currentTrack = track;
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