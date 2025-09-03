using UnityEngine;

public class MoveIngredients : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Vector3 upperPosition;
    public Vector3 lowerPosition;

    [SerializeField] private float speed;
    public bool goingUp = false;
    public bool moving = true;

    public void OnTriggerEnter2D(Collider2D collission)
    {
        moving = false;
    }



    // Update is called once per frame
    void Update()
    {
        Debug.Log("Moving: " + moving + " GoingUp: " + goingUp + " Position: " + transform.localPosition.y);
        if (transform.localPosition.y <= upperPosition.y && goingUp && moving)
        {
            transform.localPosition += new Vector3(0, 10, 0) * Time.deltaTime * speed;
        }
        else if (transform.localPosition.y >= lowerPosition.y && moving)
        {
            goingUp = false;
            transform.localPosition += new Vector3(0, -10, 0) * Time.deltaTime * speed;
            if (transform.localPosition.y <= lowerPosition.y)
            {
                goingUp = true;
            }
        }
    }
}
