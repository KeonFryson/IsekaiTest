 
using UnityEngine;

public abstract class SpellAugment : ScriptableObject
{
    // Modify the runtime context (power/range/flags/etc).
    public virtual void Modify(SpellContext ctx) { }
}