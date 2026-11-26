namespace SalvageSprint.Core
{
    // Week 1 assignment starter. Work through README stages in this same project.
    // STAGE 2: distinguish visible candidates from unknown hidden objects.
    // STAGE 3: retain a usable remembered target. STAGE 4: use failed-route feedback.
    // Delivery is supplied so the unmodified game has a complete scoring loop.
    public static class DecisionPolicy
    {
        public const string Label = "Teodora";

        public static AgentFacts Infer(Observation observation)
        {
            if (observation.Carrying)
                return new AgentFacts(true, -1, false);

            int nearest = -1;
            float distance = float.PositiveInfinity;
            foreach (Candidate candidate in observation.Candidates)
            {
                // Deliberately naive: ignores Visible, Remembered and Blocked.
                if (!(candidate.Visible || candidate.Remembered)) // added visibility and remembered to the decision
                {
                    continue;
                }

                if (candidate.Available && candidate.DistanceSquared < distance) 
                { nearest = candidate.Id; distance = candidate.DistanceSquared; }

                if (candidate.Blocked)
                {
                    return new AgentFacts(false, -1, true);// keeps it on explore
                }
            }
            return new AgentFacts(false, nearest, false);
        }

        public static Decision Choose(Observation observation) => Choose(Infer(observation));

        public static Decision Choose(AgentFacts facts)
        {
            if (facts.NeedsDelivery)
                return new Decision(TaskKind.Deliver, -1, "Cargo held: return to depot");
            return facts.HasUsableTarget
                ? new Decision(TaskKind.Collect, facts.SelectedTargetId, "Nearest candidate; reselected each tick")
                : new Decision(TaskKind.Explore, -1, "No candidate: visit the next survey point");
        }
    }
}
