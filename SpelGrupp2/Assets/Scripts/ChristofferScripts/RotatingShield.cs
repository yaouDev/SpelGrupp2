using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingShield : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationSpeedMultiplier = 2.0f;

    private EnemyHealth enemyHealth;
    private float healthPercentage;

    private void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    void Update()
    {
        healthPercentage = enemyHealth.CurrentHealthPercentage;
        //Spin the shield
        transform.Rotate(rotation * rotationSpeed * Time.deltaTime);
        //If healthPercentage is under 700, spin faster
        if (healthPercentage < 70)
        {
            transform.Rotate(rotation * (rotationSpeed * rotationSpeedMultiplier) * Time.deltaTime);
            
        }
        else
        {
            if (healthPercentage < 30)
            {
                gameObject.layer = LayerMask.NameToLayer("Bouncing");
                transform.Rotate(rotation * (rotationSpeed * rotationSpeedMultiplier) * Time.deltaTime);
                Debug.Log("Hej och h�" + rotation);
            }

        }
    }
}
