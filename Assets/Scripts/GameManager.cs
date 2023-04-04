using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private int startingPlatformCount = 10;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject resetGameButton;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private GameObject altitudeMarker;
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraScroller cameraScroller;
    [SerializeField] private PlatformManager platformManager;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip failSound;

    private bool gameEnded = false;
    private bool wasVictory = false;
    private int currentLevel = 1;
    private int currentPlatformCount;
    private float highestAltitude = 0.0f;
    private Vector3 playerStartPos;
    private AudioSource audioSource;

    public bool GetGameEnded()
    {
        return gameEnded;
    }

    public void StartGame()
    {
        if (wasVictory)
        {
            currentLevel++;
            currentPlatformCount = startingPlatformCount * currentLevel;
            highestAltitude = 0.0f;
        }
        platformManager.SetMaxPlatforms(currentPlatformCount);
        startGameButton.SetActive(false);
        cameraScroller.SetScrollEnabled(true);
        player.SetCanMoveEnabled(true);
        gameEnded = false;
        wasVictory = false;
    }

    public void ResetGame()
    {
        continueButton.SetActive(false);
        resetGameButton.SetActive(false);
        StartCoroutine(GameResetSequence());
    }

    public void SetVolumeMuted(bool state)
    {
        audioSource.volume = state ? 0.0f : 0.5f;
        player.GetComponent<AudioSource>().volume = state ? 0.0f : 0.5f;
    }

    public void ProcessGameEnd(bool victory)
    {
        if (gameEnded)
        {
            return;
        }
        
        cameraScroller.SetScrollEnabled(false);
        if (victory)
        {
            //print("game manager victory");
            continueButton.SetActive(true);
            player.Win();
            player.SetCanMoveEnabled(false);
            audioSource.clip = winSound;
            audioSource.Play();
        }
        else
        {
            resetGameButton.SetActive(true);
            if (!player.IsDead())
            {
                player.Die();
                player.SetCanMoveEnabled(false);
            }
            audioSource.clip = failSound;
            audioSource.Play();
        }
        gameEnded = true;
        wasVictory = victory;
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        instance = this;

        audioSource = Camera.main.GetComponent<AudioSource>();
    }

    private void Start()
    {
        startGameButton.SetActive(true);
        resetGameButton.SetActive(false);
        player.SetCanMoveEnabled(false);
        playerStartPos = player.transform.position;
        currentPlatformCount = startingPlatformCount;
    }

    private void FixedUpdate()
    {
        ProcessScore();
        ProcessPlayerBounds();
    }

    private void ProcessScore()
    {
        float currentAltitude;
        if (!gameEnded)
        {
            currentAltitude = player.transform.position.y - playerStartPos.y;
            highestAltitude = (currentAltitude > highestAltitude) ? currentAltitude : highestAltitude;
        }

        levelText.text = "LEVEL " + currentLevel.ToString();
        altitudeText.text = "BEST ALT. " + highestAltitude.ToString("000");

        Vector3 altMarkerPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.95f, 0));
        altMarkerPos.y = playerStartPos.y + highestAltitude;
        altMarkerPos.z = 0.0f;
        altitudeMarker.transform.position = altMarkerPos;
    }
    private void ProcessPlayerBounds()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(player.gameObject.transform.position);
        bool offScreen;
        if (!gameEnded)
        {
            offScreen = screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;
            if (offScreen)
            {
                ProcessGameEnd(false);
            }
        }
        else
        {
            offScreen = screenPos.y < (Screen.height - (Screen.height * 1.2f));
            if (offScreen)
            {
                player.SetVisible(false);
                player.ResetPlayer();
            }
        }
    }

    private IEnumerator GameResetSequence()
    {
        yield return StartCoroutine(cameraScroller.TravelToStart());
        startGameButton.SetActive(true);
        platformManager.ResetPlatforms();
        player.ResetPlayer();
        player.SetVisible(true);
    }
}
