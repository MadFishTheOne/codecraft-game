using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using System.Collections;
namespace AINamespace
{
    /// <summary>
    /// class for controlling list of units as one squadron
    /// </summary>
    public class SquadronColonel:IEnumerable
    {
        /// <summary>
        /// enumeraion of squadron states
        /// </summary>
        enum States
        {
            Gathering,//pilots are flying to their positions
            GatheringAtSpecifiedPosAngle,//gathering to a specified position and angle
            Rotating,//pilots on their positions are rotating to forward position            
            Flying,//flying to tgt
            Standing,//nothing
            Fighting,//fighting as turrets
            Nothing//null state
        }
        /// <summary>
        /// enumeration of squadron behaviours 
        /// </summary>
        public enum Behaviours
        {
            /// <summary>
            /// rotates formation to tgt and then flying forward to tgt
            /// </summary>
            FlyingToTgt,
            /// <summary>
            /// fying to tgt and then defending there
            /// </summary>
            Attacking,
            /// <summary>
            /// staying and shooting
            /// </summary>
            Defending,
            /// <summary>
            /// doing nothing
            /// </summary>
            Waiting,
            /// <summary>
            /// standing in order in specified place with specified angle
            /// </summary>
            Ordering
        }
        
        States state,neededState;
        /// <summary>
        /// current behaviour of squadron
        /// </summary>
        public Behaviours behaviour{get;private set;}
        Timer stateChangingTimer, reSortingTimer;
        /// <summary>
        /// squadron units container
        /// </summary>
        public Formation formation { get; private set; }
        GameVector flyingPos;
        bool isOrdered;
        /// <summary>
        /// true if formation is ordered,
        /// false if formation is swarm
        /// </summary>
        public bool IsOrdered
        {
            get
            {
                return isOrdered;
            }
            set
            {
                isOrdered = value;                
            }
        }
        /// <summary>
        /// create new squadron
        /// </summary>
        /// <param name="Colonel">unit that will be formation leader</param>
        /// <param name="game">game</param>
        /// <param name="FormationType">formation type to set</param>
        public SquadronColonel(UnitPilot Colonel,IGame game,FormationTypes FormationType)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            stateChangingTimer = new Timer(0.5f, game);
            behaviour = Behaviours.Waiting;
            reSortingTimer = new Timer(2, game);

