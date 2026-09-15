using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class QuestionnaireManager : MonoBehaviour
{
    // timer stuff
    public  float timer = 0f;
    private float minWaitTime = 300f; // TODO 300 minutes TODO
    public bool isWaiting = true;
    public GameObject tutorialEndScreen;
    public GameObject progressBarLastCondition;
    // Comfort
    [Header("Thermal Comfort Data")]
    public GameObject stopPedalingStartQ;
    public GameObject BorgUI;
    public GameObject comfortUIRightIntermediate;
    public ToggleGroup tgBORG;
    public GameObject comfortUILeft;
    public GameObject comfortUILeftIntermediate;
    public ToggleGroup tgComfortLeft;
    private string comfortAnswerRightIntermediate;
    private string comfortAnswerLeftIntermediate;
    private string BorgAnswer;
    private string comfortAnswerLeft;
    private bool isBorgAnswered = false;
    public ToggleGroup tgComfortRightIntermediate;
    public ToggleGroup tgComfortLeftIntermediate;

    // IPQ
    [Header("IPQ Data")]
    public GameObject ipqUi;
    public ToggleGroup tgIPQ;
    public TextMeshProUGUI textAnchorPositive;
    public TextMeshProUGUI textAnchorNegative;
    public TextMeshProUGUI questionHeader;
    //private string filePath = "Assets/Scripts/comfort_test.cs";
    private IPQ_Question[] questions;
    private string[] ipqItemNames;
    public TextAsset csvFile;
    private string[] ipqAnswers;
    private int currentQuestionIPQ = 0; 
    public bool isIPQAnswered = false;

    //PANAS
    [Header("Panas Data")]
    public GameObject panasUi;
    public ToggleGroup tgPANAS;
    public TextMeshProUGUI textAnchor1Panas;
    public TextMeshProUGUI textAnchor2Panas;
    public TextMeshProUGUI textAnchor3Panas;
    public TextMeshProUGUI textAnchor4Panas;
    public TextMeshProUGUI textAnchor5Panas;
    public TextMeshProUGUI questionHeaderPanas;
    //private string filePath = "Assets/Scripts/comfort_test.cs";
    private PANAS_Question[] questionsPanas;
    private string[] panasItemNames;
    public TextAsset csvFilePanas;
    private string[] panasAnswers;
    private int currentQuestionPanas = 0;
    public bool isPanasAnswered = false;
    private bool lastCondition = false;
    //public GameObject wait_ui;

    // csv things
    private TextWriter tw;
    private int scenecounter;
    private int envIndex;
    private int userId;
    private string filePath = Application.dataPath + "/CSV-Data/qr.csv";
    private string filePathIntermediate;
    private string filePathFinishedIds;
    //wait things
    [Header("Waiting Data")]
    public Image timeFeedback;
    public GameObject timeFeedbackUI;
    public GameObject finishUI;

    public UnityEvent questionnairesFinished;
    public UnityEvent intermediateQuestionnaireFinished;

    List<Tuple<string, string,long>> thermalSensationScoresIntermediate;
    private long[] questionTimestamps = new long[6];
    int intermediateQuestionnaireCounter = 0;
    public Image timeFeedbackLastCondition;


    // Start is called before the first frame update
    void Start()
    {
        loadIPQ();
        //loadPanas();
        setIPQQuestion(); //it's the first one
        //setPanasQuestion();
        thermalSensationScoresIntermediate = new List<Tuple<string, string,long>>();
        //does this work with active = false?
        scenecounter = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        envIndex = PlayerPrefs.GetInt("s"+scenecounter);
        filePath = Application.dataPath + "/CSV-Data/" + userId + "_count" + scenecounter + "_env" + envIndex + "_ipq_comfort.csv" ;
        filePathIntermediate = Application.dataPath + "/CSV-Data/" + userId + "_count" + scenecounter + "_env" + envIndex + "_comfortIntermediate.csv";
        filePathFinishedIds = Application.dataPath + "/CSV-Data/FinishedIDs/finishedIds.csv";
    }

    // Update is called once per frame
    void Update()
    {
        if (isWaiting)
        {
            if (timer <= minWaitTime)
            {
                timer += Time.deltaTime;
                if (isIPQAnswered && isBorgAnswered && lastCondition)
                {
                    timeFeedbackLastCondition.fillAmount = timer / minWaitTime;
                }
                if (isIPQAnswered && isBorgAnswered)
                {
                    timeFeedback.fillAmount = timer / minWaitTime;
                }
            }
            else
            {
                isWaiting = false;
            }
        }
        else 
        {
            if (isIPQAnswered && isBorgAnswered)
            {
                if (lastCondition)
                {
                    //finishUI.gameObject.SetActive(true);
                    timeFeedbackUI.SetActive(false);
                    progressBarLastCondition.SetActive(false);
                    writeFinishedID();
                    questionnairesFinished.Invoke();
                    ResetQuestionnairesAnswered();
                }
                else if (scenecounter < 3) {
                    string scenePref = "s" + scenecounter;
                    int sceneIndex = PlayerPrefs.GetInt(scenePref);
                    questionnairesFinished.Invoke();
                    timeFeedbackUI.SetActive(false);
                    stopPedalingStartQ.SetActive(true);
                    //BorgUI.SetActive(true);
                    ResetQuestionnairesAnswered();
                    //Debug.Log("Loading " + sceneIndex);
                    //SceneManager.LoadScene(sceneIndex);
                    timer = 0;
                    Debug.Log("ConditionFinished!!!!");
                }
                else
                {

                }
                
            }
        }
    }

    private void ResetQuestionnairesAnswered()
    {
        isIPQAnswered = false;
        isPanasAnswered = false;
        isBorgAnswered = false;
        resetIPQToggles();
       // resetPanasToggles();
    }

    public void StartQuestionnaire()
    {
        stopPedalingStartQ.SetActive(false);
        BorgUI.SetActive(true);
    }
    public void checkComfortQ()
    {
        Toggle toggle = tgBORG.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            BorgAnswer = toggle.name;
            // Debug.Log("Comfort: " + comfortAnswer);
           // isComfortAnswered = true;
            BorgUI.SetActive(false);
            isBorgAnswered = true;//TODO
            ipqUi.SetActive(true);
            toggle.isOn = false;
            //comfortUILeft.SetActive(true); //TODO
            
        }

        // else show input text pls

    }

    public void checkComfortQRightIntermediate()
    {
        Toggle toggle = tgComfortRightIntermediate.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            comfortAnswerRightIntermediate = toggle.name;
            // Debug.Log("Comfort: " + comfortAnswer);
            // isComfortAnswered = true;
            comfortUIRightIntermediate.SetActive(false);
            comfortUILeftIntermediate.SetActive(true); //TODO
            toggle.isOn = false;

        }
    }

    public void checkComfortQLeftIntermediate()
    {
        Toggle toggle = tgComfortLeftIntermediate.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            
            comfortAnswerLeftIntermediate = toggle.name;
            comfortUILeftIntermediate.SetActive(false);
            // Debug.Log("Comfort: " + comfortAnswer);
            toggle.isOn = false;
            saveValues();
            intermediateQuestionnaireFinished.Invoke();
            comfortUIRightIntermediate.SetActive(true);
            intermediateQuestionnaireCounter++;
        }
        if (intermediateQuestionnaireCounter >= 12) //TODO 12 if 12 times intermediate questionnaires should pop up
        {
            writeCSVIntermediate();
            intermediateQuestionnaireCounter = 0;
        }
        // else show input text pls

    }

    public void tutorialFirstQuestion()
    {
        Toggle toggle = tgComfortRightIntermediate.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            comfortAnswerRightIntermediate = toggle.name;
            // Debug.Log("Comfort: " + comfortAnswer);
            // isComfortAnswered = true;
            comfortUIRightIntermediate.SetActive(false);
            comfortUILeftIntermediate.SetActive(true); //
            toggle.isOn = false;

        }
    }

    public void tutorialSecondQuestion()
    {
        Toggle toggle = tgComfortLeftIntermediate.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {

            comfortAnswerLeftIntermediate = toggle.name;
            comfortUILeftIntermediate.SetActive(false);
            // Debug.Log("Comfort: " + comfortAnswer);
            toggle.isOn = false;
            //saveValues();
            intermediateQuestionnaireFinished.Invoke();
            comfortUIRightIntermediate.SetActive(true);
            intermediateQuestionnaireCounter++;
        }
        if (intermediateQuestionnaireCounter >= 2)
        {
            //writeCSVIntermediate();
            Debug.Log("Done");
            comfortUIRightIntermediate.SetActive(false);
            comfortUILeftIntermediate.SetActive(false);
            tutorialEndScreen.SetActive(true);
            //intermediateQuestionnaireCounter = 0;
        }
        // else show input text pls

    }

    private void saveValues()
    {
        thermalSensationScoresIntermediate.Add(Tuple.Create(comfortAnswerRightIntermediate, comfortAnswerLeftIntermediate, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
    }

    public void checkComfortQLeft()
    {
        Toggle toggle = tgComfortLeft.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            comfortAnswerLeft = toggle.name;
            // Debug.Log("Comfort: " + comfortAnswer);
            isBorgAnswered = true;
            comfortUILeft.SetActive(false); //TODO
            ipqUi.SetActive(true);
            toggle.isOn = false;

        }

        // else show input text pls

    }

    public void ResetQuestionnaireState()
    {
        scenecounter = PlayerPrefs.GetInt("scene counter");
        envIndex = PlayerPrefs.GetInt("s" + scenecounter);
        filePath = Application.dataPath + "/CSV-Data/" + userId + "_count" + scenecounter + "_env" + envIndex + "_ipq_comfort.csv";
        filePathIntermediate = Application.dataPath + "/CSV-Data/" + userId + "_count" + scenecounter + "_env" + envIndex + "_comfortIntermediate.csv";
        emptyArrays();
        BorgAnswer = null;
        comfortAnswerLeft=null;
        currentQuestionPanas = 0;
        currentQuestionIPQ = 0;
    }

    private void emptyArrays()
    {
        for (int i = 0; i < ipqAnswers.Length; i++)
        {
            ipqAnswers[i] = null;
        }
        /*for (int i = 0; i < panasAnswers.Length; i++)
        {
            panasAnswers[i] = null;
        }*/
    }

    public void checkPanas()
    {
        Toggle toggle = tgPANAS.ActiveToggles().FirstOrDefault();

        if (toggle != null)
        {

            // save answers
            //Debug.Log("current question: " + currentQuestion);
            panasAnswers[currentQuestionPanas] = toggle.name;

            // Debug.Log("Current: " + currentQuestion + "answer: " + ipqAnswers[currentQuestion]);


            if (currentQuestionPanas < panasAnswers.Length - 1) // < 13
            {
                currentQuestionPanas += 1;
                resetPanasToggles();
                setPanasQuestion();
            }
            else
            {
                Debug.Log("PANAS_done");
                // writecsv
                //writeQuestionnaireCSV();
                isPanasAnswered = true;
                panasUi.SetActive(false);
                if (scenecounter >= 2)
                {
                    finishUI.gameObject.SetActive(true);
                    writeQuestionnaireCSV();
                    writeFinishedID();
                }
                else
                {
                    //questionnairesFinished.Invoke(); //TODO Put Method in Corrrect Location
                    //timeFeedbackUI.SetActive(true);
                    timeFeedbackUI.SetActive(true);
                    writeQuestionnaireCSV();
                    


                }
            }
        }
    }

    public void checkIPQ()
    {
        Toggle toggle = tgIPQ.ActiveToggles().FirstOrDefault();

        if (toggle != null)
        {

            // save answers
            //Debug.Log("current question: " + currentQuestion);
            ipqAnswers[currentQuestionIPQ] = toggle.name;

            // Debug.Log("Current: " + currentQuestion + "answer: " + ipqAnswers[currentQuestion]);


            if (currentQuestionIPQ < ipqAnswers.Length - 1) // < 13
            {
                currentQuestionIPQ += 1;
                resetIPQToggles();
                setIPQQuestion();
            }
            else
            {
                Debug.Log("IPQ_done");
                // writecsv
                //writeQuestionnaireCSV();
                isIPQAnswered = true;
                ipqUi.SetActive(false);
                if (scenecounter >= 2)
                {

                    progressBarLastCondition.SetActive(true);
                    writeQuestionnaireCSV();
                    lastCondition = true;
                    /*                   finishUI.gameObject.SetActive(true);

                                        writeFinishedID();*/
                }
                else
                {
                    //questionnairesFinished.Invoke(); //TODO Put Method in Corrrect Location
                    //timeFeedbackUI.SetActive(true);
                    timeFeedbackUI.SetActive(true);
                    writeQuestionnaireCSV();
                    lastCondition= false;


                }
            }
        }
    }

    private void loadIPQ()
    {
        string[] lines = csvFile.text.Split('\n');

        questions = new IPQ_Question[lines.Length];
        ipqItemNames = new string[lines.Length];
        ipqAnswers = new string[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(';');

            if (values.Length == 4)
            {
                ipqItemNames[i] = values[0];
                IPQ_Question ipq_question = new IPQ_Question(values[1], values[2], values[3]);
                questions[i] = ipq_question;
                //questions[i] = new IPQ_Question(values[0], values[1], values[2]);
            }
        }
    }

    private void loadPanas()
    {
        string[] lines = csvFilePanas.text.Split('\n');

        questionsPanas = new PANAS_Question[lines.Length];
        panasItemNames = new string[lines.Length];
        panasAnswers = new string[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(';');

            if (values.Length == 7)
            {
                panasItemNames[i] = values[0];
                PANAS_Question panas_question = new PANAS_Question(values[1], values[2], values[3], values[4], values[5], values[6]);
                questionsPanas[i] = panas_question;
                //questions[i] = new IPQ_Question(values[0], values[1], values[2]);
            }
        }
    }

    private void setPanasQuestion()
    {
        questionHeaderPanas.text = questionsPanas[currentQuestionPanas].get_question();
        textAnchor1Panas.text = questionsPanas[currentQuestionPanas].get_anchor1();
        textAnchor2Panas.text = questionsPanas[currentQuestionPanas].get_anchor2();
        textAnchor3Panas.text = questionsPanas[currentQuestionPanas].get_anchor3();
        textAnchor4Panas.text = questionsPanas[currentQuestionPanas].get_anchor4();
        textAnchor5Panas.text = questionsPanas[currentQuestionPanas].get_anchor5();
    }

    private void setIPQQuestion()
    {
        questionHeader.text = questions[currentQuestionIPQ].get_question();
        textAnchorPositive.text = questions[currentQuestionIPQ].get_positive_anchor();
        textAnchorNegative.text = questions[currentQuestionIPQ].get_negative_anchor();
    }

    private void resetIPQToggles()
    {
        Toggle[] toggles = tgIPQ.GetComponentsInChildren<Toggle>();

        foreach (Toggle toggle in toggles)
        {
            toggle.isOn = false;
        }
    }

    private void resetPanasToggles()
    {
        Toggle[] toggles = tgPANAS.GetComponentsInChildren<Toggle>();

        foreach (Toggle toggle in toggles)
        {
            toggle.isOn = false;
        }
    }

    private void writeQuestionnaireCSV()
    {
        string header = "id;scene;time;" + "Borg;" + string.Join(";", ipqItemNames);//+ ";" +string.Join(";",panasItemNames);
        tw = new StreamWriter(filePath, true);
        tw.WriteLine(header);
        string answers = userId + ";" + envIndex + ";" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ";" + BorgAnswer + ";" +
                            string.Join(";", ipqAnswers); //+ ";" + string.Join(";",panasAnswers);

        tw.WriteLine(answers);
        tw.Close();
        Debug.Log("csv should be written now");
        scenecounter += 1;
        PlayerPrefs.SetInt("scene counter", scenecounter);
        
        // Debug.Log(PlayerPrefs.GetInt("scene counter"));
    }


    private void writeCSVIntermediate()
    {
        string headerIntermediate = "id;scene;thermal_sensation;Borg;time";
        StreamWriter twIntermediate = new StreamWriter(filePathIntermediate, true);
        twIntermediate.WriteLine(headerIntermediate);
        for (int i = 0; i < thermalSensationScoresIntermediate.Count; i++)
        {
            twIntermediate.WriteLine(userId + ";" + envIndex + ";" + thermalSensationScoresIntermediate[i].Item1 + ";" + thermalSensationScoresIntermediate[i].Item2 + ";" + thermalSensationScoresIntermediate[i].Item3);
        }
        thermalSensationScoresIntermediate.Clear();
        twIntermediate.Close();
        
    }

    private void writeFinishedID()
    {
        tw = new StreamWriter(filePathFinishedIds, true);
        string answers = "\n" + PlayerPrefs.GetInt("pid") + ";" + PlayerPrefs.GetString("starttime")+";"+ DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        tw.Write(answers);
        tw.Close();

    }


}

