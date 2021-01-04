using System;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

namespace DiBris.Models
{
    [Serializable]
    public class DisableParam
    {
        public float NJS { get; set; } = 20f;
        public float NPS { get; set; } = 6f;
        public float Length { get; set; } = 180f;

        public bool DoNJS { get; set; }
        public bool DoNPS { get; set; }
        public bool DoLength { get; set; }
    
        [UseConverter(typeof(EnumConverter<DisableMode>))]
        public DisableMode Mode { get; set; }
    }
}