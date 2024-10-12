using UnityEngine;
using System.Collections.Generic;
using TMPro;
using extOSC;

public class CircularMovementManagerv2 : MonoBehaviour
{
    public OSCTransmitter Transmitter;

    [System.Serializable]
    public class CircularObject
    {
        public Transform objectTransform;

        [HideInInspector]
        public bool isDragging = false;
        [HideInInspector]
        public float angle = 0.0f;
    }

    public List<CircularObject> circularObjects = new List<CircularObject>();

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
        }
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
                obj.isDragging = false;
            }
        }

        foreach (var obj in circularObjects)
        {
            if (obj.isDragging)
            {
                // Move object in a circular path
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = mousePosition - Vector2.zero; // Replacing centralPoint with Vector2.zero

                // Normalize direction to keep the object on the circumference
                direction.Normalize();

                // Set the object's position to the edge of the circle
                obj.objectTransform.position = Vector2.zero + direction * 3.0f; // Replacing centralPoint with Vector2.zero

                float cartCoordsX = obj.objectTransform.position.x;
                float cartCoordsY = obj.objectTransform.position.y;
                float cartCoordsZ = obj.objectTransform.position.z;

                float outRadius;
                float outPolar;

                outPolar = Mathf.Atan2(cartCoordsY, cartCoordsX);
                outPolar = (-outPolar * Mathf.Rad2Deg + 450) % 360;

                outRadius = Mathf.Sqrt((cartCoordsX * cartCoordsX)
                                + (cartCoordsY * cartCoordsY));

                int ObjectNumber = circularObjects.IndexOf(obj) + 1;

                string _panAddress = "" + ObjectNumber;

                var messagePan = new OSCMessage(_panAddress);
                messagePan.AddValue(OSCValue.Float(outPolar));

                Debug.Log($"Object {ObjectNumber} position: {outPolar}");

                Transmitter.Send(messagePan);
            }
        }
    }
}
