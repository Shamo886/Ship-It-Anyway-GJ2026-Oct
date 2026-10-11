
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameSFX : MonoBehaviour
{
    public static GameSFX Instance { get; private set; }

    [Header("Game Events")]
    [SerializeField] private AudioClip levelComplete;
    [SerializeField] private AudioClip applySmallPackage;
    [SerializeField] private AudioClip hitWallOrSpecial;

    [Header("Direction Sounds")]
    [SerializeField] private AudioClip upSound;
    [SerializeField] private AudioClip downSound;
    [SerializeField] private AudioClip leftSound;
    [SerializeField] private AudioClip rightSound;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        WorldBus.Victory += OnVictory;
    }

    private void OnDisable()
    {
        WorldBus.Victory -= OnVictory;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnVictory(WorldStateInfo state)
    {
        PlayLevelComplete();
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void PlayLevelComplete()
    {
        Play(levelComplete);
    }

    public void PlayApplySmall()
    {
        Play(applySmallPackage);
    }

    public void PlayHit()
    {
        Play(hitWallOrSpecial);
    }

    public void PlayUp()
    {
        Play(upSound);
    }

    public void PlayDown()
    {
        Play(downSound);
    }

    public void PlayLeft()
    {
        Play(leftSound);
    }

    public void PlayRight()
    {
        Play(rightSound);
    }

    public void PlayDirection(GameCommand command)
    {
        switch (command)
        {
            case GameCommand.Up:
                PlayUp();
                break;
            case GameCommand.Down:
                PlayDown();
                break;
            case GameCommand.Left:
                PlayLeft();
                break;
            case GameCommand.Right:
                PlayRight();
                break;
        }
    }
}
