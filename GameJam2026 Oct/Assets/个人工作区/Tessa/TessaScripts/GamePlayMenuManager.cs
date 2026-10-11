
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string homeSceneName =
        "TessaStartScene";

    [Header("UI Panels")]
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject volumePanel;

    private void Start()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);

        if (volumePanel != null)
            volumePanel.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void ReturnHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(homeSceneName);
    }

    public void ToggleHelp()
    {
        if (helpPanel == null) return;

        bool open = !helpPanel.activeSelf;

        if (volumePanel != null)
            volumePanel.SetActive(false);

        helpPanel.SetActive(open);
    }

    public void ToggleVolume()
    {
        if (volumePanel == null) return;

        bool open = !volumePanel.activeSelf;

        if (helpPanel != null)
            helpPanel.SetActive(false);

        volumePanel.SetActive(open);
    }

    public void ClosePanels()
    {
        if (helpPanel != null)
            helpPanel.SetActive(false);

        if (volumePanel != null)
            volumePanel.SetActive(false);
    }
}
