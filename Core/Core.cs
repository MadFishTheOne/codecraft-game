using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Collections;
using MiniGameInterfaces;
using System.IO;


//using System.Windows.Forms;
//using System.Drawing;
namespace CoreNamespace
{
    public class Core : IGame
    {

        class GameObjectsClass
        {
            const int border = 4000;
            const int gameObjectsCCells = 46;
            const float cellSize = border * 2 / gameObjectsCCells;
            ArrayList[,] gameObjects;
            public GameObjectsClass()
            {
                gameObjects = new ArrayList[gameObjectsCCells, gameObjectsCCells];
                for(int i=0;i<gameObjectsCCells;i++)
                    for (int j = 0; j < gameObjectsCCells; j++)
                    {
                        gameObjects[i, j] = new ArrayList();
                    }
            }
            int GetLogicCoo(float RealCoo)
            {
                int X = (int)((RealCoo + border) / (2 * border) * gameObjectsCCells);
                X = (int)Math.Min(Math.Max(X, 0), gameObjectsCCells - 1);
                return X;
            }            

            public void UpdateUnit(Unit unit)
            {
                int X = GetLogicCoo(unit.position.X);
                int Y = GetLogicCoo(unit.position.Y);
                int oldX, oldY;
                
                ArrayList node = gameObjects[X, Y];
                unit.GetLogicCoo(out oldX, out oldY);
                gameObjects[oldX, oldY].Remove(unit);

                if (!node.Contains(unit))
                {                    
                    node.Add(unit);
                    unit.SetLogicCoo(X, Y);
                }
            }
            public void UpdateShot(Shots.Shot shot)
            {
                int X = GetLogicCoo(shot.Position.X);
                int Y = GetLogicCoo(shot.Position.Y);
                int oldX, oldY;
                shot.GetLogicCoo(out oldX, out oldY);
                gameObjects[oldX, oldY].Remove(shot);
                ArrayList node = gameObjects[X, Y];
                if (!node.Contains(shot))
                {
                    
                    node.Add(shot);
                    shot.SetLogicCoo(X, Y);
                }
            }
            public void GetNearObjects(Vector2 Position, float Radius, out List<Unit> NearUnits, out List<Shots.Shot> NearShots)
            {
                NearUnits = new List<Unit>();
                NearShots = new List<Shots.Shot>();
                
                int RadiusLogic =(int)(Radius/cellSize)+1;// (int)(CruiserSize.Y / (border / (float)gameObjectsCCells)) + 1;
                int X=GetLogicCoo( Position.X);
                int Y=GetLogicCoo(Position.Y);
                //if (RadiusLogic == 0 && (GetLogicCoo(X + 10) != X || GetLogicCoo(X - 10) != X || GetLogicCoo(Y + 10) != Y || GetLogicCoo(Y - 10) != Y))
                //{
                //    RadiusLogic++;
                //}

                int minX, minY, maxX, maxY;
                minX = (int)Math.Min(Math.Max(X - RadiusLogic, 0), gameObjectsCCells - 1);
                minY = (int)Math.Min(Math.Max(Y - RadiusLogic, 0), gameObjectsCCells - 1);
                maxX = (int)Math.Min(Math.Max(X + RadiusLogic, 0), gameObjectsCCells - 1);
                maxY = (int)Math.Min(Math.Max(Y + RadiusLogic, 0), gameObjectsCCells - 1);

                int i, j, k;
                for (i = minX; i <= maxX; i++)
                    for (j = minY; j <= maxY; j++)
                    {
                        for (k = 0; k < gameObjects[i, j].Count; k++)
                        {
                            Unit nearUnit = gameObjects[i, j][k] as Unit;
                            if (nearUnit != null)
                            {
                                NearUnits.Add(nearUnit);
                            }
                            else
                            {
                                Shots.Shot nearShot = gameObjects[i, j][k] as Shots.Shot;
                                if (nearShot != null)
                                {
                                    NearShots.Add(nearShot);
                                }
                            }
                        }
                    }
            }
            internal void RemoveShot(Shots.Shot shot)
            {
                int X,Y;
                shot.GetLogicCoo(out X,out Y);
                gameObjects[X, Y].Remove(shot);
            }
            internal void RemoveUnit(Unit unit)
            {
                int X, Y;
                unit.GetLogicCoo(out X, out Y);
                gameObjects[X, Y].Remove(unit);
            }

        }
        static GameObjectsClass gameObjects;
        class AngleClass
        {
            public static float Normalize(float angle)
            {
                while (angle < -MathHelper.Pi)
                    angle += MathHelper.TwoPi;
                while (angle > MathHelper.Pi)
                    angle -= MathHelper.TwoPi;
                return angle;
            }
            public static float Distance(float angle1, float angle2)
            {
                float prevDist = Math.Abs(Normalize(angle1 - angle2));
                return (float)Math.Min(prevDist, Math.Abs(Normalize(MathHelper.TwoPi - prevDist)));
            }
            public static float Difference(float angle1, float angle2)
            {
                return Normalize(angle1 - angle2);
                //float prevDist = angle1 - angle2;
                //if (Math.Abs(prevDist) < MathHelper.Pi) return prevDist;
                //return (float)Math.Min(prevDist, Math.Abs(Normalize(MathHelper.TwoPi - prevDist)));
            }
        }

        List<IAI> players;
        string[] playersText;
        float[] playersTotalUpdateTime;
        float coreTotalUpdateTime;

