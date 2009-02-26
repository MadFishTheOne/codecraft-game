using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using System.Collections;

namespace CoreNamespace
{
    public class Shots : IEnumerable
    {
        public class Shot : IShot
        {
            public GameVector pos, direction;
            private float size;
            public float Size
            {
                get
                {
                    return size;
                }
            }
            public Circle GetSphere()
            {
                return new Circle((pos + End) * 0.5f, size * 0.5f);
            }
            GameVector forward;
            public GameVector End
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
            public Shot(GameVector Pos, GameVector Dir, float Damage, float LifeTime, Unit ParentUnit)
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
                forward = GameVector.Normalize(direction);
            }
            public void HitSomebody(GameVector where)
            {
                hitSomebody = true;
            }
            public void Update()
            {
                lifeTime -= Core.Timing.DeltaTime;
                pos += direction * Core.Timing.DeltaTime;
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
            GameVector intersection = GameVector.One * float.PositiveInfinity, currIntersection;
            foreach (Unit unit in units)
            {
                if (unit.GetRectangle().IntersectsLine(shot.pos, shot.End, out currIntersection))
                {
                    shot.hitSomebody = true;
                    if (GameVector.DistanceSquared(shot.pos, currIntersection) < GameVector.DistanceSquared(shot.pos, intersection))
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
                    Core.gameObjects.RemoveShot(shots[i]);
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
                currDelay -= Core.Timing.DeltaTime;
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
                Core.shots.Add(new Shots.Shot(owner.position + new GameVector(owner.Forward.X, owner.Forward.Y) * (owner.size.Y * 0.5f + 1),
                    owner.ForwardVector * speed, Damage, lifeTime, owner));
                return true;
            }
            else return false;
        }
    }
}
