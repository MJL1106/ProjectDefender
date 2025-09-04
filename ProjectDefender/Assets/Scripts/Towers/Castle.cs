using System;
using UnityEngine;

public class Castle : MonoBehaviour
{
   private GameManager gameManager;
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Enemy"))
      {
         other.GetComponent<Enemy>().DestroyEnemy();

         if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
         
         if (gameManager != null) gameManager.UpdateHp(-1);
      }
   }
}
