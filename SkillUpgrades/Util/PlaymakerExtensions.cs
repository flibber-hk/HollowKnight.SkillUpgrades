using System;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;

namespace SkillUpgrades.Util
{
    public static class PlayMakerExtensions
    {
        public static void AddState(this PlayMakerFSM self, FsmState state)
        {
            Fsm fsm = ReflectionHelper.GetField<PlayMakerFSM, Fsm>(self, "fsm");
            FsmState[] states = ReflectionHelper.GetField<Fsm, FsmState[]>(fsm, "states");

            FsmState[] newStates = new FsmState[states.Length + 1];
            Array.Copy(states, newStates, states.Length);
            newStates[states.Length] = state;

            ReflectionHelper.SetField(fsm, "states", newStates);
        }

        // Because copying states doesn't copy the transitions properly
        public static void FixTransitions(this FsmState state)
        {
            foreach (FsmTransition trans in state.Transitions)
            {
                trans.ToFsmState = state.Fsm.GetState(trans.ToState);
            }
        }

        public static FsmState GetState(this PlayMakerFSM self, string name)
        {
            return self.FsmStates.FirstOrDefault(state => state.Name == name);
        }

        public static void RemoveActionsOfType<T>(this FsmState self) where T : FsmStateAction
        {
            self.Actions = self.Actions.Where(action => !(action is T)).ToArray();
        }

        public static T GetActionOfType<T>(this FsmState self) where T : FsmStateAction
        {
            return self.Actions.OfType<T>().FirstOrDefault();
        }

        public static T[] GetActionsOfType<T>(this FsmState self) where T : FsmStateAction
        {
            return self.Actions.OfType<T>().ToArray();
        }

        public static void ClearTransitions(this FsmState self)
        {
            self.Transitions = new FsmTransition[0];
        }

        public static void RemoveTransitionsTo(this FsmState self, string toState)
        {
            self.Transitions = self.Transitions.Where(transition => transition.ToState != toState).ToArray();
        }

        public static void AddTransition(this FsmState self, string eventName, string toState)
        {
            FsmTransition[] transitions = new FsmTransition[self.Transitions.Length + 1];
            Array.Copy(self.Transitions, transitions, self.Transitions.Length);
            self.Transitions = transitions;

            FsmTransition trans = new FsmTransition
            {
                ToState = toState,
                ToFsmState = self.Fsm.GetState(toState),
                FsmEvent = FsmEvent.EventListContains(eventName)
                    ? FsmEvent.GetFsmEvent(eventName)
                    : new FsmEvent(eventName)
            };


            self.Transitions[self.Transitions.Length - 1] = trans;
        }

        public static void AddFirstAction(this FsmState self, FsmStateAction action)
        {
            FsmStateAction[] actions = new FsmStateAction[self.Actions.Length + 1];
            Array.Copy(self.Actions, 0, actions, 1, self.Actions.Length);
            actions[0] = action;

            self.Actions = actions;
        }

        public static void AddAction(this FsmState self, FsmStateAction action)
        {
            FsmStateAction[] actions = new FsmStateAction[self.Actions.Length + 1];
            Array.Copy(self.Actions, actions, self.Actions.Length);
            actions[self.Actions.Length] = action;

            self.Actions = actions;
        }

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
            FsmFloat newFsmFloat = new FsmFloat(name);

            FsmFloat[] floatVariables = new FsmFloat[fsm.FsmVariables.FloatVariables.Length + 1];
            System.Array.Copy(fsm.FsmVariables.FloatVariables, floatVariables, fsm.FsmVariables.FloatVariables.Length);
            floatVariables[fsm.FsmVariables.FloatVariables.Length] = newFsmFloat;
            fsm.FsmVariables.FloatVariables = floatVariables;

            return newFsmFloat;
        }
    }
}