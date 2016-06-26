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
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class WBIVTOLManager : MonoBehaviour
    {
        public const string LABEL_HOVER = "HOVR";
        public const string LABEL_VSPDINC = "VSPD +";
        public const string LABEL_VSPDZERO = "VSPD 0";
        public const string LABEL_VSPDDEC = "VSPD -";

        public static WBIVTOLManager Instance;

        public KeyCode codeToggleHover = KeyCode.Insert;
        public KeyCode codeIncreaseVSpeed = KeyCode.PageUp;
        public KeyCode codeDecreaseVSpeed = KeyCode.PageDown;
        public KeyCode codeZeroVSpeed = KeyCode.Delete;

        public bool hoverActive = false;
        public Vessel vessel;
        public Dictionary<string, KeyCode> controlCodes = new Dictionary<string,KeyCode>();

        private List<WBIMultiEngineHover> hoverEngines = new List<WBIMultiEngineHover>();
        private HoverVTOLGUI hoverGUI;
        private string hoverControlsPath;

        public void Start()
        {
            WBIVTOLManager.Instance = this;
            GameEvents.onVesselLoaded.Add(VesselWasLoaded);
            GameEvents.onVesselChange.Add(VesselWasChanged);

            hoverGUI = new HoverVTOLGUI();
            hoverGUI.vtolManager = this;

            this.vessel = FlightGlobals.ActiveVessel;

            //Get the current control code mappings
            hoverControlsPath = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(WBIVTOLManager)) + "/VTOLControls.cfg";
            loadControls();
        }

        protected void loadControls()
        {
            ConfigNode nodeControls = null;
            KeyCode keyCode;

            //Now load the controls
            nodeControls = ConfigNode.Load(hoverControlsPath);
            if (nodeControls != null)
            {
                if (nodeControls.HasValue(LABEL_HOVER))
                {
                    keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), nodeControls.GetValue(LABEL_HOVER));
                    controlCodes.Add(LABEL_HOVER, keyCode);
                    codeToggleHover = keyCode;
                }
                else
                {
                    controlCodes.Add(LABEL_HOVER, KeyCode.Insert);
                }

                if (nodeControls.HasValue(LABEL_VSPDINC))
                {
                    keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), nodeControls.GetValue(LABEL_VSPDINC));
                    controlCodes.Add(LABEL_VSPDINC, keyCode);
                    codeIncreaseVSpeed = keyCode;
                }
                else
                {
                    controlCodes.Add(LABEL_VSPDINC, KeyCode.PageUp);
                }

                if (nodeControls.HasValue(LABEL_VSPDZERO))
                {
                    keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), nodeControls.GetValue(LABEL_VSPDZERO));
                    controlCodes.Add(LABEL_VSPDZERO, keyCode);
                    codeZeroVSpeed = keyCode;
                }
                else
                {
                    controlCodes.Add(LABEL_VSPDZERO, KeyCode.Delete);
                }

                if (nodeControls.HasValue(LABEL_VSPDDEC))
                {
                    keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), nodeControls.GetValue(LABEL_VSPDDEC));
                    controlCodes.Add(LABEL_VSPDDEC, keyCode);
                    codeDecreaseVSpeed = keyCode;
                }
                else
                {
                    controlCodes.Add(LABEL_VSPDDEC, KeyCode.PageDown);
                }

            }

            //Set default values
            else
            {
                controlCodes.Add(LABEL_HOVER, KeyCode.Insert);
                controlCodes.Add(LABEL_VSPDINC, KeyCode.PageUp);
                controlCodes.Add(LABEL_VSPDZERO, KeyCode.Delete);
                controlCodes.Add(LABEL_VSPDDEC, KeyCode.PageDown);
            }
        }

        protected void saveControls()
        {
            ConfigNode nodeControls = new ConfigNode();

            nodeControls.name = "VTOL_CONTROLS";

            nodeControls.AddValue(LABEL_HOVER, controlCodes[LABEL_HOVER]);
            nodeControls.AddValue(LABEL_VSPDINC, controlCodes[LABEL_VSPDINC]);
            nodeControls.AddValue(LABEL_VSPDZERO, controlCodes[LABEL_VSPDZERO]);
            nodeControls.AddValue(LABEL_VSPDDEC, controlCodes[LABEL_VSPDDEC]);

            nodeControls.Save(hoverControlsPath);
        }

        public virtual void SetControlCodes(Dictionary<string, KeyCode> newCodes)
        {
            controlCodes = newCodes;

            foreach (string key in newCodes.Keys)
            {
                switch (key)
                {
                    case LABEL_HOVER:
                        codeToggleHover = newCodes[key];
                        break;

                    case LABEL_VSPDINC:
                        codeIncreaseVSpeed = newCodes[key];
                        break;

                    case LABEL_VSPDZERO:
                        codeZeroVSpeed = newCodes[key];
                        break;

                    case LABEL_VSPDDEC:
                        codeDecreaseVSpeed = newCodes[key];
                        break;
                }
            }

            saveControls();
        }

        public void VesselWasChanged(Vessel vessel)
        {
            FindHoverEngines(vessel);
        }

        public void VesselWasLoaded(Vessel vessel)
        {
            FindHoverEngines(vessel);
        }

        public void FindHoverEngines(Vessel vessel)
        {
            WBIMultiEngineHover hoverEngine;

            hoverEngines.Clear();

            foreach (Part part in vessel.parts)
            {
                hoverEngine = part.FindModuleImplementing<WBIMultiEngineHover>();

                if (hoverEngine != null)
                    hoverEngines.Add(hoverEngine);
            }

            this.vessel = vessel;
        }

        public void DecreaseVSpeed()
        {
            if (hoverActive == false)
                ToggleHover();

            foreach (WBIMultiEngineHover hoverEngine in hoverEngines)
                hoverEngine.DecreaseVSpeed();
        }

        public void IncreaseVSpeed()
        {
            if (hoverActive == false)
                ToggleHover();

            foreach (WBIMultiEngineHover hoverEngine in hoverEngines)
                hoverEngine.IncreaseVSpeed();
        }

        public void KillVSpeed()
        {
            if (hoverActive == false)
                ToggleHover();

            foreach (WBIMultiEngineHover hoverEngine in hoverEngines)
                hoverEngine.KillVSpeed();
        }

        public void ToggleHover()
        {
            hoverActive = !hoverActive;

            if (hoverActive)
                FindHoverEngines(this.vessel);

            foreach (WBIMultiEngineHover hoverEngine in hoverEngines)
                hoverEngine.SetHoverMode(hoverActive);
        }

        public void ShowGUI()
        {
            FindHoverEngines(this.vessel);
            hoverGUI.SetVisible(true);
        }

        public void Update()
        {
            if (Input.GetKeyDown(codeDecreaseVSpeed))
                DecreaseVSpeed();

            if (Input.GetKeyDown(codeIncreaseVSpeed))
                IncreaseVSpeed();

            if (Input.GetKeyDown(codeZeroVSpeed))
                KillVSpeed();

            if (Input.GetKeyDown(codeToggleHover))
                ToggleHover();
        }
    }
}
