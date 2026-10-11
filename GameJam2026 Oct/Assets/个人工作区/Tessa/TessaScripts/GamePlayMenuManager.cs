
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField]
    private string homeSceneName =
        "TessaStartScene";

    [Header("UI Panels")]
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject volumePanel;

    [Header("Programming")]
    [SerializeField] private ProgrammingManager programmingManager;

    private void Start()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);

        if (volumePanel != null)
            volumePanel.SetActive(false);
    }

    // Restart the CURRENT level
    public void RestartLevel()
    {
        // Clear Big Package and Small Package commands
        if (programmingManager != null)
        {
            programmingManager.ResetProgramming();
        }
        else
        {
            Debug.LogWarning(
                "GameplayMenuManager: ProgrammingManager is not assigned."
            );
        }

        // Reset the current level instead of loading Level 0
        WorldBus.SendReset();
    }

    // Return to the home scene
    public void ReturnHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    // Open or close Help
    public void ToggleHelp()
    {
        if (helpPanel == null)
            return;

        bool open = !helpPanel.activeSelf;

        if (volumePanel != null)
            volumePanel.SetActive(false);

        helpPanel.SetActive(open);
    }

    // Open or close Volume
    public void ToggleVolume()
    {
        if (volumePanel == null)
            return;

        bool open = !volumePanel.activeSelf;

        if (helpPanel != null)
            helpPanel.SetActive(false);

        volumePanel.SetActive(open);
    }

    // Close all panels
    public void ClosePanels()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);

        if (volumePanel != null)
            volumePanel.SetActive(false);
    }
}
