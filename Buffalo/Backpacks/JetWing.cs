using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code copyrighgt 2015, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/
If you want to use this code, give me a shout on the KSP forums! :)
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    class JetWing : PartModule
    {
        [KSPField()]
        public string primaryEngineFuel = "";

        [KSPField()]
        public string secondaryEngineFuel = "";

        KerbalSeat seat;
        ModuleCommand wingCommander;
        KerbalEVA evaKerbal;
        ModuleDecouple decoupler;
        MultiModeEngine multiModeEngine;
        ModuleEnginesFX primaryEngine;
        ModuleEnginesFX secondaryEngine;
        WBIResourceSwitcher resourceSwitcher;
        WBIMultiEngineHover hoverEngine;

        ModuleRCS rcsModule;
        float originalThrusterPower;

        [KSPEvent(guiActive = true, guiName = "Decouple")]
        public void Decoupler()
        {
            decoupler.Decouple();

            //Reset the decoupler so we can reuse it.
            decoupler.isDecoupled = false;
            decoupler.staged = false;
        }

        [KSPEvent(guiName = "Board", guiActiveUnfocused = true, externalToEVAOnly = true, unfocusedRange = 2f)]
        public void BoardWing()
        {
            seat.BoardSeat();
            wingCommander.MakeReference();
            evaKerbal = seat.Occupant.FindModuleImplementing<KerbalEVA>();
        }

        /*
        [KSPEvent(guiActive = true, guiName = "Test")]
        public void Test()
        {
            FlightGlobals.ActiveVessel.ctrlState.mainThrottle = 0.3f;
            FlightInputHandler.state.mainThrottle = 0.3f;
        }
         */

//        public bool engineActivated;
//        public bool sasActivated;
//        Quaternion currentHeading;

        protected void moveThrustToCOM()
        {
            if (primaryEngine == null || secondaryEngine == null)
                return;

            int transformCount = primaryEngine.thrustTransforms.Count;

            for (int index = 0; index < transformCount; index++)
                primaryEngine.thrustTransforms[index].position = this.part.vessel.CurrentCoM;

            transformCount = secondaryEngine.thrustTransforms.Count;
            for (int index = 0; index < transformCount; index++)
                secondaryEngine.thrustTransforms[index].position = this.part.vessel.CurrentCoM;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (HighLogic.LoadedSceneIsFlight == false)
                return;
            FXGroup thrusterFX;

            //Move engine transforms to center of mass
            moveThrustToCOM();

            //We need engine power to run the RCS
            if (vessel.ActionGroups[KSPActionGroup.RCS])
            {
                //Check operational state
                if (hasThrustForRCS() == false)
                {
                    rcsModule.thrusterPower = 0.0f;

                    for (int thrusterIndex = 0; thrusterIndex < rcsModule.thrusterFX.Count; thrusterIndex++)
                    {
                        thrusterFX = rcsModule.thrusterFX[thrusterIndex];
                        thrusterFX.Power = 0.0f;
                    }
                }

                else
                {
                    rcsModule.thrusterPower = originalThrusterPower;
                }
            }

            /*
            if (Input.GetKeyDown(KeyCode.Z) && engineActivated == false)
            {
                primaryEngine.Activate();
                //primaryEngine.currentThrottle = 1.0f;
                engineActivated = true;
                //wingCommander.MakeReference();
                FlightGlobals.ActiveVessel.Autopilot.SetupModules();
                FlightGlobals.ActiveVessel.Autopilot.SAS.ModuleSetup();
                FlightGlobals.ActiveVessel.Autopilot.Enable(VesselAutopilot.AutopilotMode.StabilityAssist);
//                currentHeading = Quaternion.LookRotation(vessel.transform.up) * Quaternion.Euler(90,0,0);
//                currentHeading = FlightGlobals.ActiveVessel.Autopilot.SAS.currentRotation;
//                FlightGlobals.ActiveVessel.Autopilot.SAS.LockHeading(currentHeading, true);
//                FlightGlobals.ActiveVessel.Autopilot.SAS.SetDampingMode(true);
                FlightGlobals.ActiveVessel.Autopilot.SAS.ConnectFlyByWire();
                FlightGlobals.ActiveVessel.Autopilot.SAS.SetLockPitchPID(0, 0, 0, 0);
            }

            if (engineActivated)
            {
                //primaryEngine.currentThrottle = 1.0f;
                FlightGlobals.ActiveVessel.ctrlState.mainThrottle = 1f;
                FlightInputHandler.state.mainThrottle = 1f;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                primaryEngine.currentThrottle = 0f;
                primaryEngine.Shutdown();
                engineActivated = false;
            }

            if (Input.GetKey(KeyCode.T))
            {
                if (sasActivated == false)
                {
                    part.vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                    sasActivated = true;
                }
                else
                {
                    part.vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
                    sasActivated = false;
                }
            }
             */
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight == false)
                return;

            rcsModule = this.part.FindModuleImplementing<ModuleRCS>();
            originalThrusterPower = rcsModule.thrusterPower;

            //Set landing gear action
            ModuleAnimateGeneric kickstandAnim = this.part.FindModuleImplementing<ModuleAnimateGeneric>();
            kickstandAnim.Actions[0].actionGroup = KSPActionGroup.Gear;

            //Setup the GUI
            setupGUI();
           
            //Get the primary and secondary engine
            List<ModuleEnginesFX> engineList = this.part.FindModulesImplementing<ModuleEnginesFX>();
            foreach (ModuleEnginesFX engine in engineList)
            {
                if (engine.engineID == multiModeEngine.primaryEngineID)
                    primaryEngine = engine;
                else if (engine.engineID == multiModeEngine.secondaryEngineID)
                    secondaryEngine = engine;
            }

            //Setup the engine mode
            setupEngineMode();
        }

        protected bool hasThrustForRCS()
        {
            //Check operational state
            if (primaryEngine.isOperational == false && secondaryEngine.isOperational == false)
                return false;

            //Make sure we're not out of gas
            else if (primaryEngine.propellantReqMet < 0.001f && secondaryEngine.propellantReqMet < 0.001f)
                return false;

            //Check thrust
            else if (primaryEngine.finalThrust < 0.001f && secondaryEngine.finalThrust < 0.001f)
                return false;

            return true;
        }

        protected void setupEngineMode()
        {
            //Auto-set the engines based upon fuel type.
            if (resourceSwitcher.CurrentTemplateName == primaryEngineFuel)
            {
                if (multiModeEngine.runningPrimary == false)
                    multiModeEngine.Events["ModeEvent"].Invoke();
            }

            else if (resourceSwitcher.CurrentTemplateName == secondaryEngineFuel)
            {
                if (multiModeEngine.runningPrimary)
                    multiModeEngine.Events["ModeEvent"].Invoke();
            }
        }

        protected void setupGUI()
        {
            //Hide seat GUI
            seat = this.part.FindModuleImplementing<KerbalSeat>();
            seat.Events["BoardSeat"].guiActive = false;
            seat.Events["BoardSeat"].guiActiveEditor = false;
            seat.Events["BoardSeat"].guiActiveUnfocused = false;

            //Hide probe command GUI
            wingCommander = this.part.FindModuleImplementing<ModuleCommand>();
            wingCommander.Events["MakeReference"].guiActive = false;
            wingCommander.Events["MakeReference"].guiActiveUnfocused = false;
            wingCommander.Events["RenameVessel"].guiActive = false;

            //Hide decoupler GUI
            decoupler = this.part.FindModuleImplementing<ModuleDecouple>();
            decoupler.Events["Decouple"].guiActive = false;
            decoupler.Events["Decouple"].guiActiveEditor = false;
            decoupler.Events["Decouple"].guiActiveUnfocused = false;

            //Hide MultiModeEngine toggle button
            multiModeEngine = this.part.FindModuleImplementing<MultiModeEngine>();
            multiModeEngine.Events["ModeEvent"].guiActive = false;
            multiModeEngine.Events["ModeEvent"].guiActiveEditor = false;
            multiModeEngine.Events["ModeEvent"].guiActiveUnfocused = false;

            //Hide the Close Intake button.
            ModuleResourceIntake intake = this.part.FindModuleImplementing<ModuleResourceIntake>();
            intake.Events["Deactivate"].guiActive = false;

            //Hide RCS GUI
            ModuleRCS rcs = this.part.FindModuleImplementing<ModuleRCS>();
            rcs.Fields["realISP"].guiActive = false;
            rcs.Fields["rcsEnabled"].guiActive = false;

            //Hide hover engine gui
            hoverEngine = this.part.FindModuleImplementing<WBIMultiEngineHover>();
            hoverEngine.SetGUIVisible(false);

            //Set fuel type
            resourceSwitcher = this.part.FindModuleImplementing<WBIResourceSwitcher>();
            resourceSwitcher.Fields["shortName"].guiName = "Fuel Type";
        }

    }
}
