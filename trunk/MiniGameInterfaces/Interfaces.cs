﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniGameInterfaces
{
    public struct GamePoint
    {
        public float X;
        public float Y;
        public GamePoint(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
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
        /// position in the world. in pixels
        /// </summary>
        GamePoint Position { get; }
        /// <summary>
        /// looking direction. unit vector
        /// </summary>
        GamePoint Forward { get; }
        /// <summary>
        /// time to recharge gun in seconds
        /// </summary>
        float TimeToRecharge { get; }
        /// <summary>
        /// rotation angle in radians
        /// </summary>
        float RotationAngle { get; }
        /// <summary>
        /// player owner
        /// </summary>
        int PlayerOwner { get; }
        /// <summary>
        /// unit current hit points 
        /// </summary>
        float HP { get; }
        #endregion
        #region Getting unit characteristics
        /// <summary>
        /// in pixels. X is a width, Y is a length
        /// </summary>
        GamePoint Size { get; }
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
        void GoTo(GamePoint TargetLocation, bool Stop);
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
        GamePoint Position { get; }
        /// <summary>
        /// velocity vector
        /// </summary>
        GamePoint Direction { get; }
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