using UnityEngine;

public class ArrowDebug : MonoBehaviour
{
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 0.5f, Color.red); // Z (нос)
        Debug.DrawRay(transform.position, transform.up * 0.5f, Color.green);   // Y
        Debug.DrawRay(transform.position, transform.right * 0.5f, Color.blue); // X
    }
}