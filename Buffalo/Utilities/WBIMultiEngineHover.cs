using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/*
Source code derived from FSengineHover by Snjo
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
    [KSPModule("Multi Engine Hover")]
    public class WBIMultiEngineHover : PartModule
    {
        [KSPField()]
        public float ecGenerated = 1.0f;

        [KSPField()]
        public string primaryEngineIntake = "";

        [KSPField()]
        public bool primaryCheckForOxygen = false;

        [KSPField()]
        public string secondaryEngineIntake = "";

        [KSPField()]
        public bool secondaryCheckForOxygen = false;

        [KSPField()]
        public string turbineTransformNames = "";

        [KSPField]
        public float verticalSpeedIncrements = 1f;

        [KSPField(guiActive = true, guiName = "Vertical Speed")]
        public float verticalSpeed = 0f;

        [KSPField(isPersistant = true)]
        public bool hoverActive = false;

        [KSPField]
        public float thrustSmooth = 0.01f;

        [KSPField(isPersistant = true)]
        public float maxThrust = 0f;

        [KSPField(isPersistant = true)]
        public bool maxThrustFetched = false;

        [KSPField(isPersistant = true)]
        private bool runningPrimary;

        protected List<Transform> turbineTransforms;
        protected ModuleEngines primaryEngine;
        protected ModuleEngines secondaryEngine;
        protected ModuleEngines currentEngine;
        protected MultiModeEngine multiModeEngine;
        protected float currentThrustNormalized = 0f;
        protected float targetThrustNormalized = 0f;
        protected float minThrust = 0f;
        protected bool guiVisible = true;

        [KSPEvent(guiActive = true, guiName = "Switch Engine Mode", guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 2.0f)]
        public void ToggleMode()
        {
            //Toggle the mode
            multiModeEngine.Events["ModeEvent"].Invoke();

            //Setup the intake
            SetupIntake();
        }

        [KSPEvent(guiActive = true, guiName = "Toggle Hover")]
        public void ToggleHoverMode()
        {
            hoverActive = !hoverActive;
            if (hoverActive)
                ActivateHover();
            else
                DeactivateHover();
        }

        public void SetHoverMode(bool isActive)
        {
            hoverActive = isActive;

            if (hoverActive)
                ActivateHover();
            else
                DeactivateHover();
        }

        [KSPEvent(guiActive = true, guiName = "Vertical Speed +")]
        public void IncreaseVSpeed()
        {
            verticalSpeed += verticalSpeedIncrements;
            printSpeed();
        }

        [KSPEvent(guiActive = true, guiName = "Vertical Speed -")]
        public void DecreaseVSpeed()
        {
            verticalSpeed -= verticalSpeedIncrements;
            printSpeed();
        }

        [KSPAction("Toggle Hover")]
        public void toggleHoverAction(KSPActionParam param)
        {
            ToggleHoverMode();
        }

        [KSPAction("Vertical Speed +")]
        public void increaseVerticalSpeed(KSPActionParam param)
        {
            IncreaseVSpeed();
        }

        [KSPAction("Vertical Speed -")]
        public void decreaseVerticalSpeed(KSPActionParam param)
        {
            DecreaseVSpeed();
        }

        public void KillVSpeed()
        {
            verticalSpeed = 0f;
            printSpeed();
        }

        [KSPEvent(guiActive = true, guiName = "Show GUI")]
        public void ShowGUI()
        {
            WBIVTOLManager.Instance.ShowGUI();
        }

        public void SetGUIVisible(bool isVisible)
        {
            guiVisible = isVisible;
            Events["ToggleHoverMode"].guiActive = isVisible;
            Events["IncreaseVSpeed"].guiActive = isVisible;
            Events["DecreaseVSpeed"].guiActive = isVisible;
            Fields["verticalSpeed"].guiActive = isVisible;

            //ShowGUI event is always shown...
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsFlight)
            {
                multiModeEngine = this.part.FindModuleImplementing<MultiModeEngine>();
                if (multiModeEngine == null)
                {
                    currentEngine = this.part.FindModuleImplementing<ModuleEngines>();
                }

                else //Find the primary and secondary engines
                {
                    List<ModuleEngines> engineList = this.part.FindModulesImplementing<ModuleEngines>();
                    foreach (ModuleEngines engine in engineList)
                    {
                        if (engine.engineID == multiModeEngine.primaryEngineID)
                            primaryEngine = engine;
                        else if (engine.engineID == multiModeEngine.secondaryEngineID)
                            secondaryEngine = engine;
                    }

                    //Now set the current engine
                    SetCurrentEngine();
                }

                //Set min/max thrust
                SetMinMaxThrust();

                //Set hover state
                if (hoverActive)
                    ActivateHover();
                else
                    DeactivateHover();

                //Set gui visible state
                SetGUIVisible(guiVisible);

                //Get the transforms for the turbine blades
                getTurbineTransforms();

                //Setup the intake for the current engine mode.
                SetupIntake();
            }
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (HighLogic.LoadedSceneIsFlight == false && vessel != FlightGlobals.ActiveVessel)
                return;

            //Run the hover system
            setHoverThrottle();

            //Generate electricty; for some reason the ModuleAlternator wasn't working so we'll do it ourselves.
            generateECAndSpinTurbines();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (HighLogic.LoadedSceneIsFlight && vessel == FlightGlobals.ActiveVessel)
            {
                //Make sure we have the right engine
                if (multiModeEngine != null)
                {
                    if (runningPrimary != multiModeEngine.runningPrimary)
                    {
                        SetCurrentEngine();
                        SetMinMaxThrust();
                    }
                }

                //Monitor the throttle
                if (FlightInputHandler.state.mainThrottle <= 0.001f && hoverActive)
                    DeactivateHover();
            }
        }

        #region Helpers
        protected void setHoverThrottle()
        {
            if (hoverActive)
            {
                //Do we go up or down?
                if (vessel.verticalSpeed >= verticalSpeed)
                    targetThrustNormalized = 0f;
                else if (vessel.verticalSpeed < verticalSpeed)
                    targetThrustNormalized = 1f;

                //Normalize the thrust
                currentThrustNormalized = Mathf.Lerp(currentThrustNormalized, targetThrustNormalized, thrustSmooth);

                //Calculate new thrust
                float newThrust = maxThrust * currentThrustNormalized;
                if (newThrust <= minThrust)
                    newThrust = minThrust + 0.001f;

                //Set the throttle based upon thrust
                if (currentEngine != null)
                {
                    currentEngine.currentThrottle = newThrust / maxThrust;
                }
                else
                {
                    Debug.Log("currentEngine is null");
                }
            }
        }

        protected void generateECAndSpinTurbines()
        {
            float ecToGenerate = ecGenerated * TimeWarp.fixedDeltaTime;

            if (multiModeEngine.runningPrimary)
            {
                if (primaryEngine.isOperational)
                    this.part.RequestResource("ElectricCharge", -ecToGenerate * primaryEngine.currentThrottle);

                //Spin the turbines too
                spinTurbines(primaryEngine);
            }
            else
            {
                if (secondaryEngine.isOperational)
                    this.part.RequestResource("ElectricCharge", -ecToGenerate * secondaryEngine.currentThrottle);

                //Spin the turbines too
                spinTurbines(secondaryEngine);
            }
        }

        protected void spinTurbines(ModuleEngines engine)
        {
            foreach (Transform turbineTransform in turbineTransforms)
            {
                turbineTransform.Rotate(0, 0, -30f * engine.currentThrottle);
            }
        }

        protected void getTurbineTransforms()
        {
            if (string.IsNullOrEmpty(turbineTransformNames))
                return;

            char[] delimiters = { ';' };
            string[] transformNames = turbineTransformNames.Replace(" ", "").Split(delimiters);

            //Sanity checks
            if (transformNames == null || transformNames.Length == 0)
            {
                Debug.Log("transformNames are null");
                return;
            }

            Transform[] targets;

            //Sanity checks
            if (transformNames == null || transformNames.Length == 0)
            {
                Debug.Log("transformNames are null");
                return;
            }

            //Go through all the named panels and find their transforms.
            turbineTransforms = new List<Transform>();
            foreach (string transformName in transformNames)
            {
                //Get the targets
                targets = part.FindModelTransforms(transformName);
                if (targets == null)
                {
                    Debug.Log("No targets found for " + transformName);
                    continue;
                }

                foreach (Transform target in targets)
                    turbineTransforms.Add(target);
            }

        }

        public void SetupIntake()
        {
            ModuleResourceIntake intake = this.part.FindModuleImplementing<ModuleResourceIntake>();
            if (intake == null)
                return;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition resourceDef = null;
            PartResource resource = null;
            string resourceName = "";
            bool checkForOxygen = false;

            if (multiModeEngine.runningPrimary)
            {
                resourceName = primaryEngineIntake;
                resourceDef = ResourceHelper.DefinitionForResource(resourceName);
                resource = this.part.Resources[resourceDef.name];
                checkForOxygen = primaryCheckForOxygen;
            }

            else
            {
                resourceName = secondaryEngineIntake;
                resourceDef = ResourceHelper.DefinitionForResource(resourceName);
                resource = this.part.Resources[resourceDef.name];
                checkForOxygen = secondaryCheckForOxygen;
            }

            //Change the resource to intake
            intake.resourceName = resourceName;
            intake.resourceDef = resourceDef;
            intake.resourceId = resourceDef.id;
            intake.res = resource;
            intake.checkForOxygen = checkForOxygen;
        }

        public void printSpeed()
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage("Hover Climb Rate: " + verticalSpeed, 1f, ScreenMessageStyle.UPPER_CENTER));
        }

        public void SetCurrentEngine()
        {
            if (multiModeEngine == null)
                return;

            if (multiModeEngine.runningPrimary)
            {
                runningPrimary = true;
                currentEngine = primaryEngine;
            }
            else
            {
                runningPrimary = false;
                currentEngine = secondaryEngine;
            }
        }

        public void SetMinMaxThrust()
        {
            if (currentEngine != null)
            {
                if (maxThrustFetched && maxThrust > 0f)
                {
                    currentEngine.maxThrust = maxThrust;
                }
                else
                {
                    maxThrust = currentEngine.maxThrust;
                    maxThrustFetched = true;
                }
                minThrust = currentEngine.minThrust;
            }
        }

        public void ActivateHover()
        {
            if (currentEngine != null)
            {
                hoverActive = true;
                verticalSpeed = 0f;
                vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, true);
                if (guiVisible)
                {
                    Events["ToggleHoverMode"].guiName = "Turn Off Hover Mode";
                    Events["IncreaseVSpeed"].guiActive = true;
                    Events["DecreaseVSpeed"].guiActive = true;
                    Fields["verticalSpeed"].guiActive = true;
                }
                ScreenMessages.PostScreenMessage(new ScreenMessage("Hover Mode On", 1f, ScreenMessageStyle.UPPER_CENTER));
            }
        }

        public void DeactivateHover()
        {
            if (currentEngine != null)
            {
                currentEngine.currentThrottle = FlightInputHandler.state.mainThrottle;
                hoverActive = false;
                verticalSpeed = 0f;
                vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, false);
                if (guiVisible)
                {
                    Events["ToggleHoverMode"].guiName = "Turn On Hover Mode";
                    Events["IncreaseVSpeed"].guiActive = false;
                    Events["DecreaseVSpeed"].guiActive = false;
                    Fields["verticalSpeed"].guiActive = false;
                }
                ScreenMessages.PostScreenMessage(new ScreenMessage("Hover Mode Off", 1f, ScreenMessageStyle.UPPER_CENTER));
            }
        }
        #endregion
    }
}