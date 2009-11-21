using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniGameInterfaces;

namespace AINamespace
{
    /// <summary>
    /// vector, whoose value can be relative to some units' position
    /// </summary>
    public class RelativeVector
    {
        /// <summary>
        /// unit to be used as pivot
        /// </summary>
        IUnit relativeUnit1, relativeUnit2, relativeUnit3;
        /// <summary>
        /// angle between pivot.forward and direction from pivot to vector
        /// </summary>
        float angle;
        /// <summary>
        /// dist from pivot to vector
        /// </summary>
        float dist;      
        enum Modes
        {
            ConstVector, UnitsSideAndDist, BetweenUnits, BetweenUnitsNearOne,UnitPosition
        }
        Modes mode;
        GameVector constValue;
        /// <summary>
        /// value is relative to unit's position
        /// </summary>
        /// <param name="RelativeUnit">unit to be used as a pivot</param>
        /// <param name="Angle">angle between pivot.forward and direction from pivot to this vector</param>
        /// <param name="Dist">dist from pivot to this vector</param>
        public RelativeVector(IUnit RelativeUnit, float Angle, float Dist)
        {
            relativeUnit1 = RelativeUnit;
            dist = Dist;
            angle = Angle;
            rotationOnAngle = Matrix.CreateRotation(angle);
            mode = Modes.UnitsSideAndDist;
        }
        /// <summary>
        /// value is constant vector
        /// </summary>
        /// <param name="value">value vector</param>
        public RelativeVector(GameVector value)
        {
            constValue = value;
            mode = Modes.ConstVector;
        }
        /// <summary>
        /// vlaue is between units
        /// </summary>
        /// <param name="unit1">first unit</param>
        /// <param name="unit2">second unit</param>
        public RelativeVector(IUnit unit1, IUnit unit2)
        {
            relativeUnit1 = unit1;
            relativeUnit2 = unit2;
            mode = Modes.BetweenUnits;
        }
        /// <summary>
        /// value is on line between units and on the dist form unit1
        /// </summary>
        /// <param name="unit1">first unit</param>
        /// <param name="unit2">second unit</param>
        /// <param name="Dist">distance from unit1</param>
        public RelativeVector(IUnit unit1, IUnit unit2, float Dist)
        {
            relativeUnit1 = unit1;
            relativeUnit2 = unit2;
            dist = Dist;
            mode = Modes.BetweenUnitsNearOne;
            
        }
        /// <summary>
        /// value is unit1.Position
        /// </summary>
        /// <param name="unit1">unit</param>
        public RelativeVector(IUnit unit1)
        {
            relativeUnit1 = unit1;
            mode = Modes.UnitPosition;
        }
        Matrix rotationOnAngle;
        /// <summary>
        /// true if related units are dead
        /// </summary>
        public bool Invalid
        {
            get
            {
                if (relativeUnit1 != null && relativeUnit1.Dead)
                { return true; }
                if (relativeUnit2 != null && relativeUnit2.Dead)
                { return true; }
                if (relativeUnit3 != null && relativeUnit3.Dead)
                { return true; }
                return false;
            }
        }
        /// <summary>
        /// gets a GameVector value of this RelativeVector 
        /// </summary>
        public GameVector Value
        {
            get
            {
                GameVector toValue;
                switch (mode)
                {
                    case Modes.ConstVector: return constValue;
                        
                    case Modes.UnitsSideAndDist:
                        toValue = Matrix.Mull(GameVector.UnitX//relativeUnit1.Forward//rotation not relative to unit's rotation
                            , rotationOnAngle) * dist;
                        return relativeUnit1.Position + toValue;
                    case Modes.BetweenUnits:
                        return (relativeUnit1.Position + relativeUnit2.Position) * 0.5f;
                    case Modes.BetweenUnitsNearOne:
                        GameVector toTgt = relativeUnit2.Position - relativeUnit1.Position;
                        float toTgtLength = toTgt.Length();
                        toValue = toTgt * (dist / toTgtLength);
                        return relativeUnit1.Position + toValue;
                    case Modes.UnitPosition:
                        return relativeUnit1.Position;
                        
                    default: return constValue;
                }

            }
        }

    }
}
