using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        squareGrid = new SquareGrid(map, squareSize);
    }
    private void OnDrawGizmos()
    {
        if(squareGrid!=null)
        {
            for (int rowIterator = 0; rowIterator < squareGrid.squares.GetLength(0); rowIterator++)
            {
                for (int columnIterator = 0; columnIterator < squareGrid.squares.GetLength(1); columnIterator++)
                {
                    Gizmos.color = (squareGrid.squares[rowIterator, columnIterator].topLeft.active) ? Color.black: Color.white;
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].topLeft.position, 
                        Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[rowIterator, columnIterator].topRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].topRight.position,
                        Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[rowIterator, columnIterator].bottomRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].bottomRight.position,
                        Vector3.one * 0.4f);

                    Gizmos.color = (squareGrid.squares[rowIterator, columnIterator].bottomLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].bottomLeft.position,
                        Vector3.one * 0.4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].centerTop.position,
                        Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].centerRight.position,
                        Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].centerBottom.position,
                        Vector3.one * 0.15f);
                    Gizmos.DrawCube(squareGrid.squares[rowIterator, columnIterator].centerLeft.position,
                        Vector3.one * 0.15f);
                }
            }

        }
    }
    public class SquareGrid
    {

        public Square[,] squares;
        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountOnX = map.GetLength(0);
            int nodeCountOnY = map.GetLength(1);

            float mapWidth = nodeCountOnX * squareSize;
            float mapHeight = nodeCountOnY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountOnX, nodeCountOnY];

            for(int rowIterator = 0; rowIterator<nodeCountOnX;rowIterator++)
            {
                for (int columnIterator = 0; columnIterator < nodeCountOnY; columnIterator++)
                {
                    //Going z by z. Each position is incrementing in z axis first then the x increments.
                   
                    Vector3 position = new Vector3( -mapWidth / 2 + rowIterator * squareSize + squareSize/2,
                        0,-mapHeight / 2 + columnIterator * squareSize + squareSize / 2);

                    controlNodes[rowIterator, columnIterator] = new ControlNode(position, 
                        map[rowIterator, columnIterator] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountOnX - 1, nodeCountOnY - 1];

            for (int rowIterator = 0; rowIterator < nodeCountOnX - 1; rowIterator++)
            {
                for (int columnIterator = 0; columnIterator < nodeCountOnY - 1; columnIterator++)
                {
                    //Assignment done according to the axes. Z is pointing upwards so, upper means higher z value
                    //and that means column iterator + n
                    squares[rowIterator, columnIterator] = new Square(
                        controlNodes[rowIterator, columnIterator + 1],
                        controlNodes[rowIterator + 1, columnIterator + 1],
                        controlNodes[rowIterator + 1, columnIterator],
                        controlNodes[rowIterator, columnIterator]
                        );
                }
            }
        }   
            
    }

    

    public class Square
    {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;

            centerTop = topLeft.right;
            centerRight = bottomRight.above;
            centerBottom = bottomLeft.right;
            centerLeft = bottomLeft.above;
        }
    }


    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;
        public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            this.active = active;
            above = new Node(this.position + Vector3.forward * squareSize / 2f);
            right = new Node(this.position + Vector3.right * squareSize / 2f);

        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 position)
        {
            this.position = position;
        }

    }

}
