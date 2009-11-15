using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using System.Collections;
namespace AINamespace
{
    public class SquadronColonel:IEnumerable
    {
        enum States
        {
            Gathering,//pilots are flying to their positions
            Rotating,//pilots on their positions are rotating to forward position            
            Flying,//flying to tgt
            Standing,//nothing
            Fighting,//fighting as turrets
            Nothing//null state
        }
        public enum Behaviours
        {
            FlyingToTgt,//rotates formation to tgt and then flying forward to tgt
            Attacking,//fying to tgt and then defending there
            Defending,//staying and shooting
            Waiting//doing nothing
        }
        
        States state,neededState;
        public Behaviours behaviour{get;private set;}
        Timer stateChangingTimer, reSortingTimer;
        Formation formation;
        GameVector flyingPos;
        bool isOrdered;
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
        public SquadronColonel(UnitPilot Colonel,IGame game,FormationTypes FormationType)
        {
            isOrdered = true;
            state = States.Nothing;
            neededState = States.Nothing;
            stateChangingTimer = new Timer(0.5f, game);
            behaviour = Behaviours.Waiting;
            reSortingTimer = new Timer(5, game);

            List<UnitPilot>  pilots = new List<UnitPilot>();
            switch (FormationType)
            {
                case FormationTypes.Line: formation = new LineFormation(Colonel, pilots, 100, AngleClass.pi * 0.0f); break;
                case FormationTypes.Bar: formation = new BarFormation(Colonel, pilots, 5, 200, 150, 0); break;
                case FormationTypes.Stagger: formation = new StaggerFormation(Colonel, pilots, 90, 120); break;
            }            
        }
        public SquadronColonel(UnitPilot Colonel, IGame game)
        {
            isOrdered = false;
            state = States.Nothing;
            neededState = States.Nothing;
            stateChangingTimer = new Timer(0.5f, game);
            behaviour = Behaviours.Waiting;
            reSortingTimer = new Timer(5, game);

            List<UnitPilot> pilots = new List<UnitPilot>();
            formation = new SwarmFormation(pilots);            
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
                    case Behaviours.Waiting:

                        if (AngleClass.Distance(newAngle, formation.Angle) > AngleClass.pi * 0.20f)
                            formation.SetAngle(newAngle);
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

            switch (neededState)
            {
                case States.Gathering:
                    if (state != States.Gathering)
                    { GatherOrder(); state = States.Gathering; }
                    else
                    {
                        if (reSortingTimer.TimeElapsed)
                        {
                            reSortingTimer.Reset();
                            
                            formation.SortUnitsByNearanceToPositions();
                           // GatherOrder();
                          
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
                        RotateOrder(formation.RotationAngle);
                        state = States.Rotating;
                    }
                    else
                    {
                        if (stateChangingTimer.TimeElapsed)
                        {
                            stateChangingTimer.Reset();
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
                        Fight();
                        state = States.Fighting;
                    }
                    break;
                case States.Standing:
                    
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
                        if (formation.Count > 0 && formation.CloseEnemyExists(formation.Leader.ControlledUnit.ShootingRadius * 0.5f + formation.Leader.computer.StopDistance))
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
        public void Update()
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
        #region orders
        private void GatherOrder()
        {
            
            //formation = new BarFormation(pilots[0], pilots, 5, 200, 150, (flyingPos - formation.GetMassCenter()).Angle());
            //formation.SetAngle((flyingPos - formation.GetMassCenter()).Angle());
            int ind=0;
            formation.CreatePositions();
            foreach (Formation.PilotPositionPair pilotPos in formation)
            {
                pilotPos.Pilot.GoTo(formation.GetPosition(ind));
                ind++;
            }            
        }
        public void GoToOrder(GameVector FlyingPos)
        {
            state = States.Standing;
            behaviour = Behaviours.FlyingToTgt;
            flyingPos = FlyingPos;                        
        }
        public void AttackOrder(GameVector FlyingPos)
        {
            state = States.Standing;
            behaviour = Behaviours.Attacking;
            flyingPos = FlyingPos;
        }
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
            UnitPilot.MaintainingOrderData orderData = new UnitPilot.MaintainingOrderData(formation.Leader, formation.RotationAngle, 40);
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
        public void AddUnit(UnitPilot unitPilot)
        {
formation.Add(unitPilot);
        }
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
