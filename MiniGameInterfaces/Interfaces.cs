using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniGameInterfaces
{
    public enum ShipTypes
    {
        Destroyer, Corvette, Cruiser
    }
    public struct GameVector
    {
        public float X;
        public float Y;
        public GameVector(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public static GameVector operator +(GameVector pt1, GameVector pt2)
        {
            return new GameVector(pt1.X + pt2.X, pt1.Y + pt2.Y);
        }
        public static GameVector operator -(GameVector pt1, GameVector pt2)
        {
            return new GameVector(pt1.X - pt2.X, pt1.Y - pt2.Y);
        }
        public static GameVector operator *(GameVector pt1, float op2)
        {
            return new GameVector(pt1.X * op2, pt1.Y * op2);
        }
        public static GameVector operator /(GameVector pt1, float op2)
        {
            return new GameVector(pt1.X / op2, pt1.Y / op2);
        }
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
        public float LengthSquared()
        {
            return X * X + Y * Y;
        }
        public float Angle()
        {
            return (float)Math.Atan2(Y, X);
        }
        public static GameVector Normalize(GameVector pt)
        {
            return pt / pt.Length();
        }
        public static float Dot(GameVector pt1, GameVector pt2)
        {
            return pt1.X * pt2.X + pt1.Y * pt2.Y;
        }
        public static float Cos(GameVector pt1, GameVector pt2)
        {
            return (float)(Dot(pt1, pt2) / Math.Sqrt(pt1.LengthSquared() * pt2.LengthSquared()));
        }
        public static float Distance(GameVector pt1, GameVector pt2)
        {
            return (pt2 - pt1).Length();
        }
        public static float DistanceSquared(GameVector pt1, GameVector pt2)
        {
            return (pt2 - pt1).LengthSquared();
        }

        
    }
    public interface IUnit
    {
        #region Getting unit state
        /// <summary>
        /// unit name
        /// </summary>
        string Name { get; }
        /// <summary>
        /// position in the world
        /// </summary>
        GameVector Position { get; }
        /// <summary>
        /// current speed
        /// </summary>
        float Speed { get; }
        /// <summary>
        /// looking direction. unit vector
        /// </summary>
        GameVector Forward { get; }
        /// <summary>
        /// time to recharge gun in seconds
        /// </summary>
        float TimeToRecharge { get; }
        /// <summary>
        /// rotation angle in radians
        /// </summary>
        float RotationAngle { get; }
        /// <summary>
        /// rotation speed in radians per second
        /// </summary>
        float RotationSpeed { get; }
        /// <summary>
        /// player owner
        /// </summary>
        int PlayerOwner { get; }
        /// <summary>
        /// unit current hit points 
        /// </summary>
        float HP { get; }
        /// <summary>
        /// is unit already dead
        /// </summary>
        bool Dead { get; }
        /// <summary>
        /// defines intersection with given sector
        /// </summary>
        /// <param name="pt1">sector vertex</param>
        /// <param name="pt2">sector vertex</param>
        /// <returns>true if ship intersects vector</returns>
        bool IntersectsSector(GameVector pt1, GameVector pt2);        
        #endregion
        #region Getting unit characteristics
        /// <summary>
        /// get's ship type - cruiser, corvette or destroyer
        /// </summary>
        ShipTypes ShipType { get; }
        /// <summary>
        /// in pixels. X is a width, Y is a length
        /// </summary>
        GameVector Size { get; }
        /// <summary>
        /// in pixels per seconds
        /// </summary>
        float MaxSpeed { get; }
        /// <summary>
        /// in pixels per square seconds
        /// </summary>
        float MaxSpeedAcceleration { get; }
        /// <summary>
        /// in radians per second
        /// </summary>
        float MaxRotationSpeed { get; }
        /// <summary>
        /// in radians per square second
        /// </summary>
        float MaxRotationAcceleration { get; }
        /// <summary>
        /// gun delay in seconds
        /// </summary>
        float DelayTime { get; }
        /// <summary>
        /// gun damage in HP
        /// </summary>
        float Damage { get; }
        /// <summary>
        /// shooting radius of unit's gun
        /// </summary>
        float ShootingRadius { get; }
        /// <summary>
        /// gets an angle to target vector
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        float AngleTo(GameVector target);
        #endregion
        #region Controlling the unit
        /// <summary>
        /// accelerate unit by given amount
        /// </summary>
        void Accelerate(float amount);
        /// <summary>
        /// acceleration by max value
        /// </summary>
        void Accelerate();
        /// <summary>
        /// deacceleration by max value
        /// </summary>
        void DeAccelerate();
        /// <summary>
        /// accelerate rotation by given amount
        /// </summary>
        /// <param name="amount"></param>
        void RotationAccelerate(float amount);
        /// <summary>
        /// set constant speed to move with it. unit will reach and hold this speed unitl new acceleration or setting speed command received
        /// </summary>
        void SetSpeed(float Speed);
        /// <summary>
        /// set rotation angle. unit will try to reach this angle with max acceleration and hold it.
        /// </summary>
        void SetAngle(float Angle);
        /// <summary>
        /// unit tries to shoot
        /// </summary>
        /// <returns> true if shoot was provided(if gun was recharged)</returns>
        bool Shoot();
        /// <summary>
        /// makes unit to go to target location
        /// </summary>
        /// <param name="TargetLocation">location to go to</param>
        /// <param name="Stop">true if unit must try to stop there</param>
        void GoTo(GameVector TargetLocation, bool Stop);
        /// <summary>
        /// this damage goes to every unit that was in the blow radius at the blow starting time
        /// </summary>
        float BlowDamage { get; }
        /// <summary>
        /// this radius  is used to draw blow and to damage units with this blow
        /// </summary>            
        float BlowRadius { get; }

        #endregion
    }
    public interface IShot
    {
        /// <summary>
        /// position in the world
        /// </summary>
        GameVector Position { get; }
        /// <summary>
        /// velocity vector
        /// </summary>
        GameVector Direction { get; }
    }
    public interface IGame
    {
        /// <summary>
        /// set displayed text
        /// </summary>
        void SetText(string Text);
        /// <summary>
        /// total number of units
        /// </summary>
        int UnitsCount { get; }
        /// <summary>
        /// get interface of a specific unit
        /// </summary>
        IUnit GetUnit(int Index);
        /// <summary>
        /// total number of shots
        /// </summary>
        int ShotsCount { get; }
        /// <summary>
        /// get interface of a specific shot
        /// </summary>
        IShot GetShot(int Index);
        /// <summary>
        /// gets near units and shots for specified position
        /// </summary>
        /// <param name="Position">position to specify</param>
        /// <param name="Radius">near zone radius</param>
        /// <param name="NearUnits">list of units in the near zone</param>
        /// <param name="NearShots">list of shots in the near zone</param>
        void GetNearUnits(GameVector Position, float Radius, out List<IUnit> NearUnits, out List<IShot> NearShots);
        /// <summary>
        /// total time elapsed from the begginning of the game
        /// </summary>
        float Time { get; }
        /// <summary>
        /// time elapsed from the previous update
        /// </summary>
        float TimeElapsed { get; }
    }
    public interface IAI
    {
        /// <summary>
        /// get AI author
        /// </summary>
        string Author { get; }
        /// <summary>
        /// get AI description
        /// </summary>
        string Description { get; }
        /// <summary>
        /// init AI state, and allow it to access the world
        /// </summary>
        void Init(int PlayerNumber, IGame Game);
        /// <summary>
        /// think, make strategic decisions and control units
        /// </summary>
        void Update();
    }
}
