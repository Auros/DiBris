using System;

namespace DiBris.Models
{
    [Flags]
    public enum GravityMode
    {
        Default = 0,
        Negative = 1,
        Forwards = 2,
        Backwards = 4,
        Outwards = 8,
        Inwards = 16,
    }
}