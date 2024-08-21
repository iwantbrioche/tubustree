
using Tubus.PomObjects;

namespace Tubus.Objects
{
    public class TubusTree : PhysicalObject
    {
        public class AbstractTubusTree : AbstractPhysicalObject
        {
            public readonly PlacedObject placedObject;
            public AbstractTubusTree(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject pObj)
                : base(world, TubusTypes.AbstractTubusTree, realizedObject, pos, ID)
            {
                placedObject = pObj;
            }
            public override void Realize()
            {
                base.Realize();
                realizedObject = new TubusTree(this);

            }
        }
        public new BodyChunk firstChunk
        {
            get
            {
                if (inAir) return bodyChunks[0];
                return bodyChunks[1];
            }
        }
        public BodyChunk topChunk
        {
            get
            {
                if (inAir) return bodyChunks[1];
                return bodyChunks[2];
            }
        }
        public BodyChunk bottomChunk
        {
            get
            {
                if (inAir) return null;
                return bodyChunks[0];
            }
        }
        public AbstractTubusTree abstractTubus => abstractPhysicalObject as AbstractTubusTree;
        public bool inAir;
        public int seed;
        public Vector2 origPos;

        public TubusTree(AbstractPhysicalObject abstractPhysicalObj) : base(abstractPhysicalObj) 
        {
            collisionLayer = 1;
            seed = ((TubusData)abstractTubus.placedObject.data).GetValue<int>("seed");
            origPos = abstractTubus.placedObject.pos;
        }
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            inAir = room.GetTile(room.GetTilePosition(origPos)).Terrain == Room.Tile.TerrainType.Air && room.GetTile(room.GetTilePosition(origPos) - new IntVector2(0, 1)).Terrain == Room.Tile.TerrainType.Air;
            int chunkNum = inAir ? 2 : 3;
            bodyChunks = new BodyChunk[chunkNum];
            if (inAir)
            {
                bodyChunks[0] = new(this, 0, default, 20f, 80f);
                bodyChunks[1] = new(this, 1, default, 16f, 60f);
                bodyChunks[1].collideWithObjects = false;
                bodyChunkConnections = new BodyChunkConnection[1];
                bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, -1f);
            }
            else
            {
                bodyChunks[0] = new(this, 0, default, 30f, 80f);
                bodyChunks[1] = new(this, 0, default, 20f, 80f);
                bodyChunks[2] = new(this, 1, default, 16f, 60f);
                bodyChunks[2].collideWithObjects = false;
                bodyChunkConnections = new BodyChunkConnection[2];
                bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], 20f, BodyChunkConnection.Type.Normal, 1f, -1f);
                bodyChunkConnections[1] = new(bodyChunks[1], bodyChunks[2], 14f, BodyChunkConnection.Type.Normal, 1f, -1f);
            }
        }
        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new TubusTreeGraphics(this);
        }
        public override void Update(bool eu)
        {
            Random.State state = Random.state;
            Random.InitState(seed);
            bottomChunk?.HardSetPosition(origPos - new Vector2(0f, 30f));
            firstChunk.HardSetPosition(origPos);
            topChunk.HardSetPosition(firstChunk.pos + Custom.RotateAroundOrigo(new Vector2(0f, 10f), Random.Range(-8f, 8f)));
            Random.state = state;
            base.Update(eu);
        }
        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            //being hit makes branches shake
            //gourmand slide bonk
        }
    }
}
