using UnityEngine;

public class SoilGrid : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public GameObject soilPrefab;
    public SoilBlock[,] grid;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new SoilBlock[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject soil = Instantiate(soilPrefab, new Vector3(x, y, 0), Quaternion.identity);
                grid[x, y] = soil.GetComponent<SoilBlock>();
            }
        }
    }

    public bool IsAllWatered()
    {
        foreach (var block in grid)
        {
            if (!block.isWatered) return false;
        }
        return true;
    }

    public int GetWateredCount()
    {
        int count = 0;
        foreach (var block in grid)
        {
            if (block.isWatered) count++;
        }
        return count;
    }
}