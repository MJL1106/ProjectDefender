using UnityEngine;

public class EnemySpiderVisuals : EnemyVisuals
{
    [Header("Leg Details")] public float legSpeed = 3;

    private SpiderLeg[] legs;

    protected override void Start()
    {
        base.Start();

        legs = GetComponentsInChildren<SpiderLeg>();
    }

    protected override void Update()
    {
        base.Update();

        UpdateSpiderLegs();
    }

    private void UpdateSpiderLegs()
    {
        foreach (var leg in legs)
        {
            leg.UpdateLeg();
        }
    }
}
