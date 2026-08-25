using System;

namespace Clicker.Game
{
    public enum SessionState
    {
        Ready,
        Playing,
        Finished
    }

    public readonly struct ClickResult
    {
        public ClickResult(
            bool accepted,
            bool wasCorrect,
            bool finished,
            int score,
            int currentTargetIndex,
            float remainingTime)
        {
            Accepted = accepted;
            WasCorrect = wasCorrect;
            Finished = finished;
            Score = score;
            CurrentTargetIndex = currentTargetIndex;
            RemainingTime = remainingTime;
        }

        public bool Accepted { get; }
        public bool WasCorrect { get; }
        public bool Finished { get; }
        public int Score { get; }
        public int CurrentTargetIndex { get; }
        public float RemainingTime { get; }
    }

    public interface ITargetSequence
    {
        int Next(int previousIndex, int targetCount);
    }

    /// <summary>
    /// Framework-independent round state. Unity objects never leak into the game rules.
    /// </summary>
    public sealed class ClickerSession
    {
        private readonly float roundDuration;
        private readonly float wrongTargetPenalty;
        private readonly int targetCount;
        private readonly ITargetSequence targetSequence;

        public ClickerSession(
            float roundDuration,
            float wrongTargetPenalty,
            int targetCount,
            ITargetSequence targetSequence)
        {
            if (roundDuration <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(roundDuration));
            }

            if (wrongTargetPenalty < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(wrongTargetPenalty));
            }

            if (targetCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(targetCount));
            }

            this.targetSequence = targetSequence ?? throw new ArgumentNullException(nameof(targetSequence));
            this.roundDuration = roundDuration;
            this.wrongTargetPenalty = wrongTargetPenalty;
            this.targetCount = targetCount;
        }

        public SessionState State { get; private set; } = SessionState.Ready;
        public bool IsPlaying => State == SessionState.Playing;
        public float RemainingTime { get; private set; }
        public int Score { get; private set; }
        public int CurrentTargetIndex { get; private set; } = -1;

        public void Start()
        {
            RemainingTime = roundDuration;
            Score = 0;
            CurrentTargetIndex = GetNextTarget(-1);
            State = SessionState.Playing;
        }

        /// <returns>True only on the update that ends the session.</returns>
        public bool Advance(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }

            if (!IsPlaying)
            {
                return false;
            }

            RemainingTime = Math.Max(0f, RemainingTime - deltaTime);
            if (RemainingTime > 0f)
            {
                return false;
            }

            Finish();
            return true;
        }

        public ClickResult RegisterClick(int targetIndex)
        {
            if (!IsPlaying || targetIndex < 0 || targetIndex >= targetCount)
            {
                return Snapshot(accepted: false, wasCorrect: false);
            }

            bool wasCorrect = targetIndex == CurrentTargetIndex;
            if (wasCorrect)
            {
                Score++;
                CurrentTargetIndex = GetNextTarget(CurrentTargetIndex);
            }
            else
            {
                RemainingTime = Math.Max(0f, RemainingTime - wrongTargetPenalty);
                if (RemainingTime <= 0f)
                {
                    Finish();
                }
            }

            return Snapshot(accepted: true, wasCorrect);
        }

        private int GetNextTarget(int previousIndex)
        {
            int nextIndex = targetSequence.Next(previousIndex, targetCount);
            if (nextIndex < 0 || nextIndex >= targetCount)
            {
                throw new InvalidOperationException(
                    $"Target sequence returned {nextIndex}; expected an index in [0, {targetCount - 1}].");
            }

            if (nextIndex == previousIndex)
            {
                throw new InvalidOperationException("Target sequence must not repeat the active target.");
            }

            return nextIndex;
        }

        private void Finish()
        {
            RemainingTime = 0f;
            CurrentTargetIndex = -1;
            State = SessionState.Finished;
        }

        private ClickResult Snapshot(bool accepted, bool wasCorrect)
        {
            return new ClickResult(
                accepted,
                wasCorrect,
                State == SessionState.Finished,
                Score,
                CurrentTargetIndex,
                RemainingTime);
        }
    }
}
