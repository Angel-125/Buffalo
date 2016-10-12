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
    class HoverVTOLGUI : Window<HoverVTOLGUI>
    {
        public WBIVTOLManager vtolManager;
        Texture settingsIcon;
        public HoverControlSetupGUI hoverSetupGUI = new HoverControlSetupGUI();

        public HoverVTOLGUI(string title = "", int height = 15, int width = 310) :
        base(title, width, height)
        {
            Resizable = false;
            settingsIcon = GameDatabase.Instance.GetTexture("WildBlueIndustries/Buffalo/Icons/Gear", false);
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (hoverSetupGUI == null)
            {
                hoverSetupGUI.vtolManager = this.vtolManager;
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("<color=white>Vertical Speed: {0:f2}m/s</color>", vtolManager.vessel.verticalSpeed));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(settingsIcon, new GUILayoutOption[] { GUILayout.Width(24), GUILayout.Height(24) }))
                hoverSetupGUI.SetVisible(true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            if (vtolManager.hoverActive)
            {
                if (GUILayout.Button("<color=yellow>HOVR</color>\r\n"))
                    vtolManager.ToggleHover();
            }
            else
            {
                if (GUILayout.Button("HOVR\r\n"))
                    vtolManager.ToggleHover();
            }
            GUILayout.Label(vtolManager.codeToggleHover.ToString());
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (GUILayout.Button("VSPD\r\n+"))
                vtolManager.IncreaseVSpeed();
            GUILayout.Label(vtolManager.codeIncreaseVSpeed.ToString());
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (GUILayout.Button("VSPD\r\n0"))
                vtolManager.KillVSpeed();
            GUILayout.Label(vtolManager.codeZeroVSpeed.ToString());
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (GUILayout.Button("VSPD\r\n-"))
                vtolManager.DecreaseVSpeed();
            GUILayout.Label(vtolManager.codeDecreaseVSpeed.ToString());
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
