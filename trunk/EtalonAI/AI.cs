using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    public class AI : IAI
    {
        /// <summary>
        /// time from game start
        /// </summary>
        public static float Time;
        int playerNumber;
        public static IGame game;
        #region IAI Members
        #region common methods

        public virtual string Author
        {
            get { return "Anton Etalon"; }
        }
        public virtual string Description
        {
            get { return "lirary for simplifying user AI"; }
        }
        static void SetText(string text)
        {
            game.SetText(text);
        }
        public bool IsEnemy(IUnit unitToCheck)
        {
            return unitToCheck.PlayerOwner != playerNumber;
        }
        public IGame Game
        {
            get
            {
                return game;
            }
        }
        public static string VectorToString(GameVector vec)
        {
            return "(" + Math.Round(vec.X) + ";" + Math.Round(vec.Y).ToString() + ")";
        }
        #endregion
        #region debug members
        public List<UnitPilot> friends;
        public List<IUnit> enemies;
        public List<SquadronColonel> squadrons;
        public EnemyAnalyzing enemyAnalyzing;
        Timer analyzingTimer;
        public virtual void Init(int PlayerNumber, IGame Game)
        {
            
            playerNumber = PlayerNumber;
            friends = new List<UnitPilot>();
            enemies = new List<IUnit>();
            squadrons = new List<SquadronColonel>();
            game = Game;
            IUnit unit;            
       
       
            for (int i = 0; i < game.UnitsCount; i++)
            {
                unit = game.GetUnit(i);
                if (unit.PlayerOwner == playerNumber)
                {
                    UnitPilot pilot = new UnitPilot(unit, this);
                    friends.Add(pilot);
                }
                else
                    enemies.Add(unit);
            }
            enemyAnalyzing = new EnemyAnalyzing(enemies);
            analyzingTimer = new Timer(3, game);
        }
        

        public virtual void Update()
        {
            Time = game.Time;
            //if (attacktimer.TimeElapsed) Attack();
            for (int i = 0; i < friends.Count; i++)
            {
                friends[i].Update();
                if (friends[i].ControlledUnit.HP <= 0)
                {
                    friends.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].HP <= 0)
                {
                    enemies.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < squadrons.Count; i++)
            {
                squadrons[i].Update();
            }
            analyzingTimer.Update();
            if (analyzingTimer.TimeElapsed)
            {
                enemyAnalyzing.Update();
                analyzingTimer.Reset();
            }
        }
        #endregion
        #endregion
    }
}
