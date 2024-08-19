
using TubusTreeObject;
using Unity.Mathematics;
using UnityEngine;
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
                public float depth;
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
            public int branchesStart;
            public int rootStart;
            public int bulbStart;
            public int bulbs;
            public int totalSprites
            {
                get
                {
                    int sprites = 0;
                    if (!owner.DEBUGVIZ)
                    {
                        sprites += branches.Count + roots.Count + bulbs;
                    }
                    else
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
                bulbs += branches.Count * 2;
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
                if (Random.value > 0.80f)
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
                bulbs += roots.Count;
            }
            private void GenerateBranch()
            {
                List<Vertex> vertices = [];
                Vector3 sphere3 = Random.onUnitSphere * 12f;
                Vector2 sphere2 = (Vector2)sphere3;
                float rad = 6f;
                Vertex baseVert = new((float2)sphere2 * Mathf.Pow(Random.value, 0.45f), rad);
                float depth = Random.value * 0.15f * Mathf.Sign(sphere3.z);
                baseVert.depth = depth;
                baseVert.pos.y = Mathf.Abs(baseVert.pos.y);
                vertices.Add(baseVert);

                int segments = 2;
                if (Random.value < 0.2f) segments--;

                float spread = 0.8f;
                for (int i = 0; i < segments; i++)
                {
                    rad /= Random.value + 1f;
                    float dir = Custom.Float2ToDeg(vertices[i].pos);
                    float vertDir = Mathf.Lerp(dir / 2f * -1f, dir / 2f, Random.value);
                    Vertex vert = new(Custom.DegToFloat2(vertDir) * 5f + vertices[i].pos, rad);
                    vert.depth += (Random.value * 0.3f + 0.1f) * Mathf.Sign(depth) + depth;
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
                Vector3 sphere3 = Random.onUnitSphere * 12f;
                Vector2 sphere2 = (Vector2)sphere3;
                Vector2 smallSphere = (Vector2)Random.onUnitSphere * 6f;
                smallSphere.x = Mathf.Abs(smallSphere.x) * Mathf.Sign(sphere3.x);
                smallSphere.y = Mathf.Abs(smallSphere.y) * Mathf.Sign(sphere3.y);
                sphere2 += smallSphere;
                float rad = 7f;
                Vertex baseVert = new((float2)sphere2 * Mathf.Pow(Random.value, 0.45f), rad);
                float depth = Random.value * 0.15f * Mathf.Sign(sphere3.z);
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
                    rad /= Random.value / 2f + 1f;
                    float dir = Custom.Float2ToDeg(vertices[i].pos);
                    float vertDir = Mathf.Lerp(dir / 1.75f * -1f, dir / 1.75f, Random.value);
                    Vertex vert = new(Custom.DegToFloat2(vertDir) * 5f + vertices[i].pos, rad);
                    vert.depth += (Random.value * 0.3f + 0.1f) * Mathf.Sign(depth) + depth;
                    vert.pos.y = -Mathf.Abs(vert.pos.y);
                    vert.pos = Custom.MoveTowards(vert.pos, new Vector2(vert.pos.x * 6f, vert.pos.y), Random.Range(5f, 14f));
                    vert.pos.x = vert.pos.x * spread + Random.value;
                    vert.pos.y -= down;
                    Vector2 groundPos = (Vector2)vert.pos + owner.tubusTree.origPos;
                    if (owner.tubusTree.room.GetTile(groundPos).Terrain != Room.Tile.TerrainType.Air && i > 2)
                    {
                        break;
                    }
                    else
                    {
                        if (owner.tubusTree.room.GetTile(owner.tubusTree.room.MiddleOfTile(groundPos + new Vector2(0f, 30f))).Terrain == Room.Tile.TerrainType.Air)
                        {
                            vert.pos.y -= down;
                            down *= 2f;
                        }
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
            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                if (!owner.DEBUGVIZ)
                {
                    branchesStart = firstSprite;
                    for (int i = 0; i < branches.Count; i++)
                    {
                        sLeaser.sprites[branchesStart + i] = TriangleMesh.MakeLongMesh(branches[i].Length - 1, false, true);
                        sLeaser.sprites[branchesStart + i].shader = TubusPlugin.TubusTrunk;
                        sLeaser.sprites[branchesStart + i].alpha = 0.75f;

                    }
                    rootStart = branchesStart + branches.Count;
                    for (int i = 0; i < roots.Count; i++)
                    {
                        sLeaser.sprites[rootStart + i] = TriangleMesh.MakeLongMesh(roots[i].Length - 1, false, true);
                        sLeaser.sprites[rootStart + i].shader = TubusPlugin.TubusTrunk;
                        sLeaser.sprites[rootStart + i].alpha = 0.8f;
                    }
                    bulbStart = rootStart + roots.Count;
                    for (int i = 0; i < bulbs; i++)
                    {
                        sLeaser.sprites[bulbStart + i] = new("Futile_White");
                        sLeaser.sprites[bulbStart + i].shader = TubusPlugin.TubusTrunk;
                    }
                }
                else
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
            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                if (!owner.DEBUGVIZ)
                {
                    Random.State state = Random.state;
                    Random.InitState(owner.tubusTree.seed);
                    Vector2 topChunkPos = owner.tubusTree.bodyChunks[1].pos;
                    Vector2 mainChunkPos = owner.tubusTree.bodyChunks[0].pos;
                    for (int i = 0; i < branches.Count; i++)
                    {
                        Vector2 rootPos = branches[i][0].pos;
                        for (int j = 0; j < branches[i].Length - 1; j++)
                        {
                            if (j > 0) rootPos = branches[i][j - 1].pos;
                            Vector2 branchPos = branches[i][j].pos;
                            Vector2 nextBranchPos = branches[i][j + 1].pos;
                            Vector2 normalized = (rootPos - nextBranchPos).normalized;
                            Vector2 perpDir = Custom.PerpendicularVector(normalized);
                            float branchRad = branches[i][j].rad * 1.8f;
                            float otherBranchRad = branches[i][j + 1].rad * 1.8f;
                            (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4, branchPos - perpDir * branchRad + normalized + topChunkPos - camPos);
                            (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4 + 1, branchPos + perpDir * branchRad + normalized + topChunkPos - camPos);
                            (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4 + 2, nextBranchPos - perpDir * otherBranchRad - normalized + topChunkPos - camPos);
                            (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4 + 3, nextBranchPos + perpDir * otherBranchRad - normalized + topChunkPos - camPos);
                            for (int c = 0; c < 3; c++)
                            {
                                (sLeaser.sprites[branchesStart + i] as TriangleMesh).verticeColors[j * 3 + c] = new Color(branches[i][j].depth, 0f, 0f);
                            }

                        }
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        Vector2 mainPos = roots[i][0].pos;
                        for (int j = 0; j < roots[i].Length - 1; j++)
                        {
                            if (j > 0) mainPos = roots[i][j - 1].pos;
                            Vector2 rootPos = roots[i][j].pos;
                            Vector2 nextRootPos = roots[i][j + 1].pos;
                            Vector2 normalized = (mainPos - nextRootPos).normalized;
                            Vector2 perpDir = Custom.PerpendicularVector(normalized);
                            float rootRad = roots[i][j].rad * 1.5f;
                            float otherRootRad = roots[i][j + 1].rad * 1.5f;
                            (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4, rootPos - perpDir * rootRad + normalized + mainChunkPos - camPos);
                            (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4 + 1, rootPos + perpDir * rootRad + normalized + mainChunkPos - camPos);
                            (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4 + 2, nextRootPos - perpDir * otherRootRad - normalized + mainChunkPos - camPos);
                            (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4 + 3, nextRootPos + perpDir * otherRootRad - normalized + mainChunkPos - camPos);
                            for (int c = 0; c < 4; c++)
                            {
                                (sLeaser.sprites[rootStart + i] as TriangleMesh).verticeColors[j * 4 + c] = new Color(roots[i][j].depth, 0f, 0f);
                            }
                        }
                    }
                    for (int i = 0; i < branches.Count; i++)
                    {
                        sLeaser.sprites[bulbStart + i].SetPosition((Vector2)branches[i][0].pos + topChunkPos - camPos);
                        sLeaser.sprites[bulbStart + i].rotation = Custom.VecToDeg(Custom.PerpendicularVector((Vector2)branches[i][0].pos, (Vector2)branches[i][1].pos)) + 90f;
                        sLeaser.sprites[bulbStart + i].alpha = 0.3f;
                        sLeaser.sprites[bulbStart + i].scale = Random.value + 0.2f;
                        sLeaser.sprites[bulbStart + i].scaleX = Random.value / 3f + 1f;
                        sLeaser.sprites[bulbStart + i].color = new Color(branches[i][0].depth, 0f, 0f);

                        sLeaser.sprites[bulbStart + i + branches.Count].SetPosition(Custom.MoveTowards((Vector2)branches[i][branches[i].Length - 1].pos, (Vector2)branches[i][branches[i].Length - 2].pos, 8f) + topChunkPos - camPos);
                        sLeaser.sprites[bulbStart + i + branches.Count].rotation = Custom.VecToDeg(Custom.PerpendicularVector((Vector2)branches[i][branches[i].Length - 2].pos, (Vector2)branches[i][branches[i].Length - 1].pos)) + 90f;
                        sLeaser.sprites[bulbStart + i + branches.Count].alpha = 0.5f;
                        sLeaser.sprites[bulbStart + i + branches.Count].scale = branches[i][branches[i].Length - 1].rad / 3f;
                        sLeaser.sprites[bulbStart + i + branches.Count].scaleY = Random.value / 1.5f + 0.8f;
                        sLeaser.sprites[bulbStart + i + branches.Count].color = new Color(branches[i][branches[i].Length - 1].depth, 0f, 0f);
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        sLeaser.sprites[bulbStart + i + branches.Count * 2].SetPosition((Vector2)roots[i][0].pos + mainChunkPos - camPos);
                        sLeaser.sprites[bulbStart + i + branches.Count * 2].rotation = Custom.VecToDeg(Custom.PerpendicularVector((Vector2)roots[i][0].pos, (Vector2)roots[i][1].pos)) + 90f;
                        sLeaser.sprites[bulbStart + i + branches.Count * 2].alpha = 0.4f;
                        sLeaser.sprites[bulbStart + i + branches.Count * 2].scale = Random.value + 1f;
                        sLeaser.sprites[bulbStart + i + branches.Count * 2].scaleX = Random.value / 2f + 1f;
                        //sLeaser.sprites[bulbStart + i + branches.Count * 2].color = new Color(roots[i][0].depth, 0f, 0f);
                    }
                    Random.state = state;
                }
                else
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
                if (!owner.DEBUGVIZ)
                {
                    for (int i = 0; i < branches.Count; i++)
                    {
                        sLeaser.sprites[branchesStart + i].color = Color.black;
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        sLeaser.sprites[rootStart + i].color = Color.black;
                    }
                    for (int i = 0; i < bulbs; i++)
                    {
                        sLeaser.sprites[bulbStart + i].color = Color.black;
                    }
                }
                else
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
                if (!owner.DEBUGVIZ)
                {
                    for (int i = 0; i < branches.Count; i++)
                    {
                        if (branches[i][0].depth >= 0)
                        {
                            rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[bulbStart + i]);
                            rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[branchesStart + i]);
                            rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[bulbStart + i + branches.Count]);
                        }
                        else if (branches[i][0].depth < 0)
                        {
                            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[bulbStart + i]);
                            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[branchesStart + i]);
                            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[bulbStart + i + branches.Count]);
                        }
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        if (roots[i][0].depth >= 0) 
                        {
                            rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[bulbStart + i + branches.Count * 2]);
                            rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[rootStart + i]);
                        }
                        else if (roots[i][0].depth < 0)
                        { 
                            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[bulbStart + i + branches.Count * 2]);
                            rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[rootStart + i]);
                        }
                    }
                }
                else
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
                            if (roots[i][j].depth > 0) rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[index]);
                            else if (roots[i][j].depth < 0) rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[index]);
                            index++;
                        }
                    }
                    for (int i = 0; i < roots.Count; i++)
                    {
                        for (int j = 1; j < roots[i].Length; j++)
                        {
                            if (roots[i][j - 1].depth > 0) rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[index]);
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
            DEBUGVIZ = false;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            branches = new(this);
            if (!DEBUGVIZ)
            {
                Random.State state = Random.state;
                Random.InitState(tubusTree.seed);
                sLeaser.sprites = new FSprite[2 + branches.totalSprites];
                branches.firstSprite = 2;
                sLeaser.sprites[0] = new FSprite("Futile_White");
                sLeaser.sprites[0].shader = TubusPlugin.TubusTrunk;
                sLeaser.sprites[0].alpha = 0.55f;
                sLeaser.sprites[0].scale = 2.2f;
                sLeaser.sprites[1] = new FSprite("Futile_White");
                sLeaser.sprites[1].shader = TubusPlugin.TubusTrunk;
                sLeaser.sprites[1].alpha = 0.4f;
                sLeaser.sprites[1].scale = 1.7f;
                Random.state = state;


            }
            else
            {
                sLeaser.sprites = new FSprite[2 + branches.totalSprites];
                sLeaser.sprites[0] = new FSprite("Circle20");
                sLeaser.sprites[1] = new FSprite("Circle20");
                sLeaser.sprites[0].scale = 1.5f;
                branches.firstSprite = 2;
            }

            branches.InitiateSprites(sLeaser, rCam);

            AddToContainer(sLeaser, rCam, null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");
            foreach(var sprite in sLeaser.sprites)
            {
                newContatiner.AddChild(sprite);
            }
            branches.AddToContainer(sLeaser, rCam);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (!DEBUGVIZ)
            {
                sLeaser.sprites[0].SetPosition(tubusTree.bodyChunks[0].pos - camPos);
                sLeaser.sprites[1].SetPosition(tubusTree.bodyChunks[1].pos - camPos);
                sLeaser.sprites[1].rotation = Custom.VecToDeg(tubusTree.bodyChunks[0].Rotation);
            }
            else
            {
                sLeaser.sprites[0].SetPosition(tubusTree.bodyChunks[0].pos - camPos);
                sLeaser.sprites[1].SetPosition(tubusTree.bodyChunks[1].pos - camPos);
            }

            branches.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = Color.black;
            sLeaser.sprites[1].color = Color.black;
            branches.ApplyPalette(sLeaser, rCam, palette);
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
