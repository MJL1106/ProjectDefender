using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CrossbowVisuals : MonoBehaviour
{
   private TowerCrossbow myTower;

   [SerializeField] private LineRenderer attackVisuals;
   [SerializeField] private float attackVisualDuration = .1f;

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
      myTower = GetComponent<TowerCrossbow>();
      material = new Material(meshRenderer.material);
      meshRenderer.material = material;

      UpdateMaterialsOnLineRenderers();
      
      StartCoroutine(ChangeEmission(1));
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

   public void PlayerReloadFX(float duration)
   {
      float newDuration = duration / 2;
      
      StartCoroutine(ChangeEmission(newDuration));
      StartCoroutine(UpdateRotorPosition(newDuration));
   }

   public void PlayAttackVFX(Vector3 startPoint, Vector3 endPoint)
   {
      StartCoroutine(VFXCoroutine(startPoint, endPoint));
   }

   private IEnumerator VFXCoroutine(Vector3 startPoint, Vector3 endPoint)
   {
      myTower.EnableRotation(false);
      
      attackVisuals.enabled = true;
      attackVisuals.SetPosition(0, startPoint);
      attackVisuals.SetPosition(1, endPoint);
      
      yield return new WaitForSeconds(attackVisualDuration);

      attackVisuals.enabled = false;

      myTower.EnableRotation(true);
   }

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

   private void UpdateStringVisual(LineRenderer lineRenderer, Transform startPoint, Transform endPoint)
   {
      lineRenderer.SetPosition(0, startPoint.position);
      lineRenderer.SetPosition(1, endPoint.position);
   }
}
