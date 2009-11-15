using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    public class UnitPilot
    {
        //TYPES
        public enum Behaviours
        {
            StandingBy,                     //+doing nothing
            PositionHolding,                //+attacking target (nearest to shot line), not moving
            Going,                          //+moving to specified target(realative vector)
            AttackingClosest,               //+going to nearest target and shooting it
            AttackingTarget,                //+chasing and attacking specified target
            GoingRam,                       //+shooting target and going ram
            GoingRamToClosest,              //+goes ram to closest target
            Escaping,                       //escaping from specified unit
            MaintainOrder                   //getting a right place in array
        }
        public class MaintainingOrderData
        {
            public UnitPilot Leader;
            public float RotationAngle;
            public float AllowedDistToOrderedPlace;
            public RelativeVector OrderPosition;
            public MaintainingOrderData(UnitPilot Leader, float RotationAngle, float AllowedDistToOrderedPlace)
            {
                this.Leader = Leader;
                this.RotationAngle = RotationAngle;
                this.AllowedDistToOrderedPlace = AllowedDistToOrderedPlace;
            }
        }
        //CONST FIELDS
        ShipTypes type;
        int playerNumber;
        IUnit controlledUnit;
        Radar radar;
        public OnBoardComputer computer;
        //STATE FIELDS
        public Behaviours behaviour {get;private set;}
        RelativeVector tgtPosition;
        IUnit target;
        Timer targetUpdateTime;//time to update target
        MaintainingOrderData maintainingInOrderData;
        bool escapeThreting;
        /// <summary>
        /// for Going behaviour
        /// </summary>
        bool StopNearTgt;
        //ACCESORS
        public IUnit ControlledUnit
        { get { return controlledUnit; } }
        public IUnit Target
        { get { return target; } }
        public ShipTypes Type
        { get { return type; } }
        //CTOR
        public UnitPilot(IUnit ControlledUnit, AI AI)
        {
            controlledUnit = ControlledUnit;
            radar = new Radar(controlledUnit.ShootingRadius + 50, controlledUnit, AI);
            computer = new OnBoardComputer(radar, controlledUnit);
            dangerRadius = dangerRadiusCoef *
                ((new Rectangle(GameVector.Zero, controlledUnit.Size, GameVector.UnitX)).GetBoundingCircle.Radius);
            StandBy();
            escapeThreting = true;

            //SetFlightTgt(controlledUnit.Position + new GameVector(-500, 800));
            targetUpdateTime = new Timer(1, AI.game);
            speedAim = 0;
            allowedSpeed = 0;
            AvoidingObstaclesEnabled = true;
            timer = new Timer(0.05f, AI.game);
        }
        //METHODS
        public bool IsEnemy(IUnit unit)
        {
            return radar.AI.IsEnemy(unit);
        }
        /// <summary>
        /// chooses new target periodically. 
        /// if there are targets in radar then chooses closest to shot line,
        /// choosing nearest othervise
        /// </summary>
        /// <returns>true if enemy was recalculated</returns>
        bool UpdateEnemy()
        {
            if (target != null && target.Dead) target = null;

            targetUpdateTime.Update();
            if (targetUpdateTime.TimeElapsed && (target == null || !computer.IsInThisShootRadius(target)))
            {

                targetUpdateTime.Reset();
                IUnit newTarget = computer.NearestToShootingLineEnemy();//choose closest to shot line
                if (newTarget == null)
                {
                    //choose closest in the world
                    float minDistSq = float.PositiveInfinity;
                    float currDistSq;
                    for (int i = 0; i < radar.Game.UnitsCount; i++)
                    {
                        IUnit unit = radar.Game.GetUnit(i);
                        if (!unit.Dead)
                        {
                            currDistSq = GameVector.DistanceSquared(unit.Position, controlledUnit.Position);
                            if (IsEnemy(unit) && currDistSq < minDistSq)
                            {
                                minDistSq = currDistSq;
                                newTarget = unit;
                            }
                        }
                    }
                }
                target = newTarget;
                return true;
            }
            return false;
        }
        /// <summary>
        /// turns to target(using the most quicly algorithm)
        /// </summary>
        void TurnToTarget()
        {
            if (target != null)
            {
                SetAngle(controlledUnit.AngleTo(target.Position));
            }
        }
        //TESTED 
        /// <summary>
        /// shoots if hitting enemy predicted
        /// hitting enemy predicted when enemy is on the shooting line and no friend unit is before him on shooting line
        /// </summary>
        void ShootIfWillDamageEnemy()
        {

            if (target != null)
            {
                if (computer.IsInThisShootRadius(target))
                {
                    if (computer.TargetingMistake(target) < 0.04f)
                        if (!computer.PredictFriendlyFire(target))
                        {
                            controlledUnit.Shoot();
                        }
                }
            }
        }
        #region set behaviours
        public void StandBy(float rotationAngle)
        {
            computer.WeakAvoidingObstacles = false;
            SetAngle(rotationAngle);
            behaviour = Behaviours.StandingBy;
        }
        public void StandBy()
        {
            computer.WeakAvoidingObstacles = false;
            StandBy(controlledUnit.RotationAngle);
        }
        public void HoldPosition()
        {
            computer.WeakAvoidingObstacles = false;
            behaviour = Behaviours.PositionHolding;
        }

        public void GoTo(RelativeVector Target,bool StopNearTgt)
        {
            
            computer.WeakAvoidingObstacles = false;
            this.StopNearTgt = StopNearTgt;
            //tgtPosition = Target;
            flyingTgt = Target;
            allowedSpeed = controlledUnit.Speed;
            speedAim = controlledUnit.Speed;
            allowedSpeed = 0;
            speedAim = 0;
            behaviour = Behaviours.Going;

        }
        public void GoTo(RelativeVector Target)
        {
            GoTo(Target, true);
        }
        public void AttackTarget(IUnit Target)
        {
            escapeThreting = false;
            computer.WeakAvoidingObstacles = false;
            behaviour = Behaviours.AttackingTarget;
            target = Target;
            AvoidingObstaclesEnabled = true;
            flyingTgt = new RelativeVector(target, controlledUnit, controlledUnit.ShootingRadius * 0.9f);
        }
        public void AttackClosest()
        {
            escapeThreting = false;
            computer.WeakAvoidingObstacles = false;
            behaviour = Behaviours.AttackingClosest;
            target = null;
            targetUpdateTime.ExeedDeltaTime();
            UpdateEnemy();
            AvoidingObstaclesEnabled = true;
        }
        public void GoRam(IUnit Target)
        {
            computer.WeakAvoidingObstacles = false;
            behaviour = Behaviours.GoingRam;
            target = Target;
            AvoidingObstaclesEnabled = true;
            flyingTgt = new RelativeVector(target);
        }
        public void GoRamToClosest()
        {
            computer.WeakAvoidingObstacles = false;
            behaviour = Behaviours.GoingRamToClosest;
            AvoidingObstaclesEnabled = true;
            target = null;
            targetUpdateTime.ExeedDeltaTime();
            UpdateEnemy();
        }
        public void Marshal(MaintainingOrderData data)
        {
            computer.WeakAvoidingObstacles = true;
            maintainingInOrderData = data;
            flyingTgt = data.OrderPosition;
            behaviour = Behaviours.MaintainOrder;
        }
        public bool WeakAvoidingCollizions
        {
            get { return computer.WeakAvoidingObstacles; }
            set { computer.WeakAvoidingObstacles = true; }
        }
        
        public bool EscapeThreating
        {
            get { return escapeThreting; }
            set { escapeThreting = value; }
        }
        #endregion
        public void MarshalingUpdate(UnitPilot Leader, float RotationAngle)
        {
            maintainingInOrderData.Leader = Leader;
            maintainingInOrderData.RotationAngle = RotationAngle;
        }
        Timer timer;
        bool noObstacles;
        bool escaping;
        bool EnemyRecalculated;
        public bool ReceivedToFlyingTgt { get; private set; }
        public void Update()
        {
            timer.Update();
            RemoveDeadTarget();
            //finite state machiene method. 
            //implements all behaviours of unit.
            switch (behaviour)
            {
                case Behaviours.StandingBy:
                    StopMoving();
                    if (timer.TimeElapsed)
                    {
                        timer.Reset();
                        noObstacles = AvoidObstacles(true);
                    }
                    UpdateGoingToSpeed();
                    UpdateRotatingToAngle();
                    //does nothing
                    break;
                case Behaviours.PositionHolding:
                    //"turret mode"
                    //no movings
                    //if there is a target in radius
                    //then attacks it

                    UpdateEnemy();
                    TurnToTarget();
                    ShootIfWillDamageEnemy();


                    radar.Update();
                    StopMoving();
                    if (timer.TimeElapsed)
                    {
                        timer.Reset();
                        noObstacles = AvoidObstacles(true);
                    }
                    UpdateRotatingToAngle();                   
                    UpdateGoingToSpeed();

                    break;
                case Behaviours.Going:
                    {
                        
                        radar.Update();
                        UpdateGoingToSpeed();
                        UpdateRotatingToAngle();
                        if (timer.TimeElapsed)
                        {
                            timer.Reset();
                            noObstacles = AvoidObstacles(true);
                        }
                        if (noObstacles)
                        {
                           // ReceivedToFlyingTgt = FlyToTgt(StopNearTgt);
                            ReceivedToFlyingTgt = FlyToTgt(true);
                        }
                        else ReceivedToFlyingTgt = false;
                    }
                    break;
                case Behaviours.AttackingTarget:
                    if (target == null)
                    {
                        Stop();
                        break;
                    }

                    if (timer.TimeElapsed)
                    {
                        timer.Reset();
                        noObstacles = AvoidObstacles(true);
                    }

                    if (escapeThreting)
                        escaping = EscapeFromThreatingUpdate();
                    else escaping = false;
                    if (target != null && computer.IsInThisShootRadius(target) && !escaping)
                    {
                        TurnToTarget();
                        StopMoving();
                    }
                    else
                    {
                        if (noObstacles)
                        {
                            FlyToTgt(true);
                        }
                    }

                    ShootIfWillDamageEnemy();
                    radar.Update();
                    UpdateRotatingToAngle();
                    UpdateGoingToSpeed();
                    break;
                case Behaviours.AttackingClosest:
                    EnemyRecalculated = UpdateEnemy();
                    if (target != null)
                    {
                        if (EnemyRecalculated) 
                            flyingTgt = new RelativeVector(target, controlledUnit, controlledUnit.ShootingRadius * 0.9f);
                    }
                    if (timer.TimeElapsed)
                    {
                        timer.Reset();
                        noObstacles = AvoidObstacles(true);
                    }

                    if (escapeThreting)
                        escaping= EscapeFromThreatingUpdate();
                    else escaping=false;
                    if (target != null && computer.IsInThisShootRadius(target) && !escaping)
                    {
                        TurnToTarget();
                        StopMoving();
                    }
                    else
                    {

                        if (noObstacles)
                        {                     
                            FlyToTgt(true);
                        }
                    }
                    UpdateRotatingToAngle();
                    UpdateGoingToSpeed();

                    if (target == null)
                    {
                        Stop();
                    }
                    ShootIfWillDamageEnemy();
                    radar.Update();
                    break;
                case Behaviours.GoingRam:
                    if (target != null)
                    {
                        //radar.Game.GeometryViewer.DrawPoint(target.Position, Color.White);
                        UpdateGoingToSpeed();
                        noObstacles = AvoidObstacles(false);
                        if (noObstacles)
                        {
                            FlyToTgt(false);
                        }
                        ShootIfWillDamageEnemy();
                        radar.Update();
                        UpdateRotatingToAngle();
                        if (target == null) Stop();
                    }
                    else Stop();
                    break;
                case Behaviours.GoingRamToClosest:
                    EnemyRecalculated = UpdateEnemy();
                    if (target != null)
                    {
                        //radar.Game.GeometryViewer.DrawPoint(target.Position, Color.White);
                        if (EnemyRecalculated) flyingTgt =
                            new RelativeVector(target);
                    }
                    noObstacles = AvoidObstacles(false);

                    if (noObstacles)
                    {
                        FlyToTgt(false);
                    }

                    UpdateRotatingToAngle();
                    UpdateGoingToSpeed();
                    ShootIfWillDamageEnemy();
                    radar.Update();
                    break;
                case Behaviours.MaintainOrder:
                    ////if isn't near than allowed than tries to go close, else tries to follow leader state
                    //if (GameVector.DistanceSquared(flyingTgt.Value, controlledUnit.Position) >
                    //    maintainingInOrderData.AllowedDistToOrderedPlace * maintainingInOrderData.AllowedDistToOrderedPlace)
                    //{
                    //    radar.Update();
                    //    UpdateGoingToSpeed();
                    //    UpdateRotatingToAngle();
                    //    noObstacles = AvoidObstacles(true);
                    //    if (noObstacles)
                    //    {
                    //        FlyToTgt(false);
                    //    }
                    //    //AI.game.GeometryViewer.DrawPoint(controlledUnit.Position, new Color(0,1,1,1));
                    //}
                    //else
                    //{                  
                    //follow leader state
                    SetAngle(maintainingInOrderData.RotationAngle);
                    SetSpeed(maintainingInOrderData.Leader.controlledUnit.Speed);
                    radar.Update();
                    UpdateGoingToSpeed();
                    UpdateRotatingToAngle();
                    //AI.game.GeometryViewer.DrawPoint(controlledUnit.Position, Color.Green);
                    //}

                    //UpdateGoingToSpeed();
                    //UpdateRotatingToAngle();
                    //float AimIsToRepairOrder = GameVector.Distance(flyingTgt.Value, controlledUnit.Position) / maintainingInOrderData.AllowedDistToOrderedPlace;
                    //if (AimIsToRepairOrder > 0.5f) AimIsToRepairOrder -= 0.5f;
                    //else AimIsToRepairOrder = 0;
                    //if (AimIsToRepairOrder > 1) AimIsToRepairOrder = 1;
                    //AimIsToRepairOrder = 0;

                    //if (GameVector.DistanceSquared(flyingTgt.Value, controlledUnit.Position) >
                    //    maintainingInOrderData.AllowedDistToOrderedPlace * maintainingInOrderData.AllowedDistToOrderedPlace)
                    //{
                    //    noObstacles = AvoidObstacles(true);
                    //    if (noObstacles)
                    //    {
                    //        FlyToTgt(false);
                    //    }
                    //}

                    //float NeededAngle = (angleAim - maintainingInOrderData.RotationAngle) * AimIsToRepairOrder + maintainingInOrderData.RotationAngle;
                    //float NeededSpeed = (speedAim - maintainingInOrderData.Leader.controlledUnit.Speed) * AimIsToRepairOrder + maintainingInOrderData.Leader.controlledUnit.Speed;
                    //SetAngle(NeededAngle);
                    //SetSpeed(NeededSpeed);

                    break;
            }
            
        }
        void RemoveDeadTarget()
        {
            if (target != null)
            {
                if (target.Dead)
                {
                    target = null;
                    targetUpdateTime.ExeedDeltaTime();
                }
            }
        }
        #region avoiding collizions members
        ///DANGER! HEURISTIC CONSTANTS        
        /// <summary>
        /// radius(in unit bounding sphere sizes) in which unit must escape 
        /// from his enemy, not fly around him
        /// </summary>
        const float dangerRadiusCoef = 9.0f;
        float dangerRadius;
        public bool AvoidingObstaclesEnabled;

        bool AvoidObstacles(bool EnemiesConsidered)
        {
            bool res = true;

            //if (GameVector.Distance(controlledUnit.Position, new GameVector(-2291,-318)) < 5) { }

            if (AvoidingObstaclesEnabled)
            {

                OnBoardComputer.CollizionPredictionInfo collizionInfo =
                    computer.CollizionPredictionCheck(EnemiesConsidered);
                //controlledUnit.Text=collizionInfo.CollizionExpected.ToString();
                
                float angleToObstacle = controlledUnit.AngleTo(collizionInfo.ObstacleCenter);
                float angleToFlyingTgt;
                if (flyingTgt != null && !flyingTgt.Invalid) angleToFlyingTgt = controlledUnit.AngleTo(flyingTgt.Value);
                else angleToFlyingTgt = controlledUnit.RotationAngle;
                float angleDifference = AngleClass.Difference(angleToObstacle,
                    controlledUnit.RotationAngle);
                if (Math.Abs(angleDifference) < AngleClass.pi / 2)//do something only if obstacle is forward
                {

                    float SlowingIntensity = 0;//0  - no slowing, flying in maxSpeed. 1 - stop
                    float RotatingIntensity = 0;//0  - rotating to flying tgt, 1 - rotating away from obstacle
                    #region avoiding obstacles logic
                    if (collizionInfo.CollizionExpected)
                    {
                        res = false;

                        SlowingIntensity = (250 - collizionInfo.DistToObstacle/5) / 100.0f;//SlowingIntensity = (250 - collizionInfo.DistToObstacle) / 100.0f;
                        RotatingIntensity = 1;
                        SlowingIntensity = Math.Max(0, Math.Min(1, SlowingIntensity));
                        if (Math.Abs(angleDifference) > 1.3f) SlowingIntensity = 0;

                    }
                    #endregion

                     

                    SetSpeed(allowedSpeed * (1 - SlowingIntensity));

                    float angleToEscapingPt = AngleClass.Add((angleDifference > 0) ? (-AngleClass.pi * 0.7f + angleDifference) : (AngleClass.pi * 0.7f + angleDifference),
                        controlledUnit.RotationAngle);


                    float rotatingAngle = angleToFlyingTgt * (1 - RotatingIntensity) + angleToEscapingPt * RotatingIntensity;
                    SetAngle(rotatingAngle);

                    //radar.Game.GeometryViewer.DrawLine(new Line(controlledUnit.Position,
                    //    controlledUnit.Position + GameVector.UnitX.Rotate(rotatingAngle) * 50f), Color.Green);                    
                    //radar.Game.GeometryViewer.DrawPoint(collizionInfo.ObstacleCenter, Color.Green);
                    //radar.AI.Game.GeometryViewer.DrawPoint(flyingTgt, Color.Red);
                    //radar.AI.Game.GeometryViewer.DrawLine(new Line(controlledUnit.Position, controlledUnit.Position + controlledUnit.Forward * StopDistance), Color.Blue);

                }
            }
            return res;
        }
        #endregion
        #region controlling the unit's flight
        /// <summary>
        /// current point where to fly
        /// </summary>
        public RelativeVector flyingTgt{get;private set;}
        /// <summary>
        /// current needed flying speed
        /// </summary>
        float allowedSpeed;
        //DANGER! HEURISTIC COSTANTS
        const float allowedSpeedMaxIncrement = 0.4f;
        const float allowedSpeedMaxDecrement = 0.8f;
        /// <summary>
        /// controlling unit to fly to position
        /// </summary>
        /// <param name="StopNearTgt">true if must stop near target</param>
        /// <returns>true if flying is finished</returns>
        bool FlyToTgt(bool StopNearTgt)
        {
            if (flyingTgt != null && !flyingTgt.Invalid)
            {
                float angleDifference = Math.Abs(AngleClass.Difference(controlledUnit.AngleTo(flyingTgt.Value), controlledUnit.RotationAngle));
                bool TimeToStop=GameVector.DistanceSquared(flyingTgt.Value, controlledUnit.Position) < (computer.StopDistance + 15) * (computer.StopDistance + 15);
                if (StopNearTgt && TimeToStop)
                {
                    //if (allowedSpeed - allowedSpeedMaxDecrement > 0) allowedSpeed -= allowedSpeedMaxDecrement;
                    //else 
                    allowedSpeed = 0;                    
                }
                else
                    if (angleDifference > 3.1415f / 4f)
                    {

                        //if (allowedSpeed - allowedSpeedMaxDecrement > controlledUnit.MaxSpeed / 6f) allowedSpeed -= allowedSpeedMaxDecrement;
                        //else allowedSpeed = controlledUnit.MaxSpeed / 6f;

                        if (speedAim > controlledUnit.MaxSpeed / 6f) speedAim = controlledUnit.MaxSpeed / 6f;
                        //SetSpeed(0);
                    }
                    else
                    {
                        if (allowedSpeed < controlledUnit.MaxSpeed - allowedSpeedMaxIncrement) allowedSpeed += allowedSpeedMaxIncrement;
                        else allowedSpeed = controlledUnit.MaxSpeed;
                        //SetSpeed(100000);
                    }
                SetSpeed(allowedSpeed);
                SetAngle(controlledUnit.AngleTo(flyingTgt.Value));

                
                //finishing flight is when unit is close to tgt and (unit is stopped or not need to stop)
                return (TimeToStop && ((controlledUnit.Speed < controlledUnit.MaxSpeed * 0.1f) || !StopNearTgt));
                
            }
            else return true;
        }

        float angleAim;

        void SetAngle(float Angle)
        {
            angleAim = Angle;
            rotatesToAngle = true;
        }
        void StopRotation()
        {
            //rotatesToAngle = false;
            angleAim = controlledUnit.RotationAngle + 0.001f;
        }
        void StopMoving()
        {
            SetSpeed(0);
        }
        void Stop()
        {
            StopMoving();
            StopRotation();
        }
        bool rotatesToAngle;
        void UpdateRotatingToAngle()
        {
            if (rotatesToAngle)
            {
                //neededSpeed=(rotationangle-angleaim)/dt
                //neededAcceleration=neededSpeed/dt
                float neededAcceleration = AngleClass.Distance(controlledUnit.RotationAngle, angleAim) 
                    / (AI.game.TimeElapsed * AI.game.TimeElapsed);

                //AI.game.TimeElapsed
             
                float AccelerationUp = Math.Min(controlledUnit.MaxRotationAcceleration,neededAcceleration);// *0.5f;//PROBABLY
                float AccelerationDown = Math.Min(controlledUnit.MaxRotationAcceleration, neededAcceleration);

                float rotationNeeded = AngleClass.Normalize(angleAim - controlledUnit.RotationAngle) / 3;
                ////time will be taken to stop
                //float stopTime = controlledUnit.RotationSpeed / controlledUnit.MaxRotationAcceleration;

                //ange will be spent for stop
                float rotationSpeed = controlledUnit.RotationSpeed;
                float StopAngle = rotationSpeed * rotationSpeed / controlledUnit.MaxRotationAcceleration
                    - rotationSpeed * rotationSpeed / controlledUnit.MaxRotationAcceleration //* Math.Sign(controlledUnit.RotationSpeed)
                    * 0.5f;

                //bool AccelUp = false;
                if (rotationNeeded > 0)
                {
                    if (controlledUnit.RotationSpeed < 0)
                    {
                        controlledUnit.RotationAccelerate(AccelerationUp);
                        //      AccelUp = true;
                    }
                    else
                    {
                        if (rotationNeeded - StopAngle > 0)
                        {
                            controlledUnit.RotationAccelerate(AccelerationUp);
                            //        AccelUp = true;
                        }
                        else
                        {
                            controlledUnit.RotationAccelerate(-AccelerationDown);
                            //      AccelUp = false;
                        }
                    }
                }
                else
                {
                    if (controlledUnit.RotationSpeed > 0)
                    {
                        controlledUnit.RotationAccelerate(-AccelerationUp);
                        //AccelUp = false;
                    }
                    else
                    {
                        if (rotationNeeded + StopAngle < 0)
                        {
                            controlledUnit.RotationAccelerate(-AccelerationUp);
                            //  AccelUp = false;
                        }
                        else
                        {
                            controlledUnit.RotationAccelerate(AccelerationDown);
                            //AccelUp = true;
                        }
                    }
                }
                if (Math.Abs(controlledUnit.RotationSpeed) < 0.01f && Math.Abs(rotationNeeded) < 0.001f)
                    controlledUnit.RotationAccelerate(0);

            }
        }


        float speedAim;
        //tested
        public void SetSpeed(float Speed)
        {            
            speedAim = Speed;
        }
        //twice tested
        void UpdateGoingToSpeed()
        {
           
            if (controlledUnit.Speed < speedAim)
            {
                controlledUnit.Accelerate(controlledUnit.MaxSpeedAcceleration);
            }
            else
            {
                if (controlledUnit.Speed > speedAim)
                {

                    controlledUnit.Accelerate(-controlledUnit.MaxSpeedAcceleration);
                }
                else
                {
                    controlledUnit.Accelerate(speedAim - controlledUnit.Speed);
                }
            }
        }

        #endregion
        bool EscapeFromThreatingUpdate()
        {
           
            
            List<IUnit> threating = this.computer.GetThreateningUnits();
            if (threating.Count > 0)
            {
                int RotateCW = Math.Sign(AngleClass.Difference(controlledUnit.RotationAngle, controlledUnit.AngleTo(threating[0].Position)));
                if (RotateCW == 0) RotateCW = 1;
                flyingTgt = new RelativeVector( GameVector.Normalize(threating[0].Position - controlledUnit.Position).Rotate(RotateCW
                    * AngleClass.pi * 0.5f)*100+controlledUnit.Position);
                //if (controlledUnit.Name == "Destroyer -4-") { }
                //AI.game.GeometryViewer.DrawLine(new Line(controlledUnit.Position, flyingTgt.Value), Color.Blue);
                return true;
            }
            else return false;
        }
        internal float GetClosestEnemyDist()
        {
            float MinDistSq=float.MaxValue;
            float currDistSq;
            foreach (IUnit enemy in radar.Enemies)
            {
                currDistSq=GameVector.DistanceSquared(enemy.Position,controlledUnit.Position);
                if (currDistSq < MinDistSq)
                    MinDistSq = currDistSq;
                }
            return (float)Math.Sqrt(MinDistSq);
        }
    }
}
