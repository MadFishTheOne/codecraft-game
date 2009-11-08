using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;
using System.Collections;

namespace CoreNamespace
{
    //this class incapsulates iterator on near objects to specified
    public class NearObjectsIterator : INearObjectsIterator
    {
        public const float MaxRadius = 500;
        GameVector Center;
        float Radius;
        int RadiusLogic;//in gameobjects grid
        int minX, maxX, minY, maxY, currX, currY;
        int UnitInd;//index of the next unit in one cell of gameobjects grid
        ArrayList[,] gameObjects;

        internal NearObjectsIterator(GameVector Center, float Radius, ArrayList[,] gameObjects)
        {
            this.gameObjects = gameObjects;
            this.Radius = Radius;
            UpdateCenter(Center);
            Reset();
        }
        public void UpdateCenter(GameVector NewCenter)
        {
            Center = NewCenter;
            RadiusLogic = (int)(Radius / GameObjectsClass.cellSize) + 1;
            int X = GameObjectsClass.GetLogicCoo(Center.X);
            int Y = GameObjectsClass.GetLogicCoo(Center.Y);
            minX = (int)Math.Min(Math.Max(X - RadiusLogic, 0), GameObjectsClass.gameObjectsCCells - 1);
            minY = (int)Math.Min(Math.Max(Y - RadiusLogic, 0), GameObjectsClass.gameObjectsCCells - 1);
            maxX = (int)Math.Min(Math.Max(X + RadiusLogic, 0), GameObjectsClass.gameObjectsCCells - 1);
            maxY = (int)Math.Min(Math.Max(Y + RadiusLogic, 0), GameObjectsClass.gameObjectsCCells - 1);
        }
        public void Reset()
        {
            currX = minX;
            currY = minY;
            UnitInd = 0;
        }
        public IUnit NextUnit()
        {
            IUnit curr=null;
            do
            {
                bool founded = false;
                if (gameObjects[currX, currY].Count > UnitInd)
                {
                    founded = true;
                    curr = gameObjects[currX, currY][UnitInd] as IUnit;
                    while ((curr == null)
                        || (curr.Position - Center).LengthSquared() > Radius * Radius)
                    {
                        UnitInd++;

                        if (gameObjects[currX, currY].Count <= UnitInd)
                        {
                            founded = false;
                            break;
                        }
                        else
                            curr = gameObjects[currX, currY][UnitInd] as IUnit;
                    }
                }
                if (founded)
                {
                    UnitInd++;
                    return curr;
                }
                UnitInd = 0;
                currY++;
                if (currY > maxY)
                {
                    currY = minY;
                    currX++;
                }
            }
            while (currX <= maxX);
            return null;
        }
    }
}