public class PANAS_Question
{
    private string question;
    private string anchor_1;
    private string anchor_2;
    private string anchor_3;
    private string anchor_4;
    private string anchor_5;

    public PANAS_Question(string question, string anchor_1, string anchor_2, string anchor_3, string anchor_4, string anchor_5)
    {
        this.question = question;
        this.anchor_1 = anchor_1;
        this.anchor_2 = anchor_2;
        this.anchor_3 = anchor_3;
        this.anchor_4 = anchor_4;
        this.anchor_5 = anchor_5;
    }

    public string get_question()
    {
        return question;
    }

    public string get_anchor1()
    {
        return anchor_1;
    }

    public string get_anchor2()
    {
        return anchor_2;
    }

    public string get_anchor3()
    {
        return anchor_3;
    }

    public string get_anchor4()
    {
        return anchor_4;
    }

    public string get_anchor5()
    {
        return anchor_5;
    }

}
public class IPQ_Question
{
    private string question;
    private string anchor_negative;
    private string anchor_positive;

    public IPQ_Question(string question, string anchor_negative, string anchor_positive)
    {
        this.question = question;
        this.anchor_negative = anchor_negative;
        this.anchor_positive = anchor_positive;
    }

    public string get_question()
    {
        return question;
    }

    public string get_positive_anchor()
    {
        return anchor_positive;
    }

    public string get_negative_anchor()
    {
        return anchor_negative;
    }
}