using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using Caps.RPG.World.Models.Graphics;

namespace Caps.RPG.World.Models
{
    public static class MapLoader
    {
        public static async Task<WorldMap?> LoadFromFileAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            return ParseFmgMapContent(content);
        }

        public static WorldMap? LoadFromFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var content = File.ReadAllText(path);
            return ParseFmgMapContent(content);
        }

        private static WorldMap? ParseFmgMapContent(string content)
        {
            System.Diagnostics.Debug.WriteLine("🚨 MAP LOADING DEBUG: Starting ParseFmgMapContent (JSON-only)");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            options.Converters.Add(new Point2JsonConverter());
            options.Converters.Add(new IntToBoolConverter());

            // Only support the official FMG JSON export format going forward.
            var trimmedContent = content.TrimStart();
            if (!trimmedContent.StartsWith("{"))
            {
                System.Diagnostics.Debug.WriteLine("❌ Legacy .map format support removed. Only FMG JSON export is supported.");
                return null;
            }

            System.Diagnostics.Debug.WriteLine("🚨 MAP LOADING: Detected FULL JSON export format");
            return ParseFullJsonExport(content, options);
        }

        /// <summary>
        /// Parse full JSON export format from FMG (exported via "Export to JSON" option)
        /// </summary>
        private static WorldMap? ParseFullJsonExport(string jsonContent, JsonSerializerOptions options)
        {
            System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT LOADER: Starting full JSON parsing...");

            var map = new WorldMap();

            try
            {
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                // Parse pack section - this contains most of the map data
                if (root.TryGetProperty("pack", out var packElement))
                {
                    ParsePackFromJson(packElement, map, options);
                }

                // Parse biomes section (top-level key alongside "pack")
                if (root.TryGetProperty("biomes", out var biomesElement) && biomesElement.ValueKind == JsonValueKind.Object)
                {
                    map.Biomes = ParseBiomesFromJson(biomesElement);
                    System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Loaded biomes data ({map.Biomes.Names?.Length ?? 0} biomes)");
                }

                // Parse info section for map metadata
                if (root.TryGetProperty("info", out var infoElement))
                {
                    System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Found info section");
                    if (infoElement.TryGetProperty("mapName", out var mapName))
                        System.Diagnostics.Debug.WriteLine($"  Map name: {mapName.GetString()}");
                    if (infoElement.TryGetProperty("width", out var width))
                        System.Diagnostics.Debug.WriteLine($"  Width: {width.GetInt32()}");
                    if (infoElement.TryGetProperty("height", out var height))
                        System.Diagnostics.Debug.WriteLine($"  Height: {height.GetInt32()}");
                }

                System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT LOADER: FINAL SUMMARY:");
                System.Diagnostics.Debug.WriteLine($"   Cells: {map?.Cells?.Coordinates?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"   Vertices: {map?.Vertices?.Coordinates?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"   StateIndexes: {map?.StateIndexes?.Length ?? 0}");
                System.Diagnostics.Debug.WriteLine($"   States: {map?.States?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"   Burgs: {map?.Burgs?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"   Features: {map?.Features?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"   Biomes: {map?.Biomes?.Names?.Length ?? 0}");

                return map;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT LOADER ERROR: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"🚨 Stack trace: {ex.StackTrace}");
                throw new InvalidDataException($"Failed to parse FMG JSON export: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Parse the pack section from full JSON export
        /// </summary>
        private static void ParsePackFromJson(JsonElement packElement, WorldMap map, JsonSerializerOptions options)
        {
            System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT: Parsing pack section...");

            // Parse cells array - each cell is an object with i, v, c, p, state, etc.
            if (packElement.TryGetProperty("cells", out var cellsElement) && cellsElement.ValueKind == JsonValueKind.Array)
            {
                ParseCellsFromJson(cellsElement, map);
            }

            // Parse vertices array - each vertex has i, p, v, c
            if (packElement.TryGetProperty("vertices", out var verticesElement) && verticesElement.ValueKind == JsonValueKind.Array)
            {
                ParseVerticesFromJson(verticesElement, map);
            }

            // Parse states
            if (packElement.TryGetProperty("states", out var statesElement) && statesElement.ValueKind == JsonValueKind.Array)
            {
                var statesList = new List<State>();
                ParseStates(statesElement, statesList, options);
                map.States = statesList;
                System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed {statesList.Count} states");
            }

            // Parse burgs
            if (packElement.TryGetProperty("burgs", out var burgsElement) && burgsElement.ValueKind == JsonValueKind.Array)
            {
                var burgsList = new List<Burg>();
                ParseBurgs(burgsElement, burgsList, options);
                map.Burgs = burgsList;
                System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed {burgsList.Count} burgs");
            }

            // Parse features
            if (packElement.TryGetProperty("features", out var featuresElement) && featuresElement.ValueKind == JsonValueKind.Array)
            {
                ParseFeaturesFromJson(featuresElement, map);
            }

            // Parse cultures
            if (packElement.TryGetProperty("cultures", out var culturesElement) && culturesElement.ValueKind == JsonValueKind.Array)
            {
                var culturesList = new List<Culture>();
                ParseCultures(culturesElement, culturesList, options);
                map.Cultures = culturesList;
                System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed {culturesList.Count} cultures");
            }

            // Parse religions
            if (packElement.TryGetProperty("religions", out var religionsElement) && religionsElement.ValueKind == JsonValueKind.Array)
            {
                var religionsList = new List<Religion>();
                ParseReligions(religionsElement, religionsList, options);
                map.Religions = religionsList;
                System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed {religionsList.Count} religions");
            }

            // Parse rivers (optional) - FMG export may include pack.rivers array with river metadata
            if (packElement.TryGetProperty("rivers", out var riversElement) && riversElement.ValueKind == JsonValueKind.Array)
            {
                try
                {
                    var riversJson = riversElement.GetRawText();
                    var rivers = System.Text.Json.JsonSerializer.Deserialize<List<River>>(riversJson, options);
                    map.Rivers = rivers;
                    System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed {rivers?.Count ?? 0} rivers");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Failed to parse rivers: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Parse cells from the JSON export format where each cell is a complete object
        /// </summary>
        private static void ParseCellsFromJson(JsonElement cellsArray, WorldMap map)
        {
            System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT: Parsing cells array...");

            var cellCount = cellsArray.GetArrayLength();
            System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Found {cellCount} cells");

            // Prepare arrays to store cell data
            var coordinates = new List<Point2>();
            var vertexIndexes = new List<int[]>();
            var adjacentCells = new List<int[]>();
            var stateIndexes = new List<ushort>();
            var cultureIndexes = new List<ushort>();
            var religionIndexes = new List<ushort>();
            var elevations = new List<byte>();
            var biomeIndexes = new List<byte>();
            var featureIndexes = new List<int>();
            var terrainTypes = new List<sbyte>();
            var riverIndexes = new List<ushort>();
            var waterFlux = new List<ushort>();
            var riverConfluences = new List<ushort>();

            foreach (var cellElement in cellsArray.EnumerateArray())
            {
                if (cellElement.ValueKind != JsonValueKind.Object) continue;

                // Parse coordinates (p: [x, y])
                if (cellElement.TryGetProperty("p", out var pElement) && pElement.ValueKind == JsonValueKind.Array)
                {
                    var coords = pElement.EnumerateArray().ToArray();
                    if (coords.Length >= 2)
                    {
                        double x = coords[0].GetDouble();
                        double y = coords[1].GetDouble();
                        coordinates.Add(new Point2(x, y));
                    }
                }
                else
                {
                    coordinates.Add(new Point2(0, 0)); // Placeholder
                }

                // Parse vertex indices (v: [v1, v2, v3, ...])
                if (cellElement.TryGetProperty("v", out var vElement) && vElement.ValueKind == JsonValueKind.Array)
                {
                    var vertices = vElement.EnumerateArray().Select(v => v.GetInt32()).ToArray();
                    vertexIndexes.Add(vertices);
                }
                else
                {
                    vertexIndexes.Add(Array.Empty<int>());
                }

                // Parse adjacent cells (c: [c1, c2, c3, ...])
                if (cellElement.TryGetProperty("c", out var cElement) && cElement.ValueKind == JsonValueKind.Array)
                {
                    var adjacent = cElement.EnumerateArray().Select(c => c.GetInt32()).ToArray();
                    adjacentCells.Add(adjacent);
                }
                else
                {
                    adjacentCells.Add(Array.Empty<int>());
                }

                // Parse state index
                if (cellElement.TryGetProperty("state", out var stateElement))
                {
                    stateIndexes.Add((ushort)stateElement.GetInt32());
                }
                else
                {
                    stateIndexes.Add(0);
                }

                // Parse culture index
                if (cellElement.TryGetProperty("culture", out var cultureElement))
                {
                    cultureIndexes.Add((ushort)cultureElement.GetInt32());
                }
                else
                {
                    cultureIndexes.Add(0);
                }

                // Parse religion index
                if (cellElement.TryGetProperty("religion", out var religionElement))
                {
                    religionIndexes.Add((ushort)religionElement.GetInt32());
                }
                else
                {
                    religionIndexes.Add(0);
                }

                // Parse height/elevation (h)
                if (cellElement.TryGetProperty("h", out var hElement))
                {
                    elevations.Add((byte)hElement.GetInt32());
                }
                else
                {
                    elevations.Add(0);
                }

                // Parse biome
                if (cellElement.TryGetProperty("biome", out var biomeElement))
                {
                    biomeIndexes.Add((byte)biomeElement.GetInt32());
                }
                else
                {
                    biomeIndexes.Add(0);
                }

                // Parse feature index (f)
                if (cellElement.TryGetProperty("f", out var fElement))
                {
                    featureIndexes.Add(fElement.GetInt32());
                }
                else
                {
                    featureIndexes.Add(0);
                }

                // Parse terrain type (t)
                if (cellElement.TryGetProperty("t", out var tElement))
                {
                    terrainTypes.Add((sbyte)tElement.GetInt32());
                }
                else
                {
                    terrainTypes.Add(0);
                }

                // Parse river index for cell (r)
                if (cellElement.TryGetProperty("r", out var rElement))
                {
                    try
                    {
                        riverIndexes.Add((ushort)rElement.GetInt32());
                    }
                    catch
                    {
                        riverIndexes.Add(0);
                    }
                }
                else
                {
                    riverIndexes.Add(0);
                }

                // Parse water flux (fl)
                if (cellElement.TryGetProperty("fl", out var flElement))
                {
                    try
                    {
                        waterFlux.Add((ushort)flElement.GetInt32());
                    }
                    catch
                    {
                        waterFlux.Add(0);
                    }
                }
                else
                {
                    waterFlux.Add(0);
                }

                // Parse confluence marker (conf)
                if (cellElement.TryGetProperty("conf", out var confElement))
                {
                    try
                    {
                        riverConfluences.Add((ushort)confElement.GetInt32());
                    }
                    catch
                    {
                        riverConfluences.Add(0);
                    }
                }
                else
                {
                    riverConfluences.Add(0);
                }
            }

            // Assign parsed data to map
            map.Cells = new VoronoiCells
            {
                Coordinates = coordinates.ToArray(),
                VertexIndexes = vertexIndexes.ToArray(),
                AdjacentCells = adjacentCells.ToArray()
            };
            map.StateIndexes = stateIndexes.ToArray();
            map.CultureIndexes = cultureIndexes.ToArray();
            map.ReligionIndexes = religionIndexes.ToArray();
            map.Elevation = elevations.ToArray();
            map.BiomeIndexes = biomeIndexes.ToArray();
            map.FeatureIndexes = featureIndexes.ToArray();
            map.TerrainType = terrainTypes.ToArray();

            System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Successfully parsed {coordinates.Count} cells:");
            System.Diagnostics.Debug.WriteLine($"   Coordinates: {coordinates.Count}");
            System.Diagnostics.Debug.WriteLine($"   Vertex indexes: {vertexIndexes.Count}");
            System.Diagnostics.Debug.WriteLine($"   State indexes: {stateIndexes.Count}");
            System.Diagnostics.Debug.WriteLine($"   Unique states: {stateIndexes.Distinct().Count()}");

            // Sample first few cells for verification
            if (coordinates.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT: First 5 cells:");
                for (int i = 0; i < Math.Min(5, coordinates.Count); i++)
                {
                    var coord = coordinates[i];
                    var state = stateIndexes[i];
                    var verts = vertexIndexes[i];
                    System.Diagnostics.Debug.WriteLine($"   Cell[{i}]: ({coord.X:F1}, {coord.Y:F1}) State={state} Vertices=[{string.Join(",", verts.Take(5))}...]");
                }
            }
        }

        /// <summary>
        /// Parse vertices from the JSON export format
        /// </summary>
        private static void ParseVerticesFromJson(JsonElement verticesArray, WorldMap map)
        {
            System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT: Parsing vertices array...");

            var vertexCount = verticesArray.GetArrayLength();
            System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Found {vertexCount} vertices");

            var coordinates = new List<Point2>();
            var adjacentCells = new List<int[]>();
            var adjacentVertices = new List<int[]>();

            foreach (var vertexElement in verticesArray.EnumerateArray())
            {
                if (vertexElement.ValueKind != JsonValueKind.Object) continue;

                // Parse coordinates (p: [x, y])
                if (vertexElement.TryGetProperty("p", out var pElement) && pElement.ValueKind == JsonValueKind.Array)
                {
                    var coords = pElement.EnumerateArray().ToArray();
                    if (coords.Length >= 2)
                    {
                        double x = coords[0].GetDouble();
                        double y = coords[1].GetDouble();
                        coordinates.Add(new Point2(x, y));
                    }
                }
                else
                {
                    coordinates.Add(new Point2(0, 0));
                }

                // Parse adjacent cells (c: [c1, c2, c3])
                if (vertexElement.TryGetProperty("c", out var cElement) && cElement.ValueKind == JsonValueKind.Array)
                {
                    var cells = cElement.EnumerateArray().Select(c => c.GetInt32()).ToArray();
                    adjacentCells.Add(cells);
                }
                else
                {
                    adjacentCells.Add(Array.Empty<int>());
                }

                // Parse adjacent vertices (v: [v1, v2, v3])
                if (vertexElement.TryGetProperty("v", out var vElement) && vElement.ValueKind == JsonValueKind.Array)
                {
                    var vertices = vElement.EnumerateArray().Select(v => v.GetInt32()).ToArray();
                    adjacentVertices.Add(vertices);
                }
                else
                {
                    adjacentVertices.Add(Array.Empty<int>());
                }
            }

            map.Vertices = new VoronoiVertices
            {
                Coordinates = coordinates.ToArray(),
                AdjacentCells = adjacentCells.ToArray(),
                AdjacentVertices = adjacentVertices.ToArray()
            };

            System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Successfully parsed {coordinates.Count} vertices");

            // Sample first few vertices
            if (coordinates.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT: First 5 vertices:");
                for (int i = 0; i < Math.Min(5, coordinates.Count); i++)
                {
                    var coord = coordinates[i];
                    System.Diagnostics.Debug.WriteLine($"   Vertex[{i}]: ({coord.X:F1}, {coord.Y:F1})");
                }
            }
        }

        /// <summary>
        /// Parse features from the JSON export format
        /// </summary>
        private static void ParseFeaturesFromJson(JsonElement featuresArray, WorldMap map)
        {
            System.Diagnostics.Debug.WriteLine("🚨 JSON EXPORT: Parsing features array...");

            var featuresList = new List<Feature>();
            foreach (var featureElement in featuresArray.EnumerateArray())
            {
                if (featureElement.ValueKind != JsonValueKind.Object) continue;

                var feature = new Feature();

                if (featureElement.TryGetProperty("i", out var iElement))
                    feature.Id = iElement.GetInt32();

                if (featureElement.TryGetProperty("land", out var landElement))
                    feature.Land = landElement.GetBoolean();

                if (featureElement.TryGetProperty("border", out var borderElement))
                    feature.Border = borderElement.GetBoolean();

                if (featureElement.TryGetProperty("type", out var typeElement))
                    feature.Type = typeElement.GetString();

                if (featureElement.TryGetProperty("group", out var groupElement))
                    feature.Group = groupElement.GetString();

                if (featureElement.TryGetProperty("cells", out var cellsElement))
                    feature.Cells = cellsElement.GetInt32();

                if (featureElement.TryGetProperty("firstCell", out var firstCellElement))
                    feature.FirstCell = firstCellElement.GetInt32();

                if (featureElement.TryGetProperty("name", out var nameElement))
                    feature.Name = nameElement.GetString();

                if (featureElement.TryGetProperty("vertices", out var verticesElement)
                    && verticesElement.ValueKind == JsonValueKind.Array)
                {
                    feature.Vertices = verticesElement.EnumerateArray()
                        .Select(v => v.GetInt32())
                        .ToArray();
                }

                featuresList.Add(feature);
            }

            map.Features = featuresList;
            System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed {featuresList.Count} features");

            // Count land vs water features
            var landCount = featuresList.Count(f => f.Land);
            var waterCount = featuresList.Count(f => !f.Land);
            System.Diagnostics.Debug.WriteLine($"   Land features: {landCount}, Water features: {waterCount}");
        }

        // Legacy ".map" parsing and embedded SVG/Voronoi extraction removed.
        // This code path previously attempted to parse the legacy FMG ".map" export and
        // extract Voronoi/grid/political polygons from mixed-format files. That functionality
        // has been intentionally removed to simplify the codebase. Only the official FMG
        // JSON export format is supported by this loader.

        // Removed legacy helpers for .map mixed-format parsing (notes, embedded SVG/Voronoi extraction, line-based sections)

        private static void ParseBurgs(JsonElement array, List<Caps.RPG.World.Models.Burg> burgsList, JsonSerializerOptions options)
        {
            int parsed = 0;
            int failed = 0;
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var burg = ExtractBurgFromJson(element);
                    if (burg != null)
                    {
                        burgsList.Add(burg);
                        parsed++;
                    }
                    else
                    {
                        failed++;
                        if (failed <= 3)
                        {
                            System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Failed to parse burg #{parsed + failed}");
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING: ✅ Successfully parsed {parsed} burgs out of {array.GetArrayLength()} elements (Failed: {failed})");
        }

        private static Caps.RPG.World.Models.Burg? ExtractBurgFromJson(JsonElement element)
        {
            try
            {
                var burg = new Caps.RPG.World.Models.Burg();

                // Extract basic properties with safe parsing
                if (element.TryGetProperty("i", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out int id))
                        burg.Id = id;
                    else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out id))
                        burg.Id = id;
                }

                if (element.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    burg.Name = nameProp.GetString();

                if (element.TryGetProperty("cell", out var cellProp))
                {
                    if (cellProp.ValueKind == JsonValueKind.Number && cellProp.TryGetInt32(out int cell))
                        burg.Cell = cell;
                    else if (cellProp.ValueKind == JsonValueKind.String && int.TryParse(cellProp.GetString(), out cell))
                        burg.Cell = cell;
                }

                // Coordinates
                if (element.TryGetProperty("x", out var xProp))
                {
                    if (xProp.ValueKind == JsonValueKind.Number && xProp.TryGetDouble(out double x))
                        burg.X = x;
                    else if (xProp.ValueKind == JsonValueKind.String && double.TryParse(xProp.GetString(), out x))
                        burg.X = x;
                }

                if (element.TryGetProperty("y", out var yProp))
                {
                    if (yProp.ValueKind == JsonValueKind.Number && yProp.TryGetDouble(out double y))
                        burg.Y = y;
                    else if (yProp.ValueKind == JsonValueKind.String && double.TryParse(yProp.GetString(), out y))
                        burg.Y = y;
                }

                // Other numeric properties
                if (element.TryGetProperty("culture", out var cultureProp))
                {
                    if (cultureProp.ValueKind == JsonValueKind.Number && cultureProp.TryGetInt32(out int culture))
                        burg.Culture = culture;
                    else if (cultureProp.ValueKind == JsonValueKind.String && int.TryParse(cultureProp.GetString(), out culture))
                        burg.Culture = culture;
                }

                if (element.TryGetProperty("state", out var stateProp))
                {
                    if (stateProp.ValueKind == JsonValueKind.Number && stateProp.TryGetInt32(out int state))
                        burg.State = state;
                    else if (stateProp.ValueKind == JsonValueKind.String && int.TryParse(stateProp.GetString(), out state))
                        burg.State = state;
                }

                if (element.TryGetProperty("population", out var popProp))
                {
                    if (popProp.ValueKind == JsonValueKind.Number && popProp.TryGetSingle(out float pop))
                        burg.Population = pop;
                    else if (popProp.ValueKind == JsonValueKind.String && float.TryParse(popProp.GetString(), out pop))
                        burg.Population = pop;
                }

                if (element.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                    burg.Type = typeProp.GetString();

                // Boolean properties
                if (element.TryGetProperty("capital", out var capitalProp))
                {
                    burg.Capital = capitalProp.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Number when capitalProp.TryGetInt32(out int capInt) => capInt != 0,
                        JsonValueKind.String when bool.TryParse(capitalProp.GetString(), out bool capBool) => capBool,
                        _ => false
                    };
                }

                return burg;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Burg extraction failed: {ex.Message}");
                return null;
            }
        }

        private static BiomesData ParseBiomesFromJson(JsonElement biomesElement)
        {
            var biomes = new BiomesData();

            if (biomesElement.TryGetProperty("i", out var iElement) && iElement.ValueKind == JsonValueKind.Array)
                biomes.BiomeIds = iElement.EnumerateArray().Select(x => x.GetInt32()).ToArray();

            if (biomesElement.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.Array)
                biomes.Names = nameElement.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();

            if (biomesElement.TryGetProperty("color", out var colorElement) && colorElement.ValueKind == JsonValueKind.Array)
                biomes.Colors = colorElement.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray();

            if (biomesElement.TryGetProperty("cost", out var costElement) && costElement.ValueKind == JsonValueKind.Array)
                biomes.MovementCost = costElement.EnumerateArray().Select(x => x.GetInt32()).ToArray();

            if (biomesElement.TryGetProperty("habitability", out var habitElement) && habitElement.ValueKind == JsonValueKind.Array)
                biomes.Habitability = habitElement.EnumerateArray().Select(x => x.GetInt32()).ToArray();

            if (biomesElement.TryGetProperty("iconsDensity", out var densityElement) && densityElement.ValueKind == JsonValueKind.Array)
                biomes.IconsDensity = densityElement.EnumerateArray().Select(x => x.GetInt32()).ToArray();

            // icons is a 2D array: outer = biome index, inner = icon name strings
            if (biomesElement.TryGetProperty("icons", out var iconsElement) && iconsElement.ValueKind == JsonValueKind.Array)
                biomes.Icons = iconsElement.EnumerateArray()
                    .Select(row => row.ValueKind == JsonValueKind.Array
                        ? row.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToArray()
                        : Array.Empty<string>())
                    .ToArray();

            // FMG uses "biomesMartix" (typo) in some versions, "biomesMatrix" in others
            foreach (var matrixKey in new[] { "biomesMartix", "biomesMatrix" })
            {
                if (biomesElement.TryGetProperty(matrixKey, out var matrixElement) && matrixElement.ValueKind == JsonValueKind.Array)
                {
                    biomes.BiomesMatrix = matrixElement.EnumerateArray()
                        .Select(row => row.ValueKind == JsonValueKind.Array
                            ? row.EnumerateArray().Select(x => (byte)x.GetInt32()).ToArray()
                            : Array.Empty<byte>())
                        .ToArray();
                    break;
                }
            }

            System.Diagnostics.Debug.WriteLine($"🚨 JSON EXPORT: Parsed biomes: {biomes.Names?.Length ?? 0} entries");
            return biomes;
        }

        private static void ParseCultures(JsonElement array, List<Caps.RPG.World.Models.Culture> culturesList, JsonSerializerOptions options)
        {
            int parsed = 0;
            int failed = 0;
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var culture = ExtractCultureFromJson(element);
                    if (culture != null)
                    {
                        culturesList.Add(culture);
                        parsed++;
                    }
                    else
                    {
                        failed++;
                        if (failed <= 3)
                            System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Failed to parse culture #{parsed + failed}");
                    }
                }
            }
            if (failed > 0)
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING: Cultures parsing - Success: {parsed}, Failed: {failed}");
        }

        private static Caps.RPG.World.Models.Culture? ExtractCultureFromJson(JsonElement element)
        {
            try
            {
                var culture = new Caps.RPG.World.Models.Culture();

                // Extract basic properties with safe parsing
                if (element.TryGetProperty("i", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out int id))
                        culture.Id = id;
                    else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out id))
                        culture.Id = id;
                }

                if (element.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    culture.Name = nameProp.GetString();

                if (element.TryGetProperty("base", out var baseProp))
                {
                    if (baseProp.ValueKind == JsonValueKind.Number && baseProp.TryGetInt32(out int baseVal))
                        culture.Base = baseVal;
                    else if (baseProp.ValueKind == JsonValueKind.String && int.TryParse(baseProp.GetString(), out baseVal))
                        culture.Base = baseVal;
                }

                // Origins array with flexible parsing
                if (element.TryGetProperty("origins", out var originsProp))
                {
                    culture.Origins = ExtractIntArrayFromJson(originsProp);
                }

                if (element.TryGetProperty("shield", out var shieldProp) && shieldProp.ValueKind == JsonValueKind.String)
                    culture.Shield = shieldProp.GetString();

                if (element.TryGetProperty("center", out var centerProp))
                {
                    if (centerProp.ValueKind == JsonValueKind.Number && centerProp.TryGetInt32(out int center))
                        culture.Center = center;
                    else if (centerProp.ValueKind == JsonValueKind.String && int.TryParse(centerProp.GetString(), out center))
                        culture.Center = center;
                }

                if (element.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
                    culture.Code = codeProp.GetString();

                if (element.TryGetProperty("color", out var colorProp) && colorProp.ValueKind == JsonValueKind.String)
                    culture.Color = colorProp.GetString();

                if (element.TryGetProperty("expansionism", out var expansionismProp))
                {
                    if (expansionismProp.ValueKind == JsonValueKind.Number && expansionismProp.TryGetDouble(out double exp))
                        culture.Expansionism = exp;
                    else if (expansionismProp.ValueKind == JsonValueKind.String && double.TryParse(expansionismProp.GetString(), out exp))
                        culture.Expansionism = exp;
                }

                if (element.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                    culture.Type = typeProp.GetString();

                return culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Culture extraction failed: {ex.Message}");
                return null;
            }
        }

        private static void ParseStates(JsonElement array, List<Caps.RPG.World.Models.State> statesList, JsonSerializerOptions options)
        {
            int parsed = 0;
            int failed = 0;
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var state = ExtractStateFromJson(element);
                    if (state != null)
                    {
                        statesList.Add(state);
                        parsed++;
                    }
                    else
                    {
                        failed++;
                        if (failed <= 3)
                        {
                            System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Failed to parse state #{parsed + failed}");
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING: States parsing complete - Success: {parsed}, Failed: {failed}");
        }

        private static Caps.RPG.World.Models.State? ExtractStateFromJson(JsonElement element)
        {
            try
            {
                var state = new Caps.RPG.World.Models.State();

                // Extract basic properties with safe parsing
                if (element.TryGetProperty("i", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out int id))
                        state.Id = id;
                    else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out id))
                        state.Id = id;
                }

                if (element.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    state.Name = nameProp.GetString();

                if (element.TryGetProperty("form", out var formProp) && formProp.ValueKind == JsonValueKind.String)
                    state.Form = formProp.GetString();

                if (element.TryGetProperty("formName", out var formNameProp) && formNameProp.ValueKind == JsonValueKind.String)
                    state.FormName = formNameProp.GetString();

                if (element.TryGetProperty("fullName", out var fullNameProp) && fullNameProp.ValueKind == JsonValueKind.String)
                    state.FullName = fullNameProp.GetString();

                if (element.TryGetProperty("color", out var colorProp) && colorProp.ValueKind == JsonValueKind.String)
                    state.Color = colorProp.GetString();

                // Numeric properties with safe parsing
                if (element.TryGetProperty("center", out var centerProp))
                {
                    if (centerProp.ValueKind == JsonValueKind.Number && centerProp.TryGetInt32(out int center))
                        state.Center = center;
                    else if (centerProp.ValueKind == JsonValueKind.String && int.TryParse(centerProp.GetString(), out center))
                        state.Center = center;
                }

                if (element.TryGetProperty("culture", out var cultureProp))
                {
                    if (cultureProp.ValueKind == JsonValueKind.Number && cultureProp.TryGetInt32(out int culture))
                        state.Culture = culture;
                    else if (cultureProp.ValueKind == JsonValueKind.String && int.TryParse(cultureProp.GetString(), out culture))
                        state.Culture = culture;
                }

                // Extract Pole array (critical for rendering - contains state coordinates)
                if (element.TryGetProperty("pole", out var poleProp) && poleProp.ValueKind == JsonValueKind.Array)
                {
                    var poleList = new List<double>();
                    foreach (var coordElement in poleProp.EnumerateArray())
                    {
                        if (coordElement.ValueKind == JsonValueKind.Number && coordElement.TryGetDouble(out double coord))
                        {
                            poleList.Add(coord);
                        }
                        else if (coordElement.ValueKind == JsonValueKind.String && double.TryParse(coordElement.GetString(), out coord))
                        {
                            poleList.Add(coord);
                        }
                    }
                    if (poleList.Count >= 2)
                    {
                        state.Pole = poleList.ToArray();
                    }
                }

                if (element.TryGetProperty("area", out var areaProp))
                {
                    if (areaProp.ValueKind == JsonValueKind.Number && areaProp.TryGetInt32(out int area))
                        state.Area = area;
                    else if (areaProp.ValueKind == JsonValueKind.String && int.TryParse(areaProp.GetString(), out area))
                        state.Area = area;
                }

                if (element.TryGetProperty("burgs", out var burgsProp))
                {
                    if (burgsProp.ValueKind == JsonValueKind.Number && burgsProp.TryGetInt32(out int burgs))
                        state.Burgs = burgs;
                    else if (burgsProp.ValueKind == JsonValueKind.String && int.TryParse(burgsProp.GetString(), out burgs))
                        state.Burgs = burgs;
                }

                if (element.TryGetProperty("cells", out var cellsProp))
                {
                    if (cellsProp.ValueKind == JsonValueKind.Number && cellsProp.TryGetInt32(out int cells))
                        state.Cells = cells;
                    else if (cellsProp.ValueKind == JsonValueKind.String && int.TryParse(cellsProp.GetString(), out cells))
                        state.Cells = cells;
                }

                // Extract other useful properties
                if (element.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                    state.Type = typeProp.GetString();

                if (element.TryGetProperty("expansionism", out var expansionismProp))
                {
                    if (expansionismProp.ValueKind == JsonValueKind.Number && expansionismProp.TryGetDouble(out double exp))
                        state.Expansionism = exp;
                    else if (expansionismProp.ValueKind == JsonValueKind.String && double.TryParse(expansionismProp.GetString(), out exp))
                        state.Expansionism = exp;
                }

                if (element.TryGetProperty("rural", out var ruralProp))
                {
                    if (ruralProp.ValueKind == JsonValueKind.Number && ruralProp.TryGetDouble(out double rural))
                        state.Rural = (float)rural;
                    else if (ruralProp.ValueKind == JsonValueKind.String && double.TryParse(ruralProp.GetString(), out rural))
                        state.Rural = (float)rural;
                }

                if (element.TryGetProperty("urban", out var urbanProp))
                {
                    if (urbanProp.ValueKind == JsonValueKind.Number && urbanProp.TryGetDouble(out double urban))
                        state.Urban = (float)urban;
                    else if (urbanProp.ValueKind == JsonValueKind.String && double.TryParse(urbanProp.GetString(), out urban))
                        state.Urban = (float)urban;
                }

                // Flexible Alert parsing (the problematic field)
                if (element.TryGetProperty("alert", out var alertProp))
                {
                    state.Alert = alertProp.ValueKind switch
                    {
                        JsonValueKind.Number when alertProp.TryGetInt32(out int alertInt) => alertInt,
                        JsonValueKind.String when int.TryParse(alertProp.GetString(), out int alertFromStr) => alertFromStr,
                        JsonValueKind.True => 1,
                        JsonValueKind.False => 0,
                        _ => 0
                    };
                }

                // Flexible Diplomacy array parsing (the other problematic field)
                if (element.TryGetProperty("diplomacy", out var diplomacyProp))
                {
                    state.Diplomacy = ExtractStringArrayFromJson(diplomacyProp);
                }

                // Other array properties
                if (element.TryGetProperty("neighbors", out var neighborsProp))
                    state.Neighbors = ExtractIntArrayFromJson(neighborsProp);

                if (element.TryGetProperty("provinces", out var provincesProp))
                    state.Provinces = ExtractIntArrayFromJson(provincesProp);

                return state;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: State extraction failed: {ex.Message}");
                return null;
            }
        }

        private static string[]? ExtractStringArrayFromJson(JsonElement element)
        {
            try
            {
                if (element.ValueKind != JsonValueKind.Array) return null;

                var list = new List<string>();
                foreach (var item in element.EnumerateArray())
                {
                    var str = item.ValueKind switch
                    {
                        JsonValueKind.String => item.GetString(),
                        JsonValueKind.Number when item.TryGetInt32(out int intVal) => intVal.ToString(),
                        JsonValueKind.Number when item.TryGetDouble(out double doubleVal) => doubleVal.ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => null,
                        _ => null
                    };
                    if (str != null) list.Add(str);
                }
                return list.Count > 0 ? list.ToArray() : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: String array extraction failed: {ex.Message}");
                return null;
            }
        }

        private static int[]? ExtractIntArrayFromJson(JsonElement element)
        {
            try
            {
                if (element.ValueKind != JsonValueKind.Array) return null;

                var list = new List<int>();
                foreach (var item in element.EnumerateArray())
                {
                    int? intValue = item.ValueKind switch
                    {
                        JsonValueKind.Number when item.TryGetInt32(out int intVal) => intVal,
                        JsonValueKind.String when int.TryParse(item.GetString(), out int parsedInt) => parsedInt,
                        JsonValueKind.True => 1,
                        JsonValueKind.False => 0,
                        _ => null
                    };
                    if (intValue.HasValue) list.Add(intValue.Value);
                }
                return list.Count > 0 ? list.ToArray() : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Int array extraction failed: {ex.Message}");
                return null;
            }
        }

        private static void ParseReligions(JsonElement array, List<Caps.RPG.World.Models.Religion> religionsList, JsonSerializerOptions options)
        {
            int parsed = 0;
            int failed = 0;
            foreach (var element in array.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var religion = ExtractReligionFromJson(element);
                    if (religion != null)
                    {
                        religionsList.Add(religion);
                        parsed++;
                    }
                    else
                    {
                        failed++;
                        if (failed <= 3)
                            System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Failed to parse religion #{parsed + failed}");
                    }
                }
            }
            if (failed > 0)
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING: Religions parsing - Success: {parsed}, Failed: {failed}");
        }

        private static Caps.RPG.World.Models.Religion? ExtractReligionFromJson(JsonElement element)
        {
            try
            {
                var religion = new Caps.RPG.World.Models.Religion();

                // Extract basic properties with safe parsing
                if (element.TryGetProperty("i", out var idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.Number && idProp.TryGetInt32(out int id))
                        religion.Id = id;
                    else if (idProp.ValueKind == JsonValueKind.String && int.TryParse(idProp.GetString(), out id))
                        religion.Id = id;
                }

                if (element.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                    religion.Name = nameProp.GetString();

                if (element.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String)
                    religion.Type = typeProp.GetString();

                if (element.TryGetProperty("form", out var formProp) && formProp.ValueKind == JsonValueKind.String)
                    religion.Form = formProp.GetString();

                if (element.TryGetProperty("deity", out var deityProp) && deityProp.ValueKind == JsonValueKind.String)
                    religion.Deity = deityProp.GetString();

                if (element.TryGetProperty("color", out var colorProp) && colorProp.ValueKind == JsonValueKind.String)
                    religion.Color = colorProp.GetString();

                if (element.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.String)
                    religion.Code = codeProp.GetString();

                // Origins array with flexible parsing
                if (element.TryGetProperty("origins", out var originsProp))
                {
                    religion.Origins = ExtractIntArrayFromJson(originsProp);
                }

                // Numeric properties
                if (element.TryGetProperty("center", out var centerProp))
                {
                    if (centerProp.ValueKind == JsonValueKind.Number && centerProp.TryGetInt32(out int center))
                        religion.Center = center;
                    else if (centerProp.ValueKind == JsonValueKind.String && int.TryParse(centerProp.GetString(), out center))
                        religion.Center = center;
                }

                if (element.TryGetProperty("culture", out var cultureProp))
                {
                    if (cultureProp.ValueKind == JsonValueKind.Number && cultureProp.TryGetInt32(out int culture))
                        religion.Culture = culture;
                    else if (cultureProp.ValueKind == JsonValueKind.String && int.TryParse(cultureProp.GetString(), out culture))
                        religion.Culture = culture;
                }

                if (element.TryGetProperty("expansionism", out var expansionismProp))
                {
                    if (expansionismProp.ValueKind == JsonValueKind.Number && expansionismProp.TryGetDouble(out double exp))
                        religion.Expansionism = exp;
                    else if (expansionismProp.ValueKind == JsonValueKind.String && double.TryParse(expansionismProp.GetString(), out exp))
                        religion.Expansionism = exp;
                }

                return religion;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"🚨 MAP LOADING ERROR: Religion extraction failed: {ex.Message}");
                return null;
            }
        }

        private static void ParseSvgContent(string content, Caps.RPG.World.Models.WorldMap map)
        {
            try
            {
                // Find the SVG section
                var svgStartIndex = content.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
                if (svgStartIndex == -1) return;

                var svgEndIndex = content.LastIndexOf("</svg>", StringComparison.OrdinalIgnoreCase);
                if (svgEndIndex == -1 || svgEndIndex <= svgStartIndex) return;

                var svgContent = content.Substring(svgStartIndex, svgEndIndex - svgStartIndex + 6);

                // Parse landmasses (features)
                ParseLandmassPolygons(svgContent, map);

                // Parse political boundaries
                ParsePoliticalPolygons(svgContent, map);

                Console.WriteLine("SVG content parsed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse SVG content: {ex.Message}");
            }
        }

        private static void ParseLandmassPolygons(string svgContent, Caps.RPG.World.Models.WorldMap map)
        {
            try
            {
                var landmassPolygons = new List<SvgPolygon>();
                int groupMatches = 0;
                int directMatches = 0;

                // FMG typically has multiple layers - focus on the right ones
                var landmassPatterns = new[]
                {
                    // Primary landmass groups
                    @"<g[^>]*id=""landmass""[^>]*>(.*?)</g>",
                    @"<g[^>]*id=""islands""[^>]*>(.*?)</g>", 
                    @"<g[^>]*id=""terrain""[^>]*>(.*?)</g>",

                    // Alternative patterns found in FMG
                    @"<g[^>]*class=""landmass""[^>]*>(.*?)</g>",
                    @"<g[^>]*class=""land""[^>]*>(.*?)</g>",
                    @"<g[^>]*data-layer=""landmass""[^>]*>(.*?)</g>",

                    // Cell-based landmass (more common in newer FMG versions)
                    @"<g[^>]*id=""cells""[^>]*>(.*?)</g>",
                    @"<g[^>]*class=""cells""[^>]*>(.*?)</g>"
                };

                foreach (var pattern in landmassPatterns)
                {
                    var matches = Regex.Matches(svgContent, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        groupMatches++;
                        var groupContent = match.Groups[1].Value;
                        ExtractLandmassPolygonsFromGroup(groupContent, landmassPolygons);
                    }
                }

                // Also look for direct landmass paths (not in groups)
                ExtractDirectLandmassPaths(svgContent, landmassPolygons, ref directMatches);

                // Remove overlapping or duplicate polygons
                var originalCount = landmassPolygons.Count;
                landmassPolygons = FilterAndPrioritizeLandmassPolygons(landmassPolygons);

                Console.WriteLine($"Found {groupMatches} group matches, {directMatches} direct path candidates");
                Console.WriteLine($"Before filtering: {originalCount} polygons, after filtering: {landmassPolygons.Count}");

                if (landmassPolygons.Count > 0)
                {
                    map.LandmassPolygons = landmassPolygons;
                }

                Console.WriteLine($"Parsed {landmassPolygons.Count} landmass polygons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse landmass polygons: {ex.Message}");
            }
        }

        private static void ExtractLandmassPolygonsFromGroup(string groupContent, List<SvgPolygon> landmassPolygons)
        {
            // More sophisticated path pattern that captures more attributes
            var pathPattern = @"<path[^>]*d=""([^""]+)""[^>]*(?:fill=""([^""]*)""|style=""[^""]*fill:\s*([^;""]*)[;""][^>]*)?[^>]*(?:data-id=""([^""]*)""|data-cell=""([^""]*)""|id=""([^""]*)""|class=""([^""]*)"")?[^>]*>";
            var matches = Regex.Matches(groupContent, pathPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var pathData = match.Groups[1].Value;
                var fill1 = match.Groups[2].Value;
                var fill2 = match.Groups[3].Value;
                var dataId = match.Groups[4].Value;
                var dataCell = match.Groups[5].Value;
                var id = match.Groups[6].Value;
                var className = match.Groups[7].Value;

                var fill = !string.IsNullOrEmpty(fill1) ? fill1 : fill2;
                var identifier = !string.IsNullOrEmpty(dataCell) ? dataCell : 
                                (!string.IsNullOrEmpty(dataId) ? dataId : 
                                (!string.IsNullOrEmpty(id) ? id : className));

                if (IsValidLandmassPath(pathData, fill, identifier, className))
                {
                    var (points, pathStart, bezierSegments) = ParseSvgPathImproved(pathData);
                    if (points.Count >= 3 && IsValidPolygon(points))
                    {
                        landmassPolygons.Add(new Caps.RPG.World.Models.Graphics.SvgPolygon
                        {
                            Points = points,
                            PathStartPoint = pathStart,
                            BezierSegments = bezierSegments,
                            Fill = fill,
                            Type = "landmass",
                            Id = identifier
                        });
                    }
                }
            }
        }

        private static void ExtractDirectLandmassPaths(string svgContent, List<SvgPolygon> landmassPolygons, ref int directMatches)
        {
            // Look for standalone path elements that represent landmasses
            // This pattern looks outside of specific groups
            var directPathPattern = @"<path[^>]*d=""([^""]+)""[^>]*(?:fill=""([^""]*)""|style=""[^""]*fill:\s*([^;""]*)[;""][^>]*)?[^>]*(?:data-id=""([^""]*)""|data-cell=""([^""]*)""|id=""([^""]*)""|class=""([^""]*)"")?[^>]*>";
            var pathMatches = Regex.Matches(svgContent, directPathPattern, RegexOptions.IgnoreCase);

            directMatches = pathMatches.Count;

            foreach (Match match in pathMatches)
            {
                var pathData = match.Groups[1].Value;
                var fill1 = match.Groups[2].Value;
                var fill2 = match.Groups[3].Value;
                var dataId = match.Groups[4].Value;
                var dataCell = match.Groups[5].Value;
                var id = match.Groups[6].Value;
                var className = match.Groups[7].Value;

                var fill = !string.IsNullOrEmpty(fill1) ? fill1 : fill2;
                var identifier = !string.IsNullOrEmpty(dataCell) ? dataCell : 
                                (!string.IsNullOrEmpty(dataId) ? dataId : 
                                (!string.IsNullOrEmpty(id) ? id : className));

                // Use basic validation for direct paths (less strict)
                if (IsValidLandmassPath(pathData, fill, identifier, className))
                {
                    var (points, pathStart, bezierSegments) = ParseSvgPathImproved(pathData);
                    if (points.Count >= 3 && IsValidPolygon(points))
                    {
                        landmassPolygons.Add(new Caps.RPG.World.Models.Graphics.SvgPolygon
                        {
                            Points = points,
                            PathStartPoint = pathStart,
                            BezierSegments = bezierSegments,
                            Fill = fill,
                            Type = "landmass",
                            Id = identifier
                        });
                    }
                }
            }
        }

        private static bool IsValidLandmassPath(string pathData, string fill, string identifier, string className)
        {
            if (string.IsNullOrEmpty(pathData)) return false;

            // More lenient initial filtering
            if (pathData.Length < 20) return false;

            // Check coordinate density - should have reasonable coordinate data
            var coordCount = Regex.Matches(pathData, @"-?\d+\.?\d*").Count;
            if (coordCount < 4) return false; // At least 2 coordinate pairs

            // Exclude obvious non-landmass identifiers (but be more lenient)
            if (!string.IsNullOrEmpty(identifier))
            {
                var lowerId = identifier.ToLower();
                if (lowerId.Contains("water") || lowerId.Contains("ocean") || lowerId.Contains("sea") ||
                    lowerId.Contains("river") || lowerId.Contains("lake") || 
                    lowerId.Contains("grid") || lowerId.Contains("graticule") || lowerId.Contains("coordinate"))
                    return false;

                // Be more lenient with border/line exclusions - only exclude obvious ones
                if (lowerId.Contains("border-line") || lowerId.Contains("route-line") || lowerId.Contains("road-line"))
                    return false;
            }

            // Check class names but be more permissive
            if (!string.IsNullOrEmpty(className))
            {
                var lowerClass = className.ToLower();
                if (lowerClass.Contains("water") || lowerClass.Contains("grid") ||
                    lowerClass.Contains("graticule") || lowerClass.Contains("coordinate"))
                    return false;
            }

            // Only exclude obvious water colors
            if (!string.IsNullOrEmpty(fill))
            {
                var lowerFill = fill.ToLower().Trim();

                // Only exclude clear water indicators
                if (lowerFill == "#4682b4" || lowerFill == "#0080ff" || lowerFill == "#007fff" ||
                    (lowerFill.StartsWith("#") && lowerFill.Length == 7 && 
                     lowerFill.Substring(1, 2) == "00" && lowerFill.Contains("ff"))) // Obvious blue colors
                    return false;
            }

            return true;
        }

        private static bool IsLikelyMainLandmass(string pathData, string fill, string identifier, string className)
        {
            // More lenient checks for direct paths

            // Should have some complexity but not too restrictive
            if (pathData.Length < 50) return false;

            // Should have some commands but not too many required
            var commandCount = Regex.Matches(pathData, @"[MLHVCSQTAZmlhvcsqtaz]").Count;
            if (commandCount < 2) return false;

            // Don't be too restrictive about colors - allow more variation
            if (!string.IsNullOrEmpty(fill))
            {
                var lowerFill = fill.ToLower().Trim();
                // Only exclude obvious bad colors
                if (lowerFill == "transparent" || lowerFill == "none")
                    return false;
            }

            return true;
        }

        private static List<SvgPolygon> FilterAndPrioritizeLandmassPolygons(List<SvgPolygon> polygons)
        {
            if (polygons.Count <= 1) return polygons;

            var filtered = new List<SvgPolygon>();

            // Sort by size (larger polygons first) and complexity
            var sorted = polygons.OrderByDescending(p => p.Points.Count)
                               .ThenByDescending(p => CalculatePolygonArea(p.Points))
                               .ToList();

            foreach (var polygon in sorted)
            {
                // Be more lenient with overlap detection
                bool significantOverlap = false;
                var polygonBounds = polygon.GetBounds();

                foreach (var existing in filtered)
                {
                    var existingBounds = existing.GetBounds();

                    // Check bounding box overlap
                    if (BoundsOverlap(polygonBounds, existingBounds))
                    {
                        // Only exclude if this polygon is much smaller AND significantly overlaps
                        var areaRatio = CalculatePolygonArea(polygon.Points) / Math.Max(CalculatePolygonArea(existing.Points), 1);
                        var sizeRatio = polygon.Points.Count / (double)Math.Max(existing.Points.Count, 1);

                        // Only exclude if it's much smaller (< 10% the area and < 20% the points)
                        if (areaRatio < 0.1 && sizeRatio < 0.2)
                        {
                            significantOverlap = true;
                            break;
                        }
                    }
                }

                if (!significantOverlap)
                {
                    filtered.Add(polygon);
                }
            }

            return filtered;
        }

        private static double CalculatePolygonArea(List<Point2> points)
        {
            if (points.Count < 3) return 0;

            double area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                int j = (i + 1) % points.Count;
                area += points[i].X * points[j].Y;
                area -= points[j].X * points[i].Y;
            }
            return Math.Abs(area) / 2;
        }

        private static bool BoundsOverlap((Point2 Min, Point2 Max) bounds1, (Point2 Min, Point2 Max) bounds2)
        {
            return bounds1.Min.X < bounds2.Max.X && bounds1.Max.X > bounds2.Min.X &&
                   bounds1.Min.Y < bounds2.Max.Y && bounds1.Max.Y > bounds2.Min.Y;
        }

        private static void ParsePoliticalPolygons(string svgContent, WorldMap map)
        {
            try
            {
                var politicalPolygons = new List<SvgPolygon>();

                // FMG stores individual state polygons - look for more comprehensive patterns
                var politicalPatterns = new[]
                {
                    // The issue: we're looking in the wrong groups! Need to find actual filled areas
                    // Look for groups that contain filled state polygons, not just labels/borders
                    @"<g[^>]*id=""stateAreas""[^>]*>(.*?)</g>",
                    @"<g[^>]*id=""regions""[^>]*>(.*?)</g>",
                    @"<g[^>]*id=""countries""[^>]*>(.*?)</g>",
                    @"<g[^>]*id=""realms""[^>]*>(.*?)</g>",

                    // Alternative class-based patterns for filled areas
                    @"<g[^>]*class=""stateAreas""[^>]*>(.*?)</g>",
                    @"<g[^>]*class=""regions""[^>]*>(.*?)</g>",
                    @"<g[^>]*class=""countries""[^>]*>(.*?)</g>",

                    // Data attribute patterns for areas (not borders/labels)
                    @"<g[^>]*data-layer=""regions""[^>]*>(.*?)</g>",
                    @"<g[^>]*data-layer=""countries""[^>]*>(.*?)</g>",

                    // Look in cells that might be colored by state
                    @"<g[^>]*id=""cells""[^>]*>(.*?)</g>",

                    // Sometimes states are in broader groups - search for filled polygons
                    @"<g[^>]*fill[^>]*>(.*?)</g>",

                    // Last resort - original patterns but we know these are probably labels/borders
                    @"<g[^>]*id=""states""[^>]*>(.*?)</g>",
                    @"<g[^>]*class=""states""[^>]*>(.*?)</g>",
                    @"<g[^>]*class=""political""[^>]*>(.*?)</g>"
                };

                int groupMatches = 0;
                foreach (var pattern in politicalPatterns)
                {
                    var matches = Regex.Matches(svgContent, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        groupMatches++;
                        var groupContent = match.Groups[1].Value;
                        ExtractPoliticalPolygonsFromGroup(groupContent, politicalPolygons);
                    }
                }

                // Look for individual state paths with political identifiers
                ExtractDirectPoliticalPaths(svgContent, politicalPolygons);

                Console.WriteLine($"Found {groupMatches} political group matches, extracted {politicalPolygons.Count} political polygons");

                if (politicalPolygons.Count > 0)
                {
                    map.PoliticalPolygons = politicalPolygons;
                }

                Console.WriteLine($"Parsed {politicalPolygons.Count} political polygons");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse political polygons: {ex.Message}");
            }
        }

        private static void ExtractPoliticalPolygonsFromGroup(string groupContent, List<SvgPolygon> politicalPolygons)
        {
            // Look for paths that represent individual states/countries
            var pathPattern = @"<path[^>]*d=""([^""]+)""[^>]*(?:fill=""([^""]*)""|style=""[^""]*fill:\s*([^;""]*)[;""][^>]*)?[^>]*(?:data-state=""([^""]*)""|data-id=""([^""]*)""|data-cell=""([^""]*)""|id=""([^""]*)""|class=""([^""]*)"")?[^>]*>";
            var matches = Regex.Matches(groupContent, pathPattern, RegexOptions.IgnoreCase);

            int pathsFound = matches.Count;
            int validPaths = 0;
            int processedPaths = 0;

            foreach (Match match in matches)
            {
                var pathData = match.Groups[1].Value;
                var fill1 = match.Groups[2].Value;
                var fill2 = match.Groups[3].Value;
                var dataState = match.Groups[4].Value;
                var dataId = match.Groups[5].Value;
                var dataCell = match.Groups[6].Value;
                var id = match.Groups[7].Value;
                var className = match.Groups[8].Value;

                var fill = !string.IsNullOrEmpty(fill1) ? fill1 : fill2;
                var identifier = !string.IsNullOrEmpty(dataState) ? dataState :
                                (!string.IsNullOrEmpty(dataCell) ? dataCell : 
                                (!string.IsNullOrEmpty(dataId) ? dataId : 
                                (!string.IsNullOrEmpty(id) ? id : className)));

                processedPaths++;

                // Use more lenient filtering for political paths
                if (IsValidPoliticalPath(pathData, fill, identifier, className))
                {
                    if (processedPaths <= 3) // Only debug first few paths
                        Console.WriteLine($"Processing political path #{processedPaths}: length={pathData.Length}");

                    var (points, pathStart, bezierSegments) = ParseSvgPathImproved(pathData);

                    if (processedPaths <= 3)
                        Console.WriteLine($"Path #{processedPaths} parsed to {points.Count} points");

                    if (points.Count >= 3)
                    {
                        // Temporarily bypass validation to extract all possible political polygons
                        politicalPolygons.Add(new Caps.RPG.World.Models.Graphics.SvgPolygon
                        {
                            Points = points,
                            PathStartPoint = pathStart,
                            BezierSegments = bezierSegments,
                            Fill = fill,
                            Type = "political",
                            Id = identifier
                        });
                        validPaths++;
                    }
                }
                else
                {
                    if (processedPaths <= 3)
                        Console.WriteLine($"Path #{processedPaths} rejected by IsValidPoliticalPath");
                }
            }

            Console.WriteLine($"Group political extraction: {pathsFound} paths found, {validPaths} valid political polygons extracted");
        }

        private static bool IsValidPoliticalPath(string pathData, string fill, string identifier, string className)
        {
            if (string.IsNullOrEmpty(pathData)) return false;

            if (pathData.Length < 20) return false;

            // Exclude text label paths immediately
            if (pathData.StartsWith("textPath") || pathData.Contains("Label") || pathData.Contains("Text"))
            {
                return false;
            }

            // Check coordinate density - must have actual coordinate data
            var coordCount = Regex.Matches(pathData, @"-?\d+\.?\d*").Count;
            if (coordCount < 6) return false; // Need more coordinates for a meaningful political region

            // Very minimal exclusions for geographic data
            if (!string.IsNullOrEmpty(identifier))
            {
                var lowerId = identifier.ToLower();

                // Exclude text/label paths
                if (lowerId.Contains("textpath") || lowerId.Contains("label") || lowerId.Contains("text"))
                    return false;

                if (lowerId.Contains("water") || lowerId.Contains("ocean") || lowerId.Contains("sea"))
                    return false;
            }

            return true;
        }

        private static void ExtractDirectPoliticalPaths(string svgContent, List<SvgPolygon> politicalPolygons)
        {
            // Look for individual state paths that might not be in specific groups
            var directPathPattern = @"<path[^>]*d=""([^""]+)""[^>]*(?:fill=""([^""]*)""|style=""[^""]*fill:\s*([^;""]*)[;""][^>]*)?[^>]*(?:data-state=""([^""]*)""|data-id=""([^""]*)""|data-cell=""([^""]*)""|id=""([^""]*)""|class=""([^""]*)"")?[^>]*>";
            var pathMatches = Regex.Matches(svgContent, directPathPattern, RegexOptions.IgnoreCase);

            foreach (Match match in pathMatches)
            {
                var pathData = match.Groups[1].Value;
                var fill1 = match.Groups[2].Value;
                var fill2 = match.Groups[3].Value;
                var dataState = match.Groups[4].Value;
                var dataId = match.Groups[5].Value;
                var dataCell = match.Groups[6].Value;
                var id = match.Groups[7].Value;
                var className = match.Groups[8].Value;

                var fill = !string.IsNullOrEmpty(fill1) ? fill1 : fill2;
                var identifier = !string.IsNullOrEmpty(dataState) ? dataState :
                                (!string.IsNullOrEmpty(dataCell) ? dataCell : 
                                (!string.IsNullOrEmpty(dataId) ? dataId : 
                                (!string.IsNullOrEmpty(id) ? id : className)));

                // More restrictive filtering for direct paths - must have clear political indicators
                if (IsPoliticalPath(pathData, fill, identifier, className) && 
                    HasPoliticalIndicators(fill, identifier, className))
                {
                    var (points, pathStart, bezierSegments) = ParseSvgPathImproved(pathData);
                    if (points.Count >= 3 && IsValidPolygon(points))
                    {
                        politicalPolygons.Add(new Caps.RPG.World.Models.Graphics.SvgPolygon
                        {
                            Points = points,
                            PathStartPoint = pathStart,
                            BezierSegments = bezierSegments,
                            Fill = fill,
                            Type = "political", 
                            Id = identifier
                        });
                    }
                }
            }
        }

        private static bool IsPoliticalPath(string pathData, string fill, string identifier, string className)
        {
            // Use the more lenient validation method
            return IsValidPoliticalPath(pathData, fill, identifier, className);
        }

        private static bool HasPoliticalIndicators(string fill, string identifier, string className)
        {
            // For direct paths, require stronger political indicators

            // Must have a meaningful fill color (political areas are always colored)
            if (!string.IsNullOrEmpty(fill))
            {
                var lowerFill = fill.ToLower().Trim();
                if (lowerFill != "transparent" && lowerFill != "none" && 
                    lowerFill != "#000000" && lowerFill != "black" &&
                    !lowerFill.StartsWith("url(")) // Not pattern references
                    return true;
            }

            // Or have clear political identifiers
            if (!string.IsNullOrEmpty(identifier))
            {
                var lowerId = identifier.ToLower();
                if (lowerId.Contains("state") || lowerId.Contains("country") || 
                    lowerId.Contains("political") || lowerId.Contains("realm") || 
                    lowerId.Contains("region") || lowerId.Contains("kingdom"))
                    return true;
            }

            if (!string.IsNullOrEmpty(className))
            {
                var lowerClass = className.ToLower();
                if (lowerClass.Contains("state") || lowerClass.Contains("country") || 
                    lowerClass.Contains("political") || lowerClass.Contains("realm") || 
                    lowerClass.Contains("region") || lowerClass.Contains("kingdom"))
                    return true;
            }

            return false;
        }

        private static void ExtractPolygonsFromGroup(string groupContent, List<SvgPolygon> polygons, string type)
        {
            // This method is now specific to non-landmass groups (political, etc.)
            var pathPattern = @"<path[^>]*d=""([^""]+)""[^>]*(?:fill=""([^""]*)""|style=""[^""]*fill:([^;""]*)[;""][^>]*)?[^>]*(?:data-id=""([^""]*)""|id=""([^""]*)""|class=""([^""]*)"")?[^>]*>";
            var matches = Regex.Matches(groupContent, pathPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var pathData = match.Groups[1].Value;
                var fill1 = match.Groups[2].Value;
                var fill2 = match.Groups[3].Value;
                var dataId = match.Groups[4].Value;
                var id = match.Groups[5].Value;
                var className = match.Groups[6].Value;

                var fill = !string.IsNullOrEmpty(fill1) ? fill1 : fill2;
                var identifier = !string.IsNullOrEmpty(dataId) ? dataId : (!string.IsNullOrEmpty(id) ? id : className);

                // Basic validation for non-landmass polygons
                if (pathData.Length > 20)
                {
                    var (points, pathStart, bezierSegments) = ParseSvgPathImproved(pathData);
                    if (points.Count >= 3 && IsValidPolygon(points))
                    {
                        polygons.Add(new Caps.RPG.World.Models.Graphics.SvgPolygon
                        {
                            Points = points,
                            PathStartPoint = pathStart,
                            BezierSegments = bezierSegments,
                            Fill = fill,
                            Type = type,
                            Id = identifier
                        });
                    }
                }
            }
        }

        private static bool IsLandmassPath(string pathData, string fill, string identifier)
        {
            // Keep this method for backward compatibility but redirect to the new method
            return IsValidLandmassPath(pathData, fill, identifier, "");
        }

        private static (List<Point2> Points, Point2? PathStart, List<SvgCubicSegment>? BezierSegments) ParseSvgPathImproved(string pathData)
        {
            var points = new List<Point2>();
            var bezierSegs = new List<SvgCubicSegment>();
            Point2? pathStartPoint = null;
            bool allSegmentsCubic = true;

            try
            {
                // Clean up the path data
                pathData = pathData.Trim();
                if (string.IsNullOrEmpty(pathData)) return (points, null, null);

                // More robust SVG path parser
                var commands = SplitPathCommands(pathData);
                Point2 currentPoint = new Caps.RPG.World.Models.Graphics.Point2(0, 0);
                Point2 startPoint = new Caps.RPG.World.Models.Graphics.Point2(0, 0);
                Point2 lastControlPoint = new Caps.RPG.World.Models.Graphics.Point2(0, 0);

                foreach (var command in commands)
                {
                    if (string.IsNullOrEmpty(command.Trim())) continue;

                    var cmd = command[0];
                    var parameters = command.Substring(1).Trim();

                    switch (cmd)
                    {
                        case 'M': // Move to (absolute)
                            var coords = ParseCoordinatesRobust(parameters);
                            if (coords.Count >= 2)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(coords[0], coords[1]);
                                startPoint = currentPoint;
                                pathStartPoint ??= currentPoint;
                                points.Add(currentPoint);

                                // Additional coordinate pairs are treated as line-to
                                for (int i = 2; i < coords.Count - 1; i += 2)
                                {
                                    currentPoint = new Caps.RPG.World.Models.Graphics.Point2(coords[i], coords[i + 1]);
                                    points.Add(currentPoint);
                                }
                            }
                            break;

                        case 'm': // Move to (relative)
                            coords = ParseCoordinatesRobust(parameters);
                            if (coords.Count >= 2)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + coords[0], currentPoint.Y + coords[1]);
                                startPoint = currentPoint;
                                pathStartPoint ??= currentPoint;
                                points.Add(currentPoint);

                                for (int i = 2; i < coords.Count - 1; i += 2)
                                {
                                    currentPoint = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + coords[i], currentPoint.Y + coords[i + 1]);
                                    points.Add(currentPoint);
                                }
                            }
                            break;

                        case 'L': // Line to (absolute)
                            allSegmentsCubic = false;
                            coords = ParseCoordinatesRobust(parameters);
                            for (int i = 0; i < coords.Count - 1; i += 2)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(coords[i], coords[i + 1]);
                                points.Add(currentPoint);
                            }
                            break;

                        case 'l': // Line to (relative)
                            allSegmentsCubic = false;
                            coords = ParseCoordinatesRobust(parameters);
                            for (int i = 0; i < coords.Count - 1; i += 2)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + coords[i], currentPoint.Y + coords[i + 1]);
                                points.Add(currentPoint);
                            }
                            break;

                        case 'H': // Horizontal line to (absolute)
                            allSegmentsCubic = false;
                            var xValues = ParseCoordinatesRobust(parameters);
                            foreach (var x in xValues)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(x, currentPoint.Y);
                                points.Add(currentPoint);
                            }
                            break;

                        case 'h': // Horizontal line to (relative)
                            allSegmentsCubic = false;
                            xValues = ParseCoordinatesRobust(parameters);
                            foreach (var x in xValues)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + x, currentPoint.Y);
                                points.Add(currentPoint);
                            }
                            break;

                        case 'V': // Vertical line to (absolute)
                            allSegmentsCubic = false;
                            var yValues = ParseCoordinatesRobust(parameters);
                            foreach (var y in yValues)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X, y);
                                points.Add(currentPoint);
                            }
                            break;

                        case 'v': // Vertical line to (relative)
                            allSegmentsCubic = false;
                            yValues = ParseCoordinatesRobust(parameters);
                            foreach (var y in yValues)
                            {
                                currentPoint = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X, currentPoint.Y + y);
                                points.Add(currentPoint);
                            }
                            break;

                        case 'C': // Cubic Bezier curve (absolute)
                            coords = ParseCoordinatesRobust(parameters);
                            for (int i = 0; i < coords.Count - 5; i += 6)
                            {
                                var cp1 = new Caps.RPG.World.Models.Graphics.Point2(coords[i], coords[i + 1]);
                                var cp2 = new Caps.RPG.World.Models.Graphics.Point2(coords[i + 2], coords[i + 3]);
                                var end = new Caps.RPG.World.Models.Graphics.Point2(coords[i + 4], coords[i + 5]);

                                ApproximateCubicBezier(points, currentPoint, cp1, cp2, end);
                                bezierSegs.Add(new Caps.RPG.World.Models.Graphics.SvgCubicSegment(cp1, cp2, end));
                                currentPoint = end;
                                lastControlPoint = cp2;
                            }
                            break;

                        case 'c': // Cubic Bezier curve (relative)
                            coords = ParseCoordinatesRobust(parameters);
                            for (int i = 0; i < coords.Count - 5; i += 6)
                            {
                                var cp1 = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + coords[i], currentPoint.Y + coords[i + 1]);
                                var cp2 = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + coords[i + 2], currentPoint.Y + coords[i + 3]);
                                var end = new Caps.RPG.World.Models.Graphics.Point2(currentPoint.X + coords[i + 4], currentPoint.Y + coords[i + 5]);

                                ApproximateCubicBezier(points, currentPoint, cp1, cp2, end);
                                bezierSegs.Add(new Caps.RPG.World.Models.Graphics.SvgCubicSegment(cp1, cp2, end));
                                currentPoint = end;
                                lastControlPoint = cp2;
                            }
                            break;

                        case 'S': // Smooth cubic Bezier (absolute)
                            allSegmentsCubic = false; // S commands depend on previous cp2; skip native bezier path for these
                            coords = ParseCoordinatesRobust(parameters);
                            for (int i = 0; i < coords.Count - 3; i += 4)
                            {
                                var cp1 = new Caps.RPG.World.Models.Graphics.Point2(2 * currentPoint.X - lastControlPoint.X, 2 * currentPoint.Y - lastControlPoint.Y);
                                var cp2 = new Caps.RPG.World.Models.Graphics.Point2(coords[i], coords[i + 1]);
                                var end = new Caps.RPG.World.Models.Graphics.Point2(coords[i + 2], coords[i + 3]);

                                ApproximateCubicBezier(points, currentPoint, cp1, cp2, end);
                                currentPoint = end;
                                lastControlPoint = cp2;
                            }
                            break;

                        case 'Z':
                        case 'z': // Close path
                            if (points.Count > 0 && (Math.Abs(points[^1].X - startPoint.X) > 0.1 || Math.Abs(points[^1].Y - startPoint.Y) > 0.1))
                            {
                                points.Add(startPoint);
                            }
                            break;
                    }
                }

                // Clean up duplicate consecutive points
                points = RemoveConsecutiveDuplicates(points);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing SVG path: {ex.Message}");
            }

            var hasBezierPath = allSegmentsCubic && bezierSegs.Count > 0;
            return (points, hasBezierPath ? pathStartPoint : null, hasBezierPath ? bezierSegs : null);
        }

        private static List<string> SplitPathCommands(string pathData)
        {
            // More robust command splitting that handles edge cases
            var commands = new List<string>();
            var currentCommand = "";
            var inCommand = false;

            for (int i = 0; i < pathData.Length; i++)
            {
                char c = pathData[i];

                if (char.IsLetter(c))
                {
                    if (inCommand && !string.IsNullOrEmpty(currentCommand.Trim()))
                    {
                        commands.Add(currentCommand.Trim());
                    }
                    currentCommand = c.ToString();
                    inCommand = true;
                }
                else
                {
                    currentCommand += c;
                }
            }

            if (inCommand && !string.IsNullOrEmpty(currentCommand.Trim()))
            {
                commands.Add(currentCommand.Trim());
            }

            return commands;
        }

        private static void ApproximateCubicBezier(List<Point2> points, Point2 start, Point2 cp1, Point2 cp2, Point2 end)
        {
            // Approximate cubic Bezier curve with line segments
            const int segments = 8; // Adjust for smoothness vs performance

            for (int i = 1; i <= segments; i++)
            {
                double t = (double)i / segments;
                double t2 = t * t;
                double t3 = t2 * t;
                double mt = 1 - t;
                double mt2 = mt * mt;
                double mt3 = mt2 * mt;

                double x = mt3 * start.X + 3 * mt2 * t * cp1.X + 3 * mt * t2 * cp2.X + t3 * end.X;
                double y = mt3 * start.Y + 3 * mt2 * t * cp1.Y + 3 * mt * t2 * cp2.Y + t3 * end.Y;

                points.Add(new Caps.RPG.World.Models.Graphics.Point2(x, y));
            }
        }

        private static List<Point2> RemoveConsecutiveDuplicates(List<Point2> points)
        {
            if (points.Count <= 1) return points;

            var cleaned = new List<Point2> { points[0] };

            for (int i = 1; i < points.Count; i++)
            {
                var current = points[i];
                var last = cleaned[^1];

                // Only add if it's significantly different from the last point
                if (Math.Abs(current.X - last.X) > 0.1 || Math.Abs(current.Y - last.Y) > 0.1)
                {
                    cleaned.Add(current);
                }
            }

            return cleaned;
        }

        private static bool IsValidPolygon(List<Point2> points)
        {
            if (points.Count < 3) return false;

            // Check for reasonable bounding box (not too small or too large)
            var minX = points.Min(p => p.X);
            var maxX = points.Max(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxY = points.Max(p => p.Y);

            var width = maxX - minX;
            var height = maxY - minY;

            // Skip tiny polygons (likely noise) or impossibly large ones
            if (width < 5 || height < 5 || width > 10000 || height > 10000)
                return false;

            // Check for reasonable aspect ratio (not extremely thin lines)
            var aspectRatio = Math.Max(width, height) / Math.Max(Math.Min(width, height), 1);
            if (aspectRatio > 50) return false;

            return true;
        }

        private static bool IsValidPoliticalPolygon(List<Point2> points)
        {
            if (points.Count < 3) return false;

            // Much more lenient validation for political polygons
            var minX = points.Min(p => p.X);
            var maxX = points.Max(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxY = points.Max(p => p.Y);

            var width = maxX - minX;
            var height = maxY - minY;

            // Allow much smaller polygons - political regions can be quite small
            if (width < 1 || height < 1) return false; // Very lenient

            // Allow much larger polygons too
            if (width > 50000 || height > 50000) return false; // Very lenient

            // Allow much more extreme aspect ratios for political regions
            var aspectRatio = Math.Max(width, height) / Math.Max(Math.Min(width, height), 1);
            if (aspectRatio > 200) return false; // Very lenient

            return true;
        }

        private static List<double> ParseCoordinatesRobust(string coordinateString)
        {
            var coordinates = new List<double>();

            if (string.IsNullOrEmpty(coordinateString)) return coordinates;

            // Handle various coordinate formats including negative numbers and scientific notation
            coordinateString = coordinateString.Trim();

            // Replace multiple whitespace with single space
            coordinateString = Regex.Replace(coordinateString, @"\s+", " ");

            // Split by whitespace, commas, or combination
            var parts = Regex.Split(coordinateString, @"[,\s]+")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            foreach (var part in parts)
            {
                // Handle cases where coordinates might be concatenated (e.g., "123.45-67.89")
                var cleanPart = part.Trim();

                // Split on minus signs that aren't at the start or after 'e'/'E'
                var subParts = Regex.Split(cleanPart, @"(?<!^)(?<!e)(?<!E)-").Where(p => !string.IsNullOrEmpty(p));

                bool first = true;
                foreach (var subPart in subParts)
                {
                    var valueToParse = first ? subPart : "-" + subPart;
                    first = false;

                    if (double.TryParse(valueToParse, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        // Sanity check for reasonable coordinate values
                        if (Math.Abs(value) < 1000000) // Reject extremely large values
                        {
                            coordinates.Add(value);
                        }
                    }
                }
            }

            return coordinates;
        }

        // ENHANCED FMG PARSING METHODS FOR POLITICAL MAP DATA
        // Removed additional legacy FMG/SVG/Voronoi extraction helpers. These routines
        // attempted to parse mixed-format ".map" exports and to reconstruct Voronoi
        // structures from embedded SVG or grid data. That functionality is no longer
        // supported and has been removed to keep the loader focused on the official
        // FMG JSON export format.
    }

    internal class Point2JsonConverter : JsonConverter<Point2>
    {
        public override Point2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                if (!reader.Read()) throw new JsonException("Unexpected end while reading Point2 array");
                double x = reader.GetDouble();
                if (!reader.Read()) throw new JsonException("Unexpected end while reading Point2 array");
                double y = reader.GetDouble();
                if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray) throw new JsonException("Expected end of Point2 array");
                return new Caps.RPG.World.Models.Graphics.Point2(x, y);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;
                double x = 0, y = 0;
                if (root.TryGetProperty("x", out var px) || root.TryGetProperty("X", out px)) x = px.GetDouble();
                if (root.TryGetProperty("y", out var py) || root.TryGetProperty("Y", out py)) y = py.GetDouble();
                return new Caps.RPG.World.Models.Graphics.Point2(x, y);
            }

            throw new JsonException($"Cannot convert token {reader.TokenType} to Point2");
        }

        public override void Write(Utf8JsonWriter writer, Point2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }

    internal class IntToBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32() != 0;
            }
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            throw new JsonException($"Cannot convert token {reader.TokenType} to Boolean");
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value ? 1 : 0);
        }
    }
}
