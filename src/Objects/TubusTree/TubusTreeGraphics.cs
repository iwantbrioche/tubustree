using TubusTreeObject;

namespace Tubus.Objects.TubusTree
{

    // how the fuck am i gonna comment all of this shit
    //  i dont even know how half of it works anymore
    //      :monkwail:
    public class TubusTreeGraphics : GraphicsModule
    {
        private class Limbs
        {
            private struct Branch
            {
                public struct Vertex
                {
                    public Vector2 pos;
                    public float rad;
                    public float depth;
                    public Vertex(Vector2 p, float r)
                    {
                        pos = p;
                        rad = r;
                    }
                }
                public struct Flower
                {
                    public struct Petal
                    {
                        public Vector2 pos;
                        public float rotation;
                        public Petal(Vector2 p, float r)
                        {
                            pos = p;
                            rotation = r;
                        }
                    }
                    public Vector2 pos;
                    public Petal[] innerRing;
                    public Petal[] outerRing;
                    public int totalPetals
                    {
                        get
                        {
                            return 3 + innerRing.Length + outerRing.Length;
                        }
                    }
                    public float size;
                    public Vector2 rotation;
                    public int vert;
                    public Flower(Vector2 p, float s, int extraPetals, int v)
                    {
                        pos = p;
                        size = s;
                        innerRing = new Petal[5 + extraPetals];
                        outerRing = new Petal[9 + extraPetals];
                        vert = v;
                    }
                    public bool CollidingFlowers(Flower other)
                    {
                        return Custom.DistLess(pos, other.pos, Vector2.Distance(pos, other.pos));
                    }
                }

                public List<Vertex> vertices;
                public List<Flower> flowers;
                public Branch(List<Vertex> v)
                {
                    vertices = v;
                    flowers = [];
                }

