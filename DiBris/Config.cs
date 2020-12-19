using System.Linq;
using DiBris.Models;
using IPA.Config.Stores;
using System.Collections.Generic;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace DiBris
{
    internal class Config
    {
        public virtual bool DoGravity { get; set; } = true;
        public virtual bool RemoveDebris { get; set; } = false;
        public virtual float LifetimeMultiplier { get; set; } = 1f;
        public virtual float VelocityMultiplier { get; set; } = 1f;
        public virtual float GravityMultiplier { get; set; } = 1f;
        public virtual float Scale { get; set; } = 1f;

        [NonNullable]
        public virtual DisableParam Parameters { get; set; } = new DisableParam();

        [UseConverter(typeof(ListConverter<GravityMode, EnumConverter<GravityMode>>)), NonNullable]
        public virtual List<GravityMode> Modes { get; set; } = new List<GravityMode>();

        [Ignore]
        public GravityMode Mode => (GravityMode)Modes.Select(m => (int)m).Distinct().Sum();
    }
}