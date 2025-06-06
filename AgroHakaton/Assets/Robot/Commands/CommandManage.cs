using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public List<RobotCommand> commands = new List<RobotCommand>();
    public Robot robot;
    public SoilGrid grid;
    public bool isExecuting;

    public UIManager uiManager; // ��������� ������

    public void AddCommand(RobotCommand command)
    {
        commands.Add(command);
    }

    public void RemoveCommand(int index)
    {
        commands.RemoveAt(index);
    }

    public void ClearCommands()
    {
        commands.Clear();
    }

    public IEnumerator ExecuteCommands()
    {
        isExecuting = true;

        for (int i = 0; i < commands.Count; i++)
        {
            if (!commands[i].Execute(robot, grid))
            {
                Debug.LogError($"������ ���������� ������� �� ���� {i}");
                break;
            }
            yield return new WaitForSeconds(0.5f); // ����� ��� ������������
        }

        isExecuting = false;

        // �������� ������
        if (grid.IsAllWatered())
        {
            Debug.Log("������! ��� ����� �������!");
        }
    }
}