                public bool CollidingBranch(Branch other)
                {
                    for (int i = 1; i < vertices.Count; i++)
                    {
                        for (int j = 1; j < other.vertices.Count; j++)
                        {
                            Vector2 A1 = other.vertices[j - 1].pos;
                            Vector2 A2 = other.vertices[j].pos;
                            Vector2 B1 = vertices[i - 1].pos;
                            Vector2 B2 = vertices[i].pos;
                            Vector2 intersect = Custom.LineIntersection(A1, A2, B1, B2);
                            if (Custom.DistLess(intersect, A1, Vector2.Distance(A1, A2) + other.vertices[j - 1].rad) && Custom.DistLess(intersect, A2, Vector2.Distance(A1, A2) + other.vertices[j].rad) && Custom.DistLess(intersect, B1, Vector2.Distance(B2, B1) + vertices[i - 1].rad) && Custom.DistLess(intersect, B2, Vector2.Distance(B2, B1) + vertices[i].rad))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
                public Vector2 OnBranchPos(int index, float distance)
                {
                    return Vector2.Lerp(vertices[index].pos, vertices[index + 1].pos, distance);
                }
            }


            private TubusTreeGraphics owner;
            private List<Branch> branches;
            private List<Branch> roots;
            private Room room => owner.tubus.room;
            public int firstSprite;
            private int branchesStart;
            private int rootStart;
            private int bulbStart;
            private int flowerStart;
            private int bulbs;
            public int totalSprites
            {
                get
                {
                    int total = branches.Count + roots.Count + bulbs;
                    for (int i = 0; i < branches.Count; i++)
                    {
                        for (int j = 0; j < branches[i].flowers.Count; j++)
                        {
                            total += branches[i].flowers[j].totalPetals;
                        }
                    }
                    return total;
                }
            }

            public Limbs(TubusTreeGraphics ow)
            {
                owner = ow;
                InitBranches();
                InitRoots();
                InitFlowers();
            }
            private void InitBranches()
            {
                branches = [];
                int numBranches = 0;
                int maxBranches = 3;
                if (Random.value > 0.66f) maxBranches++;

                while (numBranches < maxBranches)
                {
                    for (int i = 1; i < branches.Count; i++)
                    {
                        if (branches[i - 1].CollidingBranch(branches[i]))
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
                if (Random.value > 0.33f) maxRoots++;
                if (Random.value > 0.90f) maxRoots++;
                while (numRoots < maxRoots)
                {
                    for (int i = 1; i < roots.Count; i++)
                    {
                        if (roots[i - 1].CollidingBranch(roots[i]))
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
            private void InitFlowers()
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    int numFlowers = 0;
                    int maxFlowers = 1;
                    if (Random.value > 0.75f) maxFlowers--;

                    float sizeMult = 1f;
                    int tries = 0;
                    while (numFlowers < maxFlowers && tries <= 6)
                    {
                        for (int j = 1; j < branches[i].flowers.Count; j++)
                        {
                            if (branches[i].flowers[j - 1].CollidingFlowers(branches[i].flowers[j]))
                            {
                                branches[i].flowers.RemoveAt(j);
                                numFlowers--;
                                tries++;
                            }
                        }

                        GenerateFlower(i, sizeMult);
                        if (sizeMult > 0.4f) sizeMult -= 0.1f;
                        numFlowers++;
                    }
                    for (int l = 0; l < branches.Count; l++)
                    {
                        if (i == l) continue;

                        for (int m = 0; m < branches[l].flowers.Count; m++)
                        {
                            for (int j = 0; j < branches[i].flowers.Count; j++)
                            {
                                if (branches[i].flowers[j].CollidingFlowers(branches[l].flowers[m]))
                                {
                                    branches[i].flowers.RemoveAt(j);
                                }
                            }
                        }
                    }
                }
            }
            private void GenerateBranch()
            {
                List<Branch.Vertex> vertices = [];
                Vector3 sphere3 = Random.onUnitSphere * 12f;
                Vector2 sphere2 = (Vector2)sphere3;
                float rad = 6f;
                Branch.Vertex baseVert = new(sphere2 * Mathf.Pow(Random.value, 0.45f), rad);
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
                    float dir = Custom.VecToDeg(vertices[i].pos);
                    float vertDir = Mathf.Lerp(dir / 2f * -1f, dir / 2f, Random.value);
                    Branch.Vertex vert = new(Custom.DegToVec(vertDir) * 5f + vertices[i].pos, rad);
                    vert.depth += (Random.value * 0.5f + 0.2f) * Mathf.Sign(depth) + depth;
                    vert.pos.y = Mathf.Abs(vert.pos.y);
                    vert.pos = Custom.MoveTowards(vert.pos, new Vector2(vert.pos.x * 6f, vert.pos.y * 2f), Random.Range(9f, 20f));
                    vert.pos.x = vert.pos.x * spread + Random.value;
                    vertices.Add(vert);
                }
                branches.Add(new Branch(vertices));
            }
            private void GenerateRoot()
            {
                List<Branch.Vertex> vertices = [];
                Vector3 sphere3 = Random.onUnitSphere * 12f;
                Vector2 sphere2 = (Vector2)sphere3;
                Vector2 smallSphere = (Vector2)Random.onUnitSphere * 6f;
                smallSphere.x = Mathf.Abs(smallSphere.x) * Mathf.Sign(sphere3.x);
                smallSphere.y = Mathf.Abs(smallSphere.y) * Mathf.Sign(sphere3.y);
                sphere2 += smallSphere;
                float rad = 7f;
                Branch.Vertex baseVert = new(sphere2 * Mathf.Pow(Random.value, 0.45f), rad);
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
                    float dir = Custom.VecToDeg(vertices[i].pos);
                    float vertDir = Mathf.Lerp(dir / 1.75f * -1f, dir / 1.75f, Random.value);
                    Branch.Vertex vert = new(Custom.DegToVec(vertDir) * 5f + vertices[i].pos, rad);
                    vert.depth += (Random.value * 0.5f + 0.2f) * Mathf.Sign(depth) + depth;
                    vert.pos.y = -Mathf.Abs(vert.pos.y);
                    vert.pos = Custom.MoveTowards(vert.pos, new Vector2(vert.pos.x * 6f, vert.pos.y), Random.Range(5f, 14f));
                    vert.pos.x = vert.pos.x * spread + Random.value;
                    vert.pos.y -= down;
                    Vector2 groundPos = vert.pos + owner.tubus.origPos;
                    if (room.GetTile(room.GetTilePosition(groundPos)).Terrain != Room.Tile.TerrainType.Air && i > 2)
                    {
                        vert.pos.y -= down;
                        vertices.Add(vert);
                        break;
                    }
                    else
                    {
                        vert.pos.y -= down;
                        down *= room.GetTile(room.GetTilePosition(groundPos) - new IntVector2(0, 1)).Terrain == Room.Tile.TerrainType.Air ? 1.5f : 2f;
                        spread -= 0.1f;
                        segments++;
                    }
                    vertices.Add(vert);
                }
                roots.Add(new Branch(vertices));
            }
            private void GenerateFlower(int branchIndex, float sizeMult)
            {
                int vertIndex = Random.Range(branches[branchIndex].vertices.Count - 2, branches[branchIndex].vertices.Count - 1);
                Branch.Flower flower = new(branches[branchIndex].OnBranchPos(vertIndex, Random.value * 0.4f + 0.6f), (Random.value * 0.5f + 0.5f) * sizeMult, Random.Range(-1, 4), vertIndex);
                Vector2 facingAngle = (Vector2)Random.onUnitSphere * branches[branchIndex].vertices[vertIndex].rad;
                flower.pos += facingAngle;
                // Places the flower at a random position on the branch, then moves the position to a random point on a sphere multiplied by the radius of the branch vertex
                flower.rotation = new Vector2(Random.Range(0f, 30f), Random.Range(0f, 30f)) * Mathf.Sign(Custom.VecToDeg(facingAngle));
                float petalAngle = 360f / flower.innerRing.Length;
                float currentAngle = petalAngle;
                for (int i = 0; i < flower.innerRing.Length; i++)
                {
                    float rad = currentAngle * -Mathf.PI / 180f;
                    float xAxis = flower.rotation.x * -Mathf.PI / 180f;
                    float yAxis = flower.rotation.y * -Mathf.PI / 180f;
                    Vector2 rotPos = new(Mathf.Cos(rad - yAxis) - Mathf.Sin(rad - xAxis), Mathf.Sin(rad + yAxis) + Mathf.Cos(rad + xAxis));
                    // Rotation code from https://github.com/jakelazaroff/til/blob/main/math/rotate-a-point-around-a-circle.md

                    flower.innerRing[i].pos = flower.pos + rotPos * 2f * flower.size;
                    flower.innerRing[i].rotation = currentAngle;
                    currentAngle += petalAngle;
                }

                petalAngle = 360f / flower.outerRing.Length;
                currentAngle = petalAngle;
                for (int i = 0; i < flower.outerRing.Length; i++)
                {
                    float rad = (currentAngle + petalAngle * 0.5f) * -Mathf.PI / 180f;
                    float xAxis = flower.rotation.x * -Mathf.PI / 180f;
                    float yAxis = flower.rotation.y * -Mathf.PI / 180f;
                    Vector2 rotPos = new(Mathf.Cos(rad - yAxis) - Mathf.Sin(rad - xAxis), Mathf.Sin(rad + yAxis) + Mathf.Cos(rad + xAxis));

                    flower.outerRing[i].pos = flower.pos + rotPos * 2f * flower.size;
                    flower.outerRing[i].rotation = currentAngle;
                    currentAngle += petalAngle;
                }
                branches[branchIndex].flowers.Add(flower);
            }
            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                branchesStart = firstSprite;
                for (int i = 0; i < branches.Count; i++)
                {
                    sLeaser.sprites[branchesStart + i] = TriangleMesh.MakeLongMesh(branches[i].vertices.Count - 1, false, true);
                    sLeaser.sprites[branchesStart + i].shader = TubusPlugin.TubusTrunk;
                    sLeaser.sprites[branchesStart + i].alpha = 0.75f;
                }
                rootStart = branchesStart + branches.Count;
                for (int i = 0; i < roots.Count; i++)
                {
                    sLeaser.sprites[rootStart + i] = TriangleMesh.MakeLongMesh(roots[i].vertices.Count - 1, false, true);
                    sLeaser.sprites[rootStart + i].shader = TubusPlugin.TubusTrunk;
                    sLeaser.sprites[rootStart + i].alpha = 0.8f;
                }
                bulbStart = rootStart + roots.Count;
                for (int i = 0; i < bulbs; i++)
                {
                    sLeaser.sprites[bulbStart + i] = new("Futile_White");
                    sLeaser.sprites[bulbStart + i].shader = TubusPlugin.TubusTrunk;
                }
                flowerStart = bulbStart + bulbs;
                int flowerIndex = flowerStart;
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 0; j < branches[i].flowers.Count; j++)
                    {
                        for (int l = 0; l < branches[i].flowers[j].outerRing.Length; l++)
                        {
                            sLeaser.sprites[flowerIndex] = new("LizardLowerTeeth3.2");
                            sLeaser.sprites[flowerIndex].rotation = branches[i].flowers[j].outerRing[l].rotation;
                            sLeaser.sprites[flowerIndex].scale = 0.45f * branches[i].flowers[j].size;
                            sLeaser.sprites[flowerIndex].scaleY *= Mathf.Clamp(Mathf.Abs(branches[i].flowers[j].outerRing[l].rotation / Custom.VecToDeg(branches[i].flowers[j].rotation)), 1f, 1.6f);
                            sLeaser.sprites[flowerIndex].scaleX *= 1.25f;
                            //sLeaser.sprites[flowerIndex].shader = TubusPlugin.TubusFlower;

                            flowerIndex++;
                        }
                        for (int l = 0; l < branches[i].flowers[j].innerRing.Length; l++)
                        {
                            sLeaser.sprites[flowerIndex] = new("LizardLowerTeeth3.2");
                            sLeaser.sprites[flowerIndex].rotation = branches[i].flowers[j].innerRing[l].rotation;
                            sLeaser.sprites[flowerIndex].scale = 0.35f * branches[i].flowers[j].size;
                            sLeaser.sprites[flowerIndex].scaleX *= 1.2f;
                            //sLeaser.sprites[flowerIndex].shader = TubusPlugin.TubusFlower;

                            flowerIndex++;
                        }
                        float flowerRot = Custom.VecToDeg(branches[i].flowers[j].rotation);

                        sLeaser.sprites[flowerIndex] = new("Circle20");
                        sLeaser.sprites[flowerIndex].scale = branches[i].flowers[j].size * 0.55f;
                        sLeaser.sprites[flowerIndex].rotation = flowerRot;

                        flowerIndex++;

                        sLeaser.sprites[flowerIndex] = new("tubusMainPetal0");
                        sLeaser.sprites[flowerIndex].scale = branches[i].flowers[j].size * 0.55f;
                        sLeaser.sprites[flowerIndex].rotation = flowerRot;
                        if (branches[i].flowers[j].rotation.y > 0f) 
                            sLeaser.sprites[flowerIndex].scaleY = sLeaser.sprites[flowerIndex].scale * 1f + 0.25f * (Mathf.Abs(branches[i].flowers[j].rotation.y) / 30f);
                        else 
                            sLeaser.sprites[flowerIndex].scaleY = sLeaser.sprites[flowerIndex].scale * 0.4f + 0.35f * (Mathf.Abs(branches[i].flowers[j].rotation.y) / 30f);

                        flowerIndex++;

                        sLeaser.sprites[flowerIndex] = new("tubusMainPetal0");
                        sLeaser.sprites[flowerIndex].scale = branches[i].flowers[j].size * 0.5f;
                        sLeaser.sprites[flowerIndex].rotation = flowerRot + 180f;
                        if (branches[i].flowers[j].rotation.y > 0f)
                            sLeaser.sprites[flowerIndex].scaleY = sLeaser.sprites[flowerIndex].scale * 0.4f + 0.35f * (Mathf.Abs(branches[i].flowers[j].rotation.y) / 30f);
                        else
                            sLeaser.sprites[flowerIndex].scaleY = sLeaser.sprites[flowerIndex].scale * 1f + 0.25f * (Mathf.Abs(branches[i].flowers[j].rotation.y) / 30f);

                        flowerIndex++;
                    }
                }
            }
            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Random.State state = Random.state;
                Random.InitState(owner.tubus.seed);
                Vector2 topChunkPos = owner.tubus.topChunk.pos;
                Vector2 mainChunkPos = owner.tubus.firstChunk.pos;
                for (int i = 0; i < branches.Count; i++)
                {
                    Vector2 mainPos = branches[i].vertices[0].pos;
                    for (int j = 0; j < branches[i].vertices.Count - 1; j++)
                    {
                        if (j > 0) mainPos = branches[i].vertices[j - 1].pos;
                        Vector2 branchPos = branches[i].vertices[j].pos;
                        Vector2 nextBranchPos = branches[i].vertices[j + 1].pos;
                        Vector2 normalized = (mainPos - nextBranchPos).normalized;
                        Vector2 perpDir = Custom.PerpendicularVector(normalized);
                        float branchRad = branches[i].vertices[j].rad * 1.8f;
                        float otherBranchRad = branches[i].vertices[j + 1].rad * 1.8f;
                        (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4, branchPos - perpDir * branchRad + normalized + topChunkPos - camPos);
                        (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4 + 1, branchPos + perpDir * branchRad + normalized + topChunkPos - camPos);
                        (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4 + 2, nextBranchPos - perpDir * otherBranchRad - normalized + topChunkPos - camPos);
                        (sLeaser.sprites[branchesStart + i] as TriangleMesh).MoveVertice(j * 4 + 3, nextBranchPos + perpDir * otherBranchRad - normalized + topChunkPos - camPos);
                    }
                }
                for (int i = 0; i < roots.Count; i++)
                {
                    Vector2 mainPos = roots[i].vertices[0].pos;
                    for (int j = 0; j < roots[i].vertices.Count - 1; j++)
                    {
                        if (j > 0) mainPos = roots[i].vertices[j - 1].pos;
                        Vector2 rootPos = roots[i].vertices[j].pos;
                        Vector2 nextRootPos = roots[i].vertices[j + 1].pos;
                        Vector2 normalized = (mainPos - nextRootPos).normalized;
                        Vector2 perpDir = Custom.PerpendicularVector(normalized);
                        float rootRad = roots[i].vertices[j].rad * 1.5f;
                        float otherRootRad = roots[i].vertices[j + 1].rad * 1.5f;
                        (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4, rootPos - perpDir * rootRad + normalized + mainChunkPos - camPos); // 0, 4, 8
                        (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4 + 1, rootPos + perpDir * rootRad + normalized + mainChunkPos - camPos); // 1, 5, 9
                        (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4 + 2, nextRootPos - perpDir * otherRootRad - normalized + mainChunkPos - camPos); // 2, 6, 10
                        (sLeaser.sprites[rootStart + i] as TriangleMesh).MoveVertice(j * 4 + 3, nextRootPos + perpDir * otherRootRad - normalized + mainChunkPos - camPos); // 3, 7, 11
                    }
                }
                for (int i = 0; i < branches.Count; i++)
                {
                    sLeaser.sprites[bulbStart + i].SetPosition(branches[i].vertices[0].pos + topChunkPos - camPos);
                    sLeaser.sprites[bulbStart + i].rotation = Custom.VecToDeg(Custom.PerpendicularVector(branches[i].vertices[0].pos, branches[i].vertices[1].pos)) + 90f;
                    sLeaser.sprites[bulbStart + i].alpha = 0.3f;
                    sLeaser.sprites[bulbStart + i].scale = Random.value + 0.2f;
                    sLeaser.sprites[bulbStart + i].scaleX = Random.value / 3f + 0.6f;

                    sLeaser.sprites[bulbStart + i + branches.Count].SetPosition(branches[i].OnBranchPos(branches[i].vertices.Count - 2, 0.8f) + topChunkPos - camPos);
                    sLeaser.sprites[bulbStart + i + branches.Count].rotation = Custom.VecToDeg(Custom.PerpendicularVector(branches[i].vertices[branches[i].vertices.Count - 2].pos, branches[i].vertices[branches[i].vertices.Count - 1].pos)) + 90f;
                    sLeaser.sprites[bulbStart + i + branches.Count].alpha = 0.5f;
                    sLeaser.sprites[bulbStart + i + branches.Count].scale = branches[i].vertices[branches[i].vertices.Count - 1].rad / 3f;
                    sLeaser.sprites[bulbStart + i + branches.Count].scaleY = Random.value / 1.5f + 0.65f;
                }
                for (int i = 0; i < roots.Count; i++)
                {
                    sLeaser.sprites[bulbStart + i + branches.Count * 2].SetPosition(roots[i].vertices[0].pos + mainChunkPos - camPos);
                    sLeaser.sprites[bulbStart + i + branches.Count * 2].rotation = Custom.VecToDeg(Custom.PerpendicularVector(roots[i].vertices[0].pos, roots[i].vertices[1].pos)) + 90f;
                    sLeaser.sprites[bulbStart + i + branches.Count * 2].alpha = 0.4f;
                    sLeaser.sprites[bulbStart + i + branches.Count * 2].scale = Random.value + 1.2f;
                    sLeaser.sprites[bulbStart + i + branches.Count * 2].scaleX = Random.value / 2f + 0.8f;
                }
                int flowerIndex = flowerStart;
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 0; j < branches[i].flowers.Count; j++)
                    {
                        for (int l = 0; l < branches[i].flowers[j].outerRing.Length; l++)
                        {
                            sLeaser.sprites[flowerIndex].SetPosition(branches[i].flowers[j].outerRing[l].pos + topChunkPos - camPos);
                            flowerIndex++;
                        }
                        for (int l = 0; l < branches[i].flowers[j].innerRing.Length; l++)
                        {
                            sLeaser.sprites[flowerIndex].SetPosition(branches[i].flowers[j].innerRing[l].pos + topChunkPos - camPos);
                            flowerIndex++;
                        }
                        float flowerRot = Custom.VecToDeg(branches[i].flowers[j].rotation);

                        sLeaser.sprites[flowerIndex].SetPosition(branches[i].flowers[j].pos + topChunkPos - camPos);
                        flowerIndex++;

                        sLeaser.sprites[flowerIndex].SetPosition(branches[i].flowers[j].pos + Custom.RotateAroundOrigo(new Vector2(0f, 3.7f * branches[i].flowers[j].size), flowerRot) + topChunkPos - camPos);
                        flowerIndex++;

                        sLeaser.sprites[flowerIndex].SetPosition(branches[i].flowers[j].pos - Custom.RotateAroundOrigo(new Vector2(0f, 3.7f * branches[i].flowers[j].size), flowerRot) + topChunkPos - camPos);
                        flowerIndex++;
                    }
                }
                Random.state = state;
            }
            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                for (int i = 0; i < branches.Count; i++)
                {
                    if (branches[i].vertices[0].depth < 0f)
                    {
                        rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[bulbStart + i]);
                        rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[branchesStart + i]);
                        rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[bulbStart + i + branches.Count]);
                    }
                }
                for (int i = 0; i < roots.Count; i++)
                {
                    if (roots[i].vertices[0].depth < 0f)
                    {
                        rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[bulbStart + i + branches.Count * 2]);
                        rCam.ReturnFContainer("Background").AddChild(sLeaser.sprites[rootStart + i]);
                    }
                }
                int flowerIndex = flowerStart;
                for (int i = 0; i < branches.Count; i++)
                {
                    if (branches[i].vertices[0].depth < 0f)
                    {
                        for (int j = 0; j < branches[i].flowers.Count; j++)
                        {
                            for (int l = 0; l < branches[i].flowers[j].totalPetals; l++)
                            {
                                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[flowerIndex]);
                                flowerIndex++;
                            }
                        }
                    }
                }
            }
            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {


                for (int i = 0; i < branches.Count; i++)
                {
                    sLeaser.sprites[bulbStart + i].color = new Color(1f, 1f, 1f, Mathf.Clamp(branches[i].vertices[0].depth, 0.2f, 1f));
                    sLeaser.sprites[bulbStart + i + branches.Count].color = new Color(1f, 1f, 1f, Mathf.Clamp(branches[i].vertices[branches[i].vertices.Count - 1].depth, 0.2f, 1f));
                    for (int j = 0; j < branches[i].vertices.Count - 1; j++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            (sLeaser.sprites[branchesStart + i] as TriangleMesh).verticeColors[j * 3 + c] = new Color(1f, 1f, 1f, Mathf.Clamp(branches[i].vertices[j].depth, 0.2f, 1f));
                        }
                    }
                }
                for (int i = 0; i < roots.Count; i++)
                {
                    sLeaser.sprites[bulbStart + i + branches.Count * 2].color = Color.white;
                    for (int j = 0; j < roots[i].vertices.Count - 1; j++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            (sLeaser.sprites[rootStart + i] as TriangleMesh).verticeColors[j * 3 + c] = new Color(1f, 1f, 1f, Mathf.Clamp(roots[i].vertices[j].depth, 0.2f, 1f));
                        }
                    }
                }
                Color effectColB = palette.texture.GetPixel(30, 2);
                Color.RGBToHSV(effectColB, out float H, out float S, out float V);

                float triadicH = H + 0.33f;
                if (triadicH > 1f) triadicH -= 1f;

                Color triadicColor = Color.HSVToRGB(triadicH, S, V);

                int flowerIndex = flowerStart;
                for (int i = 0; i < branches.Count; i++)
                {
                    for (int j = 0; j < branches[i].flowers.Count; j++)
                    {
                        for (int l = 0; l < branches[i].flowers[j].outerRing.Length; l++)
                        {
                            sLeaser.sprites[flowerIndex].color = Color.Lerp(Color.Lerp(effectColB, palette.blackColor, Mathf.Clamp(palette.darkness, 0.5f - (effectColB.r + effectColB.g + effectColB.b) / 7.5f, 0.85f)), palette.fogColor, Mathf.Clamp(roots[i].vertices[branches[i].flowers[j].vert].depth, 0f, 0.2f));
                            flowerIndex++;
                        }
                        for (int l = 0; l < branches[i].flowers[j].innerRing.Length; l++)
                        {
                            Color innerPetal = triadicColor;
                            innerPetal.r -= (V + S) * 0.25f;
                            innerPetal.r = Mathf.Clamp01(innerPetal.r);

                            innerPetal.g -= (V + S) * 0.25f;
                            innerPetal.g = Mathf.Clamp01(innerPetal.g);

                            innerPetal.b -= (V + S) * 0.25f;
                            innerPetal.b = Mathf.Clamp01(innerPetal.b);

                            sLeaser.sprites[flowerIndex].color = Color.Lerp(Color.Lerp(Color.Lerp(effectColB, innerPetal, 0.55f), palette.blackColor, Mathf.Clamp(palette.darkness, 0.5f - (effectColB.r + effectColB.g + effectColB.b) / 7.5f, 0.85f)), palette.fogColor, Mathf.Clamp(roots[i].vertices[branches[i].flowers[j].vert].depth, 0f, 0.2f));
                            flowerIndex++;
                        }

                        sLeaser.sprites[flowerIndex].color = Color.Lerp(Color.Lerp(Color.HSVToRGB(H, S, V * 0.5f), palette.blackColor, Mathf.Clamp(palette.darkness, 0.5f - (effectColB.r + effectColB.g + effectColB.b) / 7.5f, 0.85f)), palette.fogColor, Mathf.Clamp(roots[i].vertices[branches[i].flowers[j].vert].depth, 0f, 0.2f));
                        flowerIndex++;

                        sLeaser.sprites[flowerIndex].color = Color.Lerp(Color.Lerp(Color.HSVToRGB(H, S * S * (0.6f - ((effectColB.g + effectColB.b) / 4f)), V * 0.6f), palette.blackColor, Mathf.Clamp(palette.darkness, 0.5f - (effectColB.r + effectColB.g + effectColB.b) / 7.5f, 0.85f)), palette.fogColor, Mathf.Clamp(roots[i].vertices[branches[i].flowers[j].vert].depth, 0f, 0.2f));
                        flowerIndex++;

                        sLeaser.sprites[flowerIndex].color = Color.Lerp(Color.Lerp(Color.HSVToRGB(H, S * S * (0.6f - ((effectColB.g + effectColB.b) / 4f)), V * 0.6f), palette.blackColor, Mathf.Clamp(palette.darkness, 0.5f - (effectColB.r + effectColB.g + effectColB.b) / 7.5f, 0.85f)), palette.fogColor, Mathf.Clamp(roots[i].vertices[branches[i].flowers[j].vert].depth, 0f, 0.2f));
                        flowerIndex++;
                    }
                }
            }
        }
        public TubusTree tubus;
        private Limbs limbs;
        public TubusTreeGraphics(PhysicalObject ow) : base(ow, false)
        {
            tubus = ow as TubusTree;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            Random.State state = Random.state;
            Random.InitState(tubus.seed);
            limbs = new(this);
            sLeaser.sprites = new FSprite[2 + limbs.totalSprites];
            limbs.firstSprite = 2;
            sLeaser.sprites[0] = new FSprite("Futile_White");
            sLeaser.sprites[0].shader = TubusPlugin.TubusTrunk;
            sLeaser.sprites[0].alpha = 0.55f;
            sLeaser.sprites[0].scale = 2.2f;
            sLeaser.sprites[1] = new FSprite("Futile_White");
            sLeaser.sprites[1].shader = TubusPlugin.TubusTrunk;
            sLeaser.sprites[1].alpha = 0.4f;
            sLeaser.sprites[1].scale = 1.7f;
            Random.state = state;

            limbs.InitiateSprites(sLeaser, rCam);

            AddToContainer(sLeaser, rCam, null);
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("Items");
            foreach (var sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContatiner.AddChild(sprite);
            }
            limbs.AddToContainer(sLeaser, rCam);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            sLeaser.sprites[0].SetPosition(tubus.firstChunk.pos - camPos);
            sLeaser.sprites[1].SetPosition(tubus.topChunk.pos - camPos);
            sLeaser.sprites[1].rotation = Custom.VecToDeg(tubus.firstChunk.Rotation);

            limbs.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = Color.white;
            sLeaser.sprites[1].color = Color.white;
            Random.State state = Random.state;
            Random.InitState(tubus.seed);
            limbs.ApplyPalette(sLeaser, rCam, palette);
            Random.state = state;
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
