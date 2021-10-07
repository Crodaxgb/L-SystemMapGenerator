using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeVisualization : MonoBehaviour
{
    public GameObject displayText;
    public NodeInformation nodeDisplayInfo;
    public GameObject offSetForText;
    private Camera cameraRef;
    private Text nodeInfoTextReference;
    void Awake()
    {
        cameraRef = Camera.main;
        nodeInfoTextReference = Instantiate(displayText, GameObject.FindObjectOfType<Canvas>().transform).GetComponent<Text>();
        //Center the text
        nodeInfoTextReference.alignment = TextAnchor.MiddleCenter;
        nodeInfoTextReference.color = Color.white;
        nodeInfoTextReference.fontSize = 30;
        //In order to display it, because during an overflow the text becomes invisible
        nodeInfoTextReference.verticalOverflow = VerticalWrapMode.Overflow;
        nodeInfoTextReference.text = nodeDisplayInfo.nodeType;
    }
    // Update is called once per frame
    void Update()
    {
        //Map the object's location to the canvas (2D plane)
        nodeInfoTextReference.transform.position = cameraRef.WorldToScreenPoint(offSetForText.transform.position);
        
    }

   
}
