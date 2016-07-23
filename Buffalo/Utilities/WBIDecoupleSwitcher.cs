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
    public class WBIDecoupleSwitcher : PartModule
    {
        ModuleDecouple[] decouplers;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Add(vesselModified);

            setupDecouplers();
        }

        private void OnDestroy()
        {
            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Remove(vesselModified);
        }

        private void vesselModified(ShipConstruct data)
        {
            setupDecouplers();
        }

        protected void setupDecouplers()
        {
            List<ModuleDecouple> decoupleModules = this.part.FindModulesImplementing<ModuleDecouple>();
            decouplers = decoupleModules.ToArray<ModuleDecouple>();
            int totalCount;

            totalCount = decouplers.Length;
            for (int index = 0; index < totalCount; index++)
            {
                if (decouplers[index].explosiveNodeID == "srf" && this.part.attachMode == AttachModes.SRF_ATTACH)
                {
                    decouplers[index].enabled = true;
                    decouplers[index].isEnabled = true;
                }

                else if (decouplers[index].explosiveNodeID != "srf" && this.part.attachMode == AttachModes.STACK)
                {
                    decouplers[index].enabled = true;
                    decouplers[index].isEnabled = true;
                }

                else
                {
                    decouplers[index].enabled = false;
                    decouplers[index].isEnabled = false;
                }
            }
        }
    }
}
