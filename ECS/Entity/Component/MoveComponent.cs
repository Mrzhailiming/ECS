using Entity.DataStruct;

namespace Entity.Component
{
    [ComponentAttribute(mComponentType = ComponentType.Base, mInitComponentType = InitComponentType.MoveComponent)]
    public class MoveComponent : IComponent
    {
        public bool IsMoving { get; set; } = false;
        public IEntity mOwner { get; set; }

        public long X { get; set; }
        public long Y { get; set; }

        public long Life { get; set; } = 100;
    }
}
