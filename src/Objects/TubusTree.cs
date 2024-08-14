
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

        public AbstractTubusTree abstractTubus => abstractPhysicalObject as AbstractTubusTree;
        public int seed;
        public Vector2 origPos;
        public TubusTree(AbstractPhysicalObject abstractPhysicalObj) : base(abstractPhysicalObj) 
        {
            bodyChunks = new BodyChunk[2];
            bodyChunks[0] = new(this, 0, default, 22f, 30f);
            bodyChunks[1] = new(this, 0, default, 12f, 30f);
            bodyChunkConnections = new BodyChunkConnection[1];
            bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], 25f, BodyChunkConnection.Type.Normal, 1f, -1f);
            collisionLayer = 1;
            seed = ((TubusData)abstractTubus.placedObject.data).GetValue<int>("seed");
            origPos = abstractTubus.placedObject.pos;
        }
        public override void InitiateGraphicsModule()
        {
            graphicsModule ??= new TubusTreeGraphics(this);
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            bodyChunks[0].HardSetPosition(room.MiddleOfTile(origPos));
            bodyChunks[1].HardSetPosition(bodyChunks[0].pos + new Vector2(0f, 20f));
        }
    }
}
