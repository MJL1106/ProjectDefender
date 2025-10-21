using System;
using UnityEngine;

public class Castle : MonoBehaviour
{
   private GameManager gameManager;
   
   private void Start()
   {
      gameManager = GameManager.instance; 
       
      if (gameManager != null)
      {
         gameManager.RegisterCastle(transform);
      }
      else
      {
         Debug.LogError("Castle could not find GameManager instance!");
      }
   }
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Enemy"))
      {
         other.GetComponent<Enemy>().RemoveEnemy();

         if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
         
         if (gameManager != null) gameManager.UpdateHp(-1);
      }
   }
}
