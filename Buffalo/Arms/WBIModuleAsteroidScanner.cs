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
    [KSPModule("Asteroid Scanner")]
    public class WBIModuleAsteroidScanner : PartModule, IModuleInfo
    {
        private const float kMessageDuration = 5.0f;
        private const string kNoAnalysisModule = "Unable to find an analysis module, cannot proceed!";
        private const string kNoPotato = "No analysis possible. It doesn't appear that you're attached to an asteroid.";

        protected AsteroidScannerInfo scannerInfo = new AsteroidScannerInfo();

        [KSPEvent(guiName = "Scan Asteroid", guiActive = true, guiActiveUnfocused = true, unfocusedRange = 3.0f)]
        public void ScanAsteroid()
        {
            ModuleAnimateGeneric animation = this.part.FindModuleImplementing<ModuleAnimateGeneric>();
            ModuleAsteroidAnalysis asteroidAnalysis = this.part.FindModuleImplementing<ModuleAsteroidAnalysis>();
            ModuleAsteroid asteroid = null;
            ModuleAsteroidInfo asteroidInfo = null;

            //Make sure we have an analysis module
            if (asteroidAnalysis == null)
            {
                ScreenMessages.PostScreenMessage(kNoAnalysisModule, kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //See if we can find the asteroid modules
            asteroid = this.part.vessel.FindPartModuleImplementing<ModuleAsteroid>();
            if (asteroid == null)
            {
                ScreenMessages.PostScreenMessage(kNoPotato, kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                return;
            }
            asteroidInfo = asteroid.part.FindModuleImplementing<ModuleAsteroidInfo>();
            if (asteroidInfo == null)
            {
                ScreenMessages.PostScreenMessage(kNoPotato, kMessageDuration, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            //Good to go
            //Extend the arm if needed.
            if (animation != null)
            {
                if (animation.Events["Toggle"].guiName == animation.startEventGUIName)
                    animation.Toggle();
            }

            //Display the window
            scannerInfo.asteroid = asteroid;
            scannerInfo.asteroidInfo = asteroidInfo;
            scannerInfo.SetVisible(true);
        }

        public string GetModuleTitle()
        {
            return "Asteroid Scanner";
        }

        public string GetPrimaryField()
        {
            return "Asteroid Scanning";
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public override string GetInfo()
        {
            string moduleInfo = "Usage: Dock your vessel to an asteroid and right-click the part. Press the Scan Asteroid button to see information about the asteroid.";

            return moduleInfo;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        }
    }
}
