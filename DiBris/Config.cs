using System;
using UnityEngine;
using DiBris.Models;
using IPA.Config.Stores;
using SiraUtil.Converters;
using IPA.Config.Stores.Attributes;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace DiBris
{
    [Serializable]
    internal class Config
    {
        [UseConverter(typeof(VersionConverter))]
        public SemVer.Version Version = new SemVer.Version("0.0.0");

        public string Name { get; set; } = "Default";
        public bool RemoveDebris { get; set; } = false;
        public float LifetimeMultiplier { get; set; } = 1f;
        public float VelocityMultiplier { get; set; } = 1f;
        public float GravityMultiplier { get; set; } = 1f;
        public float RotationMultiplier { get; set; } = 1f;
        public float Scale { get; set; } = 1f;

        [UseConverter(typeof(Vector3Converter))]
        public Vector3 AbsolutePositionOffset { get; set; } = Vector3.zero;
        public float AbsolutePositionScale { get; set; } = 1f;

        [NonNullable]
        public DisableParam Parameters { get; set; } = new DisableParam();
    }
}