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
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Flex Fuel")]
        public string currentGeneratorName = string.Empty;

        [KSPField(isPersistant = true)]
        public int currentGenerator;

        List<ModuleResourceConverter> generators;

        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 5.0f, guiName = "Toggle Fuel", guiActiveEditor = true)]
        public void ToggleGenerator()
        {
            //Disable the current generator
            generators[currentGenerator].DisableModule();

            //Get the next generator in the list
            currentGenerator += 1;
            if (currentGenerator >= generators.Count)
                currentGenerator = 0;

            //Enable the new generator
            generators[currentGenerator].EnableModule();
            currentGeneratorName = generators[currentGenerator].ConverterName;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            generators = this.part.FindModulesImplementing<ModuleResourceConverter>();

            for (int index = 0; index < generators.Count; index++)
            {
                if (index != currentGenerator)
                {
                    generators[index].DisableModule();
                }

                else
                {
                    //Now enable the current converter
                    generators[index].EnableModule();
                    currentGeneratorName = generators[index].ConverterName;
                }
            }
        }
    }
}
