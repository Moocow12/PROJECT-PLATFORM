using System.Collections;
using UnityEngine;

public class CameraScroller : MonoBehaviour
{
    [SerializeField] private float skyDarkYValue = 50.0f;
    [SerializeField] private float skyDarkColorValue = 0.15f;
    [SerializeField] private float distanceDelta = 1.0f;
    [SerializeField] private bool scrollEnabled = false;
    [SerializeField] private float scrollSpeed = 1.0f;
    [SerializeField] private float sky2Mult = 0.1f;
    [SerializeField] private float sky3Mult = 0.2f;
    [SerializeField] private SpriteRenderer sky1;
    [SerializeField] private Transform sky2;
    [SerializeField] private Transform sky3;

    private Vector3 startPos;
    private Vector3 sky2StartPos;
    private Vector3 sky3StartPos;

    public void SetScrollEnabled(bool state)
    {
        scrollEnabled = state;
    }

    public void ReturnToStart()
    {
        StartCoroutine(TravelToStart());
    }

    private void Start()
    {
        startPos = transform.position;
        sky2StartPos = sky2.position;
        sky3StartPos = sky3.position;
    }

    private void Update()
    {
        ProcessScroll();
        ProcessSky();
    }

    private void ProcessScroll()
    {
        if (scrollEnabled)
        {
            transform.Translate(Vector2.up * scrollSpeed * Time.deltaTime);
            sky2.Translate(Vector2.up * scrollSpeed * sky2Mult * Time.deltaTime);
            sky3.Translate(Vector2.up * scrollSpeed * sky3Mult * Time.deltaTime);
        }
    }

    private void ProcessSky()
    {
        float lerpValue = (transform.position.y - startPos.y) / skyDarkYValue;
        float colorValue = Mathf.Lerp(1.0f, skyDarkColorValue, lerpValue);
        sky1.color = new Color(colorValue, colorValue, colorValue);
    }

    public IEnumerator TravelToStart()
    {
        float cameraYInit = transform.position.y;
        float sky2YInit = sky2.position.y;
        float sky3YInit = sky3.position.y;

        float cameraYCurrent;
        float sky2YCurrent;
        float sky3YCurrent;

        float normalDistance = 0.0f;

        
        while (normalDistance < 1.0f)
        {
            cameraYCurrent = Mathf.SmoothStep(cameraYInit, startPos.y, normalDistance);
            transform.position = new Vector3(transform.position.x, cameraYCurrent, transform.position.z);

            sky2YCurrent = Mathf.SmoothStep(sky2YInit, sky2StartPos.y, normalDistance);
            sky2.position = new Vector3(sky2.position.x, sky2YCurrent, sky2.position.z);

            sky3YCurrent = Mathf.SmoothStep(sky3YInit, sky3StartPos.y, normalDistance);
            sky3.position = new Vector3(sky3.position.x, sky3YCurrent, sky3.position.z);

            normalDistance += distanceDelta * Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
    }
}
