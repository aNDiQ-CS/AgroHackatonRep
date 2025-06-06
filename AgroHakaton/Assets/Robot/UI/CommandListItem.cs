using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text commandText;
    [SerializeField] private Button deleteButton;

    public void Initialize(RobotCommand command, int index, System.Action onDelete)
    {
        // Устанавливаем текст в зависимости от типа команды
        string text = command switch
        {
            MoveForward => "Вперед",
            MoveBackward => "Назад",
            TurnLeft => "Поворот влево",
            TurnRight => "Поворот вправо",
            WaterCommand => "Оросить",
            ForLoop loop => $"Цикл (повторить {loop.iterations} раз)",
            _ => "Неизвестная команда"
        };

        commandText.text = $"{index + 1}. {text}";

        // Обработчик удаления
        deleteButton.onClick.AddListener(() => onDelete?.Invoke());
    }
}