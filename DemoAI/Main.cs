using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using Microsoft.Xna.Framework;

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
            get { return "A bit better than Simple AI"; }
        }

        public void Init(int PlayerNumber, IGame Game)
        {
            playerNumber = PlayerNumber;
            game = Game;
            enemy = null;
            myUnit = null;
        }
        IUnit enemy,myUnit;
        Vector2 ToVec(GamePoint pt)
        { return new Vector2(pt.X, pt.Y); }
        public void Update()
        {
            if (myUnit==null||enemy==null)
            {
                for (int i = 0; i < game.UnitsCount; i++)
                    if (playerNumber == game.GetUnit(i).PlayerOwner)
                    {myUnit=game.GetUnit(i);                    }
                    else enemy=game.GetUnit(i);                    
            }
            if (myUnit != null && enemy != null)
            {
                myUnit.GoTo(enemy.Position, false);
                Vector2 toTgt = Vector2.Normalize(ToVec(enemy.Position) - ToVec(myUnit.Position));
                if (Vector2.Dot(Vector2.Normalize(ToVec(myUnit.Forward)), toTgt)>0.8f)
                myUnit.Shoot();
            }
        }

        #endregion
    }
}
