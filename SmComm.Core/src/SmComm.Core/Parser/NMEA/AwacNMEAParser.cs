using System;
using System.Collections.Generic;
using SmSimple.Core;
using SmSimple.Core.Util;

namespace SmComm.Core.Parser.NMEA
{
    public class AwacNMEAParser
    {

        private static Dictionary<string, List<string>> nameDictionary = new Dictionary<string, List<string>>();

        public enum VarNameInfoMessage
        {
            AWAC_INFO_INSTRUMENT,
            AWAC_INFO_HEADID,
            AWAC_INFO_NUMBEAMS,
            AWAC_INFO_NUMCELLS,
            AWAC_INFO_BLANKING,
            AWAC_INFO_CELLSIZE,
            AWAC_INFO_COORD
        };

        public enum VarNameSensorMessage
        {
            AWAC_SENSOR_DATE,
            AWAC_SENSOR_TIME,
            AWAC_SENSOR_ERROR,
            AWAC_SENSOR_STATUS,
            AWAC_SENSOR_BATTERY,
            AWAC_SENSOR_SOUNDSPEED,
            AWAC_SENSOR_HEADING,
            AWAC_SENSOR_PITCH,
            AWAC_SENSOR_ROLL,
            AWAC_SENSOR_PRESSURE,
            AWAC_SENSOR_TEMPERATURE,
            AWAC_SENSOR_ANALOG1,
            AWAC_SENSOR_ANALOG2
        };

        public enum VarCurrentVelocityMessage
        {
            AWAC_CURR_VELOCITY_DATE,
            AWAC_CURR_VELOCITY_TIME,
            AWAC_CURR_VELOCITY_CELL,
            AWAC_CURR_VELOCITY_1,
            AWAC_CURR_VELOCITY_2,
            AWAC_CURR_VELOCITY_3,
            AWAC_CURR_VELOCITY_SPEED,
            AWAC_CURR_VELOCITY_DIRECTION,
            AWAC_CURR_VELOCITY_AMPLITUDE_UNIT,
            AWAC_CURR_VELOCITY_AMPLITUDE_1,
            AWAC_CURR_VELOCITY_AMPLITUDE_2,
            AWAC_CURR_VELOCITY_AMPLITUDE_3,
            AWAC_CURR_VELOCITY_CORR_1,
            AWAC_CURR_VELOCITY_CORR_2,
            AWAC_CURR_VELOCITY_CORR_3,
        };

        public enum VarWaveMessage
        {
            AWAC_WAVE_DATE,
            AWAC_WAVE_TIME,
            AWAC_WAVE_SPECTRUM_BASIS,
            AWAC_WAVE_PROCESSING_METHOD,
            AWAC_WAVE_HM0,
            AWAC_WAVE_H3,
            AWAC_WAVE_H10,
            AWAC_WAVE_HMAX,
            AWAC_WAVE_TM02,
            AWAC_WAVE_TP,
            AWAC_WAVE_TZ,
            AWAC_WAVE_DIR_TP,
            AWAC_WAVE_SPR_TP,
            AWAC_WAVE_MAIN_DIR,
            AWAC_WAVE_UNIDINDEX,
            AWAC_WAVE_MEAN_PRESSURE,
            AWAC_WAVE_NUM_NO_DETECTS,
            AWAC_WAVE_NUM_BAD_DETECTS,
            AWAC_WAVE_NEAR_SURFACE_SPEED,
            AWAC_WAVE_NEAR_SURFACE_DIR,
            AWAC_WAVE_ERROR_CODE,
        };

        internal bool RoundSecondsToClosestFullMinute { get; set; }


