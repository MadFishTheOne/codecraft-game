using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;

namespace AINamespace
{
    /// <summary>
    /// this class is used for holding information about enemy squads
    /// </summary>
    public class EnemySquad
    {
        /// <summary>
        /// creates instance
        /// </summary>
        public EnemySquad()
        {            units = new List<IUnit>();        }
        /// <summary>
        /// units belonging to this squad
        /// </summary>
        public List<IUnit> units;
        /// <summary>
        /// gets squad location
        /// circle center is in the mass center of squad
        /// radius is a dist to the farest unit from the mass center
        /// </summary>
        /// <returns>location circle</returns>
        public Circle GetPosition()
        {
            GameVector center = GameVector.Zero;
            foreach (IUnit unit in units)
                center += unit.Position;
            center /= units.Count;
            float currRSq,MaxRSq=0;
            foreach (IUnit unit in units)
            {
                currRSq = GameVector.DistanceSquared(center, unit.Position);
                if (currRSq > MaxRSq)
                { MaxRSq = currRSq; }
            }
            return new Circle(center, (float)Math.Sqrt(MaxRSq));
        }

        internal bool MustBeUnionedWith(EnemySquad enemySquad)
        {

            //get nearest units 
            GameVector center1=this.GetPosition().Center, center2=enemySquad.GetPosition().Center;
            float MinDistSq1=float.MaxValue,MinDistSq2=float.MaxValue;
            IUnit nearestUnit1=null,nearestUnit2=null;
            foreach (IUnit unit in units)
                if (GameVector.DistanceSquared(unit.Position, center2) < MinDistSq1)
                {
                    nearestUnit1 = unit;
                    MinDistSq1 = GameVector.DistanceSquared(unit.Position, center2);
                }
            foreach (IUnit unit in enemySquad.units)
                if (GameVector.DistanceSquared(unit.Position, center1) < MinDistSq2)
                {
                    nearestUnit2 = unit;
                    MinDistSq2 = GameVector.DistanceSquared(unit.Position, center1);
                }
            //find if it's needed to be unioned
            if (nearestUnit2!=null)
            foreach (IUnit unit in units)
                if (GameVector.DistanceSquared(nearestUnit2.Position, unit.Position) < EnemyAnalyzing.OneSquadDistanceSq)
                    return true;
            if (nearestUnit1 != null)
            foreach (IUnit unit in enemySquad.units)
                if (GameVector.DistanceSquared(nearestUnit1.Position, unit.Position) < EnemyAnalyzing.OneSquadDistanceSq)
                    return true;
            return false;
        }

        internal void UnionWith(EnemySquad enemySquad)
        {
            foreach (IUnit unit in enemySquad.units)
                units.Add(unit);
        }

        internal void RemoveDead()
        {
            for (int i = 0; i < units.Count; i++)
                if (units[i].Dead) units.RemoveAt(i);
        }
    }
    /// <summary>
    /// class used for enemy analyzing
    /// </summary>
    public class EnemyAnalyzing
    {
        /// <summary>
        /// distance to join units to one squad
        /// </summary>
        public static float OneSquadDistance = 500;
        /// <summary>
        /// OneSquadDistance squared
        /// </summary>
        public static float OneSquadDistanceSq = OneSquadDistance * OneSquadDistance;
        List<IUnit> enemies;
        /// <summary>
        /// list of enemy squads
        /// </summary>
        public List<EnemySquad> squads;
        int[] SquadNumbers;

        internal EnemyAnalyzing(List<IUnit> enemies)
        {
            this.enemies = enemies;
            SquadNumbers = new int[enemies.Count];
            squads = new List<EnemySquad>();
            EnemySquad squad = new EnemySquad();
            foreach (IUnit unit in enemies)
                squad.units.Add(unit);
            squads.Add(squad);
        }
        internal void Update()
        {
            //remove dead
            for (int i = 0; i < squads.Count; i++)
                squads[i].RemoveDead();
            //separate squads            
            for (int i = 0; i < squads.Count; i++)
                Separate(squads[i]);
            //union squads            
            for (int i=0;i<squads.Count-1;i++)
                for (int j = i + 1; j < squads.Count; j++)
                    if (squads[i].MustBeUnionedWith(squads[j]))
                {
                    squads[i].UnionWith(squads[j]);
                    squads.RemoveAt(j);
                    j--;
                }
        }

        private void Separate(EnemySquad squad)
        {
            //erase squad numbers
            for (int i = 0; i < squad.units.Count; i++)            
                SquadNumbers[i] = 0;
            
            //generate squads
            int NewSquadInd = 1;
            for (int i = 0; i < squad.units.Count; i++)
            {
                if (SquadNumbers[i] == 0)
                {
                    SquadNumbers[i] = NewSquadInd;
                    NewSquadInd++;
                }
                for (int j = i + 1; j < squad.units.Count; j++)
                    if (GameVector.DistanceSquared(squad.units[i].Position, squad.units[j].Position) < OneSquadDistanceSq)
                    {
                        if (SquadNumbers[j] == 0) SquadNumbers[j] = SquadNumbers[i];
                        else
                        {
                            for (int k = 0; k < squad.units.Count; k++)
                                if (SquadNumbers[k] == SquadNumbers[j]) SquadNumbers[k] = SquadNumbers[i];
                        }
                    }
            }

            List<IUnit> NewUnits = new List<IUnit>();
            bool Finished = true;
            int currNumber = SquadNumbers[0];
            for (int i = 0; i < squad.units.Count; i++)
                if (SquadNumbers[i] == currNumber)
                {
                    NewUnits.Add(squad.units[i]);
                    SquadNumbers[i] = 0;
                }
                else Finished = false;


            EnemySquad NewSquad=null;
            while (!Finished)
            {
                currNumber = 0;
                Finished = true;
                NewSquad = null;
                for (int i = 0; i < squad.units.Count; i++)
                {
                    if (SquadNumbers[i] > 0 && currNumber == 0)
                    {
                        currNumber = SquadNumbers[i];
                        NewSquad = new EnemySquad();
                        Finished = false;
                    }
                    if (currNumber > 0 && SquadNumbers[i] == currNumber)
                    {
                        NewSquad.units.Add(squad.units[i]);
                        SquadNumbers[i] = 0;
                    }
                }
                if (NewSquad!=null)
                {
                    squads.Add(NewSquad);
                }
            }
            squad.units = NewUnits;
        }
    }
}
