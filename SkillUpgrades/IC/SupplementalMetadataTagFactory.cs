using ItemChanger;
using ItemChanger.Tags;

namespace SkillUpgrades.IC
{
    /// <summary>
    /// Class containing methods for adding supplemental metadata tags to taggable objects.
    /// </summary>
    internal static class SupplementalMetadataTagFactory
    {
        private const string CmiPoolGroupProperty = "PoolGroup";
        private const string CmiModSourceProperty = "ModSource";
        private const string TagMessage = "RandoSupplementalMetadata";

        public static InteropTag AddTagToItem(AbstractItem item)
        {
            InteropTag tag = item.AddTag<InteropTag>();
            tag.Message = TagMessage;
            tag.Properties[CmiModSourceProperty] = SkillUpgrades.instance.GetName();
            tag.Properties[CmiPoolGroupProperty] = "Skills";
            return tag;
        }
    }
}
