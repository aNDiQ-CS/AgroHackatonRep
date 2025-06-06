using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text commandText;
    [SerializeField] private Button deleteButton;

    public void Initialize(RobotCommand command, int index, System.Action onDelete)
    {
        // ������������� ����� � ����������� �� ���� �������
        string text = command switch
        {
            MoveForward => "������",
            MoveBackward => "�����",
            TurnLeft => "������� �����",
            TurnRight => "������� ������",
            WaterCommand => "�������",
            ForLoop loop => $"���� (��������� {loop.iterations} ���)",
            _ => "����������� �������"
        };

        commandText.text = $"{index + 1}. {text}";

        // ���������� ��������
        deleteButton.onClick.AddListener(() => onDelete?.Invoke());
    }
}