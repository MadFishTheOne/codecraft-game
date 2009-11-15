using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
namespace AINamespace
{
    /// <summary>
    /// defines predicates for pilot using radar and unit position
    /// </summary>
    public class OnBoardComputer
    {
        Radar radar;
        public Radar Radar
        { get { return radar; } }
        IUnit unit;
        public OnBoardComputer(Radar Radar,IUnit Unit)
        { radar = Radar; unit = Unit; WeakAvoidingObstacles = false; }
        #region collizion prediction
        public bool WeakAvoidingObstacles;
        public struct CollizionPredictionInfo
        {
            public bool CollizionExpected;
            public GameVector CollizionPt;
            public GameVector ObstacleCenter;
            public float DistToObstacle;
        }
        public CollizionPredictionInfo CollizionPredictionCheck(bool EnemiesConsidered)
        {
            

            CollizionPredictionInfo res;
            res.CollizionPt =GameVector.One*float.PositiveInfinity;
            res.ObstacleCenter = GameVector.One * float.PositiveInfinity;
            res.CollizionExpected= false;
            res.DistToObstacle = float.PositiveInfinity;
            


            Rectangle selfMoveVolume = CreateMoveVolume(unit);
            Rectangle obstacleMoveVolume;

            if (EnemiesConsidered)
            {
                foreach (IUnit enemyUnit in radar.Enemies)
                {
                    obstacleMoveVolume = CreateMoveVolume(enemyUnit);
                    //if (GameVector.Distance(unit.Position, new GameVector(-1871, 304)) < 5)
                    //{
                  //  if (unit.Text == "1")
                   //     radar.Game.GeometryViewer.DrawRectangle(obstacleMoveVolume, Color.Blue);
                    //}
                    if (selfMoveVolume.IntersectsRectangle(obstacleMoveVolume))
                    {
                        if ((unit.Position - enemyUnit.Position).LengthSquared()
                            < (unit.Position - res.ObstacleCenter).LengthSquared())
                        {


                            if (GameVector.Dot(unit.Forward, enemyUnit.Position - unit.Position) > 0)
                            {
                                res.CollizionPt = obstacleMoveVolume.Center;
                                res.ObstacleCenter = enemyUnit.Position;
                                res.CollizionExpected = true;
                            }
                        }
                    }
                }
            }           
            foreach (IUnit friendUnit in radar.Friends)
            {
                obstacleMoveVolume = CreateMoveVolume(friendUnit);
                if (selfMoveVolume.IntersectsRectangle(obstacleMoveVolume))
                {
                    
                    if ((unit.Position - friendUnit.Position).LengthSquared()
                        < (unit.Position - res.ObstacleCenter).LengthSquared())
                    {
                        
                        if (GameVector.Dot(unit.Forward, friendUnit.Position - unit.Position) > 0)
                        {
                            res.CollizionPt = obstacleMoveVolume.Center;
                            res.ObstacleCenter = friendUnit.Position;
                            res.CollizionExpected = true;
                        }
                    }
                }
            }
            res.DistToObstacle = GameVector.Distance(res.ObstacleCenter,unit.Position);
            //if (unit.Text == "1")
            {
                //Color col = res.CollizionExpected ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.5f);
                //radar.Game.GeometryViewer.DrawRectangle(selfMoveVolume, col);
                //DrawStopWay();

            }

            return res;
        }
        //DEBUG
        public void DrawStopWay()
        {
            radar.Game.GeometryViewer.DrawLine(new Line(unit.Position, unit.Position + unit.Forward *
                StopDistance), Color.Blue);
        }
        /// <summary>
        /// distance of stopping if full deaccelerating used
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
        public Rectangle CreateMoveVolume(IUnit unit)
        {
            float angle = unit.RotationSpeed*0.3f;
            float speed = unit.Speed;
            float currMoveVolumeForward =(WeakAvoidingObstacles)? moveVolumeForward*0.5f:moveVolumeForward;

//            if (currMoveVolumeForward < StopDistance) { currMoveVolumeForward = StopDistance; }            
            
            GameVector forward = unit.Forward.Rotate(angle);
            GameVector center = unit.Position + forward * speed * currMoveVolumeForward * 0.5f;
            return new Rectangle(center,
                new GameVector(unit.Size.X * moveVolumeScale + speed * currMoveVolumeForward, unit.Size.Y * moveVolumeScale),
                forward);
        }
        #endregion
        #region other predicates
        static public bool TargetedAt(IUnit Object, IUnit Subject)
        {
            Line shotLine = new Line(Object.Position, Object.Position + Object.Forward * Object.ShootingRadius);
            return Subject.GetRectangle().IntersectsLine(shotLine.pt1, shotLine.pt2);
        }
        public bool TargetedAt(IUnit Subject)
        {
            return OnBoardComputer.TargetedAt(unit, Subject);
        }

