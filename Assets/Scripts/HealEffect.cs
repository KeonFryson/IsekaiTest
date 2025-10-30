 
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "Spell System/Effects/Heal")]
public class HealEffect : SpellEffect
{
    public float healMultiplier = 1f;

    public override void Apply(SpellContext ctx, GameObject target, Vector3 point, Vector3 normal)
    {
        float amount = ctx.power * healMultiplier;
        var health = target.GetComponent<Health>();
        if (health != null)
        {
            health.ApplyHeal(amount);
            return;
        }

        target.SendMessage("ApplyHeal", amount, SendMessageOptions.DontRequireReceiver);
        Debug.Log($"HealEffect healed {target.name} for {amount}");
    }
}