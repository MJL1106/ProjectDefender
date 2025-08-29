using System;
using UnityEngine;

public class Castle : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Enemy"))
      {
         Destroy(other.gameObject);
      }
   }
}
