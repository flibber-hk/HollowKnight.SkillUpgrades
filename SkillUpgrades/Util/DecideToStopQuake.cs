using System;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace SkillUpgrades.Util
{
    internal class DecideToStopQuake : ComponentAction<Rigidbody2D>
    {
        private readonly FsmFloat _hSpeed;
        private readonly FsmFloat _vSpeed;

        public DecideToStopQuake(FsmFloat hSpeed, FsmFloat vSpeed)
        {
            _hSpeed = hSpeed;
            _vSpeed = vSpeed;
        }


        public override void OnEnter()
        {
            try
            {
                UpdateCache(Fsm.FsmComponent.gameObject);
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopQuake (OnEnter/UpdateCache):\n" + e);
            }

            try
            {
                DecideToStop();
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopQuake (OnEnter):\n" + e);
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
                LogError("Error in DecideToStopQuake (OnUpdate):\n" + e);
            }
        }

        private void DecideToStop()
        {
            bool shouldStop = false;

            // Check Collision Side
            // The code for this is quite complicated so I'll just do some cursed modification of the CheckCollisionSide action

            // GetVelocity
            Vector2 vector = rigidbody2d.velocity;

            if (Math.Abs(_hSpeed.Value) >= 0.4f && Math.Abs(vector.x) < 0.2f) shouldStop = true;
            if (Math.Abs(_vSpeed.Value) >= 0.4f && Math.Abs(vector.y) < 0.2f) shouldStop = true;

            if (shouldStop)
            {
                Fsm.FsmComponent.SendEvent("HERO LANDED");
            }
        }
    }
}
