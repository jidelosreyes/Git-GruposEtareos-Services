using GruposEtareos.BI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GrupoEtareos.App.Utility
{
    public class Utility
    {
        public static void LogFile(string pMensajeError = "")
        {
            StreamWriter log;

            try
            {
                if (!Directory.Exists(UtilityEntities.LogFile))
                {
                    Directory.CreateDirectory(UtilityEntities.LogFile);
                }

                if (!File.Exists(UtilityEntities.LogFile + "AdminContracionlog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt"))
                {
                    log = new StreamWriter(UtilityEntities.LogFile + "AdminContracionlog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                }
                else
                {
                    log = File.AppendText(UtilityEntities.LogFile + "AdminContracionlog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                }

                // Write to the file:
                log.WriteLine("Data Time:" + DateTime.Now);
                log.WriteLine("NameSpace:" + UtilityEntities.sNameSpace);
                log.WriteLine("Class:" + UtilityEntities.sClass);
                log.WriteLine("Operation:" + UtilityEntities.sOperation);
                log.WriteLine("File:" + UtilityEntities.sFile);
                log.WriteLine("Error Message: " + (pMensajeError != "" ? pMensajeError + ": " : "") + UtilityEntities.exception.Message);
                log.WriteLine("Error Source:" + UtilityEntities.exception.Source);
                log.WriteLine("Error StackTrace:" + UtilityEntities.exception.StackTrace);
                log.WriteLine("Error TargetSite:" + UtilityEntities.exception.TargetSite);
                log.WriteLine("****************************************************************************************************************************************************************");
                log.WriteLine("\n");
                // Close the stream:
                log.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}