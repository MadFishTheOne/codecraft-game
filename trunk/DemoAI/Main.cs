using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;

namespace DemoAI
{
    public class Main : IAI
    {
        int playerNumber;
        IGame game;
        #region IAI Members

        public string Author
        {
            get { return "Mad Fish"; }
        }

        public string Description
        {
            get { return "Simple demo AI"; }
        }

        public void Init(int PlayerNumber, IGame Game)
        {
            playerNumber = PlayerNumber;
            game = Game;
        }

        public void Update()
        {
            for (int i = 0; i < game.UnitsCount; i++)
            {
                //Try to control ALL units :) That's a joke - core will not allow this
                //Only player's ships will be controlled
                game.GetUnit(i).GoTo(new GamePoint(300, 200), false);
                game.GetUnit(i).Shoot();
            }
        }

        #endregion
    }
}
