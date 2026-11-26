namespace SalvageSprint.Core
{
    // Week 1 assignment starter. Work through README stages in this same project.
    // STAGE 2: distinguish visible candidates from unknown hidden objects.
    // STAGE 3: retain a usable remembered target. STAGE 4: use failed-route feedback.
    // Delivery is supplied so the unmodified game has a complete scoring loop.
    public static class DecisionPolicy
    {
        public const string Label = "Teodora's Salvage Sprint";

        private static bool IsEligible(Candidate candidate)
        {
            return candidate.Available && (candidate.Visible || candidate.Remembered)
                && !candidate.Blocked;
        }

        public static AgentFacts Infer(Observation observation)
        {
            // Cargo is an actual game-state fact. Deliver even if collection
            // targets are missing, hidden, or temporarily blocked.
            if (observation.Carrying)
                return new AgentFacts(true, -1, false);

            // First look for the existing target. Match its ID, not its
            // array position. It can occur anywhere in the candidate list.

            if (observation.CurrentTargetId >= 0)// differentiates from -1
            {
                foreach (Candidate candidate in observation.Candidates)// looks first for existing target matchind its ID
                {
                    if (candidate.Id == observation.CurrentTargetId && IsEligible(candidate))
                    {
                        return new AgentFacts(false, candidate.Id, true);
                    }
                }
            }

            // If the old task is no longer useful, find a new eligible target.
            // This compares horizontal straight-line distance, not path length.
            int nearest = -1;
            float distance = float.PositiveInfinity;
            foreach (Candidate candidate in observation.Candidates)
            {
                if (IsEligible(candidate) && candidate.DistanceSquared < distance)
                {
                    nearest = candidate.Id;
                    distance = candidate.DistanceSquared;
                }
            }
            return new AgentFacts(false, nearest, false);//nearest remains -1; decision to Explore
        }

        public static Decision Choose(Observation observation) => Choose(Infer(observation));

        public static Decision Choose(AgentFacts facts)
        {
            if (facts.NeedsDelivery)
                return new Decision(TaskKind.Deliver, -1, "Cargo held: return to depot");

            if (!facts.HasUsableTarget)
                return new Decision(TaskKind.Explore, -1, "No eligible known pickup: visit the next survey point");

            string reason = facts.RetainedTarget
                ? "Retain current eligible target"
                : "Nearest eligible known pickup";

            return new Decision(TaskKind.Collect, facts.SelectedTargetId, reason);
        }
    }
}