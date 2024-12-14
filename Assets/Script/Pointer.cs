using System.Collections;
using UnityEngine;

public class PointerBounce : MonoBehaviour
{
    public float bounceHeight = 0.1f;  
    public float bounceSpeed = 5f;      
    private Vector3 initialPosition;

    public float heightOffset = 0.2f;

    private void Start()
    {
        initialPosition = transform.position;
        initialPosition.y += heightOffset;  
        transform.position = initialPosition; 

        StartCoroutine(Bounce());
    }

    public void StartBouncing()
    {
        initialPosition = transform.position;
        initialPosition.y += heightOffset; 
        transform.position = initialPosition; 
        StartCoroutine(Bounce());
    }


    private IEnumerator Bounce()
    {
        while (true)
        {
            float newY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            transform.position = new Vector3(initialPosition.x, initialPosition.y + newY, initialPosition.z);
            yield return null; 
        }
    }
}
