using UnityEngine;

namespace SkillUpgrades.Util
{
    public static class Ref
    {
        private static Rigidbody2D _heroRigidBody;
        public static Rigidbody2D HeroRigidBody
        {
            get
            {
                if (_heroRigidBody == null) _heroRigidBody = HeroController.instance.GetComponent<Rigidbody2D>();
                return _heroRigidBody;
            }
        }
    }
}
