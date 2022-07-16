using UnityEngine;

namespace SkillUpgrades.Components
{
    /// <summary>
    /// Translates the GameObject by Velocity each second.
    /// </summary>
    public class Mover : MonoBehaviour
    {
        public Vector2 Velocity;

        void FixedUpdate()
        {
            Vector2 current = transform.position;
            transform.SetPosition2D(current + Velocity * Time.fixedDeltaTime);
        }
    }
}
