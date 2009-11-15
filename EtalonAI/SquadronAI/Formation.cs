using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using System.Collections;
namespace AINamespace
{
    public enum FormationTypes
    {
        Line,Stagger,Bar,Swarm
    }
    public abstract class Formation:IEnumerable
    {
        protected UnitPilot leader;//leader is in subordinates too
        
        public class PilotPositionPair
        {
            public UnitPilot Pilot;
            public RelativeVector Position;
            public PilotPositionPair(UnitPilot Pilot, RelativeVector Position)
            {
                this.Pilot = Pilot;
                this.Position = Position;
            }

        }
        public int Count
        {
            get { return subordinates.Count; }
        }
        protected List<PilotPositionPair> subordinates;
        //protected List<UnitPilot> subordinates;
        //protected List<RelativeVector> positions;

        
        protected float rotationAngle;
        //positions of units in formation
        
        public Formation(UnitPilot Leader, List<UnitPilot> Subordinates)
        {
            leader = Leader;
            subordinates = new List<PilotPositionPair>();
            foreach (UnitPilot pilot in Subordinates)
            {
                subordinates.Add(new PilotPositionPair(pilot, new RelativeVector(GameVector.Zero)));
            }
        
        }
        public void Update()
        {
            for (int i = 0; i < subordinates.Count; i++)
            {
                //subordinates[i].Pilot.ControlledUnit.Text = subordinates[i].Pilot.ControlledUnit.Name;
                if (subordinates[i].Pilot.ControlledUnit.Dead)
                {
                    subordinates.RemoveAt(i);
                    i--;
                }
            }
        }
        public RelativeVector GetPosition(int Index)
        {
            return subordinates[Index].Position;
        }
        public float Angle
        { get { return rotationAngle; } }
        public float RotationAngle
        { get { return rotationAngle; } }
        //0 if ideal positioning, 1 if average mistake=100
        public float PositionHoldingMistake()
        {
            
                float averageMistake = 0;
                int ind = 0;
                float maxMistake = 0;
                float currMistake;
                foreach (PilotPositionPair pilotPos in subordinates)
                {
                    currMistake=GameVector.DistanceSquared(pilotPos.Pilot.ControlledUnit.Position,pilotPos.Position.Value) / 10000;
                    if (currMistake > maxMistake) { maxMistake = currMistake; }

                    averageMistake += currMistake;
                    ind++;
                }
            averageMistake/=subordinates.Count;
            return maxMistake;//mistake ;
            
        }
        //0 if ideal positioning, 1 if average mistake=pi/2.     no! let it be max mistake
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
        public UnitPilot WorstPilot()
        {
            float biggestMistake = 0;
            UnitPilot worstPilot = null;
            float currmistake;
            int ind=0;
            foreach (PilotPositionPair pilotPos in subordinates)
            {
                currmistake=GameVector.DistanceSquared(pilotPos.Pilot.ControlledUnit.Position, GetPosition(ind).Value) ;
                if (biggestMistake<currmistake)
                {
                    biggestMistake=currmistake;
                    worstPilot = pilotPos.Pilot;
                }                 
            }
            return worstPilot;
        }
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
        public void SortUnitsByNearanceToPositions()
        {
            int CPts=subordinates.Count;
            int[] PilotIndByPtInd = new int[CPts];
            int[] PtIndByPilotInd = new int[CPts];
            for (int i = 0; i < CPts; i++)
            {
                PilotIndByPtInd[i] = -1;//no pilot
                PtIndByPilotInd[i]=-1;//no point
            }
            bool complete=false;
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
                for (int currPos1=0;currPos1<subordinates.Count;currPos1++)
                    for (int currPos2 = 0; currPos2 < subordinates.Count; currPos2++)
                        if (currPos1!=currPos2)
                    {
                            int Pilot1Ind=PilotIndByPtInd[currPos1];
                            int Pilot2Ind=PilotIndByPtInd[currPos2];
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

            List<PilotPositionPair> newSubordinates = new List<PilotPositionPair>();
            for (int i = 0; i < subordinates.Count; i++)
            {
                PilotPositionPair pilotPos = new PilotPositionPair(subordinates[PilotIndByPtInd[i]].Pilot, subordinates[i].Position);
                newSubordinates.Add(pilotPos);
                //newSubordinates.Add(subordinates[PilotIndByPtInd[i]]);
            }
            subordinates.Clear();
            subordinates = newSubordinates;
        }
        public void MakePositionsRelative()
        {
            List<RelativeVector> newPositions = new List<RelativeVector>();
            for (int i=0;i<subordinates.Count;i++)
            {
                PilotPositionPair pilotPos = subordinates[i];            
                float angle, dist;
                dist = GameVector.Distance(Leader.ControlledUnit.Position,pilotPos.Position.Value);
                angle = Leader.ControlledUnit.AngleTo(pilotPos.Position.Value);
                pilotPos.Position = new RelativeVector(Leader.ControlledUnit, angle, dist);
                //newPositions.Add(new RelativeVector(Leader.ControlledUnit, angle, dist));
            }
            //positions = newPositions;
        }
        public GameVector GetMassCenter()
        {
            GameVector center = GameVector.Zero;
            foreach (PilotPositionPair pilotPos in subordinates)
            {
                center +=pilotPos.Pilot.ControlledUnit.Position;
            }
            center /= (float)subordinates.Count;
            return center;
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return subordinates.GetEnumerator();
        }

        #endregion
        public void DrawPositions()
        {
            for (int i = 0; i < subordinates.Count; i++)
                AI.game.GeometryViewer.DrawPoint(GetPosition(i).Value, Color.Green);
        }
        public void DrawDestinations()
        {
            for (int i = 0; i < subordinates.Count; i++)
            {
                AI.game.GeometryViewer.DrawLine(new Line(subordinates[i].Pilot.ControlledUnit.Position, GetPosition(i).Value), Color.Green);
            }
        }

        public abstract void SetAngle(float newAngle);
        public abstract FormationTypes Type { get; }
        public abstract void CreatePositions();



        internal void Add(UnitPilot unitPilot)
        {
            if (leader == null) leader = unitPilot;
            subordinates.Add(new PilotPositionPair( unitPilot,new RelativeVector( unitPilot.ControlledUnit.Position)));
        }

        internal void Remove(UnitPilot unitPilot)
        {
            for (int i = 0; i < subordinates.Count; i++)
                if (unitPilot.Equals(subordinates[i].Pilot))
                {
                    subordinates.RemoveAt(i);
                    break;
                }
        }
        public bool CloseEnemyExists(float CloseDist)
        {
                foreach (PilotPositionPair PilotPos in subordinates)
                {
                    if (PilotPos.Pilot.GetClosestEnemyDist() < CloseDist)
                        return true;
                }
                return false;
            
        }
    }
    class LineFormation:Formation
    {
        float distBetweenUnits;
        public LineFormation(UnitPilot Leader, List<UnitPilot> Subordinates, float DistBetweenUnits)
            : base(Leader, Subordinates)
        {
            distBetweenUnits = DistBetweenUnits;
            Line line = DefineLineFormation( rotationAngle);
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
        public LineFormation(UnitPilot Leader, List<UnitPilot> Subordinates, float DistBetweenUnits,float RotationAngle)
            : base(Leader, Subordinates)
        {
            distBetweenUnits = DistBetweenUnits;
            rotationAngle = RotationAngle;
            CreatePositions();
        }
        public override void CreatePositions()
        {
            Line line = DefineLineFormation(rotationAngle);
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
        private void CreateLinePositions(Line FormationPosition)
        {
            GameVector PositionIncrement=(FormationPosition.pt2-FormationPosition.pt1)*(1/(float)(subordinates.Count-1));            
            for (int i = 0; i < subordinates.Count; i++)
            {
               subordinates[i].Position=new RelativeVector(FormationPosition.pt1 + PositionIncrement * i);
            }
        }
        private Line DefineLineFormation( float RotationAngle)
        {
            GameVector center = GetMassCenter();
            float length = distBetweenUnits * (subordinates.Count-1);
            GameVector toVertexDir = GameVector.UnitX.Rotate(RotationAngle+AngleClass.pi*0.5f)*0.5f*length;
            return new Line(center + toVertexDir, center - toVertexDir);
        }
        public override void SetAngle(float newAngle)
        {
            rotationAngle = newAngle;
            Line line = DefineLineFormation(rotationAngle);
            CreateLinePositions(line);
            SortUnitsByNearanceToPositions();
        }
        public override FormationTypes Type
        {
            get { return FormationTypes.Line; }
        }
    }
    class StaggerFormation : Formation
    {
        float widthBetweenUnits;
        float heightBetweenUnits;
        public StaggerFormation(UnitPilot Leader, List<UnitPilot> Subordinates, float WidthBetweenUnits, float HeightBetweenUnits)
            :base(Leader,Subordinates)
        {
            widthBetweenUnits = WidthBetweenUnits;
            heightBetweenUnits = HeightBetweenUnits;

            CreatePositions();
        }
        public override void  CreatePositions()
        {
            if (subordinates.Count > 0)
            {
                GameVector center = GetMassCenter();
                float FormationLength = heightBetweenUnits * subordinates.Count / 2;
                GameVector forward = GameVector.UnitX.Rotate(rotationAngle);
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
        public override void SetAngle(float newAngle)
        {
            rotationAngle = newAngle;
            CreatePositions();
        }
        public override FormationTypes Type
        {
            get { return FormationTypes.Stagger; }
        }
    }
    class BarFormation : Formation
    {
        float widthBetweenUnits;
        float heightBetweenUnits; 
        int cUnitsInLine;
        public BarFormation(UnitPilot Leader,List<UnitPilot> Subordinates, int CUnitsInLine, float WidthBetweenUnits, float HeightBetweenUnits, float RotationAngle)
            :base(Leader,Subordinates)
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
            LinesPosDifference = GameVector.UnitX.Rotate(rotationAngle);
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
    }
    class SwarmFormation : Formation
    {
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
    }
}
