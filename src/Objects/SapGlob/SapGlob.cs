using static Tubus.Objects.RegisterObjects;


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

        public Vector2 rotation;
        public Vector2 lastRotation;
        public Vector2 tapPos;
        public bool harvested;
        public int bites = 5;
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
        }
        public override void Update(bool eu)
        {
            // gloopy effect, drips downward slowly
            // splats on ground and puddles outward slowly
            // leaves little droplets of sap that spread out when moving across surface
            // drips of sap come off
            base.Update(eu);
            lastRotation = rotation;
            if (grabbedBy.Count > 0)
            {
                harvested = true;
                if (collisionLayer == 2) room.ChangeCollisionLayerForObject(this, 1);
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                rotation.y = Mathf.Abs(rotation.y);
            }
            if (firstChunk.ContactPoint.y < 0)
            {
                firstChunk.vel.x *= 0.4f;
            }
            if (firstChunk.ContactPoint.x != 0)
            {
                firstChunk.vel.y *= 0.4f;
                firstChunk.vel.x += 0.1f * firstChunk.ContactPoint.x;
            }
            if (!harvested)
            {
                firstChunk.HardSetPosition(tapPos);
                firstChunk.pos = tapPos;
                firstChunk.lastPos = tapPos;
                firstChunk.vel = default;
            }
        }
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("Futile_White");
            sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["WaterNut"];
            sLeaser.sprites[1] = new FSprite("Futile_White");
            sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatLight"];
            AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 r = Vector3.Slerp(lastRotation, rotation, timeStacker);

            sLeaser.sprites[0].SetPosition(pos - camPos);
            sLeaser.sprites[0].rotation = Custom.VecToDeg(r);
            sLeaser.sprites[0].scale = Mathf.Lerp(0.2f, 1f, bites / 5f);
            sLeaser.sprites[0].scaleY *= 1.2f;
            sLeaser.sprites[1].SetPosition(pos - camPos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(r);
            sLeaser.sprites[1].scale = Mathf.Lerp(0.2f, 1f, bites / 5f);

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");
            foreach(var sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = new Color(0.5f, 0.8f, 1f, 0.25f);
            sLeaser.sprites[1].color = new Color(1f, 1f, 1f, 1f);
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
