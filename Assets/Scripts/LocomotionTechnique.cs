using UnityEngine;
using System.Collections;

public class LocomotionTechnique : MonoBehaviour
{
    // Please implement your locomotion technique in this script. 
    public OVRInput.Controller leftController;
    public OVRInput.Controller rightController;
    [Range(0, 10)] public float translationGain = 0.5f;
    public GameObject hmd;

    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;

    // values for the jumps
    [SerializeField] float jumpHeight = 5;
    public float jumpDuration = 2.83f;

    // number of the current field
    public int currentFieldCount;
    public bool jumping;

    // dice minigame
    public GameObject Dice;

    // T-shape interaction
    public GameObject taskStartPanel;

    // card minigame
    public GameObject Card1;
    public GameObject Card2;
    public GameObject Card3;
    public GameObject Card4;
    public GameObject CartFront;

    // reaction minigame
    public GameObject CubeOrange;
    public GameObject CubeBlue;

    // boxes to get the armswings to jump
    public GameObject JumpBoxL;
    public GameObject JumpBoxR;
    // booleans if the controllers are inside the boxes
    public bool jumpL;
    public bool jumpR;

    // field positions
    public GameObject currentField;
    public Vector3 currentFieldPos;
    public GameObject nextField;
    public Vector3 nextFieldPos;

    // direction facing the next field to place the minigames
    public Vector3 direction;
    public Vector3 up;
    public Vector3 right;

    void Start()
    {
        currentFieldCount = 0;
        jumping = false;

        // get position of the first field
        currentField = GameObject.Find("Field_" + (currentFieldCount));
        currentFieldPos = currentField.transform.position;

        // get position of the second field
        nextField = GameObject.Find("Field_" + (currentFieldCount + 1));
        nextFieldPos = nextField.transform.position;

        // compute the direction facing to the next field
        direction = new Vector3(nextFieldPos.x - currentFieldPos.x, 0f, nextFieldPos.z - currentFieldPos.z).normalized;
        up = Vector3.up;
        right = Vector3.Cross(up, direction).normalized;
        transform.rotation = Quaternion.LookRotation(nextFieldPos);

        // all not used objects are not active but they are existing so that the different scripts uses the same objects
        Dice.SetActive(false);
        taskStartPanel.SetActive(false);
        Card1.SetActive(false);
        Card2.SetActive(false);
        Card3.SetActive(false);
        Card4.SetActive(false);
        CartFront.SetActive(false);
        CubeOrange.SetActive(false);
        CubeBlue.SetActive(false);
        JumpBoxL.SetActive(false);
        JumpBoxR.SetActive(false);
        jumpL = false;
        jumpR = false;
    }

    void Update()
    {
        // get the speed of the contollers
        float leftSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).magnitude;
        float rightSpeed = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude;
        // middle the speed of the two controllers
        float totalSpeed = (leftSpeed + rightSpeed) / 2f;

        // compute the direction facing to the next field
        direction = new Vector3(nextFieldPos.x - currentFieldPos.x, 0f, nextFieldPos.z - currentFieldPos.z).normalized;
        right = Vector3.Cross(up, direction).normalized;

        // get the angle between the head rotation and the direction to the next fiels
        Vector3 currentDirection = hmd.transform.forward;
        currentDirection.y = 0;
        currentDirection.Normalize();
        float angle = Vector3.Angle(direction, currentDirection);

        // if the player is in a state possible of jumping place invisible boxes (jump boxes) in the direction to the next field
        if (parkourCounter.availableJumps != 0 && !jumping && parkourCounter.interacting == 0)
        {
            JumpBoxL.transform.rotation = Quaternion.LookRotation(direction);
            JumpBoxR.transform.rotation = Quaternion.LookRotation(direction);

            Vector3 offset = currentFieldPos + (direction * 0.5f) + (up * 1f);

            JumpBoxL.transform.position = offset - (right * 0.3f) + (up * 0.2f);
            JumpBoxR.transform.position = offset + (right * 0.3f) + (up * 0.2f);

            JumpBoxL.SetActive(true);
            JumpBoxR.SetActive(true);
        }
        // jump if the player is looking to the next field (within 20°) and both controllers are swinging fast enough
        if (jumpL && jumpR && !jumping && parkourCounter.availableJumps != 0 && parkourCounter.interacting == 0 && angle < 20 && totalSpeed > 2f)
        {
            StartCoroutine(JumpCoroutine(nextFieldPos));
            
            // set the new field values
            currentFieldCount++;
            currentField = GameObject.Find("Field_" + (currentFieldCount));
            currentFieldPos = currentField.transform.position;
            nextField = GameObject.Find("Field_" + (currentFieldCount + 1));
            nextFieldPos = nextField.transform.position;

            // set the new diretion to the next field
            direction = new Vector3(nextFieldPos.x - currentFieldPos.x, 0f, nextFieldPos.z - currentFieldPos.z).normalized;
            right = Vector3.Cross(up, direction).normalized;

            // reduce the number of available jumps by 1
            if (parkourCounter.availableJumps > 0)
            {
                parkourCounter.availableJumps--;
            }
            else
            {
                // add 1 if the player was jumping backwards
                parkourCounter.availableJumps++;
            }

            // deactivate the jump boxes
            JumpBoxL.SetActive(false);
            JumpBoxR.SetActive(false);
            jumpL = false;
            jumpR = false;
        }

