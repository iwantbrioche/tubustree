using Tubus.Objects.SapGlob;
using Tubus.Objects.TubusTree;

namespace TubusTreeObject.Hooks
{
    public static class Hooks
    {
        public static void PatchAll()
        {
            On.Spear.HitSomething += Spear_HitSomething;
            IL.Spear.LodgeInCreature_CollisionResult_bool_bool += Spear_LodgeInCreature_CollisionResult_bool_bool1;

            On.Player.Grabability += Player_Grabability;

            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
        }

        private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            if (obj is SapGlob)
            {
                return 5;
            }
            return orig(self, obj, weaponFiltered);
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is SapGlob)
            {
                return Player.ObjectGrabability.OneHand;
            }
            return orig(self, obj);
        }

        private static void Spear_LodgeInCreature_CollisionResult_bool_bool1(ILContext il)
        {
            ILCursor tubusCurs = new(il);

            try
            {
                ILLabel brLabel = null;
                tubusCurs.GotoNext(MoveType.Before, x => x.MatchBr(out brLabel),
                    x => x.MatchLdsfld<ModManager>(nameof(ModManager.MSC)),
                    x => x.MatchBrfalse(out _),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld(typeof(SharedPhysics.CollisionResult).GetField(nameof(SharedPhysics.CollisionResult.obj))),
                    x => x.MatchIsinst<JellyFish>());

                tubusCurs.GotoNext(MoveType.AfterLabel, x => x.MatchLdsfld<ModManager>(nameof(ModManager.MSC)));

                tubusCurs.Emit(OpCodes.Ldarg_0);
                tubusCurs.Emit(OpCodes.Ldarg_1);
                tubusCurs.Emit(OpCodes.Ldarg_2);
                tubusCurs.EmitDelegate((Spear self, SharedPhysics.CollisionResult result, bool eu) =>
                {
                    if (result.obj is TubusTree tubus)
                    {
                        if (result.chunk == null || tubus.bottomChunk != null && result.chunk == tubus.bottomChunk) result.chunk = tubus.firstChunk;

                        self.stuckInChunkIndex = result.chunk.index;
                        self.stuckRotation = Custom.Angle(self.throwDir.ToVector2(), self.stuckInChunk.Rotation);
                        self.firstChunk.MoveWithOtherObject(eu, self.stuckInChunk, Vector2.zero);
                        new AbstractPhysicalObject.AbstractSpearStick(self.abstractPhysicalObject, result.obj.abstractPhysicalObject, self.stuckInChunkIndex, self.stuckBodyPart, self.stuckRotation);
                        tubus.CreateGlob(result.collisionPoint, self.stuckInChunkIndex);
                        return true;
                    }
                    return false;
                });
                tubusCurs.Emit(OpCodes.Brtrue, brLabel);
            }
            catch (Exception ex)
            {
                TubusPlugin.Logger.LogError("Spear.LodgeInCreature ILHook failed!");
                throw ex;
            }
        }

        private static bool Spear_HitSomething(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj is TubusTree)
            {
                self.room.PlaySound(SoundID.Spear_Stick_In_Creature, self.firstChunk);
                self.LodgeInCreature(result, eu);
                return true;
            }
            return orig(self, result, eu);
        }

    }
}
