using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class PositionCalculation
{
    public static void CalculatePosition(List<CircleDetail> listCircle)
    {
        var rand = new Random();

        listCircle[0].posX = (float)rand.NextDouble() * GameManager.Resolution.x;
        listCircle[0].posY = (float)rand.NextDouble() * GameManager.Resolution.y;

        for (int i = 1; i < listCircle.Count; i++)
        {
            var cur = listCircle[i];
            var pre = listCircle[i - 1];
            var offset = (cur.time - pre.time) * 500;

            cur.posX = Mathf.Max(0, Mathf.Min(GameManager.Resolution.x, pre.posX + (float)(rand.NextDouble() * offset * 2 - offset)));
            cur.posY = Mathf.Max(0, Mathf.Min(GameManager.Resolution.y, pre.posY + (float)(rand.NextDouble() * offset * 2 - offset)));
        }
    }
}
