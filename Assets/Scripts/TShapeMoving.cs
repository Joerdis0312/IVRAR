using UnityEngine;

public class TShapeMoving : MonoBehaviour
{

    public ParkourCounter parkourCounter;
    public LocomotionTechnique loco;

    public GameObject donePanel;
    public GameObject TargetT;

    Vector3 startPosition;

    void Update()
    {
        // startposition with the center as the target
        startPosition = TargetT.transform.position;

        // first round: moving left-right
        if (parkourCounter.interacting == 1)
        {
            // for the third banner: moving along x-axis
            if (loco.currentFieldCount == 45)
            {
                float newPos = Mathf.PingPong(Time.time * 2f, 3f) - 1.5f;
                transform.position = new Vector3(transform.position.x, transform.position.y, startPosition.z + newPos);
            }
            // for the first and second banner: moving along y-axis
            else
            { 
                float newPos = Mathf.PingPong(Time.time * 2f, 3f) - 1.5f;
                transform.position = new Vector3(startPosition.x + newPos, transform.position.y, transform.position.z);
            }
        }
        // second round: moving up-down
        if (parkourCounter.interacting == 2)
        {
            float newPos = Mathf.PingPong(Time.time * 2f, 3f) - 1.5f;
            transform.position = new Vector3(transform.position.x, startPosition.y + newPos, transform.position.z);
        }
        // third round: turning around y-axis
        if (parkourCounter.interacting == 3)
        {
            transform.Rotate(new Vector3(0, 1, 0));
            donePanel.SetActive(true);
        }
    }
}
