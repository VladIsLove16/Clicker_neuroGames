using System.Collections.Generic;
using Clicker.Game;
using NUnit.Framework;

namespace Clicker.Tests
{
    public sealed class ClickerSessionTests
    {
        [Test]
        public void Start_InitializesRoundAndFirstTarget()
        {
            ClickerSession session = CreateSession(new FixedSequence(2));

            session.Start();

            Assert.That(session.State, Is.EqualTo(SessionState.Playing));
            Assert.That(session.RemainingTime, Is.EqualTo(30f));
            Assert.That(session.Score, Is.Zero);
            Assert.That(session.CurrentTargetIndex, Is.EqualTo(2));
        }

        [Test]
        public void CorrectClick_IncrementsScoreAndAdvancesTarget()
        {
            ClickerSession session = CreateSession(new FixedSequence(2, 5));
            session.Start();

            ClickResult result = session.RegisterClick(2);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.WasCorrect, Is.True);
            Assert.That(result.Score, Is.EqualTo(1));
            Assert.That(result.CurrentTargetIndex, Is.EqualTo(5));
            Assert.That(result.RemainingTime, Is.EqualTo(30f));
        }

        [Test]
        public void WrongClick_SubtractsPenaltyWithoutChangingTargetOrScore()
        {
            ClickerSession session = CreateSession(new FixedSequence(2));
            session.Start();

            ClickResult result = session.RegisterClick(4);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.WasCorrect, Is.False);
            Assert.That(result.Score, Is.Zero);
            Assert.That(result.CurrentTargetIndex, Is.EqualTo(2));
            Assert.That(result.RemainingTime, Is.EqualTo(29f));
        }

        [Test]
        public void Advance_WhenTimerExpires_FinishesExactlyOnce()
        {
            ClickerSession session = CreateSession(new FixedSequence(1));
            session.Start();

            bool firstFinish = session.Advance(30f);
            bool secondFinish = session.Advance(1f);

            Assert.That(firstFinish, Is.True);
            Assert.That(secondFinish, Is.False);
            Assert.That(session.State, Is.EqualTo(SessionState.Finished));
            Assert.That(session.RemainingTime, Is.Zero);
            Assert.That(session.CurrentTargetIndex, Is.EqualTo(-1));
        }

        [Test]
        public void WrongClick_CanEndRoundWhenPenaltyConsumesRemainingTime()
        {
            ClickerSession session = new(0.5f, 1f, 9, new FixedSequence(0));
            session.Start();

            ClickResult result = session.RegisterClick(1);

            Assert.That(result.Finished, Is.True);
            Assert.That(session.State, Is.EqualTo(SessionState.Finished));
            Assert.That(result.RemainingTime, Is.Zero);
        }

        private static ClickerSession CreateSession(ITargetSequence sequence)
        {
            return new ClickerSession(30f, 1f, 9, sequence);
        }

        private sealed class FixedSequence : ITargetSequence
        {
            private readonly Queue<int> indices;

            public FixedSequence(params int[] indices)
            {
                this.indices = new Queue<int>(indices);
            }

            public int Next(int previousIndex, int targetCount)
            {
                return indices.Dequeue();
            }
        }
    }
}
