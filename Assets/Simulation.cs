using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    protected List<Point> points;
    protected List<Stick> sticks;

    public Color pointCol;
    public Color fixedPointCol;
    public Color stickColor;
    public float pointRadius;
    public float stickThickness;
    public int numConstraints;
    public bool constrainStickMinLength = true;
    public bool autoStickMode;

    public bool simulating;
    bool drawingStick;
    int stickStartindex;
    public float gravity;
    int[] order;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (points == null)
        {
            points = new List<Point>();
        }
        if (sticks == null)
        {
            sticks = new List<Stick>();
        }
        CreateOrderArray();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        if (simulating)
        {
            Simulate();
        }
    }

    void LateUpdate()
    {
        Draw();
    }

    void Simulate()
    {
        foreach (Point p in points)
        {
            if (!p.isLocked)
            {
                Vector2 prevpos = p.pos;
                p.pos += p.pos - p.posOld;
                p.pos -= Vector2.down * gravity * Time.deltaTime;
                p.posOld = prevpos;
            }

        }

        for (int i = 0; i < numConstraints; i++)
        {
            
            for(int j = 0;j < sticks.Count;j++)
            {
                Stick stick = sticks[order[j]];
                Vector2 stickCenter = (stick.pointA.pos + stick.pointB.pos) / 2;
                Vector2 stickDir = (stick.pointA.pos - stick.pointB.pos).normalized;
                float length = (stick.pointA.pos - stick.pointB.pos).magnitude;
                float error = Mathf.Abs(length - stick.length);

                if(length > stick.length || constrainStickMinLength){
                    if(!stick.pointA.isLocked){
                        stick.pointA.pos = stickCenter + stickDir * stick.length / 2;
                    }
                    if(!stick.pointB.isLocked){
                        stick.pointB.pos = stickCenter - stickDir * stick.length / 2;
                    }
                }
            }
        }

    }

    protected virtual void HandleInput(Vector2 mousePos)
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            simulating = !simulating;
        }

        if (simulating)
        {

        }
        else
        {
            int i = MouseOverPointIndex(mousePos);
            bool mouseOverPoint = (i != -1);
            if (Input.GetMouseButtonDown(1) && mouseOverPoint)
            {
                points[i].isLocked = !points[i].isLocked;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (mouseOverPoint)
                {
                    drawingStick = true;
                    stickStartindex = i;
                }
                else
                {
                    points.Add(new Point() { pos = mousePos, posOld = mousePos });
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (mouseOverPoint && drawingStick)
                {
                    if (stickStartindex != i)
                    {
                        sticks.Add(new Stick(points[stickStartindex], points[i]));
                        CreateOrderArray();
                    }
                }
                drawingStick = false;
            }

            if(autoStickMode){
                sticks.Clear();
                for(int k = 0;k < points.Count;k++){
                    sticks.Add(new Stick(points[k], points[k+1]));
                    CreateOrderArray();
                }
            }

            if(Input.GetKeyDown(KeyCode.C)){
                points.Clear();
                sticks.Clear();
                CreateOrderArray();
            }
        }

    }

    int MouseOverPointIndex(Vector2 mousePos)
    {
        for (int i = 0; i < points.Count; i++)
        {
            float dst = (points[i].pos - mousePos).magnitude;
            if (dst < pointRadius)
            {
                return i;
            }
        }
        return -1;
    }

    void Draw()
    {
        foreach (Point point in points)
        {
            Visualizer.SetColour(point.isLocked ? fixedPointCol : pointCol);
            Visualizer.DrawSphere(point.pos, pointRadius);
        }
        Visualizer.SetColour(stickColor);

        foreach (Stick stick in sticks)
        {
            Visualizer.DrawLine(stick.pointA.pos, stick.pointB.pos, stickThickness);
        }

        if (drawingStick)
		{
			Visualizer.SetColour(stickColor);
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Visualizer.DrawLine(points[stickStartindex].pos, mousePos, stickThickness);
		}
    }

    public static T[] ShuffleArray<T>(T[] array, System.Random srng){
        int elementsRemainingToShuffle = array.Length;
        int randomIndex = 0;

        while(elementsRemainingToShuffle > 1){
            randomIndex = srng.Next(0, elementsRemainingToShuffle);
            T chosenElement = array[randomIndex];

            // Swap the randomly chosen element with the last unshuffled element in the array
            elementsRemainingToShuffle--;
            array[randomIndex] = array[elementsRemainingToShuffle];
            array[elementsRemainingToShuffle] = chosenElement;
        }
        return array;
    }

    protected void CreateOrderArray(){
        order = new int[sticks.Count];
        for(int i = 0;i < sticks.Count;i++){
            order[i] = i;
        }
        ShuffleArray(order, new System.Random());
    }

    public class Point
    {
        public Vector2 pos, posOld;
        public bool isLocked;
    }

    public class Stick
    {
        public Point pointA, pointB;
        public float length;
        public bool dead = false;

        public Stick(Point pointA, Point pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            length = (pointA.pos - pointB.pos).magnitude;
        }
    }
}
