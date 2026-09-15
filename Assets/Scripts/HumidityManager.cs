using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using UnityEngine;

public class HumidityManager : MonoBehaviour
{
    private bool loggingHumidity = false;
    // source: https://www.hackster.io/raisingawesome/unity-game-engine-and-arduino-serial-communication-12fdd5
    SerialPort sp;
    float next_time;
    // Start is called before the first frame update
    string safe = "";
    private WaitForSeconds readFreq = new WaitForSeconds(1f);
    private string fileName = Application.dataPath + "/CSV-Data/temp.csv";
    private TextWriter tw;
    private int scenecounter;
    private int envIndex;
    private string[] data = new string[2];
    //private float readFreq = 2f;
    private int userId;
    float timePassed = 0f;
    private bool isQuestionnaireTime = false;

    void Start()
    {
        string the_com = "";
        next_time = 1f;
        foreach (string mysps in SerialPort.GetPortNames())
        {
            print(mysps);
            if (mysps == "COM7")
            {
                the_com = mysps;
                break;
            }
        }
        if (the_com != "")
        {
            print("Setup port");
            sp = new SerialPort("\\\\.\\" + the_com, 115200);
            if (!sp.IsOpen)
            {
                print("Opening" + the_com + ", baud 115200");
                sp.Open();
                sp.ReadTimeout = 100;
                sp.Handshake = Handshake.None;
                sp.DtrEnable = true;
                if (sp.IsOpen)
                {
                    print("Open");

                }
            }
            else
            {
                print("is already opened");
            }
        }
        else
        {
            print("the com is empty");
        }
        InvokeRepeating("printHumidity", 0.5f, 0.5f);
    }

    private void printHumidity()
    {
        if (loggingHumidity)
        {
            if (!sp.IsOpen)
            {
                sp.Open();
                Debug.Log("Coroutine OPEN");
            }
            if (sp.IsOpen)
            {
                try
                {
                    string msg = sp.ReadLine();
                    safe = msg;
                    printData(msg);
                    //Debug.Log(msg);
                }
                catch (TimeoutException)
                {
                    printData(safe);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
/*        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {

            StartCoroutine(enumData());

        }*/
        /*      timePassed += Time.deltaTime;
              if (timePassed > next_time)
              { 
                  logData();
                  timePassed = 0;
              } */
    }

    private IEnumerator enumData()
    {
        Debug.Log("Coroutine OPEN");
        while (loggingHumidity)
        {
            if (!sp.IsOpen)
            {
                sp.Open();
                Debug.Log("Coroutine OPEN");
            }
            if (sp.IsOpen)
            {
                try
                {
                    string msg = sp.ReadLine();
                    safe = msg;
                    printData(msg);
                    //Debug.Log(msg);
                }
                catch (TimeoutException)
                {
                    printData(safe);
                }
            }
            yield return readFreq;
        }
    }
    /*
        private void logData()
        {

            if (!sp.IsOpen)
            {
                sp.Open();
                //print("opened sp");
            }
            if (sp.IsOpen)
            {
                try
                {
                    string msg = sp.ReadLine();
                    safe = msg;
                    printData(msg);

                }
                catch (TimeoutException)
                {
                    printData(safe);
                }
            }
        }
    */

    public void changePhase(bool questionnaireTime)
    {
        isQuestionnaireTime = questionnaireTime;
    }
    private void printData(string msg)
    {
        Debug.Log("Coroutine PRINTS");
        if (msg.Length > 0)
        {
            data = msg.Split(";");
            string dataPoint = userId + ";" + envIndex + ";" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ";" + data[0] + ";" + data[1] + ";" + data[2] + ";" + isQuestionnaireTime;
            tw = new StreamWriter(fileName, true);
            tw.WriteLine(dataPoint);
            tw.Close();
        }

    }

    public void InitializeFileLogging()
    {
        Debug.Log("Coroutine Started");
        scenecounter = PlayerPrefs.GetInt("scene counter");
        userId = PlayerPrefs.GetInt("pid");
        envIndex = PlayerPrefs.GetInt("s" + scenecounter);
        fileName = Application.dataPath + "/CSV-Data/" + userId + "_count" + scenecounter + "_env" + envIndex + "_roomtemperature.csv";
        tw = new StreamWriter(fileName, true);
        string header = "id;scene;timestamp;temp;pressure;humidity;questionnaireTime";
        tw.WriteLine(header);
        tw.Close();
        loggingHumidity = true;
    }

    public void StopFileLogging()
    {
        //keepLogging = false;
        loggingHumidity = false;
        Debug.Log("Coroutine Stopped");
    }

    void OnApplicationQuit()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Close();
            sp = null;
        }
    }
}