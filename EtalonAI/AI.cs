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
            get { return "Library for simplifying user AI"; }
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
        /// <summary>
        /// Converts vector to an outputable string
        /// Output example for GameVector(56.45f,67.96f) is "(56;68)"
        /// </summary>
        /// <param name="vec">Vector to output</param>
        /// <returns>Vector in a readable format</returns>
        public static string VectorToString(GameVector vec)
        {
            return "(" + Math.Round(vec.X) + ";" + Math.Round(vec.Y).ToString() + ")";
        }
        #endregion
        #region debug members
        /// <summary>
        /// List of currently alive friends 
        /// </summary>
        public List<UnitPilot> friends;
        /// <summary>
        /// List of currently alive enemies
        /// </summary>
        public List<IUnit> enemies;
        /// <summary>
        /// List of squadrons
        /// It is recommended to add you squadrons to this list for automatic update
        /// </summary>
        public List<Squadron> squadrons;
        /// <summary>
        /// Class for analyzing enemy
        /// </summary>
        public EnemyAnalyzing enemyAnalyzing;
        Timer analyzingTimer;
        /// <summary>
        /// AI initializing method
        /// </summary>
        /// <param name="PlayerNumber">Player number</param>
        /// <param name="Game">Played game</param>
        public virtual void Init(int PlayerNumber, IGame Game)
        {
            
            playerNumber = PlayerNumber;
            friends = new List<UnitPilot>();
            enemies = new List<IUnit>();
            squadrons = new List<Squadron>();
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
            analyzingTimer = new Timer(3);
        }
        
        /// <summary>
        /// Updating AI
        /// </summary>
        public virtual void Update()
        {
          
            Time = game.Time;
            //if (attacktimer.TimeElapsed) Attack();
            for (int i = 0; i < friends.Count; i++)
            {
                friends[i].Update();
                //game.GeometryViewer.DrawRectangle(friends[i].computer.CreateMoveVolume(friends[i].ControlledUnit), Color.Green);
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
            
            if (analyzingTimer.TimeElapsed)
            {
                enemyAnalyzing.Update();
                analyzingTimer.Reset();
            }
        }
        /// <summary>
        /// Creates squadron 
        /// Adds this squadron to the squadrons list
        /// </summary>
        /// <returns>Created squadron</returns>
        public Squadron CreateSquadron()
        {
            Squadron squadron = new Squadron(null, game);
            squadrons.Add(squadron);
            return squadron;
        }        
        #endregion
        #endregion
    }
}
