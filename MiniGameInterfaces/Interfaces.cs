using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MiniGameInterfaces
{
    public struct Color
    {
        public float r,g,b;
        public Color(float R,float G,float B)
        {
            r=R;
            g=G;
            b=B;
        }
        
    }
    public interface IDebug
    {
        void DrawRectangle(Rectangle Rectangle,Color Color);
        void DrawCircle(Circle Circle, Color Color);
        void DrawPoint(GameVector Vector, Color Color);
        void DrawLine(Stretch Line);
    }
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
        public static GameVector operator -(GameVector vec)
        {
            return new GameVector(-vec.X, -vec.Y);
        }
        public static GameVector One
        {
            get
            {
                return new GameVector(1, 1);
            }
        }
        public static GameVector Zero
        {
            get
            {
                return new GameVector(0, 0);
            }
        }
        public static GameVector UnitX
        {
            get
            {
                return new GameVector(1, 0);
            }
        }
        public static GameVector UnitY
        {
            get
            {
                return new GameVector(0, 1);
            }
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
        public GameVector Rotate(GameVector Vector,float Angle)
        {
            return new GameVector((float)(Math.Cos(Vector.X) - Math.Sin(Vector.Y)),(float)( Math.Sin(Vector.X) + Math.Cos(Vector.Y)));
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
    public struct Stretch
    {
        public GameVector pt1, pt2;
        public Stretch(GameVector Start, GameVector End)
        {
            pt1 = Start;
            pt2 = End;
        }
        public bool IntersectsStretch(Stretch Stretch2, out GameVector intersection)
        {
            float t1 = ((Stretch2.pt1.Y - pt1.Y) * (Stretch2.pt2.X - Stretch2.pt1.X) + (Stretch2.pt2.Y - Stretch2.pt1.Y) * (pt1.X - Stretch2.pt1.X))
                / ((pt2.Y - pt1.Y) * (Stretch2.pt2.X - Stretch2.pt1.X) - (Stretch2.pt2.Y - Stretch2.pt1.Y) * (pt2.X - pt1.X));
            float t2 = (pt1.X - Stretch2.pt1.X) / (Stretch2.pt2.X - Stretch2.pt1.X) + (pt2.X - pt1.X) / (Stretch2.pt2.X - Stretch2.pt1.X) * t1;
            if (float.IsNaN(t1 + t2))
            {
                t1 = ((Stretch2.pt1.X - pt1.X) * (Stretch2.pt2.Y - Stretch2.pt1.Y) + (Stretch2.pt2.X - Stretch2.pt1.X) * (pt1.Y - Stretch2.pt1.Y))
                / ((pt2.X - pt1.X) * (Stretch2.pt2.Y - Stretch2.pt1.Y) - (Stretch2.pt2.X - Stretch2.pt1.X) * (pt2.Y - pt1.Y));
                t2 = (pt1.Y - Stretch2.pt1.Y) / (Stretch2.pt2.Y - Stretch2.pt1.Y) + (pt2.Y - pt1.Y) / (Stretch2.pt2.Y - Stretch2.pt1.Y) * t1;
            }
            intersection = pt1 + (pt2 - pt1) * t1;
            return (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1);
        }
    }
    public struct Circle
    {
        GameVector center;
        float radius;
        public Circle(GameVector Center, float Radius)
        {
            center = Center;
            radius = Radius;
        }

        public bool Intersects(Stretch Stretch)
        {
            GameVector perpendicular = new GameVector();

            GameVector perpendicularBasis;
            if (!Stretch.IntersectsStretch(new Stretch(center, center + perpendicular / perpendicular.Length() * radius),
                out perpendicularBasis))
                return false;
            else
            {
                return GameVector.DistanceSquared(center, perpendicularBasis) < radius * radius;
            }
        }
        public bool Intersects(Circle Sphere)
        {
            return GameVector.DistanceSquared(center, Sphere.center) < (radius + Sphere.radius) * (radius + Sphere.radius);
        }
    }
    public struct Rectangle
    {
        public GameVector pt1, pt2, pt3, pt4;
        public Rectangle(GameVector pt1, GameVector pt2, GameVector pt3, GameVector pt4)
        {
            this.pt1 = pt1;
            this.pt2 = pt2;
            this.pt3 = pt3;
            this.pt4 = pt4;
        }

        public Circle GetSphere
        {
            get
            {
                GameVector min = new GameVector(Math.Min(Math.Min(pt1.X, pt2.X), Math.Min(pt3.X, pt4.X)),
                    Math.Min(Math.Min(pt1.Y, pt2.Y), Math.Min(pt3.Y, pt4.Y)));
                GameVector max = new GameVector(Math.Max(Math.Max(pt1.X, pt2.X), Math.Max(pt3.X, pt4.X)),
                    Math.Max(Math.Max(pt1.Y, pt2.Y), Math.Max(pt3.Y, pt4.Y)));
                GameVector center = (min + max) * 0.5f;
                return new Circle(center, GameVector.Distance(center, min));
            }
        }
        static bool LinesIntersection(GameVector Start1, GameVector End1, GameVector Start2, GameVector End2, out GameVector Intersection)
        {
            //pt1+(pt2-pt1)*t1=pt3+(pt4-pt3)*t2
            //t2=(pt1.x-pt3.x)/(pt4.x-pt3.x)+(pt2.x-pt1.x)/(pt4.x-pt3.x)*t1
            //pt1.y+(pt2.y-pt1.y)*t1=pt3.y+(pt4.y-pt3.y)*((pt1.x-pt3.x)/(pt4.x-pt3.x)+(pt2.x-pt1.x)/(pt4.x-pt3.x)*t1)
            //((pt2.y-pt1.y)- (pt4.y-pt3.y)*(pt2.x-pt1.x)/(pt4.x-pt3.x))*t1=pt3.y-pt1.y+(pt4.y-pt3.y)*(pt1.x-pt3.x)/(pt4.x-pt3.x) 
            float t1 = ((Start2.Y - Start1.Y) * (End2.X - Start2.X) + (End2.Y - Start2.Y) * (Start1.X - Start2.X))
                / ((End1.Y - Start1.Y) * (End2.X - Start2.X) - (End2.Y - Start2.Y) * (End1.X - Start1.X));
            float t2 = (Start1.X - Start2.X) / (End2.X - Start2.X) + (End1.X - Start1.X) / (End2.X - Start2.X) * t1;
            if (float.IsNaN(t1 + t2))
            {
                t1 = ((Start2.X - Start1.X) * (End2.Y - Start2.Y) + (End2.X - Start2.X) * (Start1.Y - Start2.Y))
                / ((End1.X - Start1.X) * (End2.Y - Start2.Y) - (End2.X - Start2.X) * (End1.Y - Start1.Y));
                t2 = (Start1.Y - Start2.Y) / (End2.Y - Start2.Y) + (End1.Y - Start1.Y) / (End2.Y - Start2.Y) * t1;
            }
            Intersection = Start1 + (End1 - Start1) * t1;
            return (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1);
        }
        public bool IntersectsLine(GameVector Start, GameVector End, out GameVector Intersection)
        {
            Intersection = GameVector.One * float.PositiveInfinity;
            bool res = false;
            GameVector currIntersection;
            if (LinesIntersection(pt1, pt2, Start, End, out currIntersection))
            { Intersection = currIntersection; res = true; }
            if (LinesIntersection(pt2, pt3, Start, End, out currIntersection))
            {
                if (GameVector.DistanceSquared(Start, currIntersection) < GameVector.DistanceSquared(Start, Intersection))
                    Intersection = currIntersection;
                res = true;
            }
            if (LinesIntersection(pt3, pt4, Start, End, out currIntersection))
            {
                if (GameVector.DistanceSquared(Start, currIntersection) < GameVector.DistanceSquared(Start, Intersection))
                    Intersection = currIntersection;
                res = true;
            }
            if (LinesIntersection(pt4, pt1, Start, End, out currIntersection))
            {
                if (GameVector.DistanceSquared(Start, currIntersection) < GameVector.DistanceSquared(Start, Intersection))
                    Intersection = currIntersection;
                res = true;
            }
            return res;
        }
        public bool IntersectsLine(GameVector Start, GameVector End)
        {
            GameVector currIntersection;
            if (LinesIntersection(pt1, pt2, Start, End, out currIntersection))
            { return true; }
            if (LinesIntersection(pt2, pt3, Start, End, out currIntersection))
            { return true; }
            if (LinesIntersection(pt3, pt4, Start, End, out currIntersection))
            { return true; }
            if (LinesIntersection(pt4, pt1, Start, End, out currIntersection))
            { return true; }
            return false;
        }
        public bool IntersectsRectangle(Rectangle AnotherRect)
        {
            if (IntersectsLine(AnotherRect.pt1, AnotherRect.pt2)) return true;
            if (IntersectsLine(AnotherRect.pt2, AnotherRect.pt3)) return true;
            if (IntersectsLine(AnotherRect.pt3, AnotherRect.pt4)) return true;
            if (IntersectsLine(AnotherRect.pt4, AnotherRect.pt1)) return true;
            return false;
        }
    }
}
