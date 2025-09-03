using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class AnticuchoScript : MinigameController
{
    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private int startingPosition = -60;
    [SerializeField] private int targetPosition = 50;
    [SerializeField] private float speed;
    [SerializeField] private float timeToDelay;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;
    private bool goBack = false;
    private bool shoot = false;
    private bool pegarse = false;
    private Transform hitObject;

    IEnumerator DelayAction(float delayTime)
    {
        GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(delayTime);
        goBack = true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        hitObject = collision.transform;
        pegarse = true;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && transform.localPosition.x == startingPosition)
        {
            GetComponent<BoxCollider2D>().enabled = true;
            shoot = true;
        }

        if (shoot)
        {
            if (transform.localPosition.x <= targetPosition)
            {
                transform.localPosition += new Vector3(150, 0, 0) * Time.deltaTime * speed;
            }
            if (transform.localPosition.x >= targetPosition)
            {
                StartCoroutine(DelayAction(timeToDelay));
                shoot = false;
                hitObject.SetParent(this.transform);
            }
        }

        if (goBack)
        {
            if (transform.localPosition.x >= startingPosition)
            {
                transform.localPosition += new Vector3(-30, 0, 0) * Time.deltaTime * speed;
            }
            if (transform.localPosition.x <= startingPosition)
            {
                transform.localPosition = new Vector3(startingPosition, 0, 0);
                goBack = false;
            }
        }
        pegarse = false;
    }
}
