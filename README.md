# Topdown Terrain for Godot 4.6 (C#)
Adds a `TopdownTerrain` node using multiple `TileMapLayer` to display a map with floor and walls.
To procedurally generate a `TopdownTerrain` map, use a `TerrainGenerator` node with a `GenerationPipeline`.

## Generation Pipeline
Terrain generation follows these steps:
- Subdivide the map into rectangles (rooms) using Binary Space Partitioning
  - `BSPTreeFeature` allow manipulating the `BSPTree`
- Produce a `RoomGraph` from the `BSPTree` to determine connections between rooms
  - `RoomGraphFeature` allow manipulating the `RoomGraph`
- Run every `RoomGenerator` on the list of not-yet-generated rooms, generating any amount of rooms (hiding generated rooms from the next `RoomGenerator`s)
  - Apply a list of `RoomFeature` to generate the room

## Features
### BSP Tree Feature
- `TreeMergeFeature` undoing BSP splits to make some rooms twice as big

### Room Graph Feature
- `FilledRoomFeature` replace rooms with solid wall, ensuring all rooms stay connected

### Room Feature
- `WallFeature` add a rectangular wall surrounding the room, leaving an interior
- `CorridorFeature` fill the room with wall, leaving only corridors between the connections
- `ColumnFeature` add columns of wall to the room

- `FloorFeature` fill the floor with a single material
- `FloorDecoFeature` add patches of a floor material

- `RoomFeatureCollection` groups a list of `RoomFeature`s for convenience
- `RoomFeatureOptions` applies only one of a list of `RoomFeature`s
- `WeightedRoomFeature` associates a `RoomFeature` with a weight for being picked by `RoomFeatureOptions`

## Materials and Tiles
Materials are used to set the floor/wall cells of `TopdownTerrain` and define which tiles or terrain to use in which layers.
- `WallMaterial` a collection of tiles to use for a wall cell
- `FloorMaterial` a collection of tiles to use for a floor cell
- `TerrainTile` a Terrain defined in the TileSet
- `RandomTile` a region on an atlas of a TileSet
