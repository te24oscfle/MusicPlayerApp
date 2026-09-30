using MusicPlayerApp;
using Playback;

namespace CommandSystem
{
    [Command(
        "volume", 
        "Sets volume to a value between 0 and 100. Will print out current volume if no value is provided", 
        "volume <value?>"
    )]
    public class Volume : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"Current volume is {AudioPlayer.GetVolume()}");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int value);
            int clampedValue = Math.Clamp(value, 0, 100);

            AudioPlayer.SetVolume(clampedValue);
            Console.WriteLine($"Set volume to {clampedValue}");

            return Task.CompletedTask;
        }
    }
}