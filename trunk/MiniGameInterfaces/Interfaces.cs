using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MiniGameInterfaces
{
    public struct Color
    {
        public float r,g,b,a;
        public Color(float R,float G,float B,float A)
        {
            r=R;
            g=G;
            b=B;
            a = A;
        }
        public static Color Red
        {
            get
            {
                return new Color(1, 0, 0, 1);
            }
        }
        public static Color Green
        {
            get
            {
                return new Color(0, 1, 0, 1);
            }
        }
        public static Color Blue
        {
            get
            {
                return new Color(0, 0, 1, 1);
            }
        }
        public static Color Black
        {
            get
            {
                return new Color(0, 0, 0, 1);
            }
        }
        public static Color Zero
        {
            get
            {
                return new Color(0, 0, 0, 0);
            }
        }
        public static Color White
        {
            get
            {
                return new Color(1, 1, 1, 1);
            }
        }
    }
    public interface IDebug
    {
        void DrawRectangle(Rectangle Rectangle,Color Color);
        void DrawCircle(Circle Circle, Color Color);
        void DrawPoint(GameVector Vector, Color Color);
        void DrawLine(Line Line,Color Color);
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
        public static GameVector Rotate(GameVector Vector,float Angle)
        {
            return new GameVector((float)(Vector.X * Math.Cos(Angle) - Vector.Y * Math.Sin(Angle)), (float)(Vector.X * Math.Sin(Angle) + Vector.Y*Math.Cos(Angle)));
        }
        public GameVector Rotate(float Angle)
        {
            return new GameVector((float)(X * Math.Cos(Angle) - Y * Math.Sin(Angle)), (float)(X * Math.Sin(Angle) + Y * Math.Cos(Angle)));
        }
        public GameVector Normalize()
        {
            return this / this.Length();
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
        /// <summary>
        /// provides drawing graphic primitives
        /// </summary>
        IDebug GeometryViewer { get; }
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
    public struct Line
    {
        public GameVector pt1, pt2;
        public Line(GameVector Start, GameVector End)
        {
            pt1 = Start;
            pt2 = End;
        }
        //public bool IntersectsLine(Line Line2, out GameVector intersection)
        //{
        //    float t1 = ((Line2.pt1.Y - pt1.Y) * (Line2.pt2.X - Line2.pt1.X) + (Line2.pt2.Y - Line2.pt1.Y) * (pt1.X - Line2.pt1.X))
        //        / ((pt2.Y - pt1.Y) * (Line2.pt2.X - Line2.pt1.X) - (Line2.pt2.Y - Line2.pt1.Y) * (pt2.X - pt1.X));
        //    float t2 = (pt1.X - Line2.pt1.X) / (Line2.pt2.X - Line2.pt1.X) + (pt2.X - pt1.X) / (Line2.pt2.X - Line2.pt1.X) * t1;
        //    if (float.IsNaN(t1 + t2))
        //    {
        //        t1 = ((Line2.pt1.X - pt1.X) * (Line2.pt2.Y - Line2.pt1.Y) + (Line2.pt2.X - Line2.pt1.X) * (pt1.Y - Line2.pt1.Y))
        //        / ((pt2.X - pt1.X) * (Line2.pt2.Y - Line2.pt1.Y) - (Line2.pt2.X - Line2.pt1.X) * (pt2.Y - pt1.Y));
        //        t2 = (pt1.Y - Line2.pt1.Y) / (Line2.pt2.Y - Line2.pt1.Y) + (pt2.Y - pt1.Y) / (Line2.pt2.Y - Line2.pt1.Y) * t1;
        //    }
        //    intersection = pt1 + (pt2 - pt1) * t1;
        //    return (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1);

        //}
        private const float eps = 10e-4f;
        public static bool LinesIntersection(GameVector Start1, GameVector End1, GameVector Start2, GameVector End2, out GameVector Intersection)
        {
            //forwardLeft+(forwardRight-forwardLeft)*t1=backRight+(backLeft-backRight)*t2
            //t2=(forwardLeft.x-backRight.x)/(backLeft.x-backRight.x)+(forwardRight.x-forwardLeft.x)/(backLeft.x-backRight.x)*t1
            //forwardLeft.y+(forwardRight.y-forwardLeft.y)*t1=backRight.y+(backLeft.y-backRight.y)*((forwardLeft.x-backRight.x)/(backLeft.x-backRight.x)+(forwardRight.x-forwardLeft.x)/(backLeft.x-backRight.x)*t1)
            //((forwardRight.y-forwardLeft.y)- (backLeft.y-backRight.y)*(forwardRight.x-forwardLeft.x)/(backLeft.x-backRight.x))*t1=backRight.y-forwardLeft.y+(backLeft.y-backRight.y)*(forwardLeft.x-backRight.x)/(backLeft.x-backRight.x) 
            float UpperFractionTerm = (Start2.Y - Start1.Y) * (End2.X - Start2.X) + (End2.Y - Start2.Y) * (Start1.X - Start2.X);
            float LowerFractionTerm = (End1.Y - Start1.Y) * (End2.X - Start2.X) - (End2.Y - Start2.Y) * (End1.X - Start1.X);
            float t1 = UpperFractionTerm / LowerFractionTerm;
            float t2 = ((Start1.X - Start2.X) + (End1.X - Start1.X) * t1) / (End2.X - Start2.X);

            if (float.IsNaN(t1 + t2))
            {
                //I will not calculate intersection in this shity case. Let it be
                Intersection = GameVector.Zero;

                if (Math.Abs(UpperFractionTerm) < eps)//parallel and on one line
                {
                    if (Math.Max(Start1.X, End1.X) < Math.Min(Start2.X, End2.X)) return false;
                    if (Math.Min(Start1.X, End1.X) > Math.Max(Start2.X, End2.X)) return false;
                    if (Math.Max(Start1.Y, End1.Y) < Math.Min(Start2.Y, End2.Y)) return false;
                    if (Math.Min(Start1.Y, End1.Y) > Math.Max(Start2.Y, End2.Y)) return false;
                    return true;
                }
                else return false;//parallel but not on one line
                //t1 = ((Start2.X - Start1.X) * (End2.Y - Start2.Y) + (End2.X - Start2.X) * (Start1.Y - Start2.Y))
                /// ((End1.X - Start1.X) * (End2.Y - Start2.Y) - (End2.X - Start2.X) * (End1.Y - Start1.Y));
                //t2 = (Start1.Y - Start2.Y) / (End2.Y - Start2.Y) + (End1.Y - Start1.Y) / (End2.Y - Start2.Y) * t1;

            }
            Intersection = Start1 + (End1 - Start1) * t1;
            return (t1 > -eps && t1 < 1 + eps && t2 > -eps && t2 < 1 + eps);
        }
        public float Length()
        {
            return (pt1 - pt2).Length();
        }

    }
    public struct Circle
    {
        GameVector center;
        float radius;
        float radiusSq;
        public GameVector Center
        {
            get { return center; }
        }
        public float Radius
        {
            get { return radius; }
        }
        public Circle(GameVector Center, float Radius)
        {
            center = Center;
            radius = Radius;
            radiusSq = radius * radius;
        }
        public bool Intersects(Line Line)
        {
            GameVector CenterPt1 = center - Line.pt1;
            GameVector CenterPt2 = center - Line.pt2;
            GameVector Pt2Pt1 = Line.pt2 - Line.pt1;
            float Pt2Pt1LengthSq = Pt2Pt1.LengthSquared();
            float CenterPt1LengthSq = CenterPt1.LengthSquared();


            if (CenterPt1LengthSq<= radiusSq) return true;
            if (CenterPt2.LengthSquared()<= radiusSq) return true;

            float dot=GameVector.Dot(Pt2Pt1, CenterPt1);

            if (dot < 0 || dot > Pt2Pt1LengthSq) return false;


            return Pt2Pt1LengthSq*(CenterPt1LengthSq-radiusSq)<=dot*dot;
            
        }
        public bool Intersects(Circle Circle)
        {
            return GameVector.DistanceSquared(center, Circle.center) < (radius + Circle.radius) * (radius + Circle.radius);
        }
    }
    public struct Rectangle
    {
        GameVector forwardLeft, forwardRight, backRight, backLeft;
        private GameVector center, forward, size;
        public GameVector ForwardLeft
        {get{return forwardLeft;}}
        public GameVector Pt2
        { get { return forwardRight; } }
        public GameVector Pt3
        { get { return backRight; } }
        public GameVector Pt4
        { get { return backLeft; } }
        public GameVector Center
        {
            get
            {
                return center;
            }
        }
        public GameVector Size
        {
            get
            {
                return size;
            }
        }
        public GameVector Forward
        {
            get
            {
                return forward;
            }
        }
        public float Angle
        {
            get
            {
                GameVector forward=forwardLeft-backLeft;
                return (float)Math.Atan2(forward.Y, forward.X);
            }
        }
        /// <summary>
        /// obsolete. Delete when ensure in another constructor working clear
        /// </summary>
        /// <param name="ForwardLeft"></param>
        /// <param name="ForwardRight"></param>
        /// <param name="BackRight"></param>
        /// <param name="BackLeft"></param>
        public Rectangle(GameVector ForwardLeft, GameVector ForwardRight, GameVector BackRight, GameVector BackLeft)
        {
            this.forwardLeft = ForwardLeft;
            this.forwardRight = ForwardRight;
            this.backRight = BackRight;
            this.backLeft = BackLeft;
            center = GameVector.Zero;
            size = GameVector.Zero;
            forward = GameVector.Zero;
         
        }
        public Rectangle(GameVector Center, GameVector Size, GameVector Forward)
        {
            Forward = Forward.Normalize();
            GameVector right = new GameVector(Forward.Y, -Forward.X);
            Forward *= Size.X * 0.5f;
            right *= Size.Y * 0.5f;
            this.forwardLeft = Forward - right + Center;
            this.forwardRight = Forward + right + Center;
            this.backRight = -Forward + right + Center;
            this.backLeft = -Forward - right + Center;
            center = Center;
            size = Size;
            forward = Forward;
        }
        public Circle GetBoundingCircle
        {
            get
            {
                GameVector min = new GameVector(Math.Min(Math.Min(forwardLeft.X, forwardRight.X), Math.Min(backRight.X, backLeft.X)),
                    Math.Min(Math.Min(forwardLeft.Y, forwardRight.Y), Math.Min(backRight.Y, backLeft.Y)));
                GameVector max = new GameVector(Math.Max(Math.Max(forwardLeft.X, forwardRight.X), Math.Max(backRight.X, backLeft.X)),
                    Math.Max(Math.Max(forwardLeft.Y, forwardRight.Y), Math.Max(backRight.Y, backLeft.Y)));
                GameVector center = (min + max) * 0.5f;
                return new Circle(center, GameVector.Distance(center, min));
            }
        }       
        
        public bool IntersectsLine(GameVector Start, GameVector End, out GameVector Intersection)
        {
            Intersection = GameVector.One * float.PositiveInfinity;
            bool res = false;
            GameVector currIntersection;
            if (Line.LinesIntersection(forwardLeft, forwardRight, Start, End, out currIntersection))
            { Intersection = currIntersection; res = true; }
            if (Line.LinesIntersection(forwardRight, backRight, Start, End, out currIntersection))
            {
                if (GameVector.DistanceSquared(Start, currIntersection) < GameVector.DistanceSquared(Start, Intersection))
                    Intersection = currIntersection;
                res = true;
            }
            if (Line.LinesIntersection(backRight, backLeft, Start, End, out currIntersection))
            {
                if (GameVector.DistanceSquared(Start, currIntersection) < GameVector.DistanceSquared(Start, Intersection))
                    Intersection = currIntersection;
                res = true;
            }
            if (Line.LinesIntersection(backLeft, forwardLeft, Start, End, out currIntersection))
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
            if (Line.LinesIntersection(forwardLeft, forwardRight, Start, End, out currIntersection))
            { return true; }
            if (Line.LinesIntersection(forwardRight, backRight, Start, End, out currIntersection))
            { return true; }
            if (Line.LinesIntersection(backRight, backLeft, Start, End, out currIntersection))
            { return true; }
            if (Line.LinesIntersection(backLeft, forwardLeft, Start, End, out currIntersection))
            { return true; }
            return false;
        }
        public bool IntersectsRectangle(Rectangle AnotherRect)
        {
            if (IntersectsLine(AnotherRect.forwardLeft, AnotherRect.forwardRight)) return true;
            if (IntersectsLine(AnotherRect.forwardRight, AnotherRect.backRight)) return true;
            if (IntersectsLine(AnotherRect.backRight, AnotherRect.backLeft)) return true;
            if (IntersectsLine(AnotherRect.backLeft, AnotherRect.forwardLeft)) return true;
            return false;
        }
    }
}
