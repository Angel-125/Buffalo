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
    [KSPModule("Outback")]
    public class WBIOutback : WBIResourceSwitcher
    {
        Part parentPart;

        [KSPEvent(guiActive = true, guiActiveUnfocused = true, unfocusedRange = 3.0f, guiName = "Refuel EVA Propellant", guiActiveEditor = false)]
        public void RefuelEVA()
        {
            PartResource evaProp = this.part.Resources["EVA Propellant"];

            if (evaProp != null)
                evaProp.amount = evaProp.maxAmount;
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (this.part.parent == parentPart)
                return;
            if (HighLogic.LoadedSceneIsFlight == false)
                return;
            if (this.part.parent == null)
                return;

            ModuleCommand cmd = this.part.parent.FindModuleImplementing<ModuleCommand>();
            bool attachedToRechargePart = this.part.parent.CrewCapacity > 0 && cmd != null;

            Events["RefuelEVA"].guiActive = attachedToRechargePart;
            Events["RefuelEVA"].guiActiveUnfocused = attachedToRechargePart;

            parentPart = this.part.parent;
        }
    }
}
