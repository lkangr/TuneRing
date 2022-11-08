using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class PositionCalculation
{
    public static float adjustFactor = 2f;

    public static void CalculatePosition(List<CircleDetail> listCircle)
    {
        var rand = new Random();

        listCircle[0].posX = (float)rand.NextDouble() * GameManager.Resolution.x;
        listCircle[0].posY = (float)rand.NextDouble() * GameManager.Resolution.y;

        Vector2 offset = Vector2.zero;

        float temp, powTemp;

        int color = 0, numDis = 2;

        for (int i = 1; i < listCircle.Count; i++)
        {
            var cur = listCircle[i];
            var pre = listCircle[i - 1];
            var deltaTime = (cur.time - pre.time);

            Vector2 dur = new Vector2(deltaTime * GameManager.Resolution.x, deltaTime * GameManager.Resolution.y);

            var offsetX = new Vector2(Mathf.Min(0, Mathf.Max(-dur.x, offset.x - dur.x)), Mathf.Max(0, Mathf.Min(dur.x, offset.x + dur.x)));
            var offsetY = new Vector2(Mathf.Min(0, Mathf.Max(-dur.y, offset.y - dur.y)), Mathf.Max(0, Mathf.Min(dur.y, offset.y + dur.y)));

            temp = (float)rand.NextDouble();
            if (offset.x == 0) powTemp = temp;
            else if (offset.x > 0) powTemp = Mathf.Pow(temp, adjustFactor);
            else powTemp = 1 - Mathf.Pow(temp, adjustFactor);
            var tempX = offsetX.x + (offset.x > 0 ? powTemp : 1 - powTemp) * (offsetX.y - offsetX.x);
            tempX = Mathf.Min(GameManager.Resolution.x, Mathf.Max(-GameManager.Resolution.x, tempX));

            temp = (float)rand.NextDouble();
            if (offset.y == 0) powTemp = temp;
            else if (offset.y > 0) powTemp = Mathf.Pow(temp, adjustFactor);
            else powTemp = 1 - Mathf.Pow(temp, adjustFactor);
            var tempY = offsetY.x + (offset.y > 0 ? powTemp : 1 - powTemp) * (offsetY.y - offsetY.x);
            tempY = Mathf.Min(GameManager.Resolution.y, Mathf.Max(-GameManager.Resolution.y, tempY));

            cur.posX = pre.posX + tempX;
            if (cur.posX < 0) cur.posX = -cur.posX;
            else if (cur.posX > GameManager.Resolution.x) cur.posX -= 2 * (cur.posX - GameManager.Resolution.x);

            cur.posY = pre.posY + tempY;
            if (cur.posY < 0) cur.posY = -cur.posY;
            else if (cur.posY > GameManager.Resolution.y) cur.posY -= 2 * (cur.posY - GameManager.Resolution.y);

            offset = new Vector2(cur.posX - pre.posX, cur.posY - pre.posY);

            cur.color = color;
            cur.numDis = numDis;

            numDis++;
            if (numDis > 9)
            {
                numDis = 1;
                color++;
                if (color > 3) color = 0;
            }
        }
    }
}
