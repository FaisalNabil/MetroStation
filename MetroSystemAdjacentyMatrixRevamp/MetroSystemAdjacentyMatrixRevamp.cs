using System;
using System.IO;
using System.Text;

public class MetroSystem
{
    private int[,] graph;  // 2D array to represent metro routes as a graph
    private string[] stations;  // array to store unique station names
    private string[] stationLines;  // array to store corresponding line names for each station
    private int[,] temporaryTravelTimes;  // 2D array to store temporary travel times between stations
    private const int MAX_TRAVEL_TIME = 99999999;  // large number to represent infinity, for Dijkstra's algorithm
    private int step;  // step count for logging journey details

    public MetroSystem(string input, bool isFilePath)
    {
        // The inputData can either be a CSV file path or raw string data. If it's a file path, read all lines from the file.
        // If it's raw string data, split it by newline characters. 
        // If the last line is "end" or an empty line, remove it from the data.
        string[] lines;
        if (isFilePath)
        {
            if (Path.GetExtension(input) != ".csv")
            {
                throw new ArgumentException("File must be a .csv file");
            }
            lines = File.ReadAllLines(input);
        }
        else
        {
            lines = input.Split('\n');
            if (lines[lines.Length - 1] == "end" || lines[lines.Length - 1] == "")
            {
                Array.Resize(ref lines, lines.Length - 1);
            }
        }

        // Temporary arrays to hold all stations and corresponding line names as they are parsed from the data.
        // They are twice as large as the number of lines, because each line contains 2 stations.
        string[] tempStations = new string[lines.Length * 2];
        string[] tempStationLines = new string[lines.Length * 2];
        int count = 0;

        // Parse all lines from the data. Each line should be in the format: "<lineName>,<station1>,<station2>,<travelTime>".
        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            string line = parts[0];
            string station1 = parts[1] + " (" + parts[0] + ")";
            string station2 = parts[2] + " (" + parts[0] + ")";

            // Check if the stations already exist in the temporary array. If not, add them and increase the count of unique stations.
            bool found1 = false, found2 = false;
            for (int j = 0; j < count; j++)
            {
                if (tempStations[j] == station1) found1 = true;
                if (tempStations[j] == station2) found2 = true;
                if (found1 && found2) break;
            }

            if (!found1)
            {
                tempStations[count] = station1;
                tempStationLines[count] = line;
                count++;
            }
            if (!found2)
            {
                tempStations[count] = station2;
                tempStationLines[count] = line;
                count++;
            }
        }

        stations = new string[count];
        stationLines = new string[count];
        Array.Copy(tempStations, stations, count);
        Array.Copy(tempStationLines, stationLines, count);

        graph = new int[count, count];
        temporaryTravelTimes = new int[count, count];

