using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Manages all visual effects for the TowerCrossbow.
/// Handles emission glow, string tension, rotor animation, and attack visuals.
/// </summary>
public class CrossbowVisuals : MonoBehaviour
{
   private ObjectPoolManager objectPool;
   
   [Header("Attack Visuals")] 
   [SerializeField] private GameObject onHitVfx;
   [SerializeField] private LineRenderer attackVisuals;
   [SerializeField] private float attackVisualDuration = .1f;
   private Vector3 hitPoint;

   [Header("Glowing Visuals")] [SerializeField]
   private MeshRenderer meshRenderer;

   private Material material;

   [Space] 
   private float currentIntensity;
   [SerializeField] private float maxIntensity = 150;

   [Space] 
   [SerializeField] private Color startColor;
   [SerializeField] private Color endColor;

   [Header("Rotor Visuals")] 
   [SerializeField] private Transform rotor;
   [FormerlySerializedAs("unloaded")] [SerializeField] private Transform rotorUnloaded;
   [FormerlySerializedAs("loaded")] [SerializeField] private Transform rotorLoaded;

   [Header("Front Glow String")] 
   [SerializeField] private LineRenderer frontStringL;
   [SerializeField] private LineRenderer frontStringR;

   [Space] 
   [SerializeField] private Transform frontStartPointL;
   [SerializeField] private Transform frontStartPointR;
   [SerializeField] private Transform frontEndPointL;
   [SerializeField] private Transform frontEndPointR;

   [Header("Back Glow String")] 
   [SerializeField] private LineRenderer backStringL;
   [SerializeField] private LineRenderer backStringR;
   
   [Space] 
   [SerializeField] private Transform backStartPointL;
   [SerializeField] private Transform backStartPointR;
   [SerializeField] private Transform backEndPointL;
   [SerializeField] private Transform backEndPointR;

   [SerializeField] private LineRenderer[] lineRenderers;


   private void Awake()
   {
      material = new Material(meshRenderer.material);
      meshRenderer.material = material;

      UpdateMaterialsOnLineRenderers();
      
      StartCoroutine(ChangeEmission(1));
   }

   private void Start()
   {
      objectPool = ObjectPoolManager.instance;
   }

   private void UpdateMaterialsOnLineRenderers()
   {
      foreach (var lr in lineRenderers)
      {
         lr.material = material;
      }
   }

   private void Update()
   {
      UpdateEmissionColor();
      UpdateStrings();
      
      UpdateAttackVisualsIfNeeded();
   }

   /// <summary>
   /// Spawns the 'onHitVfx' prefab at the projectile's hit location.
   /// </summary>
   public void CreateOnHitVFX(Vector3 hitPoint) => objectPool.Get(onHitVfx, hitPoint, Random.rotation);

   private void UpdateAttackVisualsIfNeeded()
   {
      if (attackVisuals.enabled && hitPoint != Vector3.zero) attackVisuals.SetPosition(1, hitPoint);
   }

   private void UpdateStrings()
   {
      UpdateStringVisual(frontStringL, frontStartPointL, frontEndPointL);
      UpdateStringVisual(frontStringR, frontStartPointR, frontEndPointR);
      UpdateStringVisual(backStringL, backStartPointL, backEndPointL);
      UpdateStringVisual(backStringR, backStartPointR, backEndPointR);
   }

   private void UpdateEmissionColor()
   {
      Color emissionColor = Color.Lerp(startColor, endColor, currentIntensity / maxIntensity);
      emissionColor = emissionColor * Mathf.LinearToGammaSpace(currentIntensity);
      material.SetColor("_EmissionColor", emissionColor);
   }

   /// <summary>
   /// Triggers the reload animation (emission glow and rotor movement) over a duration.
   /// </summary>
   public void PlayerReloadVFX(float duration)
   {
      float newDuration = duration / 2;
      
      StartCoroutine(ChangeEmission(newDuration));
      StartCoroutine(UpdateRotorPosition(newDuration));
   }

   /// <summary>
   /// Triggers the attack 'laser' visual from the start to end point.
   /// </summary>
   public void PlayAttackVFX(Vector3 startPoint, Vector3 endPoint)
   {
      StartCoroutine(VFXCoroutine(startPoint, endPoint));
   }

   /// <summary>
   /// Coroutine to show the attack LineRenderer for a short duration.
   /// </summary>
   private IEnumerator VFXCoroutine(Vector3 startPoint, Vector3 endPoint)
   {
      hitPoint = endPoint;
      
      attackVisuals.enabled = true;
      attackVisuals.SetPosition(0, startPoint);
      attackVisuals.SetPosition(1, endPoint);
      
      yield return new WaitForSeconds(attackVisualDuration);

      attackVisuals.enabled = false;
   }

   /// <summary>
   /// Coroutine to lerp the material's emission intensity over time.
   /// </summary>
   private IEnumerator ChangeEmission(float duration)
   {
      float startTime = Time.time;
      float startIntensity = 0;

      while (Time.time - startTime < duration)
      {
         // Calculates the proportion of the duration that has elapsed since the start of the Coroutine.
         float tValue = (Time.time - startTime) / duration;
         currentIntensity = Mathf.Lerp(startIntensity, maxIntensity, tValue);
         yield return null;
      }

      currentIntensity = maxIntensity;
   }

   /// <summary>
   /// Coroutine to lerp the rotor's position from 'unloaded' to 'loaded'.
   /// </summary>
   private IEnumerator UpdateRotorPosition(float duration)
   {
      float startTime = Time.time;

      while (Time.time - startTime < duration)
      {
         float tValue = (Time.time - startTime) / duration;
         rotor.position = Vector3.Lerp(rotorUnloaded.position, rotorLoaded.position, tValue);
         yield return null;
      }

      rotor.position = rotorLoaded.position;
   }

   /// <summary>
   /// Updates a LineRenderer's start and end points to match two transforms.
   /// </summary>
   private void UpdateStringVisual(LineRenderer lineRenderer, Transform startPoint, Transform endPoint)
   {
      lineRenderer.SetPosition(0, startPoint.position);
      lineRenderer.SetPosition(1, endPoint.position);
   }
}