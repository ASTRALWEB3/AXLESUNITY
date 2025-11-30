using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class SwingHitbox : MonoBehaviour
{
    // This script now only handles visual feedback
    // Targeting is handled by PlayerAttack script
    
    private void Awake()
    {
        // Ensure the collider is a trigger for visual purposes only
        GetComponent<Collider2D>().isTrigger = true;
    }

    // Note: OnTriggerEnter2D removed - targeting now handled by PlayerAttack
    // This hitbox is now purely visual feedback
}