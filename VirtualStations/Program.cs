using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualStations
{
    internal class Program
    {
        static void Main()
        {
        }

        
    }

    public class Car
    {
        private Passenger[] _passengers;
    }

    public class Train
    {
        private Car[] _cars;
    }

    public class Passenger
    {
        private int _money;
        private Ticket _ticket;
    }

    public class Route
    {
        private string _directed;
        private Train _train;
    }

    public class Ticket
    {
        private int _price;
        private Route _route;
    }

}
