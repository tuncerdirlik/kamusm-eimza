using System;
using System.Collections.Generic;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.profile;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.cmssignature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.attribute;
using tr.gov.tubitak.uekae.esya.api.cmssignature.signature;
using tr.gov.tubitak.uekae.esya.api.cmssignature.validation;
using tr.gov.tubitak.uekae.esya.asn.util;

namespace tr.gov.tubitak.uekae.esya.api.cades.example.validation
{
    [TestFixture]
    public class CadesSignatureValidation : CadesSampleBase
    {

        [Test]
        public void testValidation()
        {
            byte[] input = AsnIO.dosyadanOKU(getTestDataFolder() + "ESXLong-1.p7s");

            SignedDataValidationResult sdvr = validate(input, null);

            Assert.AreEqual(SignedData_Status.ALL_VALID, sdvr.getSDStatus());

            BaseSignedData bs = new BaseSignedData(input);
            Console.WriteLine(bs.getSignerList()[0].getSignerCertificate());
        }


        public SignedDataValidationResult validate(byte[] signature, ISignable externalContent)
        {
            return validate(signature, externalContent, getPolicy(), null);
        }

        public SignedDataValidationResult validate(byte[] signature, ISignable externalContent, TurkishESigProfile turkishESigProfile)
        {
            return validate(signature, externalContent, getPolicy(), turkishESigProfile);
        }


        public SignedDataValidationResult validate(byte[] signature, ISignable externalContent, ValidationPolicy policy, TurkishESigProfile turkishESigProfile)
        {
            Dictionary<string, object> params_ = new Dictionary<string, object>();

            if (turkishESigProfile != null)
                params_[EParameters.P_VALIDATION_PROFILE] = turkishESigProfile;

            params_[EParameters.P_CERT_VALIDATION_POLICY] = policy;

            if (externalContent != null)
                params_[EParameters.P_EXTERNAL_CONTENT] = externalContent;

            //Use only reference and their corresponding value to validate signature
            params_[EParameters.P_FORCE_STRICT_REFERENCE_USE] = true;
            //Ignore grace period which means allow usage of CRL published before signature time 
            //params_[EParameters.P_IGNORE_GRACE] = true;

            //Use multiple policies if you want to use different policies to validate different types of certificate
            // CertValidationPolicies certificateValidationPolicies = new CertValidationPolicies();
            // certificateValidationPolicies.register(CertValidationPolicies.CertificateType.DEFAULT.ToString(), getPolicy());
            // ValidationPolicy maliMuhurPolicy = PolicyReader.readValidationPolicy(new FileStream(getTestDataFolder()+"/config/certval-policy-malimuhur.xml", FileMode.Open, FileAccess.Read));
            // certificateValidationPolicies.register(CertValidationPolicies.CertificateType.MaliMuhurCertificate.ToString(), maliMuhurPolicy);
            // params_[EParameters.P_CERT_VALIDATION_POLICIES]= certificateValidationPolicies;

            SignedDataValidation sdv = new SignedDataValidation();
            SignedDataValidationResult sdvr = sdv.verify(signature, params_);

            return sdvr;
        }
    }
}