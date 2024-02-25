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

        public Car(Passenger[] passengers)
        {
            _passengers = passengers;
        }
    }

    public class Train
    {
        private Car[] _cars;

        public Train(Car[] cars)
        {
            _cars = cars;
        }
    }

    public class Passenger
    {
        private int _money;
        private Ticket _ticket;

        public Passenger(int money)
        {
            _money = money;
        }
    }

    public class Route
    {
        private string _directed;
        private Train _train;

        public Route(string directed, Train train)
        {
            _directed = directed;
            _train = train;
        }
    }

    public class Ticket
    {
        private int _price;
        private Route _route;

        public Ticket(int price, Route route)
        {
            _price = price;
            _route = route;
        }
    }

}
