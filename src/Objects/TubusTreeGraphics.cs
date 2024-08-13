

namespace Tubus.Objects
{
   public class TubusTreeGraphics : GraphicsModule
    {
        public TubusTreeObject tubusTree;
        public TubusTreeGraphics(PhysicalObject ow) : base(ow, false)
        {
            tubusTree = ow as TubusTreeObject;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("pixel")
            { 
                scale = tubusTree.bodyChunks[0].rad * 2f 
            };
            sLeaser.sprites[1] = new FSprite("pixel")
            {
                scale = tubusTree.bodyChunks[1].rad * 2f
            };

            AddToContainer(sLeaser, rCam, null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Midground");
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(tubusTree.bodyChunks[0].pos - camPos);
            sLeaser.sprites[1].SetPosition(tubusTree.bodyChunks[1].pos - camPos);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
