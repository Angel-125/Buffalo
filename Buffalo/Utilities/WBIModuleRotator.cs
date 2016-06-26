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
    public enum ERotationStates
    {
        Neutral,
        RotatingToNeutral,
        MinRotation,
        RotatingToMin,
        MaxRotation,
        RotatingToMax
    }

    [KSPModule("Rotator")]
    public class WBIModuleRotator : PartModule
    {
        [KSPField]
        public string groupTag;

        [KSPField]
        public string rotationTransformName;

        [KSPField]
        public float rotateAxisX;

        [KSPField]
        public float rotateAxisY;

        [KSPField]
        public float rotateAxisZ;

        [KSPField]
        public float minRotateAngle;

        [KSPField]
        public float maxRotateAngle;

        [KSPField]
        public float rotationDegPerSec;

        [KSPField(isPersistant = true)]
        public bool invertRotation;

        [KSPField(isPersistant = true, guiActive = true, guiName = "State")]
        public string currentRotationState;

        [KSPField(isPersistant = true, guiActive = true, guiFormat = "f2")]
        public float currentRotationAngle;

        Transform rotationTarget = null;
        Vector3 rotationAxis;
        ERotationStates rotationState;
        float degPerUpdate;

        [KSPAction("Rotate To Minimum")]
        public void ActionRotateToMin(KSPActionParam param)
        {
            RotateToMin();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Rotate To Minimum")]
        public void RotateToMin()
        {
            rotationState = ERotationStates.RotatingToMin;
            currentRotationState = rotationState.ToString();
        }

        [KSPAction("Rotate To Maximum")]
        public void ActionRotateToMax(KSPActionParam param)
        {
            RotateToMax();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Rotate To Maximum")]
        public void RotateToMax()
        {
            rotationState = ERotationStates.RotatingToMax;
            currentRotationState = rotationState.ToString();
        }

        [KSPAction("Rotate To Neutral")]
        public void ActionRotateToNeutral(KSPActionParam param)
        {
            RotateToNeutral();
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Rotate To Neutral")]
        public void RotateToNeutral()
        {
            rotationState = ERotationStates.RotatingToNeutral;
            currentRotationState = rotationState.ToString();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (string.IsNullOrEmpty(rotationTransformName))
            {
                Debug.Log("No rotation transform!");
                return;
            }

            //Get the rotation target
            rotationTarget = part.FindModelTransform(rotationTransformName);

            //Get the rotation axis
            rotationAxis = new Vector3(rotateAxisX, rotateAxisY, rotateAxisZ);

            //Get the rotation state
            if (string.IsNullOrEmpty(currentRotationState) == false)
                rotationState = (ERotationStates)Enum.Parse(typeof(ERotationStates), currentRotationState);
            else
                rotationState = ERotationStates.Neutral;

            //Calculate degrees per update
            degPerUpdate = rotationDegPerSec * TimeWarp.fixedDeltaTime;
        }

        public void FixedUpdate()
        {
            if (rotationState == ERotationStates.Neutral)
                return;

            switch (rotationState)
            {
                case ERotationStates.RotatingToNeutral:
                    if (currentRotationAngle < 0)
                        currentRotationAngle += degPerUpdate;
                    else if (currentRotationAngle > 0)
                        currentRotationAngle -= degPerUpdate;

                    if (Math.Abs(currentRotationAngle) <= degPerUpdate)
                    {
                        currentRotationAngle = 0.0f;
                        rotationState = ERotationStates.Neutral;
                        currentRotationState = rotationState.ToString();
                    }
                    break;

                case ERotationStates.RotatingToMin:
                    currentRotationAngle -= degPerUpdate;
                    if (currentRotationAngle < minRotateAngle)
                    {
                        currentRotationAngle = minRotateAngle;
                        rotationState = ERotationStates.MinRotation;
                        currentRotationState = rotationState.ToString();
                    }
                    break;

                case ERotationStates.RotatingToMax:
                    currentRotationAngle += degPerUpdate;
                    if (currentRotationAngle > maxRotateAngle)
                    {
                        currentRotationAngle = maxRotateAngle;
                        rotationState = ERotationStates.MaxRotation;
                        currentRotationState = rotationState.ToString();
                    }
                    break;
            }

            rotationTarget.transform.localEulerAngles = new Vector3(currentRotationAngle, 0, 0);
        }
    }
}
