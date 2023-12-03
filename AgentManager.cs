using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Agents;

namespace Rpg
{
    class AgentManager
    {
        BaseAgent talkTarget;

        public AgentManager(BaseAgent initial) {
            talkTarget = initial;
        }

        public void handOffTo(BaseAgent newAgent)
        {
            talkTarget = newAgent;
        }

        public void startBlocking()
        {
            while (true)
            {
                talkTarget.talkCycle(this);
            }
        }
    }
}
