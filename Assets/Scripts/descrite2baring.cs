using UnityEngine;

class discrete2baringActionSpace
{
    float bearingFromActions(int[] actions)
    {
        int action = 0;
        int actionCount = 0;
        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] == 1)
            {
                action += i * 360 / actions.Length;
                actionCount++;
            }
        }
        return actionCount == 0 ? 0 : action / actionCount;
    }
}