        public Core(int ScreenWidth, int ScreenHeight, ContentManager content, GraphicsDeviceManager graphics)
        {
            gameObjects = new GameObjectsClass();
            timing = new TimingClass();
            viewer = new Viewer(ScreenWidth, ScreenHeight, content, graphics);
            units = new List<Unit>();
            shots = new Shots(units);
        }
        public class TimingClass
        {
            public const float SpeedsMultiplier = 10;
            /// <summary>
            /// in milliseconds
            /// </summary>
            long prevTime, startTime;
            float nowTime, deltaTime, timeSpeed;
            bool paused;
            public float TimeSpeed
            {
                get
                {
                    return timeSpeed;
                }
                set
                {
                    timeSpeed = value;
                }
            }
            public bool Paused
            {
                get
                {
                    return paused;
                }
                set
                {
                    paused = value;
                }
            }
            public TimingClass()
            {
                startTime = prevTime = Environment.TickCount;
                nowTime = deltaTime = 0;
                timeSpeed = 1;
            }
            public void Update()
            {
                if (Environment.TickCount - startTime > 1000 * 6) { }
                long currTime = Environment.TickCount;
                if (!paused)
                {
                    deltaTime = (currTime - prevTime) * 0.001f;
                    if (deltaTime > 0.5f)
                        deltaTime = 0.015f;
                    deltaTime *= timeSpeed;
                    nowTime += deltaTime;
                }
                else
                    deltaTime = 0.0f;
                prevTime = currTime;
            }
            public const float maxDeltaTime = 0.1f;
            public float DeltaTime
            {
                get
                {
                    return Math.Min(deltaTime, maxDeltaTime);
                }
            }
            public float DeltaTimeGlobal
            {
                get
                {
                    return deltaTime;
                }
                set
                {
                    deltaTime = value;
                }
            }
            public float NowTime
            {
                get
                {
                    return nowTime;
                }
            }
        }
        static TimingClass timing;
        public static TimingClass Timing
        {
            get { return timing; }
        }
        public struct DerivativeControlledParameter
        {
            float currValue;
            float min, max;
            public float Min
            { get { return min; } }
            public float Max
            { get { return max; } }
            float maxDerivative;
            public float MaxDerivative
            { get { return maxDerivative; } }
            float currDerivative;
            float aimedValue;
            public float AimedValue
            { get { return aimedValue; } }
            bool aimEnabled;
            bool isAngle;
            public DerivativeControlledParameter(float CurrValue, float Min, float Max, float MaxDerivative, bool IsAngle)
            {
                this.currValue = CurrValue;
                this.max = Max;
                this.maxDerivative = MaxDerivative;
                this.min = Min;
                aimedValue = CurrValue;
                aimEnabled = false;
                currDerivative = new float();
                isAngle = IsAngle;
                if (isAngle) currValue = AngleClass.Normalize(currValue);
            }
            public float Derivative
            {
                get { return currDerivative; }
                set
                {
                    currDerivative = value;
                    if (currDerivative > maxDerivative) currDerivative = maxDerivative;
                    if (currDerivative < -maxDerivative) currDerivative = -maxDerivative;
                    aimEnabled = false;
                }
            }
            public void Update()
            {
                if (aimEnabled)
                {
                    float DeltaValue = maxDerivative * Timing.DeltaTime;
                    if (currValue > aimedValue + DeltaValue) currValue -= DeltaValue;
                    else
                    {
                        if (currValue < aimedValue - DeltaValue) currValue += DeltaValue;
                        else currValue = aimedValue;
                    }
                }
                else
                {
                    currValue += currDerivative * Core.Timing.DeltaTime;                    
                }
                if (isAngle) currValue = AngleClass.Normalize(currValue);
                if (currValue > max) currValue = max;
                if (currValue < min) currValue = min;
            }
            public float Value
            {
                get { return currValue; }
            }
            public void SetAimedValue(float AimedValue)
            {
                aimedValue = AimedValue;
                if (aimedValue > max) aimedValue = max;
                if (aimedValue < min) aimedValue = min;
                if (isAngle) aimedValue = AngleClass.Normalize(aimedValue);
                aimEnabled = true;
            }
            public void DisableAim()
            {
                aimEnabled = false;
            }
            public static implicit operator float(DerivativeControlledParameter variable)
            {
                return variable.currValue;
            }
            public bool RotateCCWToAngle(float AimedAngle, out float AimIsNearDecrementing)
            {
                float angleDist= AngleClass.Distance(AimedAngle, Value);
                if (angleDist< MathHelper.Pi / 180f * 30)
                    AimIsNearDecrementing = angleDist / (MathHelper.Pi / 180f * 30);
                else AimIsNearDecrementing = 1;
                return AngleClass.Difference(Value, AimedAngle) < 0;
                //AimedAngle = AngleClass.Normalize(AimedAngle);
                //if (aimedValue > Value) return true;                
                //return false;
            }
        }
        public class Gun
        {
            float delay, speed, lifeTime, damage;
            public float Delay
            {
                get { return delay; }
            }
            public float Damage
            {
                get
                {
                    return damage;
                }
            }
            public float Speed
            { get { return speed; } }
            public float LifeTime
            { get { return lifeTime; } }
            public float MaxDistance
            { get { return speed * lifeTime; } }
            internal Unit owner;
            float currDelay;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="Delay">in seconds</param>
            /// <param name="MaxDistance"></param>
            /// <param name="Damage"></param>
            public Gun(float Delay, float Speed, float LifeTime, float Damage)
            {
                delay = Delay;
                speed = Speed;
                lifeTime = LifeTime;
                damage = Damage;
                currDelay = 0;
            }
            public void Update()
            {
                if (currDelay >= 0)
                    currDelay -= Timing.DeltaTime;
            }
            /// <summary>
            /// gets time left to stand ready to shot
            /// </summary>
            public float CurrRechargeTime
            {
                get { return currDelay; }
            }
            public bool CanShoot
            {
                get { return currDelay <= 0; }
            }
            /// <summary>
            /// returns true if shot is successful
            /// </summary>
            /// <returns></returns>
            public bool Shoot()
            {
                if (CanShoot)
                {
                    currDelay = delay;
                    shots.Add(new Shots.Shot(owner.position + new Vector2(owner.Forward.X, owner.Forward.Y) * (owner.size.Y*0.5f +1),
                        owner.ForwardVector * speed, Damage, lifeTime,owner));
                    return true;
                }
                else return false;
            }
        }
        public class Unit : IUnit
        {
            string name;
            float blowRadius;
            ShipTypes shipType;
            public float BlowRadius
            { get { return blowRadius; } }
            float blowDamage;
            public float BlowDamage
            { get { return blowDamage; } }
            float hp;
            /// <summary>
            /// team identifier
            /// </summary>
            int team;
            /// <summary>
            /// position on the map. X is left shift, Y - top
            /// </summary>
            internal Vector2 position;
            /// <summary>
            /// Ship is a square. X is a width, Y - length. 
            /// </summary>
            internal Vector2 size;
            /// <summary>
            /// radian per second
            /// </summary>
            DerivativeControlledParameter speed;
            DerivativeControlledParameter rotationSpeed, rotationAngle;
            public Vector2 ForwardVector
            {
                get { return new Vector2((float)Math.Sin(rotationAngle), (float)Math.Cos(rotationAngle)); }
            }
            Gun gun;
            internal float timeAfterDeath;
            internal float maxTimeAfterDeath;
            private bool IsAliveInPrevLoop;
            bool goesToPoint;
            bool stopsNearPoint;
            Vector2 tgtLocation;
            Shots shots;
            public Unit(ShipTypes ShipType, string Name, Vector2 Position, Vector2 Size, DerivativeControlledParameter Speed,
                DerivativeControlledParameter RotationSpeed,
                DerivativeControlledParameter RotationAngle, Gun Gun, float HP, int team, Shots shots, float BlowDamage, float BlowRadius)
            {
                shipType = ShipType;
                blowDamage = BlowDamage;
                blowRadius = BlowRadius;
                name = Name;
                position = Position;
                size = Size;
                speed = Speed;
                rotationSpeed = RotationSpeed;
                rotationAngle = RotationAngle;
                gun = Gun;
                gun.owner = this;
                this.hp = HP;
                this.team = team;
                maxTimeAfterDeath = 5*0.2f;
                timeAfterDeath = 0;
                IsAliveInPrevLoop = true;
                this.shots = Core.shots;
            }
            public Unit(ShipTypes ShipType, int Player, Vector2 Position, float Angle,string Name)
            {
                shipType=ShipType;
                switch (shipType)
                {
                    case ShipTypes.Destroyer:
                        blowDamage = 70;
                        blowRadius = 40;
                        name = Name;
                        position = Position;
                        size = DestroyerSize;
                        speed = new DerivativeControlledParameter(0, 0, 20 * TimingClass.SpeedsMultiplier, 9* TimingClass.SpeedsMultiplier, false);
                        rotationSpeed = new DerivativeControlledParameter(0, -0.22f * TimingClass.SpeedsMultiplier, 0.22f * TimingClass.SpeedsMultiplier, 0.5f * TimingClass.SpeedsMultiplier, false);
                        rotationAngle = new DerivativeControlledParameter(Angle, -MathHelper.Pi, MathHelper.Pi, 1000, true);
                        gun = new Gun(1, 50 * TimingClass.SpeedsMultiplier, 9 / TimingClass.SpeedsMultiplier, 15);
                        gun.owner = this;
                        this.hp = 80;
                        this.team = Player;
                        maxTimeAfterDeath = 5*0.2f;
                        timeAfterDeath = 0;
                        IsAliveInPrevLoop = true;
                        this.shots = Core.shots;
                        break;
                    case ShipTypes.Corvette:
                        blowDamage = 150;
                        blowRadius = 120;
                        maxTimeAfterDeath = 8*0.2f;
                        speed = new DerivativeControlledParameter(0, 0, 5 * TimingClass.SpeedsMultiplier, 1 * TimingClass.SpeedsMultiplier, false);
                        rotationSpeed = new DerivativeControlledParameter(0, -0.12f * TimingClass.SpeedsMultiplier, 0.12f * TimingClass.SpeedsMultiplier, 0.39f * TimingClass.SpeedsMultiplier, false);
                        gun = new Gun(1, 50 * TimingClass.SpeedsMultiplier, 18 / TimingClass.SpeedsMultiplier, 40);                        
                        this.hp = 400;
                        this.team = Player;                        
                        IsAliveInPrevLoop = true;
                        this.shots = Core.shots;
                        timeAfterDeath = 0;
                        name = Name;
                        position = Position;
                        size = CorvetteSize;
                        //size.Y *= 0.9f; //real object size (see texture)
                        rotationAngle = new DerivativeControlledParameter(
                            Angle
                            , -MathHelper.Pi, MathHelper.Pi, 1000, true);
                        gun.owner = this;
                        break;
                    case ShipTypes.Cruiser:
                        blowDamage = 300;
                        blowRadius = 120;
                        maxTimeAfterDeath = 12*0.2f;
                        speed = new DerivativeControlledParameter(0, 0, 2 * TimingClass.SpeedsMultiplier, 1.0f * TimingClass.SpeedsMultiplier, false);
                        rotationSpeed = new DerivativeControlledParameter(0, -0.05f * TimingClass.SpeedsMultiplier, 0.05f * TimingClass.SpeedsMultiplier, 0.2f * TimingClass.SpeedsMultiplier, false);
                        gun = new Gun(4, 50 * TimingClass.SpeedsMultiplier, 27 / TimingClass.SpeedsMultiplier, 200);
                        this.hp = 800;
                        this.team = Player;
                        IsAliveInPrevLoop = true;
                        this.shots = Core.shots;
                        timeAfterDeath = 0;
                        name = Name;
                        position = Position;
                        size = CruiserSize;
                        //size.X *= 0.7f; //real object size (see texture)
                        //size.Y *= 0.9f; //real object size (see texture)
                        rotationAngle = new DerivativeControlledParameter(
                            0//Angle
                            , -MathHelper.Pi, MathHelper.Pi, 1000, true);
                        gun.owner = this;
                        break;
                }
            }
            internal void SetHP(float value) { hp = value; if (hp < 0)  hp = -1;}
            #region IUnit Members
            public float HP { get { return hp; } }

