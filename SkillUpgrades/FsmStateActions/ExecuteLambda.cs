using System;
using HutongGames.PlayMaker;

namespace SkillUpgrades.FsmStateActions
{
    internal class ExecuteLambda : FsmStateAction
    {
        private readonly Action _method;

        public ExecuteLambda(Action method)
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
                LogError("Error in ExecuteLambda:\n" + e);
            }

            Finish();
        }
    }
}