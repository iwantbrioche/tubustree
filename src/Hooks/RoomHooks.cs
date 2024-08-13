
using Tubus.Objects;
using Tubus.PomObjects;

namespace Tubus.Hooks
{
    public static class RoomHooks
    {
        public static void Patch()
        {
            On.Room.Loaded += Room_Loaded;
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);
            if (self.game == null) return;
            for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
            {
                if (self.roomSettings.placedObjects[i].type == TubusTypes.TubusTree)
                {
                    TubusTreeObject.AbstractTubusTree abstractTubusTree = new(self.world, null, self.GetWorldCoordinate(self.roomSettings.placedObjects[i].pos), self.game.GetNewID(), self.roomSettings.placedObjects[i]);
                    self.abstractRoom.entities.Add(abstractTubusTree);
                    abstractTubusTree.Realize();
                }
            }
        }
    }
}
