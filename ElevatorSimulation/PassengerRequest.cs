using System;
namespace ElevatorSimulation
{
    public class PassengerRequest
    {
        public PassengerRequest()
        {

        }
        public Guid RequestId { get; set; }
        public int SourceFloorIndex { get; set; }
        public int TargetFloorIndex { get; set; }
        public int ElevatorAssigned { get; set; }
        public DateTime RequestDateTime { get; set; }
        public Direction PassengerRequestDirection
        {
            get {
                if (SourceFloorIndex < TargetFloorIndex)
                    return Direction.Up;
                else
                    return Direction.Down;
            }
        }
        public PassengerRequest(int sourceFloorIndex,int targetFloorIndex,bool useRandomTime = false)
        {
            SourceFloorIndex = sourceFloorIndex;
            TargetFloorIndex = targetFloorIndex;
            RequestId = Guid.NewGuid();
            RequestDateTime = DateTime.Now.AddMinutes(DateTime.Now.Minute - (Math.Ceiling(8.0 - new Random().Next(0, 1))));
        }
        public override string ToString()
        {
            return $"For this request {RequestId}-SourceFloor:{SourceFloorIndex}-->TargetFloor{TargetFloorIndex} created at {RequestDateTime} the elevator #{ElevatorAssigned} is assigned";
        }
    }
}
