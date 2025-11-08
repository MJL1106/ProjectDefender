using UnityEngine;

/// <summary>
/// Visual handler for swarm enemies with random variant selection and bouncing animation.
/// Creates variety within swarms by randomly enabling one of multiple visual prefabs.
/// </summary>
public class EnemySwarmVisuals : EnemyVisuals
{
    [Header("Visual variants")] 
    [SerializeField] private GameObject[] variants;

    [Header("Bounce Settings")] 
    [SerializeField] private AnimationCurve bounceCurve;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float minHeight = .1f;
    [SerializeField] private float maxHeight = .3f;
    private float bounceTimer;

    protected override void Awake()
    {
        ChooseVisualVariant();
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        BounceEffect();
    }

    /// <summary>
    /// Animates vertical bobbing using animation curve.
    /// </summary>
    private void BounceEffect()
    {
        bounceTimer += Time.deltaTime * bounceSpeed;
      
        float bounceValue = bounceCurve.Evaluate(bounceTimer % 1);
        float bounceHeight = Mathf.Lerp(minHeight, maxHeight, bounceValue);
      
        visuals.localPosition = new Vector3(visuals.localPosition.x, bounceHeight, visuals.localPosition.z);
    }

    /// <summary>
    /// Randomly selects and enables one visual variant for enemy diversity.
    /// Called on spawn to ensure swarms aren't visually identical.
    /// </summary>
    private void ChooseVisualVariant()
    {
        foreach (var option in variants)
        {
            option.SetActive(false);
        }

        int randomIndex = Random.Range(0, variants.Length);
        GameObject newVisuals = variants[randomIndex];

        newVisuals.SetActive(true);
        visuals = newVisuals.transform;
    }
}