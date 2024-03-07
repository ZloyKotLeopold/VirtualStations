using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VirtualStations
{
    internal class Program
    {
        static void Main()
        {
            int passangeres = 100;

            List<Passenger> passangers = new List<Passenger>();
            Station station = new Station();
            Random random = new Random();

            for (int i = 0; i < passangeres; i++)           
                passangers.Add(new Passenger(random.Next(1000, 5000)));     

            station.PrepareFlight(passangers);
            station.SendFlights();
        }
    }

    public class Station
    {
        private List<Passenger> _passengers;
        private List<Train> _trains;

        public Station()
        {
            _trains = new List<Train>();
        }

        public void PrepareFlight(List<Passenger> passengers)
        {
            bool trainsReady = true;

            _passengers = new List<Passenger>(passengers);

            Seller seller = new Seller();

            seller.SellTickets(_passengers);

            _passengers.RemoveAll(passanger => passanger.Ticket == null);

            _passengers = _passengers.OrderBy(passager => passager.Ticket.Route.StartingSity).ToList();

            TrainConfigurator trainConfigurator = new TrainConfigurator(_passengers);

            while (trainsReady)
            {
                Train train = new Train();

                train = trainConfigurator.ConfigureTrain();

                if (train.Route != null)
                {
                    _trains.Add(train);
                }
                else
                {
                    trainsReady = false;
                }
            }                                
        }

        public void SendFlights()
        {
            foreach (var train in _trains)
            {
                Console.WriteLine($"Поезд отправляется из {train.Route.StartingSity} в {train.Route.FinalSity}, время: {DateTime.Now}");

                Thread.Sleep(5000);
            }
        }
    }

    public class TrainConfigurator
    {
        private IWagon _wagon;
        private WagonFactory _wagonFactory;
        private List<Passenger> _passangers;

        public TrainConfigurator(List<Passenger> passangers)
        {
            _passangers = new List<Passenger>(passangers);
        }

        public Train ConfigureTrain()
        {
            Train train = new Train();

            IRoute route = null;

            bool isFull = true;

            while (isFull)
            {
                _wagon = SitDownPassengeres();

                if (_wagon != null)
                {
                    foreach (var passenger in _wagon.Passengers)
                    {
                        if (route == null)
                        {
                            route = passenger.Ticket.Route;

                            train.AddRoute(route);
                        }

                        if (passenger.Ticket.Route.StartingSity == route.StartingSity &&
                            passenger.Ticket.Route.FinalSity == route.FinalSity)
                        {
                            _passangers = _passangers.Except(_wagon.Passengers).ToList();

                            train.TryAddWagon(_wagon, route);                           
                        }

                        if (!passenger.Ticket.Route.StartingSity.Contains(route.StartingSity))
                            isFull = false;

                        break;
                    }
                }
                else
                {
                    isFull = false;
                }
            }

            return train;
        }

        private IWagon SitDownPassengeres()
        {
            IWagon wagon = null;
            IRoute route = null;

            foreach (var passenger in _passangers)
            {
                if (wagon == null)
                    wagon = CreateWagon(passenger);

                if (route == null)
                    route = passenger.Ticket.Route;

                if (route.StartingSity == passenger.Ticket.Route.StartingSity &&
                    route.FinalSity == passenger.Ticket.Route.FinalSity &&
                    wagon.WagonClass == passenger.Ticket.WagonClass)
                {
                    wagon.TryAddPassenger(passenger);
                }
            }

            return wagon;
        }

        private IWagon CreateWagon(Passenger passenger)
        {
            do
            {
                if (passenger.Ticket != null)
                {
                    switch (passenger.Ticket.WagonClass)
                    {
                        case WagonTypes.LuxClass:
                            _wagonFactory = new WagonLuxFactory();
                            break;

                        case WagonTypes.CoupeClass:
                            _wagonFactory = new WagonCoupeFactory();
                            break;

                        case WagonTypes.SecondClass:
                            _wagonFactory = new WagonSecondClassFactory();
                            break;
                    }
                }
            }
            while (_wagonFactory == null);



            return _wagonFactory.GetWagon();
        }
    }

    public class Seller
    {
        private ITicket _ticket;
        private Random _random;
        private TicketFactory _ticketFactory;
        private IReadOnlyList<Passenger> _passangers;

        public Seller()
        {
            _random = new Random();
        }

        public void SellTickets(List<Passenger> passangers)
        {
            _passangers = new List<Passenger>(passangers);

            foreach (Passenger passenger in _passangers)
            {
                SetRandomPath();

                _ticket = CreatRandomTicket();

                if (passenger.CanPay(_ticket.Route.Price))
                {
                    passenger.BuyTicket(_ticket, _ticket.Route.Price);
                }
            }

            _passangers = null;
        }

        private void SetRandomPath()
        {
            const int InitialPath = 1;
            const int FinalPath = 3;

            switch (_random.Next(InitialPath, FinalPath))
            {
                case 1:
                    _ticketFactory = new TicketOneFactory();
                    break;

                case 2:
                    _ticketFactory = new TicketTwoFactory();
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        private ITicket CreatRandomTicket()
        {
            const int MaxClass = 1;
            const int MinClass = 4;

            switch (_random.Next(MaxClass, MinClass))
            {
                case 1:
                    return _ticketFactory.CreateLuxTicket();
                case 2:
                    return _ticketFactory.CreateCoupeTicket();
                case 3:
                    return _ticketFactory.CreateSecondClassTicket();
                default:
                    return null;
            }
        }
    }

    public class Route : IRoute
    {
        public string StartingSity { get; private set; }
        public string FinalSity { get; private set; }

        public int Price { get; private set; }

        public IRoute CreateRoute(string startingSity, string finalSity, int price)
        {
            StartingSity = startingSity;
            FinalSity = finalSity;
            Price = price;

            return this;
        }
    }

    #region WAGON_FACTORY

    public class WagonTypes
    {
        public const string LuxClass = "Люкс вагон";
        public const string CoupeClass = "Купе вагон";
        public const string SecondClass = "Плацкартный вагон";
    }

    public class Wagon : IWagon
    {
        private List<Passenger> _passengers;
        protected string TypeWagon;

        public Wagon()
        {
            _passengers = new List<Passenger>();
        }

        public int MaxPassengers { get; protected set; }

        public string WagonClass => TypeWagon;

        public IReadOnlyCollection<Passenger> Passengers => _passengers;

        public bool TryAddPassenger(Passenger passenger)
        {
            if (_passengers.Count <= MaxPassengers)
            {
                _passengers.Add(passenger);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class WagonLux : Wagon
    {
        public WagonLux()
        {
            TypeWagon = WagonTypes.LuxClass;
            MaxPassengers = 12;
        }
    }

    public class WagonCoupe : Wagon
    {
        public WagonCoupe()
        {
            TypeWagon = WagonTypes.CoupeClass;
            MaxPassengers = 36;
        }
    }

    public class WagonSecondClass : Wagon
    {
        public WagonSecondClass()
        {
            TypeWagon = WagonTypes.SecondClass;
            MaxPassengers = 56;
        }
    }

    public class WagonLuxFactory : WagonFactory
    {
        public override IWagon GetWagon()
        {
            return new WagonLux();
        }
    }

    public class WagonCoupeFactory : WagonFactory
    {
        public override IWagon GetWagon()
        {
            return new WagonCoupe();
        }
    }

    public class WagonSecondClassFactory : WagonFactory
    {
        public override IWagon GetWagon()
        {
            return new WagonSecondClass();
        }
    }

    public abstract class WagonFactory
    {
        public abstract IWagon GetWagon();
    }

    #endregion

    public class Train
    {
        public const int MaxLengthTrain = 15;

        private List<IWagon> _wagons;

        public Train()
        {
            _wagons = new List<IWagon>();
        }

        public IRoute Route { get; private set; }

        public IReadOnlyCollection<IWagon> Wagons => _wagons;

        public void AddRoute(IRoute route)
        {
            if (Route == null)
                Route = route;
        }

        public bool TryAddWagon(IWagon wagon, IRoute route)
        {
            if (Route.StartingSity != route.StartingSity && Route.FinalSity != route.FinalSity)
                return false;

            if (_wagons.Count <= MaxLengthTrain && wagon != null)
            {
                _wagons.Add(wagon);

                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class Passenger
    {
        private int _money;

        public Passenger(int money)
        {
            _money = money;
        }

        public ITicket Ticket { get; private set; }

        public bool CanPay(int price) => _money >= price;

        public void BuyTicket(ITicket ticket, int price)
        {
            _money -= price;

            Ticket = ticket;
        }
    }

    #region TICKET_FACTORY
    public abstract class TicketFactory
    {
        public abstract ITicket CreateLuxTicket();

        public abstract ITicket CreateCoupeTicket();

        public abstract ITicket CreateSecondClassTicket();

        protected ITicket CreateTicket(string startingSity, string finalSity, string wagonClass, int price)
        {
            IRoute route = new Route();

            route = route.CreateRoute(startingSity, finalSity, price);

            return new Ticket(route, wagonClass);
        }
    }

    public class TicketOneFactory : TicketFactory
    {
        public override ITicket CreateLuxTicket()
        {
            return CreateTicket("Москва", "Казань", "Люкс вагон", 4000);
        }

        public override ITicket CreateCoupeTicket()
        {
            return CreateTicket("Москва", "Казань", "Купе вагон", 3000);
        }

        public override ITicket CreateSecondClassTicket()
        {
            return CreateTicket("Москва", "Казань", "Плацкартный вагон", 2000);
        }
    }

    public class TicketTwoFactory : TicketFactory
    {
        public override ITicket CreateCoupeTicket()
        {
            return CreateTicket("Пермь", "Мытищи", "Купе вагон", 3000);
        }

        public override ITicket CreateLuxTicket()
        {
            return CreateTicket("Пермь", "Мытищи", "Люкс вагон", 5000);
        }

        public override ITicket CreateSecondClassTicket()
        {
            return CreateTicket("Пермь", "Мытищи", "Плацкартный вагон", 4000);
        }
    }

    public class Ticket : ITicket
    {
        public Ticket(IRoute route, string wagonClass)
        {
            Route = route;
            WagonClass = wagonClass;
        }

        public IRoute Route { get; private set; }
        public string WagonClass { get; private set; }
    }

    #endregion

    public interface ITicket
    {
        IRoute Route { get; }
        string WagonClass { get; }
    }

    public interface IRoute
    {
        string StartingSity { get; }
        string FinalSity { get; }
        int Price { get; }

        IRoute CreateRoute(string startingSity, string finalSity, int price);
    }

    public interface IWagon
    {
        int MaxPassengers { get; }
        string WagonClass { get; }
        IReadOnlyCollection<Passenger> Passengers { get; }

        bool TryAddPassenger(Passenger passenger);
    }
}