            public bool Dead
            {
                get { return hp < 0; }
            }
            public string Name
            {
                get { return name; }
            }
            public GameVector Position
            {
                get { return new GameVector(position.X, position.Y); }
            }
            public float Speed
            {
                get { return speed.Value; }
            }
            public GameVector Forward
            {
                get { return new GameVector((float)Math.Sin(rotationAngle), (float)Math.Cos(rotationAngle)); }
            }
            public float TimeToRecharge
            {
                get { return gun.CurrRechargeTime; }
            }
            public GameVector Size
            {
                get { return new GameVector(size.X, size.Y); }
            }
            public float MaxSpeed
            {
                get { return speed.Max; }
            }
            public float MaxSpeedAcceleration
            {
                get { return speed.MaxDerivative; }
            }
            public float MaxRotationSpeed
            {
                get { return rotationSpeed.Max; }
            }
            public float MaxRotationAcceleration
            {
                get { return rotationSpeed.MaxDerivative; }
            }
            public float DelayTime
            {
                get { return gun.Delay; }
            }
            public float Damage
            {
                get { return gun.Damage; }
            }
            public float RotationAngle
            {
                get { return rotationAngle.Value; }
            }
            public int PlayerOwner { get { return team; } }
            private bool AccessDenied()
            {
                return Core.CurrentPlayer != team && Core.CurrentPlayer != -1;
            }
            public ShipTypes ShipType
            {
                get { return shipType; }
            }
            public float ShootingRadius
            {
                get { return gun.MaxDistance; }
            }
            #region controlling the unit

            public void Accelerate(float amount)
            {
                if (AccessDenied()) return;
                speed.Derivative = amount;//speed.MaxDerivative;
                goesToPoint = false;
            }
            public void Accelerate()
            {
                if (AccessDenied()) return;
                speed.Derivative = speed.MaxDerivative;
                goesToPoint = false;
            }
            public void DeAccelerate()
            {
                if (AccessDenied()) return;
                speed.Derivative = -speed.MaxDerivative;
                goesToPoint = false;
            }
            public void SetSpeed(float Speed)
            {
                if (AccessDenied()) return;
                speed.SetAimedValue(Speed);
                goesToPoint = false;
            }
            public void SetSpeedGoingToTgt(float Speed)
            {
                if (AccessDenied()) return;
                speed.SetAimedValue(Speed);                
            }
            
            public void SetAngle(float Angle)
            {
                if (AccessDenied()) return;
                rotationAngle.SetAimedValue(Angle);
                goesToPoint = false;
            }
            public void SetAngleGoingToTgt(float Angle)
            {
                if (AccessDenied()) return;
                rotationAngle.SetAimedValue(Angle);
            }
            public bool Shoot()
            {
                if (AccessDenied()) return false;
                bool res = gun.Shoot();
                //shots.Add(new Shots.Shot(position + Forward * 50, position + Forward * (gun.MaxDistance+50), gun.Damage));
                return res;
            }            
            public void GoTo(GameVector TargetLocation, bool Stop)
            {
                if (AccessDenied()) return;
                    goesToPoint = true;
                    stopsNearPoint = Stop;
                    tgtLocation = new Vector2(TargetLocation.X, TargetLocation.Y);
                
            } 
            #endregion
            
