using UnityEngine;

public class RuneBehavior : MonoBehaviour
{
    public RuneData runeData;
    private float timer;

    void Start()
    {
        if (runeData == null) return;

        // Set color if prefab supports it
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = runeData.runeColor;
        }


        timer = runeData.lifetime;
    }

    void Update()
    {

        timer -= Time.deltaTime;
        if (!runeData.isInfinite && timer <= 0)
        {
            ActivateEffect();
            Destroy(gameObject);
        }
    }

    void ActivateEffect()
    {
       
        
        if (runeData.effectPrefab != null)
        {
            Instantiate(runeData.effectPrefab, transform.position, Quaternion.identity);
        }
    }
}
