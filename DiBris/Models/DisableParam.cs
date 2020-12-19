using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

namespace DiBris.Models
{
    public class DisableParam
    {
        public virtual float NJS { get; set; } = 20f;
        public virtual float NPS { get; set; } = 6f;
        public virtual float Length { get; set; } = 180f;

        public virtual bool DoNJS { get; set; }
        public virtual bool DoNPS { get; set; }
        public virtual bool DoLength { get; set; }
    
        [UseConverter(typeof(EnumConverter<DisableMode>))]
        public virtual DisableMode Mode { get; set; }
    }
}