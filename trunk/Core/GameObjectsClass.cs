using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MiniGameInterfaces;

namespace CoreNamespace
{
    class GameObjectsClass
    {
        internal const int gameObjectsCCells = 46;
        internal const float cellSize = Core.Border * 2 / gameObjectsCCells;
        internal ArrayList[,] gameObjects;
        public GameObjectsClass()
        {
            gameObjects = new ArrayList[gameObjectsCCells, gameObjectsCCells];
            for (int i = 0; i < gameObjectsCCells; i++)
                for (int j = 0; j < gameObjectsCCells; j++)
                {
                    gameObjects[i, j] = new ArrayList();
                }
        }
        internal static int GetLogicCoo(float RealCoo)
        {
            int X = (int)((RealCoo + Core.Border) / (2 * Core.Border) * gameObjectsCCells);
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
       
        public NearObjectsIterator GetNearObjects(GameVector Center, float Radius)
        {
            return new NearObjectsIterator(Center, Radius, gameObjects);            
        }
        public void GetNearObjects(GameVector Position, float Radius, out List<Unit> NearUnits, out List<Shots.Shot> NearShots)
        {
            NearUnits = new List<Unit>();
            NearShots = new List<Shots.Shot>();
            int RadiusLogic = (int)(Radius / cellSize) + 1;// (int)(CruiserSize.Y / (border / (float)gameObjectsCCells)) + 1;
            int X = GetLogicCoo(Position.X);
            int Y = GetLogicCoo(Position.Y);
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
            int X, Y;
            shot.GetLogicCoo(out X, out Y);
            gameObjects[X, Y].Remove(shot);
        }
        internal void RemoveUnit(Unit unit)
        {
            int X, Y;
            unit.GetLogicCoo(out X, out Y);
            gameObjects[X, Y].Remove(unit);
        }
    }
}
