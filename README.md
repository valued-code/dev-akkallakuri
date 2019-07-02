# dev-akkallakuri
Senior Algorithms Engineer - coding challenge

Senior Algorithms Engineer

# Java
- There are 3 elevators in a 16-floor building
- Implement a Java simulator system that schedules the elevators
- Start scheduling when simulator's start() method is invoked
- Every elevator starts on the ground floor
- Each elevator travels at velocity of 1 floor/sec and has its own Drive System
- System accepts async input from Destination-Specific Call Buttons on each floor
- Create a scheduler that enqueues each Call Request to the stationary or approaching elevator that is closest to the Requesting Floor.
- If there is more than one qualifying elevator, prefer elevators in motion over stationary elevators. Any other tie-break is arbitrary and up to you.

# Assumptions
- Every elevator serves every floor of the building; there are exactly 16 floors
- Passengers instantly enter and exit
- Each elevator has infinite capacity
- Drive System accelerates each elevator instantaneously and with a constant velocity
- The system runs indefinitely (no graceful shutdown)
- Elevators themselves do not have any buttons. The call button panel that is typically found in an elevator is instead found on each floor. Passengers specify their Destination Floor with a Destination-Specific Call Button located on each floor prior to entering the elevator. Signage above each elevator directs passengers to the appropriate elevator.

# Terminology
- Call Request: The pressing of a call button on a request floor.
- Destination Floor: The floor number requested by a passenger on another floor.
- Destination-Specific Call Button: A button on a request floor that signals the intent of a passenger on her request floor to travel to a destination floor.
- Drive System: An engine that moves each elevator from one floor to another at a constant velocity of 1 floor per second.
Request Floor: The floor number from which a call button is pressed.

# Algorithm Optimization
Now suppose that each elevator E_i is assigned a home floor h_i. We simulate the requests coming in as an endless stream of tuples (t_j, s_j, d_j) where t_j \in \mathbb{N} \cup {0} is a discrete time value, s_j and d_j are source and destination floor indices respectively. Assume that t_{j + 1} = t_{j} - ln(r / 5) where r is a random number sampled uniformly from (0,1]. The scheduler should output, at any given time t_j, the indices of the corresponding elevators assigned to the requests that minimize the distance between the elevator’s destination and home floors.

Elevators move at 1 time-step per floor.

Example, for N=2 elevators and C=10 floors, h_0 = 3, h_1 = 8

Input: (1,  2,  5), (9,  7,  8), ...

Output: (0, 1) at t = 4

This output means that the first request was assigned to elevator 0, and the second request was assigned to elevator 1.
