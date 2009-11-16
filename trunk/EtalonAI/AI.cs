using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    /// <summary>
    /// class for holding AI. inherit this class to create your own AI player
    /// </summary>
    public class AI : IAI
    {
        /// <summary>
        /// time from game start
        /// </summary>
        public static float Time;
        /// <summary>
        /// this player number
        /// </summary>
        int playerNumber;
        /// <summary>
        /// game is AI playing
        /// </summary>
        public static IGame game;
        #region IAI Members
        #region common methods
        /// <summary>
        /// AI player autor
        /// </summary>
        public virtual string Author
        {
            get { return "Anton Etalon"; }
        }
        /// <summary>
        /// AI player description
        /// </summary>
        public virtual string Description
        {
            get { return "lirary for simplifying user AI"; }
        }
        /// <summary>
        /// sets text to be outputed from AI;
        /// use \n for line breaks
        /// </summary>
        /// <param name="text">text to output</param>
        static void SetText(string text)
        {
            game.SetText(text);
        }
        /// <summary>
        /// checks if specified unit is enemy
        /// </summary>
        /// <param name="unitToCheck">unit to check</param>
        /// <returns>true if unit is enemy </returns>
        public bool IsEnemy(IUnit unitToCheck)
        {
            return unitToCheck.PlayerOwner != playerNumber;
        }
        /// <summary>
        /// gets the game
        /// </summary>
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
