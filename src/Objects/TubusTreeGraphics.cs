
using Unity.Mathematics;

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
            private List<Vertex[]> roots;

            public int firstSprite;
            public int totalSprites
            {
                get
                {
                    int sprites = 0;
                    if (owner.DEBUGVIZ)
                    {
                        for (int i = 0; i < branches.Count; i++)
                        {
                            sprites += branches[i].Length;
                            for (int j = 1; j < branches[i].Length; j++)
                            {
                                sprites++;
                            }
                        }
                        for (int i = 0; i < roots.Count; i++)
                        {
                            sprites += roots[i].Length;
                            for (int j = 1; j < roots[i].Length; j++)
                            {
                                sprites++;
                            }
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
                InitRoots();
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
            private void InitRoots()
            {
                roots = [];
                int numRoots = 0;
                int maxRoots = 3;
                if (Random.value > 0.33f)
                {
                    maxRoots++;
                }
                while (numRoots < maxRoots)
                {
                    for (int i = 1; i < roots.Count; i++)
                    {
                        if (CollidingBranch(roots[i - 1], roots[i]))
                        {
                            roots.RemoveAt(i);
                            numRoots--;
                        }
                    }
                    GenerateRoot();
                    numRoots++;
                }
            }
            private void GenerateBranch()
            {
                List<Vertex> vertices = [];
                int depth = Random.Range(-1, 2);
                Vertex baseVert = new((float2)(Vector2)Random.onUnitSphere * 12f * Mathf.Pow(Random.value, 0.45f), 5f);
                baseVert.depth = depth;
                baseVert.pos.y = Mathf.Abs(baseVert.pos.y);
                vertices.Add(baseVert);

                int segments = 2;
                if (Random.value < 0.2f) segments--;

                float spread = 0.8f;
                for (int i = 0; i < segments; i++)
                {
                    float dir = Custom.Float2ToDeg(vertices[i].pos);
                    float vertDir = Mathf.Lerp(dir / 2f * -1f, dir / 2f, Random.value);
                    Vertex vert = new(Custom.DegToFloat2(vertDir) * 5f + vertices[i].pos, 6f);
                    vert.depth = depth;
                    vert.pos.y = Mathf.Abs(vert.pos.y);
                    vert.pos = Custom.MoveTowards(vert.pos, new Vector2(vert.pos.x * 6f, vert.pos.y * 2f), Random.Range(9f, 20f));
                    vert.pos.x = vert.pos.x * spread + Random.value;
                    vertices.Add(vert);
                }
                branches.Add([.. vertices]);
            }
            private void GenerateRoot()
            {
                List<Vertex> vertices = [];
                int depth = Random.Range(-1, 2);
                if (depth < 0 && Random.value > 0.5f)
                {
                    depth = Random.Range(0, 2);
                }
                Vector2 sphere = (Vector2)Random.onUnitSphere * 12f;
                Vector2 smallSphere = (Vector2)Random.onUnitSphere * 6f;
                smallSphere.x = Mathf.Abs(smallSphere.x) * Mathf.Sign(sphere.x);
                smallSphere.y = Mathf.Abs(smallSphere.y) * Mathf.Sign(sphere.y);
                sphere += smallSphere;
                Vertex baseVert = new((float2)sphere * Mathf.Pow(Random.value, 0.45f), 5f);
                baseVert.depth = depth;
                baseVert.pos.y = -Mathf.Abs(baseVert.pos.y);
                vertices.Add(baseVert);

                int segments = 2;
                if (Random.value < 0.1f) segments--;
                else if (Random.value > 0.85f) segments++;

                float down = 3f;
                float spread = 1f;
                for (int i = 0; i < segments && segments < 5; i++)
                {
                    float dir = Custom.Float2ToDeg(vertices[i].pos);
                    float vertDir = Mathf.Lerp(dir / 1.75f * -1f, dir / 1.75f, Random.value);
                    Vertex vert = new(Custom.DegToFloat2(vertDir) * 5f + vertices[i].pos, 6f);
                    vert.depth = depth;
                    vert.pos.y = -Mathf.Abs(vert.pos.y);
                    vert.pos = Custom.MoveTowards(vert.pos, new Vector2(vert.pos.x * 6f, vert.pos.y), Random.Range(5f, 14f));
                    vert.pos.x = vert.pos.x * spread + Random.value;
                    vert.pos.y -= down;
                    Vector2 groundPos = (Vector2)vert.pos + owner.tubusTree.origPos;
                    if (owner.tubusTree.room.GetTile(groundPos).Terrain != Room.Tile.TerrainType.Air && i > 2)
                    {
                        vert.pos.y = owner.tubusTree.room.MiddleOfTile(groundPos).y + 10f;
                        break;
                    }
                    else
                    {
                        if (owner.tubusTree.room.GetTile(owner.tubusTree.room.MiddleOfTile(groundPos + new Vector2(0f, 20f))).Terrain != Room.Tile.TerrainType.Air)
                        {
                            vert.pos.y = owner.tubusTree.room.MiddleOfTile(groundPos - new Vector2(0f, 20f)).y;
                        }
                        else down *= 3f;
                        spread -= 0.1f;
                        segments++;
                    }
                    vertices.Add(vert);
                }
                roots.Add([.. vertices]);
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
                if (owner.DEBUGVIZ)
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
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 0; j < roots[i].Length; j++)
                        {
                            sLeaser.sprites[index] = new("Circle20");
                            sLeaser.sprites[index].scale = 0.3f;
                            index++;
                        }
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 1; j < roots[i].Length; j++)
                        {
                            sLeaser.sprites[index] = new("pixel");
                            sLeaser.sprites[index].anchorY = 0f;
                            sLeaser.sprites[index].scaleX = 3f;
                            index++;
                        }
                    }
                }
            }
            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, Vector2 camPos)
            {
                if (owner.DEBUGVIZ)
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
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 0; j < roots[i].Length; j++)
                        {
                            sLeaser.sprites[index].SetPosition((Vector2)roots[i][j].pos + owner.tubusTree.bodyChunks[0].pos - camPos);
                            index++;
                        }
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 1; j < roots[i].Length; j++)
                        {
                            sLeaser.sprites[index].SetPosition((Vector2)roots[i][j].pos + owner.tubusTree.bodyChunks[0].pos - camPos);
                            sLeaser.sprites[index].rotation = Custom.AimFromOneVectorToAnother((Vector2)roots[i][j].pos + owner.tubusTree.bodyChunks[0].pos, (Vector2)roots[i][j - 1].pos + owner.tubusTree.bodyChunks[0].pos);
                            sLeaser.sprites[index].scaleY = Vector2.Distance((Vector2)roots[i][j].pos + owner.tubusTree.bodyChunks[0].pos, (Vector2)roots[i][j - 1].pos + owner.tubusTree.bodyChunks[0].pos);
                            index++;
                        }
                    }
                }
            }
            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                // fade branches behind into fog color
                if (owner.DEBUGVIZ)
                {
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
                    color = Color.red;
                    for (int i = 0; i < roots.Count; i++)
                    {
                        Color.RGBToHSV(color, out var h, out var s, out var v);
                        v = 0.8f;
                        for (int j = 0; j < roots[i].Length; j++)
                        {
                            if (roots[i][j].depth > 0) v = 1f;
                            else if (roots[i][j].depth < 0) v = 0.6f;
                            color = Color.HSVToRGB(h, s, v);
                            sLeaser.sprites[index].color = color;
                            index++;
                        }
                        h += 0.2f;
                        color = Color.HSVToRGB(h, s, v);
                    }
                    color = Color.red;
                    for (int i = 0; i < roots.Count; i++)
                    {
                        Color.RGBToHSV(color, out var h, out var s, out var v);
                        v = 0.8f;
                        for (int j = 1; j < roots[i].Length; j++)
                        {
                            if (roots[i][j].depth > 0) v = 1f;
                            else if (roots[i][j].depth < 0) v = 0.6f;
                            color = Color.HSVToRGB(h, s, v);
                            sLeaser.sprites[index].color = color;
                            index++;
                        }
                        h += 0.2f;
                        color = Color.HSVToRGB(h, s, v);
                    }
                }
            }
            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                if (owner.DEBUGVIZ)
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
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 0; j < roots[i].Length; j++)
                        {
                            if (roots[i][j].depth >= 0) rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[index]);
                            else if (roots[i][j].depth < 0) rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[index]);
                            index++;
                        }
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 1; j < roots[i].Length; j++)
                        {
                            if (roots[i][j - 1].depth >= 0) rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[index]);
                            else if (roots[i][j - 1].depth < 0) rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[index]);
                            index++;
                        }
                    }
                }
            }

        }
        public TubusTree tubusTree;
        public Branches branches;
        public bool DEBUGVIZ;
        public TubusTreeGraphics(PhysicalObject ow) : base(ow, false)
        {
            tubusTree = ow as TubusTree;
            DEBUGVIZ = true;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            branches = new(this);
            if (DEBUGVIZ)
            {
                sLeaser.sprites = new FSprite[2 + branches.totalSprites];
                sLeaser.sprites[0] = new FSprite("Circle20");
                sLeaser.sprites[1] = new FSprite("Circle20");
                sLeaser.sprites[0].scale = 1.5f;
                branches.firstSprite = 2;
            }

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
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (DEBUGVIZ)
            {
                sLeaser.sprites[0].SetPosition(tubusTree.bodyChunks[0].pos - camPos);
                sLeaser.sprites[1].SetPosition(tubusTree.bodyChunks[1].pos - camPos);
            }

            branches.DrawSprites(sLeaser, camPos);
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
