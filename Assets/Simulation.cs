using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    List<Point> points;
    List<Stick> sticks;

    public Color pointCol;
    public Color fixedPointCol;
    public float pointRadius;

    public bool simulating;
    bool drawingStick;
    int stickStartindex;
    public float gravity;
    // Start is called before the first frame update
    void Start()
    {
        if (points == null)
        {
            points = new List<Point>();
        }
        if (sticks == null)
        {
            sticks = new List<Stick>();
        }
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

    void LateUpdate(){
        Draw();
    }

    void Simulate()
    {
        foreach (Point p in points)
        {
            Vector2 prevpos = p.pos;
            p.pos += p.pos - p.posOld;
            p.pos += Vector2.down * gravity * Time.deltaTime;
        }
    }

    void HandleInput(Vector2 mousePos)
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
            bool mouseOverPoint = (MouseOverPointIndex(Input.mousePosition) != -1);
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
                    }
                }
                drawingStick = false;
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

        public Stick(Point pointA, Point pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
            length = (pointA.pos - pointB.pos).magnitude;
        }
    }
}
