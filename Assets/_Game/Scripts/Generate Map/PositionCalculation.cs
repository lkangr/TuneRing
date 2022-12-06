using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class PositionCalculation
{
    public static float adjustFactor = 6f;

    public static void CalculatePosition(List<CircleDetail> listCircle)
    {
        var rand = new Random();

        listCircle[0].posX = (float)rand.NextDouble() * GameManager.Resolution.x;
        listCircle[0].posY = (float)rand.NextDouble() * GameManager.Resolution.y;

        //Vector2 offset = Vector2.zero;

        //float temp, powTemp;

        float angle = (float)(rand.NextDouble() * 360);

        int color = 0, numDis = 2;

        for (int i = 1; i < listCircle.Count; i++)
        {
            var cur = listCircle[i];
            var pre = listCircle[i - 1];
            var deltaTime = (cur.time - pre.time) * adjustFactor;

            var adj = Mathf.Min(deltaTime / 4, 1f);
            var deltaAngle = adj * 180;
            angle = angle + (-deltaAngle + (float)rand.NextDouble() * 2 * deltaAngle);

            var vtLength = DegreeToVector2(angle);
            var maxLength = Mathf.Min(GameManager.Resolution.x / Mathf.Abs(vtLength.x), GameManager.Resolution.y / Mathf.Abs(vtLength.y));
            var rlength = adj * maxLength;
            var mlength = rlength - adj * rlength;
            var length = mlength + (float)rand.NextDouble() * (rlength - mlength);

            var pos = vtLength * length + new Vector2(pre.posX, pre.posY);
            if (pos.x < 0) cur.posX = -pos.x;
            else if (pos.x > GameManager.Resolution.x) cur.posX = pos.x - 2 * (pos.x - GameManager.Resolution.x);
            else cur.posX = pos.x;

            if (pos.y < 0) cur.posY = -pos.y;
            else if (pos.y > GameManager.Resolution.y) cur.posY = pos.y - 2 * (pos.y - GameManager.Resolution.y);
            else cur.posY = pos.y;

            angle = Vector2.SignedAngle(Vector2.right, new Vector2(cur.posX - pre.posX, cur.posY - pre.posY));
            if (angle < 0) angle += 360;

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

        //for (int i = 1; i < listCircle.Count; i++)
        //{
        //    var cur = listCircle[i];
        //    var pre = listCircle[i - 1];
        //    var deltaTime = (cur.time - pre.time);

        //    Vector2 dur = new Vector2(deltaTime * GameManager.Resolution.x, deltaTime * GameManager.Resolution.y);

        //    var offsetX = new Vector2(Mathf.Min(0, Mathf.Max(-dur.x, offset.x - dur.x)), Mathf.Max(0, Mathf.Min(dur.x, offset.x + dur.x)));
        //    var offsetY = new Vector2(Mathf.Min(0, Mathf.Max(-dur.y, offset.y - dur.y)), Mathf.Max(0, Mathf.Min(dur.y, offset.y + dur.y)));

        //    temp = (float)rand.NextDouble();
        //    if (offset.x == 0) powTemp = temp;
        //    else if (offset.x > 0) powTemp = Mathf.Pow(temp, adjustFactor);
        //    else powTemp = 1 - Mathf.Pow(temp, adjustFactor);
        //    var tempX = offsetX.x + (offset.x > 0 ? powTemp : 1 - powTemp) * (offsetX.y - offsetX.x);
        //    tempX = Mathf.Min(GameManager.Resolution.x, Mathf.Max(-GameManager.Resolution.x, tempX));

        //    temp = (float)rand.NextDouble();
        //    if (offset.y == 0) powTemp = temp;
        //    else if (offset.y > 0) powTemp = Mathf.Pow(temp, adjustFactor);
        //    else powTemp = 1 - Mathf.Pow(temp, adjustFactor);
        //    var tempY = offsetY.x + (offset.y > 0 ? powTemp : 1 - powTemp) * (offsetY.y - offsetY.x);
        //    tempY = Mathf.Min(GameManager.Resolution.y, Mathf.Max(-GameManager.Resolution.y, tempY));

        //    cur.posX = pre.posX + tempX;
        //    if (cur.posX < 0) cur.posX = -cur.posX;
        //    else if (cur.posX > GameManager.Resolution.x) cur.posX -= 2 * (cur.posX - GameManager.Resolution.x);

        //    cur.posY = pre.posY + tempY;
        //    if (cur.posY < 0) cur.posY = -cur.posY;
        //    else if (cur.posY > GameManager.Resolution.y) cur.posY -= 2 * (cur.posY - GameManager.Resolution.y);

        //    offset = new Vector2(cur.posX - pre.posX, cur.posY - pre.posY);

        //    cur.color = color;
        //    cur.numDis = numDis;

        //    numDis++;
        //    if (numDis > 9)
        //    {
        //        numDis = 1;
        //        color++;
        //        if (color > 3) color = 0;
        //    }
        //}
    }

    private static Vector2 DegreeToVector2(float degree)
    {
        var radian = degree * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
}