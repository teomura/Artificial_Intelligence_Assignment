# Week 1 evidence

Keep this short. Bullet points, a small table and one diagram or code trace are enough. This is your checkpoint record, not an extra report. Replace the prompts as you work.

Name: Teodora Murariu
Exact Unity version: 6.3.26f1
Checkpoint commit / repository: https://github.com/teomura/Artificial_Intelligence_Assignment
Current stage and anything unfinished: Stage 5

## Initial prediction

Three observations the rival needs, including one it might not have:


- to have a cargo target to pick up 
- to have a way to "see" targets
- to be able to prioritise reachable instead of exclusively the closest one 

Seeing a pickup across a gap does not prove reachability because:

- there might not be a valid path (see F6: unreachable)

One question about NPC behaviour:

- Why isn't rival choosing the second closest cargo if the path is blocked for the first one?

## One complete trace

Use one actual situation. Include the observation data, derived facts, selected task and reason, movement request/result, and how feedback affects a later decision.
Identify the responsible code locations. A tiny state diagram can accompany this trace.

Unreachable closest target.  

First frame: 
TravelStatus = Idle
Observation(actor.Carrying (false), rememberedId (-1), candidates.ToArray() (0, 1));
AgentFacts(bool carrying (false), int selectedTargetId (0), bool retainedTarget (false)) (because it  ignores Visible, Remembered and Blocked).
AgentFacts.HasUsableTarget = true
Task = Collect
Reason = "Nearest candidate; reselected each tick"
Movement request => SetDestination() => TravelStatus = Pending

Second frame:
TravelStatus = Failed (because of agent.PathStatus = PathPartial)
return

Next decision frame:
TravelStatus = Failed
Marks 0 as invalid for 6s (failedUntil[rememberedId] = Time.time + 6).
Starts repeating same instructions and results as frame one and then frame two.




## Scenario record

Initial findings

| Scenario | Predicted result | Observed result and short evidence / remaining issue |
|---|---|---|
| Normal loop and restart | Collect scrap | Prioritises distance |
| Carrying cargo | Deliver cargo | If the path is blocked it fails to deliver |
| Hidden / remembered pickup | Collect remembered pickup | Goes to remembered cargo but if closer cargo becomes visible, it goes to that instead |
| Target taken | Find new target | it finds a new one but stops after picking it up |
| Unreachable target | Find new target | blocked in a 6s waiting loop|
| No usable pickup | Explore | if depo is visible it returs to it; it it's not visible it gets stuck in a loop |

## Short state-transition description:

Explore -> Collect when an eligible pickup is selected.

	DecisionPolicy.Infer finds a candidate that IsEligible whose ID matches Observation.CurrentTargetId (or the nearest if it fails).
	Choose decides to Collect the usable target and RivalBrain.Update does the "driving" to the pickup and marks the target sets a "rememberedId".

Collect -> Deliver after successful collection gives cargo.

	After the scrap is collected, Carrying = true, Infer returns NeedsDelivery = true, Choose decides to Deliver to -1
	and Rival.Brain removes rememberedId and goes to actor.DepotPosition.

Deliver -> Collect or Explore after delivery, depending on available knowledge.

	Carrying returns to false, the mext Infer will search for a known eligible candidate (visible or still remembered),
	it selects one goes straight into Collect. 
	If none is known, it goes to -1, Choose decides to Explore and RivalBrain sends rival to the next surveyPoint.

Collect -> another Collect target or Explore when the current target becomes unusable.

	If the pickup is no longer eligible, Infer will look for nearest usable target (Collect -> RetainedTarget = false)
	or it will go into Explore if nothing is eligible.



Supplied test result, with any added tests identified: All 22 tests passed

Live walkthrough checked by / recording link if used:

## One correction and one independent check

The original weakness and why it happened:

In the starter stage, when a chosen pickup was out of reach, Infer only judged on avaliability and nearest distance, 
which put a rival in a loop of indecision (nothing fed that failure back into target selection,
so the same target was re-chosen and the same route re-requested every decision tick).

What I changed and why it addresses the cause:

DecisionPolicy.IsEligible() is now used to reject blocked candidates which makes the rival drop the unreachable target and go to the nearest eligible pickup or start exploring.

My counterexample or experiment: prediction, observed result, and one limitation:

Prediction:
Predicted that the starter policy would stay locked on the unreachable target with Travel: Failed recurring, 
while the fixed version would fail within one decision interval, show Blocked: 1 on the HUD, and start collecting another pickup or cycling survey points.

Observation:
The rival abandoned the unreachable target, logged the failure, and kept scoring elsewhere, matching the prediction.

Limitation:
The 6-second block is temporary, so a permanently unreachable pickup can still be taken into account for a decision cycle and then be discarded on the next decision.

## Collaboration and help

People I discussed or playtested with: Charlie, Midas, Simon

Code, sources or substantive tools/AI assistance I used, and how I verified them:
If unfinished: what works, exact reproduction steps, and the help I need:
