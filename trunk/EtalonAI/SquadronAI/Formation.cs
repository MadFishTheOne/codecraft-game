using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using System.Collections;
namespace AINamespace
{
    /// <summary>
    /// enumeration of formation types
    /// </summary>
    public enum FormationTypes
    {
        /// <summary>
        /// units are positioned in one line
        /// </summary>
        Line,
        /// <summary>
        /// units are positioned in stagger with one unit width
        /// </summary>
        Stagger,
        /// <summary>
        /// units are positioned in bar with specified width and depth
        /// </summary>
        Bar,
        /// <summary>
        /// units are not strongly positioned, they are flying as swarm
        /// </summary>
        Swarm,
        ///// <summary>
        ///// Units are situated as they where situated at the creation time
        ///// </summary>
        //Custom
    }
    /// <summary>
    /// class for containing units array and make them to hold array figure
    /// </summary>
    public abstract class Formation : IEnumerable,IEnumerator
    {
        /// <summary>
        /// leader of formation. other units are comparing their position with leader
        /// leader is in subordinates too        
        /// </summary>
        protected UnitPilot leader;
        /// <summary>
        /// pair of pilot and his position in order
        /// </summary>
        internal class PilotPositionPair
        {
            public UnitPilot Pilot;
            public RelativeVector Position;
            public PilotPositionPair(UnitPilot Pilot, RelativeVector Position)
            {
                this.Pilot = Pilot;
                this.Position = Position;
            }

        }
        /// <summary>
        /// Gets subordinate units count
        /// </summary>
        public int Count
        {
            get { return subordinates.Count; }
        }
        /// <summary>
        /// list of units in formation
        /// </summary>
        internal List<PilotPositionPair> subordinates;
        /// <summary>
        /// forward angle of this formation
        /// </summary>
        protected float rotationAngle;
        //positions of units in formation
        /// <summary>
        /// creates new instance of formation
        /// </summary>
        /// <param name="Leader">leader of the formation</param>
        /// <param name="Subordinates">list of units in this formation. can be clear, units may be added later</param>
        public Formation(UnitPilot Leader, List<UnitPilot> Subordinates)
        {
            leader = Leader;
            subordinates = new List<PilotPositionPair>();
            foreach (UnitPilot pilot in Subordinates)
            {
                subordinates.Add(new PilotPositionPair(pilot, new RelativeVector(GameVector.Zero)));
            }

        }
        /// <summary>
        /// updates formation
        /// </summary>
        internal void Update()
        {
            UnitPilot cruiser = null;
            UnitPilot corvette = null;
            UnitPilot destroyer = null;           

            for (int i = 0; i < subordinates.Count; i++)
            {                
                if (subordinates[i].Pilot.ControlledUnit.Dead)
                {
                    subordinates.RemoveAt(i);
                    i--;
                }                
            }
            if ((leader == null || leader.ControlledUnit.Dead) && subordinates.Count > 0)
            {
                leader = subordinates[0].Pilot;
                for (int i = 0; i < subordinates.Count; i++)
                {                    
                    if (subordinates[i].Pilot.Type == ShipTypes.Destroyer) destroyer = subordinates[i].Pilot;
                    if (subordinates[i].Pilot.Type == ShipTypes.Corvette) corvette = subordinates[i].Pilot;
                    if (subordinates[i].Pilot.Type == ShipTypes.Cruiser) cruiser = subordinates[i].Pilot;
                }
            }
        }
        /// <summary>
        /// gets unit's position in order
        /// </summary>
        /// <param name="Index">index of unit in subordinates list</param>
        /// <returns>relative vector of position in order</returns>
        public RelativeVector GetPosition(int Index)
        {
            return subordinates[Index].Position;
        }
        /// <summary>
        /// gets formation forward angle in radians
        /// </summary>
        public float Angle
        { get { return rotationAngle; } }
        /// <summary>
        /// calculates mistake of holding formation figure. Has no sense for swarm formation
        /// calculates max distance from unit to his position
        /// </summary>
        /// <returns>0 if ideal positioning, 1 if max mistake=100 and so on</returns>
        public float PositionHoldingMistake()
        {
            float averageMistake = 0;
            int ind = 0;
            float maxMistake = 0;
            float currMistake;
            foreach (PilotPositionPair pilotPos in subordinates)
            {
                currMistake = GameVector.DistanceSquared(pilotPos.Pilot.ControlledUnit.Position, pilotPos.Position.Value) / 10000;
                if (currMistake > maxMistake) { maxMistake = currMistake; }

                averageMistake += currMistake;
                ind++;
            }
            averageMistake /= subordinates.Count;
            return maxMistake;//mistake ;

        }
        /// <summary>
        /// Calculates max mistake of units forward to formation forward direction.
        /// Returns 0 if units are positioned ideal, 1 if max mistake is pi/2
        /// </summary>
        public float AngleHoldingMistake
        {
            get
            {
                //float mistake = 0;
                //int ind = 0;
                //foreach (PilotPositionPair pilotPos in subordinates)
                //{
                //    mistake += AngleClass.Distance(pilotPos.Pilot.ControlledUnit.RotationAngle, rotationAngle) / (AngleClass.pi * 0.5f);
                //    ind++;
                //}
                //return mistake / subordinates.Count;
                float MaxMistake = 0;
                float currMistake;

                foreach (PilotPositionPair pilotPos in subordinates)
                {
                    currMistake = AngleClass.Distance(pilotPos.Pilot.ControlledUnit.RotationAngle, rotationAngle) / (AngleClass.pi * 0.5f);
                    if (currMistake > MaxMistake) MaxMistake = currMistake;

                }
                return MaxMistake;

            }
        }
        /// <summary>
        /// gets a worst UnitPilot - pilot that has the biggest mistake of holding his place in formation order
        /// may be used to exclude pilot from squad and make squad to ahcive it's aims without that unit
        /// </summary>
        /// <returns>worst unit</returns>
        public UnitPilot WorstPilot()
        {
            float biggestMistake = 0;
            UnitPilot worstPilot = null;
            float currmistake;
            int ind = 0;
            foreach (PilotPositionPair pilotPos in subordinates)
            {
                currmistake = GameVector.DistanceSquared(pilotPos.Pilot.ControlledUnit.Position, GetPosition(ind).Value);
                if (biggestMistake < currmistake)
                {
                    biggestMistake = currmistake;
                    worstPilot = pilotPos.Pilot;
                }
            }
            return worstPilot;
        }
        /// <summary>
        /// gets formation leader
        /// </summary>
        public UnitPilot Leader
        {
            get
            {
                return leader;
            }
        }
        private float DistToPositionSq(int PilotInd, int PtInd)
        {
            return GameVector.DistanceSquared(subordinates[PilotInd].Pilot.ControlledUnit.Position, GetPosition(PtInd).Value);
        }
        /// <summary>
        /// sorts units. Every unit become to be in pair with the most appropriate position for it in order
        /// </summary>
        public void SortUnitsByNearanceToPositions()
        {
            int CPts = subordinates.Count;
            int[] PilotIndByPtInd = new int[CPts];
            int[] PtIndByPilotInd = new int[CPts];
            bool complete = false;
            for (int i = 0; i < CPts; i++)
            {
                PilotIndByPtInd[i] = -1;//no pilot
                PtIndByPilotInd[i] = -1;//no point
            }
            do
            {
                complete = true;
                for (int ptInd = 0; ptInd < CPts; ptInd++)//for all points
                {
                    float minDistSq = float.PositiveInfinity;
                    if (PilotIndByPtInd[ptInd] != -1) minDistSq = DistToPositionSq(PilotIndByPtInd[ptInd], ptInd);
                    float currDistSq;
                    int minDistPilotInd = -1;
                    for (int pilotInd = 0; pilotInd < CPts; pilotInd++)//find nearest pilot
                        if (pilotInd != PilotIndByPtInd[ptInd])//if this pilot is not a pilot of this point
                        {
                            currDistSq = DistToPositionSq(pilotInd, ptInd);
                            if (currDistSq < minDistSq//if this pilot is nearer
                                && (PtIndByPilotInd[pilotInd] == -1 || DistToPositionSq(pilotInd, PtIndByPilotInd[pilotInd]) > currDistSq))//and accesible
                            {
                                minDistPilotInd = pilotInd;
                                minDistSq = currDistSq;
                            }
                        }
                    if (minDistPilotInd != -1)//if nearest accesible unit was found
                    {
                        complete = false;
                        //write out a pilot from the point
                        if (PtIndByPilotInd[minDistPilotInd] != -1)
                            PilotIndByPtInd[PtIndByPilotInd[minDistPilotInd]] = -1;
                        if (PilotIndByPtInd[ptInd] != -1)
                            PtIndByPilotInd[PilotIndByPtInd[ptInd]] = -1;
                        PilotIndByPtInd[ptInd] = minDistPilotInd;//write pilot in the point
                        PtIndByPilotInd[minDistPilotInd] = ptInd;//write point in the pilot               
                    }
                }
            } while (!complete);



            //minimize maximum dist between unit and his position in order
            complete = false;
            do
            {
                complete = true;
                for (int currPos1 = 0; currPos1 < subordinates.Count; currPos1++)
                    for (int currPos2 = 0; currPos2 < subordinates.Count; currPos2++)
                        if (currPos1 != currPos2)
                        {

                            int Pilot1Ind = PilotIndByPtInd[currPos1];
                            int Pilot2Ind = PilotIndByPtInd[currPos2];
                            float Pos1Pilot1DistSq = GameVector.DistanceSquared(subordinates[currPos1].Position.Value, subordinates[Pilot1Ind].Pilot.ControlledUnit.Position);
                            float Pos2Pilot2DistSq = GameVector.DistanceSquared(subordinates[currPos2].Position.Value, subordinates[Pilot2Ind].Pilot.ControlledUnit.Position);

                            float Pos1Pilot2DistSq = GameVector.DistanceSquared(subordinates[currPos1].Position.Value, subordinates[Pilot2Ind].Pilot.ControlledUnit.Position);
                            float Pos2Pilot1DistSq = GameVector.DistanceSquared(subordinates[currPos2].Position.Value, subordinates[Pilot1Ind].Pilot.ControlledUnit.Position);

                            //if swaping position's pilots can will minimize maximum dist then do it
                            if (Math.Max(Pos1Pilot1DistSq, Pos2Pilot2DistSq) > Math.Max(Pos1Pilot2DistSq, Pos2Pilot1DistSq))
                            {
                                complete = false;

                                PilotIndByPtInd[currPos1] = Pilot2Ind;
                                PilotIndByPtInd[currPos2] = Pilot1Ind;
                                PtIndByPilotInd[Pilot1Ind] = currPos2;
                                PtIndByPilotInd[Pilot2Ind] = currPos1;
                            }
                        }

            } while (!complete);


            //order list of units by given indexes
            if (subordinates.Count == 1) PilotIndByPtInd[0] = 0; //hack
            List<PilotPositionPair> newSubordinates = new List<PilotPositionPair>();
            for (int i = 0; i < subordinates.Count; i++)
            {
                PilotPositionPair pilotPos = new PilotPositionPair(subordinates[PilotIndByPtInd[i]].Pilot, subordinates[i].Position);
                newSubordinates.Add(pilotPos);
                //newSubordinates.Add(subordinates[PilotIndByPtInd[i]]);
            }
            subordinates.Clear();
            subordinates = newSubordinates;



            //send units to their new positions
            int ind = 0;
            foreach (UnitPilot pilot in this)
            {
                if (PilotIndByPtInd[ind] != ind || PtIndByPilotInd[ind] != ind)
                {
                    pilot.GoTo(this.GetPosition(ind));
                }
                ind++;
            }
        }
        internal void MakePositionsRelative()
        {
            List<RelativeVector> newPositions = new List<RelativeVector>();
            for (int i = 0; i < subordinates.Count; i++)
            {
                PilotPositionPair pilotPos = subordinates[i];
                float angle, dist;
                dist = GameVector.Distance(Leader.ControlledUnit.Position, pilotPos.Position.Value);
                angle = Leader.ControlledUnit.AngleTo(pilotPos.Position.Value);
                pilotPos.Position = new RelativeVector(Leader.ControlledUnit, angle, dist);
                //newPositions.Add(new RelativeVector(Leader.ControlledUnit, angle, dist));
            }
            //positions = newPositions;
        }
        /// <summary>
        /// gets mass center of formation units
        /// </summary>
        /// <returns>mass center vector</returns>
        public GameVector GetMassCenter()
        {
            GameVector center = GameVector.Zero;
            foreach (PilotPositionPair pilotPos in subordinates)
            {
                center += pilotPos.Pilot.ControlledUnit.Position;
            }
            center /= (float)subordinates.Count;
            return center;
        }        
        //public void DrawPositions()
        //{
        //    for (int i = 0; i < subordinates.Count; i++)
        //        AI.game.GeometryViewer.DrawPoint(GetPosition(i).Value, Color.Green);
        //}
        //public void DrawDestinations()
        //{
        //    for (int i = 0; i < subordinates.Count; i++)
        //    {
        //        AI.game.GeometryViewer.DrawLine(new Line(subordinates[i].Pilot.ControlledUnit.Position, GetPosition(i).Value), Color.Green);
        //    }
        //}
        /// <summary>
        /// sets formation forward angle
        /// </summary>
        /// <param name="newAngle">angle in radians</param>
        public abstract void SetAngle(float newAngle);
        /// <summary>
        /// gets type of this formation
        /// </summary>
        public abstract FormationTypes Type { get; }
        /// <summary>
        /// Creates positions for units in this formation
        /// </summary>
        public abstract void CreatePositions();
        /// <summary>
        /// Creates positions for units in this formation
        /// </summary>
        /// <param name="CenterPosition">Center of the new order</param>
        /// <param name="Angle">Forward angle of the new position</param>
        public abstract void CreatePositions(GameVector CenterPosition, float Angle);
        /// <summary>
        /// adds a unit to formation
        /// </summary>
        /// <param name="unitPilot">unit to add</param>
        public void Add(UnitPilot unitPilot)
        {
            if (leader == null) leader = unitPilot;
            subordinates.Add(new PilotPositionPair(unitPilot, new RelativeVector(unitPilot.ControlledUnit.Position)));
        }
        /// <summary>
        /// removes unit from formation
        /// </summary>
        /// <param name="unitPilot">unit ot remove</param>
        public void Remove(UnitPilot unitPilot)
        {
            for (int i = 0; i < subordinates.Count; i++)
                if (unitPilot.Equals(subordinates[i].Pilot))
                {
                    subordinates.RemoveAt(i);
                    break;
                }
        }
        /// <summary>
        /// defines if there are at least one enemy that is less than on CloseDist distance to some unit from this formation
        /// </summary>
        /// <param name="CloseDist">dist to check</param>
        /// <returns>true if close enemy exists</returns>
        public bool CloseEnemyExists(float CloseDist)
        {
            foreach (PilotPositionPair PilotPos in subordinates)
            {
                if (PilotPos.Pilot.GetClosestEnemyDist() < CloseDist)
                    return true;
            }
            return false;

        }
        /// <summary>
        /// Indexes units in squadron's formation
        /// </summary>
        /// <param name="ind">Unit index</param>
        /// <returns>Requested unit</returns>
        public UnitPilot this[int ind]
        {
            get
            {
                return subordinates[ind].Pilot;
            }
        }
       
