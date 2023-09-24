using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using ImzaApiTest.src.tr.gov.tubitak.uekae.esya.api;
using log4net;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.infra.tsclient;

namespace tr.gov.tubitak.uekae.esya.api.cades.example
{
    /**
     * Provides required variables and functions for ASiC examples
     */

    public class CadesSampleBase : SampleBase
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string testDataFolder;
        private static readonly string policyFile;
        private static readonly TSSettings tsSettings;
        private static ValidationPolicy policy;

        static CadesSampleBase()
        {
            try
            {
                testDataFolder = getRootDir() + @"\testVerileri\";
                policyFile = getRootDir() + @"\config\certval-policy-test.xml";
                tsSettings = new TSSettings("http://tzd.kamusm.gov.tr", 2, "12345678", DigestAlg.SHA256);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ValidationPolicy getPolicy()
        {
            if (policy == null)
                try
                {
                    policy = PolicyReader.readValidationPolicy(new FileStream(policyFile, FileMode.Open));
                }
                catch (FileNotFoundException e)
                {
                    throw new SystemException("Policy file could not be found", e);
                }
            return policy;
        }

        public static string getTestDataFolder()
        {
            return testDataFolder;
        }

        public static TSSettings getTSSettings()
        {
            //for getting test TimeStamp or qualified TimeStamp account, mail to bilgi@kamusm.gov.tr.
            //This configuration, user ID (2) and password (PASSWORD), is invalid. 
            return tsSettings;
        }
    }
}