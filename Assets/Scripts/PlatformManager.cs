using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [SerializeField] private int xPosOffsetMin = 2;
    [SerializeField] private int xPosOffsetMax = 4;
    [SerializeField] private float cameraYDeltaThreshold = 5.0f;
    [SerializeField] private float xPosOffsetValue = 1.0f;
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private GameObject winningPlatformPrefab;
    
    private int platformCountMax = 10;
    private int platformCountCurrent = 1;
    private float prevPlatformXPos = 0.0f;
    private float cameraYDelta;
    private float prevCameraYPos;
    private List<GameObject> platforms = new List<GameObject>();

    public void ResetPlatforms()
    {
        for (int i = 0; i < platforms.Count;)
        {
            Destroy(platforms[i].gameObject);
            platforms.RemoveAt(i);
        }
        cameraYDelta = cameraYDeltaThreshold;
        platformCountCurrent = 1;
    }

    public void SetMaxPlatforms(int value)
    {
        platformCountMax = value;
    }

    private void Start()
    {
        prevCameraYPos = Camera.main.transform.position.y;
        cameraYDelta = cameraYDeltaThreshold;
    }

    private void Update()
    {
        ProcessPlatforms();
    }

    private void ProcessPlatforms()
    {
        if (cameraYDelta >= cameraYDeltaThreshold)
        {
            if (platformCountCurrent < platformCountMax - 1)
            {
                SpawnNewPlatform();
                cameraYDelta = 0.0f;
                platformCountCurrent++;
            }
            else if (platformCountCurrent < platformCountMax)
            {
                SpawnWinningPlatform();
                platformCountCurrent++;
            }
        }
        
        ProcessPlatformYDelta();
    }

    private void SpawnNewPlatform()
    {
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height * 1.1f));
        float newXOffset = Random.Range(xPosOffsetMin, xPosOffsetMax);
        bool isOffsetPositive = Random.Range(0, 2) == 1;
        newXOffset *= isOffsetPositive ? 1.0f : -1.0f;
        spawnPos.x = prevPlatformXPos + (newXOffset * xPosOffsetValue);
        spawnPos.z = 1.0f;

        GameObject newPlatform = Instantiate(platformPrefab, spawnPos, Quaternion.identity, transform);
        platforms.Add(newPlatform);
    }

    private void SpawnWinningPlatform()
    {
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 1.1f));
        spawnPos.z = 0.0f;

        GameObject newPlatform = Instantiate(winningPlatformPrefab, spawnPos, Quaternion.identity, transform);
        platforms.Add(newPlatform);
    }

    private void ProcessPlatformYDelta()
    {
        float yPosDelta = Camera.main.transform.position.y - prevCameraYPos;
        cameraYDelta += yPosDelta;
        prevCameraYPos = Camera.main.transform.position.y;
    }
}
