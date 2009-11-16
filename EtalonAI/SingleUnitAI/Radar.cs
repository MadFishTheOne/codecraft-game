using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    /// <summary>
    /// radar class, used by UnitPilot class to iterate throught near friends or enemies
    /// </summary>
    public class Radar//tested
    {
        /// <summary>
        /// sight radius for the radar
        /// </summary>
        public float Radius { get; private set; }
        private INearObjectsIterator NearUnits;
        /// <summary>
        /// resets iterator for radar seen units
        /// </summary>
        public void ResetIterator()
        {
            NearUnits.Reset();
        }
        /// <summary>
        /// gets current unit seen by radar. 
        /// use ResetIterator() to reset iteration,
        /// NextFriend() or NextEnemy to iterate throught units
        /// </summary>
        public IUnit CurrUnit { get; private set; }
        /// <summary>
        /// writes to CurrUnit next friend seen by this radar.
        /// use ResetIterator() to reset iteration
        /// </summary>
        /// <returns>true if next friend exists, false if iteration is finished</returns>
        public bool NextFriend()
        {
            do
            {
                CurrUnit = NearUnits.NextUnit();
                if (CurrUnit == null) return false;
            } while (IsEnemy(CurrUnit));
            return true;
        }
        /// <summary>
        /// writes to CurrUnit next enemy seen by this radar.
        /// use ResetIterator() to reset iteration
        /// </summary>
        /// <returns>true if next enemy exists, false if iteration is finished</returns>
        public bool NextEnemy()
        {
            do
            {
                CurrUnit = NearUnits.NextUnit();
                if (CurrUnit == null) return false;
            } while (!IsEnemy(CurrUnit));
            return true;
        }
        /// <summary>
        /// unit that holds this radar
        /// </summary>
        IUnit holderUnit;
        AI ai;
        internal AI AI
        { get { return ai; } }
        internal IGame Game
        { get { return ai.Game; } }
        internal bool IsEnemy(IUnit unit)
        {
            return ai.IsEnemy(unit);
        }
        /// <summary>
        /// creates new radar instance. 
        /// Radar is created automatically for each UnitPilot class instance, has radius a little more than his ShootingRadius
        /// </summary>
        /// <param name="Radius">radar range</param>
        /// <param name="HolderUnit">unit that holds this radar</param>
        /// <param name="AI">AI that HolderUnit belongs to</param>
        internal Radar(float Radius, IUnit HolderUnit, AI AI)
        {
            holderUnit = HolderUnit;
            ai = AI;
            this.Radius = Radius;
            NearUnits = ai.Game.GetNearUnits(holderUnit.Position, Radius);
            Scan();
        }
        internal void Update()
        {
            Scan();
        }
        private void Scan()
        {
            NearUnits.UpdateCenter(holderUnit.Position);
        }
    }
}
