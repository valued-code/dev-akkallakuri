using System;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace ElevatorSimulation
{
    public class ElevatorScheduler
    {
        public bool InputIsStream = true;
        private readonly object locker = new object();
        public Elevator[] Elevators { get; set; }
        public Floor[] Floors { get; set; }

        public SortedDictionary<DateTime, ConcurrentQueue<PassengerRequest>> PassengerRequestStream = 
            new SortedDictionary<DateTime, ConcurrentQueue<PassengerRequest>>();

        // this dictionary will be useful to get at any instant point of time tj what elevator are assigned to which requests.
        public SortedDictionary<DateTime, List<PassengerRequest>> OutPassengerRequestStream =
            new SortedDictionary<DateTime, List<PassengerRequest>>();

        // create a background thread and keep on firing based on infinite loop
        public BackgroundWorker scheduleElevators = new BackgroundWorker();
        private int StartFloor = 0;
        public ElevatorScheduler(Elevator[] elevators,Floor[] floors)
        {
            Elevators = elevators;
            Floors = floors;
            scheduleElevators.DoWork += ScheduleElevators_DoWork;
        }

        //triggers scheduling.
        public void StartScheduling()
        {
            scheduleElevators.RunWorkerAsync();
        }
        private void ScheduleElevators_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (!InputIsStream)
                    ScheduleElevators();
                else
                {
                    var stream = GetStream();
                    foreach (var item in stream)
                    {
                        if (!PassengerRequestStream.ContainsKey(item.RequestDateTime))
                            PassengerRequestStream.Add(item.RequestDateTime, new ConcurrentQueue<PassengerRequest>());
                        PassengerRequestStream[item.RequestDateTime].Enqueue(item);
                    }
                    ScheduleStreamOfElevatorRequests();
                }
                Thread.Sleep(2000000);
            }
        }
        private List<PassengerRequest> GetStream()
        {
            int numberOfFloors = Floors.Length;
            int count = 0;
            List<PassengerRequest> requests = new List<PassengerRequest>();
            while (count < numberOfFloors)
            {
                Random random = new Random();
                int floorIndex = random.Next(0, Floors.Length);
                int directionFlag = random.Next(0, 2);
                Direction direction = Direction.None;

                if (directionFlag == 0)
                    direction = Direction.Up;
                else if (directionFlag == 1)
                    direction = Direction.Down;

                if (floorIndex == 0)
                    direction = Direction.Up;
                else if (floorIndex == numberOfFloors - 1)
                    direction = Direction.Down;

                int exitFloorIndex = -1;
                if (direction == Direction.Up)
                {
                    exitFloorIndex = random.Next(numberOfFloors);
                    while (exitFloorIndex <= floorIndex)
                        exitFloorIndex = random.Next(numberOfFloors);
                }
                else
                {
                    exitFloorIndex = random.Next(numberOfFloors);
                    while (exitFloorIndex >= floorIndex)
                        exitFloorIndex = random.Next(numberOfFloors);
                }
                count++;
                PassengerRequest request = new PassengerRequest(floorIndex, exitFloorIndex, true);
                Console.WriteLine($"Created request(stream) with {request.SourceFloorIndex}-->{request.TargetFloorIndex} and TimeStamp {request.RequestDateTime}");
                requests.Add(request);
            }
            return requests;
        }
        public void ScheduleStreamOfElevatorRequests()
        {
            lock (locker)
            {
                foreach (var kvp in PassengerRequestStream)
                {
                    var currentRequests = new List<PassengerRequest>();
                    foreach (var item in kvp.Value)
                    {
                        PassengerRequest request;
                        PassengerRequestStream[item.RequestDateTime].TryDequeue(out request);
                        if (request != null)
                        {
                            var optimumElevatorAssigned = -1;
                            var minimumSteps = int.MinValue;
                            for (int elevatorIndex = 0; elevatorIndex < Elevators.Length; elevatorIndex++)
                            {
                                // get the closest elevator.
                                var distance = Elevators[elevatorIndex].GetDistanceToNearestElevator(request);
                                if (minimumSteps < distance)
                                {
                                    minimumSteps = distance;
                                    optimumElevatorAssigned = elevatorIndex;
                                }
                            }
                            if (optimumElevatorAssigned != -1)
                            {
                                request.ElevatorAssigned = optimumElevatorAssigned;
                                currentRequests.Add(request);
                            }
                        }
                    }
                    //assigning the request to the elevator.
                    for (int elevatorIndex = 0; elevatorIndex < Elevators.Length; elevatorIndex++)
                    {
                        // get the closest elevator.
                        var requests = currentRequests.Where(l => l.ElevatorAssigned == elevatorIndex);
                        foreach (var r in requests)
                        {
                            Console.WriteLine($"For the following request {r.SourceFloorIndex}-->{r.TargetFloorIndex} the elevator # {elevatorIndex} is assigned");
                            if (!OutPassengerRequestStream.Keys.Contains(r.RequestDateTime))
                                OutPassengerRequestStream.Add(r.RequestDateTime, new List<PassengerRequest>());

                            OutPassengerRequestStream[r.RequestDateTime].Add(r);
                        }
                        if (requests.Any())
                            Elevators[elevatorIndex].AssignJobs(requests);
                    }
                }
            }
        }

        //schedules elevators based on random input generated.
        public void ScheduleElevators()
        {
            bool foundPassenger = false;
            PassengerRequest outValue = new PassengerRequest();
            int requestingFloor = -1;
            for (int floorIndex = StartFloor; floorIndex < Floors.Length; floorIndex++)
            {
                var floor = Floors[floorIndex];
                if (floor.PassengerRequests.Count > 0)
                {
                    floor.PassengerRequests.TryDequeue(out outValue);
                    if (outValue != null)
                    {
                        if (floorIndex == Floors.Length - 1)
                            StartFloor = 0;
                        else
                            StartFloor = floorIndex + 1;
                        
                        foundPassenger = true;
                        requestingFloor = floorIndex;
                        break;
                    }
                }
                if (floorIndex == Floors.Length - 1)
                    StartFloor = 0;
                else
                    StartFloor = floorIndex + 1;
            }
            if (foundPassenger)
            {
                var optimumElevatorAssigned = -1;
                var minimumSteps = int.MinValue;
                for (int elevatorIndex = 0; elevatorIndex < Elevators.Length; elevatorIndex++)
                {
                    // get the closest elevator.
                    var distance = Elevators[elevatorIndex].GetDistanceToNearestElevator(outValue);
                    if(minimumSteps < distance)
                    {
                        minimumSteps = distance;
                        optimumElevatorAssigned = elevatorIndex;
                    }
                }
                if (optimumElevatorAssigned != -1)
                {
                    Console.WriteLine($"For the following request {outValue.SourceFloorIndex}-->{outValue.TargetFloorIndex} the elevator # {optimumElevatorAssigned} is assigned");
                    //assigning the request to the elevator.
                    Elevators[optimumElevatorAssigned].AssignJob(outValue);
                }
            }
        }

        public List<string> GetElevatorsAssigned(DateTime dateTime)
        {
            var elevatorsAssigned = new List<string>();
            if (OutPassengerRequestStream.Keys.Contains(dateTime))
            {
                foreach (var item in OutPassengerRequestStream[dateTime])
                {
                    elevatorsAssigned.Add(item.ToString());
                }
            }
            return elevatorsAssigned;
        }
    }
}
