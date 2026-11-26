using System;

namespace SalvageSprint.Core
{
    public enum TaskKind
    { Explore, Collect, Deliver }

    public enum TravelStatus
    { Idle, Pending, Moving, Arrived, Failed }

    public readonly struct Candidate
    {
        public readonly int Id;
        public readonly float DistanceSquared;
        public readonly bool Available, Visible, Remembered, Blocked;

        public Candidate(int id, float distanceSquared, bool available, bool visible,
            bool remembered = false, bool blocked = false)
        {
            Id = id;
            DistanceSquared = distanceSquared;
            Available = available;
            Visible = visible;
            Remembered = remembered;
            Blocked = blocked;
        }
    }

    public readonly struct Observation
    {
        public readonly bool Carrying;// true if the agent currently holds cargo; collection handled by MathcController
        public readonly int CurrentTargetId;// pursued/retained pickup ID (-1 it there's no current target)
        public readonly Candidate[] Candidates;// candidate data from sensor. Does not establish IsEligible or Remembered

        public Observation(bool carrying, int currentTargetId, Candidate[] candidates)
        {
            Carrying = carrying;
            CurrentTargetId = currentTargetId;
            Candidates = candidates ?? Array.Empty<Candidate>();
        }
    }

    public readonly struct Decision
    {
        public readonly TaskKind Task;
        public readonly int TargetId;//selected pickup ID for Collect. Deliver and Explore use -1; brain establishes destinations.
        public readonly string Reason;

        public Decision(TaskKind task, int targetId, string reason)
        { Task = task; TargetId = targetId; Reason = reason; }
    }

    public readonly struct AgentFacts
    {
        public readonly bool NeedsDelivery, HasUsableTarget, RetainedTarget;
        public readonly int SelectedTargetId;

        public AgentFacts(bool carrying, int selectedTargetId, bool retainedTarget)
        {
            NeedsDelivery = carrying;
            SelectedTargetId = selectedTargetId;
            HasUsableTarget = selectedTargetId >= 0;
            RetainedTarget = retainedTarget;
        }
    }

    public static class NavigationFeedback
    {
        // A request is asynchronous. A complete route is not the same as arrival.
        // parameters: onMesh, pending, completePath, hasPath, remainingDistance, stoppingDistance
        // onMesh: true if the agent is on a valid NavMesh surface; false if it has fallen off or is otherwise invalid.
        // pending: true if the agent is still calculating a path; false if it has finished calculating.
        // completePath: true if the agent has a complete path to the destination; false if the path is partial or invalid.
        // hasPath: true if the agent currently has a path; false if it does not.
        // remainingDistance: remaining path distance. Positive infinity means that the distance is unknown.
        // stoppingDistance: the distance from the destination at which the agent will stop; used to determine if the agent has arrived.
        public static TravelStatus Classify(bool onMesh, bool pending, bool completePath,
            bool hasPath, float remainingDistance, float stoppingDistance)
        {
            // whithout a NavMesh placement, the agent cannot navigate.
            if (!onMesh)
                return TravelStatus.Failed;

            // while Unity calculates the route, other path values may not describe hte newle requested journey yet.
            if (pending) return TravelStatus.Pending;

            // partial/invalid routes fail this task
            //NaN adn negative distances are also unusable feedback.
            if (!completePath || float.IsNaN(remainingDistance) || remainingDistance < 0)
                return TravelStatus.Failed;

            // Positive infinity means the remaining distance is unknown.
            // It does not, by itself, mean the route is impossible.
            //
            // If a complete path exists, keep the journey in progress.
            // Navigator's no-progress timeout still handles a stuck agent.
            // "Moving" here describes travel in progress, not guaranteed
            // physical movement during this particular frame.
            if (float.IsPositiveInfinity(remainingDistance)) return hasPath ? TravelStatus.Moving : TravelStatus.Failed;

            // Arrival requires a known distance within the stopping threshold.
            // This still does not prove that collection/delivery has happened.
            if (remainingDistance <= stoppingDistance) return TravelStatus.Arrived;
            return hasPath ? TravelStatus.Moving : TravelStatus.Failed;
        }
    }
}