using System;
using System.IO;
using System.Text;

namespace MetroSystemAdjacentyListRevamp
{

    public class Node<T>
    {
        public T Data { get; set; }
        public Node<T> Next { get; set; }

        public Node(T data)
        {
            this.Data = data;
        }
    }

    public class LinkedList<T>
    {
        public Node<T> Head { get; private set; }
        public Node<T> Tail { get; private set; }
        public int Count { get; private set; }

        public void Add(T data)
        {
            Node<T> node = new Node<T>(data);

            if (Head == null)
            {
                Head = node;
                Tail = node;
            }
            else
            {
                Tail.Next = node;
                Tail = node;
            }

            Count++;
        }

        public Node<T> Find(Func<T, bool> predicate)
        {
            Node<T> current = Head;
            while (current != null)
            {
                if (predicate(current.Data))
                    return current;
                current = current.Next;
            }
            return null;
        }
        public void ForEach(Action<T> action)
        {
            Node<T> current = Head;
            while (current != null)
            {
                action(current.Data);
                current = current.Next;
            }
        }
    }

    public class Station : IComparable<Station>
    {
        public string Name { get; }
        public LinkedList<StationConnection> Connections { get; }
        public string Line { get; }
        public bool IsOpen { get; set; }
        public int MinTravelTime { get; set; }  // For Dijkstra's algorithm
        public Station PreviousStation { get; set; }  // For Dijkstra's algorithm

        public Station(string name, string line)
        {
            Name = name;
            Connections = new LinkedList<StationConnection>();
            Line = line;
            IsOpen = true;
            MinTravelTime = 99999999;
            PreviousStation = null;
        }

        public int CompareTo(Station other)
        {
            return MinTravelTime.CompareTo(other.MinTravelTime);
        }
    }

    public class Line
    {
        public string Name { get; }
        public Line PreviousLine { get; set; }  // For Dijkstra's algorithm

        public Line(string name)
        {
            Name = name;
            PreviousLine = null;
        }
    }

    public class StationConnection
    {
        public Station Station { get; }
        public int TravelTime { get; set; }
        public int OriginalTravelTime { get; }

        public StationConnection(Station station, int travelTime)
        {
            Station = station;
            TravelTime = travelTime;
            OriginalTravelTime = travelTime;
        }
    }
    public class DijkstraResult
    {
        public string Path { get; set; }
        public int TotalJourneyTime { get; set; }
    }


    public class PriorityQueue<T> where T : IComparable<T>
    {
        private T[] data;
        private int count;

        public PriorityQueue(int capacity)
        {
            data = new T[capacity];
            count = 0;
        }

        public void Enqueue(T item)
        {
            if (count == data.Length)
                throw new Exception("Queue is full.");

            data[count] = item;
            count++;
        }

        public T DequeueMin()
        {
            int minIndex = 0;
            for (int i = 1; i < count; i++)
            {
                if (data[i].CompareTo(data[minIndex]) < 0)
                    minIndex = i;
            }

            T minItem = data[minIndex];
            data[minIndex] = data[count - 1];
            count--;

            return minItem;
        }

        public int Count
        {
            get { return count; }
        }
    }
    public class MetroSystem
    {
        private const int MAX_TRAVEL_TIME = 99999999;  // large number to represent infinity, for Dijkstra's algorithm
        private LinkedList<Line> lines;
        private LinkedList<Station> stations;
        private LinkedList<StationConnection> originalTravelTimes;

        public MetroSystem(string inputData, bool isCSV)
        {
            lines = new LinkedList<Line>();
            stations = new LinkedList<Station>();
            originalTravelTimes = new LinkedList<StationConnection>();

            if (isCSV)
            {
                LoadDataFromCSV(inputData);
            }
            else
            {
                LoadDataManually(inputData);
            }
        }

        private void LoadDataFromCSV(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] data = line.Split(',');

                    // Create stations and connections
                    Station station1 = AddStation(data[1], data[0]);
                    Station station2 = AddStation(data[2], data[0]);
                    AddConnection(station1, station2, int.Parse(data[3]));