        public static Dictionary<string, List<string>> VariableDescriptions = new Dictionary<string, List<string>>() {

            {VarNameInfoMessage.AWAC_INFO_INSTRUMENT.ToString(),  new List<string>() {"Sensor type", string.Empty }},
            {VarNameInfoMessage.AWAC_INFO_HEADID.ToString(), new List<string>() { "Head ID", string.Empty }},
            {VarNameInfoMessage.AWAC_INFO_NUMBEAMS.ToString(),  new List<string>() {"Number of beams", string.Empty }},
            {VarNameInfoMessage.AWAC_INFO_NUMCELLS.ToString(),  new List<string>() {"Number of cells", string.Empty }},
            {VarNameInfoMessage.AWAC_INFO_BLANKING.ToString(),  new List<string>() {"Blanking (m)", "m" }},
            {VarNameInfoMessage.AWAC_INFO_CELLSIZE.ToString(),  new List<string>() {"Cell size (m)",  "m" }},
            {VarNameInfoMessage.AWAC_INFO_COORD.ToString(),  new List<string>() {"Coordinate system", string.Empty }},

            {VarNameSensorMessage.AWAC_SENSOR_DATE.ToString(), new List<string>() { "Sensor message date", string.Empty }},
            {VarNameSensorMessage.AWAC_SENSOR_TIME.ToString(),  new List<string>() {"Sensor message time", string.Empty }},
            {VarNameSensorMessage.AWAC_SENSOR_ERROR.ToString(), new List<string>() { "Error code (hex)", string.Empty }},
            {VarNameSensorMessage.AWAC_SENSOR_STATUS.ToString(),  new List<string>() {"Status code (hex)", string.Empty }},
            {VarNameSensorMessage.AWAC_SENSOR_BATTERY.ToString(),  new List<string>() {"Battery voltage (V)", "volt" }},
            {VarNameSensorMessage.AWAC_SENSOR_SOUNDSPEED.ToString(), new List<string>() {"Sound speed (m/s)", "metersPerSec" }},
            {VarNameSensorMessage.AWAC_SENSOR_HEADING.ToString(), new List<string>() { "Heading (deg)", "arcDeg" }},
            {VarNameSensorMessage.AWAC_SENSOR_PITCH.ToString(), new List<string>() { "Pitch (deg)",  "arcDeg" }},
            {VarNameSensorMessage.AWAC_SENSOR_ROLL.ToString(), new List<string>() { "Roll (deg)",  "arcDeg" }},
            {VarNameSensorMessage.AWAC_SENSOR_PRESSURE.ToString(), new List<string>() { "Pressure (dbar)", string.Empty }},
            {VarNameSensorMessage.AWAC_SENSOR_TEMPERATURE.ToString(), new List<string>() { "Temperature (deg C)", "Celsius" }},
            {VarNameSensorMessage.AWAC_SENSOR_ANALOG1.ToString(), new List<string>() { "Analog input #1 (counts)", string.Empty }},
            {VarNameSensorMessage.AWAC_SENSOR_ANALOG2.ToString(),  new List<string>() {"Analog input #2 (counts)", string.Empty }},

            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_DATE.ToString(), new List<string>() { "Current velocity message date", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_TIME.ToString(),  new List<string>() {"Current velocity message time", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_CELL.ToString(), new List<string>() { "Current velocity message cell number", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_1.ToString(), new List<string>() { "Current velocity 1", "metersPerSec" }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_2.ToString(), new List<string>() { "Current velocity 2",  "metersPerSec"  }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_3.ToString(),  new List<string>() {"Current velocity 3",  "metersPerSec" }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_SPEED.ToString(),  new List<string>() {"Current speed - cell", "metersPerSec" }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_DIRECTION.ToString(),  new List<string>() {"Current direction - cell", "arcDeg" }},

            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_AMPLITUDE_UNIT.ToString(), new List<string>() { "Current velocity amplitude units", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_AMPLITUDE_1.ToString(), new List<string>() { "Current velocity amplitude 1", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_AMPLITUDE_2.ToString(), new List<string>() { "Current velocity amplitude 2", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_AMPLITUDE_3.ToString(), new List<string>() { "Current velocity amplitude 3", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_CORR_1.ToString(), new List<string>() { "Current velocity correlation 1", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_CORR_2.ToString(), new List<string>() { "Current velocity correlation 2", string.Empty }},
            {VarCurrentVelocityMessage.AWAC_CURR_VELOCITY_CORR_3.ToString(),  new List<string>() {"Current velocity correlation 3", string.Empty }},

             {VarWaveMessage.AWAC_WAVE_DATE.ToString(), new List<string>() { "Wave message date", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_TIME.ToString(), new List<string>() { "Wave message date", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_SPECTRUM_BASIS.ToString(), new List<string>() { "Spectrum basis type", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_PROCESSING_METHOD.ToString(), new List<string>() { "Processing method", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_HM0.ToString(),  new List<string>() {"Hm0 - Spectral significant wave height [m]",  "m" }},
             {VarWaveMessage.AWAC_WAVE_H3.ToString(), new List<string>() { "H3 [m] - AST significant wave height (mean of largest 1/3)",  "m"}},
             {VarWaveMessage.AWAC_WAVE_H10.ToString(), new List<string>() { "H10 [m] - AST wave height(mean of largest 1/10)",  "m" }},
             {VarWaveMessage.AWAC_WAVE_HMAX.ToString(), new List<string>() { "Hmax [m] - AST max wave height in wave ensemble",  "m" }},
             {VarWaveMessage.AWAC_WAVE_TM02.ToString(), new List<string>() { "Tm02 - Mean period spectrum based [sec]", "s" }},
             {VarWaveMessage.AWAC_WAVE_TP.ToString(),  new List<string>() {"Tp [s] - Peak period",  "s"  }},
             {VarWaveMessage.AWAC_WAVE_TZ.ToString(),  new List<string>() {"Tz [s] - AST mean zero-crossing period",  "s"  }},
             {VarWaveMessage.AWAC_WAVE_DIR_TP.ToString(), new List<string>() { "DirTp [deg] - Direction at Tp",  "arcDeg" }},
             {VarWaveMessage.AWAC_WAVE_SPR_TP.ToString(),  new List<string>() {"SprTp [deg] - Spreading at Tp",  "arcDeg" }},
             {VarWaveMessage.AWAC_WAVE_MAIN_DIR.ToString(), new List<string>() { "Mean wave direction",  "arcDeg" }},
             {VarWaveMessage.AWAC_WAVE_UNIDINDEX.ToString(), new List<string>() { "Unidirectivity Index [1/65535]", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_MEAN_PRESSURE.ToString(), new List<string>() { "Mean pressure (dbar)", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_NUM_NO_DETECTS.ToString(), new List<string>() { "Number of no detects", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_NUM_BAD_DETECTS.ToString(),  new List<string>() {"Number of bad detects", string.Empty }},
             {VarWaveMessage.AWAC_WAVE_NEAR_SURFACE_SPEED.ToString(), new List<string>() { "Near surface current speed",  "metersPerSec" }},
             {VarWaveMessage.AWAC_WAVE_NEAR_SURFACE_DIR.ToString(), new List<string>() { "Near surface current direction", "arcDeg"}},
             {VarWaveMessage.AWAC_WAVE_ERROR_CODE.ToString(),  new List<string>() {"Wave message error code", string.Empty }},
        };

        // 
        public List<string> CellSpeedVariables { get; set; }
        public List<string> CellDirectionVariables { get; set; }
        public List<string> CellDepthVariables { get; set; }


        public MeasMsg Parse(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return null;

                //   Debug.WriteLine("Parsing AWAC message:" + message);

                int dollarIndex = message.IndexOf("$");
                if (dollarIndex == -1)
                    return null;

                int crLfIndex = message.IndexOf("/r/n");
                if (crLfIndex == -1)
                    message = message + Environment.NewLine;

                if (nameDictionary.Count == 0)
                    InitDictionary();

                int index = message.IndexOf("$PN");
                if (index == -1)
                    return null;
                if (index > 0)
                    message = message.Substring(index);



                NMEAProprietarySentence nmeaSentence = (NMEAParser.Parse(message)) as NMEAProprietarySentence;
                return ParseMsg(nmeaSentence);

            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "Error while parsing NMEA message." + message);
                return null;
            }
        }

        private MeasMsg ParseMsg(NMEAProprietarySentence nmeaSentence)
        {
            MeasMsg result = new MeasMsg();
            result.Time = DateTimeEx.Now;
            var s = nmeaSentence.SentenceIDString.ToUpperInvariant();

            if (!nameDictionary.ContainsKey(s))
                return null;

            if (s == "C")
                return ParseCellVelocityData(nmeaSentence, result);


            int len = nmeaSentence.parameters.Length;
            for (int i = 0; i < len; i++)
            {
                string dataValue = string.Empty;
                if (null != nmeaSentence.parameters[i])
                    dataValue = nmeaSentence.parameters[i].ToString();

                result.AddMeas(nameDictionary[s][i], dataValue);
            }

            if (s == "I")
                return AddCellDepths(nmeaSentence, result);

            return result;
        }


        private MeasMsg ParseCellVelocityData(NMEAProprietarySentence nmeaSentence, MeasMsg measMsg)
        {
            const int cellIdIndex = 2;
            const int speedIndex = 6;
            const int dirIndex = 7;
            int cell;
            string cellId = nmeaSentence.parameters[cellIdIndex].ToString();

            if (!int.TryParse(cellId, out cell))
            {
                SimpleFileWriter.WriteLineToEventFile(DirectoryName.EventLog, StringManager.GetString("Unable to parse Nortec AWAC NMEA message cell id. Received cellId was:") + cellId);
                return measMsg;
            }

            int index = cell - 1;
            // Ignore any message from cells which do not have variables defined
            if (-1 < index && index < CellSpeedVariables.Count)
            {
                var dataValue = nmeaSentence.parameters[speedIndex].ToString();
                measMsg.AddMeas(CellSpeedVariables[index], dataValue);
            }

            if (-1 < index && index < CellDirectionVariables.Count)
            {
                var dataValue = nmeaSentence.parameters[dirIndex].ToString();
                measMsg.AddMeas(CellDirectionVariables[index], dataValue);
            }

            if (measMsg.Count == 0)
                return null;


            return measMsg;
        }

        private MeasMsg AddCellDepths(NMEAProprietarySentence nmeaSentence, MeasMsg measMsg)
        {

            bool cannotCompute = false;
            const int blankingIndex = 4;
            const int cellSizeIndex = 5;
            double blanking = 0;
            double cellSize = 0;
            if (!measMsg.GetNumericDoubleObsValueByName(nameDictionary["I"][blankingIndex], out blanking))
                cannotCompute = true;
            else if (!measMsg.GetNumericDoubleObsValueByName(nameDictionary["I"][cellSizeIndex], out cellSize))
                cannotCompute = true;

            for (int i = 0; i < CellDepthVariables.Count; i++)
            {
                string dataValue = "///";
                if (!cannotCompute)
                {
                    double depth = blanking + (i + 1) * cellSize;
                    dataValue = depth.ToString();
                }
                measMsg.AddMeas(CellDepthVariables[i], dataValue.ToString());
            }

            return measMsg;
        }

        private void InitDictionary()
        {
            AddVariables<VarNameInfoMessage>("I");
            AddVariables<VarNameSensorMessage>("S");
            AddVariables<VarCurrentVelocityMessage>("C");
            AddVariables<VarWaveMessage>("W");
        }

        private void AddVariables<T>(string id)
        {
            List<string> list = new List<string>();
            foreach (T eType in Enum.GetValues(typeof(T)))
            {
                list.Add(eType.ToString());
            }

            nameDictionary.Add(id, list);
        }
    }
}
