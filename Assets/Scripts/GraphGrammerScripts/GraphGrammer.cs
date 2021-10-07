
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Graph_and_Grammer;

public class GraphGrammer : MonoBehaviour
{
    Graph graph;
    public GameObject[] nodeArray;
    public Material lineRendererMat;
    StartRule initilializeTheLevelRule;
    List<GraphRule> listOfRules;
    private bool isLevelInitialized = false;
    float timer = 0f;
    void Start()
    {
        listOfRules = new List<GraphRule>();
        //Generate an initial graph
        graph = new Graph();
        //start with a start node which the start rule will be applied to
        Node startNode = new Node(worldPosition: Vector3.zero, nodeType: "Level");
        //Add it to the graph
        graph.AddNode(startNode);
        initilializeTheLevelRule = new StartRule();
        //Append a rule to the rule list
        listOfRules.Add(new VariateRule());
        listOfRules.Add(new AddNodeBetweenRule());
        listOfRules.Add(new DisjointRule());
        listOfRules.Add(new AddNodeRule("room"));
        listOfRules.Add(new AddNodeRule("enemy"));
        listOfRules.Add(new AddNodeRule("supplier"));
        listOfRules.Add(new DeleteNodesRule());
        listOfRules.Add(new EnemyRule(4, "Enemy"));
        listOfRules.Add(new BossRule(3, "Boss"));
        listOfRules.Add(new SupplierRule(new List<string>() { "enemy", "enemy", "room" }));
        listOfRules.Add(new SupplierRule(new List<string>() { "boss", "room" }));
        listOfRules.Add(new SupplierRule(new List<string>() { "room", "boss", "enemy" }));
        listOfRules.Add(new HookToRandom("room", 1, 3));
        listOfRules.Add(new HookToRandom("room", 1, 3));
        listOfRules.Add(new HookToRandom("enemy", 1, 3));
        listOfRules.Add(new HookToRandom("boss", 1, 3));
        listOfRules.Add(new HookToRandom("supplier", 1, 3));
        //GenerateTestGraph();
        UpdateGraph();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && timer > 0.2f)
        {
            timer = 0f;
            //If the level is not initialized yet first initialize it
            if (!isLevelInitialized)
            {
                initilializeTheLevelRule.ApplyRule(graph);
                //Then mark it as initilized, One way lock
                isLevelInitialized = true;
                UpdateGraph();
            }
            else
            {
                //Select a random rule and apply it 
                listOfRules.ElementAt(Random.Range(0, listOfRules.Count)).ApplyRule(graph);
                UpdateGraph();
            }


        }
        //if(isLevelInitialized)
        //{
        timer += Time.deltaTime;
        //}

    }


    private void OnDrawGizmos()
    {
        /*
        if(graph == null)
        {
            Start();
        }
        foreach(var node in graph.NodeList)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(node.WorldPosition, 0.250f);
            //DrawString(node.nodeId.ToString(), node.Data, Color.white);           
            foreach (var neighborNode in node.Neighbors)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawLine(neighborNode.WorldPosition, node.WorldPosition);
                
            }
        } 
        */
    }
    private void UpdateGraph()
    {
        //First delete every child of this object. This will clean the screen from the previous
        //Iterations of the graph
        //List<Vector3> nodePositions = new List<Vector3>();
        //Delete the text objects under the canvas
        foreach (Transform childText in GameObject.FindObjectOfType<Canvas>().GetComponentInChildren<Transform>())
        {
            Destroy(childText.gameObject);
        }

        //Delete the instantiated node prefabs
        foreach (Transform child in this.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }

        foreach (Node component in graph.NodeList)
        {
            LineRenderer lrRef = null;

            List<Vector3> nodePositions = new List<Vector3>();
            if (component.NodeType.Equals("start"))
            {
                GameObject obj = Instantiate(nodeArray[0], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            else if (component.NodeType.Equals("room"))
            {
                GameObject obj = Instantiate(nodeArray[1], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            else if (component.NodeType.Equals("end"))
            {
                GameObject obj = Instantiate(nodeArray[2], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            else if (component.NodeType.Equals("level"))
            {
                GameObject obj = Instantiate(nodeArray[3], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            else if (component.NodeType.Equals("supplier"))
            {
                GameObject obj = Instantiate(nodeArray[4], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            else if (component.NodeType.Equals("enemy"))
            {
                GameObject obj = Instantiate(nodeArray[5], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            else if (component.NodeType.Equals("boss"))
            {
                GameObject obj = Instantiate(nodeArray[6], component.WorldPosition, Quaternion.identity, this.transform);
                lrRef = obj.AddComponent<LineRenderer>();
            }
            //Material assignment
            lrRef.material = lineRendererMat;
            lrRef.startWidth = 0.1f;
            lrRef.endWidth = 0.1f;
            foreach (Node neighbor in component.Neighbors)
            {
                //Assign the neighbor first    
                nodePositions.Add(neighbor.WorldPosition);
                //Come back to the current node position
                nodePositions.Add(component.WorldPosition);
            }
            lrRef.positionCount = nodePositions.Count;
            lrRef.SetPositions(nodePositions.ToArray());

        }



    }
    static void DrawString(string text, Vector3 worldPos, Color? colour = null)
    {
        UnityEditor.Handles.BeginGUI();
        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
        Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        UnityEditor.Handles.EndGUI();
    }

    private void MoveAwayFromAddition()
    {
        /*
        Vector3 additionPlace = (cube2.transform.position + cube1.transform.position) / 2;
        Vector3 relativePos3 = cube3.transform.InverseTransformPoint(additionPlace).normalized;       

        cube3.transform.position -= relativePos3;        

        Vector3 targetDirection = (cube2.transform.position - cube1.transform.position).normalized;
        Vector3 perpDir = Vector3.Cross(targetDirection, Vector3.back);
        Vector3 midPoint = (cube1.transform.position + cube2.transform.position) / 2;
        Vector3 offSet = midPoint + (perpDir * -5);

        cube3.transform.position = offSet;
        */
    }
    private void GenerateTestGraph()
    {/*
        
        graph = new Graph(EdgeType.UNDIRECTED);
        var node1 = new Node(new Vector3(0, 0, 0), "Start");
        var node2 = new Node(new Vector3(5, 0, 0), "Room");
        var node3 = new Node(new Vector3(10, 0, 0), "Boss");
        var node4 = new Node(new Vector3(15, 0, 0), "Enemy");
        var node5 = new Node(new Vector3(20, 0, 0), "Enemy");
        var node6 = new Node(new Vector3(25, 0, 0), "End");
        var node7 = new Node(new Vector3(15, 10, 0), "Enemy");
        var node8 = new Node(new Vector3(25, -10, 0), "Room");
        var node9 = new Node(new Vector3(0, 10, 0), "Room");
        var node10 = new Node(new Vector3(0, -10, 0), "Room");
        graph.AddNode(node1);
        graph.AddNode(node2, node1);
        graph.AddNode(node3, node2);
        graph.AddNode(node4, node3);
        graph.AddNode(node5, node4);
        graph.AddNode(node6, node5);

        graph.AddNode(node7, node4);
        graph.AddNode(node8, node6);
        graph.AddNode(node9, node2);
        graph.AddNode(node10, node2);
        
        // Convert Enemy&Boss case
        
        graph = new Graph(EdgeType.UNDIRECTED);
        var node1 = new Node(new Vector3(0, 0, 0), "Start");
        var node2 = new Node(new Vector3(5, 0, 0), "Room");
        var node3 = new Node(new Vector3(10, 0, 0), "Room");
        var node4 = new Node(new Vector3(15, 0, 0), "Room");
        var node5 = new Node(new Vector3(20, 0, 0), "Room");
        var node6 = new Node(new Vector3(25, 0, 0), "End");
        var node7 = new Node(new Vector3(15, 10, 0), "Room");
        var node8 = new Node(new Vector3(25, -10, 0), "Room");
        var node9 = new Node(new Vector3(0, 10, 0), "Room");
        var node10 = new Node(new Vector3(0, -10, 0), "Room");
        graph.AddNode(node1);
        graph.AddNode(node2, node1);
        graph.AddNode(node3, node2);
        graph.AddNode(node4, node3);
        graph.AddNode(node5, node4);
        graph.AddNode(node6, node5);
        
        graph.AddNode(node7, node4);
        graph.AddNode(node8, node6);
        graph.AddNode(node9, node2);
        graph.AddNode(node10, node2);
        */
        /*
         * Delete Case
        graph = new Graph(EdgeType.UNDIRECTED);
        var node1 = new Node(new Vector3(0, 0, 0), "Start");
        var node2 = new Node(new Vector3(5, 0, 0), "Room");
        var node3 = new Node(new Vector3(10, 0, 0), "Room");
        var node4 = new Node(new Vector3(15, 0, 0), "Room");
        var node5 = new Node(new Vector3(20, 0, 0), "Room");
        var node6 = new Node(new Vector3(25, 0, 0), "Room");
        var node7 = new Node(new Vector3(30, 0, 0), "Room");
        var node8 = new Node(new Vector3(35, 0, 0), "End");
        var node9 = new Node(new Vector3(10, 10, 0), "Room");
        graph.AddNode(node1);
        graph.AddNode(node2, node1);
        graph.AddNode(node3, node2);
        graph.AddNode(node4, node3);
        graph.AddNode(node5, node4);
        graph.AddNode(node6, node5);
        graph.AddNode(node7, node6);
        graph.AddNode(node8, node7);
        graph.AddNode(node9);
        graph.AddEdge(node9, node4);
        /*
        *Whole case
        graph = new Graph(EdgeType.UNDIRECTED);
        var node1 = new Node(new Vector3(0, 10, 0), "Start");
        var node2 = new Node(new Vector3(0, 5, 0), "Room");
        var node3 = new Node(new Vector3(0, 0, 0), "Room");
        var node4 = new Node(new Vector3(0, -5, 0), "End");
        var node5 = new Node(new Vector3(0, 0, 5), "End");
        var node6 = new Node(new Vector3(0, 0, 10), "Room");
        var node7 = new Node(new Vector3(0, 10, 15), "End");
        var node8 = new Node(new Vector3(0, 5, 15), "Room");
        var node9 = new Node(new Vector3(0, 0, 15), "Room");
        var node10 = new Node(new Vector3(0, -5, 15), "Room");
        var node11 = new Node(new Vector3(0, -10, 15), "End");
        var node12 = new Node(new Vector3(0, 0, 20), "Room");
        var node13 = new Node(new Vector3(0, 0, 25), "Room");
        var node14 = new Node(new Vector3(0, 0, 30), "Room");
        var node15 = new Node(new Vector3(0, 0, 35), "End");
        var node16 = new Node(new Vector3(0, 20, 35), "End");

        graph.AddNode(node1);
        graph.AddNode(node2, node1);
        graph.AddNode(node3, node2);
        graph.AddNode(node4, node3);
        graph.AddNode(node5, node3);
        graph.AddNode(node6, node5);
        graph.AddNode(node7);
        graph.AddNode(node8);
        graph.AddEdge(node7, node8);
        graph.AddNode(node9, node8);
        graph.AddEdge(node9, node6);
        graph.AddNode(node10, node9);
        graph.AddNode(node11, node10);
        graph.AddNode(node12, node9);
        graph.AddNode(node13, node12);
        graph.AddNode(node14, node13);
        graph.AddNode(node15, node14);
        graph.AddNode(node16, node8);

        var r1 = new Node(new Vector3(0, 0, 0), "Room");
        var r2 = new Node(new Vector3(0, 0, 0), "Room");
        var r3 = new Node(new Vector3(0, 0, 0), "End");
        //var r4 = new Node(new Vector3(0, 0, 0), "Room");
        var leftHandSide = new List<Node>() { r1, r2, r3 };

        var canditateMatches = GraphRuleHelper.RuleSearch(leftHandSide, graph);

        foreach (var candidateChain in canditateMatches)
        {
            Debug.Log("####################");
            foreach (var candidateNode in candidateChain)
            {
                Debug.Log(candidateNode.nodeId);
            }

        }*/
    }
}