                    AddChangeStations(station1);
                    AddChangeStations(station2);
                    AddLine(data[0]);
                }
            }
        }

        private void LoadDataManually(string inputData)
        {
            string[] inputLines = inputData.Split('\n');
            foreach (string line in inputLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;  // This line is added

                string[] data = line.Split(',');

                // Create stations and connections
                Station station1 = AddStation(data[1], data[0]);
                Station station2 = AddStation(data[2], data[0]);
                AddConnection(station1, station2, int.Parse(data[3]));

                AddChangeStations(station1);
                AddChangeStations(station2);
                AddLine(data[0]);
            }
        }

        private Line AddLine(string name)
        {
            // Check if the line already exists
            Node<Line> current = lines.Head;
            while (current != null)
            {
                if (current.Data.Name == name)
                {
                    return current.Data;
                }

                current = current.Next;
            }

            // If the line does not exist, create a new one
            Line line = new Line(name);
            lines.Add(line);

            return line;
        }
        private Station AddStation(string name, string lineName)
        {
            // Check if the station already exists
            Node<Station> current = stations.Head;
            while (current != null)
            {
                if (current.Data.Name == name && current.Data.Line == lineName)
                {
                    // The station exists, but we need to add the new line to it
                    return current.Data;
                }

                current = current.Next;
            }

            // If the station does not exist, create a new one and add the line to it
            Station station = new Station(name, lineName);
            stations.Add(station);

            return station;
        }


        private void AddConnection(Station station1, Station station2, int travelTime)
        {
            // Add connection from station1 to station2
            StationConnection connection1 = new StationConnection(station2, travelTime);
            station1.Connections.Add(connection1);

            // Add connection from station2 to station1
            StationConnection connection2 = new StationConnection(station1, travelTime);
            station2.Connections.Add(connection2);

            // Save the original travel times for both connections
            originalTravelTimes.Add(new StationConnection(station2, travelTime));
            originalTravelTimes.Add(new StationConnection(station1, travelTime));
        }

        private void AddChangeStations(Station station)
        {
            Node<Station> current = stations.Head;
            while (current != null)
            {
                //if there is a station with same name and different line, add it to the current station with 0 travel time
                if (current.Data.Name == station.Name && current.Data.Line != station.Line)
                {
                    AddConnection(station, current.Data, 0);
                }

                current = current.Next;
            }

        }

        public void PrintPath(Station station, StringBuilder journeyDetails, ref int step)
        {
            if (station == null)
                return;

            // Base case: if this is the source station, print it and return
            if (station.PreviousStation == null)
            {
                journeyDetails.AppendLine($"({step++}) Start: {station.Name}");
                return;
            }

            // Recursive call with the previous station
            PrintPath(station.PreviousStation, journeyDetails, ref step);

            // If we changed lines
            if (station.Name == station.PreviousStation.Name && station.Line != station.PreviousStation.Line)
            {
                journeyDetails.AppendLine($"({step++}) Change: {station.PreviousStation.Name} ({station.PreviousStation.Line}) to {station.Name} ({station.Line})");
                return;
            }

            int travelTime = station.MinTravelTime - station.PreviousStation.MinTravelTime;

            journeyDetails.AppendLine($"({step++})\t{station.PreviousStation.Name} ({station.PreviousStation.Line}) to {station.Name} ({station.Line}) \t({travelTime} mins)");
        }


        public DijkstraResult Dijkstra(string startStationName, string startStationLine, string endStationName, string endStationLine)
        {
            StringBuilder journeyDetails = new StringBuilder();
            int step = 1;

            // Find the start and end stations
            Node<Station> startStationNode = stations.Find(station => station.Name == startStationName && station.Line == startStationLine);
            Node<Station> endStationNode = stations.Find(station => station.Name == endStationName && station.Line == endStationLine);

            if (startStationNode == null || endStationNode == null)
            {
                return new DijkstraResult()
                {
                    Path = "Invalid station name.",
                    TotalJourneyTime = MAX_TRAVEL_TIME
                };
            }

            Station startStation = startStationNode.Data;
            Station endStation = endStationNode.Data;

            // Initialize Dijkstra's algorithm
            stations.ForEach(node =>
            {
                node.MinTravelTime = MAX_TRAVEL_TIME;
                node.PreviousStation = null;
            });

            startStation.MinTravelTime = 0;

            // Create a priority queue
            PriorityQueue<Station> priorityQueue = new PriorityQueue<Station>(stations.Count);
            stations.ForEach(node =>
            {
                priorityQueue.Enqueue(node);
            });

            while (priorityQueue.Count > 0)
            {
                // Get the station with the minimum travel time
                Station station = priorityQueue.DequeueMin();

                // Ignore closed stations
                if (!station.IsOpen)
                {
                    continue;
                }

                // Update travel times for neighboring stations
                station.Connections.ForEach(connection =>
                {
                    Station neighborStation = connection.Station;
                    int newTravelTime = station.MinTravelTime + connection.TravelTime;

                    if (newTravelTime < neighborStation.MinTravelTime)
                    {
                        neighborStation.MinTravelTime = newTravelTime;
                        neighborStation.PreviousStation = station;
                    }
                });
            }

            // Construct the shortest path
            if (endStation.MinTravelTime == MAX_TRAVEL_TIME)
            {
                return new DijkstraResult()
                {
                    Path = $"No route found from {startStationName} to {endStationName}.",
                    TotalJourneyTime = MAX_TRAVEL_TIME
                };
            }
            else
            {
                if (endStation.MinTravelTime != MAX_TRAVEL_TIME)
                {
                    journeyDetails.AppendLine("Shortest path from " + startStationName + " to " + endStationName + ": ");
                    PrintPath(endStation, journeyDetails, ref step);
                    journeyDetails.AppendLine($"({step}) End: {endStation.Name}");
                    journeyDetails.AppendLine("Total Journey Time: " + endStation.MinTravelTime + " minutes");
                }

                return new DijkstraResult()
                { Path = journeyDetails.ToString(), TotalJourneyTime = endStation.MinTravelTime };
            }
        }

        public void FindFastestRoute(string startStationName, string endStationName)
        {
            DijkstraResult minRoute = null;


            GetAllStationCombinations(startStationName, endStationName).ForEach(node =>
            {
                var pair = node.Split('~');
                var stationFrom = pair[0].Split('(')[0].Trim();
                var lineFrom = pair[0].Split('(')[1].Trim().Trim(')');
                var stationTo = pair[1].Split('(')[0].Trim();
                var lineTo = pair[1].Split('(')[1].Trim().Trim(')');

                var route = Dijkstra(stationFrom, lineFrom, stationTo, lineTo);

                if (route.TotalJourneyTime != MAX_TRAVEL_TIME && (minRoute == null || route.TotalJourneyTime < minRoute.TotalJourneyTime) && !route.Path.Contains("(2) Change"))
                {
                    minRoute = route;
                }
            });

            if (minRoute != null)
            {
                Console.WriteLine(minRoute.Path);
            }
            else
            {
                Console.WriteLine("No valid route found.");
            }
        }

        public LinkedList<string> GetAllStationCombinations(string startStationName, string endStationName)
        {
            LinkedList<string> startStationCombinations = new LinkedList<string>();
            LinkedList<string> endStationCombinations = new LinkedList<string>();

            // Iterate through each station and line, looking for stations that match startStationName and endStationName
            stations.ForEach(stationNode =>
            {
                lines.ForEach(line =>
                {
                    string stationWithLine = stationNode.Name + " (" + line.Name + ")";
                    if (stationNode.Name.Contains(startStationName) && stationNode.Line == line.Name && startStationCombinations.Find(station => station == stationWithLine) == null)
                        startStationCombinations.Add(stationWithLine);

                    if (stationNode.Name.Contains(endStationName) && stationNode.Line == line.Name && endStationCombinations.Find(station => station == stationWithLine) == null)
                        endStationCombinations.Add(stationWithLine);
                });
            });

            LinkedList<string> allCombinations = new LinkedList<string>();

            // Create all combinations of startStationCombinations and endStationCombinations
            startStationCombinations.ForEach(startStation =>
            {
                endStationCombinations.ForEach(endStation =>
                {
                    string combination = startStation + " ~ " + endStation;
                    allCombinations.Add(combination);
                });
            });

            return allCombinations;
        }

        public void UpdateTravelTime(string startStationName, string endStationName, int newTime, bool isDelay)
        {
            GetAllStationCombinations(startStationName, endStationName).ForEach(node =>
            {
                var pair = node.Split('~');
                var stationFrom = pair[0].Split('(')[0].Trim();
                var lineFrom = pair[0].Split('(')[1].Trim().Trim(')');
                var stationTo = pair[1].Split('(')[0].Trim();
                var lineTo = pair[1].Split('(')[1].Trim().Trim(')');


                // Find the two stations
                Node<Station> stationNodeFrom = stations.Find(station => station.Name == stationFrom && station.Line == lineFrom);
                Node<Station> stationNodeTo = stations.Find(station => station.Name == stationTo && station.Line == lineTo);

                if (stationNodeFrom == null || stationNodeTo == null)
                {
                    Console.WriteLine("Invalid station name.");
                    return;
                }

                // Find the connections from station1 to station2 and from station2 to station1
                Node<StationConnection> connectionNode1 = stationNodeFrom.Data.Connections.Find(connection => connection.Station == stationNodeTo.Data);
                Node<StationConnection> connectionNode2 = stationNodeTo.Data.Connections.Find(connection => connection.Station == stationNodeFrom.Data);
                if (connectionNode1 == null || connectionNode2 == null)
                {
                    Console.WriteLine($"No connection from {stationFrom} ({lineFrom}) to {stationTo} ({lineTo}) or vice versa.");
                    return;
                }

                // Update the travel times
                if (isDelay)
                {
                    // Add the delay to the current travel times
                    connectionNode1.Data.TravelTime += newTime;
                    connectionNode2.Data.TravelTime += newTime;
                }
                else
                {
                    // Set the new travel times
                    connectionNode1.Data.TravelTime = newTime;
                    connectionNode2.Data.TravelTime = newTime;
                }

                Console.WriteLine($"Travel time from {stationFrom} ({lineFrom}) to {stationTo} ({lineTo}) and vice versa updated to {connectionNode1.Data.TravelTime} minutes.");
            });
        }

        public void SetRouteStatus(string startStationName, string endStationName, bool isOpen)
        {
            GetAllStationCombinations(startStationName, endStationName).ForEach(node =>
            {
                var pair = node.Split('~');
                var stationFrom = pair[0].Split('(')[0].Trim();
                var lineFrom = pair[0].Split('(')[1].Trim().Trim(')');
                var stationTo = pair[1].Split('(')[0].Trim();
                var lineTo = pair[1].Split('(')[1].Trim().Trim(')');

                // Find the two stations
                Node<Station> stationNodeFrom = stations.Find(station => station.Name == stationFrom && station.Line == lineFrom);
                Node<Station> stationNodeTo = stations.Find(station => station.Name == stationTo && station.Line == lineTo);

                if (stationNodeFrom == null || stationNodeTo == null)
                {
                    Console.WriteLine("Invalid station name.");
                    return;
                }

                // Find the connections from station1 to station2 and from station2 to station1
                Node<StationConnection> connectionNode1 = stationNodeFrom.Data.Connections.Find(connection => connection.Station == stationNodeTo.Data);
                Node<StationConnection> connectionNode2 = stationNodeTo.Data.Connections.Find(connection => connection.Station == stationNodeFrom.Data);
                if (connectionNode1 == null || connectionNode2 == null)
                {
                    Console.WriteLine($"No connection from {stationFrom} ({lineFrom}) to {stationTo} ({lineTo}) or vice versa.");
                    return;
                }

                // Update the route statuses
                stationNodeFrom.Data.IsOpen = isOpen;
                stationNodeTo.Data.IsOpen = isOpen;

                Console.WriteLine($"Station {stationFrom} ({lineFrom}) to {stationTo} ({lineTo}) are now {(isOpen ? "open" : "closed")}.");
            });
        }

        public void ResetTravelTime(string startStationName, string endStationName)
        {
            GetAllStationCombinations(startStationName, endStationName).ForEach(node =>
            {
                var pair = node.Split('~');
                var stationFrom = pair[0].Split('(')[0].Trim();
                var lineFrom = pair[0].Split('(')[1].Trim().Trim(')');
                var stationTo = pair[1].Split('(')[0].Trim();
                var lineTo = pair[1].Split('(')[1].Trim().Trim(')');

                // Find the two stations
                Node<Station> stationNodeFrom = stations.Find(station => station.Name == stationFrom && station.Line == lineFrom);
                Node<Station> stationNodeTo = stations.Find(station => station.Name == stationTo && station.Line == lineTo);

                if (stationNodeFrom == null || stationNodeTo == null)
                {
                    Console.WriteLine("Invalid station name.");
                    return;
                }

                // Find the connection from station1 to station2
                Node<StationConnection> connectionNode = stationNodeFrom.Data.Connections.Find(connection => connection.Station == stationNodeTo.Data);
                if (connectionNode == null)
                {
                    Console.WriteLine($"No connection from {stationFrom} ({lineFrom}) to {stationTo} ({lineTo}).");
                    return;
                }

                // Reset the travel time to the original travel time
                connectionNode.Data.TravelTime = connectionNode.Data.OriginalTravelTime;

                Console.WriteLine($"Travel time from {stationFrom} ({lineFrom}) to {stationTo} ({lineTo}) reset to {connectionNode.Data.TravelTime} minutes.");
            });
        }

        public void PrintClosedRoutes()
        {
            Console.WriteLine("Closed routes:");
            Node<Station> stationNode = stations.Head;

            while (stationNode != null)
            {
                Node<StationConnection> connectionNode = stationNode.Data.Connections.Head;

                while (connectionNode != null)
                {
                    if (!stationNode.Data.IsOpen && !connectionNode.Data.Station.IsOpen) // Check if either station is closed
                    {
                        Console.WriteLine($"{stationNode.Data.Name} - {connectionNode.Data.Station.Name}");
                    }

                    connectionNode = connectionNode.Next;
                }

                stationNode = stationNode.Next;
            }
        }


        public void PrintDelayedRoutes()
        {
            Console.WriteLine("Delayed routes:");
            Node<Station> stationNode = stations.Head;

            while (stationNode != null)
            {
                Node<StationConnection> connectionNode = stationNode.Data.Connections.Head;

                while (connectionNode != null)
                {
                    if (connectionNode.Data.TravelTime > connectionNode.Data.OriginalTravelTime)
                    {
                        Console.WriteLine($"{stationNode.Data.Name} - {connectionNode.Data.Station.Name} : {connectionNode.Data.OriginalTravelTime} min now {connectionNode.Data.TravelTime} min");
                    }

                    connectionNode = connectionNode.Next;
                }

                stationNode = stationNode.Next;
            }
        }

    }

    public class Program
    {
        static void Main(string[] args)
        {
            // Welcome message
            Console.WriteLine("Welcome to Metro System!");

            Console.WriteLine("Enter the metro data: ");
            Console.WriteLine("1. Manually");
            Console.WriteLine("2. From CSV file");
            string choice = Console.ReadLine();
            MetroSystem metroSystem = null;

            // User enters metro data manually
            if (choice == "1")
            {
                Console.WriteLine("Enter metro data in this format - zone,station1,station2,distance");
                Console.WriteLine("When you're done, enter 'end'");
                string input = "";
                string line;

                while ((line = Console.ReadLine()) != "end")
                {
                    input += line + '\n';
                }

                // Create a new MetroSystem instance with the manually entered data
                metroSystem = new MetroSystem(input, false);
            }
            // User enters metro data from a CSV file
            else if (choice == "2")
            {
                Console.WriteLine("Enter the path of the CSV file: ");
                string path = Console.ReadLine();

                // Create a new MetroSystem instance with the data from the CSV file
                metroSystem = new MetroSystem(path, true);
            }

            // Infinite loop for the metro operations
            while (true)
            {
                Console.WriteLine("Choose an operation: ");
                Console.WriteLine("1. Find fastest route");
                Console.WriteLine("2. Update travel time");
                Console.WriteLine("3. Set route status");
                Console.WriteLine("4. Reset travel time");
                Console.WriteLine("5. Print closed routes");
                Console.WriteLine("6. Print delayed routes");
                Console.WriteLine("7. Exit");
                string operation = Console.ReadLine();

                switch (operation)
                {
                    // Find the fastest route between two stations
                    case "1":
                        Console.WriteLine("Enter the start station: ");
                        string startStation = Console.ReadLine();

                        Console.WriteLine("Enter the end station: ");
                        string endStation = Console.ReadLine();

                        metroSystem.FindFastestRoute(startStation, endStation);
                        break;

                    // Update the travel time between two stations
                    case "2":
                        Console.WriteLine("Enter the first station: ");
                        string station1 = Console.ReadLine();

                        Console.WriteLine("Enter the second station: ");
                        string station2 = Console.ReadLine();

                        Console.WriteLine("Enter the new travel time: ");
                        int newTime = int.Parse(Console.ReadLine());

                        metroSystem.UpdateTravelTime(station1, station2, newTime, true);
                        break;

                    // Set the status of a route (open or closed)
                    case "3":
                        Console.WriteLine("Enter the first station: ");
                        station1 = Console.ReadLine();

                        Console.WriteLine("Enter the second station: ");
                        station2 = Console.ReadLine();

                        Console.WriteLine("Is the route open? (yes/no)");
                        bool isOpen = (Console.ReadLine().ToLower() == "yes");

                        metroSystem.SetRouteStatus(station1, station2, isOpen);
                        break;

                    // Reset the travel time between two stations
                    case "4":
                        Console.WriteLine("Enter the first station: ");
                        station1 = Console.ReadLine();

                        Console.WriteLine("Enter the second station: ");
                        station2 = Console.ReadLine();

                        metroSystem.ResetTravelTime(station1, station2);
                        break;

                    // Print the list of closed routes
                    case "5":
                        metroSystem.PrintClosedRoutes();
                        break;

                    // Print the list of delayed routes
                    case "6":
                        metroSystem.PrintDelayedRoutes();
                        break;

                    // Exit the program
                    case "7":
                        return;

                    default:
                        Console.WriteLine("Invalid operation. Please choose 1, 2, 3, 4, 5, 6, or 7.");
                        break;
                }
            }
        }
    }
}
