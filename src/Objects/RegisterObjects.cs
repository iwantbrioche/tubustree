using static Pom.Pom;

namespace Tubus.PomObjects
{
    public static class RegisterObjects
    {
        public static void RegisterPOMObjects()
        {
            RegisterManagedObject(new TubusObjectType());
        }
    }
}
