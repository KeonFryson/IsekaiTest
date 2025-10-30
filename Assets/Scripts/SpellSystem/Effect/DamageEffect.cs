using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "Spell System/Effects/Damage")]
public class DamageEffect : SpellEffect
{
    [Tooltip("Base damage applied (multiplied by ctx.power)")]
    public float baseDamage = 10f;

    // Called when the shape resolved a target/object
    public override void Apply(SpellContext ctx, GameObject target, Vector3 point, Vector3 normal)
    {
        float damage = baseDamage * (ctx != null ? ctx.power : 1f);
        Debug.Log($"DamageEffect.Apply: target={(target ? target.name : "null")}, damage={damage}, point={point}, caster={(ctx?.caster ? ctx.caster.name : "null")}");

        if (target == null)
        {
            Debug.LogWarning("DamageEffect.Apply called with null target");
            return;
        }

        // Try to invoke common damage methods by reflection with support for different signatures:
        // - ApplyDamage(float)
        // - ApplyDamage(float, Vector3)
        // - TakeDamage(float)
        // - TakeDamage(float, Vector3)
        // - any similar method name with compatible parameter types
        bool invoked = false;

        var components = target.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null) continue;
            var type = comp.GetType();
            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            catch
            {
                continue;
            }

            foreach (var m in methods.Where(m => string.Equals(m.Name, "ApplyDamage", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(m.Name, "TakeDamage", StringComparison.OrdinalIgnoreCase)
                                             || string.Equals(m.Name, "ApplyHit", StringComparison.OrdinalIgnoreCase)))
            {
                var ps = m.GetParameters();
                object[] args = null;

                // single-parameter numeric (float/double/int)
                if (ps.Length == 1 && IsNumericType(ps[0].ParameterType))
                {
                    args = new object[] { Convert.ChangeType(damage, ps[0].ParameterType) };
                }
                // two parameters: numeric + Vector3 / Vector2 / GameObject / Transform
                else if (ps.Length == 2 && IsNumericType(ps[0].ParameterType))
                {
                    var second = ps[1].ParameterType;
                    if (second == typeof(Vector3))
                        args = new object[] { Convert.ChangeType(damage, ps[0].ParameterType), point };
                    else if (second == typeof(Vector2))
                        args = new object[] { Convert.ChangeType(damage, ps[0].ParameterType), new Vector2(point.x, point.y) };
                    else if (second == typeof(GameObject))
                        args = new object[] { Convert.ChangeType(damage, ps[0].ParameterType), target };
                    else if (second == typeof(Transform))
                        args = new object[] { Convert.ChangeType(damage, ps[0].ParameterType), target.transform };
                }

                if (args != null)
                {
                    try
                    {
                        m.Invoke(comp, args);
                        Debug.Log($"DamageEffect: invoked {m.Name} on {type.Name}");
                        invoked = true;
                        break;
                    }
                    catch (TargetInvocationException tie)
                    {
                        Debug.LogException(tie.InnerException ?? tie);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            if (invoked) break;
        }

        //if (!invoked)
        //{
        //    // Safe fallback: Try SendMessage for common single-arg "TakeDamage" (won't call ApplyDamage that needs extra args)
        //    target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        //    Debug.LogWarning("DamageEffect: no compatible damage method found via reflection; used SendMessage fallback for TakeDamage.");
        //}
    }

    static bool IsNumericType(Type t)
    {
        return t == typeof(float) || t == typeof(double) || t == typeof(int) || t == typeof(long) || t == typeof(short);
    }

    // Optional hook when shape misses
    public override void OnMiss(SpellContext ctx, Vector3 missPoint)
    {
        Debug.Log($"DamageEffect.OnMiss: caster={(ctx?.caster ? ctx.caster.name : "null")}, missPoint={missPoint}");
    }
}