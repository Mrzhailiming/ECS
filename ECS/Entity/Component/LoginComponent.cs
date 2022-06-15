using Entity.DataStruct;

namespace Entity.Component
{
    [ComponentAttribute(mComponentType = ComponentType.Base, mInitComponentType = InitComponentType.LoginComponent)]
    public class LoginComponent : IComponent
    {
        public IEntity mOwner { get; set; }

        public LoginState mLoginState = LoginState.Loging;

    }
}
