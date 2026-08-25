using UnityEngine;

namespace Clicker.Game
{
    public sealed class UnityRandomTargetSequence : ITargetSequence
    {
        public int Next(int previousIndex, int targetCount)
        {
            if (previousIndex < 0)
            {
                return Random.Range(0, targetCount);
            }

            int next = Random.Range(0, targetCount - 1);
            return next >= previousIndex ? next + 1 : next;
        }
    }
}
