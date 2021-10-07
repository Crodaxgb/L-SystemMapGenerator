using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class Graph_and_Grammer
{
    public enum EdgeType { DIRECTED, UNDIRECTED }
    public class Graph
    {

        private List<Node> nodeList;
        private Queue<int> nodeIdQueue;
        //The start index for the nodeIds
        private int nodeIdGen = 0;
        private EdgeType graphEdgeType;        

        public Graph(EdgeType graphEdgeType = EdgeType.UNDIRECTED)
        {
            NodeList = new List<Node>();
            this.graphEdgeType = graphEdgeType;
            nodeIdQueue = new Queue<int>();
            IncrementIdCounter();
        }
        public Graph(List<Node> nodeList, EdgeType graphEdgeType)
        {
            this.NodeList = nodeList;
            this.graphEdgeType = graphEdgeType;
            nodeIdQueue = new Queue<int>();
            IncrementIdCounter();
        }
        public List<Node> FindItem(string searchType)
        {
            return nodeList.FindAll(elements => elements.NodeType == searchType.ToLower());
        }
        public void AddNode(Node node)
        {
            node.nodeId = GenerateId();
            //Add the node to the list
            NodeList.Add(node);

        }
        public void AddNode(Vector3 worldPosition)
        {
            var node = new Node(worldPosition, "Room");
            node.nodeId = GenerateId();
            //Add the node to the list
            NodeList.Add(node);
        }
        public void AddNode(Node nodeToBeAdded, Node edgeConnectTo, float cost = default)
        {
            nodeToBeAdded.nodeId = GenerateId();
            //Add the node to the list
            NodeList.Add(nodeToBeAdded);
            //Assign its edges
            AddEdge(nodeToBeAdded, edgeConnectTo, cost);
        }
        public void AddNode(Vector3 nodeToBeAdded, string type1, Vector3 edgeConnectTo, string type2, float cost = 0.0f)
        {
            //Create and add the node to the list
            var node = new Node(nodeToBeAdded, type1);
            node.nodeId = GenerateId();
            NodeList.Add(node);
            var _to = new Node(edgeConnectTo, type1);
            //Assign its edges
            AddEdge(node, _to, cost);
        }
        public void AddNode(Node nodeToBeAdded, Node[] neighbors, float[] costs)
        {
            //Add the node first
            NodeList.Add(nodeToBeAdded);
            //then assign its neighbors by connecting their edges
            for(int arrayIndex = 0; arrayIndex < costs.Length; arrayIndex++)
            {
                AddEdge(nodeToBeAdded, neighbors[arrayIndex], costs[arrayIndex]);
            }
        }

        public void AddEdge(Node nodeItself, Node to, float cost = 0.0f)
        {
            if (graphEdgeType == EdgeType.DIRECTED)
            {
                nodeItself.Neighbors.Add(to);
                nodeItself.Costs.Add(cost);
            }
            else //That means it's an undirected one so both of the links needs to be filled
            {
                nodeItself.Neighbors.Add(to);
                nodeItself.Costs.Add(cost);
                //To node needs to be assigned with the from node
                to.Neighbors.Add(nodeItself);
                to.Costs.Add(cost);
            }
        }
        public void DeleteEdge(Node fromNode, Node toNode)
        {
            if (graphEdgeType == EdgeType.DIRECTED)
            {
                fromNode.Neighbors.Remove(toNode);
            }
            else //That means it's an undirected one so both of the links needs to be deleted
            {
                fromNode.Neighbors.Remove(toNode);
                toNode.Neighbors.Remove(fromNode);
            }
        }
        public void DeleteNode(Node nodeToBeDeleted, bool ensureConnectivitiy = false)
        {
            //ensure connectivity prevents the dangling nodes to appear
            if (graphEdgeType == EdgeType.UNDIRECTED)
            {
                //If the graph is undirected all of the neighbors must have a reference to that node
                //So iterate all of the neighbors and delete the references
                foreach (var neighbor in nodeToBeDeleted.Neighbors)
                {
                    
                    neighbor.Neighbors.Remove(nodeToBeDeleted);
                    //If the neigboring node remains with no neighbors after deletion
                    //Randomly connect it to another node
                    if(ensureConnectivitiy)
                    {
                        
                        //If the count reached to zero that means it has no neighbors
                        if(neighbor.Neighbors.Count == 0)
                        {
                            Debug.Log("Connectivity Provided");
                            Node randomNode;
                            //Ensure that it won't be the self and the node to be deleted
                            do
                            {
                                randomNode = this.nodeList.ElementAt(UnityEngine.Random.Range(0, nodeList.Count));
                            } while (randomNode == neighbor || randomNode == nodeToBeDeleted);
                                //Add an edge between those two
                            this.AddEdge(neighbor, randomNode);
                        }
                    }
                }
            }
            //But if it is directed we don't have a way of knowing the exact referencing nodes
            //So we need to iterate through all of the nodes in the graph
            else
            {
                //Iterate each node
                foreach (var eachNode in nodeList)
                {
                    //If a node's network, neighborhood contains that node, delete its reference
                    if (eachNode.Neighbors.Contains(nodeToBeDeleted))
                    {
                        eachNode.Neighbors.Remove(nodeToBeDeleted);

                        //If the neigboring node remains with no neighbors after deletion
                        //Randomly connect it to another node
                        if (ensureConnectivitiy)
                        {

                            //If the count reached to zero that means it has no neighbors
                            if (eachNode.Neighbors.Count == 0)
                            {
                                Node randomNode;
                                //Ensure that it won't be the self and the node to be deleted
                                do
                                {
                                    randomNode = this.nodeList.ElementAt(UnityEngine.Random.Range(0, nodeList.Count));
                                } while (randomNode == eachNode || randomNode == nodeToBeDeleted) ;
                                    //Add an edge between those two
                                this.AddEdge(eachNode, randomNode);
                            }
                        }
                    }
                }
            }
            //Return the id so it can be used for another node
            nodeIdQueue.Enqueue(nodeToBeDeleted.nodeId);       
            //Then finally remove the node from the graph
            nodeList.Remove(nodeToBeDeleted);
           
        }

        private int GenerateId()
        {

            if (nodeIdQueue.Count > 0)
            {
                return nodeIdQueue.Dequeue();
            }
            else
            {
                IncrementIdCounter();
                return nodeIdQueue.Dequeue();
            }
        }
        private void IncrementIdCounter()
        {
            nodeIdGen++;
            nodeIdQueue.Enqueue(nodeIdGen);
        }
        public List<Node> NodeList { get => nodeList; set => nodeList = value; }
    }
    public class Node
    {
        Vector3 worldPosition;
        internal int nodeId { get; set; }
        private string nodeType;
        HashSet<Node> neighbors;
        List<float> costs;
        Node expandedFrom;
        private bool isExpanded = false;
        public Node(string nodeType = null)
        {

            WorldPosition = new Vector3(0, 0, 0);
            nodeId = -1;
            NodeType = "";
            Neighbors = new HashSet<Node>();
            Costs = new List<float>();
            //Start listening to this event
            GraphRuleHelper.ResetEvent += ResetVisitState;        
            ExpandedFrom = null;
            this.nodeType = nodeType;
        }

        public Node(Vector3 worldPosition, string nodeType = "")
        {

            this.worldPosition = worldPosition;
            this.NodeType = nodeType.ToLower();            
            Neighbors = new HashSet<Node>();
            Costs = new List<float>();
            //Start listening to this event
            GraphRuleHelper.ResetEvent += ResetVisitState;            
            ExpandedFrom = null;
        }

        public Node(Vector3 worldPosition, string nodeType,HashSet<Node> neighbors)
                    : this(worldPosition, nodeType)
        {
            this.Neighbors = neighbors;
        }
                
        public bool IsExpanded { get => isExpanded; set => isExpanded = value; }
        public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }
        public HashSet<Node> Neighbors { get => neighbors; set => neighbors = value; }
        public List<float> Costs { get => costs; set => costs = value; }
        public Node ExpandedFrom { get => expandedFrom; set => expandedFrom = value; }
        public string NodeType { get => nodeType; set => nodeType = value; }
        public void ResetVisitState() { IsExpanded = false; ExpandedFrom = null; }
       
        

    }
    public static class GraphRuleHelper
    {
        public static event Action ResetEvent = delegate { };
        public static List<List<Node>> RuleSearch
            (List<Node> leftHandSide, Graph rightHandSide)
        {
           
            //Get the starting point for the rule match, candidates in the graph will be searched from the starting position
            var startingNode = leftHandSide.ElementAt(0);            
            //The nodes in the graph which has the same type with starting node of the rule
            var candidateStartNodes = new List<Node>();
            //The return value of the function will be stored in this list
            var candidateChains = new List<List<Node>>();
           
            //Get the points from the graph which fits with the initial type of the rule set.
            foreach (var graphNodes in rightHandSide.NodeList)
            {
                if(graphNodes.NodeType.Equals(startingNode.NodeType))
                {
                   
                    //If it matches the starting criteria append it to list
                    candidateStartNodes.Add(graphNodes);
                }
            }
            //If the rule has only one condition just return the values you found
            if(leftHandSide.Count == 1)
            {
                foreach(Node candidateStarters in candidateStartNodes)
                {
                    candidateChains.Add(new List<Node>() { candidateStarters });
                }

                return candidateChains;
            }

            
            int candidateIndex = 0;
            
            var expandedQueue = new Queue<Node>();
            while (candidateIndex < candidateStartNodes.Count)
            {
                
                //Get the node at the defined index and apply a DFS on it until it reaches the whole graph
                var ruleCandidate = candidateStartNodes.ElementAt(candidateIndex);
                
                expandedQueue.Enqueue(ruleCandidate);
                //This variable will indicate the addition of a matching neighbor
                
                do
                {
                    //Get the next rule from the queue
                    ruleCandidate = expandedQueue.Dequeue();
                    //Mark this node as expanded
                    ruleCandidate.IsExpanded = true;                  
                    //Start looking to its neighbors one by one append the fitting neighbours to the queue
                    for(int neighborIndex = 0; neighborIndex < ruleCandidate.Neighbors.Count; neighborIndex++)
                    {

                        //Get the next neighbor 
                        var neighborNode = ruleCandidate.Neighbors.ElementAt(neighborIndex);                       
                      
                        if (!expandedQueue.Contains(neighborNode) && !neighborNode.IsExpanded)
                        {
                            
                            //Mark as expanded
                            neighborNode.IsExpanded = true;
                            //Assign the expander node
                            neighborNode.ExpandedFrom = ruleCandidate;
                            expandedQueue.Enqueue(neighborNode);
                            //After each addition in the queue we send the added element and check if we
                            //can construct a rule with the latest addition                           
                            (bool isFound, var candidateNodes) = CheckIfRuleFormed(neighborNode, leftHandSide);
                            if(isFound)
                            {
                                //IF the candidate is not already found by the DFS algorithm
                                if(ContainsAllItems(candidateChains, candidateNodes))
                                {
                                    //Add it to the return value
                                    candidateChains.Add(candidateNodes);
                                }
                            }


                        }
                    }
                   
                } while (expandedQueue.Count != 0);
                //Reset all the node's states
                ResetEvent();
                //Take the next candidate for the evaluation
                candidateIndex++;
                             
                
            }
            return candidateChains;
        }

        private static (bool, List<Node>) CheckIfRuleFormed(Node currentNode, List<Node> leftHandSide)
        {
            //Generate a path until the first ancestor
            var path = new List<Node>();
            //assign a pointer starting from the current Node
            var pathTracer = currentNode;

            while(pathTracer != null)
            {
                //Append it to the path
                path.Add(pathTracer);
                //get its parent
                pathTracer = pathTracer.ExpandedFrom;
            }
            //Reverse the path because we started from the leaf nodes
            path.Reverse();
            bool flag = true;
            //If the formed graph is smaller dont even loop the nodes
            if(path.Count< leftHandSide.Count)
            {
                flag = false;
                return (flag, path);
            }
            
            //Check if a rule is formed
            for (int index = 0; index < leftHandSide.Count ; index++)
            {
                
                if(!path[index].NodeType.Equals(leftHandSide[index].NodeType))
                {                    
                    //If there is a missmatch just quit
                    flag = false;
                    break;
                }
            }

            return (flag, path.GetRange(0, leftHandSide.Count));
        }

        public static bool ContainsAllItems(List<List <Node> > nestedList, List<Node> innerList)
        {
            //for every element in the nested list       
            foreach(var element in nestedList)
            {
                //Check if the nested contains the inner in any way
                if (!element.Except(innerList).Any())
                {
                    return false;
                }
            }
            return true;
        }
        public static void CheckIfPathBroken(Graph graphToCheck)
        {
            //Get a pointer to ending and starting point in the graph
            Node startNode = null, endNode = null;
            //Search in the graph and get the start and end points
            foreach(Node graphNode in graphToCheck.NodeList)
            {
                if(graphNode.NodeType.Equals("start"))
                {
                    startNode = graphNode;
                }
                else if(graphNode.NodeType.Equals("end"))
                {
                    endNode = graphNode;
                }
            }
            //perform a breadth first searh starting from the start node
            BreadthFirstSearch(startNode);
            //Check if there's a path between start and end
            List<Node> pathFromStart = new List<Node>();
            //Start a pointer from the end node
            Node pointerNode = endNode;
            while(pointerNode != null)
            {
                pathFromStart.Add(pointerNode);
                pointerNode = pointerNode.ExpandedFrom;
            }
            //Reverse the path and check the first element
            pathFromStart.Reverse();
            //If the first element in the path is the start node that means the connection is valid
            if(pathFromStart[0] == startNode)
            {
                return;
            }
            //this time start from the start node and get the longest path
            pathFromStart = new List<Node>();
            pointerNode = startNode;
            while (pointerNode != null)
            {
                pathFromStart.Add(pointerNode);
                pointerNode = pointerNode.ExpandedFrom;
            }
            //If it is not connect it with the latest element in the end node
            //But for optimal path we need to perform another BFS starting from the end node
            //Reset all the states
            ResetEvent();
            BreadthFirstSearch(endNode);
            pointerNode = endNode;
            List<Node> pathFromEnd = new List<Node>();
            while (pointerNode != null)
            {
                pathFromEnd.Add(pointerNode);
                pointerNode = pointerNode.ExpandedFrom;
            }            
            //Connect the latest elements in both list
            graphToCheck.AddEdge(pathFromStart.ElementAt(pathFromStart.Count - 1), pathFromEnd.ElementAt(pathFromEnd.Count - 1));
            Debug.Log("Path Restored");
            //Reset the states of all the nodes
            ResetEvent();
        }
        public static void BreadthFirstSearch(Node startingPoint)
        {
            Queue<Node> expandedQueue = new Queue<Node>();
            //Enqueue the start node
            expandedQueue.Enqueue(startingPoint);
            //We'll perform a Breadth First Search algorithm to find if there is a path between start & end
            #region breadth first search
            do
            {
                //Get the next rule from the queue
                Node ruleCandidate = expandedQueue.Dequeue();
                //Mark this node as expanded
                ruleCandidate.IsExpanded = true;
                //Start looking to its neighbors one by one append the fitting neighbours to the queue
                for (int neighborIndex = 0; neighborIndex < ruleCandidate.Neighbors.Count; neighborIndex++)
                {

                    //Get the next neighbor 
                    var neighborNode = ruleCandidate.Neighbors.ElementAt(neighborIndex);
                    if (!expandedQueue.Contains(neighborNode) && !neighborNode.IsExpanded)
                    {
                        //Mark as expanded
                        neighborNode.IsExpanded = true;
                        //Assign the expander node
                        neighborNode.ExpandedFrom = ruleCandidate;
                        expandedQueue.Enqueue(neighborNode);
                        //After each addition in the queue we send the added element and check if we
                        //can construct a rule with the latest addition 
                    }
                }

            } while (expandedQueue.Count != 0);
            #endregion breadth first search
        }

    }
    public abstract class GraphRule
    {
        protected static GameObject tempTransform = new GameObject();
        private List<Node> leftHandSide;
        private Vector3 offSetForNodes;
        //All of the extenders will alter the graph by using this function
        public abstract void ApplyRule(Graph graphToAlter);
        //All of the extenders will populate their left hand side with this function call
        public abstract void PopulateRule();
        public GraphRule()
        {
            leftHandSide = new List<Node>();
            offSetForNodes = new Vector3(5, 0, 0);
            PopulateRule();
        }
        protected List<Node> RandomlySelectCandidate(Graph graphToAlter)
        {
            //Get all the possible combinations that matches the current rule
            List<List<Node>> candidateMatchNodes = GraphRuleHelper.RuleSearch(this.LeftHandSide, graphToAlter);
            if (candidateMatchNodes.Count == 0) return null; // failsafe
            //Decide on one candidate, randomly
            int randomIndex = UnityEngine.Random.Range(0, candidateMatchNodes.Count);
            List<Node> selectedSubset = candidateMatchNodes.ElementAt(randomIndex);
            return selectedSubset;
        }
        protected List<Node> LeftHandSide { get => leftHandSide; set => leftHandSide = value; }
        protected Vector3 OffSetForNodes { get => offSetForNodes; set => offSetForNodes = value; }
    }

    public class StartRule : GraphRule
    {
        private int initialRoomCount;
        public StartRule() : base()
        {
            initialRoomCount = 10;
        }
        public override void ApplyRule(Graph graphToAlter)
        {            
            //There must be only a one start node so this notation should be safe
            Node levelNode = GraphRuleHelper.RuleSearch(LeftHandSide, graphToAlter)[0][0];
            //Delete the level node it'll be generated with start and and nodes
            graphToAlter.DeleteNode(levelNode);
            //generate a start node
            Node startNode = new Node(worldPosition:Vector3.zero, nodeType:"Start");
            //Add it to the graph
            graphToAlter.AddNode(startNode);           
            //Set two pointers so we can iterate one by one
            Node pointerNode = startNode, previousNode = startNode;
            //Initialize a specific number of room nodes and an and node for the level
            for(int roomCount = 0; roomCount < initialRoomCount; roomCount++)
            {
                //assign and point to the next node
                pointerNode = new Node(pointerNode.WorldPosition + OffSetForNodes, nodeType:"Room");
                graphToAlter.AddNode(pointerNode, previousNode);
                previousNode = pointerNode;
            }
            //Lastly add the end room node
            pointerNode = new Node(pointerNode.WorldPosition + OffSetForNodes, nodeType: "End");
            graphToAlter.AddNode(pointerNode, previousNode);
        }

        public override void PopulateRule()
        {
            Node startNode = new Node(worldPosition: Vector3.zero,nodeType: "Level");
            LeftHandSide.Add(startNode);
        }
    }

    public class VariateRule : GraphRule
    {
        float variateRange;
        public VariateRule() : base()
        {
            variateRange = 10f;
        }
        public override void ApplyRule(Graph graphToAlter)
        {
            //Randomly select a candidate
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe
            //The selected nodes are coming in the same order with the rule set
            //So the first element in the rule table corresponds to the first element in selectedSubset etc.
            //Variate the position of the middle element
            selectedSubset[1].WorldPosition += new Vector3(UnityEngine.Random.Range(-variateRange / 4, variateRange / 4), UnityEngine.Random.Range(-variateRange, variateRange), 0);
        }

        public override void PopulateRule()
        {
            //Create a rule that'll search for a 3 room node
            for(int ruleIndex = 0; ruleIndex < 3; ruleIndex++)
            {
                LeftHandSide.Add(new Node(worldPosition:Vector3.zero, nodeType:"Room"));
            }
        }
    }
    public class AddNodeBetweenRule : GraphRule
    {
        //Generate a temp game object so we can use the inversetransformpoint on it
        //It is static so we only generating it once and the all add node instances are sharing it
        //This temp will act like an object with the nodes position
        
        float spreadMultiplier, addThreshold;
        int loopCounter;
        public AddNodeBetweenRule() : base()
        {
            addThreshold = 1.5f;
            spreadMultiplier = 1f;
        }

        public override void ApplyRule(Graph graphToAlter)
        {                        
            loopCounter = 0;
            bool tooClose = false;
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe
            Vector3 middlePoint = Vector3.zero;
            while (!tooClose && loopCounter < 20)
            {

                selectedSubset = RandomlySelectCandidate(graphToAlter);
                //Calculate the middle point between two found nodes, because the new node
                //Will be inserted between them
                middlePoint = (selectedSubset[0].WorldPosition + selectedSubset[1].WorldPosition) / 2;

                //Look for each node position in the graph if the new node to be added is too close to anyone
                //Select a new candidate            

                foreach (Node graphNode in graphToAlter.NodeList)
                {
                    if (Vector3.Distance(graphNode.WorldPosition, middlePoint) < addThreshold)
                    {
                        tooClose = true;
                        break;
                    }
                }
                //If a node is too close close the bool key so the search can continue
                //But increment the counter so it won't end up in a infinite loop
                if (tooClose)
                {
                    tooClose = false;
                    loopCounter++;
                }
                else
                {
                    break;
                }

            }
            //If you couldn't find a great place to add just return.
            if(loopCounter >= 20 && !tooClose)
            {                
                return;
            }
         
            //Generate a new node with type of room
            Node middleRoomNode = new Node(worldPosition:middlePoint, nodeType:"Room");          
            
            //Spread out every node in the graph
            foreach(Node graphNode in graphToAlter.NodeList)
            {
                tempTransform.transform.position = graphNode.WorldPosition;
                Vector3 relativePosition = tempTransform.transform.InverseTransformPoint(middlePoint).normalized;
                graphNode.WorldPosition -= relativePosition * spreadMultiplier;
            }      
            //Arrange the edges and append this node to the graph structure
            //Connect it to the left node
            graphToAlter.AddNode(middleRoomNode, selectedSubset[0]);
            //Connect it to the right node
            graphToAlter.AddEdge(middleRoomNode, selectedSubset[1]);
        }

        public override void PopulateRule()
        {
            //Create a rule that'll search for a 2 room node, the node will be added between the nodes
            for (int ruleIndex = 0; ruleIndex < 2; ruleIndex++)
            {
                LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: "Room"));
            }
        }
    }
    public class AddNodeRule : GraphRule
    {
        protected string searchType;
        public AddNodeRule(string searchType) : base()
        {
            this.searchType = searchType;
            PopulateRule();
        }
        public override void ApplyRule(Graph graphToAlter)
        {
            float minRange = -2, maxRange = 2;
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe
            //generate a new node just above the found one
            Vector3 randomPosition = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-10f, 10f), 0);

            //Ensure a random position outside of the too close region
            if(randomPosition.x > minRange && randomPosition.x <= 0)
            {
                randomPosition.x += minRange;
            }
            else if(randomPosition.x < maxRange && randomPosition.x >=0 )
            {
                randomPosition.x += maxRange;
            }

            if (randomPosition.y > minRange && randomPosition.y <= 0)
            {
                randomPosition.y += minRange;
            }
            else if (randomPosition.y < maxRange && randomPosition.y >= 0)
            {
                randomPosition.y += maxRange;
            }
            randomPosition += selectedSubset[0].WorldPosition;
            Node newNode = new Node(worldPosition: randomPosition, nodeType:"Room");
            //add it to the graph
            graphToAlter.AddNode(newNode, selectedSubset[0]);
        }

        public override void PopulateRule()
        {
            if (searchType == null) return;//failsafe
            LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: searchType));
        }
    }

    public class DisjointRule : GraphRule
    {
        private float middleScaler;
        public DisjointRule() : base()
        {
            middleScaler = 5f;
        }
        public override void ApplyRule(Graph graphToAlter)
        {
            //Randomly select a candidate
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe
            //The selected four nodes will form a diamond like shape
            //Assigning pointer to each of those nodes in the graph, we'll use the pointers in order to
            //Arrange their neighborhood
            Node node1 = selectedSubset[0];
            Node node2 = selectedSubset[1];
            Node node3 = selectedSubset[2];
            Node node4 = selectedSubset[3];

            //delete the link between 2 and 3 so 2 won't be pointing the 3 and vice versa
            graphToAlter.DeleteEdge(node2, node3);
            //Add an edge between 2 and 4
            graphToAlter.AddEdge(node2, node4);
            //Add an edge between 1 and 3
            graphToAlter.AddEdge(node1, node3);
            //Extract a vector that points from node 1 to node 4
            Vector3 targetDirection = (node4.WorldPosition - node1.WorldPosition).normalized;
            //Take a vector which is perpendicular to target direction, so the generated cross product
            //Will be on the same plane with middle point            
            Vector3 perpDir = Vector3.Cross(targetDirection, Vector3.forward);
            //Calculate the middle point between node 1 & 4
            Vector3 midPoint = (node1.WorldPosition + node4.WorldPosition) / 2;
            //Generate an offset that is perpendicular to the middle point plane
            Vector3 offSetNode2 = midPoint + (perpDir * - middleScaler);
            Vector3 offSetNode3 = midPoint + (perpDir *  middleScaler);
            //assign node 2 to upper part
            node2.WorldPosition = offSetNode2;
            //assign node 3 to lower part
            node3.WorldPosition = offSetNode3;
        }

        public override void PopulateRule()
        {
            //Create a rule that'll search for a 4 room node, 
            //those rooms will be turned into disjoints
            for (int ruleIndex = 0; ruleIndex < 4; ruleIndex++)
            {
                LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: "Room"));
            }
        }
    }
    public class DeleteNodesRule : GraphRule
    {
        private int deletionCount = 4;
        private float clusterMultiplier;
        public DeleteNodesRule() : base(){ clusterMultiplier = 7.5f; }
        //In order to prevent the overpopulation we need to delete some of the nodes at some probability
        //deletion count - 2 will be deleted
        public override void ApplyRule(Graph graphToAlter)
        {
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe            
            //We need to delete the intermediate values because the first and the last node will
            //stay on the graph in order to prevent the dangling edges, so we need the last and the first
            //node of the search result
            Node first = selectedSubset.ElementAt(0);
            Node last = selectedSubset.ElementAt(selectedSubset.Count - 1);
            Vector3 middlePoint = Vector3.zero;
            //All the intermediate nodes can be deleted with the built in graph function            
            for(int deletionIndex=1; deletionIndex < deletionCount - 1; deletionIndex++)
            {
                //Calculate the middle point
                middlePoint += selectedSubset.ElementAt(deletionIndex).WorldPosition;
                graphToAlter.DeleteNode(selectedSubset.ElementAt(deletionIndex), ensureConnectivitiy:true);
            }
            //Calculate the average midd point
            middlePoint /= (deletionCount - 2);
            //be sure to add an edge between the first and the last
            //Written  graph library prevents duplicate edges so it can be done without worrying of duplicates
            graphToAlter.AddEdge(first, last);
            //Advance the first and last to the middle point of the deletion
            //First node
            tempTransform.transform.position = first.WorldPosition;
            Vector3 relativePosition = tempTransform.transform.InverseTransformPoint(middlePoint).normalized;
            first.WorldPosition += relativePosition * clusterMultiplier;
            //last node
            tempTransform.transform.position = last.WorldPosition;
            relativePosition = tempTransform.transform.InverseTransformPoint(middlePoint).normalized;
            last.WorldPosition += relativePosition * clusterMultiplier;
        }

        public override void PopulateRule()
        {
            for (int ruleIndex = 0; ruleIndex < deletionCount; ruleIndex++)
            {             
                LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: "Room"));
            }
        }
    }
    public class EnemyRule : GraphRule
    {
        protected int enemyNodeCount;
        protected string generatedNodeType;
        public EnemyRule(int enemyNodeCount = 4, string generatedNodeType="Enemy") : base()
        {
            this.enemyNodeCount = enemyNodeCount;
            this.generatedNodeType = generatedNodeType;
            PopulateRule();
        }
        //In order to prevent the room overpopulation and to provide the game with challanges
        //Enemies will be generated from this rule
        public override void ApplyRule(Graph graphToAlter)
        {
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe    
            //This rule is also created with the intentions of clearing some connections
            //So this variable will hold the minimum desired connection amount
            int connectionAmount = enemyNodeCount / 2;
            //All of the neighborhood information of the room nodes will be held in this hashset
            HashSet<Node> neighborHoodInformation = new HashSet<Node>();
            //for each node in the candidate chain
            foreach(Node chainNodes in selectedSubset)
            {
                //For each of their neighbors
                foreach(Node neighboringNode in chainNodes.Neighbors)
                {
                    neighborHoodInformation.Add(neighboringNode);
                }
                
            }
            //The middle point of the chain
            Vector3 middlePoint = Vector3.zero;
            //After that all the nodes can be deleted in the chain by ensuring the connectivity issue
            for (int deletionIndex = 0; deletionIndex < enemyNodeCount; deletionIndex++)
            {
                //Calculate the middle point
                middlePoint += selectedSubset.ElementAt(deletionIndex).WorldPosition;
                //Remove the neighbors that are being in the remove queue, we don't want any dead connections
                neighborHoodInformation.Remove(selectedSubset.ElementAt(deletionIndex));
                graphToAlter.DeleteNode(selectedSubset.ElementAt(deletionIndex), ensureConnectivitiy: true);
            }
            //Check if a path is broken
            GraphRuleHelper.CheckIfPathBroken(graphToAlter);
            if (neighborHoodInformation.Count < connectionAmount) return; //failsafe
            //Taking the average point to calculate the middle
            middlePoint /= enemyNodeCount;
            //Creating a new enemy node at that location
            Node enemyNode = new Node(worldPosition: middlePoint, nodeType: generatedNodeType);
            //Append it to the graph
            graphToAlter.AddNode(enemyNode);
            //Create connectionAmount of connections
            for(int randomIndex = 0; randomIndex < connectionAmount; randomIndex++)
            {
                Node randomNode = neighborHoodInformation.ElementAt(UnityEngine.Random.Range(0, neighborHoodInformation.Count));
                //Continue the selection process until the random node from the neigbhorhood
                graphToAlter.AddEdge(enemyNode, randomNode);
                //Ensure that it wont be selected again
                neighborHoodInformation.Remove(randomNode);
            }

        }

        public override void PopulateRule()
        {           
            for (int ruleIndex = 0; ruleIndex < enemyNodeCount; ruleIndex++)
            {
                LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: "Room"));
            }
        }

        
    }
    //If there are multiple enemies convert them into a boss
    //Actually the logic will be similar to the enemy logic so it can extend from it
    public class BossRule : EnemyRule
    {
        public BossRule(int enemyNodeCount = 3, string generatedNodeType = "Boss") : base(enemyNodeCount, generatedNodeType)
        {
            this.enemyNodeCount = enemyNodeCount;
            this.generatedNodeType = generatedNodeType;
            
        }
        public override void PopulateRule()
        {           
            for (int ruleIndex = 0; ruleIndex < enemyNodeCount; ruleIndex++)
            {               
                LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: "Enemy"));
            }
           
        }
    }

    public class SupplierRule : GraphRule
    {
        protected List<string> ruleList;
        public SupplierRule(List<string> ruleList) : base()
        {
            this.ruleList = ruleList;
            PopulateRule();
        }
        public override void ApplyRule(Graph graphToAlter)
        {
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe    
            //Just convert a room's type into a supplier type if the match is found
            foreach(Node candidateNodes in selectedSubset)
            {
                if(candidateNodes.NodeType.Equals("room"))
                {
                    candidateNodes.NodeType = "supplier";
                }
            }
        }

        public override void PopulateRule()
        {
            //If its not initialized just return
            if (ruleList == null)
            { 
                return;
            }
            foreach(string supplierElement in ruleList)
            {
                LeftHandSide.Add(new Node(worldPosition: Vector3.zero, nodeType: supplierElement));
            }
        }
    }
    public class HookToRandom : GraphRule
    {
        //It searches for a parameterized node type
        //If the found node has minimal connections, the apply method generates a new edge
        protected string searchString;
        protected int minimalConnectionAmount, maxConnectionAmount;
        public HookToRandom(string searchString, int minimalConnectionAmount, int maxConnectionAmount) : base()
        {
            this.searchString = searchString;
            this.minimalConnectionAmount = minimalConnectionAmount;
            this.maxConnectionAmount = maxConnectionAmount;
            PopulateRule();
        }
        public override void ApplyRule(Graph graphToAlter)
        {
            List<Node> selectedSubset = RandomlySelectCandidate(graphToAlter);
            if (selectedSubset == null) return; // failsafe    
            Node searchedNode = selectedSubset[0];
            Node randomNode;
            if(searchedNode.Neighbors.Count <= minimalConnectionAmount)
            {
                do
                {
                    //Select a random node but ensure that it is not the searched node
                    //and not its one of neighbors
                    randomNode = graphToAlter.NodeList.ElementAt(UnityEngine.Random.Range(0, graphToAlter.NodeList.Count));
                } while (randomNode == searchedNode || searchedNode.Neighbors.Contains(randomNode));
                graphToAlter.AddEdge(searchedNode, randomNode);
            }
            else if(searchedNode.Neighbors.Count >= maxConnectionAmount)
            {
                Node randomToBeDeleted;
                int failSafeCounter = 0;
                do
                {
                    //Select an edge to be deleted from the neighbors of the resultant search
                    randomToBeDeleted = searchedNode.Neighbors.ElementAt(UnityEngine.Random.Range(0, searchedNode.Neighbors.Count));
                    //It may not fit the criteria so a failsafe counter is needed
                    failSafeCounter++;
                } while ((randomToBeDeleted.Neighbors.Count <= 2 || randomToBeDeleted == searchedNode) && failSafeCounter < 20);
                //If a node like that is found in the provided criterias
                if(failSafeCounter < 20)
                {
                    graphToAlter.DeleteEdge(searchedNode, randomToBeDeleted);
                }
                else
                {
                    return;
                }
                 
                do
                {
                    //Select a random node but ensure that it is not the searched node
                    //and not its one of neighbors
                    randomNode = graphToAlter.NodeList.ElementAt(UnityEngine.Random.Range(0, graphToAlter.NodeList.Count));
                } while (randomNode == searchedNode || searchedNode.Neighbors.Contains(randomNode));
                graphToAlter.AddEdge(searchedNode, randomNode);
            }
        }

        public override void PopulateRule()
        {
            if (searchString == null) return;
            LeftHandSide.Add(new Node(worldPosition:Vector3.zero, nodeType:searchString));
        }
    }

}
