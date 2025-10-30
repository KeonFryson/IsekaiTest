
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellDefinition", menuName = "Spell System/Spell Definition")]
public class SpellDefinition : ScriptableObject
{
    public SpellShape shape;
    public SpellEffect effect;
    public List<SpellAugment> augments = new List<SpellAugment>();

    [Header("Base parameters")]
    public float basePower = 10f;
    public float baseRange = 20f;

    // Public API to cast the spell
    public void Cast(Transform caster, Vector3 origin, Vector3 direction)
    {
        if (shape == null || effect == null)
        {
            Debug.LogWarning("SpellDefinition missing shape or effect.");
            return;
        }

        var ctx = new SpellContext
        {
            caster = caster,
            origin = origin,
            direction = direction.normalized,
            power = basePower,
            range = baseRange,
            definition = this,
            augments = new List<SpellAugment>(augments)
        };

        // Apply augments to modify the context
        if (ctx.augments != null)
        {
            foreach (var a in ctx.augments)
            {
                if (a != null)
                    a.Modify(ctx);
            }
        }

        // Execute shape which will use the effect to apply to targets
        shape.Execute(ctx, effect);
    }
}