using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class LSystemEngine
{
    public struct TurtleState
    {   public Vector2 position;
        public float Headings;
        public int Depth;
    }

    public static string Expand(LSystemData data, int iterations)
    { Dictionary<char, string> rules = new Dictionary<char, string>();

        foreach (LSystemRule Rule in data.rules)


        {
            if (!string.IsNullOrEmpty(Rule.symbol))

            {
                rules[Rule.symbol[0]] = Rule.replacement;

            }

        }

        string result = data.axiom;
        for (int i = 0; i < iterations; i++)
        { StringBuilder NewString = new StringBuilder();

            foreach (char c in result)
            {
                if (rules.ContainsKey(c))
                 { NewString . Append( rules [ c] ); }
                else
                {
                     NewString. Append( c);
                }
            } result = NewString.ToString();
        }
        return result;
    }
    public static int BuildLines(
        string Text,
        float angle,
        float STEP,
        float startHeading,
        Mesh mesh)
    {   List<Vector3> vertices = new List<Vector3>();
        Stack<TurtleState> states = new Stack<TurtleState>();

        TurtleState TURTLE = new TurtleState();

        TURTLE.position = Vector2.zero;
        TURTLE.Headings = startHeading;
        TURTLE.Depth = 0;

        foreach (char c in Text)
        { if (c == 'F' || c == 'G')
            {
                Vector2 nextPosition = MoveForward(
                     TURTLE. position,TURTLE .Headings,STEP);vertices.Add(TURTLE.position);vertices.Add(nextPosition);TURTLE.position = nextPosition;}
            else if (c == 'f')
            {TURTLE.position = MoveForward( TURTLE. position,TURTLE.Headings, STEP );}
            else if (c == '+')
            { TURTLE.Headings += angle;}
            else if (c == '-')
            { TURTLE.Headings -= angle;}
            else if (c == '[')
            { states.Push(TURTLE); TURTLE.Depth++; }
            else if (c == ']')
            {if (states.Count > 0)  {  TURTLE = states.Pop(); } }
        }

        mesh.Clear();

        if (vertices.Count == 0)
        {return 0;}

        int[] indices = new int[vertices.Count];

        for (int i = 0; i < indices.Length; i++)
        { indices[i] = i; }

        mesh.SetVertices(vertices);
        mesh.SetIndices(indices, MeshTopology.Lines,0);

        mesh.RecalculateBounds();

        return vertices.Count / 2;
    }

    private static Vector2 MoveForward(
        Vector2 position,float heading,float distance)
    {   float radians = heading * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(-Mathf.Sin(radians),Mathf.Cos(radians));
        return position + direction * distance;
    }
}