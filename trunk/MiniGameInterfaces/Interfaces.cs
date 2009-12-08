using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MiniGameInterfaces    
{
    /// <summary>
    /// struct for representing a color, usually used for debug geometry viewing
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// red component, r=[0..1]
        /// </summary>
        public float r;
        /// <summary>
        /// green component, g=[0..1]
        /// </summary>
        public float g;
        /// <summary>
        /// blue component, b=[0..1]
        /// </summary>
            public float b;
            /// <summary>
            /// alpha component, a=[0..1]
            /// </summary>
            public float a;
        /// <summary>
        /// creates new color instance
        /// </summary>
            /// <param name="R">red component,  r=[0..1]</param>
            /// <param name="G">green component,  r=[0..1]</param>
            /// <param name="B">blue component,  r=[0..1]</param>
            /// <param name="A">alpha component,  r=[0..1]</param>
        public Color(float R, float G, float B, float A)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }
        /// <summary>
        /// red color instance
        /// </summary>
        public static Color Red
        {
            get
            {
                return new Color(1, 0, 0, 1);
            }
        }
        /// <summary>
        /// green color instance
        /// </summary>
        public static Color Green
        {
            get
            {
                return new Color(0, 1, 0, 1);
            }
        }
        /// <summary>
        /// blue color instance
        /// </summary>
        public static Color Blue
        {
            get
            {
                return new Color(0, 0, 1, 1);
            }
        }
        /// <summary>
        /// black color instance
        /// </summary>
        public static Color Black
        {
            get
            {
                return new Color(0, 0, 0, 1);
            }
        }
        /// <summary>
        /// zero color instance (black and transparent)
        /// </summary>
        public static Color Zero
        {
            get
            {
                return new Color(0, 0, 0, 0);
            }
        }
        /// <summary>
        /// white color instance
        /// </summary>
        public static Color White
        {
            get
            {
                return new Color(1, 1, 1, 1);
            }
        }
    }
    /// <summary>
    /// interface for drawing custom geometry, usually used for debug
    /// </summary>
    public interface IDebug
    {
        /// <summary>
        /// draws a rectangle
        /// </summary>
        /// <param name="Rectangle">rectangle to draw</param>
        /// <param name="Color">color to use</param>
        void DrawRectangle(Rectangle Rectangle, Color Color);
        /// <summary>
        /// draws a circle
        /// </summary>
        /// <param name="Circle">circle to draw</param>
        /// <param name="Color">color to use</param>
        void DrawCircle(Circle Circle, Color Color);
        /// <summary>
        /// draws a point (as a small circle)
        /// </summary>
        /// <param name="Vector">point position</param>
        /// <param name="Color">color to use</param>
        void DrawPoint(GameVector Vector, Color Color);
        /// <summary>
        /// draws a line (thin rectangle)
        /// </summary>
        /// <param name="Line">line to draw</param>
        /// <param name="Color">color to use</param>
        void DrawLine(Line Line, Color Color);
    }
    /// <summary>
    /// ship types enumeration
    /// </summary>
    public enum ShipTypes
    {
        /// <summary>
        /// fast, light-armed unit, good for using in number and against cruisers
        /// </summary>
        Destroyer, 
        /// <summary>
        /// middle-armed unit with quick-firing weapon, good for using against destroyers
        /// </summary>
        Corvette, 
        /// <summary>
        /// slow, heavy-armed unit with highly destructive weapon, good for using against corvettes
        /// </summary>
        Cruiser
    }
    /// <summary>
    /// struct, representing a vector
    /// </summary>
    public struct GameVector
    {
        /// <summary>
        /// X-component of the vector
        /// </summary>
        public float X;
        /// <summary>
        /// Y-component of the vector
        /// </summary>
        public float Y;
        /// <summary>
        /// creates a new instance of GameVector
        /// </summary>
        /// <param name="X">X-component of the vector</param>
        /// <param name="Y">Y-component of the vector</param>
        public GameVector(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }
        /// <summary>
        /// adds two vectors by components
        /// </summary>
        /// <param name="pt1">first operand</param>
        /// <param name="pt2">second operand</param>
        /// <returns>result vector</returns>
        public static GameVector operator +(GameVector pt1, GameVector pt2)
        {
            return new GameVector(pt1.X + pt2.X, pt1.Y + pt2.Y);
        }
        /// <summary>
        /// substracts two vectors by components
        /// </summary>
        /// <param name="pt1">first operand</param>
        /// <param name="pt2">second operand</param>
        /// <returns>result vector</returns>
        public static GameVector operator -(GameVector pt1, GameVector pt2)
        {
            return new GameVector(pt1.X - pt2.X, pt1.Y - pt2.Y);
        }
        /// <summary>
        /// multiplies vector with float by component
        /// </summary>
        /// <param name="pt1">vector to multiply</param>
        /// <param name="op2">float value to multiply</param>
        /// <returns>result vector</returns>
        public static GameVector operator *(GameVector pt1, float op2)
        {
            return new GameVector(pt1.X * op2, pt1.Y * op2);
        }
        /// <summary>
        /// divides vector by float
        /// </summary>
        /// <param name="pt1">vector to divide</param>
        /// <param name="op2">float to divide on</param>
        /// <returns>result vector</returns>
        public static GameVector operator /(GameVector pt1, float op2)
        {
            return new GameVector(pt1.X / op2, pt1.Y / op2);
        }
        /// <summary>
        /// negates a vector
        /// </summary>
        /// <param name="vec">vector to negate</param>
        /// <returns>negated vector</returns>
        public static GameVector operator -(GameVector vec)
        {
            return new GameVector(-vec.X, -vec.Y);
        }
        /// <summary>
        /// returns instance to GameVector(1,1)
        /// </summary>
        public static GameVector One
        {
            get
            {
                return new GameVector(1, 1);
            }
        }
        /// <summary>
        /// returns instance to GameVector(0,0)
        /// </summary>
        public static GameVector Zero
        {
            get
            {
                return new GameVector(0, 0);
            }
        }
        /// <summary>
        /// returns instance to GameVector(1,0)
        /// </summary>
        public static GameVector UnitX
        {
            get
            {
                return new GameVector(1, 0);
            }
        }
        /// <summary>
        /// returns instance to GameVector(0,1)
        /// </summary>
        public static GameVector UnitY
        {
            get
            {
                return new GameVector(0, 1);
            }
        }
        /// <summary>
        /// finds vector length
        /// </summary>
        /// <returns>float length of the vector</returns>
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }
        /// <summary>
        /// finds vector length square
        /// </summary>
        /// <returns>float length square of the vector</returns>
        public float LengthSquared()
        {
            return X * X + Y * Y;
        }
        /// <summary>
        /// finds vector azimuth in radians, angle between GameVector(1,0) and this gamevector
        /// </summary>
        /// <returns>float azimuth</returns>
        public float Angle()
        {
            return (float)Math.Atan2(Y, X);
        }
        /// <summary>
        /// normalizes a given vector
        /// </summary>
        /// <param name="pt">vector to normalize</param>
        /// <returns>normalized vector</returns>
        public static GameVector Normalize(GameVector pt)
        {
            return pt / pt.Length();
        }
        /// <summary>
        /// finds a dot product of two vectors
        /// </summary>
        /// <param name="pt1">first vector</param>
        /// <param name="pt2">second vector</param>
        /// <returns>float dot product</returns>
        public static float Dot(GameVector pt1, GameVector pt2)
        {
            return pt1.X * pt2.X + pt1.Y * pt2.Y;
        }
        /// <summary>
        /// finds a cos between two vectors (in radians)
        /// </summary>
        /// <param name="pt1">first operand</param>
        /// <param name="pt2">second operand</param>
        /// <returns>float cosine</returns>
        public static float Cos(GameVector pt1, GameVector pt2)
        {
            return (float)(Dot(pt1, pt2) / Math.Sqrt(pt1.LengthSquared() * pt2.LengthSquared()));
        }
        /// <summary>
        /// finds distance between two points
        /// </summary>
        /// <param name="pt1">first operand</param>
        /// <param name="pt2">second operand</param>
        /// <returns>float distance</returns>
        public static float Distance(GameVector pt1, GameVector pt2)
        {
            return (pt2 - pt1).Length();
        }
        /// <summary>
        /// finds distance square between two points
        /// </summary>
        /// <param name="pt1">first operand</param>
        /// <param name="pt2">second operand</param>
        /// <returns>float distance square</returns>
        public static float DistanceSquared(GameVector pt1, GameVector pt2)
        {
            return (pt2 - pt1).LengthSquared();
        }
        /// <summary>
        /// calculates rotated vector
        /// </summary>
        /// <param name="Vector">vector to rotate</param>
        /// <param name="Angle">angle in radians to rotate CCW</param>
        /// <returns>rotated vector</returns>
        public static GameVector Rotate(GameVector Vector, float Angle)
        {
            return new GameVector((float)(Vector.X * Math.Cos(Angle) - Vector.Y * Math.Sin(Angle)), (float)(Vector.X * Math.Sin(Angle) + Vector.Y * Math.Cos(Angle)));
        }
        /// <summary>
        /// rotates vector with specified angle
        /// </summary>
        /// <param name="Angle">angle in radians</param>
        /// <returns>rotated vector</returns>
        public GameVector Rotate(float Angle)
        {
            return new GameVector((float)(X * Math.Cos(Angle) - Y * Math.Sin(Angle)), (float)(X * Math.Sin(Angle) + Y * Math.Cos(Angle)));
        }
        /// <summary>
        /// calculates normalized vector
        /// </summary>
        /// <returns>normalized vector</returns>
        public GameVector Normalize()
        {
            return this / this.Length();
        }
        /// <summary>
        /// Calculates angle direction to Target vector end from this vector end
        /// </summary>
        /// <param name="Target">target vector</param>
        /// <returns>angle to target</returns>
        public float AngleTo(GameVector Target)
        {
            return (Target - this).Angle();
        }
        /// <summary>
        /// Changes vector length to specified value
        /// </summary>
        /// <param name="length">New vector length</param>
        public void SetLength(float length)
        {            
            this *= length / Length();
        }
    }
    /// <summary>
    /// interface for every unit in the game
    /// </summary>
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
        /// <summary>
        /// get's unit geometry
        /// </summary>
        /// <returns></returns>
        Rectangle GetRectangle();
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
        /// <summary>
        /// this damage goes to every unit that was in the blow radius at the blow starting time
        /// </summary>
        float BlowDamage { get; }
        /// <summary>
        /// this radius  is used to draw blow and to damage units with this blow
        /// </summary>            
        float BlowRadius { get; }
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
        /// unit tries to shoot
        /// </summary>
        /// <returns> true if shoot was provided(if gun was recharged)</returns>
        bool Shoot();
        ////obsolete
        ///// <summary>
        ///// set constant speed to move with it. unit will reach and hold this speed unitl new acceleration or setting speed command received
        ///// </summary>
        //void SetSpeed(float Speed);
        ///// <summary>
        ///// set rotation angle. unit will try to reach this angle with max acceleration and hold it.
        ///// </summary>
        //void SetAngle(float Angle);
        ///// <summary>
        ///// makes unit to go to target location
        ///// </summary>
        ///// <param name="TargetLocation">location to go to</param>
        ///// <param name="Stop">true if unit must try to stop there</param>
        //void GoTo(GameVector TargetLocation, bool Stop);
        #endregion
        #region Debug
        /// <summary>
        /// this text will be viewed on screen near unit
        /// </summary>
        string Text { get; set; }
        #endregion
    }
    /// <summary>
    /// interface for every shot in the game
    /// </summary>
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
    /// <summary>
    /// interface for iteration on near objects for specified unit
    /// </summary>
    public interface INearObjectsIterator
    {
        /// <summary>
        /// resets iterator
        /// </summary>
        void Reset();
        /// <summary>
        /// gets next near unit. null if all units has being iterated
        /// </summary>
        /// <returns>IUnit</returns>
        IUnit NextUnit();
        /// <summary>
        /// updates center of near zone
        /// </summary>
        /// <param name="NewCenter">new center of the near zone</param>
        void UpdateCenter(GameVector NewCenter);
    }
    /// <summary>
    /// interface for game. provides all information about the game
    /// </summary>
    public interface IGame
    {
       /// <summary>
        /// set displayed text
       /// </summary>
       /// <param name="Text">text to display</param>
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
        /// <param name="Index">show index</param>
        /// <returns>specified shot</returns>
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
        /// gets near units
        /// this overload is faster than overload with Lists,
        /// becouse it calculates next unit in iteration time and doesn't create any lists in memory
        /// </summary>
        /// <param name="Position">position to specify</param>
        /// <param name="Radius">near zone radius, maximum is MaxRadius</param>
        /// <returns>iterator to near objects</returns>
        INearObjectsIterator GetNearUnits(GameVector Position, float Radius);
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
    /// <summary>
    /// interface for game player AI
    /// </summary>
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
    /// <summary>
    /// struct, representing a line
    /// </summary>
    public struct Line
    {
        /// <summary>
        /// line start
        /// </summary>
        public GameVector pt1;
        /// <summary>
        /// line end
        /// </summary>
        public GameVector pt2;
        /// <summary>
        /// line direction
        /// </summary>
        public GameVector dir;
        /// <summary>
        /// creates a new instance of a line
        /// </summary>
        /// <param name="Start">line start</param>
        /// <param name="End">line end</param>
        public Line(GameVector Start, GameVector End)
        {
            pt1 = Start;
            pt2 = End;
            dir = pt2 - pt1;
        }        
        private const float eps = 10e-4f;
        /// <summary>
        /// calculates two lines intersection
        /// </summary>
        /// <param name="Start1">first line start</param>
        /// <param name="End1">first line end</param>
        /// <param name="Start2">second line start</param>
        /// <param name="End2">second line end</param>
        /// <param name="Intersection">lines intersection point</param>
        /// <returns>true if sectors of specified lines intersects</returns>
        public static bool LinesIntersection(GameVector Start1, GameVector End1, GameVector Start2, GameVector End2, out GameVector Intersection)
        {
            //forwardLeft+(forwardRight-forwardLeft)*t1=backRight+(backLeft-backRight)*t2
            //t2=(forwardLeft.x-backRight.x)/(backLeft.x-backRight.x)+(forwardRight.x-forwardLeft.x)/(backLeft.x-backRight.x)*t1
            //forwardLeft.y+(forwardRight.y-forwardLeft.y)*t1=backRight.y+(backLeft.y-backRight.y)*((forwardLeft.x-backRight.x)/(backLeft.x-backRight.x)+(forwardRight.x-forwardLeft.x)/(backLeft.x-backRight.x)*t1)
            //((forwardRight.y-forwardLeft.y)- (backLeft.y-backRight.y)*(forwardRight.x-forwardLeft.x)/(backLeft.x-backRight.x))*t1=backRight.y-forwardLeft.y+(backLeft.y-backRight.y)*(forwardLeft.x-backRight.x)/(backLeft.x-backRight.x) 
            float UpperFractionTerm = (Start2.Y - Start1.Y) * (End2.X - Start2.X) + (End2.Y - Start2.Y) * (Start1.X - Start2.X);
            float LowerFractionTerm = (End1.Y - Start1.Y) * (End2.X - Start2.X) - (End2.Y - Start2.Y) * (End1.X - Start1.X);
            float t1 = UpperFractionTerm / LowerFractionTerm;
            float t2;
            if (Math.Abs(End2.X - Start2.X) > Math.Abs(End2.Y - Start2.Y))
                t2 = ((Start1.X - Start2.X) + (End1.X - Start1.X) * t1) / (End2.X - Start2.X);
            else
                t2 = ((Start1.Y - Start2.Y) + (End1.Y - Start1.Y) * t1) / (End2.Y - Start2.Y);
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
                // ((End1.X - Start1.X) * (End2.Y - Start2.Y) - (End2.X - Start2.X) * (End1.Y - Start1.Y));
                //t2 = (Start1.Y - Start2.Y) / (End2.Y - Start2.Y) + (End1.Y - Start1.Y) / (End2.Y - Start2.Y) * t1;
            }
            Intersection = Start1 + (End1 - Start1) * t1;
            return (t1 > -eps && t1 < 1 + eps && t2 > -eps && t2 < 1 + eps);
        }
        /// <summary>
        /// defines whether this line sector intersects another lines sector
        /// </summary>
        /// <param name="AnotherLine">line to check for intersection</param>
        /// <returns>true if intersection exists</returns>
        public bool Intersects(Line AnotherLine)
        {
            GameVector intersection;
            return LinesIntersection(pt1, pt2, AnotherLine.pt1, AnotherLine.pt2, out intersection);
        }
        /// <summary>
        /// calculates line sector length
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return (pt1 - pt2).Length();
        }        
        internal bool AreOnOneSide(GameVector Pt1, GameVector Pt2)
        {
            return ((Pt1.X - pt1.X) * dir.Y - (Pt1.Y - pt1.Y) * dir.X) * (dir.X * (Pt2.Y - pt1.Y) - dir.Y * (Pt2.X - pt1.X)) < 0;
        }
    }
    /// <summary>
    /// struct, representing circle
    /// </summary>
    public struct Circle
    {
        GameVector center;
        float radius;
        float radiusSq;
        /// <summary>
        /// circle center
        /// </summary>
        public GameVector Center
        {
            get { return center; }
        }
        /// <summary>
        /// circle radius
        /// </summary>
        public float Radius
        {
            get { return radius; }
        }
        /// <summary>
        /// creates new circle instance
        /// </summary>
        /// <param name="Center">circle center</param>
        /// <param name="Radius">circle radius</param>
        public Circle(GameVector Center, float Radius)
        {
            center = Center;
            radius = Radius;
            radiusSq = radius * radius;
        }
        /// <summary>
        /// calculates intersection with specified line sector
        /// </summary>
        /// <param name="Line">line to check for intersection</param>
        /// <returns></returns>
        public bool Intersects(Line Line)
        {
            GameVector CenterPt1 = center - Line.pt1;
            GameVector CenterPt2 = center - Line.pt2;
            GameVector Pt2Pt1 = Line.pt2 - Line.pt1;
            float Pt2Pt1LengthSq = Pt2Pt1.LengthSquared();
            float CenterPt1LengthSq = CenterPt1.LengthSquared();
            if (CenterPt1LengthSq <= radiusSq) return true;
            if (CenterPt2.LengthSquared() <= radiusSq) return true;
            float dot = GameVector.Dot(Pt2Pt1, CenterPt1);
            if (dot < 0 || dot > Pt2Pt1LengthSq) return false;
            return Pt2Pt1LengthSq * (CenterPt1LengthSq - radiusSq) <= dot * dot;
        }
        /// <summary>
        /// calculates intersection with specified circle
        /// </summary>
        /// <param name="Circle">circle to check for intersection</param>
        /// <returns></returns>
        public bool Intersects(Circle Circle)
        {
            return GameVector.DistanceSquared(center, Circle.center) < (radius + Circle.radius) * (radius + Circle.radius);
        }
    }
    /// <summary>
    /// struct, representing rectangle
    /// </summary>
    public struct Rectangle
    {
        GameVector forwardLeft, forwardRight, backRight, backLeft;
        private GameVector center, forward, size;
        /// <summary>
        /// rectangle forward left corner
        /// </summary>
        public GameVector ForwardLeft
        { get { return forwardLeft; } }
        /// <summary>
        /// rectangle forward right corner
        /// </summary>
        public GameVector ForwardRight
        { get { return forwardRight; } }
        /// <summary>
        /// rectangle back right corner
        /// </summary>
        public GameVector BackRight
        { get { return backRight; } }
        /// <summary>
        /// rectangle back left corner
        /// </summary>
        public GameVector BackLeft
        { get { return backLeft; } }
        /// <summary>
        /// rectangle center
        /// </summary>
        public GameVector Center
        {
            get
            {
                return center;
            }
        }
        /// <summary>
        /// vector containing rectangle width and height
        /// </summary>
        public GameVector Size
        {
            get
            {
                return size;
            }
        }
        /// <summary>
        /// rectangle forward direction
        /// </summary>
        public GameVector Forward
        {
            get
            {
                return forward;
            }
        }
        /// <summary>
        /// recctangle right direction
        /// </summary>
        public GameVector Right
        {
            get
            {
                return new GameVector(forward.Y, -forward.X);
            }
        }
        /// <summary>
        /// rectangle azimuth in radians (angle between forward vector and GameVector(1,0))
        /// </summary>
        public float Angle
        {
            get
            {
                GameVector forward = forwardLeft - backLeft;
                return (float)Math.Atan2(forward.Y, forward.X);
            }
        }
        /// <summary>
        /// obsolete. Do not use this method
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
            center = (ForwardLeft + ForwardRight + BackRight + BackLeft) * 0.25f;
            size = new GameVector((forwardLeft - backLeft).Length(), (forwardLeft - forwardRight).Length());
            forward = (forwardLeft - backLeft).Normalize();
        }
        /// <summary>
        /// Creates new rectangle struct
        /// </summary>
        /// <param name="Center">Rectangle center</param>
        /// <param name="Size">Size vector, X for length,Y for width</param>
        /// <param name="Forward">Forward direction</param>
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
        /// <summary>
        /// gets bounding circle for this rectangle
        /// </summary>
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
        /// <summary>
        /// calculates intersection with specified line
        /// </summary>
        /// <param name="Start">line start</param>
        /// <param name="End">line end</param>
        /// <param name="Intersection">intersection point</param>
        /// <returns>true if intersection exists</returns>
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
        /// <summary>
        /// calculates intersection with specified line
        /// </summary>
        /// <param name="Start">line start</param>
        /// <param name="End">line end</param>
        /// <returns>true if intersection exists</returns>
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
        static GameVector centerToCenter;
        static bool nearestedgeIsVertical1, nearest1edgeIsPositive1, nearest2edgeIsPositive1,
            nearestedgeIsVertical2, nearest1edgeIsPositive2, nearest2edgeIsPositive2;
        static Line nearestedge1, nearestedge2;
        //Pt111CanItersect - point first (1) of rectangle first (1) is on the one side of another rectangle nearest(1) edge
        static bool Pt111CanItersect, Pt121CanItersect, Pt131CanItersect, Pt141CanItersect,
            Pt112CanItersect, Pt122CanItersect, Pt132CanItersect, Pt142CanItersect,
            Pt211CanItersect, Pt221CanItersect, Pt231CanItersect, Pt241CanItersect,
            Pt212CanItersect, Pt222CanItersect, Pt232CanItersect, Pt242CanItersect;
        static float dotVertical;
        static float dotHorisontal;
        /// <summary>
        /// calculates intersection to another rectangle
        /// </summary>
        /// <param name="rect">rectangle to check for intersection</param>
        /// <returns>true if intersection exists</returns>
        public bool IntersectsRectangle(Rectangle rect)
        {
            centerToCenter = rect.center - this.center;
            //find nearest edge 1 
            dotVertical = GameVector.Dot(forward, centerToCenter);
            dotHorisontal = GameVector.Dot(Right, centerToCenter);
            if (Math.Abs(dotVertical) > Math.Abs(dotHorisontal))
            {
                nearestedgeIsVertical1 = true;
                nearest1edgeIsPositive1 = dotVertical > 0;
                nearest2edgeIsPositive1 = dotHorisontal > 0;
            }
            else
            {
                nearestedgeIsVertical1 = false;
                nearest1edgeIsPositive1 = dotHorisontal > 0;
                nearest2edgeIsPositive1 = dotVertical > 0;
            }
            //create nearest edge as line
            if (nearestedgeIsVertical1)
            {
                if (nearest1edgeIsPositive1) nearestedge1 = new Line(forwardLeft, forwardRight);
                else nearestedge1 = new Line(backLeft, backRight);
            }
            else
            {
                if (nearest1edgeIsPositive1) nearestedge1 = new Line(forwardRight, backRight);
                else nearestedge1 = new Line(forwardLeft, backLeft);
            }
            // cut rect if it is fully on one side of this rectangle
            Pt111CanItersect = nearestedge1.AreOnOneSide(center, rect.backLeft);
            Pt121CanItersect = nearestedge1.AreOnOneSide(center, rect.backRight);
            Pt131CanItersect = nearestedge1.AreOnOneSide(center, rect.forwardLeft);
            Pt141CanItersect = nearestedge1.AreOnOneSide(center, rect.forwardRight);
            if (!Pt111CanItersect && !Pt121CanItersect && !Pt131CanItersect && !Pt141CanItersect)
                return false;
            //find nearest edge2
            dotVertical = -GameVector.Dot(rect.forward, centerToCenter);
            dotHorisontal = -GameVector.Dot(rect.Right, centerToCenter);
            if (Math.Abs(dotVertical) > Math.Abs(dotHorisontal))
            {
                nearestedgeIsVertical2 = true;
                nearest1edgeIsPositive2 = dotVertical > 0;
                nearest2edgeIsPositive2 = dotHorisontal > 0;
            }
            else
            {
                nearestedgeIsVertical2 = false;
                nearest1edgeIsPositive2 = dotHorisontal > 0;
                nearest2edgeIsPositive2 = dotVertical > 0;
            }
            //create nearest edge as line
            if (nearestedgeIsVertical2)
            {
                if (nearest1edgeIsPositive2) nearestedge2 = new Line(rect.forwardLeft, rect.forwardRight);
                else nearestedge2 = new Line(rect.backLeft, rect.backRight);
            }
            else
            {
                if (nearest1edgeIsPositive2) nearestedge2 = new Line(rect.forwardRight, rect.backRight);
                else nearestedge2 = new Line(rect.forwardLeft, rect.backLeft);
            }
            // cut this rect if it is fully on one side of rect
            Pt211CanItersect = nearestedge2.AreOnOneSide(rect.center, backLeft);
            Pt221CanItersect = nearestedge2.AreOnOneSide(rect.center, backRight);
            Pt231CanItersect = nearestedge2.AreOnOneSide(rect.center, forwardLeft);
            Pt241CanItersect = nearestedge2.AreOnOneSide(rect.center, forwardRight);
            if (!Pt211CanItersect && !Pt221CanItersect && !Pt231CanItersect && !Pt241CanItersect)
                return false;
            //form next nearest edge of this rectangle            
            if (!nearestedgeIsVertical1)
            {
                if (nearest2edgeIsPositive1) nearestedge1 = new Line(forwardLeft, forwardRight);
                else nearestedge1 = new Line(backLeft, backRight);
            }
            else
            {
                if (nearest2edgeIsPositive1) nearestedge1 = new Line(forwardRight, backRight);
                else nearestedge1 = new Line(forwardLeft, backLeft);
            }
            // cut rect if it is fully on one side of this rectangle
            Pt112CanItersect = nearestedge1.AreOnOneSide(center, rect.backLeft);
            Pt122CanItersect = nearestedge1.AreOnOneSide(center, rect.backRight);
            Pt132CanItersect = nearestedge1.AreOnOneSide(center, rect.forwardLeft);
            Pt142CanItersect = nearestedge1.AreOnOneSide(center, rect.forwardRight);
            if (!Pt112CanItersect && !Pt122CanItersect && !Pt132CanItersect && !Pt142CanItersect)
                return false;
            //create next nearest edge as line
            if (!nearestedgeIsVertical2)
            {
                if (nearest2edgeIsPositive2) nearestedge2 = new Line(rect.forwardLeft, rect.forwardRight);
                else nearestedge2 = new Line(rect.backLeft, rect.backRight);
            }
            else
            {
                if (nearest2edgeIsPositive2) nearestedge2 = new Line(rect.forwardRight, rect.backRight);
                else nearestedge2 = new Line(rect.forwardLeft, rect.backLeft);
            }
            // cut this rect if it is fully on one side of rect
            Pt212CanItersect = nearestedge2.AreOnOneSide(rect.center, backLeft);
            Pt222CanItersect = nearestedge2.AreOnOneSide(rect.center, backRight);
            Pt232CanItersect = nearestedge2.AreOnOneSide(rect.center, forwardLeft);
            Pt242CanItersect = nearestedge2.AreOnOneSide(rect.center, forwardRight);
            if (!Pt212CanItersect && !Pt222CanItersect && !Pt232CanItersect && !Pt242CanItersect)
                return false;
            return ((Pt111CanItersect && Pt112CanItersect) ||
                (Pt121CanItersect && Pt122CanItersect) ||
                (Pt131CanItersect && Pt132CanItersect) ||
                (Pt141CanItersect && Pt142CanItersect)
                ||
                (Pt211CanItersect && Pt212CanItersect) ||
                (Pt221CanItersect && Pt222CanItersect) ||
                (Pt231CanItersect && Pt232CanItersect) ||
                (Pt241CanItersect && Pt242CanItersect)
                );
        }
    }
}
