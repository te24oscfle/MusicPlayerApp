using Playback;

namespace CommandSystem
{
    [Command(
        "setShuffleState", 
        "Set shuffle state. 0: Disabled. 1: Enabled. Provide no argument to get current shuffle state", 
        "setShuffleState <shuffleState>"
    )]
    public class SetShuffleState : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"Current shuffle state: {QueueManager.ShuffleState}"); 
                return Task.CompletedTask;  
            }
            
            int.TryParse(arguments[0], out int shuffleStateId);
            if(shuffleStateId < 0 || shuffleStateId > 1)
            {
                Console.WriteLine("Invalid shuffle state");
                Console.WriteLine("Valid shuffle states: \n0: Disabled. 1: Shuffle Queue.");
                return Task.CompletedTask;
            }
            ShuffleState shuffleState = (ShuffleState)shuffleStateId;
            QueueManager.SetShuffleState(shuffleState);

            return Task.CompletedTask;
        }
    }
}