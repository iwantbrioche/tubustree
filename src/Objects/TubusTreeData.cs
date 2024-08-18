
using DevInterface;
using static Pom.Pom;

namespace Tubus.PomObjects
{
    public class TubusObjectType : ManagedObjectType
    {
        public TubusObjectType() : base(TubusTypes.TubusTree.value, "Tubus Tree", null, typeof(TubusData), typeof(TubusRepresentation))
        {
        }

    }
    public class TubusData : ManagedData
    {
        private static readonly List<ManagedField> tubusFields = [
            new IntegerField("seed", 0, 10000, 1, ManagedFieldWithPanel.ControlType.slider, "Seed")];
        public TubusData(PlacedObject owner) : base(owner, [.. tubusFields]) 
        {
        }
    }
    public class TubusRepresentation : ManagedRepresentation
    {
        public TubusRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj)
        {
        }
    }
    public static class TubusTypes
    {
        public static PlacedObject.Type TubusTree = new("Tubus Tree", true);
        public static AbstractPhysicalObject.AbstractObjectType AbstractTubusTree = new("AbstractTubusTree", true);
    }

}
