using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using iaik.pkcs.pkcs11.wrapper;
using log4net;
using tr.gov.tubitak.uekae.esya.api.asn.x509;
using tr.gov.tubitak.uekae.esya.api.common;
using tr.gov.tubitak.uekae.esya.api.common.crypto;
using tr.gov.tubitak.uekae.esya.api.common.util;
using tr.gov.tubitak.uekae.esya.api.common.util.bag;
using tr.gov.tubitak.uekae.esya.api.crypto.alg;
using tr.gov.tubitak.uekae.esya.api.crypto.util;
using tr.gov.tubitak.uekae.esya.api.smartcard.gui;
using tr.gov.tubitak.uekae.esya.api.smartcard.pkcs11;

/**
 * SmartCardManager handles user smart card operations.
 * Works with Java 6
 */

namespace tr.gov.tubitak.uekae.esya.api.cmssignature.example.util
{
    public class SmartCardManager
    {
        private static readonly ILog LOGGER = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private static Object lockObject = new Object();

        private static SmartCardManager mSCManager;

        private readonly string mSerialNumber;

        private readonly int mSlotCount;

        protected IBaseSmartCard bsc;

        private ECertificate mEncryptionCert;

        private ECertificate mSignatureCert;

        protected BaseSigner mSigner;


        /**
         *
         * @throws SmartCardException
         */

        private SmartCardManager()
        {
            try
            {
                LOGGER.Debug("New SmartCardManager will be created");
                string[] terminals = SmartOp.getCardTerminals();
                string terminal;


                if ((terminals == null) || (terminals.Length == 0))
                    throw new SmartCardException("Kart takılı kart okuyucu bulunamadı");

                LOGGER.Debug("Kart okuyucu sayısı : " + terminals.Length);

                int index = 0;
                if (terminals.Length == 1)
                    terminal = terminals[index];
                else
                {
                    index = askOption(null, terminals, "Okuyucu Listesi");
                    terminal = terminals[index];
                }
                LOGGER.Debug("PKCS11 Smartcard will be created");
                Pair<long, CardType> slotAndCardType = SmartOp.getSlotAndCardType(terminal);
                bsc = new P11SmartCard(slotAndCardType.getmObj2());
                bsc.openSession(slotAndCardType.getmObj1());

                mSerialNumber = StringUtil.ToString(bsc.getSerial());
                mSlotCount = terminals.Length;
            }
            catch (SmartCardException e)
            {
                throw e;
            }
            catch (PKCS11Exception e)
            {
                throw new SmartCardException("Pkcs11 exception", e);
            }
            catch (IOException e)
            {
                throw new SmartCardException("Smart Card IO exception", e);
            }
        }

        /**
         * Singleton is used for this class. If many card placed, it wants to user to select one of cards.
         * If there is a influential change in the smart card environment, it  repeat the selection process.
         * The influential change can be: 
         * 		If there is a new smart card connected to system.
         * 		The cached card is removed from system.
         * These situations are checked in getInstance() function. So for your non-squential SmartCard Operation,
         * call getInstance() function to check any change in the system.
         *
         * In order to reset thse selections, call reset function.
         * 
         * @return SmartCardManager instance
         * @throws SmartCardException
         */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static SmartCardManager getInstance()
        {
            if (mSCManager == null)
            {
                mSCManager = new SmartCardManager();
                return mSCManager;
            }
            //Check is there any change
            try
            {
                //If there is a new card in the system, user will select a smartcard. 
                //Create new SmartCard.
                if (mSCManager.getSlotCount() < SmartOp.getCardTerminals().Length)
                {
                    LOGGER.Debug("New card pluged in to system");
                    mSCManager = null;
                    return getInstance();
                }

                //If used card is removed, select new card.
                string availableSerial = null;
                try
                {
                    availableSerial = StringUtil.ToString(mSCManager.getBasicSmartCard().getSerial());
                }
                catch (SmartCardException ex)
                {
                    LOGGER.Debug("Card removed");
                    mSCManager = null;
                    return getInstance();
                }
                if (!mSCManager.getSelectedSerialNumber().Equals(availableSerial))
                {
                    LOGGER.Debug("Serial number changed. New card is placed to system");
                    mSCManager = null;
                    return getInstance();
                }

                return mSCManager;
            }
            catch (SmartCardException e)
            {
                mSCManager = null;
                throw;
            }
        }

