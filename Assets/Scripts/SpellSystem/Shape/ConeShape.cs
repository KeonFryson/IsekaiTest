 
using UnityEngine;

[CreateAssetMenu(fileName = "ConeShape", menuName = "Spell System/Shapes/Cone")]
public class ConeShape : SpellShape
{
    public float angle = 45f;
    public LayerMask mask = ~0;
    public float step = 0.5f; // sample spacing for overlap checks (optional)

    public override void Execute(SpellContext ctx, SpellEffect effect)
    {
        if (effect == null) return;

        Collider[] hits = Physics.OverlapSphere(ctx.origin, ctx.range, mask, QueryTriggerInteraction.Ignore);
        foreach (var c in hits)
        {
            if (c.transform == ctx.caster) continue;
            Vector3 toTarget = (c.transform.position - ctx.origin);
            float dist = toTarget.magnitude;
            if (dist <= 0.001f) continue;
            float a = Vector3.Angle(ctx.direction, toTarget);
            if (a <= angle)
            {
                // pass approximate hit info (no exact contact point)
                effect.Apply(ctx, c.gameObject, c.transform.position, -ctx.direction);
            }
        }
    }
}