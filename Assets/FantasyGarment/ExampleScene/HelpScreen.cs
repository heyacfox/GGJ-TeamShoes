using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpScreen : MonoBehaviour
{
    public GameObject helpPanel;
    public bool showHelpOnStart = true;
    private bool showHelp;

    private void Start()
    {
        showHelp = showHelpOnStart; ;
        helpPanel.SetActive(showHelp);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showHelp = !showHelp;
            helpPanel.SetActive(showHelp);
        }
    }
}
