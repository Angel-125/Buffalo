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
    class BuffaloSettings : Window<BuffaloSettings>
    {
        static public float rcsVolume;

        string settingsPath;

        public BuffaloSettings() :
        base("Buffalo Settings", 300, 150)
        {
            Resizable = false;
            settingsPath = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(BuffaloSettings)) + "/Settings.cfg";
            loadSettings();
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("RCS Volume");
            rcsVolume = GUILayout.HorizontalSlider(rcsVolume, 0, 1);
            GUILayout.EndVertical();

            WBIModuleRCS.soundEffectVolume = GameSettings.SHIP_VOLUME * rcsVolume;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);
            ConfigNode nodeSettings = new ConfigNode();

            if (newValue)
            {
                loadSettings();
            }

            else
            {
                nodeSettings.name = "SETTINGS";
                nodeSettings.AddValue("rcsVolume", WBIModuleRCS.soundEffectVolume.ToString());
                nodeSettings.Save(settingsPath);
            }
        }

        protected void loadSettings()
        {
            ConfigNode nodeSettings = new ConfigNode();
            string value;

            //Now load settings
            nodeSettings = ConfigNode.Load(settingsPath);
            if (nodeSettings != null)
            {
                value = nodeSettings.GetValue("rcsVolume");
                if (string.IsNullOrEmpty(value) == false)
                    rcsVolume = float.Parse(value);
                else
                    rcsVolume = 0.003f;
            }
            else
            {
                rcsVolume = 0.003f;
            }
        }

        public static float GetRCSVolume()
        {
            if (rcsVolume != -1.0f)
                return rcsVolume;

            string path = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(BuffaloSettings)) + "/Settings.cfg";
            ConfigNode nodeSettings;

            nodeSettings = ConfigNode.Load(path);
            if (nodeSettings == null)
                return 0f;
            rcsVolume = float.Parse(nodeSettings.GetValue("rcsVolume"));

            return rcsVolume;
        }
    }
}
