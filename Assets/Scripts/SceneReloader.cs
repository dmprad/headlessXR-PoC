using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public string scene1Name;
    public string scene2Name;
    public float holdDuration = 3f;
    public GameObject menu;
    float holdTimer = 0f;
    private bool isPressed = false;
    public Transform centerEye;
    public float distanceUI = 1.5f;

    void Update()
    {
        bool a = OVRInput.Get(OVRInput.Button.One);   // A
        bool b = OVRInput.Get(OVRInput.Button.Two);   // B
        bool x = OVRInput.Get(OVRInput.Button.Three); // X
        bool y = OVRInput.Get(OVRInput.Button.Four);  // Y
        bool z = OVRInput.Get(OVRInput.Button.Start);  // Menu 


        if (a && b && x && y)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdDuration)
            {
                ReloadScene();
            }
        }
        else
        {
            holdTimer = 0f;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Start))
        {
            if (!isPressed)
            {
                bool newState = !menu.activeSelf;
                menu.SetActive(newState);

                if (newState)
                {
                    Vector3 forward = centerEye.forward;
                    forward.y = 0f;
                    forward.Normalize();

                    Vector3 uiPos =
                        centerEye.position +
                        forward * distanceUI +
                        Vector3.up * 0.1f;

                    uiPos.y = Mathf.Max(uiPos.y, 0.4f);

                    menu.transform.position = uiPos;

                    menu.transform.rotation =
                        Quaternion.LookRotation(forward, Vector3.up);
                }

                isPressed = true;
            }
        }

        if (OVRInput.GetUp(OVRInput.RawButton.Start))
        {
            isPressed = false;
        }

    }

    public void ReloadScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void LoadScene1()
    {
        SceneManager.LoadScene(scene1Name);
    }

    public void LoadScene2()
    {
        SceneManager.LoadScene(scene2Name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
