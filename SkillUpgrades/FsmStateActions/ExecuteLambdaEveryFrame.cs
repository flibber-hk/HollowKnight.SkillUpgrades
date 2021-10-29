using System;
using HutongGames.PlayMaker;

namespace SkillUpgrades.FsmStateActions
{
    public class ExecuteLambdaEveryFrame : FsmStateAction
    {
        private readonly Action<bool> _method;

        /// <summary>
        /// FsmStateAction to execute the given method every frame. The method will be passed true the first frame, and false on subsequent frames.
        /// </summary>
        public ExecuteLambdaEveryFrame(Action<bool> method)
        {
            _method = method;
        }
        public ExecuteLambdaEveryFrame(Action method)
        {
            _method = (firstFrame) => method();
        }

        public override void OnEnter()
        {
            try
            {
                _method(true);
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
                _method(false);
            }
            catch (Exception e)
            {
                LogError("Error in ExecuteLambdaEveryFrame (OnUpdate):\n" + e);
            }
        }
    }
}