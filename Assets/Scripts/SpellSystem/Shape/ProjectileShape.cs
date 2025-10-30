
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileShape", menuName = "Spell System/Shapes/Projectile (Ray)")]
public class ProjectileShape : SpellShape
{
    [Tooltip("Visualize the ray for this many seconds using Debug.DrawRay")]
    [SerializeField] private float debugDrawDuration = 2f;
    [Tooltip("Spawn a small debug sphere at hit point")]
    [SerializeField] private bool spawnHitMark = true;
    [Tooltip("Size of the debug sphere spawned at hit point")]
    [SerializeField] private float hitMarkSize = 0.15f;
    [SerializeField] private bool debugLog = false;
    public override void Execute(SpellContext ctx, SpellEffect effect)
    {
        if (ctx == null || effect == null)
        {
            Debug.LogWarning("ProjectileShape.Execute called with null ctx or effect");
            return;
        }

        Vector3 origin = ctx.origin;
        float range = ctx.range;

        // Determine where the player is looking:
        // Priority:
        // 1) If caster has a Camera in children, use that camera forward.
        // 2) If caster present, use caster.forward.
        // 3) Fallback to ctx.direction (normalized).
        Vector3 finalDirection = Vector3.forward;
        if (ctx.caster != null)
        {
            var cam = ctx.caster.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                finalDirection = cam.transform.forward;
            }
            else
            {
                finalDirection = ctx.caster.forward;
            }
        }
        else if (Camera.main != null)
        {
            finalDirection = Camera.main.transform.forward;
        }
        else
        {
            finalDirection = ctx.direction.normalized;
        }

        finalDirection = finalDirection.normalized;

        if (debugLog)
        {
            Debug.Log($"ProjectileShape.Execute: caster={(ctx.caster ? ctx.caster.name : "null")}, origin={origin}, direction={finalDirection}, range={range}");
            Debug.DrawRay(origin, finalDirection * range, Color.red, debugDrawDuration);
        }
           
       

        if (Physics.Raycast(origin, finalDirection, out RaycastHit hit, range))
        {
            GameObject hitObj = hit.collider ? hit.collider.gameObject : null;

            if (debugLog)
            {
                Debug.Log($"ProjectileShape hit: {(hitObj ? hitObj.name : "unknown")} at {hit.point}, normal={hit.normal}");

                // Replace the debug ray with a green ray to indicate a hit
                Debug.DrawRay(origin, finalDirection * hit.distance, Color.green, debugDrawDuration);
            }
            


            // Show a small '+' marker at the hit point using Debug.DrawLine instead of spawning a sphere
            if (spawnHitMark)
            {
                // Determine camera-forward to orient the marker facing the camera when possible
                Vector3 camForward = finalDirection;
                var cam = ctx.caster != null ? ctx.caster.GetComponentInChildren<Camera>() : Camera.main;
                if (cam != null) camForward = cam.transform.forward;

                // Build two perpendicular directions on the plane tangent to the hit normal
                Vector3 right = Vector3.Cross(camForward, hit.normal).normalized;
                if (right.sqrMagnitude < 1e-6f) right = Vector3.right;
                Vector3 up = Vector3.Cross(hit.normal, right).normalized;

                float s = Mathf.Max(0.001f, hitMarkSize);

                if (debugLog)
                {
                    Debug.DrawLine(hit.point - right * s, hit.point + right * s, Color.green, debugDrawDuration);
                    Debug.DrawLine(hit.point - up * s, hit.point + up * s, Color.green, debugDrawDuration);
                }
            }

            effect.Apply(ctx, hitObj, hit.point, hit.normal);
        }
        else
        {
            Vector3 missPoint = origin + finalDirection * range;
            Debug.Log($"ProjectileShape missed. Miss point: {missPoint}");
            effect.OnMiss(ctx, missPoint);
        }
    }
}