﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        }
        List<IAI> players;
        List<string> playersText;
        public Core(int ScreenWidth, int ScreenHeight, ContentManager content, GraphicsDeviceManager graphics)
        {
            timing = new TimingClass();
            viewer = new Viewer(ScreenWidth, ScreenHeight, content, graphics);
            units = new List<Unit>();
            shots = new Shots(units);
        }
        public class TimingClass
        {
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
                    //deltaTime = 0.015f;
                    deltaTime *= timeSpeed;
                    nowTime += deltaTime;
                }
                else
                    deltaTime = 0.0f;
                prevTime = currTime;
            }
            public const float maxDeltaTime = 0.05f;
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
                    if (currValue > max) currValue = max;
                    if (currValue < min) currValue = min;
                }
                if (isAngle) currValue = AngleClass.Normalize(currValue);
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
            public bool RotateCCWToAngle(float AimedAngle, out bool AimIsNear)
            {
                if (Math.Abs(AimedAngle - Value) < MathHelper.Pi / 180f * 1) AimIsNear = true;
                else AimIsNear = false;
                AimedAngle = AngleClass.Normalize(AimedAngle);
                if (aimedValue > Value) return true;
                //if (aimedValue - Value > 0 && aimedValue - Value < MathHelper.Pi) return true;
                //if (aimedValue - Value + MathHelper.TwoPi > 0 && aimedValue - Value + MathHelper.TwoPi < MathHelper.Pi) return true;
                return false;
                //if (aimedValue - Value < 0 && aimedValue - Value >- MathHelper.Pi) return false;
                //if (aimedValue - Value - MathHelper.TwoPi < 0 && aimedValue - Value - MathHelper.TwoPi > -MathHelper.Pi) return false;
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
                    shots.Add(new Shots.Shot(owner.position + new Vector2(owner.Forward.X, owner.Forward.Y) * owner.size.Y * 0.6f,
                        owner.ForwardVector * speed, Damage, lifeTime,owner.ShipType));
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
                maxTimeAfterDeath = 5;
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
                        speed = new DerivativeControlledParameter(0, 0, 20, 9, false);
                        rotationSpeed = new DerivativeControlledParameter(0,-0.72f,0.72f,0.5f,false);
                        rotationAngle = new DerivativeControlledParameter(Angle, -MathHelper.Pi, MathHelper.Pi, 1000, true);
                        gun = new Gun(3,50,9,15);
                        gun.owner = this;
                        this.hp = 80;
                        this.team = Player;
                        maxTimeAfterDeath = 5;
                        timeAfterDeath = 0;
                        IsAliveInPrevLoop = true;
                        this.shots = Core.shots;
                        break;
                    case ShipTypes.Corvette:
                        blowDamage = 150;
                        blowRadius = 120;
                        maxTimeAfterDeath = 8;
                        speed = new DerivativeControlledParameter(0, 0, 5, 1, false);
                        rotationSpeed = new DerivativeControlledParameter(0, -0.32f, 0.32f, 0.2f, false);
                        gun = new Gun(4, 50, 18, 40);                        
                        this.hp = 400;

                        this.team = Player;                        
                        IsAliveInPrevLoop = true;
                        this.shots = Core.shots;
                        timeAfterDeath = 0;
                        name = Name;
                        position = Position;
                        size = CorvetteSize;
                        rotationAngle = new DerivativeControlledParameter(Angle, -MathHelper.Pi, MathHelper.Pi, 1000, true);
                        gun.owner = this;
                        break;
                    case ShipTypes.Cruiser:
                        blowDamage = 300;
                        blowRadius = 120;
                        maxTimeAfterDeath = 12;                        
                        speed = new DerivativeControlledParameter(0, 0, 2, 1.0f, false);
                        rotationSpeed = new DerivativeControlledParameter(0, -0.07f, 0.07f, 0.04f, false);                        
                        gun = new Gun(15, 50, 27, 200);
                        this.hp = 800;

                        this.team = Player;
                        IsAliveInPrevLoop = true;
                        this.shots = Core.shots;
                        timeAfterDeath = 0;
                        name = Name;
                        position = Position;
                        size = CruiserSize;
                        rotationAngle = new DerivativeControlledParameter(Angle, -MathHelper.Pi, MathHelper.Pi, 1000, true);
                        gun.owner = this;
                        break;
                }
            }

            internal void SetHP(float value) { hp = value; }
            #region IUnit Members
            public float HP { get { return hp; } }
            public string Name
            {
                get { return name; }
            }
            public MiniGameInterfaces.GameVector Position
            {
                get { return new GameVector(position.X, position.Y); }
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

            #region controlling the unit

            public void Accelerate(float amount)
            {
                if (AccessDenied()) return;
                speed.Derivative = amount;//speed.MaxDerivative;
            }
            public void Accelerate()
            {
                if (AccessDenied()) return;
                speed.Derivative = speed.MaxDerivative;
            }
            public void DeAccelerate()
            {
                if (AccessDenied()) return;
                speed.Derivative = -speed.MaxDerivative;
            }
            public void SetSpeed(float Speed)
            {
                if (AccessDenied()) return;
                speed.SetAimedValue(Speed);
            }
            public void SetAngle(float Angle)
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
                        SetAngle(AngleToTgt);
                        float distanceSq = Vector2.DistanceSquared(position, tgtLocation);
                        float timeToStop = speed.Value / speed.MaxDerivative;
                        float StopDistanceSq = speed.Value * timeToStop - speed.MaxDerivative * timeToStop * timeToStop / 2;
                        if (AngleClass.Distance(GetAngleTo(tgtLocation), rotationAngle.Value) < MathHelper.PiOver4 &&
                            (StopDistanceSq > Vector2.DistanceSquared(position, tgtLocation) || !stopsNearPoint))
                            SetSpeed(MaxSpeed);
                        else SetSpeed(0);
                        //if (distanceSq < 30*30&&speed.Value<10) { goesToPoint = false; }
                    }
                    //hp -= 1;
                    gun.Update();
                    bool AimIsNear;
                    //rotationAngle.RotateCCWToAngle(rotationAngle.AimedValue, out AimIsNear);
                    if (rotationAngle.RotateCCWToAngle(rotationAngle.AimedValue, out AimIsNear))
                    {
                        rotationSpeed.Derivative = rotationSpeed.MaxDerivative;
                    }
                    else
                    {
                        rotationSpeed.Derivative = -rotationSpeed.MaxDerivative;
                    }
                    if (AimIsNear) rotationSpeed.Derivative = -rotationSpeed.Value;
                    rotationSpeed.Update();
                    rotationAngle.Derivative = rotationSpeed.Value;
                    //                rotationAngle.Derivative = (rotationAngle.AimedValue - rotationAngle.Value) * 0.05f;
                    rotationAngle.Update();
                    speed.Update();
                    position += ForwardVector * speed * Timing.DeltaTime;
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
                CruiserTexture, LaserTexture,
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

                environmentEffect.Parameters["Size"].SetValue(new Vector2(20000,20000));

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
                        if (units[currUnit].size == DestroyerSize)
                        {
                            DestroyerBatchParams[CDestroyersInBatch].X = units[currUnit].position.X;
                            DestroyerBatchParams[CDestroyersInBatch].Y = units[currUnit].position.Y;
                            DestroyerBatchParams[CDestroyersInBatch].Z = units[currUnit].RotationAngle;
                            DestroyerBatchParams[CDestroyersInBatch].W = units[currUnit].PlayerOwner;
                            CDestroyersInBatch++;
                            if (CDestroyersInBatch == MaxBatchSize) DrawUnitBatch(DestroyerBatchParams, ref CDestroyersInBatch, DestroyerTexture, DestroyerSize);
                        }
                        if (units[currUnit].size == CorvetteSize)
                        {
                            CorvetteBatchParams[CCorvettesInBatch].X = units[currUnit].position.X;
                            CorvetteBatchParams[CCorvettesInBatch].Y = units[currUnit].position.Y;
                            CorvetteBatchParams[CCorvettesInBatch].Z = units[currUnit].RotationAngle;
                            CorvetteBatchParams[CCorvettesInBatch].W = units[currUnit].PlayerOwner;
                            CCorvettesInBatch++;
                            if (CCorvettesInBatch == MaxBatchSize) DrawUnitBatch(CorvetteBatchParams, ref CCorvettesInBatch, CorvetteTexture, CorvetteSize);
                        }
                        if (units[currUnit].size == CruiserSize)
                        {
                            CruiserBatchParams[CCruisersInBatch].X = units[currUnit].position.X;
                            CruiserBatchParams[CCruisersInBatch].Y = units[currUnit].position.Y;
                            CruiserBatchParams[CCruisersInBatch].Z = units[currUnit].RotationAngle;
                            CruiserBatchParams[CCruisersInBatch].W = units[currUnit].PlayerOwner;
                            CCruisersInBatch++;
                            if (CCruisersInBatch == MaxBatchSize) DrawUnitBatch(CruiserBatchParams, ref CCruisersInBatch, CruiserTexture, CruiserSize);
                        }
                    }
                    currUnit++;
                }
                if (CDestroyersInBatch > 0) DrawUnitBatch(DestroyerBatchParams, ref CDestroyersInBatch, DestroyerTexture, DestroyerSize);
                if (CCorvettesInBatch > 0) DrawUnitBatch(CorvetteBatchParams, ref CCorvettesInBatch, CorvetteTexture, CorvetteSize);
                if (CCruisersInBatch > 0) DrawUnitBatch(CruiserBatchParams, ref CCruisersInBatch, CruiserTexture, CruiserSize);
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
            void DrawUnitBatch(Vector4[] UnitInstanceParams, ref int CUnits, Texture2D Text, Vector2 Size)
            {
                shipEffect.Begin();
                shipEffect.Parameters["Size"].SetValue(Size);
                shipEffect.Parameters["tex"].SetValue(Text);
                shipEffect.Parameters["Positions"].SetValue(UnitInstanceParams);
                EffectPass p = shipEffect.CurrentTechnique.Passes[0];
                p.Begin();
                graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, CUnits * 6, 0, CUnits * 2);
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
                public Shot(Vector2 Pos, Vector2 Dir, float Damage, float LifeTime,ShipTypes OwnerType)
                {
                    switch (OwnerType)
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
        public void Draw()
        {
            Core.viewer.graphics.GraphicsDevice.Clear(Color.Black);
            viewer.DrawEnvironment();
            viewer.DrawUnits(units);
            viewer.DrawShots(shots);
            //
            int[] destroyers = new int[players.Count];
            int[] corvettes = new int[players.Count];
            int[] cruisers = new int[players.Count];
            int[] total = new int[players.Count];
            string[] infoString = new string[players.Count];
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
                infoString[i] = destroyers[i].ToString() + "+" + corvettes[i].ToString() + "+" + cruisers[i].ToString() + "=" + total[i].ToString();
            //
            string[] lines;
            viewer.DrawText(players[0].Author, new Vector2(100, 20), 0, new Color(Core.Viewer.TeamColors[0]));
            viewer.DrawText(players[0].Description, new Vector2(100, 40), 0, Color.Gray);
            viewer.DrawText(infoString[0], new Vector2(100, 60), 0, Color.White);
            lines = playersText[0].Split(new char[] { '\n' });
            for (int i = 0; i < lines.Length; i++)
                viewer.DrawText(lines[i], new Vector2(100, 80 + i * 20), 0, Color.Yellow);
            viewer.DrawText("vs.", new Vector2(Core.viewer.screenWidth / 2, 20), 1, Color.White);
            viewer.DrawText(players[1].Author, new Vector2(Core.viewer.screenWidth - 100, 20), 2, new Color(Core.Viewer.TeamColors[1]));
            viewer.DrawText(players[1].Description, new Vector2(Core.viewer.screenWidth - 100, 40), 2, Color.Gray);
            viewer.DrawText(infoString[1], new Vector2(Core.viewer.screenWidth - 100, 60), 2, Color.White);
            lines = playersText[1].Split(new char[] { '\n' });
            for (int i = 0; i < lines.Length; i++)
                viewer.DrawText(lines[i], new Vector2(Core.viewer.screenWidth - 100, 80 + i * 20), 2, Color.Yellow);
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
        }
        public void Update()
        {
            CurrentPlayer = 0;
            foreach (IAI player in players)
            {
                player.Update();
                CurrentPlayer++;
            }
            CurrentPlayer = -1;
            ViewProj = Matrix.CreateLookAt(CameraPosition, new Vector3(CameraPosition.X, CameraPosition.Y, 0), new Vector3(0, -1, 0)) *
                 Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)viewer.screenWidth / (float)viewer.screenHeight, 10, 100000);
            for (int i = 0; i < units.Count; i++)
            {
                //units[i].Shoot();
                units[i].Update();
                if (units[i].IsDying)
                    DamageAllAround(units[i].position, units[i].BlowRadius, units[i].BlowDamage);
                if (units[i].TimeToDie)
                {
                    units.RemoveAt(i);
                    i--;
                }
            }
            UnitIntersections();
            ShotsWithUnitsIntersections();
            shots.Update();
        }
        private void ShotsWithUnitsIntersections()
        {
            foreach (Unit unit in units)
                if (unit.HP >= 0)
                {
                    Rectangle rect = unit.GetRectangle();
                    BoundingSphere sphere = rect.BoxBoundingSphere;
                    for (int i = 0; i < shots.shots.Count; i++)
                    {
                        if (unit.ShipType == ShipTypes.Destroyer&&Vector2.Distance(unit.position,shots.shots[i].pos)<10) { }
                        if (shots.shots[i].GetBoundingSphere().Intersects(sphere))
                        {
                            if (unit.ShipType ==  ShipTypes.Destroyer) { }
                            if (rect.IntersectsLine(shots.shots[i].pos, shots.shots[i].End))
                            {
                                //if (unit.Name == "Ship2") { }
                                unit.SetHP(unit.HP - shots.shots[i].damage);
                                shots.shots.RemoveAt(i);
                            }
                        }
                    }
                }
        }
        private void DamageAllAround(Vector2 pos, float radius, float Damage)
        {
            foreach (Unit unit in units)
            {
                if (Vector2.DistanceSquared(unit.position, pos) <= radius * radius)
                { unit.SetHP(unit.HP - Damage); }
            }
        }
        private void UnitIntersections()
        {
            for (int i = 0; i < units.Count - 1; i++)
                if (units[i].HP >= 0)
                {
                    Rectangle rect1 = units[i].GetRectangle();
                    BoundingSphere sphere1 = rect1.BoxBoundingSphere;
                    for (int j = i + 1; j < units.Count; j++)
                        if (units[j].HP >= 0)
                        {
                            Rectangle rect2 = units[j].GetRectangle();
                            if (sphere1.Intersects(rect2.BoxBoundingSphere))
                            {
                                if (rect1.IntersectsRectangle(rect2))
                                {
                                    float hp1 = units[i].HP;
                                    float hp2 = units[j].HP;
                                    units[i].SetHP(hp1 - hp2 * 1.5f);
                                    units[j].SetHP(hp2 - hp1 * 1.5f);
                                }
                            }
                        }
                }
        }
        System.Collections.Generic.List<Unit> units;
        public static Vector2 DestroyerSize = new Vector2(25, 25);
        public static Vector2 CorvetteSize = new Vector2(20, 80);
        public static Vector2 CruiserSize = new Vector2(40, 160);
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
                CreateUnitsForPlayer(currTeam,CCruisers,CCorvettes,CDestroyers,new Vector2(0,currTeam*1000));
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
            float angle = (currTeam > 0) ? MathHelper.Pi : 0;
            for (int i = 0; i < CCruisers; i++)
            {
                units.Add(new Unit(ShipTypes.Cruiser, currTeam, pos + new Vector2(150 * i, 0), angle, "Cruiser -" + i.ToString() + "-"));
            }
            for (int i = 0; i < CCorvettes; i++)
            {
                units.Add(new Unit(ShipTypes.Corvette, currTeam, pos + new Vector2(150 * (i + CCruisers), 0), angle, "Corvette -" + i.ToString() + "-"));
            }
            for (int i = 0; i < CDestroyers; i++)
            {
                units.Add(new Unit(ShipTypes.Destroyer, currTeam, pos + new Vector2(150 * (i + CCruisers + CCorvettes), 0), angle, "Destroyer -" + i.ToString() + "-"));
            }

        }
        public void Reset(List<IAI> Players)
        {
            players = Players;
            playersText = new List<string>();
            for (int i = 0; i < players.Count; i++)
                playersText.Add("");
            for (int i = 0; i < players.Count; i++)
                players[i].Init(i, this);
            units.Clear();
            shots.Clear();
            AddUnits();
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

        #endregion
    }
}