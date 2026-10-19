using Playback;

namespace CommandSystem
{
    [Command(
        "loopState", 
        "Set loop state. 0: Disabled. 1: Loop Queue. 2. Loop Single Track. Provide no argument to get current loop state", 
        "loopState <loopState>"
    )]
    public class SetLoop : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"Current loop state: {QueueManager.LoopState}"); 
                return Task.CompletedTask;  
            }
            
            int.TryParse(arguments[0], out int loopStateId);

            LoopState loopState = (LoopState)loopStateId;
            QueueManager.SetLoopQueue(loopState);

            return Task.CompletedTask;
        }
    }
}