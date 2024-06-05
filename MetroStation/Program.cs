using System;
using System.IO;
using System.Text;

public class Station
{
    public string Name { get; set; }
    public string Zone { get; set; }
}

public class MetroSystem
{
    private int[,] adjacencyMatrix;
    private int[,] temporaryAdjacencyMatrix;
    private string[,] issuesMatrix;
    private Station[] stations;
    // Constructor for the MetroSystem class
    public MetroSystem(string input, bool isFilePath)
    {
        string[] lines;

        if (isFilePath)
        {
            // Check if the provided file is a CSV file
            if (Path.GetExtension(input) != ".csv")
            {
                throw new ArgumentException("File must be a .csv file");
            }

            // Read all lines from the file
            lines = File.ReadAllLines(input);
        }
        else
        {
            // If input is not a file, split it into lines
            lines = input.Split('\n');

            // If the last line is 'end' or empty, remove it
            if (lines[lines.Length - 1] == "end" || lines[lines.Length - 1] == "")
            {
                Array.Resize(ref lines, lines.Length - 1);
            }
        }

        // Calculate the number of stations and initialize the station-related arrays and matrices
        int numberOfStations = lines.Length;
        stations = new Station[numberOfStations];
        adjacencyMatrix = new int[numberOfStations, numberOfStations];
        temporaryAdjacencyMatrix = new int[numberOfStations, numberOfStations];
        issuesMatrix = new string[numberOfStations, numberOfStations];

        // Initialize all distances in adjacency matrices to int.MaxValue, and set all issues to null
        for (int i = 0; i < numberOfStations; i++)
        {
            for (int j = 0; j < numberOfStations; j++)
            {
                adjacencyMatrix[i, j] = int.MaxValue;
                temporaryAdjacencyMatrix[i, j] = int.MaxValue;
                issuesMatrix[i, j] = null;
            }
        }

        // Process each line of the input
        for (int i = 0; i < numberOfStations; i++)
        {
            var line = lines[i].Split(',');

            // If the line doesn't have enough fields, print an error and skip it
            if (line.Length < 4)
            {
                Console.WriteLine("Invalid data in line {0}. Each line should have 4 fields.", i + 1);
                continue;
            }

            // Parse the station data from the line
            var station1 = new Station { Zone = line[0], Name = line[1] };
            var station2 = new Station { Zone = line[0], Name = line[2] };

            // If the distance field is not an integer, print an error and skip the line
            int distance;
            if (!int.TryParse(line[3], out distance))
            {
                Console.WriteLine("Invalid distance value in line {0}. Distance should be an integer.", i + 1);
                continue;
            }

            // Find the indices of the stations in the array, adding them if they're not found
            int index1 = GetStationIndex(station1.Name);
            if (index1 == -1)
            {
                index1 = AddStation(station1);
            }

            int index2 = GetStationIndex(station2.Name);
            if (index2 == -1)
            {
                index2 = AddStation(station2);
            }

            // Update the adjacency matrix with the distance between the two stations
            adjacencyMatrix[index1, index2] = distance;
            adjacencyMatrix[index2, index1] = distance;
        }
    }

