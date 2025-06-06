using System.Collections.Generic;
using UnityEngine;

public abstract class RobotCommand : MonoBehaviour
{
    public abstract bool Execute(Robot robot, SoilGrid grid);
    public virtual void Simulate(Robot robot) { }
}

// Движение вперед
public class MoveForward : RobotCommand
{
    public override bool Execute(Robot robot, SoilGrid grid)
    {
        Vector2Int newPos = robot.GetNextPosition();
        if (!IsValidPosition(newPos, grid)) return false;

        robot.MoveTo(newPos);
        return true;
    }

    bool IsValidPosition(Vector2Int pos, SoilGrid grid)
    {
        return pos.x >= 0 &&
               pos.y >= 0 &&
               pos.x < grid.width &&
               pos.y < grid.height;
    }
}

public class MoveBackward : RobotCommand
{
    public override bool Execute(Robot robot, SoilGrid grid)
    {
        Vector2Int newPos = robot.GetNextPosition();
        if (!IsValidPosition(newPos, grid)) return false;

        robot.MoveTo(newPos);
        return true;
    }

    bool IsValidPosition(Vector2Int pos, SoilGrid grid)
    {
        return pos.x >= 0 &&
               pos.y >= 0 &&
               pos.x < grid.width &&
               pos.y < grid.height;
    }
}


// Поворот
public class TurnLeft : RobotCommand
{
    public override bool Execute(Robot robot, SoilGrid grid)
    {
        robot.Turn(-90);
        return true;
    }
}

public class TurnRight : RobotCommand
{
    public override bool Execute(Robot robot, SoilGrid grid)
    {
        robot.Turn(90);
        return true;
    }
}

// Орошение
public class WaterCommand : RobotCommand
{
    public override bool Execute(Robot robot, SoilGrid grid)
    {
        grid.grid[robot.position.x, robot.position.y].Water();
        return true;
    }
}

public class ForLoop : RobotCommand
{
    public int iterations;
    public List<RobotCommand> commands = new List<RobotCommand>();
    private UIManager uiManager;

    void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    public override bool Execute(Robot robot, SoilGrid grid)
    {
        for (int i = 0; i < iterations; i++)
        {
            foreach (var cmd in commands)
            {
                if (!cmd.Execute(robot, grid))
                    return false;
            }
        }
        return true;
    }
}