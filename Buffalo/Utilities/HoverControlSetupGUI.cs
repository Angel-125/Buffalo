using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

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
    class HoverControlSetupGUI : Window<HoverControlSetupGUI>
    {
        public WBIVTOLManager vtolManager;

        protected string[] controlLabels = null;
        protected Dictionary<string, KeyCode> controlCodes;
        protected int currentControl = 0;

        public HoverControlSetupGUI(string title = "Hover Control Setup", int height = 45, int width = 390) :
        base(title, width, height)
        {
            Resizable = false;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (newValue)
            {
                controlCodes = vtolManager.controlCodes;

                List<string> labels = new List<string>();

                foreach (string key in controlCodes.Keys)
                    labels.Add(key);

                controlLabels = labels.ToArray();
            }
        }

        protected override void DrawWindowContents(int windowId)
        {
            GUILayout.BeginVertical();

            //Draw current control codes
            GUILayout.BeginHorizontal();
            foreach (string key in controlCodes.Keys)
                drawControlLabel(key + "\r\n" + controlCodes[key].ToString());
            GUILayout.EndHorizontal();

            //Draw instructions
            GUILayout.Label(string.Format("<color=white>Click on a button and then press a key to map the control.</color>"));

            //Get current control that we're going to change
            currentControl = GUILayout.SelectionGrid(currentControl, controlLabels, 4);

            //If user presses a key, remap the current control
            Event evnt = Event.current;
            if (evnt.keyCode != KeyCode.None)
                controlCodes[controlLabels[currentControl]] = evnt.keyCode;

            //Draw OK and Cancel buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Accept"))
            {
                SetVisible(false);
                vtolManager.SetControlCodes(controlCodes);
            }

            if (GUILayout.Button("Cancel"))
                SetVisible(false);

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        protected void drawControlLabel(string labelText)
        {
            GUILayout.BeginScrollView(new Vector2(0, 0), new GUIStyle(GUI.skin.textArea), GUILayout.Height(60));
            GUILayout.Label(labelText);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
        }
    }

}
