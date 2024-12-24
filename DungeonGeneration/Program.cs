using System.Drawing;

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
        public void GenerateDungeon(int roomCount, int minRoomSize, int maxRoomSize, int buffer = 2)
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
                    if (newRoom.Overlaps(room, buffer))
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
            List<Room> unconnectedRooms = new List<Room>(rooms);

            while (unconnectedRooms.Count > 0)
            {
                Room currentRoom = unconnectedRooms[random.Next(unconnectedRooms.Count)];
                List<Room> roomsToConnect = [];

                // Calculate distance to each room
                List<(Room, int)> closest = new List<(Room, int)>();
                foreach (Room room in rooms)
                {
                    int distance = CalculateDistance(currentRoom, room);
                    closest.Add((room, distance));
                }

                // Sort by distance and take the closest n
                closest.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                roomsToConnect.AddRange(closest.Where(r => r.Item1 != currentRoom).Take(random.Next(1, n)).Select(pair => pair.Item1));

                // If a closest room was found, connect them
                foreach (Room room in roomsToConnect)
                {
                    List<(int startX, int startY, int endX, int endY)> paths = FindPaths(currentRoom, room);
                    if (paths != null && paths.Count > 0)
                    {
                        int width = random.Next(3, 6);
                        var randomPoint = paths[random.Next(paths.Count)];
                        while (!IsWithinRoomBounds(currentRoom, room, randomPoint.startX, randomPoint.startY, randomPoint.endX, randomPoint.endY, width / 2))
                        {
                            randomPoint = paths[random.Next(paths.Count)];
                        }

                        int startX = randomPoint.startX;
                        int startY = randomPoint.startY;
                        int endX = randomPoint.endX;
                        int endY = randomPoint.endY;

                        if (random.Next(2) == 0)
                        {
                            if (startX - endX != 0) CreateHorizontalCorridor(startX, endX, startY, width);
                            if (startY - endY != 0) CreateVerticalCorridor(startY, endY, endX, width);
                        }
                        else
                        {
                            if (startY - endY != 0) CreateVerticalCorridor(startY, endY, startX, width);
                            if (startX - endX != 0) CreateHorizontalCorridor(startX, endX, endY, width);
                        }

                        unconnectedRooms.Remove(room);
                    }
                }
            }
        }

        // Find all closest points between the walls of two rooms
        private List<(int, int, int, int)> FindPaths(Room roomA, Room roomB)
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

        // Create a horizontal corridor between two points with random width
        private void CreateHorizontalCorridor(int x1, int x2, int y, int width)
        {
            // Adjust to expand by half the width on both sides of the central line

            for (int i = -width / 2; i <= width / 2; i++)
            {
                for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
                {
                    int newY = y + i; // Expand vertically along y axis
                    if (IsWithinBounds(x, newY))
                    {
                        grid[x, newY] = '#';  // Place corridor tiles
                    }
                }
            }
        }

        // Create a vertical corridor between two points with random width
        private void CreateVerticalCorridor(int y1, int y2, int x, int width)
        {
            // Adjust to expand by half the width on both sides of the central line

            for (int i = -width / 2; i <= width / 2; i++)
            {
                for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
                {
                    int newX = x + i;  // Expand horizontally along x axis
                    if (IsWithinBounds(newX, y))
                    {
                        grid[newX, y] = '#';  // Place corridor tiles
                    }
                }
            }
        }

        private bool IsWithinRoomBounds(Room roomA, Room roomB, int startX, int startY, int endX, int endY, int widthFromCenterLine)
        {
            int successesStart = 0;
            Room startRoom = roomA.Contains(startX, startY) ? roomA : roomB;
            successesStart += startRoom.Contains(startX + widthFromCenterLine, startY) == true ? 1 : 0;
            successesStart += startRoom.Contains(startX - widthFromCenterLine, startY) == true ? 1 : 0;
            successesStart += startRoom.Contains(startX, startY + widthFromCenterLine) == true ? 1 : 0;
            successesStart += startRoom.Contains(startX, startY - widthFromCenterLine) == true ? 1 : 0;

            int successesEnd = 0;
            Room endRoom = roomA.Contains(endX, endY) ? roomA : roomB;
            successesEnd += endRoom.Contains(endX + widthFromCenterLine, endY) == true ? 1 : 0;
            successesEnd += endRoom.Contains(endX - widthFromCenterLine, endY) == true ? 1 : 0;
            successesEnd += endRoom.Contains(endX, endY + widthFromCenterLine) == true ? 1 : 0;
            successesEnd += endRoom.Contains(endX, endY - widthFromCenterLine) == true ? 1 : 0;

            return successesStart >= 3 && successesEnd >= 3;
        }

        // Helper method to ensure we're within the grid bounds
        private bool IsWithinBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
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
            Dungeon dungeon = new Dungeon(100, 100);

            // Generate a new dungeon
            dungeon.GenerateDungeon(15, 10, 20);
            dungeon.PrintDungeon();

            // Export to file
            dungeon.ExportToFile("dungeon.txt");

            // Import from file and print
            //Dungeon loadedDungeon = new Dungeon(40, 20);
            //loadedDungeon.ImportFromFile("dungeon.txt");
            //loadedDungeon.PrintDungeon();
        }
    }
}