using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace DungeonGenerator
{
    public class Dungeon
    {
        private int scaleFactor = 2;

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
                    Room.roomCharRunning--;
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
                    grid[i, j] = room.RoomChar.FirstOrDefault();
                }
            }
        }

        // Connect rooms using corridors to the n closest rooms
        private void ConnectClosestRooms(int n = 1)
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
                    for (int i = 0; i < Math.Min(n, closestUnconnected.Count); i++)
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
                    List<(int startX, int startY, int endX, int endY)> closestPoints = FindClosestPoints(currentRoom, closestRoom);
                    if (closestPoints != null && closestPoints.Count > 0)
                    {
                        var randomPoint = closestPoints[random.Next(closestPoints.Count)];
                        int startX = randomPoint.startX;
                        int startY = randomPoint.startY;
                        int endX = randomPoint.endX;
                        int endY = randomPoint.endY;

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
        }

        // Find all closest points between the walls of two rooms
        private List<(int, int, int, int)> FindClosestPoints(Room roomA, Room roomB)
        {
            int closestDistance = int.MaxValue;
            List<(int, int, int, int)> closestPoints = [];

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
                                closestPoints.Clear();  // Clear previous closest points
                                closestPoints.Add((x1, y1, x2, y2)); // Add new closest pair
                            }
                            else if (distance == closestDistance)
                            {
                                closestPoints.Add((x1, y1, x2, y2)); // Add to the list of closest pairs
                            }
                        }
                    }
                }
            }

            return closestPoints;
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
        public static int roomCharRunning = 0; // max 16 rooms right now
        public string RoomChar = (roomCharRunning++).ToString("X");
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

        public bool Contains(int x, int y)
        {
            return new Rectangle(X, Y, Width, Height).Contains(x, y);
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