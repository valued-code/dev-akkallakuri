using System;
using System.Collections.Concurrent;

namespace ElevatorSimulation
{
    public class Floor
    {
        //Threadsafe concurrent queue.
        public ConcurrentQueue<PassengerRequest> PassengerRequests { get; set; }
        public Floor()
        {
            PassengerRequests = new ConcurrentQueue<PassengerRequest>();
        }
        public void CreatePassengerRequests(int numberOfFloors, int floorIndex)
        {
            Random random = new Random();
            Direction direction = Direction.None;
            int directionFlag = random.Next(0, 2);

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
            Console.WriteLine($"Created a request for SourceFloor: {floorIndex} and ExitFloor {exitFloorIndex}");
            PassengerRequests.Enqueue(new PassengerRequest(floorIndex, exitFloorIndex));
        }
    }
}
