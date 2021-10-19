using System;
using HutongGames.PlayMaker;

namespace SkillUpgrades.FsmStateActions
{
    internal class ExecuteLambdaEveryFrame : FsmStateAction
    {
        private readonly Action _method;

        public ExecuteLambdaEveryFrame(Action method)
        {
            _method = method;
        }

        public override void OnEnter()
        {
            try
            {
                _method();
            }
            catch (Exception e)
            {
                LogError("Error in ExecuteLambdaEveryFrame (OnEnter):\n" + e);
            }
        }

        public override void OnUpdate()
        {
            try
            {
                _method();
            }
            catch (Exception e)
            {
                LogError("Error in ExecuteLambdaEveryFrame (OnUpdate):\n" + e);
            }
        }
    }
}