            #endregion
            private float GetAngleTo(Vector2 Target)
            {
                return (float)Math.Atan2(Target.X - position.X, Target.Y - position.Y);
            }
            public bool TimeToDie
            { get { return timeAfterDeath >= maxTimeAfterDeath; } }
            internal void Update()
            {
                
                if (hp >= 0)
                {
                    if (goesToPoint)
                    {
                        float AngleToTgt = GetAngleTo(tgtLocation);
                        
                        SetAngleGoingToTgt(AngleToTgt);
                        float distanceSq = Vector2.DistanceSquared(position, tgtLocation);
                        float timeToStop = speed.Value / speed.MaxDerivative;
                        float StopDistanceSq = speed.Value * timeToStop - speed.MaxDerivative * timeToStop * timeToStop / 2;
                        if (AngleClass.Distance(GetAngleTo(tgtLocation), rotationAngle.Value) < MathHelper.PiOver4 &&
                            (StopDistanceSq < Vector2.DistanceSquared(position, tgtLocation) || !stopsNearPoint))
                            SetSpeedGoingToTgt(MaxSpeed);
                        else SetSpeedGoingToTgt(0);
                        //if (distanceSq < 30*30&&speed.Value<10) { goesToPoint = false; }
                    }
                    //hp -= 1;
                    gun.Update();
                    float AimIsNearDecrementing;
                    //rotationAngle.RotateCCWToAngle(rotationAngle.AimedValue, out AimIsNear);
                    if (rotationAngle.RotateCCWToAngle(rotationAngle.AimedValue, out AimIsNearDecrementing))
                    {
                        if (rotationSpeed.Value<0)
                        rotationSpeed.Derivative = rotationSpeed.MaxDerivative;
                        else
                            rotationSpeed.Derivative = rotationSpeed.MaxDerivative * AimIsNearDecrementing;
                    }
                    else
                    {
                        if (rotationSpeed.Value > 0)
                            rotationSpeed.Derivative = -rotationSpeed.MaxDerivative;
                        else
                            rotationSpeed.Derivative = -rotationSpeed.MaxDerivative * AimIsNearDecrementing;
                        
                    }
                    //rotationSpeed.Derivative *= AimIsNearDecrementing;
                    rotationSpeed.Update();
                    if (PlayerOwner == 0) { }
                    rotationAngle.Derivative = rotationSpeed.Value;
                    //                rotationAngle.Derivative = (rotationAngle.AimedValue - rotationAngle.Value) * 0.05f;
                    rotationAngle.Update();
                    speed.Update();
                    position += ForwardVector * speed.Value * Timing.DeltaTime;
                    isDying = false;
                }
                else
                {
                    if (IsAliveInPrevLoop) isDying = true;
                    else isDying = false;
                    timeAfterDeath += Timing.DeltaTime;
                    IsAliveInPrevLoop = false;
                }
            }
            bool isDying;
            internal bool IsDying
            {
                get { return isDying; }
            }
            internal Rectangle GetRectangle()
            {
                Vector2 forward = ForwardVector;
                Vector2 right = new Vector2(forward.Y, -forward.X);
                forward *= size.Y*0.5f;
                right *= size.X*0.5f;
                return new Rectangle(forward - right + position, forward + right + position,
                    -forward + right + position, -forward - right + position);
            }
            int logicX, logicY;
            internal void GetLogicCoo(out int oldX, out int oldY)
            {
                oldX = logicX;
                oldY = logicY;
            }
            internal void SetLogicCoo(int X, int Y)
            {
                logicX = X;
                logicY=Y;
            }
        }
        public class Viewer
        {
            static public Vector3[] TeamColors = { new Vector3(1,0,0),new Vector3(0,1,0),new Vector3(0,0,1),
                                          new Vector3(1,1,0),new Vector3(0,1,1),new Vector3(1,0,1)};
            SpriteFont font;
            public int screenHeight, screenWidth;
            public int ScreenHeight
            {
                get { return screenHeight; }
            }
            public int ScreenWidth
            { get { return screenWidth; } }
            SpriteBatch spriteBatch;
            ContentManager content;
            Texture2D DestroyerTexture, CorvetteTexture,
                CruiserTexture,
                DestroyerSmall, CorvetteSmall,
                CruiserSmall, 
                LaserTexture,
                //EngineTexture,
                //MiniMapTexture,
                FirePunchTexture,EnvironmentTexture;
            internal GraphicsDeviceManager graphics;
            VertexDeclaration vertexDecl;
            #region vertex declaration
            public VertexElement[] VertexElements = new VertexElement[]
{
    //position
    new VertexElement(0, 0, VertexElementFormat.Vector3,
                            VertexElementMethod.Default, VertexElementUsage.Position, 0),
    //index
    new VertexElement(0, 12, VertexElementFormat.Single,
                             VertexElementMethod.Default,
                             VertexElementUsage.PointSize, 0),
    //texture coo
    new VertexElement(0, 12+4, VertexElementFormat.Vector2,
                             VertexElementMethod.Default,
                             VertexElementUsage.TextureCoordinate, 0),
};
            #endregion
            public struct VertexPositionIndexTexture
            {
                public Vector3 Position;
                public float Index;
                public Vector2 TexCoo;
            }
            IndexBuffer indexBuffer;
            VertexBuffer vertexBuffer;
            Effect shipEffect, blowEffect, shotEffect,environmentEffect;
            public Viewer(int ScreenWidth, int ScreenHeight, ContentManager Content, GraphicsDeviceManager Graphics)
            {
                this.screenHeight = ScreenHeight;
                this.screenWidth = ScreenWidth;
                content = Content;
                graphics = Graphics;
            }
            void CreateBuffers()
            {
                vertexDecl = new VertexDeclaration(graphics.GraphicsDevice, VertexElements);
                VertexPositionIndexTexture[] vertexData = new VertexPositionIndexTexture[256 * 6];
                for (int i = 0; i < 256; i++)
                {
                    vertexData[i * 6 + 0].Position = new Vector3(1, 1, 0);
                    vertexData[i * 6 + 1].Position = new Vector3(1, -1, 0);
                    vertexData[i * 6 + 2].Position = new Vector3(-1, -1, 0);
                    vertexData[i * 6 + 3].Position = new Vector3(-1, -1, 0);
                    vertexData[i * 6 + 4].Position = new Vector3(-1, 1, 0);
                    vertexData[i * 6 + 5].Position = new Vector3(1, 1, 0);
                    vertexData[i * 6 + 0].TexCoo = new Vector2(1, 1);
                    vertexData[i * 6 + 1].TexCoo = new Vector2(1, 0);
                    vertexData[i * 6 + 2].TexCoo = new Vector2(0, 0);
                    vertexData[i * 6 + 3].TexCoo = new Vector2(0, 0);
                    vertexData[i * 6 + 4].TexCoo = new Vector2(0, 1);
                    vertexData[i * 6 + 5].TexCoo = new Vector2(1, 1);
                    vertexData[i * 6 + 0].Index = i;
                    vertexData[i * 6 + 1].Index = i;
                    vertexData[i * 6 + 2].Index = i;
                    vertexData[i * 6 + 3].Index = i;
                    vertexData[i * 6 + 4].Index = i;
                    vertexData[i * 6 + 5].Index = i;
                }
                vertexBuffer = new VertexBuffer(graphics.GraphicsDevice, vertexData.Length * vertexDecl.GetVertexStrideSize(0),
                    BufferUsage.None);
                vertexBuffer.SetData<VertexPositionIndexTexture>(vertexData);
                indexBuffer = new IndexBuffer(graphics.GraphicsDevice, 16 * vertexData.Length,
                     BufferUsage.None, IndexElementSize.SixteenBits);
                UInt16[] indexData = new ushort[vertexData.Length];
                for (ushort i = 0; i < indexData.Length; i++)
                    indexData[i] = i;
                indexBuffer.SetData<UInt16>(indexData);
            }
            public void LoadContent()
            {
                spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
                font = content.Load<SpriteFont>("testfont");
                DestroyerTexture = content.Load<Texture2D>("textures\\DestroyerTexture");
                CorvetteTexture = content.Load<Texture2D>("textures\\CorvetteTexture");
                CruiserTexture = content.Load<Texture2D>("textures\\CruiserTexture");
                DestroyerSmall = content.Load<Texture2D>("textures\\DestroyerSmall");
                CorvetteSmall = content.Load<Texture2D>("textures\\CorvetteSmall");
                CruiserSmall = content.Load<Texture2D>("textures\\CruiserSmall");

                LaserTexture = content.Load<Texture2D>("textures\\LaserTexture");
                EnvironmentTexture = content.Load<Texture2D>("textures\\EnvironmentTexture");
                //EngineTexture = content.Load<Texture2D>("textures\\EngineTexture");
                //MiniMapTexture = content.Load<Texture2D>("textures\\MinimapTexture");
                FirePunchTexture = content.Load<Texture2D>("textures\\BlowTexture");
                shipEffect = content.Load<Effect>("effects\\ShipEffect");
                blowEffect = content.Load<Effect>("effects\\BlowEffect");
                shotEffect = content.Load<Effect>("effects\\ShotEffect");
                environmentEffect =  content.Load<Effect>("effects\\EnvironmentEffect");
                CreateBuffers();
            }
            const int BlowDetalization = 5;//billboards count in one blow            
            const int MaxBatchSize = 240;
            public void DrawEnvironment()
            {
                Vector4[] param=new Vector4[1];
                param[0].X = 0;
                param[0].Y = 0;
                param[0].Z = 0;
                param[0].W = 0;
                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
                graphics.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
                graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.One;                
                graphics.GraphicsDevice.VertexDeclaration = vertexDecl;
                graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, vertexDecl.GetVertexStrideSize(0));
                graphics.GraphicsDevice.Indices = indexBuffer;
                environmentEffect.Parameters["ViewProj"].SetValue(ViewProj);
                environmentEffect.Parameters["PlayerColors"].SetValue(TeamColors);
                environmentEffect.Parameters["tex"].SetValue(EnvironmentTexture);
                environmentEffect.Parameters["Positions"].SetValue(param);
                environmentEffect.Parameters["Size"].SetValue(new Vector2(30000,30000));
                environmentEffect.Begin();
                EffectPass p = environmentEffect.CurrentTechnique.Passes[0];
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 1 * 6, 0, 1 * 2);
                p.End();
                environmentEffect.End();
            }
            public void DrawUnits(List<Unit> units)
            {
                graphics.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                graphics.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
                graphics.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
                graphics.GraphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
                graphics.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.One;
                shipEffect.Parameters["ViewProj"].SetValue(ViewProj);
                shipEffect.Parameters["PlayerColors"].SetValue(TeamColors);
                blowEffect.Parameters["ViewProj"].SetValue(ViewProj);
                blowEffect.Parameters["tex"].SetValue(FirePunchTexture);
                blowEffect.Parameters["Detalization"].SetValue(BlowDetalization);
                graphics.GraphicsDevice.VertexDeclaration = vertexDecl;
                graphics.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, vertexDecl.GetVertexStrideSize(0));
                graphics.GraphicsDevice.Indices = indexBuffer;
                Vector4[] CorvetteBatchParams = new Vector4[MaxBatchSize];
                Vector4[] DestroyerBatchParams = new Vector4[MaxBatchSize];
                Vector4[] CruiserBatchParams = new Vector4[MaxBatchSize];
                Vector4[] BlowBatchParams = new Vector4[MaxBatchSize];
                int CDestroyersInBatch = 0;
                int CCorvettesInBatch = 0;
                int CCruisersInBatch = 0;
                int CBlowsInBatch = 0;
                int currUnit = 0;
                
