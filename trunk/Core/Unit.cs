using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using Microsoft.Xna.Framework;

namespace CoreNamespace
{
    public class Unit : IUnit
    {
        string text;
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
        internal GameVector position;
        /// <summary>
        /// Ship is a square. X is a width, Y - length. 
        /// </summary>
        internal GameVector size;
        /// <summary>
        /// radian per second
        /// </summary>
        DerivativeControlledParameter speed;
        DerivativeControlledParameter rotationSpeed, rotationAngle;
        public GameVector ForwardVector
        {
            get { return new GameVector((float)Math.Cos(rotationAngle), (float)Math.Sin(rotationAngle)); }
        }
        Gun gun;
        internal float timeAfterDeath;
        internal float maxTimeAfterDeath;
        private bool IsAliveInPrevLoop;
        
        
        GameVector tgtLocation;
        Shots shots;
        public Unit(ShipTypes ShipType, string Name, GameVector Position, GameVector Size, DerivativeControlledParameter Speed,
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
            maxTimeAfterDeath = 5 * 0.2f;
            timeAfterDeath = 0;
            IsAliveInPrevLoop = true;
            this.shots = Core.shots;
        }
        public Unit(ShipTypes ShipType, int Player, GameVector Position, float Angle, string Name)
        {
            shipType = ShipType;
            switch (shipType)
            {
                case ShipTypes.Destroyer:
                    blowDamage = 70;
                    blowRadius = 40;
                    name = Name;
                    position = Position;
                    size = Core.DestroyerSize;
                    speed = new DerivativeControlledParameter(0, 0, 20 * TimingClass.SpeedsMultiplier, 9 * TimingClass.SpeedsMultiplier, false);
                    rotationSpeed = new DerivativeControlledParameter(0, -0.22f * TimingClass.SpeedsMultiplier, 0.22f * TimingClass.SpeedsMultiplier, 0.5f * TimingClass.SpeedsMultiplier, false);
                    rotationAngle = new DerivativeControlledParameter(Angle, -MathHelper.Pi, MathHelper.Pi, 1000, true);
                    gun = new Gun(1, 50 * TimingClass.SpeedsMultiplier, 9 / TimingClass.SpeedsMultiplier, 15);
                    gun.owner = this;
                    this.hp = 80;
                    this.team = Player;
                    maxTimeAfterDeath = 5 * 0.2f;
                    timeAfterDeath = 0;
                    IsAliveInPrevLoop = true;
                    this.shots = Core.shots;
                    break;
                case ShipTypes.Corvette:
                    blowDamage = 150;
                    blowRadius = 120;
                    maxTimeAfterDeath = 5 * 0.2f;
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
                    size = Core.CorvetteSize;
                    //size.Y *= 0.9f; //real object size (see texture)
                    rotationAngle = new DerivativeControlledParameter(
                        Angle
                        , -MathHelper.Pi, MathHelper.Pi, 1000, true);
                    gun.owner = this;
                    break;
                case ShipTypes.Cruiser:
                    blowDamage = 300;
                    blowRadius = 150;
                    maxTimeAfterDeath = 8 * 0.2f;
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
                    size = Core.CruiserSize;
                    //size.X *= 0.7f; //real object size (see texture)
                    //size.Y *= 0.9f; //real object size (see texture)
                    rotationAngle = new DerivativeControlledParameter(
                        Angle
                        , -MathHelper.Pi, MathHelper.Pi, 1000, true);
                    gun.owner = this;
                    break;
            }
        }
        internal void SetHP(float value) { hp = value; if (hp < 0)  hp = -1; }
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
            get
            {
                return new GameVector(ForwardVector.X, ForwardVector.Y);
            }
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
        public float RotationSpeed
        {
            get { return rotationSpeed.Value; }
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
        public float AngleTo(GameVector target)
        {
            return GetAngleTo(new GameVector(target.X, target.Y));
        }
        public bool IntersectsSector(GameVector pt1, GameVector pt2)
        {
            GameVector intersection;
            return this.GetRectangle().IntersectsLine(
                pt1, pt2, out intersection);
        }
        #region controlling the unit

        public void Accelerate(float amount)
        {
            if (AccessDenied()) return;
            speed.DisableAim();
            speed.Derivative = amount;//speed.MaxDerivative;
            goesToPoint = false;
        }
        public void Accelerate()
        {
            if (AccessDenied()) return;
            speed.DisableAim();
            speed.Derivative = speed.MaxDerivative;
            goesToPoint = false;
        }
        public void DeAccelerate()
        {
            if (AccessDenied()) return;
            speed.DisableAim();
            speed.Derivative = -speed.MaxDerivative;
            goesToPoint = false;
        }
        public void RotationAccelerate(float amount)
        {
            if (AccessDenied()) return;
            rotationAngle.DisableAim();
            rotationSpeed.Derivative = amount;
            goesToPoint = false;
        }
        
        public bool Shoot()
        {
            if (AccessDenied()) return false;
            bool res = gun.Shoot();
            //shots.Add(new Shots.Shot(position + Forward * 50, position + Forward * (gun.MaxDistance+50), gun.Damage));
            return res;
        }
        
        #endregion
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }
        #endregion
        private float GetAngleTo(GameVector Target)
        {
            return (float)Math.Atan2(Target.Y - position.Y, Target.X - position.X);
        }
        public bool TimeToDie
        { get { return timeAfterDeath >= maxTimeAfterDeath; } }
        #region obsolete
        bool goesToPoint;

