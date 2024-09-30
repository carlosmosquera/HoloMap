using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using extOSC; 

public class DropdownHandlerObject : MonoBehaviour
{
    public TMP_Dropdown masterDropdown; // Reference to the Master dropdown
    public List<GameObject> gameObjects; // List of GameObjects to manage
    public List<TMP_Dropdown> objectDropdowns; // List of Dropdowns for each GameObject

    public OSCTransmitter OSCTransmitter;
    private string _objSel = "/objsel/";

    void Start()
    {


        // Add listeners to dropdowns
        masterDropdown.onValueChanged.AddListener(delegate { UpdateGameObjects(); });

        foreach (TMP_Dropdown dropdown in objectDropdowns)
        {
            dropdown.onValueChanged.AddListener(delegate { UpdateGameObjects(); });
        }

        // Initialize the GameObjects' active state
        UpdateGameObjects();
    }

    void UpdateGameObjects()
    {
        int masterValue = masterDropdown.value;

        for (int i = 0; i < gameObjects.Count; i++)
        {
            int objectValue = objectDropdowns[i].value;

            var messageObjSel = new OSCMessage(_objSel + gameObjects[i].name);
            messageObjSel.AddValue(OSCValue.Int(objectValue));
            

            OSCTransmitter.Send(messageObjSel);
            // Activate the GameObject if its dropdown value matches the master dropdown value
            if (objectValue == masterValue)
            {
                gameObjects[i].SetActive(true);

                
            }
            else
            {
                gameObjects[i].SetActive(false);
            }

        }
    }
}
