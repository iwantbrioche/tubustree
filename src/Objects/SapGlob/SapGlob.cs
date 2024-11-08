
using TubusTreeObject;

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
        private Color sapCol;
        public float side;
        public bool harvested;
        public bool spawnHarvested;
        public float harvestTimer;
        public float harvestTimerMax = 250;
        public float splat;
        public int splatDir;
        private float puddle;
        private int bites = 5;
        public Spear tapSpear;
        public TubusTree.TubusTree tubus;
        public bool spearStickable = true;
        private bool stuckOnSpear;
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
            color = new Color(0.5f, 0.8f, 1f);
            sapCol = color;
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
                harvestTimer += 1.25f - ((harvestTimer / harvestTimerMax) * 0.5f);
                if (harvestTimer > 200f)
                {
                    harvestTimer -= Mathf.Pow((harvestTimer / harvestTimerMax) - 0.3f, 1.5f);
                }
            }
            if (splat > 0f)
            {
                splat *= 0.8f;
            }

            if (room.game.devToolsActive && Input.GetKey("b"))
            {
                firstChunk.vel += Custom.DirVec(firstChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras.FirstOrDefault().pos) * 3f;
            }
            rotation = Vector2.ClampMagnitude(firstChunk.vel, 1f);
            if (grabbedBy.Count > 0)
            {
                harvested = true;
                spearStickable = false;
                stuckOnSpear = false;
                if (collisionLayer == 2) room.ChangeCollisionLayerForObject(this, 1);
                CollideWithObjects = true;
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
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
                firstChunk.vel = default;
            }
            if (spawnHarvested)
            {
                harvestTimer = harvestTimerMax;
                harvested = true;
                if (collisionLayer == 2) room.ChangeCollisionLayerForObject(this, 1);
                CollideWithObjects = true;
            }
            if (tapSpear != null)
            {
                if (harvestTimer >= harvestTimerMax)
                {
                    if (tapSpear.stuckInObject != tubus && spearStickable)
                    {
                        harvested = true;
                        TryStickToSpear();
                        room.ChangeCollisionLayerForObject(this, tapSpear.collisionLayer);
                    }
                }
                else
                {
                    if (tapSpear.stuckInObject != tubus)
                    {
                        spearStickable = false;
                    }
                }
                if (stuckOnSpear && tapSpear.mode == Spear.Mode.StuckInWall)
                {
                    spearStickable = false;
                    stuckOnSpear = false;
                    abstractPhysicalObject.LoseAllStuckObjects();
                }
            }
        }
        public void TryStickToSpear()
        {
            foreach (var stuckObject in tapSpear.abstractPhysicalObject.stuckObjects)
            {
                if (stuckObject.A == abstractPhysicalObject || stuckObject.B == abstractPhysicalObject)
                {
                    stuckOnSpear = true;
                    return;
                }
            }
            int stuckObjs = 0;
            int onSpearPos = 0;
            for (int i = 0; i < tapSpear.abstractPhysicalObject.stuckObjects.Count; i++)
            {
                if (tapSpear.abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick)
                {
                    if ((tapSpear.abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition == onSpearPos)
                    {
                        onSpearPos++;
                    }
                    stuckObjs++;
                }
            }
            if (stuckObjs <= 5 && stuckObjs < 5)
            {
                new AbstractPhysicalObject.ImpaledOnSpearStick(tapSpear.abstractPhysicalObject, abstractPhysicalObject, 0, onSpearPos);
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
            sLeaser.sprites[0].shader = TubusPlugin.TubusSap;
            sLeaser.sprites[0].alpha = 0.1f;
            sLeaser.sprites[1] = new FSprite("Futile_White");
            sLeaser.sprites[1].shader = TubusPlugin.TubusSap;
            sLeaser.sprites[1].alpha = 0.6f;
            AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 lastPos = Vector2.Lerp(firstChunk.lastLastPos, firstChunk.lastPos, timeStacker);
            Vector2 r = Vector3.Slerp(lastRotation, rotation, timeStacker);

            sLeaser.sprites[0].SetPosition(pos - camPos);
            sLeaser.sprites[0].rotation = Custom.VecToDeg(r);
            sLeaser.sprites[0].anchorY = 0.5f;
            sLeaser.sprites[0].alpha = 0.1f;
            sLeaser.sprites[1].anchorY = 0.75f;
            sLeaser.sprites[1].SetPosition(pos - camPos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(r);
            sLeaser.sprites[1].alpha = 0.6f;

            if (harvestTimer <= harvestTimerMax && !harvested)
            {
                sLeaser.sprites[0].scale = 0.1f + ((harvestTimer / harvestTimerMax) - 0.1f);
                sLeaser.sprites[1].scale = 0.1f + ((harvestTimer / harvestTimerMax) - 0.1f);
                sapCol = Color.Lerp(new Color(0.1f, 0.20f, 0.35f, 0.1f), color, Mathf.Pow(harvestTimer / harvestTimerMax, 1.5f));
            }
            else
            {
                sLeaser.sprites[0].scale = Mathf.Lerp(0.2f, 1f, bites / 5f);
                sLeaser.sprites[1].scale = Mathf.Lerp(0.2f, 1f, bites / 5f);
            }
            if (!harvested)
            {
                sLeaser.sprites[0].SetPosition(pos + new Vector2(0f, -(harvestTimer / harvestTimerMax) * 5f) - camPos);
                sLeaser.sprites[0].rotation = Custom.Angle(tapPos - new Vector2(6f * side, 0f), pos) + (-10f * side) + 180f;
                sLeaser.sprites[0].scaleY *= 0.5f + (harvestTimer / harvestTimerMax);
                sLeaser.sprites[0].scaleX *= 0.4f + ((harvestTimer / harvestTimerMax) * 0.3f);
                sLeaser.sprites[0].anchorY = 0.4f;
                sLeaser.sprites[0].alpha = 0.3f;
                sLeaser.sprites[1].SetPosition(pos + new Vector2(0f, -(harvestTimer / harvestTimerMax) * (8f + (harvestTimer / harvestTimerMax) * 2f)) - camPos);
                sLeaser.sprites[1].scaleY *= 1 + ((harvestTimer / harvestTimerMax) * 0.15f);
                sLeaser.sprites[1].scaleX *= 0.55f + ((harvestTimer / harvestTimerMax) * 0.4f);
                sLeaser.sprites[1].anchorY = 0.5f;
            }
            else if (stuckOnSpear && tapSpear != null)
            {
                sLeaser.sprites[1].anchorY = 0.25f;
                sLeaser.sprites[1].scaleY *= 1.2f + Mathf.Clamp((lastPos - pos).magnitude / 18f, 0f, 0.8f);
                sLeaser.sprites[1].scaleX *= 0.8f;
                sLeaser.sprites[1].rotation = 180f * (1f - ((lastPos - pos).x / 25f));
                if (tapSpear.inFrontOfObjects == 1)
                {
                    AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
                }
                else
                {
                    AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
                }
            }
            else
            {
                if (!stuckOnSpear && sLeaser.sprites[0].container != rCam.ReturnFContainer("Items"))
                {
                    AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
                }
                if (firstChunk.ContactPoint.y != 0)
                {
                    //spread out in puddle on floor
                    sLeaser.sprites[0].scaleY *= 1f - puddle / 6f;
                    sLeaser.sprites[0].scaleX *= 1f + puddle / 4f;
                    sLeaser.sprites[0].x += firstChunk.vel.x * 2f;
                    sLeaser.sprites[0].alpha = 0.2f;
                    sLeaser.sprites[1].anchorY = 0.4f;
                    sLeaser.sprites[1].scaleY *= 1.45f - puddle / 3f;
                    sLeaser.sprites[1].scaleX *= 1.15f + puddle;
                    sLeaser.sprites[1].rotation = 180f * (1f - (firstChunk.vel.x / 18f));
                    sLeaser.sprites[1].alpha = 0.3f;
                }
                else if (firstChunk.ContactPoint.x != 0)
                {
                    //gloop down walls
                }
                else if (grabbedBy.Count == 0)
                {
                    sLeaser.sprites[0].scaleY *= 1.2f + Mathf.Clamp(firstChunk.vel.magnitude / 18f, 0f, 1.5f);
                    sLeaser.sprites[0].rotation += 180f;
                    sLeaser.sprites[1].scaleY *= 0.8f + Mathf.Clamp(firstChunk.vel.magnitude / 18f, 0f, 1.5f);
                    sLeaser.sprites[1].rotation += 180f;
                    sLeaser.sprites[1].alpha = 0f;
                }
                else
                {
                    sLeaser.sprites[0].rotation += 180f;
                    sLeaser.sprites[0].alpha = 0.25f;
                    sLeaser.sprites[1].scaleY *= 0.8f;
                    sLeaser.sprites[1].alpha = 1f;
                }
            }

            sLeaser.sprites[0].color = sapCol;
            sLeaser.sprites[1].color = sapCol;

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
            newContatiner.AddChild(sLeaser.sprites[1]);
            newContatiner.AddChild(sLeaser.sprites[0]);
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[1].color = color;
        }
        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            bites--;
            room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, firstChunk.pos, 0.8f, 0.8f);
            room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos, 0.95f, 1.35f);
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
