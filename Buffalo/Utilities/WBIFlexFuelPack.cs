using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using ModuleWheels;

/*
Source code copyright 2016, by Michael Billard (Angel-125)
License: GNU General Public License Version 3
License URL: http://www.gnu.org/licenses/
Wild Blue Industries is trademarked by Michael Billard and may be used for non-commercial purposes. All other rights reserved.
Note that Wild Blue Industries is a ficticious entity 
created for entertainment purposes. It is in no way meant to represent a real entity.
Any similarity to a real entity is purely coincidental.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
namespace WildBlueIndustries
{
    [KSPModule("Flex Fuel Switcher")]
    public class WBIFlexFuelPack : PartModule
    {
        ModuleResourceConverter converter;
        bool particlesEnabled;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            WBIConvertibleStorage storage = this.part.FindModuleImplementing<WBIConvertibleStorage>();
            if (storage != null)
            {
                storage.Events["ReconfigureStorage"].guiName = "Select Fuel";
                storage.SetWindowTitle("Select Fuel");

                if (HighLogic.LoadedSceneIsFlight)
                    storage.onModuleRedecorated += new ModuleRedecoratedEvent(storage_onModuleRedecorated);
            }
        }

        void storage_onModuleRedecorated(ConfigNode templateNode)
        {
            converter = this.part.FindModuleImplementing<ModuleResourceConverter>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (converter == null && HighLogic.LoadedSceneIsFlight)
                converter = this.part.FindModuleImplementing<ModuleResourceConverter>();

            if (converter.IsActivated == false && particlesEnabled)
            {
                particlesEnabled = false;
                setEmittersVisible(false);
            }

        }

        protected void setEmittersVisible(bool isVisible)
        {
            KSPParticleEmitter[] emitters = part.GetComponentsInChildren<KSPParticleEmitter>();
            int totalCount = emitters.Length;
            KSPParticleEmitter emitter;

            for (int index = 0; index < totalCount; index++)
            {
                emitter = emitters[index];
                emitter.emit = isVisible;
                emitter.enabled = isVisible;
            }
        }
    }
}
