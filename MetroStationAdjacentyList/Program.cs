using System;
using System.IO;
using System.Text;

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
    public string Zone { get; }
    public LinkedList<StationConnection> Connections { get; }
    public bool IsOpen { get; set; }
    public int MinTravelTime { get; set; }  // For Dijkstra's algorithm
    public Station PreviousStation { get; set; }  // For Dijkstra's algorithm

    public Station(string name, string zone)
    {
        Name = name;
        Zone = zone;
        Connections = new LinkedList<StationConnection>();
        IsOpen = true;
        MinTravelTime = int.MaxValue;
        PreviousStation = null;
    }

    public int CompareTo(Station other)
    {
        return MinTravelTime.CompareTo(other.MinTravelTime);
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
    private LinkedList<Station> stations;
    private LinkedList<StationConnection> originalTravelTimes;

    public MetroSystem(string inputData, bool isCSV)
    {
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
            }
        }
    }

    private void LoadDataManually(string inputData)
    {
        string[] lines = inputData.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;  // This line is added

            string[] data = line.Split(',');

            // Create stations and connections
            Station station1 = AddStation(data[1], data[0]);
            Station station2 = AddStation(data[2], data[0]);
            AddConnection(station1, station2, int.Parse(data[3]));
        }
    }


    private Station AddStation(string name, string zone)
    {
        // Check if the station already exists
        Node<Station> current = stations.Head;
        while (current != null)
        {
            if (current.Data.Name == name && current.Data.Zone == zone)
            {
                return current.Data;
            }

            current = current.Next;
        }

        // If the station does not exist, create a new one
        Station station = new Station(name, zone);
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

    public void FindFastestRoute(string startStationName, string endStationName)
    {
        // Find the start and end stations
        Node<Station> startStationNode = stations.Find(station => station.Name == startStationName);
        Node<Station> endStationNode = stations.Find(station => station.Name == endStationName);

        if (startStationNode == null || endStationNode == null)
        {
            Console.WriteLine("Invalid station name.");
            return;
        }

        Station startStation = startStationNode.Data;
        Station endStation = endStationNode.Data;

        // Initialize Dijkstra's algorithm
        stations.ForEach(node =>
        {
            node.MinTravelTime = int.MaxValue;
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

            // Ignore closed stations and stations in different zones
            if (!station.IsOpen || station.Zone != startStation.Zone)
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

        // Print the shortest path
        if (endStation.MinTravelTime == int.MaxValue)
        {
            Console.WriteLine($"No route found from {startStationName} to {endStationName}.");
        }
        else
        {
            string path = endStation.Name;
            Station station = endStation.PreviousStation;

            while (station != null)
            {
                path = station.Name + " -> " + path;
                station = station.PreviousStation;
            }

            Console.WriteLine($"Shortest route from {startStationName} to {endStationName}: {path}");
            Console.WriteLine($"Total Journey Time: {endStation.MinTravelTime} minutes");
        }
    }

    public void UpdateTravelTime(string station1, string station2, int newTime, bool isDelay)
    {
        // Find the two stations
        Node<Station> stationNode1 = stations.Find(station => station.Name == station1);
        Node<Station> stationNode2 = stations.Find(station => station.Name == station2);

        if (stationNode1 == null || stationNode2 == null)
        {
            Console.WriteLine("Invalid station name.");
            return;
        }

        // Find the connections from station1 to station2 and from station2 to station1
        Node<StationConnection> connectionNode1 = stationNode1.Data.Connections.Find(connection => connection.Station == stationNode2.Data);
        Node<StationConnection> connectionNode2 = stationNode2.Data.Connections.Find(connection => connection.Station == stationNode1.Data);
        if (connectionNode1 == null || connectionNode2 == null)
        {
            Console.WriteLine($"No connection from {station1} to {station2} or vice versa.");
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

        Console.WriteLine($"Travel time from {station1} to {station2} and vice versa updated to {connectionNode1.Data.TravelTime} minutes.");
    }

    public void SetRouteStatus(string station1, string station2, bool isOpen)
    {
        // Find the two stations
        Node<Station> stationNode1 = stations.Find(station => station.Name == station1);
        Node<Station> stationNode2 = stations.Find(station => station.Name == station2);

        if (stationNode1 == null || stationNode2 == null)
        {
            Console.WriteLine("Invalid station name.");
            return;
        }

        // Update the route statuses
        stationNode1.Data.IsOpen = isOpen;
        stationNode2.Data.IsOpen = isOpen;

        Console.WriteLine($"Station {station1} and {station2} are now {(isOpen ? "open" : "closed")}.");
    }



    public void ResetTravelTime(string station1, string station2)
    {
        // Find the two stations
        Node<Station> stationNode1 = stations.Find(station => station.Name == station1);
        Node<Station> stationNode2 = stations.Find(station => station.Name == station2);

        if (stationNode1 == null || stationNode2 == null)
        {
            Console.WriteLine("Invalid station name.");
            return;
        }

        // Find the connection from station1 to station2
        Node<StationConnection> connectionNode = stationNode1.Data.Connections.Find(connection => connection.Station == stationNode2.Data);
        if (connectionNode == null)
        {
            Console.WriteLine($"No connection from {station1} to {station2}.");
            return;
        }

        // Reset the travel time to the original travel time
        connectionNode.Data.TravelTime = connectionNode.Data.OriginalTravelTime;

        Console.WriteLine($"Travel time from {station1} to {station2} reset to {connectionNode.Data.TravelTime} minutes.");
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

