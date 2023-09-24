using System;
using System.Collections.Generic;
using ImzaApiTest.src.tr.gov.tubitak.uekae.esya.api;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.pkcs1pkcs8;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.common.util.bag;
using tr.gov.tubitak.uekae.esya.api.crypto;
using tr.gov.tubitak.uekae.esya.api.crypto.provider.core.baseTypes;
using tr.gov.tubitak.uekae.esya.api.smartcard.pkcs11;

namespace tr.gov.tubitak.uekae.esya.api.src.smartcard.pfx
{
    public class LoadPfx : SampleBase
    {
        private String aLabel = "TestCertificate";
       
        [Test]
        public void test_1_LoadPfx()
        {
            SmartCard sc = new SmartCard(CardType.AKIS);
            long session = sc.openSession(1);
            sc.login(session,getPin());

            string pfxPath = getRootDir() + "/sertifika deposu/test1@test.com_745418.pfx";
            IPfxParser pfxParser = Crypto.getPfxParser();
            pfxParser.loadPfx(pfxPath, "745418");
           
            List<Pair<ECertificate, IPrivateKey>> entries = pfxParser.getCertificatesAndKeys();
            foreach (Pair<ECertificate, IPrivateKey> pair in entries)
            {
                ECertificate cert = pair.first();
                ESubjectPublicKeyInfo subjectPublicKeyInfo = cert.getSubjectPublicKeyInfo();
                EPrivateKeyInfo privateKeyInfo = new EPrivateKeyInfo(pair.getmObj2().getEncoded());
                            
                sc.importKeyPair(session, aLabel, subjectPublicKeyInfo, privateKeyInfo, null, null, true, false);              
                sc.importCertificate(session, aLabel, cert);
            }

            sc.logout(session);
        }

        [Test]
        public void test_2_DeleteObjects()
        {
            SmartCard sc = new SmartCard(CardType.AKIS);                 
            long session = sc.openSession(1);
  
            sc.login(session, getPin());

            sc.deleteCertificate(session, aLabel);
            sc.deletePrivateObject(session, aLabel);
            sc.deletePublicObject(session, aLabel);

            sc.logout(session);
        }
    }
}
