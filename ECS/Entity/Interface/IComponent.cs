using System;
using System.Collections.Generic;
using System.Text;

namespace Entity
{
    public interface IComponent
    {
        IEntity mOwner { get; }
    }
}
