using System;
using System.Collections.Generic;
using System.IO;

namespace DungeonGenerator
{
    public class Dungeon
    {
        private int width;
        private int height;
        private char[,] grid;
        private Random random;

        // List to store existing rooms
        private List<Room> rooms;

        public Dungeon(int width, int height)
        {
            this.width = width;
            this.height = height;
            grid = new char[width, height];
            random = new Random(0);
            rooms = new List<Room>();
            InitializeGrid();
        }

        // Initialize grid with empty spaces
        private void InitializeGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = '.';
                }
            }
        }

        // Generate a random dungeon layout with overlap detection
        public void GenerateDungeon(int roomCount, int minRoomSize, int maxRoomSize)
        {
            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = random.Next(minRoomSize, maxRoomSize);
                int roomHeight = random.Next(minRoomSize, maxRoomSize);
                int roomX = random.Next(1, width - roomWidth - 1);
                int roomY = random.Next(1, height - roomHeight - 1);

                Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight);

                // Check for overlap with existing rooms
                bool overlaps = false;
                foreach (Room room in rooms)
                {
                    if (newRoom.Overlaps(room, 1))
                    {
                        overlaps = true;
                        break;
                    }
                }

                // If no overlap, create room and add it to the list
                if (!overlaps)
                {
                    CreateRoom(newRoom);
                    rooms.Add(newRoom);
                }
                else
                {
                    i--;
                }
            }

            // Connect rooms with corridors
            ConnectClosestRooms(3);
        }

        // Create a room at the specified location
        private void CreateRoom(Room room)
        {
            for (int i = room.X; i < room.X + room.Width; i++)
            {
                for (int j = room.Y; j < room.Y + room.Height; j++)
                {
                    grid[i, j] = 'X';
                }
            }
        }

        // Connect rooms using corridors to the n closest rooms
        private void ConnectClosestRooms(int maxConnections = 1)
        {
            // Create a list of connected rooms
            HashSet<Room> connectedRooms = new HashSet<Room>();
            List<Room> unconnectedRooms = new List<Room>(rooms);

            // Start by connecting the first room
            connectedRooms.Add(unconnectedRooms[0]);
            unconnectedRooms.RemoveAt(0);

            while (unconnectedRooms.Count > 0)
            {
                Room closestRoom = null;
                int closestDistance = int.MaxValue;
                Room currentRoom = null;

                // For each connected room, find its n closest unconnected rooms
                foreach (Room connected in connectedRooms)
                {
                    List<(Room, int)> closestUnconnected = new List<(Room, int)>();

                    foreach (Room unconnected in unconnectedRooms)
                    {
                        int distance = CalculateDistance(connected, unconnected);
                        closestUnconnected.Add((unconnected, distance));
                    }

                    // Sort by distance and take the closest n
                    closestUnconnected.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                    int numToConnect = Math.Min(random.Next(1, maxConnections), closestUnconnected.Count);
                    for (int i = 0; i < numToConnect; i++)
                    {
                        var (unconnected, distance) = closestUnconnected[i];
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestRoom = unconnected;
                            currentRoom = connected;  // Remember the connected room as well
                        }
                    }
                }

                // If a closest room was found, connect them
                if (closestRoom != null)
                {
                    (int startX, int startY, int endX, int endY) = FindClosestPoints(currentRoom, closestRoom);

                    if (random.Next(2) == 0)
                    {
                        CreateHorizontalCorridor(startX, endX, startY);
                        CreateVerticalCorridor(startY, endY, endX);
                    }
                    else
                    {
                        CreateVerticalCorridor(startY, endY, startX);
                        CreateHorizontalCorridor(startX, endX, endY);
                    }

                    // Move the newly connected room to the connected list
                    connectedRooms.Add(closestRoom);
                    unconnectedRooms.Remove(closestRoom);
                }
            }
        }



        // Find the closest points between the walls of two rooms
        private (int, int, int, int) FindClosestPoints(Room roomA, Room roomB)
        {
            int closestDistance = int.MaxValue;
            int bestX1 = 0, bestY1 = 0, bestX2 = 0, bestY2 = 0;

            // Check each wall point of roomA with each wall point of roomB
            for (int x1 = roomA.X; x1 < roomA.X + roomA.Width; x1++)
            {
                for (int y1 = roomA.Y; y1 < roomA.Y + roomA.Height; y1++)
                {
                    for (int x2 = roomB.X; x2 < roomB.X + roomB.Width; x2++)
                    {
                        for (int y2 = roomB.Y; y2 < roomB.Y + roomB.Height; y2++)
                        {
                            int distance = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                bestX1 = x1;
                                bestY1 = y1;
                                bestX2 = x2;
                                bestY2 = y2;
                            }
                        }
                    }
                }
            }

            return (bestX1, bestY1, bestX2, bestY2);
        }

        // Calculate the distance between two rooms (center-to-center)
        private int CalculateDistance(Room roomA, Room roomB)
        {
            int centerAx = roomA.X + roomA.Width / 2;
            int centerAy = roomA.Y + roomA.Height / 2;
            int centerBx = roomB.X + roomB.Width / 2;
            int centerBy = roomB.Y + roomB.Height / 2;

            return Math.Abs(centerAx - centerBx) + Math.Abs(centerAy - centerBy);
        }

        // Create a horizontal corridor between two points
        private void CreateHorizontalCorridor(int x1, int x2, int y)
        {
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                grid[x, y] = '#';
            }
        }

        // Create a vertical corridor between two points
        private void CreateVerticalCorridor(int y1, int y2, int x)
        {
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                grid[x, y] = '#';
            }
        }

        // Export dungeon to a text file
        public void ExportToFile(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        writer.Write(grid[x, y]);
                    }
                    writer.WriteLine();  // New line after each row
                }
            }
            Console.WriteLine($"Dungeon exported to {fileName}");
        }

        // Import dungeon from a text file
        public void ImportFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("File not found!");
                return;
            }

            string[] lines = File.ReadAllLines(fileName);
            height = lines.Length;
            width = lines[0].Length;
            grid = new char[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    grid[x, y] = lines[y][x];
                }
            }
            Console.WriteLine($"Dungeon imported from {fileName}");
        }

        // Print the dungeon to the console
        public void PrintDungeon()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Console.Write(grid[x, y]);
                }
                Console.WriteLine();
            }
        }

        public override string ToString()
        {
            string builder = "";
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    builder += grid[x, y];
                }
                builder += '\n';
            }
            return builder;
        }
    }

    // Class to represent a room
    public class Room
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public int CenterX => X + Width / 2;
        public int CenterY => Y + Height / 2;

        public Room(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        // Check if this room overlaps with another room
        public bool Overlaps(Room other)
        {
            return Overlaps(other, 0);
        }

        // Check if this room overlaps with another room, considering a buffer
        public bool Overlaps(Room other, int buffer)
        {
            return X - buffer < other.X + other.Width &&
                   X + Width + buffer > other.X &&
                   Y - buffer < other.Y + other.Height &&
                   Y + Height + buffer > other.Y;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Dungeon dungeon = new Dungeon(40, 40);

            // Generate a new dungeon
            dungeon.GenerateDungeon(15, 3, 8);
            dungeon.PrintDungeon();

            // Export to file
            dungeon.ExportToFile("dungeon.txt");

            // Import from file and print
            Dungeon loadedDungeon = new Dungeon(40, 20);
            loadedDungeon.ImportFromFile("dungeon.txt");
            loadedDungeon.PrintDungeon();
        }
    }
}