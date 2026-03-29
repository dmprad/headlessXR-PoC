using UnityEngine;
using UnityEngine.Rendering;

public class HmdFreeze : MonoBehaviour
{
    public OVRCameraRig cameraRig;
    public bool headless;
    public EnemyChase1 enemyAI;

    public Vector3 frozenHeadWorldPos;
    public Quaternion frozenHeadWorldRot;

    public bool headTracked;
    public bool leftControllerTracked;
    public bool rightControllerTracked;


    void Update()
    {
        // Toggle headless on A
        if (OVRInput.GetDown(OVRInput.Button.One) /*&& OVRInput.Get(OVRInput.Button.Two)*/)
        {
            headless = !headless;
            //enemyAI.SetHeadless(headless);

            if (headless)
            {
                CaptureFreezePose();

            }
                
            Debug.Log("Headless: " + headless);
        }

        if (!OVRManager.isHmdPresent)
        {
            Debug.Log("HMD NOT PRESENT!");
        }
        else
        {
            HMDStatus();
        }

    }

    void LateUpdate()
    {
        if (!headless || cameraRig == null) return;

        Transform rig = cameraRig.transform;
        Transform head = cameraRig.centerEyeAnchor;

        // Head pose in rig-local tracking space (set by tracking each frame)
        Vector3 headLocalPos = head.localPosition;
        Quaternion headLocalRot = head.localRotation;

       
        /*
        // Force head world pose to stay exactly at the frozen pose:
        // rigRot * headLocalRot == frozenHeadWorldRot
        rig.rotation = frozenHeadWorldRot * Quaternion.Inverse(headLocalRot);

        // rigPos + rigRot * headLocalPos == frozenHeadWorldPos
        rig.position = frozenHeadWorldPos - (rig.rotation * headLocalPos);    
        */
    }

    void CaptureFreezePose()
    {
        if (cameraRig == null) return;

        Transform head = cameraRig.centerEyeAnchor;
        frozenHeadWorldPos = head.position;
        frozenHeadWorldRot = head.rotation;

        Debug.Log("Frozen head at: " + frozenHeadWorldPos);
    }

    void HMDStatus()
    {
        headTracked = OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.Head)
                && OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.Head);

        leftControllerTracked = OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandLeft);
        rightControllerTracked = OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandRight);

        Debug.Log(
            "Head: " + (headTracked ? "Yes" : "No") +
            " | Left: " + (leftControllerTracked ? "Yes" : "No") +
            " | Right: " + (rightControllerTracked ? "Yes" : "No")
        );
    }
}
