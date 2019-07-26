using System;
using System.ComponentModel;
using System.Threading;

namespace ElevatorSimulation
{
    public class Building
    {
        public int NumberOfElevators { get; set; }
        public int NumberOfFloors { get; set; }
        public Elevator[] Elevators { get; set; }

        protected ElevatorScheduler scheduler;
        public Floor[] Floors { get; set; }

        //the flag is used for debug.
        public bool Debug = false;

        private Random random;

        // create a background thread and keep on firing based on infinite while loop.
        private BackgroundWorker passengerSimulation = new BackgroundWorker();

        private void PassengerSimulation_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                CreateRequestRandomly();
                Thread.Sleep(1000);
            }
        }
        public Building(int numberOfElevators,int numberOfFloors)
        {
            this.NumberOfElevators = numberOfElevators;
            this.NumberOfFloors = numberOfFloors;
            this.Elevators = new Elevator[this.NumberOfElevators];
            for(int eleIndex = 0;eleIndex < numberOfElevators; eleIndex++)
                this.Elevators[eleIndex] = new Elevator(eleIndex, numberOfFloors);
            
            this.Floors = new Floor[this.NumberOfFloors];
            for(int flrIndex = 0;flrIndex < numberOfFloors; flrIndex++)
            {
                this.Floors[flrIndex] = new Floor();
            }
            random = new Random();
            passengerSimulation.DoWork += PassengerSimulation_DoWork;
            if (!Debug)
            {
                //passengerSimulation.RunWorkerAsync();
            }
            scheduler = new ElevatorScheduler(this.Elevators, this.Floors);
            //if (Debug)
            //{
            //    this.Floors[0].PassengerRequests.Enqueue(new PassengerRequest(0, 7));
            //    this.Floors[1].PassengerRequests.Enqueue(new PassengerRequest(1, 0));
            //    this.Floors[2].PassengerRequests.Enqueue(new PassengerRequest(2, 1));
            //    this.Floors[3].PassengerRequests.Enqueue(new PassengerRequest(3, 2));
            //    this.Floors[4].PassengerRequests.Enqueue(new PassengerRequest(4, 2));
            //    this.Floors[5].PassengerRequests.Enqueue(new PassengerRequest(5, 2));
            //    this.Floors[6].PassengerRequests.Enqueue(new PassengerRequest(6, 2));
            //    this.Floors[7].PassengerRequests.Enqueue(new PassengerRequest(7, 2));
            //    this.Floors[8].PassengerRequests.Enqueue(new PassengerRequest(8, 7));
            //    this.Floors[9].PassengerRequests.Enqueue(new PassengerRequest(9, 7));
            //    this.Floors[10].PassengerRequests.Enqueue(new PassengerRequest(10, 7));
            //    this.Floors[11].PassengerRequests.Enqueue(new PassengerRequest(11, 7));
            //    this.Floors[12].PassengerRequests.Enqueue(new PassengerRequest(12, 7));
            //    this.Floors[13].PassengerRequests.Enqueue(new PassengerRequest(13, 7));
            //    this.Floors[14].PassengerRequests.Enqueue(new PassengerRequest(14, 7));
            //    this.Floors[15].PassengerRequests.Enqueue(new PassengerRequest(15, 7));
            //}
            scheduler.StartScheduling();
        }
        //the following function will create each floor's destination floor requests randomly with random direction.
        private void CreateRequestRandomly()
        {
            for (int i = 0; i < Floors.Length; i++)
            {
                this.Floors[i].CreatePassengerRequests(this.NumberOfFloors,i);
            }
        }
    }
}
