using UnityEngine;

namespace SkillUpgrades.Components
{
    /// <summary>
    /// Component that sets the gameobject's collider to enabled iff the knight is superdashing.
    /// </summary>
    public class SuperdashMonitor : MonoBehaviour
    {
        private enum OverrideState
        {
            /// <summary>
            /// Collider is disabled
            /// </summary>
            Disabled,
            /// <summary>
            /// Collider should be disabled, but we are overriding it
            /// </summary>
            Overridden,
            /// <summary>
            /// Collider is enabled
            /// </summary>
            Enabled
        }

        private Collider2D _col;
        private OverrideState ColliderOverrideState;

        void Start()
        {
            _col = GetComponent<Collider2D>();
            ColliderOverrideState = _col.enabled ? OverrideState.Enabled : OverrideState.Disabled;
        }

        void Update()
        {
            switch (ColliderOverrideState)
            {
                case OverrideState.Disabled:
                    if (_col.enabled)
                    {
                        // Lost track of collider state
                        ColliderOverrideState = OverrideState.Enabled;
                        break;
                    }
                    if (HeroController.instance.cState.superDashing)
                    {
                        // Knight is superdashing, we should override to make the collider active
                        _col.enabled = true;
                        ColliderOverrideState = OverrideState.Overridden;
                    }
                    break;
                case OverrideState.Enabled:
                    if (!_col.enabled)
                    {
                        // Lost track of collider state
                        ColliderOverrideState = OverrideState.Disabled;
                    }
                    // No need to toggle the collider - something else wants it active, and we don't *want* it inactive
                    break;
                case OverrideState.Overridden:
                    if (!_col.enabled)
                    {
                        // Lost track of collider state
                        if (HeroController.instance.cState.superDashing)
                        {
                            // Reactivate collider
                            _col.enabled = true;
                        }
                        else
                        {
                            // Resynchronize state
                            ColliderOverrideState = OverrideState.Disabled;
                        }
                        break;
                    }
                    if (!HeroController.instance.cState.superDashing)
                    {
                        // Knight is not superdashing; we should stop overriding
                        _col.enabled = false;
                        ColliderOverrideState = OverrideState.Disabled;
                    }
                    break;
            }
        }
    }
}
