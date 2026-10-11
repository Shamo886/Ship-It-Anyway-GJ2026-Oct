using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndSceneCreditsController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject endingContent;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private ScrollRect creditsScrollRect;

    [Header("Timing")]
    [SerializeField] private float delayBeforeCredits = 20f;

    [Header("Credits Scroll")]
    [SerializeField] private float scrollSpeed = 0.05f;

    [Header("Scene")]
    [SerializeField] private string startSceneName = "StartScene";

    private void Start()
    {
        creditsPanel.SetActive(false);

        if (endingContent != null)
            endingContent.SetActive(true);

        StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        // 等待结尾内容播放
        yield return new WaitForSecondsRealtime(delayBeforeCredits);

        if (endingContent != null)
            endingContent.SetActive(false);

        creditsPanel.SetActive(true);

        // 等一帧，让 UI 完成布局
        yield return null;

        // 从最顶部开始
        creditsScrollRect.verticalNormalizedPosition = 1f;

        // 自动滚动
        while (creditsScrollRect.verticalNormalizedPosition > 0f)
        {
            creditsScrollRect.verticalNormalizedPosition -=
                scrollSpeed * Time.unscaledDeltaTime;

            yield return null;
        }

        // 确保停在最底部
        creditsScrollRect.verticalNormalizedPosition = 0f;

        // Credits 一结束，立刻回开始场景
        SceneManager.LoadScene(startSceneName);
    }
}