        #region IEnumerable Members

        /// <summary>
        /// IEnumerable inherited members
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            ((IEnumerator)this).Reset();
            return (IEnumerator)this;
        }

        #endregion

        #region IEnumerator Members
        int pos=-1;
        object IEnumerator.Current
        {
            get { return subordinates[pos].Pilot; }
        }

        bool IEnumerator.MoveNext()
        {
            pos++;
            return (pos < subordinates.Count);
        }

        void IEnumerator.Reset()
        {
            pos = -1;
        }

        #endregion
    }
    /// <summary>
    /// units are positioned in one line
    /// </summary>
    class LineFormation : Formation
    {
        float distBetweenUnits;
        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="Leader">formation leader</param>
        /// <param name="Subordinates">list of units</param>
        /// <param name="DistBetweenUnits">distance between neirbours</param>
        public LineFormation(UnitPilot Leader, List<UnitPilot> Subordinates, float DistBetweenUnits)
            : base(Leader, Subordinates)
        {
            distBetweenUnits = DistBetweenUnits;
            Line line = DefineLineFormation(rotationAngle, GetMassCenter());
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="Leader">formation leader</param>
        /// <param name="Subordinates">list of units</param>
        /// <param name="DistBetweenUnits">distance between neirbours</param>
        /// <param name="RotationAngle">forward angle in radians</param>
        public LineFormation(UnitPilot Leader, List<UnitPilot> Subordinates, float DistBetweenUnits, float RotationAngle)
            : base(Leader, Subordinates)
        {
            distBetweenUnits = DistBetweenUnits;
            rotationAngle = RotationAngle;
            CreatePositions();
        }
        public override void CreatePositions()
        {
            Line line = DefineLineFormation(rotationAngle, GetMassCenter());
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
        private void CreateLinePositions(Line FormationPosition)
        {
            GameVector PositionIncrement = (FormationPosition.pt2 - FormationPosition.pt1) * (1 / (float)(Count - 1));
            if (Count <= 1) PositionIncrement = GameVector.Zero;
            for (int i = 0; i < Count; i++)
            {
                subordinates[i].Position = new RelativeVector(FormationPosition.pt1 + PositionIncrement * i);
            }
        }
        private Line DefineLineFormation(float RotationAngle, GameVector center)
        {
            float length = distBetweenUnits * (subordinates.Count - 1);
            GameVector toVertexDir = GameVector.UnitX.Rotate(RotationAngle + AngleClass.pi * 0.5f) * 0.5f * length;
            return new Line(center + toVertexDir, center - toVertexDir);
        }
        public override void SetAngle(float newAngle)
        {
            rotationAngle = newAngle;
            Line line = DefineLineFormation(rotationAngle, GetMassCenter());
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
        public override FormationTypes Type
        {
            get { return FormationTypes.Line; }
        }
        public override void CreatePositions(GameVector CenterPosition, float Angle)
        {
            rotationAngle = Angle;
            Line line = DefineLineFormation(rotationAngle, CenterPosition);
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
    }
    /// <summary>
    /// units are positioned in stagger with one unit width
    /// </summary>
    class StaggerFormation : Formation
    {
        float widthBetweenUnits;
        float heightBetweenUnits;
        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="Leader">formation leader</param>
        /// <param name="Subordinates">list of units</param>
        /// <param name="WidthBetweenUnits">width between neirbours</param>
        /// <param name="HeightBetweenUnits">depth between neirbours</param>
        public StaggerFormation(UnitPilot Leader, List<UnitPilot> Subordinates, float WidthBetweenUnits, float HeightBetweenUnits)
            : base(Leader, Subordinates)
        {
            widthBetweenUnits = WidthBetweenUnits;
            heightBetweenUnits = HeightBetweenUnits;

            CreatePositions();
        }
        public override void CreatePositions()
        {
            CreatePositions(GetMassCenter(), rotationAngle);
        }
        public override void SetAngle(float newAngle)
        {
            rotationAngle = newAngle;
            CreatePositions();
        }
        public override FormationTypes Type
        {
            get { return FormationTypes.Stagger; }
        }
        public override void CreatePositions(GameVector CenterPosition, float Angle)
        {
            if (subordinates.Count > 0)
            {
                rotationAngle = Angle;
                GameVector center = CenterPosition;
                float FormationLength = heightBetweenUnits * subordinates.Count / 2;
                GameVector forward = GameVector.UnitX.Rotate(Angle);
                GameVector right = forward.Rotate(AngleClass.pi * 0.5f);
                int PositionInd;

                subordinates[0].Position = new RelativeVector(center + forward * FormationLength * 0.5f);
                for (int currPilot = 1; currPilot < subordinates.Count; currPilot += 2)
                {
                    PositionInd = currPilot / 2 + 1;
                    subordinates[currPilot].Position = new RelativeVector(center + forward * (FormationLength * 0.5f - PositionInd * heightBetweenUnits)
                        + right * (PositionInd * widthBetweenUnits));

                    if (currPilot + 1 < subordinates.Count)
                        subordinates[currPilot + 1].Position = new RelativeVector(center + forward * (FormationLength * 0.5f - PositionInd * heightBetweenUnits)
                        - right * (PositionInd * widthBetweenUnits));
                }
            }
        }
    }
    /// <summary>
    /// units are positioned in bar with specified width and depth
    /// </summary>
    class BarFormation : Formation
    {
        float widthBetweenUnits;
        float heightBetweenUnits;
        int cUnitsInLine;
        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="Leader">formation leader</param>
        /// <param name="Subordinates">list of units</param>
        /// <param name="CUnitsInLine"></param>
        /// <param name="WidthBetweenUnits"></param>
        /// <param name="HeightBetweenUnits"></param>
        /// <param name="RotationAngle"></param>
        public BarFormation(UnitPilot Leader, List<UnitPilot> Subordinates, int CUnitsInLine, float WidthBetweenUnits, float HeightBetweenUnits, float RotationAngle)
            : base(Leader, Subordinates)
        {
            cUnitsInLine = CUnitsInLine;
            rotationAngle = RotationAngle;
            widthBetweenUnits = WidthBetweenUnits;
            heightBetweenUnits = HeightBetweenUnits;

            CreatePositions();

        }
        public override void CreatePositions()
        {
            if (subordinates.Count > 0)
            {
                GameVector center = GetMassCenter();
                DefineLinesFormation();
                int CLines = (subordinates.Count - 1) / cUnitsInLine + 1;
                int ind = 0;
                for (int currLine = 0; currLine < CLines; currLine++)
                {
                    for (int currColumn = 0; currColumn < cUnitsInLine && ind < subordinates.Count; currColumn++)
                    {
                        subordinates[ind].Position =
                            new RelativeVector(center -
                            LinesPosDifference * (currLine - CLines * 0.5f + 0.5f) +
                            NearUnitsPosDifference * (currColumn - cUnitsInLine * 0.5f + 0.5f));
                        ind++;
                    }
                }
                SortUnitsByNearanceToPositions();
            }
        }
        GameVector NearUnitsPosDifference;
        GameVector LinesPosDifference;
        private void DefineLinesFormation()
        {
            DefineLinesFormation(rotationAngle);
        }
        private void DefineLinesFormation(float Angle)
        {
            LinesPosDifference = GameVector.UnitX.Rotate(Angle);
            NearUnitsPosDifference = LinesPosDifference.Rotate(AngleClass.pi * 0.5f);
            LinesPosDifference *= widthBetweenUnits;
            NearUnitsPosDifference *= heightBetweenUnits;
        }
        public override void SetAngle(float newAngle)
        {
            rotationAngle = newAngle;
            CreatePositions();
        }
        public override FormationTypes Type
        {
            get { return FormationTypes.Bar; }
        }
        public override void CreatePositions(GameVector CenterPosition, float Angle)
        {
            if (subordinates.Count > 0)
            {
                rotationAngle = Angle;
                GameVector center = CenterPosition;
                DefineLinesFormation(Angle);
                int CLines = (subordinates.Count - 1) / cUnitsInLine + 1;
                int ind = 0;
                for (int currLine = 0; currLine < CLines; currLine++)
                {
                    for (int currColumn = 0; currColumn < cUnitsInLine && ind < subordinates.Count; currColumn++)
                    {
                        subordinates[ind].Position =
                            new RelativeVector(center -
                            LinesPosDifference * (currLine - CLines * 0.5f + 0.5f) +
                            NearUnitsPosDifference * (currColumn - cUnitsInLine * 0.5f + 0.5f));
                        ind++;
                    }
                }
                SortUnitsByNearanceToPositions();
            }
        }
    }
    /// <summary>
    /// units are not strongly positioned, they are flying as swarm
    /// </summary>
    class SwarmFormation : Formation
    {
        /// <summary>
        /// creates new instance
        /// </summary>
        /// <param name="subordinates">list of units</param>
        public SwarmFormation(List<UnitPilot> subordinates)
            : base(null, subordinates)
        {
        }
        public override void SetAngle(float newAngle)
        {
            //nothing to do
        }
        public override FormationTypes Type
        {
            get { return FormationTypes.Swarm; }
        }
        public override void CreatePositions()
        {
            //nothing to do
        }
        public override void CreatePositions(GameVector CenterPosition, float Angle)
        {
            //nothing to do
        }
    }
    //class CustomFormation : Formation
    //{
    //    /// <summary>
    //    /// Creates custom formation.
    //    /// Units are ardered as they are ordered just before formation creation
    //    /// </summary>
    //    /// <param name="subordinates"></param>
    //    /// <param name="AngleForward">this angle defines forward direction of formation</param>
    //    public CustomFormation(List<UnitPilot> subordinates,float AngleForward)
    //        : base(null, subordinates)
    //    {
    //        SavePositions(AngleForward);
    //        CreatePositions();
    //    }
    //    List<GameVector> PositionsArray;
    //    private void SavePositions(float AngleForward)
    //    {
    //        PositionsArray = new List<GameVector>();
    //        throw new NotImplementedException();
    //    }
    //    public override void SetAngle(float newAngle)
    //    {
    //        rotationAngle = newAngle;
    //        CreatePositions();
    //    }
    //    public override FormationTypes Type
    //    {
    //        get { return FormationTypes.Custom; }
    //    }
    //    public override void CreatePositions()
    //    {
    //        CreatePositions(GetMassCenter(), rotationAngle);
    //    }
    //    public override void CreatePositions(GameVector CenterPosition, float Angle)
    //    {
    //        rotationAngle = Angle;
    //        Update();
    //        GameVector currCenter = GetMassCenter();
    //        List<RelativeVector> newPositions = new List<RelativeVector>();
    //        for (int i = 0; i < subordinates.Count; i++)
    //        {
    //            PilotPositionPair pilotPos = subordinates[i];
    //            float angle, dist;
    //            dist = GameVector.Distance(Leader.ControlledUnit.Position, pilotPos.Position.Value);
    //            angle = Leader.ControlledUnit.AngleTo(pilotPos.Position.Value);
    //            pilotPos.Position = new RelativeVector(Leader.ControlledUnit, angle, dist);
    //            //newPositions.Add(new RelativeVector(Leader.ControlledUnit, angle, dist));
    //        }
    //    }
    //}
}
