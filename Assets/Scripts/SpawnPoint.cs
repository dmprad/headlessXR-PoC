using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform cameraRig; // OVRCameraRig

    void Start()
    {
        if (spawnPoint == null || cameraRig == null)
            return;

        Vector3 offset = cameraRig.position - cameraRig.GetComponentInChildren<Camera>().transform.position;
        offset.y = 0f;

        cameraRig.position = spawnPoint.position + offset;
        cameraRig.rotation = spawnPoint.rotation;

    }
}