using Playback;

namespace CommandSystem
{
    [Command(
        "setLoopState", 
        "Set loop state. 0: Disabled. 1: Loop Queue. 2. Loop Single Track. Provide no argument to get current loop state", 
        "setLoopState <loopState>"
    )]
    public class SetLoopState : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine($"Current loop state: {QueueManager.LoopState}"); 
                return Task.CompletedTask;  
            }
            
            int.TryParse(arguments[0], out int loopStateId);
            if(loopStateId < 0 || loopStateId > 2)
            {
                Console.WriteLine("Invalid loop state");
                Console.WriteLine("Valid loop states: \n0: Disabled. 1: Loop Queue. 2. Loop Single Track.");
                return Task.CompletedTask;
            }
            LoopState loopState = (LoopState)loopStateId;
            QueueManager.SetLoopState(loopState);

            return Task.CompletedTask;
        }
    }
}