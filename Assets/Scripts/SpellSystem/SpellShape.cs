
using UnityEngine;

public abstract class SpellShape : ScriptableObject
{
    // The shape is responsible for selecting targets / spawning projectiles and then invoking effect.Apply(...)
    public abstract void Execute(SpellContext ctx, SpellEffect effect);
}