
using Unity.Mathematics;
using static Tubus.Objects.TubusTreeGraphics;

namespace Tubus.Objects
{
   public class TubusTreeGraphics : GraphicsModule
    {
        public class Branches
        {
            public struct Vertex
            {
                public float2 pos;
                public float rad;
                public int depth;
                public Vertex(float2 p, float r)
                {
                    pos = p;
                    rad = r;
                }
            }

            private TubusTreeGraphics owner;
            private List<Vertex[]> branches;

            public int firstSprite;
            public int totalSprites
            {
                get
                {
                    int sprites = 0;
                    for (int i = 0; i < branches.Count; i++)
                    {
                        sprites += branches[i].Length;
                        for (int j = 1; j < branches[i].Length; j++)
                        {
                            sprites++;
                        }
                    }
                    return sprites;
                }
            }

            public Branches(TubusTreeGraphics ow)
            {
                owner = ow;
                Random.State state = Random.state;
                Random.InitState(ow.tubusTree.seed);
                InitBranches();
                Random.state = state;
            }
            private void InitBranches()
            {
                branches = [];
                int numBranches = 0;
                int maxBranches = 3;
                if (Random.value > 0.66f)
                {
                    maxBranches++;
                }
                while (numBranches < maxBranches)
                {
                    for (int i = 1; i < branches.Count; i++)
                    {
                        if (CollidingBranch(branches[i - 1], branches[i]))
                        {
                            branches.RemoveAt(i);
                            numBranches--;
                        }
                    }
                    GenerateBranch();
                    numBranches++;
                }
            }
            private void GenerateBranch()
            {
                List<Vertex> vertices = [];
                int depth = Random.Range(-1, 2);
                Debug.Log(depth);
                Vertex baseVert = new((float2)(Vector2)Random.onUnitSphere * 12f * Mathf.Pow(Random.value, 0.45f), 5f);
                baseVert.depth = depth;
                baseVert.pos.y = Mathf.Abs(baseVert.pos.y);
                vertices.Add(baseVert);
                int segments = 2;
                if (Random.value < 0.2f)
                {
                    segments--;
                }
                else if (Random.value > 0.85f)
                {
                    segments++;
                }
                for (int i = 0; i < segments; i++)
                {
                    float dir = Custom.Float2ToDeg(vertices[i].pos);
                    Vertex vert = new(Custom.DegToFloat2(Mathf.Lerp(dir / 2f * -1f, dir / 2f, Random.value)) * 5f + vertices[i].pos, 6f);
                    vert.depth = depth;
                    vert.pos.y = Mathf.Abs(vert.pos.y);
                    vert.pos = Vector2.MoveTowards(vert.pos, new float2(0f, 8f), -Random.Range(2f, 6f));
                    vert.pos.x *= Random.value + 0.8f;
                    vertices.Add(vert);
                }
                branches.Add([.. vertices]);
            }
            public bool CollidingBranch(Vertex[] branch, Vertex[] other)
            {
                for (int i = 0; i < branch.Length; i++)
                {
                    for (int j = 0; j < other.Length; j++)
                    {
                        if (Custom.DistLess(branch[i].pos, other[j].pos, branch[i].rad + other[j].rad))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser)
            {
                int index = firstSprite;
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 0; j < branches[i].Length; j++)
                    {
                        sLeaser.sprites[index] = new("Circle20");
                        sLeaser.sprites[index].scale = 0.3f;
                        index++;
                    }
                }
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 1; j < branches[i].Length; j++)
                    {
                        sLeaser.sprites[index] = new("pixel");
                        sLeaser.sprites[index].anchorY = 0f;
                        sLeaser.sprites[index].scaleX = 3f;
                        index++;
                    }
                }
            }
            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, Vector2 camPos)
            {
                int index = firstSprite;
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 0; j < branches[i].Length; j++)
                    {
                        sLeaser.sprites[index].SetPosition((Vector2)branches[i][j].pos + owner.tubusTree.bodyChunks[1].pos - camPos);
                        index++;
                    }
                }
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 1; j < branches[i].Length; j++)
                    {
                        sLeaser.sprites[index].SetPosition((Vector2)branches[i][j].pos + owner.tubusTree.bodyChunks[1].pos - camPos);
                        sLeaser.sprites[index].rotation = Custom.AimFromOneVectorToAnother((Vector2)branches[i][j].pos + owner.tubusTree.bodyChunks[1].pos, (Vector2)branches[i][j - 1].pos + owner.tubusTree.bodyChunks[1].pos);
                        sLeaser.sprites[index].scaleY = Vector2.Distance((Vector2)branches[i][j].pos + owner.tubusTree.bodyChunks[1].pos, (Vector2)branches[i][j - 1].pos + owner.tubusTree.bodyChunks[1].pos);
                        index++;
                    }
                }
            }
            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                // fade branches behind into fog color
                int index = firstSprite;
                Color color = Color.red;
                for (int i = 0; i < branches.Count; i++)
                {
                    Color.RGBToHSV(color, out var h, out var s, out var v);
                    v = 0.8f;
                    for (int j = 0; j < branches[i].Length; j++)
                    {
                        if (branches[i][j].depth > 0) v = 1f;
                        else if (branches[i][j].depth < 0) v = 0.6f;
                        color = Color.HSVToRGB(h, s, v);
                        sLeaser.sprites[index].color = color;
                        index++;
                    }
                    h += 0.2f;
                    color = Color.HSVToRGB(h, s, v);
                }
                color = Color.red;
                for (int i = 0; i < branches.Count; i++)
                {
                    Color.RGBToHSV(color, out var h, out var s, out var v);
                    v = 0.8f;
                    for (int j = 1; j < branches[i].Length; j++)
                    {
                        if (branches[i][j].depth > 0) v = 1f;
                        else if (branches[i][j].depth < 0) v = 0.6f;
                        color = Color.HSVToRGB(h, s, v);
                        sLeaser.sprites[index].color = color;
                        index++;
                    }
                    h += 0.2f;
                    color = Color.HSVToRGB(h, s, v);
                }
            }
            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                int index = firstSprite;
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 0; j < branches[i].Length; j++)
                    {
                        if (branches[i][j].depth >= 0) rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[index]);
                        else if (branches[i][j].depth < 0) rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[index]);
                        index++;
                    }
                }
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 1; j < branches[i].Length; j++)
                    {
                        if (branches[i][j - 1].depth >= 0) rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[index]);
                        else if (branches[i][j - 1].depth < 0) rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[index]);
                        index++;
                    }
                }
            }

        }
        public TubusTreeObject tubusTree;
        public Branches branches;
        public TubusTreeGraphics(PhysicalObject ow) : base(ow, false)
        {
            tubusTree = ow as TubusTreeObject;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            branches = new(this);
            sLeaser.sprites = new FSprite[2 + branches.totalSprites];
            sLeaser.sprites[0] = new FSprite("Circle20");
            sLeaser.sprites[1] = new FSprite("Circle20");
            sLeaser.sprites[0].scale = 1.3f;
            branches.firstSprite = 2;
            branches.InitiateSprites(sLeaser);

            AddToContainer(sLeaser, rCam, null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Midground");
            foreach(var sprite in sLeaser.sprites)
            {
                newContatiner.AddChild(sprite);
            }
            branches.AddToContainer(sLeaser, rCam);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(tubusTree.bodyChunks[0].pos - camPos);
            sLeaser.sprites[1].SetPosition(tubusTree.bodyChunks[1].pos - camPos);
            branches.DrawSprites(sLeaser, camPos);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            branches.ApplyPalette(sLeaser, rCam, palette);
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
