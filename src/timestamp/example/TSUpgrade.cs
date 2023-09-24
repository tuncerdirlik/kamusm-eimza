using System;
using System.Collections.Generic;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.cms;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.cades.example;
using tr.gov.tubitak.uekae.esya.api.cades.example.validation;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.api.common;
using tr.gov.tubitak.uekae.esya.api.signature.attribute;
using tr.gov.tubitak.uekae.esya.asn.util;

namespace tr.gov.tubitak.uekae.esya.api.timestamp.example
{
    [TestFixture]
    public class TSUpgrade: CadesSampleBase
    {
        [Test]
        public void upgradeTimeStampTest()
        {              
            String timeStampFile = "zdFile";      
            bool upgraded = upgradeTimeStamp(timeStampFile);

            if (upgraded)
             Console.WriteLine("Upgrade edildi.");
            else
             Console.WriteLine("Upgrade edilmedi.");
        }

        private bool upgradeTimeStamp(String fileToBeUpgraded) 
        {

            bool upgraded = false;

            byte[] fileToBeUpgradedInBytes = AsnIO.dosyadanOKU(fileToBeUpgraded);

            BaseSignedData baseSignedData = new BaseSignedData(fileToBeUpgradedInBytes);
            Signer signer = baseSignedData.getSignerList()[0];

            if(signer.getType().Equals(ESignatureType.TYPE_BES))
            {
                ECertificate signerCertificate = signer.getSignerCertificate();
                if(isInTheTimeOfUpgrade(signerCertificate))
                {
                    Dictionary<String, Object> parameters = getParametersForUpgrade();

                    signer.convert(ESignatureType.TYPE_ESXLong, parameters);
                    upgraded = true;

                    CadesSignatureValidation validationUtil = new CadesSignatureValidation();
                    SignedDataValidationResult sdvr = validationUtil.validate(signer.getBaseSignedData().getEncoded(), null);
                    Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());                  
                }
            }
           else if(signer.getType().Equals(ESignatureType.TYPE_EST) ||  signer.getType().Equals(ESignatureType.TYPE_ESXLong) || signer.getType().Equals(ESignatureType.TYPE_ESA))
           {
               List<TimestampInfo> allTimeStamps = signer.getAllTimeStamps();
               TimestampInfo latestTimestampInfo = allTimeStamps[allTimeStamps.Count - 1];

               ESignedData tsSignedData = latestTimestampInfo.getSignedData();
               ECertificate tsCert = tsSignedData.getSignerInfo(0).getSignerCertificate(tsSignedData.getCertificates());

               if(isInTheTimeOfUpgrade(tsCert))
               {

                   Dictionary<String, Object> parameters = getParametersForUpgrade();

                   signer.convert(ESignatureType.TYPE_ESA, parameters);
                   upgraded = true;

                   CadesSignatureValidation validationUtil = new CadesSignatureValidation();
                   SignedDataValidationResult sdvr = validationUtil.validate(signer.getBaseSignedData().getEncoded(), null);
                   Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());            
               }
           } 
           return upgraded;
         }

        private Dictionary<string, object> getParametersForUpgrade() 
        {

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            //Time stamp will be needed for XLONG-type signature
            //Archive time stamp will be needed for ESA-type signature
            parameters[EParameters.P_TSS_INFO] = getTSSettings();         
            parameters[EParameters.P_CERT_VALIDATION_POLICY] = getPolicy();
             
           return parameters;
       }

       private bool isInTheTimeOfUpgrade(ECertificate cert) 
       {
           DateTime? certStartTime = cert.getNotBefore();
           DateTime? certEndTime = cert.getNotAfter();

           DateTime? now = DateTime.UtcNow;

           if (!(now.Value.ToUniversalTime() > certStartTime.Value.ToUniversalTime() && now.Value.ToUniversalTime() < certEndTime.Value.ToUniversalTime()))
               throw new ESYAException("Certificate validity period is between " + cert.getNotBefore().Value.ToUniversalTime() + " and " + cert.getNotAfter().Value.ToUniversalTime());

           long threeMonthsAheadInMilliseconds = now.Value.ToUniversalTime().AddMonths(3).Ticks;        
           long certEndTimeInMilliseconds = certEndTime.Value.ToUniversalTime().Ticks;

           if(threeMonthsAheadInMilliseconds >= certEndTimeInMilliseconds)
             return true;
           else
             return false;
       }
    }
}
