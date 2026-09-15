using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System;
using System.Drawing.Printing;
using System.IO;
using UnityEngine.SceneManagement;
using System.Net;


public class SerialPortRoomManagr : MonoBehaviour
{
    private SerialPort serialPort;
    public string portName = "COM6"; // Change to your exact COM port

    void Start()
    {
        // Open the port cleanly with standard settings
        serialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
        serialPort.Open();

        // Start with the lines low (0 Volts)
        serialPort.RtsEnable = true;
        serialPort.DtrEnable = true;
        Debug.Log("Port open. Voltage set to 0V.");
    }

    private void Update()
    {

    }

    // Call this to trigger marker
    public void TriggerEventMarker(int state)
    {

        if (serialPort != null && serialPort.IsOpen)
        {
            //byte[] dataBuffer = new byte[] { 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31 };
            if (state == 1)
            {
                ResetMarkers();
                serialPort.Write("01");
            }
            else if (state == 2)
            {
                ResetMarkers();
                serialPort.Write("02");
            }
            else if (state == 3)
            {
                ResetMarkers();
                serialPort.Write("04");
            }
            else if (state == 4)
            {
                ResetMarkers();
                serialPort.Write("08");
            }
            //Debug.Log("Sent bit to COM port.");

            // 2. Toggle the state for the NEXT run

        }
    }

    public void ResetMarkers()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write("00");
        }
    }

    public void CloseSerialPort()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write("00");
            serialPort.Close();
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Write("00");
            serialPort.Close();
        }
    }
}
