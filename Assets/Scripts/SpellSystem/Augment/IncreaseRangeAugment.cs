
using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseRangeAugment", menuName = "Spell System/Augments/Increase Range")]
public class IncreaseRangeAugment : SpellAugment
{
    public float extraRange = 5f;

    public override void Modify(SpellContext ctx)
    {
        ctx.range += extraRange;
    }
}