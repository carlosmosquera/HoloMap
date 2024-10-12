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
        public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer

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