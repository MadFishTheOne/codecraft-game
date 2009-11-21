using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using AINamespace;
namespace EtalonAIUser
{
    public class UserAI :AINamespace.AI
    {
        public override string Author
        {
            get { return "Cluster team"; }
        }
        public override string Description
        {
            get { return "doing nothing"; }
        }        
        public override void Init(int PlayerNumber, IGame Game)
        {
            base.Init(PlayerNumber, Game);
        }
        public override void Update()
        {           
            base.Update();           
        }
    }
}
