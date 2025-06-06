using UnityEngine;

public class Robot : MonoBehaviour
{
    public Vector2Int position;
    public int direction; // 0: вверх, 1: вправо, 2: вниз, 3: влево

    public void MoveTo(Vector2Int newPos)
    {
        position = newPos;
        transform.position = new Vector3(newPos.x, 0.5f, newPos.y);
    }

    public void Turn(int angle)
    {
        direction = (direction + (angle / 90) + 4) % 4;
        transform.Rotate(0, angle, 0);
    }

    public Vector2Int GetNextPosition()
    {
        return position + new Vector2Int(
            direction == 1 ? 1 : direction == 3 ? -1 : 0,
            direction == 0 ? 1 : direction == 2 ? -1 : 0
        );
    }
}