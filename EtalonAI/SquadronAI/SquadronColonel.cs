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
    public class Squadron:IEnumerable
    {
        /// <summary>
        /// enumeraion of squadron states
        /// </summary>
        public enum States
        {
            /// <summary>
            /// Pilots are flying to their positions
            /// </summary>
            Gathering,
            /// <summary>
            /// Gathering to a specified position and angle
            /// </summary>
            GatheringAtSpecifiedPosAngle,
            /// <summary>
            /// Pilots are on their positions and rotating to forward angle
            /// </summary>
            Rotating,
            /// <summary>
            /// Pilots are flying to tgt            
            /// </summary>
            Flying,
            /// <summary>
            /// Pilots are attacking closest
            /// </summary>
            Fighting,
            /// <summary>
            /// Pilots are holding their positions. 
            /// They are fighting as turrets - standing and shooting if they can
            /// </summary>
            Defending,
            /// <summary>
            /// Null state. 
            /// Units are not receiving any orders from squadron. They can be controlled separately
            /// </summary>
            Nothing
        }
        /// <summary>
        /// enumeration of squadron behaviours 
        /// </summary>
        public enum Behaviours
        {
            /// <summary>
            /// Rotates formation to tgt and then flying forward to tgt. Does not attack units during flying
            /// </summary>
            FlyingToTgt,
            /// <summary>
            /// Units are flying to target place. If there are any enemy forces on the way, attacks them
            /// </summary>
            Attacking,
            /// <summary>
            /// Staying and shooting
            /// </summary>
            HoldingPosition,
            /// <summary>
            /// Units are standing. If there are any enemy forces in shooting radius, attacks enemies
            /// </summary>
            Waiting,
            /// <summary>
            /// Units are flying to take place in order. Order is situated in specified place with specified angle 
            ///  If there are any enemy forces on the way, attacks them
            /// </summary>
            Ordering,
            /// <summary>
            /// Squad is not controlling its units
            /// </summary>
            Nothing,
            /// <summary>
            /// Units are attacking closest enemy unit
            /// </summary>
            AttackingClosest
        }
        /// <summary>
        /// Gets current squad state
        /// </summary>
        public States State
        {
            get { return state; }
        }
        States state,neededState;
        /// <summary>
        /// Name of squadron. It may be useful for squadron identification.
        /// </summary>
        public string Name;
        /// <summary>
        /// current behaviour of squadron
        /// </summary>
        public Behaviours behaviour{get;private set;}
        Timer stateChangingTimer, reSortingTimer;
        /// <summary>
        /// Squardon units count
        /// </summary>
        public int Count
        {
            get { return formation.Count; }
        }
        /// <summary>
        /// squadron units container
        /// </summary>
        public Formation formation { get; private set; }
        /// <summary>
        /// Gets flying position of unit. 
        /// This parameter is valuable only if unit is flying to target position
        /// </summary>
        public GameVector flyingPos { get; private set; }
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
        public Squadron(UnitPilot Colonel,IGame game,FormationTypes FormationType)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            stateChangingTimer = new Timer(0.5f);
            behaviour = Behaviours.Waiting;
            reSortingTimer = new Timer(2);

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
        public Squadron(UnitPilot Colonel, IGame game)
        {
            isOrdered = false;
            state = States.Nothing;
            neededState = States.Nothing;
            stateChangingTimer = new Timer(0.5f);
            behaviour = Behaviours.Waiting;
            reSortingTimer = new Timer(2);

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
            foreach (UnitPilot pilot in formation)
            {                pilots.Add(pilot);            }
            switch (NewType)
            {
                case FormationTypes.Line: formation = new LineFormation(Colonel, pilots, 100, AngleClass.pi * 0.0f); break;
                case FormationTypes.Bar: formation = new BarFormation(Colonel, pilots, 5, 200, 150, 0); break;
                case FormationTypes.Stagger: formation = new StaggerFormation(Colonel, pilots, 90, 120); break;
                case FormationTypes.Swarm: formation = new SwarmFormation(pilots); break;
                //case FormationTypes.Custom: formation = new CustomFormation(pilots,0); break;
            }            
        }
        /// <summary>
        /// Sets formation type to parametrized stagger
        /// </summary>
        /// <param name="WidthBetweenUnits">Width between two neirbours</param>
        /// <param name="DepthBetweenUnits">Depth between two neirbours</param>
        
        public void SetFormationTypeStagger(float WidthBetweenUnits, float DepthBetweenUnits)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots = new List<UnitPilot>();
            foreach(UnitPilot pilot in formation)            
            { pilots.Add(pilot); }
            formation = new StaggerFormation(Colonel, pilots,WidthBetweenUnits, DepthBetweenUnits);
            //formation.SetAngle(Angle);
        }
        /// <summary>
        /// Sets formation type to parametrized bar
        /// </summary>
        /// <param name="CUnitsInLine"></param>
        /// <param name="WidthBetweenUnits">Width between two neirbours</param>
        /// <param name="DepthBetweenUnits">Depth between two neirbours</param>        
        public void SetFormationTypeBar(int CUnitsInLine,float WidthBetweenUnits, float DepthBetweenUnits)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots = new List<UnitPilot>();
            foreach (UnitPilot pilot in formation)
            { pilots.Add(pilot); }
            formation = new BarFormation(Colonel, pilots, CUnitsInLine, WidthBetweenUnits, DepthBetweenUnits,0);
        }
        
        /// <summary>
        /// Sets formation type to parametrized line
        /// </summary>
        /// <param name="WidthBetweenUnits">Width between two neirbours. Use from 150 to 250 as a common distance</param>        
        public void SetFormationTypeLine(float WidthBetweenUnits)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            behaviour = Behaviours.Waiting;

            UnitPilot Colonel = formation.Leader;
            List<UnitPilot> pilots = new List<UnitPilot>();
            foreach (UnitPilot pilot in formation)
            { pilots.Add(pilot); }
            formation = new LineFormation(Colonel, pilots, WidthBetweenUnits, 0);
        }
        private void UpdateOrdered()        
        {
            
             //AI.game.GeometryViewer.DrawPoint(flyingPos, Color.Green);
        //     AI.game.GeometryViewer.DrawPoint(formation.GetMassCenter(), Color.Blue);
             
            if (stateChangingTimer.TimeElapsed)
            {
                
                float newAngle = (flyingPos - formation.GetMassCenter()).Angle();
                
                switch (behaviour)
                {
                    case Behaviours.AttackingClosest:
                        neededState = States.Fighting;
                        break;
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

                        if (formation.Leader != null)
                            if (formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius))
                            { neededState = States.Fighting; }

                        break;
                    case Behaviours.Waiting:

                        //if (AngleClass.Distance(newAngle, formation.Angle) > AngleClass.pi * 0.20f)
                        //{
                        //    formation.SetAngle(newAngle);
                        //}
                        neededState = States.Nothing;
                        if (formation.PositionHoldingMistake() < 0.05f)
                        { neededState = States.Rotating; }
                        else
                        { neededState = States.Gathering; }

                        if (formation.Leader != null)
                            if (formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius))
                            { neededState = States.Fighting; }

                        break;
                    case Behaviours.HoldingPosition:
                        neededState = States.Defending;
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
                            neededState = States.Nothing;                            
                            behaviour = Behaviours.Waiting;
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
                            neededState = States.Nothing;
                            behaviour = Behaviours.Waiting;
                        }

                    
                        if (formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius*1.1f+formation.Leader.computer.StopDistance))
                        {
                            neededState = States.Fighting;
                        }
                        break;
                    case Behaviours.Nothing:
                        {
                            neededState = States.Nothing;
                        }
                        break;
                }
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
                case States.Defending:
                    if (state != States.Defending)
                    {
                        foreach (UnitPilot pilot in formation)
                            pilot.HoldPosition();
                        state = States.Defending;
                    }
                    break;
                case States.Fighting:
                    if (state != States.Fighting)
                    {
                        //Fight();
                        foreach (UnitPilot pilot in formation)
                            pilot.AttackClosest();
                        state = States.Fighting;
                    }
                    break;
                case States.Nothing:
                    //if (state != States.Standing)
                    //{
                    //    state = States.Standing;
                    //    foreach (Formation.PilotPositionPair pilotpos in formation.subordinates)
                    //        pilotpos.Pilot.HoldPosition();
                    //}
                    state = States.Nothing;
                    break;
                default :
                    break;
            }
                    
                    

                    formation.Update();
            //DEBUG
                    //formation.DrawPositions();
                    //formation.DrawDestinations();


        }
        private void UpdateSwarm()
        {
            if (stateChangingTimer.TimeElapsed)
            {
                

                switch (behaviour)
                {
                    case Behaviours.AttackingClosest:
                        neededState = States.Fighting;
                        break;
                    case Behaviours.Waiting:
                        neededState = States.Nothing;
                        if (formation.Leader != null)
                            if (formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius))
                            { neededState = States.Fighting; }
                        break;
                    case Behaviours.HoldingPosition:
                        neededState = States.Defending;
                        break;
                    case Behaviours.FlyingToTgt:    
                           neededState = States.Flying;
                           if (ReceivedToFlyingTgt)
                           { neededState = States.Nothing; behaviour = Behaviours.Waiting; }

                           
                        break;
                    case Behaviours.Attacking:
                        neededState = States.Flying;                        
                        if (ReceivedToFlyingTgt)
                        { neededState = States.Nothing; }
                        if (formation.Count > 0 && formation.CloseEnemyExists(formation[0].ControlledUnit.ShootingRadius * 1.1f + formation[0].computer.StopDistance))
                        { neededState = States.Fighting; }
                        break;
                    case Behaviours.Nothing:
                        {
                            neededState = States.Nothing;
                        }
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
                case States.Defending:
                    if (state != States.Defending)
                    {
                        foreach (UnitPilot pilot in formation)
                            pilot.HoldPosition();
                        state = States.Defending;
                    }
                    break;
                case States.Nothing:
                    state = States.Nothing;
                    //if (state != States.Standing)
                    //{
                    //    HoldPosition();
                    //    state = States.Standing;
                    //}
                    break;
                default:
                    break;
            }
            

            formation.Update();
        }

        private void FlySwarm()
        {
            //GameVector center = formation.GetMassCenter();
            //foreach (Formation.PilotPositionPair pilotPos in formation)
            //{
            //    pilotPos.Pilot.GoTo(new RelativeVector(flyingPos+pilotPos.Pilot.ControlledUnit.Position-center));
            //}

            foreach (UnitPilot pilot in formation)
            {
                pilot.GoTo(new RelativeVector( flyingPos));
            }
        }

        private void FightSwarm()
        {
            foreach (UnitPilot pilot in formation)
            {
                pilot.AttackClosest();
            }
        }
        internal void Update()
        {
            if (IsOrdered) UpdateOrdered();
            else UpdateSwarm();
        }

        private void Fight()
        {
            foreach (UnitPilot pilot in formation)
            {
                pilot.HoldPosition();
            }
        }
        private void HoldPosition()
        {
            foreach (UnitPilot pilot in formation)
            {
                pilot.HoldPosition();
            }
        }

        private void Gather(GameVector position, float Angle)
        {

            //formation = new BarFormation(pilots[0], pilots, 5, 200, 150, (flyingPos - formation.GetMassCenter()).Angle());
            //formation.SetAngle((flyingPos - formation.GetMassCenter()).Angle());
            int ind = 0;
            formation.CreatePositions(position, Angle);
            foreach (UnitPilot pilot in formation)
            {
                pilot.GoTo(formation.GetPosition(ind));
                ind++;
            }
        }
        /// <summary>
        /// True if squadron's mass center is near it's flying target.
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
            foreach (UnitPilot pilot in formation)
            {
                if (pilot != formation.Leader)
                {
                    orderData.OrderPosition = formation.GetPosition(ind);
                    pilot.Marshal(orderData);
                    //pilot.GoTo(formation.GetPosition(ind));
                }
                ind++;
            }
        }

        private void RotateOrder(float rotationAngle)
        {
            foreach (UnitPilot pilot in formation)
            {
                pilot.StandBy(rotationAngle);
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
        float GatherAngle;
        GameVector GatherPosition;

        #region orders
        
        /// <summary>
        /// Order. 
        /// Units are forced to array - go to positions to create specified order.        
        /// </summary>
        /// <param name="position">position of the new order</param>
        /// <param name="Angle">angle of the new order</param>            
        public void ArrayOrder(GameVector position,float Angle)
        {
            GatherAngle = Angle;
            GatherPosition = position;
            flyingPos = position;
            state = States.Nothing;
            behaviour = Behaviours.Ordering;           
            formation.CreatePositions(position, Angle);              
        }
       
        /// <summary>
        /// Order.
        /// Units are forced to fly at specified position
        /// </summary>
        /// <param name="FlyingPos">position to fly to</param>
        public void GoToOrder(GameVector FlyingPos)
        {
            state = States.Nothing;
            behaviour = Behaviours.FlyingToTgt;
            flyingPos = FlyingPos;
        }
     
        /// <summary>
        /// Order.
        /// Orderes squadron to attack specified position
        /// </summary>
        /// <param name="FlyingPos">position to attack</param>
        public void AttackOrder(GameVector FlyingPos)
        {
            state = States.Nothing;
            behaviour = Behaviours.Attacking;
            flyingPos =FlyingPos;
        }
        /// <summary>
        /// Order.
        /// Units are staying at their positions.
        /// They are shooting enemy if are able to do this from their places.
        /// </summary>
        public void HoldPositionOrder()
        {
            behaviour = Behaviours.HoldingPosition;
            HoldPosition();
        }
        /// <summary>
        /// Order.
        /// Units become to be not controlled by this squadron
        /// </summary>
        public void NullOrder()
        {
            behaviour = Behaviours.Nothing;
        }
        /// <summary>
        /// Order.
        /// Units are waiting at their positions.
        /// If enemy is founded in shooting radius, units are attacking him
        /// </summary>
        public void StandByOrder()
        {
            behaviour = Behaviours.Waiting;
        }
       
        #endregion        
    
        #region IEnumerable Members
        /// <summary>
        /// IEnumerable implementation member
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return formation.GetEnumerator();
        }

        #endregion
        /// <summary>
        /// Sets maximum allowed speed for every unit in squadron
        /// </summary>
        /// <param name="speed">Desired maximum speed</param>
        public void SetSpeed(float speed)
        {
            foreach (UnitPilot unit in formation)
                unit.SetMaxSpeed(speed);            
        }
        /// <summary>
        /// Order.
        /// Units are attacking closest enemy
        /// </summary>
        public void AttackClosest()
        {
            behaviour = Behaviours.AttackingClosest;
        }
    }
}
