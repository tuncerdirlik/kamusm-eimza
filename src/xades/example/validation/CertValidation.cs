using System;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.certificate.validation;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.check.certificate;
using tr.gov.tubitak.uekae.esya.api.certificate.validation.policy;
using tr.gov.tubitak.uekae.esya.api.xades.example;

namespace tr.gov.tubitak.uekae.esya.api.xmlsignature.example.validation
{
    /**
     * Provides validation functions for certificates
     */

    public class CertValidation : XadesSampleBase
    {
        /**
         * Validates given certificate
         */

        public static bool validateCertificate(ECertificate certificate)
        {
            try
            {
                // generate policy which going to be used in validation
                ValidationPolicy policy = new ValidationPolicy();
                string policyPath = getRootDir() + "/config/certval-policy-test.xml";
                policy = PolicyReader.readValidationPolicy(policyPath);

                // generate validation system
                ValidationSystem vs = CertificateValidation.createValidationSystem(policy);
                vs.setBaseValidationTime(DateTime.UtcNow);

                // validate certificate
                CertificateStatusInfo csi = CertificateValidation.validateCertificate(vs, certificate);

                // return true if certificate is valid, false otherwise
                if (csi.getCertificateStatus() != CertificateStatus.VALID)
                    return false;
                if (csi.getCertificateStatus() == CertificateStatus.VALID)
                    return true;
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while validating certificate", e);
            }
            return false;
        }
    }
}