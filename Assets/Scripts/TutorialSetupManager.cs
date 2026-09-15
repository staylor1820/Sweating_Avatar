using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSetupManager : MonoBehaviour
{
    // Latin Square csv load
    // Start is called before the first frame update
    void Start()
    {
      
        loadScene();
    }

   

    private void loadScene()
    {

        SceneManager.LoadScene(1);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
