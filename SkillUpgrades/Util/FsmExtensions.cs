using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace SkillUpgrades.Util
{
    public static class FsmExtensions
    {
        public static void SwapXandY(this GetVelocity2d self)
        {
            (self.x, self.y) = (self.y, self.x);
        }
        public static void SwapXandY(this SetVelocity2d self)
        {
            (self.x, self.y) = (self.y, self.x);
        }

        public static void SetBottomToLeft(this CheckCollisionSide self)
        {
            (self.bottomHitEvent, self.leftHitEvent) = (self.leftHitEvent, self.bottomHitEvent);
        }
        public static void SetBottomToRight(this CheckCollisionSide self)
        {
            (self.bottomHitEvent, self.rightHitEvent) = (self.rightHitEvent, self.bottomHitEvent);
        }

        public static FsmFloat AddFsmFloat(this PlayMakerFSM fsm, string name)
        {
            FsmFloat newFsmFloat = new(name);

            FsmFloat[] floatVariables = new FsmFloat[fsm.FsmVariables.FloatVariables.Length + 1];
            Array.Copy(fsm.FsmVariables.FloatVariables, floatVariables, fsm.FsmVariables.FloatVariables.Length);
            floatVariables[fsm.FsmVariables.FloatVariables.Length] = newFsmFloat;
            fsm.FsmVariables.FloatVariables = floatVariables;

            return newFsmFloat;
        }
    }
}