        /**
         * BaseSigner interface for the requested certificate. Do not forget to logout after your crypto 
         * operation finished
         * @param aCardPIN
         * @param aCert
         * @return
         * @throws SmartCardException
         */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public BaseSigner getSigner(string aCardPIN, ECertificate aCert, string aSigningAlg,
            IAlgorithmParameterSpec aParams)
        {
            if (mSigner == null)
            {
                bsc.login(aCardPIN);
                mSigner = bsc.getSigner(aCert, aSigningAlg, aParams);
            }
            return mSigner;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public BaseSigner getSigner(string aCardPIN, ECertificate aCert)
        {
            if (mSigner == null)
            {
                bsc.login(aCardPIN);

                SignatureAlg algorithmType = SignatureAlg.fromAlgorithmIdentifier(aCert.getPublicKeyAlgorithm()).getmObj1();

                if (algorithmType.getAsymmetricAlg().Equals(AsymmetricAlg.RSA)) {                

                    mSigner = bsc.getSigner(aCert, Algorithms.SIGNATURE_RSA_SHA256);

                } else if (algorithmType.getAsymmetricAlg().Equals(AsymmetricAlg.ECDSA)) {  
                    
                    SignatureAlg signatureAlg = ECUtil.getConvenientECSignatureAlgForECCertificate(aCert);
                    mSigner = bsc.getSigner(aCert, signatureAlg.ToString());
                } else {
                    throw new SmartCardException("Unknown algorithm type: " + algorithmType);
                }                
            }
            return mSigner;
        }


        /**
         * Logouts from smart card. 
         * @throws SmartCardException
         */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void logout()
        {
            mSigner = null;
            bsc.logout();
        }

        /**
         * Returns for the signature certificate. If there are more than one certificates in the card in requested
         * attributes, it wants user to select the certificate. It caches the selected certificate, to reset cache,
         * call reset function.
         * 
         * @param checkIsQualified Only selects the qualified certificates if it is true.
         * @param checkBeingNonQualified Only selects the non-qualified certificates if it is true. 
         * if the two parameters are false, it selects all certificates.
         * if the two parameters are true, it throws ESYAException. A certificate can not be qualified and non qualified at
         * the same time.
         * 
         * @return certificate
         * @throws SmartCardException
         * @throws ESYAException
         */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ECertificate getSignatureCertificate(bool checkIsQualified, bool checkBeingNonQualified)
        {
            if (mSignatureCert == null)
            {
                List<byte[]> allCerts = bsc.getSignatureCertificates();
                mSignatureCert = selectCertificate(checkIsQualified, checkBeingNonQualified, allCerts);
            }

            return mSignatureCert;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ECertificate getSignatureCertificate(bool isQualified)
        {
            return getSignatureCertificate(isQualified, !isQualified);
        }

        /**
         * Returns for the encryption certificate. If there are more than one certificates in the card in requested
         * attributes, it wants user to select the certificate. It caches the selected certificate, to reset cache,
         * call reset function.
         * 
         * @param checkIsQualified
         * @param checkBeingNonQualified
         * @return
         * @throws SmartCardException
         * @throws ESYAException
         */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ECertificate getEncryptionCertificate(bool checkIsQualified, bool checkBeingNonQualified)
        {
            if (mEncryptionCert == null)
            {
                List<byte[]> allCerts = bsc.getEncryptionCertificates();
                mEncryptionCert = selectCertificate(checkIsQualified, checkBeingNonQualified, allCerts);
            }

            return mEncryptionCert;
        }

        private ECertificate selectCertificate(bool checkIsQualified, bool checkBeingNonQualified, List<byte[]> aCerts)
        {
            if ((aCerts != null) && (aCerts.Count == 0))
                throw new ESYAException("Kartta sertifika bulunmuyor");

            if (checkIsQualified && checkBeingNonQualified)
                throw new ESYAException(
                    "Bir sertifika ya nitelikli sertifikadir, ya niteliksiz sertifikadir. Hem nitelikli hem niteliksiz olamaz");

            List<ECertificate> certs = new List<ECertificate>();

            foreach (byte[] bs in aCerts)
            {
                ECertificate cert = new ECertificate(bs);
                if (!checkIsDateValid(cert))
                    continue;

                if (checkIsQualified)
                {
                    if (cert.isQualifiedCertificate())
                        certs.Add(cert);
                }
                else if (checkBeingNonQualified)
                {
                    if (!cert.isQualifiedCertificate())
                        certs.Add(cert);
                }
                else
                {
                    certs.Add(cert);
                }
            }

            ECertificate selectedCert = null;

            if (certs.Count == 0)
            {
                if (checkIsQualified)
                    throw new ESYAException("Kartta nitelikli sertifika bulunmuyor");
                if (checkBeingNonQualified)
                    throw new ESYAException("Kartta niteliksiz sertifika bulunmuyor");
            }
            else if (certs.Count == 1)
            {
                selectedCert = certs[0];
            }
            else
            {
                string[] optionList = new string[certs.Count];
                for (int i = 0; i < certs.Count; i++)
                    optionList[i] = certs[i].getSubject().getCommonNameAttribute() + " " + certs[i].getSerialNumberHex();

                int result = askOption(null, optionList, "Sertifika Listesi");

                if (result < 0)
                    selectedCert = null;
                else
                    selectedCert = certs[result];
            }
            return selectedCert;
        }

        private bool checkIsDateValid(ECertificate cert)
        {
            DateTime? certStartTime = cert.getNotBefore();
            DateTime? certEndTime = cert.getNotAfter();

            DateTime? now = DateTime.UtcNow;

            if (now.Value.ToUniversalTime() > certStartTime.Value.ToUniversalTime() && now.Value.ToUniversalTime() < certEndTime.Value.ToUniversalTime())
                return true;
            else
                return false;
        }


        private string getSelectedSerialNumber()
        {
            return mSerialNumber;
        }

        private int getSlotCount()
        {
            return mSlotCount;
        }

        public IBaseSmartCard getBasicSmartCard()
        {
            return bsc;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void reset()
        {
            mSCManager = null;
        }


        public static int askOption(Control aParent, string[] aSecenekList, string aDescription)
        {
            SelectorGUI selector = new SelectorGUI(aParent);
            return selector.Select(aDescription, aSecenekList);
        }
    }
}