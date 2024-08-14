
using Tubus.Objects;
using Tubus.PomObjects;

namespace TubusTreeObject.Hooks
{
    public static class Hooks
    {
        public static void PatchAll()
        {
            On.Room.Loaded += Room_Loaded;
            On.Spear.LodgeInCreature_CollisionResult_bool_bool += Spear_LodgeInCreature_CollisionResult_bool_bool;
            On.Spear.HitSomething += Spear_HitSomething;
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

        private static void Spear_LodgeInCreature_CollisionResult_bool_bool(On.Spear.orig_LodgeInCreature_CollisionResult_bool_bool orig, Spear self, SharedPhysics.CollisionResult result, bool eu, bool isJellyFish)
        {
            if (result.obj is TubusTree)
            {
                self.stuckInObject = result.obj;
                self.ChangeMode(Spear.Mode.StuckInCreature);
                result.chunk ??= result.obj.firstChunk;
                self.stuckInChunkIndex = result.chunk.index;
                self.stuckRotation = Custom.Angle(self.throwDir.ToVector2(), self.stuckInChunk.Rotation);
                self.firstChunk.MoveWithOtherObject(eu, self.stuckInChunk, Vector2.zero);
                new AbstractPhysicalObject.AbstractSpearStick(self.abstractPhysicalObject, result.obj.abstractPhysicalObject, self.stuckInChunkIndex, self.stuckBodyPart, self.stuckRotation);
                return;
            }
            orig(self, result, eu, isJellyFish);
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game == null) return;
            for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
            {
                if (self.roomSettings.placedObjects[i].type == TubusTypes.TubusTree)
                {
                    TubusTree.AbstractTubusTree abstractTubusTree = new(self.world, null, self.GetWorldCoordinate(self.roomSettings.placedObjects[i].pos), self.game.GetNewID(), self.roomSettings.placedObjects[i]);
                    self.abstractRoom.entities.Add(abstractTubusTree);
                    abstractTubusTree.Realize();
                }
            }
        }
    }
}
