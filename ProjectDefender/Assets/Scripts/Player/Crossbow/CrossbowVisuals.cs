using System;
using System.Collections;
using UnityEngine;

public class CrossbowVisuals : MonoBehaviour
{
   private TowerCrossbow myTower;
   
   [SerializeField] private LineRenderer attackVisuals;
   [SerializeField] private float attackVisualDuration = .1f;

   [Header("Glowing Visuals")]
   [SerializeField] private MeshRenderer meshRenderer;
   private Material material;
   
   [Space]
   private float currentIntensity;
   [SerializeField] private float maxIntensity = 150;

   [Space] 
   [SerializeField] private Color startColor;
   [SerializeField] private Color endColor;


   private void Awake()
   {
      myTower = GetComponent<TowerCrossbow>();

      material = new Material(meshRenderer.material);

      meshRenderer.material = material;

      StartCoroutine(ChangeEmission(1));
   }

   private void Update()
   {
      UpdateEmissionColor();
   }

   private void UpdateEmissionColor()
   {
      Color emissionColor = Color.Lerp(startColor, endColor, currentIntensity / maxIntensity);
      emissionColor = emissionColor * Mathf.LinearToGammaSpace(currentIntensity);
      material.SetColor("_EmissionColor", emissionColor);
   }

   public void PlayerReloadFX(float duration)
   {
      StartCoroutine(ChangeEmission(duration / 2));
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
}