    // Get the index of a station by its name, or return -1 if it's not found
    public int GetStationIndex(string name)
    {
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null && stations[i].Name == name)
            {
                // If the station is found, return its index
                return i;
            }
        }

        // If the station is not found, return -1
        return -1;
    }

    // Add a new station to the first null spot in the stations array and return its index
    public int AddStation(Station station)
    {
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] == null)
            {
                stations[i] = station;
                return i;
            }
        }

        // If there's no room for a new station, throw an exception
        throw new Exception("No room to add more stations");
    }

    // Find the fastest route between two stations using Dijkstra's algorithm
    public void FindFastestRoute(string startStation, string endStation)
    {
        int start = GetStationIndex(startStation);
        int end = GetStationIndex(endStation);

        if (start == -1 || end == -1)
        {
            // If one or both stations are not found, print an error and return
            Console.WriteLine("One or both stations are not found.");
            return;
        }

        if (stations[start].Zone != stations[end].Zone)
        {
            // If the stations are not in the same zone, print an error and return
            Console.WriteLine("Stations are not in the same zone.");
            return;
        }

        // Initialize the distances and previous stations arrays
        int[] distances = new int[stations.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue;
        }
        distances[start] = 0;

        int[] previous = new int[stations.Length];
        for (int i = 0; i < previous.Length; i++)
        {
            previous[i] = -1;
        }

        // Initialize the unvisited set
        bool[] unvisited = new bool[stations.Length];
        for (int i = 0; i < unvisited.Length; i++)
        {
            unvisited[i] = true;
        }

        while (true)
        {
            // Select the unvisited node with the smallest distance
            int minDistance = int.MaxValue;
            int minIndex = -1;
            for (int i = 0; i < distances.Length; i++)
            {
                if (unvisited[i] && distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    minIndex = i;
                }
            }

            if (minIndex == -1)
            {
                // If all remaining nodes are unreachable, break the loop
                break;
            }

            // Visit the selected node
            unvisited[minIndex] = false;

            // Update the distances to the neighboring nodes
            for (int i = 0; i < stations.Length; i++)
            {
                int currentDistance;

                //if (!string.IsNullOrEmpty(issuesMatrix[minIndex, i])) // if there is an issue on this route
                if (temporaryAdjacencyMatrix[minIndex, i] != int.MaxValue)
                {
                    // Use the temporary adjacency matrix if there's an issue on this route
                    currentDistance = temporaryAdjacencyMatrix[minIndex, i];
                }
                else
                {
                    // Otherwise, use the original adjacency matrix
                    currentDistance = adjacencyMatrix[minIndex, i];
                }

                if (currentDistance != int.MaxValue)
                {
                    // If the route is reachable, update the distance if necessary
                    int altDistance = distances[minIndex] + currentDistance;
                    if (altDistance < distances[i])
                    {
                        distances[i] = altDistance;
                        previous[i] = minIndex;
                    }
                }
            }
        }

        // Print the shortest path
        if (distances[end] == int.MaxValue)
        {
            // If no route is found, print an error message
            Console.WriteLine("No route found from {0} to {1}.", startStation, endStation);
        }
        else
        {
            // If a route is found, print the route
            var path = new string[stations.Length];
            int currentIndex = end;
            int count = 0;
            while (currentIndex != -1)
            {
                path[count] = stations[currentIndex].Name;
                currentIndex = previous[currentIndex];
                count++;
            }

            StringBuilder pathOutput = new StringBuilder();
            pathOutput.Append("Shortest route from " + startStation + " to " + endStation + ": ");
            for (int i = count - 1; i >= 0; i--)
            {
                pathOutput.Append(path[i]);
                if (i != 0)
                    pathOutput.Append(" -> ");
            }
            pathOutput.Append(", Total Journey Time: " + distances[end] + " minutes");

            Console.WriteLine(pathOutput.ToString());
        }
    }

    // Update travel time between two stations
    public void UpdateTravelTime(string station1, string station2, int newTime, bool isTemporary)
    {
        int index1 = GetStationIndex(station1);
        int index2 = GetStationIndex(station2);

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
            // If the change is temporary, update the temporary adjacency matrix
            temporaryAdjacencyMatrix[index1, index2] = newTime;
            temporaryAdjacencyMatrix[index2, index1] = newTime;
            Console.WriteLine("Temporary travel time between {0} and {1} has been updated to {2} minutes.", station1, station2, newTime);
        }
        else
        {
            // If the change is permanent, update the original adjacency matrix
            adjacencyMatrix[index1, index2] = newTime;
            adjacencyMatrix[index2, index1] = newTime;
            Console.WriteLine("Permanent travel time between {0} and {1} has been updated to {2} minutes.", station1, station2, newTime);
        }
    }

    // Set the status of a route between two stations
    public void SetRouteStatus(string station1, string station2, bool isOpen)
    {
        // Get the indices of the stations in the stations array
        int index1 = GetStationIndex(station1);
        int index2 = GetStationIndex(station2);

        if (index1 == -1 || index2 == -1)
        {
            // If one or both stations are not found, print an error and return
            Console.WriteLine("One or both stations are not found.");
            return;
        }

        if (isOpen)
        {
            // If the route is to be opened
            if (adjacencyMatrix[index1, index2] == int.MaxValue)
            {
                // If the route was previously closed, print a message indicating the route is now open
                Console.WriteLine("Route from {0} to {1} is now open. Please set the travel time.", station1, station2);
            }
            else
            {
                // If the route was already open, print a message indicating the route is already open
                Console.WriteLine("Route from {0} to {1} is already open.", station1, station2);
            }
        }
        else
        {
            // If the route is to be closed, update the adjacency matrix to indicate the route is closed
            adjacencyMatrix[index1, index2] = int.MaxValue;
            adjacencyMatrix[index2, index1] = int.MaxValue;

            // Print a message indicating the route is now closed
            Console.WriteLine("Route from {0} to {1} is now closed.", station1, station2);
        }
    }

    // Reset the travel time between two stations to its original value
    public void ResetTravelTime(string station1, string station2)
    {
        // Get the indices of the stations in the stations array
        int index1 = GetStationIndex(station1);
        int index2 = GetStationIndex(station2);

        if (index1 == -1 || index2 == -1)
        {
            // If one or both stations are not found, print an error and return
            Console.WriteLine("One or both stations are not found.");
            return;
        }

        // Reset the travel time in the temporary adjacency matrix to the original value in the adjacency matrix
        temporaryAdjacencyMatrix[index1, index2] = adjacencyMatrix[index1, index2];
        temporaryAdjacencyMatrix[index2, index1] = adjacencyMatrix[index2, index1];

        // Print a message indicating the travel time has been reset
        Console.WriteLine("Travel time between {0} and {1} has been reset to its original value.", station1, station2);
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
                if (adjacencyMatrix[i, j] == int.MaxValue)
                {
                    Console.WriteLine("{0}: {1} - {2} : route closed",
                        stations[i].Zone,
                        stations[i].Name,
                        stations[j].Name);
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
                if (temporaryAdjacencyMatrix[i, j] != adjacencyMatrix[i, j] && temporaryAdjacencyMatrix[i, j] != int.MaxValue)
                {
                    Console.WriteLine("{0}: {1} - {2} : {3} min now {4} min",
                        stations[i].Zone,
                        stations[i].Name,
                        stations[j].Name,
                        adjacencyMatrix[i, j],
                        temporaryAdjacencyMatrix[i, j]);
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



