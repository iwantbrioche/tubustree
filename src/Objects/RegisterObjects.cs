
using Tubus.Objects.TubusTree;
using static Pom.Pom;

namespace Tubus.Objects
{
    public static class RegisterObjects
    {
        public static void RegisterPOMObjects()
        {
            // Registers a new ManagedObject with pom using the TubusObjectType
            RegisterManagedObject(new TubusObjectType());
        }
        public static class ObjectTypes
        {
            // Registers the enums for the Tubus Tree and SapGlob
            public static readonly PlacedObject.Type TubusTree = new("Tubus Tree", true);
            public static readonly AbstractPhysicalObject.AbstractObjectType AbstractTubusTree = new("AbstractTubusTree", true);
            public static readonly AbstractPhysicalObject.AbstractObjectType SapGlob = new("SapGlob", true);
        }
    }
}
