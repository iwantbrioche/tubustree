
using DevInterface;
using static Pom.Pom;
using static Tubus.Objects.RegisterObjects;

namespace Tubus.Objects.TubusTree
{
    public class TubusObjectType : ManagedObjectType
    {
        public TubusObjectType() : base(ObjectTypes.TubusTree.value, "Tubus Tree", null, typeof(TubusData), typeof(TubusRepresentation)) { }
        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            TubusTree.AbstractTubusTree abstractTubusTree = new(room.world, null, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), placedObject);
            room.abstractRoom.entities.Add(abstractTubusTree);
            abstractTubusTree.Realize();
            return null;
        }

    }
    public class TubusData : ManagedData
    {
        private static readonly List<ManagedField> tubusFields = [
            new IntegerField("seed", 0, 10000, 1, ManagedFieldWithPanel.ControlType.slider, "Seed")];
        public TubusData(PlacedObject owner) : base(owner, [.. tubusFields]) { }
    }
    public class TubusRepresentation : ManagedRepresentation
    {
        public TubusRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj) { }
    }

}
