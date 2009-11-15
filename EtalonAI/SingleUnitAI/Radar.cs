using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    public class Radar//tested
    {
        //sight radius
        private float radius;
        /// <summary>
        /// enemies in radar radius
        /// </summary>
        List<IUnit> enemies;
        /// <summary>
        /// friends in radius
        /// </summary>
        List<IUnit> friends;
        /// <summary>
        /// unit that holds this radar
        /// </summary>
        IUnit holderUnit;
        /// <summary>
        /// timer of radar
        /// </summary>
        Timer timer;
        AI ai;

        public AI AI
        {get{return ai;}}
        public float Radius
        { get { return radius; } }
        public List<IUnit> Enemies
        { get { return enemies; } }
        public List<IUnit> Friends
        { get { return friends; } }
        public IGame Game
        {
            get { return ai.Game; }
        }
        public Radar(float Radius,IUnit HolderUnit,AI AI)
        {
            holderUnit = HolderUnit;
            timer = new Timer(0.05f,AI.game);
            ai = AI;
            radius = Radius;
            enemies = new List<IUnit>();
            friends = new List<IUnit>();
        }
        public void Update()
        {
            timer.Update();
            if (timer.TimeElapsed)
            {

                timer.Reset();
                Scan();
            }
        }
        private void Scan()
        {
            enemies.Clear();
            friends.Clear();
            List<IUnit> nearUnits;
            List<IShot> nearShots;
            ai.Game.GetNearUnits(holderUnit.Position, radius, out nearUnits, out nearShots);
            foreach (IUnit unit in nearUnits)
                if (!unit.Dead)
            {
                if (ai.IsEnemy(unit))
                {
                    enemies.Add(unit);
                }
                else
                {
                    if (unit != holderUnit)
                    {
                        friends.Add(unit);
                    }
                }
            }
        }
        public class TargetingToEnemy:Predicate
        {
            public override void RecalculateValue()
            {
            }
        }
    }
}
