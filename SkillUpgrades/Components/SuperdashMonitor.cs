using UnityEngine;

namespace SkillUpgrades.Components
{
    /// <summary>
    /// Component that sets the gameobject's collider to enabled iff the knight is superdashing.
    /// </summary>
    public class SuperdashMonitor : MonoBehaviour
    {
        private Collider2D _col;

        void Start()
        {
            _col = GetComponent<Collider2D>();
        }

        void Update()
        {
            if (!_col.enabled && HeroController.instance.cState.superDashing)
            {
                _col.enabled = true;
            }
            else if (_col.enabled && !HeroController.instance.cState.superDashing)
            {
                _col.enabled = false;
            }
        }
    }
}
