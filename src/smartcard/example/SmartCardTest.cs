using System;
using System.Collections.Generic;
using ImzaApiTest.src.tr.gov.tubitak.uekae.esya.api;
using NUnit.Framework;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.common.util.bag;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.smartcard.pkcs11;
using tr.gov.tubitak.uekae.esya.api.smartcard.util;

/**
 * Several smart card operations are shown.
 */

namespace tr.gov.tubitak.uekae.esya.api.cmssignature.example.smartcard
{
    [TestFixture]
    public class SmartCardTest : SampleBase
    {
        /**
        * Certificates in smart card are read and the common names of certificates are printed to the standard output.
        * @throws Exception
        */

        [Test]
        public void testListCertInSmartCard()
        {
            SmartCard sc = new SmartCard(CardType.AKIS);
            long[] slots = sc.getSlotList();
            long session = sc.openSession(slots[0]);
            sc.login(session, getPin());
            List<byte[]> certBytes = sc.getSignatureCertificates(session);
            foreach (byte[] bs in certBytes)
            {
                ECertificate cert = new ECertificate(bs);
                Console.WriteLine(cert.getSubject().getCommonNameAttribute());
            }
            sc.logout(session);
            sc.closeSession(session);
        }

        /**
         * Key labels of signature keys are printed to standard output.
         * @throws PKCS11Exception
         * @throws IOException
         * @throws SmartCardException
         */

        [Test]
        public void testListKeyLabels()
        {
            SmartCard sc = new SmartCard(CardType.AKIS);
            long slot = sc.getSlotList()[0];
            long session = sc.openSession(slot);
            sc.login(session, getPin());
            string[] labels = sc.getSignatureKeyLabels(session);
            foreach (string label in labels)
                Console.WriteLine(label);
            sc.logout(session);
        }

        /**
         * Get card type and slot number of the connected smart cards and prints them.  
         * @throws Exception
         */

        [Test]
        public void testprintSmartCard_1()
        {
            List<Pair<long, CardType>> terminals = SmartOp.findCardTypesAndSlots();
            foreach (Pair<long, CardType> objects in terminals)
            {
                long slot1 = objects.getmObj1();
                CardType cardType = objects.getmObj2();
                Console.WriteLine(slot1 + ":" + cardType);
            }
        }

        /**
         * The name of card readers, the slot of the card and the type of the card are printed.
         * @throws Exception
         */

        [Test]
        public void testselectSmartCard_2()
        {
            //terminal names are taken
            string[] terminals = SmartOp.getCardTerminals();
            foreach (string terminal in terminals)
            {
                //card type and slot number is taken of the terminal
                Pair<long, CardType> slotAndCardType = SmartOp.getSlotAndCardType(terminal);
                Console.WriteLine("Terminal: " + terminal + " Slot: " + slotAndCardType.getmObj1()
                                  + " CardType: " + slotAndCardType.getmObj2());
            }
        }

        /**
         * If there are more than one connected smart cards to the system, it wants user to select the card. 
         * A GUI appears.
         * @throws Exception
         */

        [Test]
        public void testselectSmartCard_3()
        {
            try
            {
                Pair<long, CardType> card = SmartOp.findCardTypeAndSlot(null);
                long slot = card.getmObj1();
                CardType cardType = card.getmObj2();
                Console.WriteLine("Slot: " + slot + " Card Type: " + cardType);

                SmartCard sc = new SmartCard(cardType);
                long session = sc.openSession(slot);
                sc.login(session, getPin());
                ECertificate cert = new ECertificate(sc.getSignatureCertificates(session)[0]);

                SCSignerWithCertSerialNo signer = new SCSignerWithCertSerialNo(sc, session, slot,
                    cert.getSerialNumber().GetData(), SignatureAlg.RSA_SHA1.getName());
                sc.logout(session);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}