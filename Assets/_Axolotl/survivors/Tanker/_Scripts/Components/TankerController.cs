using UnityEngine;
using RoR2;

namespace Axolotl.Tanker.Modules.Components
{
   public class TankerController : MonoBehaviour
   {

      private CharacterBody characterBody;
      private CharacterModel model;
      private ChildLocator childLocator;
      private Animator modelAnimator;


      private void Awake()
		{
         this.characterBody = this.gameObject.GetComponent<CharacterBody>();
         this.childLocator = this.gameObject.GetComponentInChildren<ChildLocator>();
         this.model = this.gameObject.GetComponentInChildren<CharacterModel>();
         this.modelAnimator = this.gameObject.GetComponentInChildren<Animator>();
      }
   }
}