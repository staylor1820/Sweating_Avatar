using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

public class TutorialManager: MonoBehaviour
{
    // Target stuff
    [Header("TargetManager")]
    private float targetTimout = 7f; // TODO 7f
    private float targetTimer = 0f;
    private float heatingTime;
    public float maxHeating;
    public GameObject targetHolder;
    private int numTargets;
    // private bool isTargetVisibile = false;
    private bool isTargetTimeout = false;
    private bool isTargetVisible = false;
    public GameObject heaterLeft;
    public GameObject heaterRight;
    public GameObject questionnaireUI;
    public GameObject questionnaireUIIntermediate;
    public GameObject headset;
    public GameObject startExpUI;
    // Gaze stuff
    [Header("Gaze Interaction Manager")]
    private Vector3 hitPosition = new Vector3(0, 0, 0);
    public Camera cam;
    public GameObject reticle;
    private float gazeTimer = 0f;
    private float minGazeTime = 5f;
    public Image timeFeedback;
    private bool leftHandIsHeating;
    private bool rightHandIsHeating;
    public bool StartTutorial = false;
    // questionnaire stuff
    [Header("Questionnaire Manager")]
    private float questionnaireTimeInterval = 5f; // TODO 90
    private int questionnaireCount = 0; // TODO
    private bool isQuestionaireDone = false;
    public Slider slider;
    public GameObject thermalSensationUI;
    private bool isIntermediateQuestionTime = false;
    private float[] thermalSensationScores = new float[6];
    private long[] questionTimestamps = new long[6];
    public GameObject instructionsUI;
    public Material rightHeatMat;
    public Material leftHeatMat;
    // Scene Manager
    private float sceneTimer = 0f;
    private float interMediatequestionTimer = 0f;
    private float maxSceneTime = 600f; // TODO 10 minutes
    private TextWriter tw;
    private string filePath = Application.dataPath + "/CSV-Data/env_.csv";
    //public GameObject leftControllerRay;
    //public GameObject rightControllerRay;
    int conditionIndex;
    int userId;
    int currentScene;
    bool experimentRunning;
    bool questionnaireIsBeingFilled = false;
    public Color hotColor;
    public Color coldColor;
    public Color emissionColor;
    public GameObject finishUI;
    public QuestionnaireManager QuestionnaireController;
    public GameObject GazeInteraction;
    private int intermediateQuestionCounter = 0;
    public GameObject leftRayInteractor;
    public GameObject rightRayInteractor;
    private bool experimentStart = false;
    private int experimentstartcounter = 0;
    public GameObject questionnaireSound;
    private int QuestionCounter = 0;
    // Start is called before the first frame update
    void Start()
    {
        //leftControllerRay.gameObject.SetActive(false);
        //rightControllerRay.gameObject.SetActive(false);
        experimentRunning = true;
        heatingTime = 0f;
        //startExpUI.SetActive(true);
        /*numTargets = targetHolder.transform.childCount;
        currentScene = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        conditionIndex = PlayerPrefs.GetInt("s"+currentScene);
        Debug.Log("counter: " + currentScene + ", index: " + conditionIndex);
        rightHeatMat.SetColor("_EmissionColor", coldColor);
        rightHeatMat.DisableKeyword("_EMISSION");
        rightHeatMat.SetColor("_BaseColor", coldColor);
        leftHeatMat.SetColor("_EmissionColor", coldColor);
        leftHeatMat.DisableKeyword("_EMISSION");
        leftHeatMat.SetColor("_BaseColor", coldColor);
        //filePath = Application.dataPath + "/CSV-Data/" + userId + "_count" + currentScene + "_env" + conditionIndex + "_sensation.csv" ;
        //InitializeFileLogging();
        //activateCorrectHeater();*/
        //leftRayInteractor.SetActive(false);
        //rightRayInteractor.SetActive(false);

    }

    private void InitializeFileLogging()
    {
        filePath = Application.dataPath + "/CSV-Data/" + userId + "_count" + currentScene + "_env" + conditionIndex + "_conditions.csv";
        string header = "id;conditionIndex;positionx;positiony;leftHandInHeater;rightHandInHeater;timestamp;gender";
        tw = new StreamWriter(filePath, true);
        tw.WriteLine(header);
        tw.Close();
    }

