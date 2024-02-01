using UnityEngine;
using UnityEngine.UI;
namespace FellOnline.Shared
{
    [CreateAssetMenu(fileName = "New HairStyle", menuName = "FellOnline/Character/Customization/Hair", order = 1)]
    public class CharacterHairTemplate  : CachedScriptableObject<CharacterHairTemplate>, ICachedObject
    {
        public int HairID;
        public Sprite HairImage;
        public string HairDescription;
    }
}