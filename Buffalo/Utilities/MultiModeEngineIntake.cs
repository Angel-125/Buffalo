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
    public class MultiModeEngineIntake : MultiModeEngine
    {
        [KSPField()]
        public string primaryEngineIntake;

        [KSPField()]
        public bool primaryCheckForOxygen;

        [KSPField()]
        public string secondaryEngineIntake;

        [KSPField()]
        public bool secondaryCheckForOxygen;

        [KSPEvent(guiActive = true, guiName = "Toggle Mode", guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 2.0f)]
        public void ToggleEngineMode()
        {
            //Toggle the mode
            this.Events["ModeEvent"].Invoke();

            //Setup the intake
            SetupIntake();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsEditor == false && HighLogic.LoadedSceneIsFlight == false)
                return;

            //Hide MultiModeEngine toggle button
            this.Events["ModeEvent"].guiActive = false;
            this.Events["ModeEvent"].guiActiveEditor = false;
            this.Events["ModeEvent"].guiActiveUnfocused = false;

            SetupIntake();
        }

        public void SetupIntake()
        {
            ModuleResourceIntake intake = this.part.FindModuleImplementing<ModuleResourceIntake>();
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition resourceDef = null;
            PartResource resource = null;
            string resourceName = "";
            bool checkForOxygen = false;

            if (runningPrimary)
            {
                resourceName = primaryEngineIntake;
                resourceDef = ResourceHelper.DefinitionForResource(resourceName);
                resource = this.part.Resources[resourceDef.name];
                checkForOxygen = primaryCheckForOxygen;

                this.Events["ToggleEngineMode"].guiName = secondaryEngineID;
            }

            else
            {
                resourceName = secondaryEngineIntake;
                resourceDef = ResourceHelper.DefinitionForResource(resourceName);
                resource = this.part.Resources[resourceDef.name];
                checkForOxygen = secondaryCheckForOxygen;

                this.Events["ToggleEngineMode"].guiName = primaryEngineID;
            }

            //Change the resource to intake
            intake.resourceName = resourceName;
            intake.resourceDef = resourceDef;
            intake.resourceId = resourceDef.id;
            intake.res = resource;
            intake.checkForOxygen = checkForOxygen;
        }
    }
}
