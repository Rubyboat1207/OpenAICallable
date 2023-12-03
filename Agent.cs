namespace Rpg.Agents
{
    abstract class BaseAgent
    {
        public void talkCycle(AgentManager manager)
        {
            string? dialogue = null;
            while (shouldContinueTalking(dialogue))
            {
                dialogue = getDialogue();

                if (dialogue is null)
                {
                    continue;
                }

                if (!respond(dialogue))
                {
                    return;
                }
            }

            manager.handOffTo(getHandOffReciever());
        }

        public string? getDialogue()
        {
            return Console.ReadLine();
        }

        public abstract bool shouldContinueTalking(string? input);

        public abstract bool respond(string input);

        public abstract BaseAgent getHandOffReciever();
    }


    // abstract class EnvironmentAgent : BaseAgent
    // {
    //     List<Tuple<string, BaseAgent>> handoffs = new();
    //     BaseAgent? selectedAgent;

    //     public override BaseAgent getHandOffReciever()
    //     {
    //         if(selectedAgent is null)
    //         {
    //             throw new Exception("Conversation with Env was concluded without replacement selected.");
    //         }

    //         return selectedAgent;
    //     }

    //     public override bool respond(string input)
    //     {
    //         var agent = handoffs.Find(pair => pair is not null && pair.Item1 == input);
    //         if (agent is not null)
    //         {
    //             selectedAgent = agent.Item2;

    //             return false;
    //         }

    //         if(int.TryParse(input, out int value))
    //         {
    //             if(value >= 1 && value <= handoffs.Count)
    //             {
    //                 selectedAgent = handoffs[value].Item2;

    //                 return false;
    //             }
    //         }

    //         return true;
    //     }

    //     public override bool shouldContinueTalking(string? input)
    //     {

    //     }
    // }
}