        // These are for the game mechanism.
        if (OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.Button.Four))
        {
            if (parkourCounter.parkourStart)
            {
                // reset to the current field if you get lost
                transform.position = currentFieldPos;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {

        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.scoreText.text = "";
            selectionTaskMeasure.partSumErr = 0f;
            selectionTaskMeasure.partSumTime = 0f;
            // rotation: facing to the next field
            selectionTaskMeasure.taskUI.transform.LookAt(nextFieldPos);
            selectionTaskMeasure.taskStartPanel.SetActive(true);
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("field"))
        {
            // fields with the T-shape interaction (even if jumps are available)
            if (currentFieldCount == 18 || currentFieldCount == 33 || currentFieldCount == 45)
            {
                taskStartPanel.SetActive(true);
            }
            // if no jumps are available choose the type of the field random
            else if (parkourCounter.availableJumps == 0)
            {
                int mode = Random.Range(1, 4);
                if (mode == 1)
                {
                    diceStartCoroutine();
                }
                if (mode == 2)
                {
                    cardStartCoroutine();
                }
                if (mode == 3)
                {
                    reactionStartRoutine();
                }
            }
        }
    }

    // jump to the target position with linear interpolation
    IEnumerator JumpCoroutine(Vector3 target)
    {
        jumping = true;
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / jumpDuration;

            // linear interpolation for horizontal moving
            Vector3 currentPos = Vector3.Lerp(startPos, target, normalizedTime);

            // parabel for the height
            currentPos.y += Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;

            transform.position = currentPos;
            yield return null;
        }

        // place the player at the exact correct position so that it is not slightly off
        transform.position = target;
        jumping = false;
    }

    void diceStartCoroutine()
    {
        // place dice facing to the next field
        Vector3 offset = new Vector3(direction.x * 0.75f, 1.5f, direction.z * 0.75f);
        Dice.transform.position = currentFieldPos + offset;
        Dice.transform.rotation = Quaternion.LookRotation(direction);
        Dice.transform.Rotate(-90, 0, 0);
        Dice.SetActive(true);
    }

    void cardStartCoroutine()
    {
        // place the cards facing to the next field
        Vector3 offset = currentFieldPos + (direction * 0.5f) + (up * 1.75f);

        // upper left
        Card1.transform.position = offset - (right * 0.3f) + (up * 0.4f);
        Card1.transform.rotation = Quaternion.LookRotation(direction);
        Card1.transform.Rotate(90, 90, 0);
        Card1.SetActive(true);

        // upper right
        Card2.transform.position = offset + (right * 0.3f) + (up * 0.4f);
        Card2.transform.rotation = Quaternion.LookRotation(direction);
        Card2.transform.Rotate(90, 90, 0);
        Card2.SetActive(true);

        // lower left
        Card3.transform.position = offset - (right * 0.3f) - (up * 0.4f);
        Card3.transform.rotation = Quaternion.LookRotation(direction);
        Card3.transform.Rotate(90, 90, 0);
        Card3.SetActive(true);

        // lower right
        Card4.transform.position = offset + (right * 0.3f) - (up * 0.4f);
        Card4.transform.rotation = Quaternion.LookRotation(direction);
        Card4.transform.Rotate(90, 90, 0);
        Card4.SetActive(true);
    }

    void reactionStartRoutine()
    {
        // place a random reaction box at a ranfom point
        int r = Random.Range(1, 3);
        Vector3 offset = currentFieldPos + (direction * 0.5f) + (up * 1.5f);

        float randUp = Random.Range(-0.4f, 0.4f);
        float randSide = Random.Range(-0.4f, 0.4f);

        // the box for the left hand more on the left side
        if (r == 1)
        {
            CubeBlue.transform.position = offset + (right * (randSide-0.1f)) + (up * randUp);
            CubeBlue.SetActive(true);
        }
        // the box for the right hand more on the right side
        else if (r == 2)
        {
            CubeOrange.transform.position = offset + (right * (randSide+0.1f)) + (up * randUp);
            CubeOrange.SetActive(true);
        }
    }
}