        bool stopsNearPoint;
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
        public void GoTo(GameVector TargetLocation, bool Stop)
        {
            if (AccessDenied()) return;
            goesToPoint = true;
            stopsNearPoint = Stop;
            tgtLocation = new GameVector(TargetLocation.X, TargetLocation.Y);

        }
#endregion

        internal void Update()
        {
            if (hp >= 0)
            {
                #region obsolete
                if (goesToPoint)
                {
                    float AngleToTgt = GetAngleTo(tgtLocation);
                    SetAngleGoingToTgt(AngleToTgt);
                    float distanceSq = GameVector.DistanceSquared(position, tgtLocation);
                    float timeToStop = speed.Value / speed.MaxDerivative;
                    float StopDistanceSq = speed.Value * timeToStop - speed.MaxDerivative * timeToStop * timeToStop / 2;
                    if (AngleClass.Distance(GetAngleTo(tgtLocation), rotationAngle.Value) < MathHelper.PiOver4 &&
                        (StopDistanceSq < GameVector.DistanceSquared(position, tgtLocation) || !stopsNearPoint))
                        SetSpeedGoingToTgt(MaxSpeed);
                    else SetSpeedGoingToTgt(0);
                    //if (distanceSq < 30*30&&speed.Value<10) { goesToPoint = false; }
                }
                #endregion
                gun.Update();
                if (rotationAngle.AimEnabled)
                {
                    float AimIsNearDecrementing;
                    //rotationAngle.RotateCCWToAngle(rotationAngle.AimedValue, out AimIsNear);
                    if (rotationAngle.RotateCCWToAngle(rotationAngle.AimedValue, out AimIsNearDecrementing))
                    {
                        if (rotationSpeed.Value < 0)
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
                    //if (StopRotation && Math.Abs(rotationSpeed.Value) < 0.08f) 
                    //    rotationSpeed = new DerivativeControlledParameter(0, rotationSpeed.Min, rotationSpeed.Max, rotationSpeed.MaxDerivative, false);
                }
                //rotationSpeed.Derivative *= AimIsNearDecrementing;
                
                rotationSpeed.Update();
                rotationAngle.Derivative = rotationSpeed.Value;
                //                rotationAngle.Derivative = (rotationAngle.AimedValue - rotationAngle.Value) * 0.05f;
                rotationAngle.Update();
                speed.Update();
                position += ForwardVector * speed.Value * Core.Timing.DeltaTime;
                isDying = false;
            }
            else
            {
                if (IsAliveInPrevLoop) isDying = true;
                else isDying = false;
                timeAfterDeath += Core.Timing.DeltaTime;
                IsAliveInPrevLoop = false;
            }
        }
        bool isDying;
        internal bool IsDying
        {
            get { return isDying; }
        }
        internal MiniGameInterfaces.Rectangle GetRectangle()
        {
            GameVector forward = ForwardVector;
            GameVector right = new GameVector(forward.Y, -forward.X);
            forward *= size.X * 0.5f;
            right *= size.Y * 0.5f;
            return new MiniGameInterfaces.Rectangle(forward - right + position, forward + right + position,
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
            logicY = Y;
        }
    }
}
