 
using UnityEngine;

[CreateAssetMenu(fileName = "IncreasePowerAugment", menuName = "Spell System/Augments/Increase Power")]
public class IncreasePowerAugment : SpellAugment
{
    public float multiplier = 1.5f;

    public override void Modify(SpellContext ctx)
    {
        ctx.power *= multiplier;
    }
}