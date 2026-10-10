
using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

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
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }
}