using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Command Buttons")]
    public Button moveForwardBtn;
    public Button moveBackBtn;
    public Button turnLeftBtn;
    public Button turnRightBtn;
    public Button waterBtn;
    public Button loopBtn;

    [Header("Control Buttons")]
    public Button executeBtn;
    public Button clearBtn;

    [Header("Program List")]
    public Transform commandListContainer;
    public GameObject commandListItemPrefab;

    [Header("Loop Panel")]
    public GameObject loopPanel;
    public TMP_InputField loopCountInput;
    public Button loopAddBtn;
    public Button loopCancelBtn;

    [Header("Status Display")]
    public TMP_Text positionText;
    public TMP_Text directionText;
    public TMP_Text statusText;

    private CommandManager commandManager;
    private Robot robot;
    private SoilGrid grid;

    private bool isAddingLoop = false;
    private ForLoop currentLoop;

    void Start()
    {
        commandManager = FindObjectOfType<CommandManager>();
        robot = FindObjectOfType<Robot>();
        grid = FindObjectOfType<SoilGrid>();

        SetupButtonListeners();
        loopPanel.SetActive(false);
    }

    void Update()
    {
        UpdateStatusDisplay();
    }

    void SetupButtonListeners()
    {
        // Основные команды
        moveForwardBtn.onClick.AddListener(() => AddCommand(new MoveForward()));
        moveBackBtn.onClick.AddListener(() => AddCommand(new MoveBackward()));
        turnLeftBtn.onClick.AddListener(() => AddCommand(new TurnLeft()));
        turnRightBtn.onClick.AddListener(() => AddCommand(new TurnRight()));
        waterBtn.onClick.AddListener(() => AddCommand(new WaterCommand()));

        // Управление
        executeBtn.onClick.AddListener(StartExecution);
        clearBtn.onClick.AddListener(ClearProgram);

        // Циклы
        loopBtn.onClick.AddListener(OpenLoopPanel);
        loopAddBtn.onClick.AddListener(AddLoopCommand);
        loopCancelBtn.onClick.AddListener(CloseLoopPanel);
    }

    void AddCommand(RobotCommand command)
    {
        if (isAddingLoop)
        {
            currentLoop.commands.Add(command);
        }
        else
        {
            commandManager.AddCommand(command);
        }
        UpdateCommandList();
    }

    void StartExecution()
    {
        if (!commandManager.isExecuting)
        {
            StartCoroutine(commandManager.ExecuteCommands());
        }
    }

    void ClearProgram()
    {
        commandManager.ClearCommands();
        isAddingLoop = false;
        currentLoop = null;
        UpdateCommandList();
    }

    void OpenLoopPanel()
    {
        loopPanel.SetActive(true);
    }

    void CloseLoopPanel()
    {
        loopPanel.SetActive(false);
    }

    void AddLoopCommand()
    {
        if (int.TryParse(loopCountInput.text, out int count) && count > 0)
        {
            currentLoop = new ForLoop { iterations = count };
            commandManager.AddCommand(currentLoop);
            isAddingLoop = true;
            loopPanel.SetActive(false);
            UpdateCommandList();
        }
        else
        {
            statusText.text = "Введите корректное число!";
        }
    }

    public void EndLoop()
    {
        isAddingLoop = false;
        currentLoop = null;
    }

    void UpdateCommandList()
    {
        // Очищаем текущий список
        foreach (Transform child in commandListContainer)
        {
            Destroy(child.gameObject);
        }

        // Добавляем все команды
        for (int i = 0; i < commandManager.commands.Count; i++)
        {
            RobotCommand cmd = commandManager.commands[i];
            GameObject item = Instantiate(commandListItemPrefab, commandListContainer);
            CommandListItem itemScript = item.GetComponent<CommandListItem>();

            itemScript.Initialize(
                command: cmd,
                index: i,
                onDelete: () => {
                    commandManager.RemoveCommand(i);
                    UpdateCommandList();
                }
            );
        }
    }

    void UpdateStatusDisplay()
    {
        positionText.text = $"Позиция: {robot.position.x}, {robot.position.y}";

        string direction = "";
        switch (robot.direction)
        {
            case 0: direction = "Вверх"; break;
            case 1: direction = "Вправо"; break;
            case 2: direction = "Вниз"; break;
            case 3: direction = "Влево"; break;
        }
        directionText.text = $"Направление: {direction}";

        if (grid.IsAllWatered())
        {
            statusText.text = "ПОБЕДА! Все почвы орошены!";
        }
        else if (commandManager.isExecuting)
        {
            statusText.text = "Выполнение программы...";
        }
        else
        {
            int watered = grid.GetWateredCount();
            int total = grid.width * grid.height;
            statusText.text = $"Орошено: {watered}/{total}";
        }
    }
}