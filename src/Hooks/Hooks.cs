
using Tubus.Hooks;

namespace TubusTree.Hooks
{
    public static class Hooks
    {
        public static void PatchAll()
        {
            RoomHooks.Patch();
        }
    }
}
