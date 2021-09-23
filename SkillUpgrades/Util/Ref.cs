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

        private static Collider2D _heroCollider;
        public static Collider2D HeroCollider
        {
            get
            {
                if (_heroCollider == null) _heroCollider = HeroController.instance.GetComponent<Collider2D>();
                return _heroCollider;
            }
            internal set => _heroCollider = value;
        }
    }
}
