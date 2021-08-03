using EntityStates;
using Axolotl.Tanker.Modules.Components;

namespace Axolotl.Tanker.SkillStates
{
    public class BaseHenrySkillState : BaseSkillState
    {
        protected HenryController henryController;

        public override void OnEnter()
        {
            this.henryController = base.GetComponent<HenryController>();
            base.OnEnter();
        }
    }
}