using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class XRManualLoader : MonoBehaviour
{
    // manuelles Laden, damit es nicht zu race-conditions kommt und das Spiel korrekt startet


    void Start()
    {
        // Starte die Initialisierung mit einer kleinen Verzögerung
        StartCoroutine(StartXRCoroutine());
    }

    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initialisiere XR...");

        // Initialisiert den in den Settings gewählten Loader (z.B. Oculus/OpenXR)
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("XR Initialisierung fehlgeschlagen. Prüfe die Logs!");
        }
        else
        {
            Debug.Log("XR initialisiert. Starte Subsysteme...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    // Optional: XR beim Beenden sauber herunterfahren
    void OnDisable()
    {
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            Debug.Log("XR sauber gestoppt.");
        }
    }
}