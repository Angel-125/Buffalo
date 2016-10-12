using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WildBlueIndustries
{
    [KSPModule("Grappler")]
    class WBIModuleGrappleNode : ModuleGrappleNode
    {
        ModuleAnimateGeneric anim = null;

        public override void OnStart(StartState st)
        {
            base.OnStart(st);
            anim = this.part.FindModuleImplementing<ModuleAnimateGeneric>();
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
        }

        public void OnVesselWasModified(Vessel vessel)
        {
            if (anim == null)
                return;

            //Have we released our target?
            if (vessel == this.part.vessel)
            {
                if (dockedPartUId == 0 || otherVesselInfo == null)
                {
                    if (anim.Events["Toggle"].guiName == anim.startEventGUIName)
                        anim.Toggle();
                }
            }
        }
    }
}
