/*****************************************************************
* COMPONENT OF: Graybox scene builder (Editor only)
* REQUIRED DEPENDENCIES: UnityEngine vector types
* DESCRIPTION: Generates a repeatable connected maze and finds its entrance-to-exit route.
* VERSION 1.0: Adds a 5-by-5 maze with branching dead ends and a verified long solution.
*****************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class PrototypeMazeLayout
{
    public const int Columns = 5;
    public const int Rows = 5;
    public const float CellWidth = 4f;
    public const float CellDepth = 3.6f;

    // Directions are east, north, west, south; opposite directions differ by two indices.
    private static readonly Vector2Int[] Directions =
        { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };
    private readonly int[,] passages = new int[Columns, Rows];
    public List<Vector2Int> Solution { get; private set; }

    public PrototypeMazeLayout()
    {
        // Fixed seeds keep every scene rebuild identical; reject layouts with a short main route.
        for (int seed = 100; seed < 300; seed++)
        {
            System.Array.Clear(passages, 0, passages.Length);
            CarvePassages(new System.Random(seed));
            Solution = FindSolution();
            if (Solution.Count >= 17 && CountDeadEnds() >= 4)
            {
                return;
            }
        }
        throw new System.InvalidOperationException("Could not generate the required connected maze.");
    }

    // Expose the connectivity separately from wall geometry so it can be checked before building.
    public bool IsOpen(Vector2Int cell, int direction)
    {
        return (passages[cell.x, cell.y] & (1 << direction)) != 0;
    }

    public static Vector3 CellCenter(Vector2Int cell)
    {
        return new Vector3(-33f + cell.x * CellWidth, 1f, -7.2f + cell.y * CellDepth);
    }

    private void CarvePassages(System.Random random)
    {
        bool[,] visited = new bool[Columns, Rows];
        Stack<Vector2Int> path = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(0, 2);
        visited[current.x, current.y] = true;
        path.Push(current);
        List<int> choices = new List<int>(4);
        while (path.Count > 0)
        {
            current = path.Peek();
            choices.Clear();
            for (int direction = 0; direction < 4; direction++)
            {
                Vector2Int neighbor = current + Directions[direction];
                if (IsInside(neighbor) && !visited[neighbor.x, neighbor.y])
                {
                    choices.Add(direction);
                }
            }
            if (choices.Count == 0)
            {
                // Return to the last junction when the current corridor reaches a dead end.
                path.Pop();
                continue;
            }
            int chosen = choices[random.Next(choices.Count)];
            Vector2Int next = current + Directions[chosen];
            // Open both faces of the same passage so travel works in either direction.
            passages[current.x, current.y] |= 1 << chosen;
            passages[next.x, next.y] |= 1 << ((chosen + 2) % 4);
            visited[next.x, next.y] = true;
            path.Push(next);
        }
    }

    private List<Vector2Int> FindSolution()
    {
        Vector2Int entrance = new Vector2Int(0, 2);
        Vector2Int exit = new Vector2Int(4, 2);
        Dictionary<Vector2Int, Vector2Int> previous = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        previous[entrance] = entrance;
        frontier.Enqueue(entrance);
        while (frontier.Count > 0)
        {
            Vector2Int cell = frontier.Dequeue();
            for (int direction = 0; direction < 4; direction++)
            {
                Vector2Int next = cell + Directions[direction];
                if (IsOpen(cell, direction) && !previous.ContainsKey(next))
                {
                    previous[next] = cell;
                    frontier.Enqueue(next);
                }
            }
        }
        List<Vector2Int> solution = new List<Vector2Int>();
        for (Vector2Int cell = exit; cell != entrance; cell = previous[cell])
        {
            solution.Add(cell);
        }
        solution.Add(entrance);
        solution.Reverse();
        return solution;
    }

    private int CountDeadEnds()
    {
        int count = 0;
        foreach (int exits in passages)
        {
            // A power of two contains one set bit, meaning this cell has only one passage.
            if (exits != 0 && (exits & (exits - 1)) == 0)
            {
                count++;
            }
        }
        return count;
    }

    private static bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Columns && cell.y >= 0 && cell.y < Rows;
    }
}
