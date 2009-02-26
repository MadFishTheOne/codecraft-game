using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace CoreNamespace
{
    public struct DerivativeControlledParameter
    {
        float currValue;
        float prevValue;
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
        public bool AimEnabled
        { get { return aimEnabled; } }
        bool isAngle;
        public DerivativeControlledParameter(float CurrValue, float Min, float Max, float MaxDerivative, bool IsAngle)
        {
            prevValue = CurrValue;
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
            prevValue = currValue;
            if (aimEnabled)
            {
                float DeltaValue = maxDerivative * Core.Timing.DeltaTime;
                if (currValue > aimedValue + DeltaValue) currValue -= maxDerivative;
                else
                {
                    if (currValue < aimedValue - DeltaValue) currValue += maxDerivative;
                    else currValue = aimedValue;
                }
            }
            else
            {
                currValue += currDerivative * Core.Timing.DeltaTime;
            }
            if (isAngle) currValue = AngleClass.Normalize(currValue);
            else
            {
                if (currValue > max) currValue = max;
                if (currValue < min) currValue = min;
                if (prevValue * currValue < 0) currValue = 0;
            }
        }
        public float Value
        {
            get { return currValue; }
        }
        public void SetAimedValue(float AimedValue)
        {
            aimedValue = AimedValue;
            if (isAngle) aimedValue = AngleClass.Normalize(aimedValue);
            else
            {
                if (aimedValue > max) aimedValue = max;
                if (aimedValue < min) aimedValue = min;
            }
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
        public bool RotateCCWToAngle(float AimedAngle, out float AimIsNearDecrementing)//,out bool Equals)
        {
            float angleDist = AngleClass.Distance(AimedAngle, Value);
            //    Equals = false;
            if (angleDist < MathHelper.Pi / 180f * 30)
            {
                AimIsNearDecrementing = angleDist / (MathHelper.Pi / 180f * 30);
                //if (angleDist < MathHelper.Pi / 180f * 1)
                //{
                //    Equals = true;
                //}
            }
            else AimIsNearDecrementing = 1;
            return AngleClass.Difference(Value, AimedAngle) < 0;
            //AimedAngle = AngleClass.Normalize(AimedAngle);
            //if (aimedValue > Value) return true;                
            //return false;
        }
    }
}
