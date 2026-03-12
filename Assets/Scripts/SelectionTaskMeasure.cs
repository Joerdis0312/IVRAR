using System.Collections;
using UnityEngine;
using TMPro;
public class SelectionTaskMeasure : MonoBehaviour
{
    public GameObject TargetT;
    public GameObject ObjectT;

    public GameObject taskStartPanel;
    // panel to stop the moving object
    public GameObject stopPanel;
    public GameObject donePanel;
    public TMP_Text startPanelText;
    public TMP_Text scoreText;
    public int completeCount;
    public bool isTaskStart;
    public bool isTaskEnd;
    public bool isCountdown;
    public Vector3 manipulationError;
    public float taskTime;
    public GameObject taskUI;
    public ParkourCounter parkourCounter;
    public DataRecording dataRecording;
    public LocomotionTechnique loco;
    private int part;
    public float partSumTime;
    public float partSumErr;


    // Start is called before the first frame update
    void Start()
    {
        parkourCounter = GetComponent<ParkourCounter>();
        dataRecording = GetComponent<DataRecording>();
        part = 1;
        donePanel.SetActive(false);
        scoreText.text = "Part" + part.ToString();
        taskStartPanel.SetActive(false);
        stopPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isTaskStart)
        {
            // recording time
            taskTime += Time.deltaTime;
        }

        if (isCountdown)
        {
            taskTime += Time.deltaTime;
            startPanelText.text = (3.0 - taskTime).ToString("F1");
        }
    }

    public void StartOneTask()
    {
        taskTime = 0f;
        taskStartPanel.SetActive(false);
        stopPanel.SetActive(true);
        TargetT.SetActive(true);
        ObjectT.SetActive(true);
        float randX = Random.Range(0.0f, 360.0f);
        float randY = Random.Range(0.0f, 360.0f);
        float randZ = Random.Range(0.0f, 360.0f);
        ObjectT.transform.position = taskUI.transform.position + taskUI.transform.forward*2.0f + taskUI.transform.up * 1.5f;
        TargetT.transform.position = taskUI.transform.position + taskUI.transform.forward*2.0f + taskUI.transform.up * 1.5f;

        TargetT.transform.rotation = Quaternion.Euler(randX, randY, randZ);
        ObjectT.transform.rotation = Quaternion.Euler(randX, randY, randZ);
    }

    public void EndOneTask()
    {
        donePanel.SetActive(false);
        stopPanel.SetActive(false);
        taskStartPanel.SetActive(false);

        // release
        isTaskEnd = true;
        isTaskStart = false;
        
        // distance error
        manipulationError = Vector3.zero;
        for (int i = 0; i < TargetT.transform.childCount; i++)
        {
            manipulationError += TargetT.transform.GetChild(i).transform.position - ObjectT.transform.GetChild(i).transform.position;
        }
        scoreText.text = scoreText.text + "Time: " + taskTime.ToString("F1") + ", offset: " + manipulationError.magnitude.ToString("F2") + "\n";
        partSumErr += manipulationError.magnitude;
        // add jumps based on how well it was done
        if (manipulationError.magnitude < 0.2)
        {
            parkourCounter.availableJumps += 6;
            parkourCounter.lastRoll = 6;
        }
        else if (manipulationError.magnitude < 0.4)
        {
            parkourCounter.availableJumps += 5;
            parkourCounter.lastRoll = 5;
        }
        else if (manipulationError.magnitude < 0.7)
        {
            parkourCounter.availableJumps += 4;
            parkourCounter.lastRoll = 4;
        }
        else if (manipulationError.magnitude < 1)
        {
            parkourCounter.availableJumps += 3;
            parkourCounter.lastRoll = 3;
        }
        else if (manipulationError.magnitude < 1.5)
        {
            parkourCounter.availableJumps += 2;
            parkourCounter.lastRoll = 2;
        }
        else
        {
            parkourCounter.availableJumps += 1;
            parkourCounter.lastRoll = 1;
        }
        partSumTime += taskTime;
        dataRecording.AddOneData(parkourCounter.locomotionTech.stage.ToString(), completeCount, taskTime, manipulationError);

        TargetT.SetActive(false);
        ObjectT.SetActive(false);
    }

}
