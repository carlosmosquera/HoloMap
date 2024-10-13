using System.Collections.Generic;
using UnityEngine;
using TMPro;
using extOSC;
using UnityEngine.UI; // For Button UI

public class CircularMovementManagerv2 : MonoBehaviour
{
    public OSCTransmitter Transmitter;
    public OSCTransmitter Transmitter10000;

    public OSCReceiver Receiver;

    public Button sendButton;
    public Button snapButton;

    [System.Serializable]
    public class CircularObject
    {
        public Transform objectTransform;
        public SpriteRenderer spriteRenderer;

        [HideInInspector]
        public bool isDragging = false;
        [HideInInspector]
        public bool isSelected = false; // New flag to indicate if the object is selected
        [HideInInspector]
        public int angle = 0;  // Store angle as an integer
    }

    public List<CircularObject> circularObjects = new List<CircularObject>();
    public List<float> degreeAngles; // List of degree angles provided by the user

    void Start()
    {
        // Get degree angles from CustomZoneSpawner
        CustomZoneSpawner zoneSpawner = FindObjectOfType<CustomZoneSpawner>();
        if (zoneSpawner != null)
        {
            degreeAngles = zoneSpawner.degreeAngles;
        }
        else
        {
            Debug.LogError("CustomZoneSpawner not found in the scene.");
        }

        //sendButton.onClick.AddListener(OnSendBang);
        snapButton.onClick.AddListener(SnapToClosestAngle);

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
                            obj.isSelected = true;
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

    void SnapToClosestAngle()
    {
        foreach (var obj in circularObjects)
        {
            if (obj.isSelected)
            {
                // Find the closest angle from degreeAngles list
                float currentAngle = Mathf.Atan2(obj.objectTransform.position.y, obj.objectTransform.position.x) * Mathf.Rad2Deg;
                float closestAngle = FindClosestAngle(currentAngle);

                // Convert closest angle to Cartesian coordinates and set object's position
                float angleInRadians = closestAngle * Mathf.Deg2Rad;
                Vector2 snappedPosition = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * 3.0f;
                obj.objectTransform.position = snappedPosition;

                Debug.Log($"Object snapped to closest angle: {closestAngle} degrees");
            }
        }
    }

    float FindClosestAngle(float currentAngle)
    {
        float minDistance = Mathf.Infinity;
        float closestAngle = currentAngle;

        foreach (float angle in degreeAngles)
        {
            float distance = Mathf.Abs(Mathf.DeltaAngle(currentAngle, angle));
            if (distance < minDistance)
            {
                minDistance = distance;
                closestAngle = angle;
            }
        }

        return closestAngle;
    }
}