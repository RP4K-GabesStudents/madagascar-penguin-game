using System;
using UnityEngine;

namespace Common.Extensions
{
    
    /// <summary>
    /// Marks a serialized Object/Behaviour field as requiring an implementation of a
    /// given interface. Pair with RequireInterfaceDrawer (Editor) to validate the
    /// inspector assignment at drop time.
    /// </summary>
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public readonly Type InterfaceType;

        public RequireInterfaceAttribute(Type interfaceType) => InterfaceType = interfaceType;
    }
}