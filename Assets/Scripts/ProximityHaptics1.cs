using UnityEngine;

public class ProximityHaptics1 : MonoBehaviour
{
    [Header("Refs")]
    public OVRCameraRig cameraRig;
    public Transform enemy;
    public HmdFreeze hmdFreeze;

    [Header("Distance Tuning")]
    public float maxDistance = 1.5f;   // farther than this = almost no haptics
    public float minDistance = 0.2f;   // at or below this = max haptics
    public float distanceExponent = 2.0f; // >1 = stronger emphasis on very small distances

    [Header("Haptics Amplitude")]
    [Range(0f, 1f)] public float minAmplitude = 0.02f;
    [Range(0f, 1f)] public float maxAmplitude = 0.5f;

    [Header("Haptics Pulse Speed")]
    public float minPulseSpeed = 1.0f;
    public float maxPulseSpeed = 6.0f;

    [Header("Player Position Smoothing")]
    public float smoothingSpeed = 8f;

    [Header("Estimated Player Position (Debug)")]
    public Vector3 estimatedPlayerWorldPos;
    public Vector3 rawEstimatedPlayerWorldPos;
    public bool leftTracked;
    public bool rightTracked;
    public bool hasValidPlayerPosition;
    public float currentDistance;
    public float currentProximity01;
    public float currentAmplitude;
    public float currentPulseSpeed;
    public float currentFinalAmplitude;

    Vector3 lastValidPlayerWorldPos;
    bool hasInitializedSmoothedPosition = false;

    void Update()
    {
        if (hmdFreeze == null || !hmdFreeze.headless || enemy == null || cameraRig == null)
        {
            StopHaptics();
            return;
        }

        UpdateEstimatedPlayerPosition();

        if (!hasValidPlayerPosition)
        {
            StopHaptics();
            return;
        }

        Vector3 playerFlat = estimatedPlayerWorldPos;
        Vector3 enemyFlat = enemy.position;

        // Only horizontal distance matters
        playerFlat.y = 0f;
        enemyFlat.y = 0f;

        float distance = Vector3.Distance(playerFlat, enemyFlat);
        currentDistance = distance;

        // 0 = far away, 1 = very close
        float t = Mathf.InverseLerp(maxDistance, minDistance, distance);
        t = Mathf.Clamp01(t);

        // Stronger emphasis on close range
        t = Mathf.Pow(t, distanceExponent);
        currentProximity01 = t;

        float amplitude = Mathf.Lerp(minAmplitude, maxAmplitude, t);
        float pulseSpeed = Mathf.Lerp(minPulseSpeed, maxPulseSpeed, t);

        currentAmplitude = amplitude;
        currentPulseSpeed = pulseSpeed;

        // Smooth pulse between 0 and 1
        float pulse = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float finalAmplitude = amplitude * pulse;
        currentFinalAmplitude = finalAmplitude;

        // Keep frequency fixed, pulse feeling comes from amplitude modulation
        OVRInput.SetControllerVibration(1f, finalAmplitude, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(1f, finalAmplitude, OVRInput.Controller.RTouch);
    }

    void UpdateEstimatedPlayerPosition()
    {
        leftTracked = OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandLeft);
        rightTracked = OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandRight);

        Vector3 leftWorld = Vector3.zero;
        Vector3 rightWorld = Vector3.zero;

        if (leftTracked)
        {
            Vector3 leftLocal = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
            leftWorld = cameraRig.trackingSpace.TransformPoint(leftLocal);
        }

        if (rightTracked)
        {
            Vector3 rightLocal = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            rightWorld = cameraRig.trackingSpace.TransformPoint(rightLocal);
        }

        if (leftTracked && rightTracked)
        {
            // Best case: midpoint between both controllers
            rawEstimatedPlayerWorldPos = (leftWorld + rightWorld) * 0.5f;
            lastValidPlayerWorldPos = rawEstimatedPlayerWorldPos;
            hasValidPlayerPosition = true;
        }
        else if (rightTracked)
        {
            // Fallback: only right controller tracked
            rawEstimatedPlayerWorldPos = rightWorld;
            lastValidPlayerWorldPos = rawEstimatedPlayerWorldPos;
            hasValidPlayerPosition = true;
        }
        else if (leftTracked)
        {
            // Fallback: only left controller tracked
            rawEstimatedPlayerWorldPos = leftWorld;
            lastValidPlayerWorldPos = rawEstimatedPlayerWorldPos;
            hasValidPlayerPosition = true;
        }
        else
        {
            // Last fallback: keep last known valid position
            rawEstimatedPlayerWorldPos = lastValidPlayerWorldPos;
            hasValidPlayerPosition = lastValidPlayerWorldPos != Vector3.zero;
        }

        if (!hasValidPlayerPosition)
            return;

        if (!hasInitializedSmoothedPosition)
        {
            estimatedPlayerWorldPos = rawEstimatedPlayerWorldPos;
            hasInitializedSmoothedPosition = true;
        }
        else
        {
            estimatedPlayerWorldPos = Vector3.Lerp(
                estimatedPlayerWorldPos,
                rawEstimatedPlayerWorldPos,
                Time.deltaTime * smoothingSpeed
            );
        }
    }

    void OnDisable()
    {
        StopHaptics();
    }

    void OnDestroy()
    {
        StopHaptics();
    }

    void StopHaptics()
    {
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
        currentFinalAmplitude = 0f;
    }
}