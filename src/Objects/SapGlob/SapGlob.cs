
namespace Tubus.Objects.SapGlob
{
    public class SapGlob : PlayerCarryableItem, IDrawable, IPlayerEdible
    {
        public class AbstractSapGlob : AbstractConsumable
        {
            public AbstractSapGlob(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID id)
                : base(world, ObjectTypes.SapGlob, realizedObject, pos, id, -1, -1, null)
            { 
            }
            public override void Realize()
            {
                base.Realize();
                realizedObject = new SapGlob(this);
            }
        }

        private Vector2 rotation;
        private Vector2 lastRotation;
        public Vector2 tapPos;
        public bool harvested;
        public int harvestTimer;
        public int harvestTimerMax = 200;
        public float splat;
        public int splatDir;
        private float puddle;
        private int bites = 5;
        public Spear tapSpear;
        public TubusTree.TubusTree tubus;
        public int BitesLeft => bites;
        public int FoodPoints => 2;
        public bool Edible => true;
        public bool AutomaticPickUp => true;
        public SapGlob(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new(this, 0, default, 3f, 0.25f);
            bodyChunkConnections = [];
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0f;
            surfaceFriction = 0.9f;
            collisionLayer = 2;
            waterFriction = 0.95f;
            buoyancy = 0f;
            canBeHitByWeapons = false;
            CollideWithObjects = false;
        }
        public override void Update(bool eu)
        {
            // gloopy effect, drips downward slowly
            // splats on ground and puddles outward slowly
            // leaves little droplets of sap that spread out when moving across surface
            // drips of sap come off
            base.Update(eu);
            lastRotation = rotation;

            if (harvestTimer < harvestTimerMax)
            {
                harvestTimer++;
            }
            if (splat > 0f)
            {
                splat *= 0.8f;
            }

            rotation = Vector2.ClampMagnitude(firstChunk.vel, 1f);
            if (grabbedBy.Count > 0)
            {
                harvested = true;
                if (collisionLayer == 2) room.ChangeCollisionLayerForObject(this, 1);
                CollideWithObjects = true;
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                rotation.y = Mathf.Abs(rotation.y);
            }
            if (firstChunk.ContactPoint.y != 0)
            {
                firstChunk.vel.x *= 0.7f;
                rotation = Vector2.MoveTowards(rotation, Vector2.down, Mathf.InverseLerp(1f, 0f, rotation.y));
                if (puddle < 0.75f)
                {
                    puddle += (0.03f - (0.03f * puddle)) * (1f + splat);
                }
            }
            else
            {
                puddle = 0f;
            }
            if (firstChunk.ContactPoint.x != 0)
            {
                firstChunk.vel.y *= 0.4f;
                firstChunk.vel.x += 0.1f * firstChunk.ContactPoint.x;
                if (firstChunk.vel.y > 0.4f)
                {
                    firstChunk.vel.x += 0.1f * firstChunk.ContactPoint.x;
                }
            }
            if (!harvested)
            {
                firstChunk.HardSetPosition(tapPos);
                firstChunk.pos = tapPos;
                firstChunk.lastPos = tapPos;
                firstChunk.vel = default;
            }
        }
        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);
            if (speed > 1.2f && firstContact)
            {
                room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, firstChunk, false, Custom.LerpMap(speed, 1.2f, 6f, 0.2f, 1f), 1f);
                splat = speed / 10f;
                splatDir = direction.y;
            }
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("Futile_White");
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
            sLeaser.sprites[0].alpha = 0.3f;
            sLeaser.sprites[1] = new FSprite("Futile_White");
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["JaggedCircle"];
            sLeaser.sprites[1].alpha = 0.5f;
            AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 r = Vector3.Slerp(lastRotation, rotation, timeStacker);

            sLeaser.sprites[0].SetPosition(pos - camPos);
            sLeaser.sprites[0].rotation = Custom.VecToDeg(r);
            sLeaser.sprites[1].anchorY = 0.75f;
            sLeaser.sprites[1].SetPosition(pos - camPos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(r);

            if (harvestTimer < harvestTimerMax)
            {
                sLeaser.sprites[0].scale = 0.1f + ((harvestTimer / (float)harvestTimerMax) - 0.1f);
                sLeaser.sprites[1].scale = 0.1f + ((harvestTimer / (float)harvestTimerMax) - 0.1f);
            }
            else
            {
                sLeaser.sprites[0].scale = Mathf.Lerp(0.2f, 1f, bites / 5f);
                sLeaser.sprites[1].scale = Mathf.Lerp(0.2f, 1f, bites / 5f);
            }

            if (firstChunk.ContactPoint.y != 0)
            {
                sLeaser.sprites[0].scaleY *= 1f - puddle / 6f;
                sLeaser.sprites[0].scaleX *= 1f + puddle / 4f;
                sLeaser.sprites[0].x += firstChunk.vel.x * 2f;
                sLeaser.sprites[1].anchorY = 0.3f;
                sLeaser.sprites[1].scaleY *= 1.45f - puddle / 3f;
                sLeaser.sprites[1].scaleX *= 1.15f + puddle;
                sLeaser.sprites[1].rotation = 180f * (1f - (firstChunk.vel.x / 18f));
            }
            else if (firstChunk.ContactPoint.x != 0)
            {

            }
            else if (grabbedBy.Count == 0)
            {
                sLeaser.sprites[0].scaleY *= 1.2f + Mathf.Clamp(firstChunk.vel.magnitude / 18f, 0f, 1.5f);
                sLeaser.sprites[1].scaleY *= 0.8f + Mathf.Clamp(firstChunk.vel.magnitude / 18f, 0f, 1.5f);
            }
            else
            {
                sLeaser.sprites[1].scaleY *= 0.8f;
            }

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");
            for(int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = new Color(0.5f, 0.8f, 1f, 0.25f);
            sLeaser.sprites[1].color = new Color(0.5f, 0.8f, 1f, 0.25f);
        }
        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            bites--;
            room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, firstChunk.pos, 0.6f, 0.8f);
            room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos, 0.8f, 1.35f);
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                Destroy();
            }
        }
        public void ThrowByPlayer()
        {

        }
    }
}