                while (currUnit < units.Count)
                {
                    if (units[currUnit].HP < 0)
                    {
                        for (int i = 0; i < BlowDetalization; i++)
                        {
                            BlowBatchParams[CBlowsInBatch].X = units[currUnit].position.X;
                            BlowBatchParams[CBlowsInBatch].Y = units[currUnit].position.Y;
                            BlowBatchParams[CBlowsInBatch].Z = units[currUnit].BlowRadius;
                            BlowBatchParams[CBlowsInBatch].W = (float)units[currUnit].timeAfterDeath / (float)units[currUnit].maxTimeAfterDeath;
                            CBlowsInBatch++;
                            if (CBlowsInBatch == MaxBatchSize) DrawBlowBatch(BlowBatchParams, ref CBlowsInBatch);
                        }
                    }
                    else
                    {
                        if (units[currUnit].ShipType == ShipTypes.Destroyer)
                        {
                            DestroyerBatchParams[CDestroyersInBatch].X = units[currUnit].position.X;
                            DestroyerBatchParams[CDestroyersInBatch].Y = units[currUnit].position.Y;
                            DestroyerBatchParams[CDestroyersInBatch].Z = units[currUnit].RotationAngle;
                            DestroyerBatchParams[CDestroyersInBatch].W = units[currUnit].PlayerOwner;
                            CDestroyersInBatch++;
                            if (CDestroyersInBatch == MaxBatchSize) DrawUnitBatch(DestroyerBatchParams, ref CDestroyersInBatch, DestroyerTexture,DestroyerSmall, DestroyerSize);
                        }
                        if (units[currUnit].ShipType == ShipTypes.Corvette)
                        {
                            CorvetteBatchParams[CCorvettesInBatch].X = units[currUnit].position.X;
                            CorvetteBatchParams[CCorvettesInBatch].Y = units[currUnit].position.Y;
                            CorvetteBatchParams[CCorvettesInBatch].Z = units[currUnit].RotationAngle;
                            CorvetteBatchParams[CCorvettesInBatch].W = units[currUnit].PlayerOwner;
                            CCorvettesInBatch++;
                            if (CCorvettesInBatch == MaxBatchSize) DrawUnitBatch(CorvetteBatchParams, ref CCorvettesInBatch, CorvetteTexture,CorvetteSmall, CorvetteSize);
                        }
                        if (units[currUnit].ShipType == ShipTypes.Cruiser)
                        {
                            CruiserBatchParams[CCruisersInBatch].X = units[currUnit].position.X;
                            CruiserBatchParams[CCruisersInBatch].Y = units[currUnit].position.Y;
                            CruiserBatchParams[CCruisersInBatch].Z = units[currUnit].RotationAngle;
                            CruiserBatchParams[CCruisersInBatch].W = units[currUnit].PlayerOwner;
                            CCruisersInBatch++;
                            if (CCruisersInBatch == MaxBatchSize) DrawUnitBatch(CruiserBatchParams, ref CCruisersInBatch, CruiserTexture,CruiserSmall, CruiserSize);
                        }
                    }
                    currUnit++;
                }
                if (CDestroyersInBatch > 0) DrawUnitBatch(DestroyerBatchParams, ref CDestroyersInBatch, DestroyerTexture,DestroyerSmall, DestroyerSize);
                if (CCorvettesInBatch > 0) DrawUnitBatch(CorvetteBatchParams, ref CCorvettesInBatch, CorvetteTexture,CorvetteSmall, CorvetteSize);
                if (CCruisersInBatch > 0) DrawUnitBatch(CruiserBatchParams, ref CCruisersInBatch, CruiserTexture,CruiserSmall, CruiserSize);
                if (CBlowsInBatch > 0) DrawBlowBatch(BlowBatchParams, ref CBlowsInBatch);
            }
            private void DrawBlowBatch(Vector4[] BlowBatchParams, ref int CBlowsInBatch)
            {
                blowEffect.Begin();
                blowEffect.Parameters["Params"].SetValue(BlowBatchParams);
                EffectPass p = blowEffect.CurrentTechnique.Passes[0];
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CBlowsInBatch * 6, 0, CBlowsInBatch * 2);
                p.End();
                CBlowsInBatch = 0;
                blowEffect.End();
            }
            void DrawUnitBatch(Vector4[] UnitInstanceParams, ref int CUnits, Texture2D Text,Texture2D TextSmall, Vector2 Size)
            {
                float BigLength = 4000;
                shipEffect.Begin();
                float SizeMultiplier = 1.2f;
                if (CameraPosition.Z > BigLength) SizeMultiplier *= CameraPosition.Z / BigLength;
                
                shipEffect.Parameters["Size"].SetValue(Size * SizeMultiplier);
                shipEffect.Parameters["tex"].SetValue((CameraPosition.Z < BigLength) ? Text : TextSmall);
                shipEffect.Parameters["Positions"].SetValue(UnitInstanceParams);
                EffectPass p = shipEffect.CurrentTechnique.Passes[0];
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CUnits * 6, 0, CUnits * 2);
                p.End();
                shipEffect.Parameters["tex"].SetValue(Text);
                p.Begin();
                if (CameraPosition.Z > BigLength)
                {
                    
                    graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CUnits * 6, 0, CUnits * 2);
                }
                p.End();
                CUnits = 0;
                shipEffect.End();
            }
            private void DrawShotBatch(Vector4[] ShotBatchParams1, Vector3[] ShotBatchParams2, ref int CShotsInBatch)
            {
                shotEffect.Begin();
                shotEffect.Parameters["tex"].SetValue(LaserTexture);
                shotEffect.Parameters["Positions"].SetValue(ShotBatchParams1);
                shotEffect.Parameters["Params"].SetValue(ShotBatchParams2);
                EffectPass p = shotEffect.CurrentTechnique.Passes[0];
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CShotsInBatch * 6, 0, CShotsInBatch * 2);
                p.End();
                CShotsInBatch = 0;
                shotEffect.End();
            }
            internal void DrawShots(Shots shots)
            {
                shotEffect.Parameters["ViewProj"].SetValue(ViewProj);
                Vector4[] ShotBatchParams1 = new Vector4[MaxBatchSize / 2];
                Vector3[] ShotBatchParams2 = new Vector3[MaxBatchSize / 2];
                int CShotsInBatch = 0;
                foreach (Shots.Shot shot in shots)
                {
                    ShotBatchParams1[CShotsInBatch].X = shot.pos.X;
                    ShotBatchParams1[CShotsInBatch].Y = shot.pos.Y;
                    ShotBatchParams1[CShotsInBatch].Z = shot.End.X;
                    ShotBatchParams1[CShotsInBatch].W = shot.End.Y;
                    ShotBatchParams2[CShotsInBatch].X = 1;// (shot.hitSomebody) ? 1 : -1;
                    ShotBatchParams2[CShotsInBatch].Y = 0;
                    ShotBatchParams2[CShotsInBatch].Z = shot.Size;//width
                    CShotsInBatch++;
                    if (CShotsInBatch >= MaxBatchSize / 2)
                        DrawShotBatch(ShotBatchParams1, ShotBatchParams2, ref CShotsInBatch);
                }
                if (CShotsInBatch > 0)
                    DrawShotBatch(ShotBatchParams1, ShotBatchParams2, ref CShotsInBatch);
            }
            public void DrawText(string text, Vector2 pos, int align, Color color)
            {
                spriteBatch.Begin();
                Vector2 fontOrigin = new Vector2(0.0f, 0.0f);;
                switch (align)
                {
                    case 1:
                        fontOrigin = new Vector2(font.MeasureString(text).X / 2, 0.0f);
                        break;
                    case 2:
                        fontOrigin = new Vector2(font.MeasureString(text).X, 0.0f);
                        break;
                }
                spriteBatch.DrawString(font, text, pos + new Vector2(2, 2), Color.Black, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(font, text, pos, color, 0, fontOrigin, 1.0f, SpriteEffects.None, 0.5f);
                spriteBatch.End();
            }
        }
        public class Shots : IEnumerable
        {
            public class Shot : IShot
            {
                public Vector2 pos, direction;
                private float size;
                public float Size
                {
                    get
                    {
                        return size;
                    }
                }
                public BoundingSphere GetBoundingSphere()
                {
                    return new BoundingSphere(new Microsoft.Xna.Framework.Vector3((pos + End) * 0.5f, 0), size * 0.5f);
                }
                Vector2 forward;
                public Vector2 End
                {
                    get
                    {
                        return pos + forward * size;
                    }
                }
                public bool hitSomebody;
                public float damage;
                public float lifeTime;
                //public const long MaxLifeTime = 2000;
                Unit parent;
                public bool IsChildOf(Unit unitToCheck)
                {
                    return unitToCheck == parent;
                }

                public Shot(Vector2 Pos, Vector2 Dir, float Damage, float LifeTime,Unit ParentUnit)
                {
                    parent = ParentUnit;
                    switch (parent.ShipType)
                    {
                        case ShipTypes.Destroyer:
                            size = 24; break;
                        case ShipTypes.Corvette:
                            size = 48; break;
                        case ShipTypes.Cruiser:
                            size = 90; break;
                        default: size = 1; break;
                    }
                    pos = Pos;
                    direction = Dir;
                    damage = Damage;
                    lifeTime = LifeTime;
                    hitSomebody = false;
                    forward = Vector2.Normalize(direction);
                }
                public void HitSomebody(Vector2 where)
                {
                    hitSomebody = true;
                }
                public void Update()
                {
                    lifeTime -= Timing.DeltaTime;
                    pos += direction * Timing.DeltaTime;
                }
                #region IShot Members
                public GameVector Position
                {
                    get { return new GameVector(pos.X, pos.Y); }
                }
                public GameVector Direction
                {
                    get { return new GameVector(direction.X, direction.Y); }
                }
                #endregion
                int logicX, logicY;
                internal void GetLogicCoo(out int oldX, out int oldY)
                {
                    oldX = logicX;
                    oldY = logicY;
                }
                internal void SetLogicCoo(int X, int Y)
                {
                    logicX = X;
                    logicY = Y;
                }
            }
            public List<Shot> shots;
            List<Unit> units;
            public Shots(List<Unit> Units)
            {
                shots = new List<Shot>();
                units = Units;
            }
            public void Add(Shot shot)
            {
                shots.Add(shot);
                Vector2 intersection = Vector2.One * float.PositiveInfinity, currIntersection;
                foreach (Unit unit in units)
                {
                    if (unit.GetRectangle().IntersectsLine(shot.pos, shot.End, out currIntersection))
                    {
                        shot.hitSomebody = true;
                        if (Vector2.DistanceSquared(shot.pos, currIntersection) < Vector2.DistanceSquared(shot.pos, intersection))
                        {
                            intersection = currIntersection;
                        }
                    }
                }
                //if (shot.hitSomebody)
                //{
                //    shot.end = intersection;
                //}
            }
            public void Update()
            {
                for (int i = 0; i < shots.Count; i++)
                {
                    shots[i].Update();
                    if (shots[i].lifeTime <= 0)
                    {
                        gameObjects.RemoveShot(shots[i]);
                        shots.RemoveAt(i);
                        i--;
                    }
                }
            }
            #region IEnumerable Members
            IEnumerator IEnumerable.GetEnumerator()
            {
                return shots.GetEnumerator();
            }
            #endregion
            internal void Clear()
            {
                shots.Clear();
            }
        }
        static internal Shots shots;
        public static Viewer viewer;
        internal static int CurrentPlayer;

        public string SecondsToString(float seconds)
        {
            int iMinutes = (int)(seconds / 60);
            seconds -= iMinutes * 60;
            int iSeconds = (int)seconds;
            seconds -= iSeconds;
            int iMillis = (int)(seconds * 1000);
            string sSeconds = iSeconds.ToString();
            while (sSeconds.Length < 2)
                sSeconds = "0" + sSeconds;
            string sMillis = iMillis.ToString();
            while (sMillis.Length < 4)
                sMillis = "0" + sMillis;
            return iMinutes.ToString() + ":" + sSeconds + "." + sMillis;
        }

        public void Draw()
        {
            Core.viewer.graphics.GraphicsDevice.Clear(Color.Black);
            ViewProj = Matrix.CreateLookAt(CameraPosition, new Vector3(CameraPosition.X, CameraPosition.Y, 0), new Vector3(0, -1, 0)) *
                 Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)viewer.screenWidth / (float)viewer.screenHeight, 10, 100000);
            //
            viewer.DrawEnvironment();
            viewer.DrawUnits(units);
            viewer.DrawShots(shots);
            //
            int[] destroyers = new int[players.Count];
            int[] corvettes = new int[players.Count];
            int[] cruisers = new int[players.Count];
            int[] total = new int[players.Count];
            string[] infoString = new string[players.Count];
            string[] timeString = new string[players.Count];
            bool gameEnd = true;
            bool gameDraw = true;
            int gameWinner = -1;
            for (int i = 0; i < units.Count; i++)
            {
                switch (units[i].ShipType)
                {
                    case ShipTypes.Destroyer:
                        destroyers[units[i].PlayerOwner]++;
                        break;
                    case ShipTypes.Corvette:
                        corvettes[units[i].PlayerOwner]++;
                        break;
                    case ShipTypes.Cruiser:
                        cruisers[units[i].PlayerOwner]++;
                        break;
                }
                total[units[i].PlayerOwner]++;
                gameDraw = false;
                if ((gameWinner != -1) && (units[i].PlayerOwner != gameWinner))
                    gameEnd = false;
                gameWinner = units[i].PlayerOwner;
            }
            for (int i = 0; i < players.Count; i++)
            {
                infoString[i] = destroyers[i].ToString() + "+" + corvettes[i].ToString() + "+" + cruisers[i].ToString() + "=" + total[i].ToString();
                timeString[i] = SecondsToString(playersTotalUpdateTime[i]);
            }
            string coreTimeString = SecondsToString(coreTotalUpdateTime);
            //
            string[] lines;
            viewer.DrawText(players[0].Author, new Vector2(100, 20), 0, new Color(Core.Viewer.TeamColors[0]));
            viewer.DrawText(players[0].Description, new Vector2(100, 40), 0, Color.Gray);
            viewer.DrawText(infoString[0], new Vector2(100, 60), 0, Color.White);
            viewer.DrawText(timeString[0], new Vector2(100, 80), 0, Color.White);
            lines = playersText[0].Split(new char[] { '\n' });
            for (int i = 0; i < lines.Length; i++)
                viewer.DrawText(lines[i], new Vector2(100, 120 + i * 20), 0, Color.Yellow);
            viewer.DrawText("vs.", new Vector2(Core.viewer.screenWidth / 2, 20), 1, Color.White);
            viewer.DrawText("Core update time:", new Vector2(Core.viewer.screenWidth / 2, 60), 1, Color.White);
            viewer.DrawText(coreTimeString, new Vector2(Core.viewer.screenWidth / 2, 80), 1, Color.White);
            viewer.DrawText(players[1].Author, new Vector2(Core.viewer.screenWidth - 100, 20), 2, new Color(Core.Viewer.TeamColors[1]));
            viewer.DrawText(players[1].Description, new Vector2(Core.viewer.screenWidth - 100, 40), 2, Color.Gray);
            viewer.DrawText(infoString[1], new Vector2(Core.viewer.screenWidth - 100, 60), 2, Color.White);
            viewer.DrawText(timeString[1], new Vector2(Core.viewer.screenWidth - 100, 80), 2, Color.White);
            lines = playersText[1].Split(new char[] { '\n' });
            for (int i = 0; i < lines.Length; i++)
                viewer.DrawText(lines[i], new Vector2(Core.viewer.screenWidth - 100, 120 + i * 20), 2, Color.Yellow);
            //
            if (gameEnd)
            {
                if (gameDraw)
                {
                    viewer.DrawText("DRAW!", new Vector2(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2), 1, Color.White);
                }
                else
                {
                    viewer.DrawText("Winner:", new Vector2(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2), 1, Color.White);
                    viewer.DrawText(players[gameWinner].Author, new Vector2(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2 + 20), 1, new Color(Core.Viewer.TeamColors[gameWinner]));
                    viewer.DrawText(players[gameWinner].Description, new Vector2(Core.viewer.screenWidth / 2, Core.viewer.screenHeight / 2 + 40), 1, Color.Gray);
                }
            }
            //
            viewer.DrawText("Time speed: " + Timing.TimeSpeed.ToString(), new Vector2(10, Core.viewer.screenHeight - 30), 0, Color.White);
        }
        public void Update()
        {
            Stopwatch sw = new Stopwatch();
            for (CurrentPlayer = 0; CurrentPlayer < players.Count; CurrentPlayer++)
            {
                sw.Reset();
                sw.Start();
                players[CurrentPlayer].Update();
                playersTotalUpdateTime[CurrentPlayer] += ((float)sw.ElapsedTicks) / Stopwatch.Frequency;
            }
            CurrentPlayer = -1;
            sw.Reset();
            sw.Start();
            for (int i = 0; i < units.Count; i++)
            {
                //units[i].Shoot();
                units[i].Update();
                if (units[i].IsDying)
                    DamageAllAround(units[i].position, units[i].BlowRadius, units[i].BlowDamage);
                if (units[i].TimeToDie)
                {
                    gameObjects.RemoveUnit(units[i]);
                    units.RemoveAt(i);
                    i--;
                }
            }
            shots.Update();
            UnitIntersections();
            
            coreTotalUpdateTime += ((float)sw.ElapsedTicks) / Stopwatch.Frequency;
        }
        //private void ShotsWithUnitsIntersections()
        //{
        //    foreach (Unit unit in units)
        //        if (unit.HP >= 0)
        //        {
        //            Rectangle rect = unit.GetRectangle();
        //            BoundingSphere sphere = rect.BoxBoundingSphere;
        //            for (int i = 0; i < shots.shots.Count; i++)
        //            {
        //               // if (unit.ShipType == ShipTypes.Destroyer&&Vector2.Distance(unit.position,shots.shots[i].pos)<10) { }
        //                if (shots.shots[i].GetBoundingSphere().Intersects(sphere))
        //                {
        //                    if (unit.ShipType ==  ShipTypes.Destroyer) { }
        //                    if (rect.IntersectsLine(shots.shots[i].pos, shots.shots[i].End))
        //                    {
        //                        //if (unit.Name == "Ship2") { }
        //                        unit.SetHP(unit.HP - shots.shots[i].damage);
        //                        shots.shots.RemoveAt(i);
        //                    }
        //                }
        //            }
        //        }
        //}
        private void DamageAllAround(Vector2 pos, float radius, float Damage)
        {
            foreach (Unit unit in units)
            {
                if (Vector2.DistanceSquared(unit.position, pos) <= radius * radius)
                { unit.SetHP(unit.HP - Damage); }
            }
        }
        private void 
            UnitIntersections()
        {
            foreach (Unit unit in units)
            {
                if (unit.ShipType == ShipTypes.Destroyer)
                { }
                gameObjects.UpdateUnit(unit);            }
            foreach (Shots.Shot shot in shots)
            { gameObjects.UpdateShot(shot); }
            for (int i = 0; i < units.Count ; i++)
                if (units[i].HP >= 0)
                {
                    if (units[i].ShipType == ShipTypes.Destroyer)
                    { }
                    Rectangle rect1 = units[i].GetRectangle();
                    BoundingSphere sphere1 = rect1.BoxBoundingSphere;
                    List<Unit> nearUnits = new List<Unit>();
                    List<Shots.Shot> nearShots = new List<Shots.Shot>();
                    gameObjects.GetNearObjects(units[i].position,0, out nearUnits, out nearShots);
                    if (units[i].ShipType == ShipTypes.Destroyer)
                    { }
                    foreach (Unit nearUnit in nearUnits)
                        if (nearUnit!=units[i])
                    {
                        Rectangle rect2 = nearUnit.GetRectangle();
                        if (sphere1.Intersects(rect2.BoxBoundingSphere))
                        {
                            if (rect1.IntersectsRectangle(rect2))
                            {
                                float hp1 = units[i].HP;
                                float hp2 = nearUnit.HP;
                                units[i].SetHP(hp1 - hp2 * 1.5f);
                                nearUnit.SetHP(hp2 - hp1 * 1.5f);
                            }
                        }
                    }
                    foreach (Shots.Shot shot in nearShots)
                        if (shot.lifeTime>0&&!shot.IsChildOf(units[i]))
                    {
                        if (shot.GetBoundingSphere().Intersects(sphere1))
                        {                            
                            if (rect1.IntersectsLine(shot.pos, shot.End))
                            {
                                //if (unit.Name == "Ship2") { }
                                units[i].SetHP(units[i].HP - shot.damage);
                                shots.shots.Remove(shot);
                                gameObjects.RemoveShot(shot);
                            }
                        }
                    }
                }
                else
                {
                    gameObjects.RemoveUnit(units[i]);
                }
        }
        System.Collections.Generic.List<Unit> units;
        public static Vector2 DestroyerSize = new Vector2(25, 25);
        public static Vector2 CorvetteSize = new Vector2(20, 80);
        public static Vector2 CruiserSize = new Vector2(35, 140);
        public void AddUnits()
        {
            StreamReader rd=  File.OpenText("units to create.txt");
            int currTeam = 0;
            int CCruisers=0,CCorvettes=0,CDestroyers=0;
            while (rd.ReadLine().Contains("Player"))
            {
                string text = rd.ReadLine();
                if (text.Contains("Cruisers"))
                {
                    text = text.Remove(0, text.IndexOf(':')+1);
                    CCruisers = Convert.ToInt32(text);
                }
                else CCruisers = 0;
                text = rd.ReadLine();
                if (text.Contains("Corvettes"))
                {
                    text = text.Remove(0, text.IndexOf(':')+1);
                    CCorvettes = Convert.ToInt32(text);
                }
                else CCorvettes = 0;
                text = rd.ReadLine();
                if (text.Contains("Destroyers"))
                {
                    text = text.Remove(0, text.IndexOf(':')+1);
                    CDestroyers = Convert.ToInt32(text);
                }
                else CDestroyers = 0;
                CreateUnitsForPlayer(currTeam,CCruisers,CCorvettes,CDestroyers,new Vector2(0,000));
                currTeam++;
                if (rd.EndOfStream) { break; }
                
            }
            rd.Close();


            
           
            //units[0].GoTo(new GameVector(300, 200), false);

            //units[0].SetAngle(MathHelper.PiOver2);
            //units[0].SetSpeed(15f);
        }
        private void CreateUnitsForPlayer(int currTeam, int CCruisers, int CCorvettes, int CDestroyers, Vector2 pos)
        {
            int sign = currTeam == 0 ? -1 : 1;
            int MaxShipsInLine = 8;
            int CShips = 0;
            float angle = (currTeam > 0) ? MathHelper.Pi : 0;
            Vector2 position;
            for (int i = 0; i < CDestroyers; i++)
            {
                position = pos + new Vector2(150 * (CShips % MaxShipsInLine) + 75 * ((CShips / MaxShipsInLine) % 2), 150 * (CShips / MaxShipsInLine));
                position.Y += 2000;
                position.Y *= sign;
                position.X += 23;
                units.Add(new Unit(ShipTypes.Destroyer, currTeam, position, angle, "Destroyer -" + i.ToString() + "-"));
                CShips++;

            }           
            for (int i = 0; i < CCorvettes; i++)
            {
                position = pos + new Vector2(150 * (CShips % MaxShipsInLine) + 75 * ((CShips / MaxShipsInLine) % 2), 150 * (CShips / MaxShipsInLine));
                position.Y += 2000;
                position.Y *= sign;
                position.X += 23;
                units.Add(new Unit(ShipTypes.Corvette, currTeam, position, angle, "Corvette -" + i.ToString() + "-"));
                CShips++;
            }

            for (int i = 0; i < CCruisers; i++)
            {
                position = pos + new Vector2(150 * (CShips % MaxShipsInLine) + 75 * ((CShips / MaxShipsInLine) % 2), 150 * (CShips / MaxShipsInLine));
                position.Y += 2000;
                position.Y *= sign;
                position.X += 23;
                units.Add(new Unit(ShipTypes.Cruiser, currTeam, position, angle, "Cruiser -" + i.ToString() + "-"));
                CShips++;
            }

        }
        public void Reset(List<IAI> Players)
        {
            Timing.TimeSpeed = 1.0f;
            CameraPosition = new Vector3(0,0,9000);
            players = Players;
            playersText = new string[players.Count];
            playersTotalUpdateTime = new float[players.Count];
            coreTotalUpdateTime = 0;
            units.Clear();
            shots.Clear();
            gameObjects = new GameObjectsClass();
            AddUnits();
            for (CurrentPlayer = 0; CurrentPlayer < players.Count; CurrentPlayer++)
            {
                playersText[CurrentPlayer] = "";
                players[CurrentPlayer].Init(CurrentPlayer, this);
            }
        }
        public struct Rectangle
        {
            public Vector2 pt1, pt2, pt3, pt4;
            public Rectangle(Vector2 pt1, Vector2 pt2, Vector2 pt3, Vector2 pt4)
            {
                this.pt1 = pt1;
                this.pt2 = pt2;
                this.pt3 = pt3;
                this.pt4 = pt4;
            }
            public BoundingSphere BoxBoundingSphere
            {
                get
                {
                    Vector2 min = new Vector2(Math.Min(Math.Min(pt1.X, pt2.X), Math.Min(pt3.X, pt4.X)),
                        Math.Min(Math.Min(pt1.Y, pt2.Y), Math.Min(pt3.Y, pt4.Y)));
                    Vector2 max = new Vector2(Math.Max(Math.Max(pt1.X, pt2.X), Math.Max(pt3.X, pt4.X)),
                        Math.Max(Math.Max(pt1.Y, pt2.Y), Math.Max(pt3.Y, pt4.Y)));
                    Vector2 center = (min + max) * 0.5f;
                    return new BoundingSphere(new Vector3(center, 0), Vector2.Distance(center, min));
                }
            }
            static bool LinesIntersection(Vector2 pt1, Vector2 pt2, Vector2 pt3, Vector2 pt4, out Vector2 intersection)
            {
                //pt1+(pt2-pt1)*t1=pt3+(pt4-pt3)*t2
                //t2=(pt1.x-pt3.x)/(pt4.x-pt3.x)+(pt2.x-pt1.x)/(pt4.x-pt3.x)*t1
                //pt1.y+(pt2.y-pt1.y)*t1=pt3.y+(pt4.y-pt3.y)*((pt1.x-pt3.x)/(pt4.x-pt3.x)+(pt2.x-pt1.x)/(pt4.x-pt3.x)*t1)
                //((pt2.y-pt1.y)- (pt4.y-pt3.y)*(pt2.x-pt1.x)/(pt4.x-pt3.x))*t1=pt3.y-pt1.y+(pt4.y-pt3.y)*(pt1.x-pt3.x)/(pt4.x-pt3.x) 
                float t1 = ((pt3.Y - pt1.Y) * (pt4.X - pt3.X) + (pt4.Y - pt3.Y) * (pt1.X - pt3.X))
                    / ((pt2.Y - pt1.Y) * (pt4.X - pt3.X) - (pt4.Y - pt3.Y) * (pt2.X - pt1.X));
                float t2 = (pt1.X - pt3.X) / (pt4.X - pt3.X) + (pt2.X - pt1.X) / (pt4.X - pt3.X) * t1;
                if (float.IsNaN(t1 + t2))
                {
                    t1 = ((pt3.X - pt1.X) * (pt4.Y - pt3.Y) + (pt4.X - pt3.X) * (pt1.Y - pt3.Y))
                    / ((pt2.X - pt1.X) * (pt4.Y - pt3.Y) - (pt4.X - pt3.X) * (pt2.Y - pt1.Y));
                     t2 = (pt1.Y - pt3.Y) / (pt4.Y - pt3.Y) + (pt2.Y - pt1.Y) / (pt4.Y - pt3.Y) * t1;
                }
                intersection = pt1 + (pt2 - pt1) * t1;
                return (t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1);
            }
            public bool IntersectsLine(Vector2 pt5, Vector2 pt6, out Vector2 Intersection)
            {
                Intersection = Vector2.One * float.PositiveInfinity;
                bool res = false;
                Vector2 currIntersection;
                if (LinesIntersection(pt1, pt2, pt5, pt6, out currIntersection))
                { Intersection = currIntersection; res = true; }
                if (LinesIntersection(pt2, pt3, pt5, pt6, out currIntersection))
                {
                    if (Vector2.DistanceSquared(pt5, currIntersection) < Vector2.DistanceSquared(pt5, Intersection))
                        Intersection = currIntersection;
                    res = true;
                }
                if (LinesIntersection(pt3, pt4, pt5, pt6, out currIntersection))
                {
                    if (Vector2.DistanceSquared(pt5, currIntersection) < Vector2.DistanceSquared(pt5, Intersection))
                        Intersection = currIntersection;
                    res = true;
                }
                if (LinesIntersection(pt4, pt1, pt5, pt6, out currIntersection))
                {
                    if (Vector2.DistanceSquared(pt5, currIntersection) < Vector2.DistanceSquared(pt5, Intersection))
                        Intersection = currIntersection;
                    res = true;
                }
                return res;
            }
            public bool IntersectsLine(Vector2 pt5, Vector2 pt6)
            {
                Vector2 currIntersection;
                if (LinesIntersection(pt1, pt2, pt5, pt6, out currIntersection))
                { return true; }
                if (LinesIntersection(pt2, pt3, pt5, pt6, out currIntersection))
                { return true; }
                if (LinesIntersection(pt3, pt4, pt5, pt6, out currIntersection))
                { return true; }
                if (LinesIntersection(pt4, pt1, pt5, pt6, out currIntersection))
                { return true; }
                return false;
            }
            public bool IntersectsRectangle(Rectangle anotherRect)
            {
                if (IntersectsLine(anotherRect.pt1, anotherRect.pt2)) return true;
                if (IntersectsLine(anotherRect.pt2, anotherRect.pt3)) return true;
                if (IntersectsLine(anotherRect.pt3, anotherRect.pt4)) return true;
                if (IntersectsLine(anotherRect.pt4, anotherRect.pt1)) return true;
                return false;
            }
        }
        static public Vector3 CameraPosition;
        static Matrix ViewProj;
        #region IGame Members

        void IGame.SetText(string Text)
        {
            playersText[CurrentPlayer] = Text;
        }

        int IGame.UnitsCount
        {
            get { return units.Count; }
        }

        IUnit IGame.GetUnit(int Index)
        {
            return (IUnit)units[Index];
        }

        int IGame.ShotsCount
        {
            get { return shots.shots.Count; }
        }

        IShot IGame.GetShot(int Index)
        {
            return (IShot)shots.shots[Index];
        }
         
        public void GetNearUnits(GameVector Position, float Radius,out List<IUnit> NearUnits,out List<IShot>  NearShots)
        {
            List<Unit> nearUnits;
            List<Shots.Shot> nearShots;
            gameObjects.GetNearObjects(new Vector2( Position.X,Position.Y), Radius, out nearUnits, out nearShots);
            NearUnits = new List<IUnit>();
            foreach (Unit unit in nearUnits)
            {
                NearUnits.Add((IUnit)unit);
            }
            NearShots= new List<IShot>();
            foreach (IShot shot in nearShots)
            {
                NearShots.Add((IShot)shot);
            }
        }

        public float Time
        {
            get
            {
                return timing.NowTime;
            }
        }

        public float TimeElapsed
        {
            get
            {
                return timing.DeltaTime;
            }
        }

        #endregion
    }
}
