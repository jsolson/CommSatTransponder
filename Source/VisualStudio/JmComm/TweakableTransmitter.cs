using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CommNet;

namespace JmComm
{
    public class ModuleJmTweakableTransmitter : ModuleDataTransmitter
    {
        private double baseAntennaPower = 0.0d;
        private double basePacketResourceCost = 0.0;
        private float basePacketSize = 0.0f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Data Rate Percent"), UI_FloatRange(minValue = 5f, maxValue = 100f, stepIncrement = 5f)]
        public float dataRatePercent = 100f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Tx. Power Percent"), UI_FloatRange(minValue = 25f, maxValue = 400f, stepIncrement = 5f)]
        public float powerPercent = 100f;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "EC/Mit")]
        public string ecPerMitText = string.Empty;
        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "Mits/Sec")]
        public string bandwidthText = string.Empty;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            basePacketResourceCost = packetResourceCost;
            basePacketSize = packetSize;
            baseAntennaPower = antennaPower;

            Fields[nameof(dataRatePercent)].uiControlEditor.onFieldChanged = OnTweakableChanged;
            Fields[nameof(dataRatePercent)].uiControlFlight.onFieldChanged = OnTweakableChanged;
            Fields[nameof(powerPercent)].uiControlEditor.onFieldChanged = OnTweakableChanged;
            Fields[nameof(powerPercent)].uiControlFlight.onFieldChanged = OnTweakableChanged;

            UpdateTweakable();
        }

        private void OnTweakableChanged(BaseField field, object what)
        {
            UpdateTweakable();
        }
        
        private void UpdateTweakable()
        {
            packetResourceCost = basePacketResourceCost * (powerPercent / 100);
            antennaPower = baseAntennaPower * Math.Sqrt(powerPercent / 100);

            packetSize = basePacketSize * (dataRatePercent / 100);
            antennaPower = antennaPower / (dataRatePercent / 100);

            powerText = FormatPowerText(antennaPower) + " (" + (antennaPower / baseAntennaPower).ToString("P0") + ")";
            if (antennaCombinable)
                powerText = powerText + " (Combinable)";

            ecPerMitText = (packetResourceCost / packetSize).ToString("N1");
            bandwidthText = (1 / packetInterval * packetSize).ToString("N1");

            if (HighLogic.LoadedSceneIsFlight && HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet)
                CommNetNetwork.Instance.CommNet.Rebuild();
        }

        private string FormatPowerText(double power)
        {
            string returnValue = string.Empty;
            if (power >= 1000000000)
            {
                power = power / 1000000000;
                returnValue = power.ToString("N0") + "G";
            }
            else if (power >= 1000000)
            {
                power = power / 1000000;
                returnValue = power.ToString("N2") + "M";
            }
            else if (power >= 1000)
            {
                power = power / 1000;
                returnValue = power.ToString("N3") + "K";
            }
            else
            {
                returnValue = power.ToString("N0");
            }
            return returnValue;
        }
    }
}
