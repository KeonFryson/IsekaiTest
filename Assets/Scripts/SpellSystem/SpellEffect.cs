 
using UnityEngine;

public abstract class SpellEffect : ScriptableObject
{
    // Called when the shape resolved a target/object
    public abstract void Apply(SpellContext ctx, GameObject target, Vector3 point, Vector3 normal);

    // Optional hook when shape misses
    public virtual void OnMiss(SpellContext ctx, Vector3 missPoint) { }
}