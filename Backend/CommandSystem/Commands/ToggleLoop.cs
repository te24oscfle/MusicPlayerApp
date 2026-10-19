using Playback;

namespace CommandSystem
{
    [Command(
        "setLoop", 
        "Set loop state. 0: Disabled. 1: Loop Queue. 2. Loop Single Track", 
        "setLoop <loopState>"
    )]
    public class SetLoop : ICommand
    {
        public Task Execute(string[] arguments)
        {
            if (arguments.Length == 0)
            {
                Console.WriteLine("No loop state provided.");
                Console.WriteLine("Valid loop states: \n0: Disabled. 1: Loop Queue. 2. Loop Single Track");
                return Task.CompletedTask;  
            }
            
            int.TryParse(arguments[0], out int loopStateId);

            LoopState loopState = (LoopState)loopStateId;
            QueueManager.SetLoopQueue(loopState);

            return Task.CompletedTask;
        }
    }
}