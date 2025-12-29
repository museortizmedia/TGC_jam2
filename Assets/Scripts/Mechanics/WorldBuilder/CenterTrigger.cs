using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CenterTrigger : MonoBehaviour
{
    [SerializeField] WorldBuilder worldBuilder;

    void OnTriggerEnter(Collider other)
    {
        worldBuilder.ReportPlayerInCenter(other.gameObject);
        Debug.Log("CenterTrigger: OnTriggerEnter with " + other.name);
    }

    void OnTriggerStay(Collider other) {
        //Debug.Log("CenterTrigger: OnTriggerEnter with " + other.name);
    }
    
    void OnTriggerExit(Collider other)
    {
        //Debug.Log("CenterTrigger: OnTriggerExit with " + other.name);
    }
}
