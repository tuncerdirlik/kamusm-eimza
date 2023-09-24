using System;
using System.IO;
using System.Reflection;
using ImzaApiTest.src.tr.gov.tubitak.uekae.esya.api;
using log4net;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cmssignature.example.util;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.api.signature;
using tr.gov.tubitak.uekae.esya.api.signature.config;
using tr.gov.tubitak.uekae.esya.api.signature.sigpackage;

namespace tr.gov.tubitak.uekae.esya.api.asic.example
{
    /**
     * Provides required variables and functions for ASiC examples
     */

    public class AsicSampleBase : SampleBase
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string baseDir; // base directory where signatures created
        private static readonly FileInfo file; // file to be signed
        private static readonly ECertificate certificate; // certificate
        private static readonly BaseSigner signer; // signer

        static AsicSampleBase()
        {
            try
            {
                baseDir = getRootDir() + "/testVerileri/";
                file = new FileInfo(baseDir + "sample.txt");
                certificate = SmartCardManager.getInstance().getSignatureCertificate(isQualified());
                signer = SmartCardManager.getInstance().getSigner(getPin(), certificate);
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        /**
         * Creates an appropriate file name for ASiC signatures
         *
         * @param packageType package type of the signature, ASiC_S or ASiC_E
         * @param format      format of the signature, CAdES or XAdES
         * @param type        type of the signature, BES etc.
         * @return file name of associated signature as string
         */

        protected string fileName(PackageType packageType, SignatureFormat format, SignatureType type)
        {
            string fileName = baseDir + packageType + "-" + format + "-" + type;
            switch (packageType)
            {
                case PackageType.ASiC_S:
                    return fileName + ".asics";
                case PackageType.ASiC_E:
                    return fileName + ".asice";
            }
            return null;
        }

        /**
         * Reads an ASiC signature
         *
         * @param packageType type of the ASiC signature to be read, ASiC_S or ASiC_E
         * @param format      format of the ASiC signature to be read, CAdES or XAdES
         * @param type        type of the ASiC signature to be read, BES etc.
         * @return signature package of ASiC signature
         * @throws Exception
         */

        protected SignaturePackage read(PackageType packageType, SignatureFormat format, SignatureType type)
        {
            Context c = createContext();
            FileInfo f = new FileInfo(fileName(packageType, format, type));
            return SignaturePackageFactory.readPackage(c, f);
        }

        /**
         * Creates context for signature creation and validation
         *
         * @return created context
         */

        protected Context createContext()
        {
            Uri uri = new Uri(baseDir);
            Context c = new Context(new Uri(uri.AbsoluteUri));
            c.setConfig(new Config(getRootDir() + "/config/esya-signature-config.xml"));
            //c.setData(getContent()); //for detached CAdES signatures validation
            return c;
        }

        /**
         * Gets certificate from smartcard
         *
         * @return the certificate
         */

        protected ECertificate getCertificate()
        {
            return certificate;
        }

        /**
         * Gets signer from smartcard
         *
         * @return the signer
         */

        protected BaseSigner getSigner()
        {
            return signer;
        }

        /**
         * Gets file to be signed
         *
         * @return the file
         * @throws Exception
         */

        protected FileInfo getFile()
        {
            return file;
        }
    }
}