            List<UnitPilot>  pilots = new List<UnitPilot>();
            switch (FormationType)
            {
                case FormationTypes.Line: formation = new LineFormation(Colonel, pilots, 100, AngleClass.pi * 0.0f); break;
                case FormationTypes.Bar: formation = new BarFormation(Colonel, pilots, 5, 200, 150, 0); break;
                case FormationTypes.Stagger: formation = new StaggerFormation(Colonel, pilots, 90, 120); break;
            }            
        }
        /// <summary>
        /// create new squadron
        /// </summary>
        /// <param name="Colonel">unit that will be formation leader</param>
        /// <param name="game">game</param>
        public SquadronColonel(UnitPilot Colonel, IGame game)
        {
            isOrdered = false;
            state = States.Nothing;
            neededState = States.Nothing;
            stateChangingTimer = new Timer(0.5f, game);
            behaviour = Behaviours.Waiting;
            reSortingTimer = new Timer(2, game);

            List<UnitPilot> pilots = new List<UnitPilot>();
            formation = new SwarmFormation(pilots);            
        }
        /// <summary>
        /// sets a new formation type
        /// </summary>
        /// <param name="NewType">new formation type</param>
        public void SetFormationType(FormationTypes NewType)
        {
            isOrdered = NewType!= FormationTypes.Swarm;
            state = States.Nothing;
            neededState = States.Nothing;            
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots =new List<UnitPilot>();
            foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
            {                pilots.Add(pilotpos.Pilot);            }
            switch (NewType)
            {
                case FormationTypes.Line: formation = new LineFormation(Colonel, pilots, 100, AngleClass.pi * 0.0f); break;
                case FormationTypes.Bar: formation = new BarFormation(Colonel, pilots, 5, 200, 150, 0); break;
                case FormationTypes.Stagger: formation = new StaggerFormation(Colonel, pilots, 90, 120); break;
                case FormationTypes.Swarm: formation = new SwarmFormation(pilots); break;
            }            
        }
        /// <summary>
        /// Sets formation type to parametrized stagger
        /// </summary>
        /// <param name="WidthBetweenUnits">Width between two neirbours</param>
        /// <param name="DepthBetweenUnits">Depth between two neirbours</param>
        /// <param name="Angle">Rotation angle</param>
        public void SetFormationTypeStagger(float WidthBetweenUnits, float DepthBetweenUnits,float Angle)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots = new List<UnitPilot>();
            foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
            { pilots.Add(pilotpos.Pilot); }
            formation = new StaggerFormation(Colonel, pilots,WidthBetweenUnits, DepthBetweenUnits);
            formation.SetAngle(Angle);
        }
        /// <summary>
        /// Sets formation type to parametrized bar
        /// </summary>
        /// <param name="CUnitsInLine"></param>
        /// <param name="WidthBetweenUnits">Width between two neirbours</param>
        /// <param name="DepthBetweenUnits">Depth between two neirbours</param>
        /// <param name="Angle">Rotation angle</param>
        public void SetFormationTypeBar(int CUnitsInLine,float WidthBetweenUnits, float DepthBetweenUnits,float Angle)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots = new List<UnitPilot>();
            foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
            { pilots.Add(pilotpos.Pilot); }
            formation = new BarFormation(Colonel, pilots, CUnitsInLine, WidthBetweenUnits, DepthBetweenUnits,Angle);
        }
        /// <summary>
        /// Sets formation type to parametrized line
        /// </summary>
        /// <param name="WidthBetweenUnits">Width between two neirbours</param>
        /// <param name="Angle">Rotation angle</param>
        public void SetFormationTypeLine(float WidthBetweenUnits,float Angle)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots = new List<UnitPilot>();
            foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
            { pilots.Add(pilotpos.Pilot); }
            formation = new LineFormation(Colonel, pilots, WidthBetweenUnits, Angle);
        }
        private void UpdateOrdered()        
        {
            
             //AI.game.GeometryViewer.DrawPoint(flyingPos, Color.Green);
        //     AI.game.GeometryViewer.DrawPoint(formation.GetMassCenter(), Color.Blue);
             
            if (stateChangingTimer.TimeElapsed)
            {
                stateChangingTimer.Reset();
                float newAngle = (flyingPos - formation.GetMassCenter()).Angle();
                
                switch (behaviour)
                {
                    case Behaviours.Ordering:
                        if (formation.PositionHoldingMistake() < 0.05f)
                        {
                            if (formation.AngleHoldingMistake > 0.1f)
                                neededState = States.Rotating;
                            else
                            {
                                behaviour = Behaviours.Waiting;
                                //neededState = States.Fighting;
                            }
                        }
                        else
                        { neededState = States.GatheringAtSpecifiedPosAngle; }
                        break;
                    case Behaviours.Waiting:

                        //if (AngleClass.Distance(newAngle, formation.Angle) > AngleClass.pi * 0.20f)
                        //{
                        //    formation.SetAngle(newAngle);
                        //}
                        neededState = States.Standing;
                        if (formation.PositionHoldingMistake() < 0.05f)
                        { neededState = States.Rotating; }
                        else
                        { neededState = States.Gathering; }
                        

                        break;
                    case Behaviours.Defending:
                        neededState = States.Fighting;
                        break;
                    case Behaviours.FlyingToTgt:
                        if (AngleClass.Distance(newAngle, formation.Angle) > AngleClass.pi * 0.20f)
                            formation.SetAngle(newAngle);
                        if (
                            ((state== States.Gathering)&&(formation.PositionHoldingMistake() > 0.1f))//gather to err<=0.1
                            ||
                            ((state != States.Gathering) && (formation.PositionHoldingMistake() > 1.5f))//gather if err>1.5
                            )
                        { neededState = States.Gathering; }
                        else
                        {
                            if (formation.AngleHoldingMistake > 0.10f)
                            { neededState = States.Rotating; }
                            else { neededState = States.Flying; }
                        }
                        if (ReceivedToFlyingTgt)
                        {
                            neededState = States.Standing;
                        }
                        break;
                    case Behaviours.Attacking:                       

                        if (AngleClass.Distance(newAngle, formation.Angle) > AngleClass.pi * 0.20f)
                            formation.SetAngle(newAngle);
                        if (
                           ((state == States.Gathering) && (formation.PositionHoldingMistake() > 0.1f))//gather to err<=0.1
                           ||
                           ((state != States.Gathering) && (formation.PositionHoldingMistake() > 1.5f))//gather if err>1.5
                           )
                        { neededState = States.Gathering; }
                        else
                        {
                            if (formation.AngleHoldingMistake > 0.10f)
                            { neededState = States.Rotating; }
                            else { neededState = States.Flying; }
                        }
                        if (ReceivedToFlyingTgt)
                        {
                            neededState = States.Standing;
                        }

                    
                        if (formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius*0.5f+formation.Leader.computer.StopDistance))
                        {
                            neededState = States.Fighting;
                        }
                        break;
                }
            }
            
            //debug
            if (formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius))
            {
                neededState = States.Fighting;
            }

            switch (neededState)
            {
                case States.Gathering:
                    if (state != States.Gathering)
                    { 
                        Gather(formation.GetMassCenter(),formation.Angle); 
                        state = States.Gathering; 
                    }
                    else
                    {
                        if (reSortingTimer.TimeElapsed)
                        {
                            reSortingTimer.Reset();
                            
                            formation.SortUnitsByNearanceToPositions();
                            
                            //if (formation.PositionHoldingMistake < 0.05f)
                            //{
                            //    neededState = States.Rotating;
                            //}
                        }
                    }
                    break;
                case States.GatheringAtSpecifiedPosAngle:
                    if (state != States.GatheringAtSpecifiedPosAngle)
                    {
                        Gather(GatherPosition, GatherAngle);
                        state = States.GatheringAtSpecifiedPosAngle;
                    }
                    else
                    {
                        if (reSortingTimer.TimeElapsed)
                        {
                            reSortingTimer.Reset();

                            formation.SortUnitsByNearanceToPositions();
                             
                            //if (formation.PositionHoldingMistake < 0.05f)
                            //{
                            //    neededState = States.Rotating;
                            //}
                        }
                    }
                    break;
                case States.Rotating:
                    if (state != States.Rotating)
                    {
                        formation.MakePositionsRelative();
                        RotateOrder(formation.Angle);
                        state = States.Rotating;
                    }
                    else
                    {
                        if (stateChangingTimer.TimeElapsed)
                        {
                            stateChangingTimer.Reset();
                            RotateOrder(formation.Angle);
                            //if (formation.AngleHoldingMistake < 0.2f)
                            //{
                            //    neededState = States.Flying;
                            //}
                        }
                    }
                    break;
                case States.Flying:
                    if (state != States.Flying)
                    {
                        Fly();
                        state = States.Flying;
                    }                    
                    break;
                case States.Fighting:
                    if (state != States.Fighting)
                    {
                        //Fight();
                        foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
                            pilotpos.Pilot.AttackClosest();
                        state = States.Fighting;
                    }
                    break;
                case States.Standing:
                    if (state != States.Standing)
                    {
                        state = States.Standing;
                        foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
                            pilotpos.Pilot.HoldPosition();
                    }
                    break;
                default :
                    break;
            }
                    stateChangingTimer.Update();
                    reSortingTimer.Update();

                    formation.Update();
            //DEBUG
                    //formation.DrawPositions();
                    //formation.DrawDestinations();


        }
        private void UpdateSwarm()
        {
            if (stateChangingTimer.TimeElapsed)
            {
                stateChangingTimer.Reset();                

                switch (behaviour)
                {
                    case Behaviours.Waiting:
                        neededState = States.Standing;
                        break;
                    case Behaviours.Defending:
                        neededState = States.Standing;
                        break;
                    case Behaviours.FlyingToTgt:    
                           neededState = States.Flying;
                           if (ReceivedToFlyingTgt)
                           { neededState = States.Standing; }
                        break;
                    case Behaviours.Attacking:
                        neededState = States.Flying;
                        if (formation.Count > 0 && formation.CloseEnemyExists(formation.subordinates[0].Pilot.ControlledUnit.ShootingRadius * 0.5f + formation.subordinates[0].Pilot.computer.StopDistance))
                        { neededState = States.Fighting; }
                        if (ReceivedToFlyingTgt)
                        { neededState = States.Standing; }
                        break;
                }
            }

            switch (neededState)
            {              
                case States.Flying:
                    if (state != States.Flying)
                    {
                        FlySwarm();
                        state = States.Flying;
                    }
                    break;
                case States.Fighting:
                    if (state != States.Fighting)
                    {
                        FightSwarm();
                        state = States.Fighting;
                    }
                    break;
                case States.Standing:
                    if (state != States.Standing)
                    {
                        HoldPosition();
                        state = States.Standing;
                    }
                    break;
                default:
                    break;
            }
            stateChangingTimer.Update();
            reSortingTimer.Update();

            formation.Update();
        }

        private void FlySwarm()
        {
            //GameVector center = formation.GetMassCenter();
            //foreach (Formation.PilotPositionPair pilotPos in formation)
            //{
            //    pilotPos.Pilot.GoTo(new RelativeVector(flyingPos+pilotPos.Pilot.ControlledUnit.Position-center));
            //}
            
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.GoTo(new RelativeVector(flyingPos));
            }
        }

        private void FightSwarm()
        {
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.AttackClosest();
            }
        }
        internal void Update()
        {
            if (IsOrdered) UpdateOrdered();
            else UpdateSwarm();
        }

        private void Fight()
        {
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.HoldPosition();
            }
        }
        private void HoldPosition()
        {
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.HoldPosition();
            }
        }

        private void Gather(GameVector position, float Angle)
        {

            //formation = new BarFormation(pilots[0], pilots, 5, 200, 150, (flyingPos - formation.GetMassCenter()).Angle());
            //formation.SetAngle((flyingPos - formation.GetMassCenter()).Angle());
            int ind = 0;
            formation.CreatePositions(position, Angle);
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.GoTo(formation.GetPosition(ind));
                ind++;
            }
        }
        float GatherAngle;
        GameVector GatherPosition;
        #region orders
        
        /// <summary>
        /// order to create units order -
        /// to create unit order in specified place with specified angle
        /// </summary>
        /// <param name="position">position of the new order</param>
        /// <param name="Angle">angle of the new order</param>            
        public void CreateUnitsOrderOrder(GameVector position,float Angle)
        {
            GatherAngle = Angle;
            GatherPosition = position;
            state = States.Standing;
            behaviour = Behaviours.Ordering;           
            formation.CreatePositions(position, Angle);              
        }
       
        /// <summary>
        /// orderes squadron to fly to specified position
        /// </summary>
        /// <param name="FlyingPos">position to fly to</param>
        public void GoToOrder(GameVector FlyingPos)
        {
            state = States.Standing;
            behaviour = Behaviours.FlyingToTgt;
            flyingPos = FlyingPos;                        
        }
        /// <summary>
        /// orderes squadron to attack specified position
        /// </summary>
        /// <param name="FlyingPos">position to attack</param>
        public void AttackOrder(GameVector FlyingPos)
        {
            state = States.Standing;
            behaviour = Behaviours.Attacking;
            flyingPos = FlyingPos;
        }
        /// <summary>
        /// true if squadron's mass center is near it's flying target.
        /// </summary>
        public bool ReceivedToFlyingTgt
        {
            get
            {
                return GameVector.Distance(formation.GetMassCenter(), flyingPos) < 100;
            }
        }   
        /// <summary>
        /// flies to flyingPos
        /// </summary>
        private void Fly()
        {
            //flyingPos = FlyingPos;
            //find position of leader if formation center is in the FlyingPos
            GameVector leaderFlyingPos = flyingPos + formation.Leader.ControlledUnit.Position - formation.GetMassCenter();
            //make it forward
            GameVector FlyingDir = GameVector.UnitX.Rotate(formation.Angle);
            leaderFlyingPos = formation.Leader.ControlledUnit.Position +
                FlyingDir * GameVector.Dot(leaderFlyingPos - formation.Leader.ControlledUnit.Position, FlyingDir);
            formation.Leader.GoTo(new RelativeVector(leaderFlyingPos));
            formation.Leader.WeakAvoidingCollizions = true;
            int ind = 0;
            UnitPilot.MaintainingOrderData orderData = new UnitPilot.MaintainingOrderData(formation.Leader, formation.Angle, 40);
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                if (pilotPos.Pilot != formation.Leader)
                {
                    orderData.OrderPosition = formation.GetPosition(ind);
                    pilotPos.Pilot.Marshal(orderData);
                    //pilot.GoTo(formation.GetPosition(ind));
                }
                ind++;
            }
        }

        private void RotateOrder(float rotationAngle)
        {
          
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.StandBy(rotationAngle);
            }
        }
        /// <summary>
        /// adds a unit to squadron
        /// </summary>
        /// <param name="unitPilot">unit to add</param>
        public void AddUnit(UnitPilot unitPilot)
        {
formation.Add(unitPilot);
        }
        /// <summary>
        /// removes unit from a squadron
        /// </summary>
        /// <param name="unitPilot">unit to remove</param>
        public void RemoveUnit(UnitPilot unitPilot)
        {
           formation.Remove(unitPilot);
        }
        #endregion        
    
        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return formation.GetEnumerator();
        }

        #endregion
    }
}
