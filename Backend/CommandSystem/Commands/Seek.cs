using Playback;

namespace CommandSystem
{
    [Command(
        "seek", 
        "Seeks playback to provided position.", 
        "seek <positionInSeconds>"
    )]
    public class Seek : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"No position was provided");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int position);

            AudioPlayer.Seek(position);
            Console.WriteLine($"Seeked to {position}");

            return Task.CompletedTask;
        }
    }

    [Command(
        "seekforward", 
        "Seeks forward", 
        "seekforward <seconds>"
    )]
    public class SeekForward : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"No time was provided");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int seconds);

            int currentPosition = (int)AudioPlayer.GetCurrentPosition();
            int newPosition = currentPosition + seconds;
            AudioPlayer.Seek(newPosition);
            Console.WriteLine($"Seeked to {newPosition}");

            return Task.CompletedTask;
        }
    }

    [Command(
        "seekbackward", 
        "Seeks backward", 
        "seekbackward <seconds>"
    )]
    public class SeekBackward : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"No time was provided");
                return Task.CompletedTask;
            }
            
            int.TryParse(arguments[0], out int seconds);

            int currentPosition = (int)AudioPlayer.GetCurrentPosition();
            int newPosition = Math.Max(0, currentPosition - seconds);
            AudioPlayer.Seek(newPosition);
            Console.WriteLine($"Seeked to {newPosition}");

            return Task.CompletedTask;
        }
    }
}