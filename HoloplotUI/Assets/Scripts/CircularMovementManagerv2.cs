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

    public CustomZoneSpawner zoneSpawner; // Reference to CustomZoneSpawner to access degreeAngles

    [System.Serializable]
    public class CircularObject
    {
        public Transform objectTransform;
        public SpriteRenderer spriteRenderer;

        [HideInInspector]
        public bool isDragging = false;
        [HideInInspector]
        public int angle = 0;  // Store angle as an integer
        [HideInInspector]
        public float snappedAngle = float.NaN; // Store snapped angle
    }

    public List<CircularObject> circularObjects = new List<CircularObject>();

    public Button snapButton; // Reference to the Snap Button

    private CircularObject lastSelectedObject = null; // Track the last selected object

    void Start()
    {
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

        snapButton.onClick.AddListener(SnapObjectsToClosestAngle); // Assign Snap Button click event
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
                            obj.spriteRenderer.color = new Color(0.3f, 0.3f, 1.0f); // Darker blue when dragging
                            obj.spriteRenderer.material.SetFloat("_Glossiness", 0.4f); // Adding glossiness for a 3D effect
                            lastSelectedObject = obj; // Update the last selected object
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

    void SnapObjectsToClosestAngle()
    {
        if (lastSelectedObject != null)
        {
            // Find the closest angle from the degreeAngles list
            float currentAngle = Mathf.Atan2(lastSelectedObject.objectTransform.position.y, lastSelectedObject.objectTransform.position.x) * Mathf.Rad2Deg;
            currentAngle = (currentAngle + 360) % 360; // Normalize angle to [0, 360)

            float closestAngle = zoneSpawner.degreeAngles[0];
            float minDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, closestAngle));

            foreach (float angle in zoneSpawner.degreeAngles)
            {
                float difference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, angle));
                if (difference < minDifference)
                {
                    minDifference = difference;
                    closestAngle = angle;
                }
            }

            // Snap the object to the closest angle
            lastSelectedObject.snappedAngle = closestAngle;
            float radiansClosest = (450 - closestAngle) % 360 * Mathf.Deg2Rad;
            float xClosest = Mathf.Cos(radiansClosest) * 3.0f;
            float yClosest = Mathf.Sin(radiansClosest) * 3.0f;

            lastSelectedObject.objectTransform.position = new Vector2(xClosest, yClosest);

            Debug.Log($"Object snapped to {closestAngle} degrees");
        }
    }
}