using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;
using System.Collections.Concurrent;
using System;

namespace ElevatorSimulation
{
    public enum ElevatorStatus
    {
        Idle,
        Moving
    }

    public enum Direction
    {
        Up,
        Down,
        None
    }

    //Comparer for UpQueue
    public class UpQueueComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y)
                return 1;
            if (x < y)
                return -1;
            return 0;
        }
    }
    //Comparer for DownQueue
    public class DownQueueComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x < y)
                return 1;
            if (x > y)
                return -1;
            return 0;
        }
    }
    public class Elevator
    {
        private readonly object locker = new object();
        protected ElevatorStatus CurrentElevatorStatus { get; set; }
        protected int CurrentFloor { get; set; }

        // these two queues behaves like priority queues, but not thread safe.
        // .net framework does not supports priority queues in c#, I spent most the time to search for that.
        // Because these two queues not thread safe, I have to implement thread synchronization.
        protected SortedSet<int> UpQueue = new SortedSet<int>(new UpQueueComparer());
        protected SortedSet<int> DownQueue = new SortedSet<int>(new DownQueueComparer());
        protected ConcurrentQueue<PassengerRequest> PassengerRequests { get; set; }
        protected int ElevatorNumber { get; set; }
        protected Direction ElevatorDirection { get; set; }

        // These are two backgroud threads,one for receving the requests and other for performing the actual job.
        protected BackgroundWorker performJob = new BackgroundWorker();
        protected BackgroundWorker elevatorThread = new BackgroundWorker();
        protected int NumberOfFloors;
        public Elevator(int elevatorNumber,int numberOfFloors)
        {
            CurrentFloor = 0;
            CurrentElevatorStatus = ElevatorStatus.Idle;
            PassengerRequests = new ConcurrentQueue<PassengerRequest>();
            ElevatorNumber = elevatorNumber;
            NumberOfFloors = numberOfFloors;
            performJob.DoWork += PerformJob;
            performJob.RunWorkerAsync();
            elevatorThread.DoWork += ElevatorThread_DoWork;
            elevatorThread.RunWorkerAsync();
        }

        private void ElevatorThread()
        {
            lock (locker)
            {
                if (PassengerRequests != null && PassengerRequests.Count > 0)
                {
                    PassengerRequest request;
                    PassengerRequests.TryPeek(out request);
                    if (request != null)
                    {
                        //coming down to pick a person and go up
                        if (CurrentElevatorStatus == ElevatorStatus.Idle 
                            && request.PassengerRequestDirection == Direction.Up 
                            && CurrentFloor > request.SourceFloorIndex 
                            && UpQueue.Count == 0)
                        {
                            DownQueue.Add(request.SourceFloorIndex);
                            DownQueue.Add(request.TargetFloorIndex);
                            CurrentElevatorStatus = ElevatorStatus.Moving;
                            ElevatorDirection = Direction.Up;
                            Console.WriteLine($"Elevator# {ElevatorNumber} is moving down from {CurrentFloor}-->{request.SourceFloorIndex} to serve (up)_request,{request.SourceFloorIndex}-->{request.TargetFloorIndex}");
                        }
                        //coming up to pick a person and go down
                        else if(CurrentElevatorStatus == ElevatorStatus.Idle 
                            && request.PassengerRequestDirection == Direction.Down
                            && CurrentFloor < request.SourceFloorIndex 
                            && DownQueue.Count == 0)
                        {
                            UpQueue.Add(request.SourceFloorIndex);
                            UpQueue.Add(request.TargetFloorIndex);
                            CurrentElevatorStatus = ElevatorStatus.Moving;
                            ElevatorDirection = Direction.Down;
                            Console.WriteLine($"Elevator# {ElevatorNumber} is moving up from {CurrentFloor}-->{request.SourceFloorIndex} to serve (down)_request,{request.SourceFloorIndex}-->{request.TargetFloorIndex}");
                        }
                        else if(request.PassengerRequestDirection == Direction.Up 
                            && CurrentFloor <= request.SourceFloorIndex 
                            && DownQueue.Count == 0)
                        { 
                            UpQueue.Add(request.SourceFloorIndex);
                            UpQueue.Add(request.TargetFloorIndex);
                            ElevatorDirection = Direction.Up;
                            CurrentElevatorStatus = ElevatorStatus.Moving;
                            PassengerRequests.TryDequeue(out request);
                            Console.WriteLine($"Elevator# {ElevatorNumber} received a request_details(UP) " +
                                $":{request.SourceFloorIndex}-->{request.TargetFloorIndex}");
                        }
                        else if (request.PassengerRequestDirection == Direction.Down 
                            && CurrentFloor >= request.SourceFloorIndex 
                            && UpQueue.Count == 0)
                        {
                            DownQueue.Add(request.SourceFloorIndex);
                            DownQueue.Add(request.TargetFloorIndex);
                            ElevatorDirection = Direction.Down;
                            CurrentElevatorStatus = ElevatorStatus.Moving;
                            PassengerRequests.TryDequeue(out request);
                            Console.WriteLine($"Elevator# {ElevatorNumber} received a request_details(DOWN) :{request.SourceFloorIndex}-->{request.TargetFloorIndex}");
                        }
                    }
                }
            }
        }
        private void ElevatorThread_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                ElevatorThread();
                Thread.Sleep(2000);
            }
        }
        private void PerformJob(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                PerformJob();
                Thread.Sleep(1000);
            }
        }

        public int GetDistanceToNearestElevator(PassengerRequest request)
        {
            int optDistance = -1;
            int distance = Math.Abs(CurrentFloor - request.SourceFloorIndex);
            if (CurrentElevatorStatus == ElevatorStatus.Idle)
                optDistance = NumberOfFloors + 1 - distance;
            else if (ElevatorDirection == Direction.Down)
            {
                if (request.SourceFloorIndex > CurrentFloor)
                    optDistance = 1;
                else if (request.SourceFloorIndex < CurrentFloor)
                {
                    if (request.PassengerRequestDirection == ElevatorDirection)
                        optDistance = NumberOfFloors + 2 - distance;
                    else
                        optDistance = NumberOfFloors + 1 - distance;
                }
            }
            else if(ElevatorDirection == Direction.Up)
            {
                if (request.SourceFloorIndex < CurrentFloor)
                    optDistance = 1;
                else if (request.SourceFloorIndex > CurrentFloor)
                {
                    if (request.PassengerRequestDirection == ElevatorDirection)
                        optDistance = NumberOfFloors + 2 - distance;
                    else
                        optDistance = NumberOfFloors + 1 - distance;
                }
            }
            return optDistance;
        }
        //public int GetDistance(PassengerRequest request)
        //{
        //    lock (locker)
        //    {
        //        if (CurrentElevatorStatus == ElevatorStatus.Idle
        //        && CurrentFloor == request.SourceFloorIndex)
        //            return 0;
        //        else if (CurrentElevatorStatus == ElevatorStatus.Idle)
        //            return Math.Abs(CurrentFloor - request.SourceFloorIndex);
        //        else if (CurrentElevatorStatus == ElevatorStatus.Moving)
        //        {
        //            //passenger wants to go up and elevator also going up and 
        //            //elevator current floor is less than passenger 
        //            //source floor
        //            //passenger wants to go down and elevator also going down 
        //            //and elevator current floor is above than passenger 
        //            //source floor
                    
        //                if (
        //                    (CurrentFloor < request.SourceFloorIndex
        //                        && request.PassengerRequestDirection == Direction.Up
        //                        && ElevatorDirection == Direction.Up) 
        //                 || (CurrentFloor > request.SourceFloorIndex
        //                        && request.PassengerRequestDirection == Direction.Down
        //                        && ElevatorDirection == Direction.Down)
        //                )
        //           return Math.Abs(request.SourceFloorIndex - CurrentFloor);
        //        }
        //    }
        //    return int.MaxValue;
        //}
        private void PerformJob()
        {
            lock (locker)
            {
                if (UpQueue.Count > 0)
                {
                    Console.WriteLine($"Elevator# {ElevatorNumber} Direction(UP) is in {CurrentFloor}");
                    if(UpQueue.Contains(CurrentFloor))
                        UpQueue.Remove(CurrentFloor);
                    if (UpQueue.Count == 0)
                    {
                        CurrentElevatorStatus = ElevatorStatus.Idle;
                        ElevatorDirection = Direction.None;
                        Console.WriteLine($"Elevator #{ElevatorNumber} is in Idle status and current floor is {CurrentFloor}");
                    }
                    else
                    {
                        if (CurrentFloor + 1 <= UpQueue.Max && UpQueue.Count > 0)
                            CurrentFloor += 1;
                    }
                }
                if (DownQueue.Count > 0)
                {
                    Console.WriteLine($"Elevator# {ElevatorNumber} Direction(DOWN) is in {CurrentFloor}");
                    if (DownQueue.Contains(CurrentFloor))
                        DownQueue.Remove(CurrentFloor);
                    if (DownQueue.Count == 0)
                    {
                        CurrentElevatorStatus = ElevatorStatus.Idle;
                        ElevatorDirection = Direction.None;
                        Console.WriteLine($"Elevator# {ElevatorNumber} is in Idle status and current floor is {CurrentFloor}");
                    }
                    else
                    {
                        if (CurrentFloor - 1 >= DownQueue.Max && DownQueue.Count > 0)
                            CurrentFloor -= 1;
                    }
                }
            }
        }
        public void AssignJob(PassengerRequest passengerRequest)
        {
            PassengerRequests.Enqueue(passengerRequest);
        }
        public void AssignJobs(IEnumerable<PassengerRequest> passengerRequests)
        {
            foreach (var item in passengerRequests)
            {
                PassengerRequests.Enqueue(item);
            }
        }
    }
}
