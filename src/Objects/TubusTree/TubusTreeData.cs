
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
            new IntegerField("seed", 0, 10000, 1, ManagedFieldWithPanel.ControlType.slider, "Seed"),
            new FloatField("trunkred", 0f, 255f, 0f, 1f, ManagedFieldWithPanel.ControlType.slider, "Trunk R"),
            new FloatField("trunkgreen", 0f, 255f, 0f, 1f, ManagedFieldWithPanel.ControlType.slider, "Trunk G"),
            new FloatField("trunkblue", 0f, 255f, 0f, 1f, ManagedFieldWithPanel.ControlType.slider, "Trunk B"),
            new FloatField("flowerhue", 0f, 360, 0f, 1f, ManagedFieldWithPanel.ControlType.slider, "Flower H"),
            new FloatField("flowersat", 50f, 85f, 85f, 1f, ManagedFieldWithPanel.ControlType.slider, "Flower S"),
            new FloatField("flowerlum", 50f, 70f, 60f, 1f, ManagedFieldWithPanel.ControlType.slider, "Flower L")];
        public TubusData(PlacedObject owner) : base(owner, [.. tubusFields]) { }
    }
    public class TubusRepresentation : ManagedRepresentation
    {
        public TubusRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj) { }
    }

}
