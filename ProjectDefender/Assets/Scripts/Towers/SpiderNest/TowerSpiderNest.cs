using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TowerSpiderNest : Tower
{
    [Header("Spider nest details")] 
    [SerializeField] private GameObject spiderPrefab;
    [SerializeField] private float damage;
    
    [Range(0,1)]
    [SerializeField] private float attackTimeMultiplier = .4f;
    [SerializeField] private float reloadTimeMultiplier = .6f;
    
    [Space]
    [SerializeField] private Transform[] attachPoint;
    [SerializeField] private Transform[] webSet;
    [SerializeField] private Transform[] attachPointRef;
    
    private GameObject[] activeSpider;
    private int spiderIndex;
    private Vector3 spiderPointOffset = new Vector3(0, -.17f, 0);

    protected override void Start()
    {
        base.Start();
        InitializeSpiders();
        reloadTimeMultiplier = 1 - attackTimeMultiplier;
    }

    protected override void Update()
    {
        base.Update();
        UpdateAttachPointsPosition();
    }

    protected override bool CanAttack()
    {
        return Time.time > lastTimeAttacked + attackCooldown; //&& AtLeastOneEnemyAround();
    }

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(AttackCo());
    }

    private IEnumerator AttackCo()
    {
        Transform currentWeb = webSet[spiderIndex];
        Transform currentAttachPoint = attachPoint[spiderIndex];
        float attackTime = (attackCooldown / 4) * attackTimeMultiplier;
        float reloadTime = (attackCooldown / 4) * reloadTimeMultiplier;
        
        yield return ChangeScaleCo(currentWeb, 1, attackTime);
        activeSpider[spiderIndex].GetComponent<ProjectileSpiderNest>().SetupSpider(damage);

        yield return ChangeScaleCo(currentWeb, .1f, reloadTime);
        activeSpider[spiderIndex] = objectPool.Get(spiderPrefab, currentAttachPoint.position + spiderPointOffset, Quaternion.identity, currentAttachPoint);

        spiderIndex = (spiderIndex + 1) % attachPoint.Length;
    }

    private void UpdateAttachPointsPosition()
    {
        for (int i = 0; i < attachPoint.Length; i++)
        {
            attachPoint[i].position = attachPointRef[i].position;
        }
    }

    private void InitializeSpiders()
    {
        activeSpider = new GameObject[attachPoint.Length];

        for (int i = 0; i < activeSpider.Length; i++)
        {
            GameObject newSpider =
                objectPool.Get(spiderPrefab, attachPoint[i].position + spiderPointOffset, Quaternion.identity, attachPoint[i]);
            activeSpider[i] = newSpider;
        }
    }
    
    private IEnumerator ChangeScaleCo(Transform transform, float newScale, float duration = .25f)
    {
        float time = 0;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(1,newScale,1);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
