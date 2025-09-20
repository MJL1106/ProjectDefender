using UnityEngine;

public class EnemySpiderVisuals : EnemyVisuals
{
    [Header("Leg Details")] public float legSpeed = 3;

    public float increasedLegSpeed = 10;

    private SpiderLeg[] legs;

    [Header("Body animation")] [SerializeField]
    private Transform bodyTransform;

    [SerializeField] private float bodyAnimSpeed = 1;
    [SerializeField] private float maxHeight = .1f;

    private Vector3 startPosition;
    private float elapsedTime;

    [Header("Smoke animation")] [SerializeField]
    private ParticleSystem[] smokeVfx;

    [SerializeField] private float smokeCooldown;
    private float smokeTimer;


    protected override void Awake()
    {
        base.Awake();
        legs = GetComponentsInChildren<SpiderLeg>();
    }

    protected override void Start()
    {
        base.Start();
        startPosition = bodyTransform.localPosition;
    }

    protected override void Update()
    {
        base.Update();

        AnimateBody();
        ActivateSmokeVfxIfCan();
        UpdateSpiderLegs();
    }

    private void AnimateBody()
    {
        elapsedTime += Time.deltaTime * bodyAnimSpeed;

        float sinValue = (Mathf.Sin(elapsedTime) + 1) / 2;
        float newY = Mathf.Lerp(0, maxHeight, sinValue);

        bodyTransform.localPosition = startPosition + new Vector3(0, newY, 0);
    }

    private void ActivateSmokeVfxIfCan()
    {
        smokeTimer -= Time.deltaTime;

        if (smokeTimer < 0)
        {
            smokeTimer = smokeCooldown;

            foreach (var smoke in smokeVfx)
            {
                smoke.Play();
            }
        }
    }

    private void UpdateSpiderLegs()
    {
        foreach (var leg in legs)
        {
            leg.UpdateLeg();
        }
    }

    public void BrieflySpeedUpLegs()
    {
        foreach (var leg in legs)
        {
            leg.SpeedUpLeg();
        }
    }
}