    private void activateCorrectHeater()
    {
        switch (conditionIndex)
        {
            case 1:
                rightHeatMat.SetColor("_EmissionColor", emissionColor * 1.81f);
                rightHeatMat.EnableKeyword("_EMISSION");
                rightHeatMat.SetColor("_BaseColor", hotColor);
                leftHeatMat.SetColor("_EmissionColor", emissionColor * 1.81f);
                leftHeatMat.EnableKeyword("_EMISSION");
                leftHeatMat.SetColor("_BaseColor", hotColor);
                break;
            case 2:
                rightHeatMat.SetColor("_EmissionColor", coldColor);
                rightHeatMat.DisableKeyword("_EMISSION");
                rightHeatMat.SetColor("_BaseColor", coldColor);
                leftHeatMat.SetColor("_EmissionColor", coldColor);
                leftHeatMat.DisableKeyword("_EMISSION");
                leftHeatMat.SetColor("_BaseColor", coldColor);
                break;
            default:
                break;
        }
        //InvokeRepeating("WritePositionDataToCSV", 0.5f, 0.5f);
    }


    void WritePositionDataToCSV()
    {
        tw = new StreamWriter(filePath, true);
        tw.WriteLine(userId + ";" + conditionIndex + ";" + headset.transform.position.x + ";" + headset.transform.position.y + ";" + leftHandIsHeating + ";" + rightHandIsHeating +";" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()+";"+PlayerPrefs.GetString("Gender"));
        tw.Close();
        Debug.Log("csv written");
    }
    // Update is called once per frame
    void Update()
    {


            if (StartTutorial && !isIntermediateQuestionTime)
            {
            experimentStart = true;
            experimentstartcounter = 1;
            
            //InitializeFileLogging();
            //activateCorrectHeater();
            startExpUI.SetActive(false);
            instructionsUI.SetActive(true);
            heatingTime += Time.deltaTime;
            timeFeedback.fillAmount = heatingTime / maxHeating;
             checkSceneTime();
            }

        
    }

    private void checkTargets()
    {

        if (isTargetTimeout)
        {
            // wait for x seconds 
            targetTimer += Time.deltaTime;
            if (targetTimer > targetTimout)
            {
                isTargetTimeout = false;
                targetTimer = 0f;
            }
        }
        else if (!isTargetVisible)
        {
            // show random target
            targetHolder.transform.GetChild(UnityEngine.Random.Range(0, numTargets)).gameObject.SetActive(true);
            isTargetVisible = true;
        }



    }

    public void LeftHandTouched()
    {
        leftHandIsHeating = true;
        Debug.Log(leftHandIsHeating);
        //ShowQuestionnaire();
        //instructionsUI.SetActive(false);
    }

    public void LeftHandRemoved()
    {
        leftHandIsHeating = false;
        Debug.Log(leftHandIsHeating);
    }


    public void RightHandTouched()
    {
        rightHandIsHeating = true;
        Debug.Log(rightHandIsHeating);
        //ShowQuestionnaire();
        //instructionsUI.SetActive(false);
    }

    public void RightHandRemoved()
    {
        rightHandIsHeating = false;
        Debug.Log(rightHandIsHeating);
    }

    private void checkSceneTime()
    {
        interMediatequestionTimer += Time.deltaTime;

        // do things that must be done when its no question time
        if (interMediatequestionTimer >= 10)
        {
            // Show the questionnaire
            Debug.Log("Intermedia Question Time!");
            ShowThermalPerceptionQuestionnaire();
            isIntermediateQuestionTime = true;
            // Reset sceneTimer for the next interval
            interMediatequestionTimer = 0f;
            intermediateQuestionCounter++;
        }
    }

    private void ShowThermalPerceptionQuestionnaire()
    {

            instructionsUI.SetActive(false);
            questionnaireUI.SetActive(false);
            questionnaireUIIntermediate.SetActive(true);
            GazeInteraction.SetActive(true);
            questionnaireSound.GetComponent<AudioSource>().Play();
            //questionnaireUI.SetActive(true);
        

    }



    public void ResetThermalPerceptionQuestionnaire()
    {
        GazeInteraction.SetActive(false);
        instructionsUI.SetActive(true);
        questionnaireUI.SetActive(false);
        questionnaireUIIntermediate.SetActive(false);
        //interMediatequestionTimer = 0f;
        isIntermediateQuestionTime = false;
        QuestionCounter++;
        if (QuestionCounter >= 3)
        {
            Debug.Log("Ende Tutorial");
        }
    }

