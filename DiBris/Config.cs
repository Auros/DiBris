﻿using System;
using UnityEngine;
using DiBris.Models;
using IPA.Config.Stores;
using SiraUtil.Converters;
using System.Collections.Generic;
using IPA.Config.Stores.Converters;
using IPA.Config.Stores.Attributes;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace DiBris
{
    [Serializable]
    internal class Config
    {
        [Ignore]
        public Action<Config>? Updated;

        [UseConverter(typeof(VersionConverter))]
        public SemVer.Version Version { get; set; } = new SemVer.Version("0.0.0");

        // Initial Design

        public string Name { get; set; } = "Default";
        public bool RemoveDebris { get; set; } = false;
        public float LifetimeMultiplier { get; set; } = 1f;
        public float VelocityMultiplier { get; set; } = 1f;
        public float GravityMultiplier { get; set; } = 1f;
        public float RotationMultiplier { get; set; } = 1f;
        public float Scale { get; set; } = 1f;

        public float AbsolutePositionOffsetX { get; set; } = 0f;
        public float AbsolutePositionOffsetY { get; set; } = 0f;
        public float AbsolutePositionOffsetZ { get; set; } = 0f;

        [Ignore]
        public Vector3 AbsolutePositionOffset => new Vector3(AbsolutePositionOffsetX, AbsolutePositionOffsetY, AbsolutePositionOffsetZ);

        public float AbsolutePositionScale { get; set; } = 1f;

        [NonNullable]
        public DisableParam Parameters { get; set; } = new DisableParam();

        [NonNullable, UseConverter(typeof(ListConverter<string>))]
        public List<string> MirrorConfigs { get; set; } = new List<string>();

        // 1.0.0-a2
        public bool FixateRotationToZero { get; set; } = false;
        public bool FixateZPos { get; set; } = false;

        public bool FixedLifetime { get; set; } = false;
        public float FixedLifetimeLength { get; set; } = 0f;

        public bool SnapToGrid { get; set; } = false;
        public float GridScale { get; set; } = 1f;


        // Virtual Config State Methods
        public virtual void Save() { }
        public virtual void CopyFrom(Config _) { }
        public virtual void Changed() => Updated?.Invoke(this);
    }
}