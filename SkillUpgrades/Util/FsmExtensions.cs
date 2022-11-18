using System;
using System.Linq;
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

        public static void RedirectTransitionTo(this FsmState state, string origTarget, string newTarget)
        {
            FsmState newTargetState = state.Fsm.GetState(newTarget);
            foreach (FsmTransition trans in state.Transitions.Where(t => t.ToFsmState?.Name == origTarget))
            {
                SkillUpgrades.instance.LogDebug($"RTT: {state.Name} -> {origTarget} to {newTarget}");
                trans.ToFsmState = newTargetState;
                trans.ToState = newTarget;
            }
        }

        /// <summary>
        /// Make all transitions to origTarget instead point to newTarget
        /// Doesn't work with global transitions pointing to origTarget
        /// </summary>
        public static void RedirectAllTransitionsTo(this PlayMakerFSM fsm, string origTarget, string newTarget)
        {
            foreach (FsmState state in fsm.FsmStates)
            {
                state.RedirectTransitionTo(origTarget, newTarget);
            }
        }
    }
}
