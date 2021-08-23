using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace SkillUpgrades.Util
{
    internal class DecideToStopAction : ComponentAction<Rigidbody2D>
    {
        private readonly FsmFloat _hSpeed;
        private readonly FsmFloat _vSpeed;
        private readonly FsmBool _zeroLast;

        public DecideToStopAction(FsmFloat hSpeed, FsmFloat vSpeed, FsmBool zeroLast)
        {
            _hSpeed = hSpeed;
            _vSpeed = vSpeed;
            _zeroLast = zeroLast;
        }


        public override void OnEnter()
        {
            try
            {
                UpdateCache(Fsm.FsmComponent.gameObject);
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopAction (OnEnter/UpdateCache):\n" + e);
            }

            try
            {
                DecideToStop();
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopAction (OnEnter):\n" + e);
            }
        }

        public override void OnUpdate()
        {
            try
            {
                DecideToStop();
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopAction (OnUpdate):\n" + e);
            }
        }

        private void DecideToStop()
        {
            Vector2 vector = rigidbody2d.velocity;

            if (Math.Abs(_hSpeed.Value) >= 0.1f && Math.Abs(vector.x) < 0.1f) _zeroLast.Value = true;
            if (Math.Abs(_vSpeed.Value) >= 0.1f && Math.Abs(vector.y) < 0.1f) _zeroLast.Value = true;
        }
    }
}
