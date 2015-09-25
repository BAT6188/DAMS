using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Config;
using NLog.Targets;
using NLog;

namespace DAMS
{
    public class LogConfig
    {
        //log初始化
        public static void initLogger(string policeNo)
        {
            //logger = LogManager.GetCurrentClassLogger();

            // Step 1. Create configuration object 
            LoggingConfiguration logConfig = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            //RichTextBoxTarget rtbTarget = new RichTextBoxTarget();
            //logConfig.AddTarget("richTextBox", rtbTarget);
            //rtbTarget.FormName = "frmMainWindow"; // your winform class name
            //rtbTarget.ControlName = "rtbLog"; // your RichTextBox control/variable name

            FileTarget fileTarget = new FileTarget();
            logConfig.AddTarget("logFile", fileTarget);

            // Step 3. Set target properties
            string commonLayout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${logger} ${message}";
            //rtbTarget.Layout = commonLayout;

            //string curDatetimeStr = DateTime.Now.ToString();
            DateTime curDateTime = DateTime.Now;
            string curDatetimeStr = String.Format("{0:yyyy-MM-dd}", curDateTime); //"2013-06-11"
            fileTarget.FileName = "D:/Log/" + policeNo+"_" + curDatetimeStr + "_log.txt"; //{D:Log/2013-06-11_142102_log.txt'}
            fileTarget.Layout = commonLayout;

            // Step 4. Define rules
            //LoggingRule ruleRichTextBox = new LoggingRule("*", LogLevel.Debug, rtbTarget);
            //logConfig.LoggingRules.Add(ruleRichTextBox);

            LoggingRule ruleFile = new LoggingRule("*", LogLevel.Debug, fileTarget);
            logConfig.LoggingRules.Add(ruleFile);

            // Step 5. Activate the configuration
            LogManager.Configuration = logConfig;

            // Example usage
            //Logger logger = LogManager.GetLogger("Amazon");
           
        }

        //添加Log
        public static void info(string policeNo,string log) 
        {
            initLogger(policeNo);
            Logger logger = LogManager.GetLogger("");
            logger.Info(policeNo+" "+log);
        }

        public static void error(string policeNo, string log)
        {
            initLogger(policeNo);
            Logger logger = LogManager.GetLogger("");
            logger.Error(policeNo + " " + log);
        }


    }
}
