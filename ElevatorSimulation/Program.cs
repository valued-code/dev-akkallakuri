using System.Threading;

namespace ElevatorSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            var building = new Building(3, 16);
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
