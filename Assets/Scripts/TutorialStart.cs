using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static UnityEditor.PlayerSettings;

public class TutorialStart : MonoBehaviour
{
    [SerializeField] private GameObject tutorialUI;

    private bool hasPaused = false;

    public Transform centerEye;
    public float distanceUI = 1.5f;

    private IEnumerator Start()
    {
        // Sicherheit: Spiel erstmal auf normal
        Time.timeScale = 1f;

        if (tutorialUI == null)
        {
            Debug.LogWarning("TutorialStart: tutorialUI ist nicht gesetzt.");
            yield break;
        }

        // UI zuerst sichtbar machen
        tutorialUI.SetActive(true);

        // Warten, bis Canvas / Layout / XR UI komplett aufgebaut sind
        yield return new WaitForEndOfFrame();

        // Optional noch ein weiterer Frame für mehr Stabilität
        yield return null;

        Vector3 forward = centerEye.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 uiPos =
            centerEye.position +
            forward * distanceUI +
            Vector3.up * 0.1f;

        uiPos.y = Mathf.Max(uiPos.y, 0.5f);

        tutorialUI.transform.position = uiPos;

        tutorialUI.transform.rotation =
            Quaternion.LookRotation(forward, Vector3.up);

        // Erst jetzt pausieren
        Time.timeScale = 0f;
        hasPaused = true;
    }

    public void OnPressOK()
    {
        if (tutorialUI != null)
            tutorialUI.SetActive(false);

        if (hasPaused)
        {
            Time.timeScale = 1f;
            hasPaused = false;
        }
    }

    private void OnDisable()
    {
        // Falls das Objekt deaktiviert / Szene gewechselt wird:
        // Spiel sicher wieder entpausen
        Time.timeScale = 1f;
        hasPaused = false;
    }

    private void OnDestroy()
    {
        // Zusätzliche Absicherung beim Szenenwechsel
        Time.timeScale = 1f;
        hasPaused = false;
    }
}