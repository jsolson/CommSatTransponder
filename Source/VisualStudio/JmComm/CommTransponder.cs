using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommNet;
using UnityEngine;

namespace JmComm
{
    public class ModuleJmCommTransponder : PartModule, ICommAntenna
    {
        private List<ICommAntenna> vesselTransmitters;
        private DoubleCurve rangeCurve;

        [KSPField(isPersistant = true)]
        public double antennaPower = 0.0;

        [KSPField()]
        public double maxAntennaPower = 1000000.0;

        private double vesselMaxPower = 0.0;

        [KSPField(guiActive = true, isPersistant = false, guiName = "Antennas")]
        public string antennaCountText = string.Empty;
        [KSPField(guiActive = true, isPersistant = false, guiName = "Active Antennas")]
        public string activeAntennaCountText = string.Empty;
        [KSPField(guiActive = true, isPersistant = false, guiName = "Prime Antenna")]
        public string primeAntennaText = string.Empty;
        [KSPField(guiActive = true, isPersistant = false, guiName = "Power")]
        public string powerText = "0";

        public override void OnAwake()
        {
            base.OnAwake();
            
            rangeCurve = new DoubleCurve();
            rangeCurve.Add(0, 0, 0, 0);
            rangeCurve.Add(1, 1, 0, 0);

            vesselTransmitters = new List<ICommAntenna>();
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            GameEvents.onVesselWasModified.Add(OnVesselWasModified);

            if (HighLogic.LoadedSceneIsFlight)
            {
                UpdateTransmitters();
            }
        }

        private void OnDestroy()
        {
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
        }

        public override void OnSave(ConfigNode node)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                UpdatePower();
            }

            base.OnSave(node);
        }

        private void OnVesselWasModified(Vessel v)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                UpdateTransmitters();
            }
        }

        public override string GetInfo()
        {
            return "Max Rating: " + FormatPowerText(maxAntennaPower) + " (Combinable)";
        }

        public void UpdateTransmitters()
        {
            antennaPower = 0.0;
            vesselMaxPower = 0.0;
            int antennaCount = 0;

            List<ICommAntenna> vt = vessel.FindPartModulesImplementing<ICommAntenna>();

            vesselTransmitters.Clear();
            foreach (ICommAntenna t in vt)
            {
                if (t is ModuleJmCommTransponder)
                {
                    vesselMaxPower += ((ModuleJmCommTransponder)t).maxAntennaPower;
                }
                else
                {
                    vesselTransmitters.Add(t);
                    antennaCount += 1;
                }
            }
            antennaCountText = antennaCount.ToString();
        }

        public void UpdatePower()
        {
            double combWeightedExponent = 0.0;
            double combPowerSum = 0.0;
            double combStrongestPower = 0.0;
            double combPower = 0.0;
            int combCount = 0;
            int antennaCount = 0;
            double strongestPower = 0.0;

            foreach (ICommAntenna t in vesselTransmitters)
            {
                if (t.CanComm())
                {
                    if (t.CommPower > strongestPower)
                    {
                        strongestPower = t.CommPower;
                        if (t is PartModule)
                        {
                            primeAntennaText = ((PartModule)t).part.partInfo.title;
                        } else
                        {
                            primeAntennaText = "Unknown"; // shouldn't happen?
                        }
                    }
                    
                    if (t.CommCombinable)
                    {
                        if (t.CommPower > combStrongestPower)
                        {
                            combStrongestPower = t.CommPower;
                        }
                        combWeightedExponent += t.CommPower * t.CommCombinableExponent;
                        combPowerSum += t.CommPower;
                        combCount += 1;
                    }

                    antennaCount += 1;
                }
            }

            if (combPowerSum > 0)
            {
                combPower = combStrongestPower * Math.Pow(combPowerSum / combStrongestPower, combWeightedExponent / combPowerSum);
                if (combPower > strongestPower)
                {
                    strongestPower = combPower;
                    primeAntennaText = "Combined (" + combCount.ToString() + ")";
                }
            }

            SetPower(strongestPower);
            activeAntennaCountText = antennaCount.ToString();
        }

        public void SetPower(double power)
        {
            if (power > vesselMaxPower)
            {
                power = vesselMaxPower;
            }

            if (antennaPower != power)
            {
                antennaPower = power;
                powerText = FormatPowerText(power);
            }
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

        //
        // ICommAntenna interface
        //
        bool ICommAntenna.CommCombinable
        {
            get
            {
                return false;
            }
        }

        double ICommAntenna.CommCombinableExponent
        {
            get
            {
                return 0.0;
            }
        }

        double ICommAntenna.CommPower
        {
            get
            {
                UpdatePower();
                return antennaPower;
            }
        }

        AntennaType ICommAntenna.CommType
        {
            get
            {
                return AntennaType.RELAY;
            }
        }

        DoubleCurve ICommAntenna.CommRangeCurve
        {
            get
            {
                return rangeCurve;
            }
        }

        DoubleCurve ICommAntenna.CommScienceCurve
        {
            get
            {
                return rangeCurve;
            }
        }

        bool ICommAntenna.CanComm()
        {
            return true;
        }

        bool ICommAntenna.CanCommUnloaded(ProtoPartModuleSnapshot mSnap)
        {
            return true;
        }

        bool ICommAntenna.CanScienceTo(bool combined, double bPower, double sqrDistance)
        {
            return false;
        }

        double ICommAntenna.CommPowerUnloaded(ProtoPartModuleSnapshot mSnap)
        {
            double returnValue = 0.0;
            if (mSnap != null && mSnap.moduleValues.HasValue("antennaPower"))
            {
                double.TryParse(mSnap.moduleValues.GetValue("antennaPower"), out returnValue);
            }
            return returnValue;
        }
    }
}