        // Initialize all edges in the graph to maximum travel time.
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                graph[i, j] = MAX_TRAVEL_TIME;
                temporaryTravelTimes[i, j] = -1;
            }
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            string station1 = parts[1] + " (" + parts[0] + ")"; ;
            string station2 = parts[2] + " (" + parts[0] + ")"; ;
            int travelTime = int.Parse(parts[3]);
            int index1 = Array.IndexOf(stations, station1);
            int index2 = Array.IndexOf(stations, station2);

            graph[index1, index2] = travelTime;
            graph[index2, index1] = travelTime;
        }

        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                if (stations[i].Split('(')[0] == stations[j].Split('(')[0])
                {
                    if (stations[i].Split('(')[1] != stations[j].Split('(')[1])
                    {
                        graph[i, j] = 0;
                        graph[j, i] = 0;
                    }
                }
            }
        }
    }


    // To get station index by name
    private int GetStationIndex(string stationName)
    {
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] == stationName)
                return i;
        }
        return -1; // not found
    }

    // Find the vertex with minimum distance value, from the set of vertices not yet included in shortest path tree
    private int MinDistance(int[] dist, bool[] sptSet)
    {
        // Initialize minimum value
        int min = MAX_TRAVEL_TIME, min_index = -1;

        for (int v = 0; v < stations.Length; v++)
            if (sptSet[v] == false && dist[v] <= min)
            {
                min = dist[v];
                min_index = v;
            }

        return min_index;
    }

    // Print the shortest path from src to target using parent array
    public void PrintPath(int[] parent, int stationIndex, int src, StringBuilder journeyDetails)
    {
        if (stationIndex == src)
        {
            //Console.WriteLine($"({step++}) Start: {stations[src]} ({GetStationLine(src)})");
            journeyDetails.AppendLine($"({step++}) Start: {stations[src]}");
            return;
        }

        PrintPath(parent, parent[stationIndex], src, journeyDetails);

        // Correctly log the line changes
        //if (GetStationLine(parent[stationIndex]) != GetStationLine(stationIndex))
        if (stations[parent[stationIndex]].Split('(')[0] == stations[stationIndex].Split('(')[0] && stations[parent[stationIndex]].Split('(')[1] != stations[stationIndex].Split('(')[1])
        {
            //Console.WriteLine($"({step++}) Change: {stations[parent[stationIndex]]} ({GetStationLine(parent[stationIndex])}) to {stations[stationIndex]} ({GetStationLine(stationIndex)})");
            journeyDetails.AppendLine($"({step++}) Change: {stations[parent[stationIndex]]} to {stations[stationIndex]}");
            return;
        }

        int travelTime = (temporaryTravelTimes[parent[stationIndex], stationIndex] != -1 &&
            temporaryTravelTimes[parent[stationIndex], stationIndex] != MAX_TRAVEL_TIME) ?
            temporaryTravelTimes[parent[stationIndex], stationIndex] : graph[parent[stationIndex], stationIndex];

        //Console.WriteLine($"({step++})\t{stations[parent[stationIndex]]} ({GetStationLine(parent[stationIndex])}) to {stations[stationIndex]} ({GetStationLine(stationIndex)})\t({graph[parent[stationIndex], stationIndex]} mins)");
        journeyDetails.AppendLine($"({step++})\t{stations[parent[stationIndex]]} to {stations[stationIndex]} \t({travelTime} mins)");
    }

    // Implement Dijkstra's algorithm using adjacency matrix
    public void Dijkstra(string startStation, string endStation, string[][] allCombinationsTravelTimes, int index)
    {
        // Get the indices of the start and end stations in the 'stations' array
        int src = GetStationIndex(startStation);
        int dest = GetStationIndex(endStation);

        // Initialize the arrays to hold the shortest distances from the source to each station (dist), 
        // whether each station is included in the shortest path tree (sptSet), 
        // and the parent station in the shortest path from the source to each station (parent)
        int[] dist = new int[stations.Length];
        bool[] sptSet = new bool[stations.Length];
        int[] parent = new int[stations.Length];

        // Set initial values: parent[i] to -1 as there's no path yet, dist[i] to MAX_TRAVEL_TIME as initial distance is considered infinite, and sptSet[i] to false as no stations are included in the path yet
        for (int i = 0; i < stations.Length; i++)
        {
            parent[i] = -1;
            dist[i] = MAX_TRAVEL_TIME;
            sptSet[i] = false;
        }

        // Distance from the source to itself is always 0
        dist[src] = 0;

        // Main loop of Dijkstra's algorithm. This will run for every station except for the last one
        for (int count = 0; count < stations.Length - 1; count++)
        {
            // Pick the station with the minimum distance value, from the set of stations not yet included in the shortest path tree
            int u = MinDistance(dist, sptSet);

            // Include the picked station into the shortest path tree
            sptSet[u] = true;

            // Update dist[v] only for the stations v that are not in the shortest path tree, have an edge from u, 
            // and have the same name as u (ignoring leading/trailing white spaces), and total weight of path from src to v through u is smaller than current value of dist[v]
            for (int v = 0; v < stations.Length; v++)
            {
                if (!sptSet[v])
                {
                    if (temporaryTravelTimes[u, v] != -1 && temporaryTravelTimes[u, v] != MAX_TRAVEL_TIME)
                    {
                        if (dist[u] + temporaryTravelTimes[u, v] < dist[v])
                        {
                            // If the new path is shorter, update the distance and set u as v's parent
                            parent[v] = u;
                            dist[v] = dist[u] + temporaryTravelTimes[u, v];
                        }
                    }
                    else
                    {
                        //temporaryTravelTimes max value means line is closed
                        if (dist[u] + graph[u, v] < dist[v] && temporaryTravelTimes[u, v] != MAX_TRAVEL_TIME)
                        {
                            // If the new path is shorter, update the distance and set u as v's parent
                            parent[v] = u;
                            dist[v] = dist[u] + graph[u, v];
                        }
                    }
                }
            }
        }

        StringBuilder journeyDetails = new StringBuilder();
        if (dist[dest] != MAX_TRAVEL_TIME)
        {
            journeyDetails.AppendLine("Shortest path from " + startStation + " to " + endStation + ": ");
            step = 1;
            PrintPath(parent, dest, src, journeyDetails);
            journeyDetails.AppendLine($"({step}) End: {stations[dest]}");
            journeyDetails.AppendLine("Total Journey Time: " + dist[dest] + " minutes");
        }
        allCombinationsTravelTimes[index][0] = journeyDetails.ToString();
        allCombinationsTravelTimes[index][1] = dist[dest].ToString();
    }

    // Get input from user and get all possible combination of paths
    public void FindFastestRoute(string startStation, string endStation)
    {
        string[][] allCombinations = GetAllStationCombination(startStation, endStation);
        string[][] allCombinationsTravelTimes = new string[allCombinations.Length][];

        for (int i = 0; i < allCombinations.Length; i++)
        {
            allCombinationsTravelTimes[i] = new string[2]; // initialise the inner array
            Dijkstra(allCombinations[i][0], allCombinations[i][1], allCombinationsTravelTimes, i);
        }

        int minimumTravelTime = MAX_TRAVEL_TIME;
        int minimumTravelTimeIndex = MAX_TRAVEL_TIME;
        for (int i = 0; i < allCombinationsTravelTimes.Length; i++)
        {
            int travelTime = Convert.ToInt32(allCombinationsTravelTimes[i][1]);
            if (minimumTravelTime > travelTime && !allCombinationsTravelTimes[i][0].Contains("(2) Change"))
            {
                minimumTravelTime = travelTime;
                minimumTravelTimeIndex = i;
            }
        }
        Console.WriteLine(allCombinationsTravelTimes[minimumTravelTimeIndex][0]);
    }

    // Helper function to check if an array contains a specific string
    private bool ArrayContains(string[] array, string value)
    {
        foreach (string s in array)
        {
            if (s == value)
                return true;
        }

        return false;
    }

    public string[][] GetAllStationCombination(string startStation, string endStation)
    {
        string[] startStationCombinations = new string[stations.Length * stationLines.Length];
        string[] endStationCombinations = new string[stations.Length * stationLines.Length];
        int startStationCount = 0;
        int endStationCount = 0;

        for (int i = 0; i < stationLines.Length; i++)
        {
            for (int j = 0; j < stations.Length; j++)
            {
                if (stations[j].Contains(stationLines[i]))
                {
                    if (stations[j].Contains(startStation) && !ArrayContains(startStationCombinations, stations[j]))
                        startStationCombinations[startStationCount++] = stations[j];

                    if (stations[j].Contains(endStation) && !ArrayContains(endStationCombinations, stations[j]))
                        endStationCombinations[endStationCount++] = stations[j];
                }
            }
        }

        string[][] allCombinations = new string[startStationCount * endStationCount][];
        int allIndex = 0;
        for (int i = 0; i < startStationCount; i++)
        {
            for (int j = 0; j < endStationCount; j++)
            {
                allCombinations[allIndex] = new string[] { startStationCombinations[i], endStationCombinations[j] };
                allIndex++;
            }
        }

        return allCombinations;
    }

    // Update travel time between two stations
    public void UpdateTravelTime(string startStation, string endStation, int newTime, bool isTemporary)
    {
        string[][] allCombinations = GetAllStationCombination(startStation, endStation);

        int index1, index2;
        for (int i = 0; i < allCombinations.Length; i++)
        {
            // Get the indices of the stations in the stations array
            index1 = GetStationIndex(allCombinations[i][0]);
            index2 = GetStationIndex(allCombinations[i][1]);

            if (index1 == -1 || index2 == -1)
            {
                // If one or both stations are not found, print an error and return
                Console.WriteLine("One or both stations are not found.");
                return;
            }

            if (newTime < 0)
            {
                // If the new time is negative, print an error and return
                Console.WriteLine("Invalid travel time. Travel time should be a positive integer.");
                return;
            }

            if (isTemporary)
            {
                if (graph[index1, index2] != MAX_TRAVEL_TIME && temporaryTravelTimes[index1, index2] != MAX_TRAVEL_TIME)
                {
                    // If the change is temporary, update the temporary adjacency matrix
                    temporaryTravelTimes[index1, index2] = newTime;
                    temporaryTravelTimes[index2, index1] = newTime;
                    Console.WriteLine("Temporary travel time between {0} and {1} has been updated to {2} minutes.", allCombinations[i][0], allCombinations[i][1], newTime);
                }
            }
            else
            {
                if (graph[index1, index2] != MAX_TRAVEL_TIME)
                {
                    // If the change is permanent, update the original adjacency matrix
                    graph[index1, index2] = newTime;
                    graph[index2, index1] = newTime;
                    Console.WriteLine("Permanent travel time between {0} and {1} has been updated to {2} minutes.", allCombinations[i][0], allCombinations[i][1], newTime);
                }
            }
        }
    }

    // Set the status of a route between two stations
    public void SetRouteStatus(string startStation, string endStation, bool isOpen)
    {
        string[][] allCombinations = GetAllStationCombination(startStation, endStation);
        int index1, index2;
        for (int i = 0; i < allCombinations.Length; i++)
        {
            // Get the indices of the stations in the stations array
            index1 = GetStationIndex(allCombinations[i][0]);
            index2 = GetStationIndex(allCombinations[i][1]);

            if (index1 == -1 || index2 == -1)
            {
                // If one or both stations are not found, print an error and return
                Console.WriteLine("One or both stations are not found.");
                return;
            }

            if (graph[index1, index2] != MAX_TRAVEL_TIME)
            {
                if (isOpen)
                {
                    // If the route is to be opened
                    if (temporaryTravelTimes[index1, index2] == MAX_TRAVEL_TIME)
                    {
                        temporaryTravelTimes[index1, index2] = -1;

                        // If the route was previously closed, print a message indicating the route is now open
                        Console.WriteLine("Route from {0} to {1} is now open. Please set the travel time.", allCombinations[i][0], allCombinations[i][1]);
                    }
                    else
                    {
                        // If the route was already open, print a message indicating the route is already open
                        Console.WriteLine("Route from {0} to {1} is already open.", allCombinations[i][0], allCombinations[i][1]);
                    }
                }
                else
                {
                    // If the route is to be closed, update the adjacency matrix to indicate the route is closed
                    temporaryTravelTimes[index1, index2] = MAX_TRAVEL_TIME;
                    temporaryTravelTimes[index2, index1] = MAX_TRAVEL_TIME;

                    // Print a message indicating the route is now closed
                    Console.WriteLine("Route from {0} to {1} is now closed.", allCombinations[i][0], allCombinations[i][1]);
                }
            }
        }
    }

    // Reset the travel time between two stations to its original value
    public void ResetTravelTime(string startStation, string endStation)
    {
        string[][] allCombinations = GetAllStationCombination(startStation, endStation);
        int index1, index2;
        for (int i = 0; i < allCombinations.Length; i++)
        {
            // Get the indices of the stations in the stations array
            index1 = GetStationIndex(allCombinations[i][0]);
            index2 = GetStationIndex(allCombinations[i][1]);

            if (index1 == -1 || index2 == -1)
            {
                // If one or both stations are not found, print an error and return
                Console.WriteLine("One or both stations are not found.");
                return;
            }

            if (temporaryTravelTimes[index1, index2] != MAX_TRAVEL_TIME)
            {
                // Reset the travel time in the temporary adjacency matrix to the original value in the adjacency matrix
                temporaryTravelTimes[index1, index2] = -1;
                temporaryTravelTimes[index2, index1] = -1;

                // Print a message indicating the travel time has been reset
                Console.WriteLine("Travel time between {0} and {1} has been reset to its original value.", allCombinations[i][0], allCombinations[i][1]);
            }
        }
    }

    // Print all the routes that are closed
    public void PrintClosedRoutes()
    {
        Console.WriteLine("Closed routes:");
        for (int i = 0; i < stations.Length; i++)
        {
            for (int j = 0; j < stations.Length; j++)
            {
                // If the route is closed, print the route
                if (temporaryTravelTimes[i, j] == MAX_TRAVEL_TIME)
                {
                    Console.WriteLine("{0} - {1} : route closed",
                        stations[i],
                        stations[j]);
                }
            }
        }
    }

    // Print all the routes that are delayed
    public void PrintDelayedRoutes()
    {
        Console.WriteLine("Delayed routes:");
        for (int i = 0; i < stations.Length; i++)
        {
            for (int j = 0; j < stations.Length; j++)
            {
                // If the route is delayed, print
                if (temporaryTravelTimes[i, j] != graph[i, j] && temporaryTravelTimes[i, j] != -1 && temporaryTravelTimes[i, j] != MAX_TRAVEL_TIME)
                {
                    Console.WriteLine("{0} - {1} : {2} min now {3} min",
                        stations[i],
                        stations[j],
                        graph[i, j],
                        temporaryTravelTimes[i, j]);
                }
            }
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



