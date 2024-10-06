
using static Tubus.Objects.SapGlob.SapGlob;
using static Tubus.Objects.ObjectTypes;

namespace Tubus.Objects.TubusTree
{
    public class TubusTree : PhysicalObject
    {
        public class AbstractTubusTree : AbstractPhysicalObject
        {
            public readonly PlacedObject placedObject;
            public AbstractTubusTree(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, PlacedObject pObj)
                : base(world, ObjectTypes.AbstractTubusTree, realizedObject, pos, ID)
            {
                placedObject = pObj;
            }
            public override void Realize()
            {
                base.Realize();
                realizedObject = new TubusTree(this, world);

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
        public bool inAir { get; private set; }
        public int seed;
        public Vector2 origPos;

        public TubusTree(AbstractPhysicalObject abstractPhysicalObj, World world) : base(abstractPhysicalObj)
        {
            collisionLayer = 1;
            seed = ((TubusData)abstractTubus.placedObject.data).GetValue<int>("seed");
            // This is how you access your ManagedField's values from the placedObject.data, you cast it to your ManagedData class and use the GetValue<T>(string fieldName) method
            // T is the type of value you want and fieldName is the key from your ManagedField

            origPos = abstractTubus.placedObject.pos;
            room = world.GetAbstractRoom(abstractPhysicalObj.pos.room).realizedRoom;

            if (room != null)
            {
                inAir = room.GetTile(room.GetTilePosition(origPos)).Terrain == Room.Tile.TerrainType.Air && room.GetTile(room.GetTilePosition(origPos - new Vector2(0f, 10f)) - new IntVector2(0, 1)).Terrain == Room.Tile.TerrainType.Air;
                int chunkNum = inAir ? 2 : 3;
                bodyChunks = new BodyChunk[chunkNum];
                if (inAir)
                {
                    bodyChunks[0] = new(this, 0, default, 20f, 80f);
                    bodyChunks[1] = new(this, 1, default, 18f, 60f);
                    bodyChunks[1].collideWithObjects = false;
                    bodyChunkConnections = new BodyChunkConnection[1];
                    bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, -1f);
                }
                else
                {
                    bodyChunks[0] = new(this, 0, default, 30f, 80f);
                    bodyChunks[1] = new(this, 1, default, 20f, 80f);
                    bodyChunks[2] = new(this, 2, default, 18f, 60f);
                    bodyChunks[2].collideWithObjects = false;
                    bodyChunkConnections = new BodyChunkConnection[2];
                    bodyChunkConnections[0] = new(bodyChunks[0], bodyChunks[1], 20f, BodyChunkConnection.Type.Normal, 1f, -1f);
                    bodyChunkConnections[1] = new(bodyChunks[1], bodyChunks[2], 14f, BodyChunkConnection.Type.Normal, 1f, -1f);
                }

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
            //being hit makes branches shake TODO
            //gourmand slide bonk
            if (otherObject is Player player)
            {
                // dont boink below certain velocity
                if (player.animation == Player.AnimationIndex.BellySlide && otherObject.bodyChunks[otherChunk].vel.x > 6f)
                {
                    float dir = Custom.Angle(bodyChunks[myChunk].pos, otherObject.bodyChunks[otherChunk].pos);
                    if (dir > 0f)
                    {
                        room.PlaySound(SoundID.Rock_Hit_Creature, bodyChunks[myChunk]);
                        room.AddObject(new ExplosionSpikes(room, bodyChunks[myChunk].pos + Custom.DirVec(bodyChunks[myChunk].pos, otherObject.bodyChunks[otherChunk].pos) * otherObject.bodyChunks[otherChunk].rad, 5, 2f, 8f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
                        otherObject.bodyChunks[otherChunk].vel *= -0.75f;
                        player.Stun(35);
                    }
                }
            }
        }
        public void CreateGlob(Vector2 pos, int index)
        {
            float stickAngle = Custom.AimFromOneVectorToAnother(bodyChunks[index].pos, pos);
            pos = bodyChunks[index].pos + Custom.DirVec(bodyChunks[index].pos, stickAngle) * bodyChunks[index].rad;

            AbstractSapGlob abstractSapGlob = new(room.world, null, room.GetWorldCoordinate(pos), room.game.GetNewID());
            room.abstractRoom.AddEntity(abstractSapGlob);
            abstractSapGlob.pos = room.GetWorldCoordinate(origPos);
            abstractSapGlob.RealizeInRoom();
            (abstractSapGlob.realizedObject as SapGlob.SapGlob).tapPos = pos;
        }
    }
}
