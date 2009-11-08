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
            get { return "Cluster Team"; }
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
            if (myUnit == null)
                game.SetText("FUCK!\nEPIC FAIL!!11");
            if (enemy == null)
                game.SetText("Yahoo!\nI'm the winner!");
            
            if (myUnit != null && enemy != null)
            {
                myUnit.Text = "i'm myUnit";
                INearObjectsIterator it=game.GetNearUnits(myUnit.Position, 200);
                game.GeometryViewer.DrawCircle(new Circle(myUnit.Position, 200), new Color(0, 0, 1, 0.5f));
                IUnit nearUnit;
                while ((nearUnit = it.NextUnit())!=null)
                {
                    game.GeometryViewer.DrawCircle(new Circle(nearUnit.Position, 20), Color.Red);
                }

                game.SetText("pos.X="+myUnit.Position.X+
                    "\npos.Y="+myUnit.Position.Y+
                    "\nforw.X="+myUnit.Forward.X+
                    "\nforw.Y="+myUnit.Forward.Y+
                    "\nangle="+myUnit.RotationAngle);
                GameVector toTgt = enemy.Position - myUnit.Position;
                if (toTgt.Length() > 40)
                    myUnit.GoTo(enemy.Position, false);
                else myUnit.SetSpeed(0);
                if (GameVector.Cos(myUnit.Forward, toTgt) > 0.8f)
                    myUnit.Shoot();
                if (myUnit.HP < 0)
                    myUnit = null;
                if (enemy.HP < 0)
                    enemy = null;
            }
        }
        #endregion
    }
}
