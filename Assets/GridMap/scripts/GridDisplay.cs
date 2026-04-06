using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridDisplay : MonoBehaviour
{
    [SerializeField] private PlayerGrid player;
    private GridTest<HeatMapObject> grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new GridTest<HeatMapObject>(10, 5, 10f, new Vector3(-20, -10), (GridTest<HeatMapObject> g, int x, int y) => new HeatMapObject(g, x, y));
        Vector3 centerOffset = Vector3.one * (grid.GetCellSize() * 0.5f);
        player.transform.position = grid.GetWorldPosition(0, 0) + centerOffset;
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 vec = Camera.main.ScreenToWorldPoint(mousePos);
            int x, y;

            grid.GetXY(vec, out x, out y);

            Vector3 targetPos = grid.GetWorldPosition(x, y) + new Vector3(5f, 5f); // center of cell
            Vector3 currentPosition = grid.GetWorldPosition((int)player.transform.position.x, (int)player.transform.position.y);

            player.transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPos,
            5f * Time.deltaTime
        ); ;

            HeatMapObject heatMapObject = grid.GetGridObject(vec);

            Debug.Log(heatMapObject);

            if (heatMapObject != null)
            {
                heatMapObject.AddValue(2);
            }
        }
    }
}

public class HeatMapObject
{
    private const int MIN = 0;
    private const int MAX = 100;

    private GridTest<HeatMapObject> grid;
    private int x;
    private int y;

    private int value;


    public HeatMapObject(GridTest<HeatMapObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void AddValue(int addValue)
    {
        value += addValue;
        Mathf.Clamp(value, MIN, MAX);
        grid.TriggerObjectChanged(x, y);
    }

    public float GetValueNormalized()
    {
        return (float)value / MAX;
    }

    public override string ToString()
    {
        return value.ToString();
    }

}
