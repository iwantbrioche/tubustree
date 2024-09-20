
using Tubus.Objects.TubusTree;
using static Pom.Pom;

namespace Tubus.Objects
{
    public static class RegisterObjects
    {
        public static void RegisterPOMObjects()
        {
            RegisterManagedObject(new TubusObjectType());
        }
        public static class ObjectTypes
        {
            public static readonly PlacedObject.Type TubusTree = new("Tubus Tree", true);
            public static readonly AbstractPhysicalObject.AbstractObjectType AbstractTubusTree = new("AbstractTubusTree", true);
            public static readonly AbstractPhysicalObject.AbstractObjectType SapGlob = new("SapGlob", true);
        }
    }
}
