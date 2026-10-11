
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Game Settings")]
    [SerializeField] private string gameSceneName;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    public void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);
    }

    public void PlayGame()
    {
        LoadLevel(gameSceneName);
    }

    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning(
                "Scene name is empty! Check StartMenuManager."
            );
            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}