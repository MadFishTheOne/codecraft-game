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
            get { return "AntonEtalon"; }
        }
        public string Description
        {
            get { return "AI Demo stub"; }
        }
        public void Init(int PlayerNumber, IGame Game)
        {
            playerNumber = PlayerNumber;
            game = Game;
            enemy = null;
            myUnit = null;
        }
        IUnit enemy, myUnit;
        public void Update()
        {
            if (myUnit == null || enemy == null)
            {
                for (int i = 0; i < game.UnitsCount; i++)
                    if (playerNumber == game.GetUnit(i).PlayerOwner)
                    {
                        myUnit = game.GetUnit(i); 
                    }
                    else enemy = game.GetUnit(i);
            }
            if (myUnit != null && enemy != null)
            {
                GameVector toTgt = enemy.Position - myUnit.Position;
                if (toTgt.Length() > 40)
                    myUnit.GoTo(enemy.Position, false);
                else myUnit.SetSpeed(0);
                
                if (GameVector.Cos(myUnit.Forward, toTgt) > 0.8f)
                    myUnit.Shoot();
            }
        }
        #endregion
    }
}