    private void ShowQuestionnaire()
    {
        questionnaireSound.GetComponent<AudioSource>().Play();
        //leftRayInteractor.SetActive(true);
        //rightRayInteractor.SetActive(true);
        GazeInteraction.SetActive(true);
        //maybe reset value of slider
        slider.value = 50;
        //thermalSensationUI.SetActive(true);
        instructionsUI.SetActive(false);
        questionnaireUI.SetActive(true);
        //CancelInvoke("WritePositionDataToCSV");
        QuestionnaireController.timer = 0;
        QuestionnaireController.isWaiting = true;
        heatingTime = 0;
        questionnaireIsBeingFilled = true;
        //rightHeatMat.SetColor("_EmissionColor", coldColor);
        //rightHeatMat.DisableKeyword("_EMISSION");
        //rightHeatMat.SetColor("_BaseColor", coldColor);
        //leftHeatMat.SetColor("_EmissionColor", coldColor);
        //leftHeatMat.DisableKeyword("_EMISSION");
        //leftHeatMat.SetColor("_BaseColor", coldColor);
        //QuestionnaireController.
        //leftControllerRay.gameObject.SetActive(true);
        //rightControllerRay.gameObject.SetActive(true);
    }

    public void loadNewCondition()
    {
       /* experimentStart = false;
        experimentstartcounter = 0;
        startExpUI.SetActive(true);
        questionnaireUI.SetActive(false);
        currentScene = PlayerPrefs.GetInt("scene counter");
        leftRayInteractor.SetActive(false);
        rightRayInteractor.SetActive(false);
        QuestionnaireController.ResetQuestionnaireState();
        questionnaireIsBeingFilled = false;
        GazeInteraction.SetActive(false);
        Debug.Log("CURRENTSCENE: "+currentScene);
        if (currentScene <= 2)
        {
            //currentScene = currentScene + 1;
            conditionIndex = PlayerPrefs.GetInt("s" + currentScene);
            Debug.Log("counter: " + currentScene + ", index: " + conditionIndex);
            questionnaireUI.SetActive(false);
            heatingTime = 0;
            timeFeedback.fillAmount = 0;
            interMediatequestionTimer = 0;
            //activateCorrectHeater();
            //InitializeFileLogging();
            //InvokeRepeating("WritePositionDataToCSV", 0.5f, 0.5f);
            intermediateQuestionCounter = 0;
            isIntermediateQuestionTime = false;
        }
        else
        {
            rightHeatMat.SetColor("_EmissionColor", coldColor);
            rightHeatMat.DisableKeyword("_EMISSION");
            rightHeatMat.SetColor("_BaseColor", coldColor);
            leftHeatMat.SetColor("_EmissionColor", coldColor);
            leftHeatMat.DisableKeyword("_EMISSION");
            leftHeatMat.SetColor("_BaseColor", coldColor);
            instructionsUI.SetActive(false);
            finishUI.SetActive(true);
            questionnaireUI.SetActive(false);
            experimentRunning = false;
        }
       */
    }

    // called when UI-Button is pressed 
    public void checkQuestionnaireDone()
    {
        // save
        // set avtive false
        if (questionnaireCount <= 5)
        {

            thermalSensationScores[questionnaireCount] = slider.value;
            questionTimestamps[questionnaireCount] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            questionnaireCount += 1;
        }

        // 6th questionnaire taken
        if (questionnaireCount > 5)
        {
            //writeCSV();
            isQuestionaireDone = true;
        }
        thermalSensationUI.SetActive(false);
        //leftControllerRay.gameObject.SetActive(false);
        //rightControllerRay.gameObject.SetActive(false);
        isIntermediateQuestionTime = false;
    }

    private void writeCSV()
    {
        string header = "id;scene;thermal_sensation_90; thermal_sensation_180; thermal_sensation_270; thermal_sensation_360; thermal_sensation_450;thermal_sensation_540; time_90; time_180; time_270; time_360; time_450; time_540";
        tw = new StreamWriter(filePath, true);
        tw.WriteLine(header);

        string answers = string.Join(";", thermalSensationScores);
        string timestamps = string.Join(";", questionTimestamps);
        tw.WriteLine(userId + ";"+ conditionIndex + ";" + answers + ";" + timestamps);
        tw.Close();
        Debug.Log("csv written");
    }



    private void resetGaze()
    {
        reticle.SetActive(false);
        if (gazeTimer > 0)
        {
            gazeTimer -= Time.deltaTime;
        }
        if (timeFeedback != null)
        {
            timeFeedback.fillAmount = gazeTimer / minGazeTime;
        }

    }

}
