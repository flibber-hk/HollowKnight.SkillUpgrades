using System;
using System.Linq;
using UnityEngine;

namespace SkillUpgrades.Components
{
    /// <summary>
    /// Class that can rotate an object without moving its collider
    /// Supports objects with a BoxCollider or a PolygonCollider
    /// </summary>
    public class StableRotator : MonoBehaviour
    {
        protected PolygonCollider2D _collider;
        protected Vector2[] _originalPoints;
        protected Vector2 _originalOffset;

        public virtual void Awake()
        {
            // Get Data
            Collider2D col2d = GetComponent<Collider2D>();

            if (col2d is PolygonCollider2D)
            {
                _collider = col2d as PolygonCollider2D;
                return;
            }
            else if (col2d is not BoxCollider2D)
            {
                SkillUpgrades.instance.LogWarn("Unable to replace collider with Polygon collider");
                throw new InvalidOperationException();
            }

            Vector2 heropos = transform.position;
            Vector2 center = col2d.bounds.center;
            Vector2 extents = col2d.bounds.extents;
            _originalOffset = col2d.offset;

            // Set Data
            PolygonCollider2D newCol2d = gameObject.AddComponent<PolygonCollider2D>();
            _originalPoints = new Vector2[]
            {
                center + new Vector2(extents.x, extents.y) - heropos - _originalOffset,
                center + new Vector2(extents.x, -extents.y) - heropos - _originalOffset,
                center + new Vector2(-extents.x, -extents.y) - heropos - _originalOffset,
                center + new Vector2(-extents.x, extents.y) - heropos - _originalOffset
            };
            newCol2d.SetPath(0, _originalPoints);
            newCol2d.offset = _originalOffset;

            _collider = newCol2d;

            Destroy(col2d);
        }

        /// <summary>
        /// Rotate the object counterclockwise by angle degrees
        /// </summary>
        /// <param name="angle">Angle to rotate</param>
        /// <param name="respectFacingDirection">If true, instead rotate clockwise when the object's local x scale is negative</param>
        public virtual void Rotate(float angle, bool respectFacingDirection = true)
        {
            float scale = transform.localScale.x < 0 ? -1 : 1;

            Vector2[] colliderBounds = _collider.GetPath(0);
            float rotation = angle * (respectFacingDirection ? scale : 1);
            transform.Rotate(0, 0, rotation);
            _collider.SetPath(0, ApplyRotationToPoints(colliderBounds, -rotation * scale));
        }

        /// <summary>
        /// Set the rotation to be angle degrees counterclockwise. Equivalent to ResetRotation() followed by Rotate()
        /// </summary>
        /// <param name="angle">Angle to rotate</param>
        /// <param name="respectFacingDirection">If this is true, instead set it to be a clockwise rotation when the hero is facing right</param>
        public void SetRotation(float angle, bool respectFacingDirection = true)
        {
            ResetRotation();
            Rotate(angle, respectFacingDirection);
        }

        /// <summary>
        /// Reset the object's rotation
        /// </summary>
        public virtual void ResetRotation()
        {
            transform.rotation = Quaternion.identity;
            _collider.SetPath(0, _originalPoints);
            _collider.offset = _originalOffset;
        }

        protected Vector2[] ApplyRotationToPoints(Vector2[] points, float rotation)
        {
            float radians = rotation * Mathf.PI / 180f;

            return points
                .Select(v => v + _originalOffset)
                .Select(v => new Vector2(Mathf.Cos(radians) * v.x - Mathf.Sin(radians) * v.y, Mathf.Sin(radians) * v.x + Mathf.Cos(radians) * v.y))
                .Select(v => v - _originalOffset)
                .ToArray();
        }
    }
}
