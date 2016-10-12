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
    class AsteroidScannerInfo : Window<AsteroidScannerInfo>
    {
        public ModuleAsteroid asteroid = null;
        public ModuleAsteroidInfo asteroidInfo = null;

        Vector2 scrollPosition, scrollPos2 =  new Vector2();
        List<ModuleAsteroidResource> astroResources = new List<ModuleAsteroidResource>();

        public AsteroidScannerInfo() :
            base("Asteroid Analysis", 380, 450)
        {
            Resizable = false;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);
            astroResources.Clear();
            astroResources = this.asteroid.part.FindModulesImplementing<ModuleAsteroidResource>();
        }

        protected override void DrawWindowContents(int windowId)
        {
            string resourceString = null;
            string resourcePercentage = asteroidInfo.resources;
            string displayMass = asteroidInfo.displayMass;
            if (asteroid == null || asteroidInfo == null)
                return;

            GUILayout.BeginVertical();
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label("<color=white><b>Name: </b>" + asteroid.AsteroidName + "</color>");
            GUILayout.Label(string.Format("<color=white><b>Density: </b>{0:f2}t/L</color>", asteroid.density));
            GUILayout.Label("<color=white><b>Mass: </b>" + displayMass + "</color>");
            GUILayout.Label("<color=white><b>Resources Mass: </b>" + resourcePercentage + "</color>");
            GUILayout.EndScrollView();

            GUILayout.Label("<color=white><b>Resource Composition</b></color>");
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
            foreach (ModuleAsteroidResource resource in astroResources)
            {
                if (resource.abundance > 0.0f)
                {
                    resourceString = string.Format("{0:f2}%", resource.displayAbundance * 100f);
                    GUILayout.Label("<color=white><b>" + resource.resourceName + ": </b> " + resourceString + "</color>");
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
