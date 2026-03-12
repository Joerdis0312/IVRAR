using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System.Collections;

public class MyGrab : MonoBehaviour
{
    public OVRInput.Controller controller;
    public SelectionTaskMeasure selectionTaskMeasure;
    public LocomotionTechnique loco;

    public ParkourCounter parkourCounter;

    // dice minigame
    public GameObject Dice;

    // card minigame
    public GameObject Card1;
    public GameObject Card2;
    public GameObject Card3;
    public GameObject Card4;
    public GameObject CardFront;
    public TMP_Text cardText;
    public GameObject cardTextGO;

    // reaction minigame
    public GameObject CubeOrange;
    public GameObject CubeBlue;

    void start()
    {
        parkourCounter.interacting = 0;
        parkourCounter.reactionCount = 0;
    }

    void Update()
    {
        // stop the reaction minigame 10 seconds after hitting the first cube
        // controller == OVRInput.Controller.LTouch: otherwise it will be executed for both hands which leads to some inconsistences
        if ((Time.time - parkourCounter.reactionTime) > 10 && parkourCounter.isTimerRunning && controller == OVRInput.Controller.LTouch)
        {
            CubeOrange.SetActive(false);
            CubeBlue.SetActive(false);

            // get jumps based on how many cubes are hit in the 10 seconds
            parkourCounter.availableJumps = (int)(parkourCounter.reactionCount / 4.0f);

            if (parkourCounter.availableJumps == 0)
            {
                parkourCounter.availableJumps = 1;
            }

            // remember the roll
            parkourCounter.lastRoll = parkourCounter.availableJumps;

            // reset the values for the next time
            parkourCounter.isTimerRunning = false;
            parkourCounter.reactionCount = 0;
            parkourCounter.reactionTime = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("dice") && !loco.jumping)
        {
            GetComponent<AudioSource>().Play();
            diceRollCoroutine();
        }
        else if (other.gameObject.CompareTag("selectionTaskStart") && !loco.jumping)
        {
            GetComponent<AudioSource>().Play();
            // start the t-shape interaction with mode 1: moving left-right
            parkourCounter.interacting = 1;
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.StartOneTask();
        }
        else if (other.gameObject.CompareTag("stop") && !loco.jumping)
        {
            GetComponent<AudioSource>().Play();
            // chage the moving to the next
            parkourCounter.interacting += 1;
        }
        else if (other.gameObject.CompareTag("done") && !loco.jumping)
        {
            GetComponent<AudioSource>().Play();
            // reset for the next time
            parkourCounter.interacting = 0;
            selectionTaskMeasure.isTaskStart = false;
            selectionTaskMeasure.EndOneTask();
        }
        else if (other.gameObject.CompareTag("card") && !CardFront.activeSelf && !loco.jumping)
        {
            GetComponent<AudioSource>().Play();
            cardRollCoroutine(other.gameObject);
        }
        else if (other.gameObject.CompareTag("cube") && !loco.jumping)
        {
            // orange cubes can only hit by the right hand
            if (other.gameObject == CubeOrange && controller == OVRInput.Controller.RTouch)
            {
                CubeOrange.SetActive(false);
                if (!parkourCounter.isTimerRunning)
                {
                    parkourCounter.reactionTime = Time.time;
                    parkourCounter.isTimerRunning = true;
                }
                parkourCounter.reactionCount += 1;
                GetComponent<AudioSource>().Play();
                reactionRollRoutine();
            }
            // blue cubes can only hit by the left hand
            if (other.gameObject == CubeBlue && controller == OVRInput.Controller.LTouch)
            {
                CubeBlue.SetActive(false);
                if (!parkourCounter.isTimerRunning)
                {
                    parkourCounter.reactionTime = Time.time;
                    parkourCounter.isTimerRunning = true;
                }
                parkourCounter.reactionCount += 1;
                GetComponent<AudioSource>().Play();
                reactionRollRoutine();
            }
        }
        // left controller is in the left jumpBox
        else if (other.gameObject.CompareTag("jumpL") && controller == OVRInput.Controller.LTouch)
        {
            loco.jumpL = true;
        }
        // right controller is in the right jumpBox
        else if (other.gameObject.CompareTag("jumpR") && controller == OVRInput.Controller.RTouch)
        {
            loco.jumpR = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // left controller is not in the left jumpBox
        if (other.gameObject.CompareTag("jumpL") && controller == OVRInput.Controller.LTouch)
        {
            loco.jumpL = false;
        }
        // right controller is not in the right jumpBox
        else if (other.gameObject.CompareTag("jumpR") && controller == OVRInput.Controller.RTouch)
        {
            loco.jumpR = false;
        }
    }

    void diceRollCoroutine()
    {
        // get a random number for hitting the dice
        int jumps = Random.Range(1, 7);
        parkourCounter.availableJumps = jumps;
        // remember the roll
        parkourCounter.lastRoll = parkourCounter.availableJumps;
        Dice.SetActive(false);
    }

    void cardRollCoroutine(GameObject card)
    {
        // show the card front side at the position of the hitted card
        CardFront.transform.position = card.transform.position + loco.direction * 0.5f;

        CardFront.transform.localRotation = card.transform.localRotation;
        CardFront.transform.Rotate(0, 0, 180);
        CardFront.SetActive(true);
        card.SetActive(false);

        // choose random wich card you get
        int mode = Random.Range(1, 8);
        // one field backward
        if (mode == 1)
        {
            // you can't get back to last t-shape interaction - instead one field forward
            if (loco.currentFieldCount == 0 || loco.currentFieldCount == 19 || loco.currentFieldCount == 34)
            {
                parkourCounter.availableJumps = 1;
                cardText.text = "One field \n forward";
            }
            else
            {
                parkourCounter.availableJumps = -1;
                cardText.text = "One field \n backward";
                // set the next field to the last, so that you can jump to this field
                loco.currentFieldCount -= 2;
                loco.nextField = GameObject.Find("Field_" + (loco.currentFieldCount + 1));
                loco.nextFieldPos = loco.nextField.transform.position;
            }
        }
        // one field forward
        if (mode == 2)
        {
            parkourCounter.availableJumps = 1;
            cardText.text = "One field \n forward";
        }
        // two fields forward
        if (mode == 3)
        {
            parkourCounter.availableJumps = 2;
            cardText.text = "Two fields \n forward";
        }
        // three fields forward
        if (mode == 4)
        {
            parkourCounter.availableJumps = 3;
            cardText.text = "Three fields \n forward";
        }
        // same as the last roll
        if (mode == 5)
        {
            parkourCounter.availableJumps = parkourCounter.lastRoll;
            cardText.text = "Last roll \n again";
        }
        // half as the last roll
        if (mode == 6)
        {
            // if last roll was -1 or 1: instead one field forward
            if (parkourCounter.lastRoll == -1 || parkourCounter.lastRoll == 1)
            {
                parkourCounter.availableJumps = 1;
                cardText.text = "One field \n forward";
            }
            else
            {
                // get the half of last roll (rounded down)
                parkourCounter.availableJumps = (int)(parkourCounter.lastRoll/2.0f);
                cardText.text = "Half of \n last roll";
            }
        }
        // twice the last roll
        if (mode == 7)
        {
            // if last roll was -1: instead one field forward
            if (parkourCounter.lastRoll == -1)
            {
                parkourCounter.availableJumps = 1;
                cardText.text = "One field \n forward";
            }
            else
            {
                parkourCounter.availableJumps = parkourCounter.lastRoll * 2;
                cardText.text = "Double of \n last roll";
            }
        }

        // remember the roll
        parkourCounter.lastRoll = parkourCounter.availableJumps;

        // deactivate the cards after two seconds - so that you have time to read which card you got
        StartDelayedDisable();
    }

    void StartDelayedDisable()
    {
        StartCoroutine(DisableAfterTime(2f));
    }

    IEnumerator DisableAfterTime(float delay)
    {
        // wait for number of delay seconds
        yield return new WaitForSeconds(delay);

        // deactivate the cards
        Card1.SetActive(false);
        Card2.SetActive(false);
        Card3.SetActive(false);
        Card4.SetActive(false);
        CardFront.SetActive(false);
    }

    void reactionRollRoutine()
    {
        // place a random reaction box at a ranfom point
        int r = Random.Range(1, 3);
        Vector3 offset = loco.currentFieldPos + (loco.direction * 0.5f) + (loco.up * 1.5f);

        float randUp = Random.Range(-0.4f, 0.4f);
        float randSide = Random.Range(-0.4f, 0.4f);

        // the box for the left hand more on the left side
        if (r == 1)
        {
            CubeBlue.transform.position = offset + (loco.right * (randSide - 0.1f)) + (loco.up * randUp);
            CubeBlue.SetActive(true);
        }
        // the box for the right hand more on the right side
        else if (r == 2)
        {
            CubeOrange.transform.position = offset + (loco.right * (randSide + 0.1f)) + (loco.up * randUp);
            CubeOrange.SetActive(true);
        }
    }

}