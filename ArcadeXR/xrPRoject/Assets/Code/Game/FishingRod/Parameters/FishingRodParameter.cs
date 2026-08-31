using UnityEngine;
using Core.Catching;
using System;


namespace Game.FishingRod.transfer //use this to have include paths like in cpp!, includes all classes!
{
    /// <summary>
    /// Upgrade parameter für Boye UND angel!
    /// abstraktion der extraction. 
    /// 
    /// Value muss immer beim 2 index sein nikolas. rodparams_<name>_<value>
    /// </summary>
    public class FishingRodParameters
    {
        public static FishingRodParameters Instance = null;

        public static void MakeInstance()
        {
            if(Instance == null)
            {
                Instance = new FishingRodParameters();
            }
        }


        public float MiniGameRadius = 4.0f;
        public float MiniGameTime = 1.0f;
        public float PullStrength = 1.0f; //multiplier
        public int depthLevel = 1;

        private string nameMiniGameRadius = "minigameradius";
        private string nameMiniGameTime = "minigametime";
        private string namePullStrength = "pullstrength";
        private string nameDepthLevel = "depthlevel";

        public FishingRodParameters()
        {
            Instance = this;

            //depthLevel = 2; //debug

            Debug.Log("RODPARAMSDEBUG Parameter Log INITIAL " + ParameterLog());
            //DebugExtraction();
        }

        public void IncrementDepth()
        {
            int level = depthLevel + 1;
            UpdateDepthLevel(level);
        }

        private void UpdateDepthLevel(int level)
        {
            depthLevel = level;
        }

        public int DepthLevelAsIndex()
        {
            int index = Math.Max(depthLevel - 1, 0);
            return index;
        }


        //NOT TESTED!
        public void ParseEventString(string inString)
        {
            if (RelatedToRodParams(inString))
            {
                //handle decompose and upgrades
                //Decompose the string using the underscore character
                string[] splitArray = inString.Split('_');

                string constrcuted = "RODPARAMSDEBUG ParseEventString reconstructed: ";
                foreach (string s in splitArray)
                {
                    constrcuted += s;
                    constrcuted += " , ";
                }
                Debug.Log(constrcuted);



                ExtractValueFloat(inString, nameMiniGameRadius, splitArray, out MiniGameRadius, MiniGameRadius);
                ExtractValueFloat(inString, nameMiniGameTime, splitArray, out MiniGameTime, MiniGameTime);
                ExtractValueFloat(inString, namePullStrength, splitArray, out PullStrength, PullStrength);
                ExtractValueInt(inString, nameDepthLevel, splitArray, out depthLevel, depthLevel);
                
                //min 1 bei depth level
                if(depthLevel < 1)
                {
                    depthLevel = 1;
                }
            }
        }

        private bool RelatedToRodParams(string s)
        {
            return Contains(s, "rodparams");
            //RodParams_depth_2
        }

        private bool Contains(string s, string other)
        {
            return s.ToLower().Contains(other.ToLower());
        }


        /*private void ExtractMiniGameRadius(string inString, string[] splitArray)
        {
            int asInt = 0;
            if (ExtractInt(inString, nameMiniGameRadius, splitArray, 2, out asInt))
            {
                //copy extracted value.
                MiniGameRadius = asInt;
            }
        }*/

        private void ExtractValueFloat(
            string inString, string target, string[] splitArray, out float value,
            float defaultValue
        )
        {
            int asInt = 0;
            if (ExtractInt(inString, target, splitArray, 2, out asInt))
            {
                //copy extracted value.
                value = asInt;
                Debug.Log("RODPARAMSDEBUG EXTRACTED INT AS FLOAT " + asInt + " or " + value);
                return;
            }
            value = defaultValue;

        }

        private void ExtractValueInt(
            string inString, string target, string[] splitArray, out int value,
            int defaultValue
        )
        {
            int asInt = 0;
            if (ExtractInt(inString, target, splitArray, 2, out asInt))
            {
                //copy extracted value.
                value = asInt;
                return;
            }
            value = defaultValue;
        }

        //abstract extraction from event
        //event structure: rodParams _ radius _ <value> //index 2

        private bool ExtractInt(
            string inString,
            string targetedValue,
            string[] splitArray,
            int indexInArray,
            out int value
        )
        {
            if (inString != null && targetedValue != null && splitArray != null)
            {
                if (Contains(inString, targetedValue))
                {
                    if (indexInArray >= 0 && indexInArray < splitArray.Length)
                    {
                        string valueextracted = splitArray[indexInArray]; //rodParams _ radius _ <value> //index 2
                        if (ToInt(valueextracted, out value))
                        {
                            //nothing.
                            Debug.Log("RODPARAMSDEBUG EXTRACTED INT " + value);
                            return true;
                        }
                    }
                }
            }
            value = -1;
            return false;
        }

        private bool ToInt(string stringIn, out int value)
        {
            if (int.TryParse(stringIn, out value))
            {
                Debug.Log("RODPARAMSDEBUG Parameter parse int OK: " + stringIn);
                return true;
            }
            Debug.Log("RODPARAMSDEBUG Parameter parse int failed: " + stringIn);
            return false;
        }

        //to be implemented! 
        void DebugExtraction()
        {
            //debug here.

            Debug.Log("RODPARAMSDEBUG Parameter Log Before " + ParameterLog());


            //rodparams_<name>_<value>
            //rodparams_minigameradius_<value>
            //rodparams_minigametime_<value>
            //rodparams_pullstrength_<value>
            //rodparams_nameDepthLevel_<value>
            ParseEventString("rodparams_minigameradius_5");
            ParseEventString("rodparams_minigametime_2");
            ParseEventString("rodparams_pullstrength_2");
            ParseEventString("rodparams_depthlevel_2");
            

            Debug.Log("RODPARAMSDEBUG Parameter Log After " + ParameterLog());
        }

        public string ParameterLog()
        {
            string s =
            "Fishing Rod Params: " +
            nameMiniGameRadius + MiniGameRadius + " , " +
            nameMiniGameTime + MiniGameTime + " , " +
            namePullStrength + PullStrength + " , " +
            nameDepthLevel + depthLevel;
            return s;
        }


    }
}
