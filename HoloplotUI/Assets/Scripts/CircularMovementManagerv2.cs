using UnityEngine;
using System.Collections.Generic;
using TMPro;
using extOSC;
using UnityEngine.UI; // For Button UI

public class CircularMovementManagerv2 : MonoBehaviour
{
    public OSCTransmitter Transmitter;
    public OSCTransmitter Transmitter10000;

    public OSCReceiver Receiver;

    public Button sendButton;

    [System.Serializable]
    public class CircularObject
    {
        public Transform objectTransform;
        public SpriteRenderer spriteRenderer;

        [HideInInspector]
        public bool isDragging = false;
        [HideInInspector]
        public int angle = 0;  // Store angle as an integer
    }

    public List<CircularObject> circularObjects = new List<CircularObject>();

    void Start()
    {
        //sendButton.onClick.AddListener(OnSendBang);

        // Initialize the number for each object
        for (int i = 0; i < circularObjects.Count; i++)
        {
            CircularObject obj = circularObjects[i];

            // Create a new GameObject for the number
            GameObject numberObject = new GameObject("Number");
            numberObject.transform.SetParent(obj.objectTransform);
            numberObject.transform.localPosition = Vector3.zero;

            // Add and configure the TextMesh component
            TextMeshPro textMesh = numberObject.AddComponent<TextMeshPro>();
            textMesh.text = (i + 1).ToString();
            textMesh.fontSize = 4;
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.sortingOrder = 9;

            // Adjust the position if necessary
            numberObject.transform.localPosition = new Vector3(0, 0, -1);

            // Add a SpriteRenderer if not already set
            if (obj.spriteRenderer == null)
            {
                obj.spriteRenderer = obj.objectTransform.GetComponent<SpriteRenderer>();
                if (obj.spriteRenderer == null)
                {
                    obj.spriteRenderer = obj.objectTransform.gameObject.AddComponent<SpriteRenderer>();
                }
            }

            // Set the initial appearance of the circular object
            obj.spriteRenderer.color = new Color(0.7f, 0.7f, 1.0f); // Light blue
            obj.spriteRenderer.sortingOrder = 8;
        }

        // Bind receiver in Start to avoid re-binding in Update
        //Receiver.Bind("/objectPositions", OnReceivePositions);
    }

    //void OnSendBang()
    //{
    //    // Create a new OSC message with a specific address
    //    var message = new OSCMessage("/bang");

    //    // Add an integer value of 1 (acting as a "bang")
    //    message.AddValue(OSCValue.Int(1));

    //    // Send the message via the transmitter
    //    Transmitter10000.Send(message);

    //    // Optional: Print debug info
    //    Debug.Log("Bang message sent to Max/MSP");
    //}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Get all hits under the mouse position
            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hits.Length > 0)
            {
                // Find the topmost object in the circularObjects list that matches a hit
                foreach (var hit in hits)
                {
                    foreach (var obj in circularObjects)
                    {
                        if (hit.collider.transform == obj.objectTransform)
                        {
                            obj.isDragging = true;
                            obj.spriteRenderer.color = new Color(0.3f, 0.3f, 1.0f); // Darker blue when dragging
                            obj.spriteRenderer.material.SetFloat("_Glossiness", 0.4f); // Adding glossiness for a 3D effect
                            break;
                        }
                    }
                    if (circularObjects.Exists(o => o.isDragging)) break; // Break if one is found
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            foreach (var obj in circularObjects)
            {
                if (obj.isDragging)
                {
                    obj.isDragging = false;
                    obj.spriteRenderer.color = new Color(0.7f, 0.7f, 1.0f); // Revert to light blue when not dragging
                }
            }
        }

        foreach (var obj in circularObjects)
        {
            if (obj.isDragging)
            {
                // Move object in a circular path
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = mousePosition.normalized;

                // Set the object's position to the edge of the circle with a radius of 3.0f
                obj.objectTransform.position = direction * 3.0f;

                // Convert position to polar angle in degrees, rounded to an integer
                float outPolarFloat = Mathf.Atan2(obj.objectTransform.position.y, obj.objectTransform.position.x) * Mathf.Rad2Deg;
                int outPolar = Mathf.RoundToInt((450 - outPolarFloat) % 360); // Convert to integer and normalize to [0, 360)

                int ObjectNumber = circularObjects.IndexOf(obj) + 1;

                // Create an OSC message with the address and polar angle
                var messagePan = new OSCMessage("/objectPosition");
                messagePan.AddValue(OSCValue.Int(ObjectNumber));
                messagePan.AddValue(OSCValue.Int(outPolar));  // Send outPolar as an integer

                Debug.Log($"Object {ObjectNumber} position: {outPolar} degrees");

                Transmitter.Send(messagePan);
            }
        }
    }

    //private void OnReceivePositions(OSCMessage message)
    //{
    //    Debug.Log(message.Values.Count);
    //    Debug.Log(message.Values[0].StringValue);

    //    if (message.Values.Count == 32) // 1 string + 32 integer arguments
    //    {
    //        string messageLabel = message.Values[0].StringValue; // Assuming it's a descriptive string
    //        Debug.Log($"Received OSC Message - Label: {messageLabel}");

    //        for (int i = 0; i < 32; i++)
    //        {
    //            int angle = message.Values[i + 1].IntValue;

    //            if (i < circularObjects.Count)
    //            {
    //                CircularObject obj = circularObjects[i];

    //                // Convert angle back to Cartesian coordinates and set object's position
    //                float angleInRadians = angle * Mathf.Deg2Rad;
    //                Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    //                obj.objectTransform.position = direction * 3.0f;  // Assuming the radius is still 3.0f

    //                Debug.Log($"Object {i + 1} position updated based on received angle: {angle} degrees");
    //            }
    //            else
    //            {
    //                Debug.LogWarning($"Received object index {i + 1} is out of bounds for the circular objects list.");
    //            }
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Received OSC message does not have the expected number of arguments.");
    //    }
    //}
}
