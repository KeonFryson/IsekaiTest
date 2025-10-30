
using System.Collections.Generic;
using UnityEngine;

public class SpellContext
{
    public Transform caster;
    public Vector3 origin;
    public Vector3 direction;
    public float power;
    public float range;
    public SpellDefinition definition;
    public List<SpellAugment> augments;
}