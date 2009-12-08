using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    /// <summary>
    /// this class is used by UnitPilot to provide information about tactical situation
    /// </summary>
    public class OnBoardComputer
    {
        Radar radar;
        /// <summary>
        /// Radar of this unit
        /// </summary>
        public Radar Radar
        { get { return radar; } }
        IUnit unit;
        internal OnBoardComputer(Radar Radar, IUnit Unit)
        { radar = Radar; unit = Unit; WeakAvoidingObstacles = false; }
        #region collizion prediction
        internal bool WeakAvoidingObstacles;
        internal struct CollizionPredictionInfo
        {
            public bool CollizionExpected;
            public GameVector CollizionPt;
            public GameVector ObstacleCenter;
            public float DistToObstacle;
        }
        internal CollizionPredictionInfo CollizionPredictionCheck(bool EnemiesConsidered)
        {


            CollizionPredictionInfo res;
            res.CollizionPt = GameVector.One * float.PositiveInfinity;
            res.ObstacleCenter = GameVector.One * float.PositiveInfinity;
            res.CollizionExpected = false;
            res.DistToObstacle = float.PositiveInfinity;



            Rectangle selfMoveVolume = CreateMoveVolume(unit);
            Rectangle obstacleMoveVolume;

            if (EnemiesConsidered)
            {
                radar.ResetIterator();
                while (radar.NextEnemy())
                {
                    obstacleMoveVolume = CreateMoveVolume(radar.CurrUnit);

                    if (selfMoveVolume.IntersectsRectangle(obstacleMoveVolume))
                    {
                        if ((unit.Position - radar.CurrUnit.Position).LengthSquared()
                            < (unit.Position - res.ObstacleCenter).LengthSquared())
                        {


                            if (GameVector.Dot(unit.Forward, radar.CurrUnit.Position - unit.Position) > 0)
                            {
                                res.CollizionPt = obstacleMoveVolume.Center;
                                res.ObstacleCenter = radar.CurrUnit.Position;
                                res.CollizionExpected = true;
                            }
                        }
                    }
                }
            }
            radar.ResetIterator();
            while (radar.NextFriend())
            {
                obstacleMoveVolume = CreateMoveVolume(radar.CurrUnit);
                if (selfMoveVolume.IntersectsRectangle(obstacleMoveVolume))
                {

                    if ((unit.Position - radar.CurrUnit.Position).LengthSquared()
                        < (unit.Position - res.ObstacleCenter).LengthSquared())
                    {

                        if (GameVector.Dot(unit.Forward, radar.CurrUnit.Position - unit.Position) > 0)
                        {
                            res.CollizionPt = obstacleMoveVolume.Center;
                            res.ObstacleCenter = radar.CurrUnit.Position;
                            res.CollizionExpected = true;
                        }
                    }
                }
            }
            res.DistToObstacle = GameVector.Distance(res.ObstacleCenter, unit.Position);
            //if (unit.Text == "1")
            {
                //Color col = res.CollizionExpected ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.5f);
                //radar.Game.GeometryViewer.DrawRectangle(selfMoveVolume, col);
                //DrawStopWay();

            }

            return res;
        }
        ////DEBUG
        //public void DrawStopWay()
        //{
        //    radar.Game.GeometryViewer.DrawLine(new Line(unit.Position, unit.Position + unit.Forward *
        //        StopDistance), Color.Blue);
        //}
        /// <summary>
        /// distance of stopping flight if full deaccelerating is used
        /// </summary>
        public float StopDistance
        {
            get
            {
                return unit.Speed * unit.Speed / unit.MaxSpeedAcceleration * 0.5f;
            }
        }
        //DANGER! HEURISTIC CONSTANTS!
        const float moveVolumeScale = 2.8f;// 2.8f;
        const float moveVolumeForward = 1.2f;// 1.2f;
        internal Rectangle CreateMoveVolume(IUnit unit)
        {
            float angle = unit.RotationSpeed * 0.3f;
            float speed = unit.Speed;
            float currMoveVolumeForward = (WeakAvoidingObstacles) ? moveVolumeForward * 0.5f : moveVolumeForward;

            //            if (currMoveVolumeForward < StopDistance) { currMoveVolumeForward = StopDistance; }            

            GameVector forward = unit.Forward.Rotate(angle);
            GameVector center = unit.Position + forward * speed * currMoveVolumeForward * 0.5f;
            return new Rectangle(center,
                new GameVector(unit.Size.X * moveVolumeScale + speed * currMoveVolumeForward, unit.Size.Y * moveVolumeScale),
                forward);
        }
        #endregion
        #region other predicates
        /// <summary>
        /// defines if one unit is targeted to another
        /// </summary>
        /// <param name="Object">unit that is checking to be targeting</param>
        /// <param name="Subject">unit that is checking to be targeted</param>
        /// <returns>true if Object is targeted to Subject</returns>
        static public bool TargetedAt(IUnit Object, IUnit Subject)
        {
            Line shotLine = new Line(Object.Position, Object.Position + Object.Forward * Object.ShootingRadius);
            return Subject.GetRectangle().IntersectsLine(shotLine.pt1, shotLine.pt2);
        }
        /// <summary>
        /// defines if this unit is targeted to another
        /// </summary>
        /// <param name="Subject">unit that is checking to be targeted</param>
        /// <returns>true if this unit is targeted to Subject</returns>
        public bool TargetedAt(IUnit Subject)
        {
            return OnBoardComputer.TargetedAt(unit, Subject);
        }
        
        static internal float TargetingMistake(IUnit Object, IUnit Subject)
        {
            return Math.Abs(Object.AngleTo(Subject.Position) - Object.RotationAngle);
        }
        //TESTED
        /// <summary>
        /// calculates absolute targeting mistake in radians
        /// </summary>
        /// <param name="Subject"></param>
        /// <returns></returns>
        internal float TargetingMistake(IUnit Subject)
        {
            return TargetingMistake(unit, Subject);
            //return Math.Abs(unit.AngleTo(Subject.Position)-unit.RotationAngle);
        }
        /// <summary>
        /// defines if Subject is in shooting radius of this unit
        /// </summary>
        /// <param name="Subject">unit to check</param>
        /// <returns>true if Subject is in shooting radius of this unit</returns>
        public bool IsInThisShootRadius(IUnit Subject)
        {
            return GameVector.DistanceSquared(unit.Position, Subject.Position) < unit.ShootingRadius * unit.ShootingRadius;
        }
        /// <summary>
        /// defines if this unit is in shooting radius of the Subject unit
        /// </summary>
        /// <param name="Subject">unit to check</param>
        /// <returns>true if this unit is in shooting radius of Subject</returns>
        public bool IsInHisShootRadius(IUnit Subject)
        {
            return GameVector.DistanceSquared(unit.Position, Subject.Position) < Subject.ShootingRadius * Subject.ShootingRadius;
        }
        //TESTED
        /// <summary>
        /// calculates nearest toshooting line enemy
        /// searching minimum angle difference between direction to unit and forward direction
        /// </summary>
        /// <returns>nearest unit</returns>
        public IUnit NearestToShootingLineEnemy()
        {
            float minAngleDifference = float.PositiveInfinity;
            float currAngleDifference;
            IUnit res = null;
            radar.ResetIterator();
            while (radar.NextEnemy())
            {
                if (!radar.CurrUnit.Dead && IsInThisShootRadius(radar.CurrUnit))
                {
                    currAngleDifference = TargetingMistake(radar.CurrUnit);
                    if (currAngleDifference < minAngleDifference)
                    {
                        minAngleDifference = currAngleDifference;
                        res = radar.CurrUnit;
                    }
                }
            }
            return res;
        }
        ///TESTED. but needs weeker (than presised) condition for positive rezult
        /// <summary>
        /// predicts hit to friend before shoot will hit specified enemy
        /// use this method only if enemy is on the shooting line, e.g. TargetingMistake(enemy) is small (0.04 for example)
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        internal bool PredictFriendlyFire(IUnit enemy)
        {
            Line shootingLine = ShootingLineOf(unit);
            float distToEnemySq = GameVector.DistanceSquared(unit.Position, enemy.Position);
            float currDistSq;
            radar.ResetIterator();
            while (radar.NextFriend())
            {
                if (radar.CurrUnit != unit)
                {
                    if (radar.CurrUnit.IntersectsSector(shootingLine.pt1, shootingLine.pt2))
                    {
                        currDistSq = GameVector.DistanceSquared(unit.Position, radar.CurrUnit.Position);
                        if (currDistSq < distToEnemySq)
                            return true;
                    }
                }
            }
            return false;
        }
        #endregion
        //TESTED
        /// <summary>
        /// gets line that starts with unit's position and ends on the shootingRadius distance just forward to unit
        /// </summary>
        public Line ShootingLine
        {
            get
            {
                return ShootingLineOf(unit);
            }
        }
        //TESTED
        /// <summary>
        /// gets line that starts with unit's position and ends on the shootingRadius distance just forward to unit
        /// </summary>
        /// <param name="unit">unit to use</param>
        /// <returns>shooting line</returns>
        public static Line ShootingLineOf(IUnit unit)
        {
            return new Line(unit.Position, unit.Position + unit.Forward * unit.ShootingRadius);
        }
        /// <summary>
        /// gets list of enemies that are turned to this unit
        /// </summary>
        /// <returns>List of units</returns>
        public List<IUnit> GetThreateningUnits()
        {
            List<IUnit> res = new List<IUnit>();
            radar.ResetIterator();
            while (radar.NextEnemy())
            {
                if (Math.Abs(radar.CurrUnit.AngleTo(unit.Position) - radar.CurrUnit.RotationAngle) < AngleClass.pi / 180 * 5)
                { res.Add(radar.CurrUnit); }
            }

            return res;
        }
        /// <summary>
        /// Determines whether unit is threated by enemy.
        /// Threating means to be turned to this unit and to be in shooting radius
        /// </summary>
        /// <param name="enemy">enemy to check</param>
        /// <returns>true if specified enemy is threating this unit</returns>
        public bool IsThreatedBy(IUnit enemy)
        {
            return AngleClass.Distance(enemy.RotationAngle,enemy.Position.AngleTo(unit.Position))<AngleClass.pi / 180 * 5;            
        }
    }
}
