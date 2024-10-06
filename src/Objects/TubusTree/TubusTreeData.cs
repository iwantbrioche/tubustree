
using DevInterface;
using static Pom.Pom;
using static Tubus.Objects.ObjectTypes;

namespace Tubus.Objects.TubusTree
{
    // The ManagedObjectType for the Tubus Tree, this is what gets registered into POM as a DevTools object
    public class TubusObjectType : ManagedObjectType
    {
        // The constructor takes in the name of the object, what catagory it goes into, the objectType of your UAD, the ManagedData, and the ManagedRepresentation
        public TubusObjectType() : base(ObjectTypes.TubusTree.value, "Tubus Tree", null, typeof(TubusData), typeof(TubusRepresentation)) { }

        // The MakeObject method is what adds your object to the room, because the Tubus Tree isn't a simple UAD, we override the MakeObject method and handle it with our own logic
        // Because we are already adding the object to the room, we return null after our logic to tell POM that we don't need an object added, this is also why we don't need an objectType in our constructor
        //  For a simple UAD you don't need to override this, as passing in the UAD's objectType will allow POM to add it to the room on its own (..I think)
        public override UpdatableAndDeletable MakeObject(PlacedObject placedObject, Room room)
        {
            TubusTree.AbstractTubusTree abstractTubusTree = new(room.world, null, room.GetWorldCoordinate(placedObject.pos), room.game.GetNewID(), placedObject);
            room.abstractRoom.entities.Add(abstractTubusTree);
            abstractTubusTree.Realize();
            return null;
        }

    }

    // The ManagedData class for the Tubus Tree, it holds the data to be used by the Tubus Tree
    public class TubusData : ManagedData
    {
        // A list of ManagedField that holds the slider for the Tubus Tree generation seed, the seed is added to this list and then put into the class through the constructor
        // It doesn't have to be a list, the constructor takes in a nullable array, but it works fine by turning it into an array with [.. tubusFields] or tubusFields.toArray()
        //  The ManagedFields use a key to access the data, to access the field from your object, cast the placedObject.data to your ManagedData type and use the GetValue<T>(string fieldName)
        //      method by replacing T with what type you want to get out and the fieldName with the key set in your ManagedField constructor
        private static readonly List<ManagedField> tubusFields = [
            new IntegerField("seed", 0, 10000, 1, ManagedFieldWithPanel.ControlType.slider, "Seed")];

        // The constructor takes in the ManagedFields to be used for the object's placedObject.data, if no ManagedFields are required then pass in null instead of a ManagedField array
        public TubusData(PlacedObject owner) : base(owner, [.. tubusFields]) { }
    }

    // The ManagedRepresentation class for the Tubus Tree, this controls the UI interface by creating controls for the ManagedFields
    public class TubusRepresentation : ManagedRepresentation
    {
        public TubusRepresentation(PlacedObject.Type placedType, ObjectsPage objPage, PlacedObject pObj) : base(placedType, objPage, pObj) { }
    }

}
