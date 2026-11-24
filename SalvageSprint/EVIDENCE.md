# Week 1 evidence

Keep this short. Bullet points, a small table and one diagram or code trace are enough. This is your checkpoint record, not an extra report. Replace the prompts as you work.

Name: Teodora Murariu
Exact Unity version: 6.3.26f1
Checkpoint commit / repository: https://github.com/teomura/Artificial_Intelligence_Assignment
Current stage and anything unfinished: Stage 1

## Initial prediction

Three observations the rival needs, including one it might not have:

- to have a cargo target depot
- to have a way to "see" targets
- to be able to prioritise reachable instead of exclusively the closest one 

Seeing a pickup across a gap does not prove reachability because:

- there might not be a valid path (see F6: unreachable)

One question about NPC behaviour:

- Why isn't rival choosing the second closest cargo if the path is blocked for the first one?

## One complete trace

Use one actual situation. Include the observation data, derived facts, selected task and reason, movement request/result, and how feedback affects a later decision. Identify the responsible code locations. A tiny state diagram can accompany this trace.

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

| Scenario | Predicted result | Observed result and short evidence / remaining issue |
|---|---|---|
| Normal loop and restart | Collect scrap | Prioritises distance |
| Carrying cargo | | |
| Hidden / remembered pickup | | |
| Target taken | | |
| Unreachable target | | |
| No usable pickup | | |

Supplied test result, with any added tests identified:
Live walkthrough checked by / recording link if used:

## One correction and one independent check

The original weakness and why it happened:
What I changed and why it addresses the cause:
My counterexample or experiment: prediction, observed result, and one limitation:

## Collaboration and help

People I discussed or playtested with:
Code, sources or substantive tools/AI assistance I used, and how I verified them:
If unfinished: what works, exact reproduction steps, and the help I need:
