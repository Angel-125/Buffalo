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
    public class ModuleOmniWheel : PartModule
    {
        [KSPField]
        public string wheelPylonName;

        [KSPField]
        public string wheelOuterPistonName;

        [KSPField(isPersistant = true)]
        public bool isNarrowWheelbase;

        [KSPField(isPersistant = true)]
        public bool toggledAtLeastOnce;

        Transform wheelPylon;
        Transform wheelOuterPiston;

        [KSPAction("Toggle Wheelbase")]
        public void ToggleWheelbaseAction(KSPActionParam param)
        {
            ToggleWheelbase();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true)]
        public void ToggleWheelbase()
        {
            isNarrowWheelbase = !isNarrowWheelbase;

            //To avoid weirdness with moving the meshes, record that we toggled the wheelbase at least once.
            //It's a hack but it works...
            toggledAtLeastOnce = true;

            setWheelbase();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            wheelPylon = findTransform(wheelPylonName);
            wheelOuterPiston = findTransform(wheelOuterPistonName);

            setWheelbase();
        }

        //Trying to use animation to shorten/widen the wheelbase looks really wonky, so we'll do this in code by moving the transforms.
        //Sadly this doesn't work to well when we have stuff piled onto the chassis. I think this is basically the right approach but
        //shelved for now.
        private void setWheelbase()
        {
            if (!toggledAtLeastOnce)
                return;

            ModuleWheelBase wheelBase = this.part.FindModuleImplementing<ModuleWheelBase>();

            if (isNarrowWheelbase)
            {
                wheelBase.wheelColliderHost.Translate(0.185f, 0f, 0f);
                wheelPylon.Translate(0.185f, 0f, 0f);
            }
            else
            {
                wheelBase.wheelColliderHost.Translate(-0.185f, 0f, 0f);
                wheelPylon.Translate(-0.185f, 0f, 0f);
            }

            ModuleWheelSteering steering = this.part.FindModuleImplementing<ModuleWheelSteering>();
        }

        private Transform findTransform(string transformName)
        {
            Transform[] targets;

            //Get the targets
            targets = part.FindModelTransforms(transformName);
            if (targets == null)
            {
                Debug.Log("No targets found for " + transformName);
                return null;
            }

            return targets[0];
        }
    }
}