        static public float TargetingMistake(IUnit Object, IUnit Subject)
        {
            return Math.Abs(Object.AngleTo(Subject.Position) - Object.RotationAngle);
        }
        //TESTED
        /// <summary>
        /// calculates absolute targeting mistake in radians
        /// </summary>
        /// <param name="Subject"></param>
        /// <returns></returns>
        public float TargetingMistake(IUnit Subject)
        {
            return TargetingMistake(unit, Subject);
            //return Math.Abs(unit.AngleTo(Subject.Position)-unit.RotationAngle);
        }
        public bool IsInThisShootRadius(IUnit Subject)
        {
            return GameVector.DistanceSquared(unit.Position, Subject.Position) < unit.ShootingRadius * unit.ShootingRadius;
        }
        public bool IsInHisShootRadius(IUnit Subject)
        {
            return GameVector.DistanceSquared(unit.Position, Subject.Position) < Subject.ShootingRadius * Subject.ShootingRadius;
        }
        //TESTED
        public IUnit NearestToShootingLineEnemy()
        {
            float minAngleDifference = float.PositiveInfinity;
            float currAngleDifference;
            IUnit res = null;
            foreach (IUnit enemy in radar.Enemies)
                if (!enemy.Dead&&IsInThisShootRadius(enemy)) 
            {                
                currAngleDifference = TargetingMistake(enemy);
                if (currAngleDifference < minAngleDifference)
                {
                    minAngleDifference = currAngleDifference;
                    res = enemy;
                }
            }
            return res;
        }
        ///TESTED. but needs weeker (than presised) condition for positive rezult
        /// <summary>
        /// predicts hit to friend before shoot will hit specified enemy(use only if enemy is on the shooting line)
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public bool PredictFriendlyFire(IUnit enemy)
        {
            Line shootingLine = ShootingLineOf(unit);
            float distToEnemySq = GameVector.DistanceSquared(unit.Position, enemy.Position);
            float currDistSq;
            foreach (IUnit friend in radar.Friends)
                if (friend!=unit)
            {
                    if (friend.IntersectsSector(shootingLine.pt1,shootingLine.pt2))
                    {
                        currDistSq = GameVector.DistanceSquared(unit.Position, friend.Position);
                        if (currDistSq < distToEnemySq)
                        return true;
                    }
            }
            return false;
        }
        #endregion
        //TESTED
        public Line ShootingLine
        {
            get
            {
                return ShootingLineOf(unit);
            }
        }
        //TESTED
        public static Line ShootingLineOf(IUnit unit)
        {
            return new Line(unit.Position, unit.Position + unit.Forward * unit.ShootingRadius);
        }
        //gets list of enemies that are looking to this unit
        public List<IUnit> GetThreateningUnits()
        {
            List<IUnit> res = new List<IUnit>();
            foreach (IUnit enemy in radar.Enemies)
            {
                if (Math.Abs(enemy.AngleTo(unit.Position)-enemy.RotationAngle) < AngleClass.pi / 180 * 5)
                { res.Add(enemy); }
            }
            return res;
        }
    }
}
