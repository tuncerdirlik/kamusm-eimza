using System;
using tr.gov.tubitak.uekae.esya.api.cades.example.convert;
using tr.gov.tubitak.uekae.esya.api.cades.example.sign;

/*
 * Projede SampleBase.cs dosyasında akılı kart pin ayarları bulunmaktadir. Kendi ayarlarınıza göre düzenleyiniz.
 */
namespace Dotnet_API_Test
{
    public class Test
    {
        public static void Main(string[] args)
        {
            CadesTest();
        }
        public static void CadesTest()
        {
            Console.WriteLine("------------------BESImza at------------------------");
            BESSign besSign = new BESSign();
            besSign.testSimpleSign();

            Console.WriteLine("------------------ESTImza at------------------------");
            ESTSign estSign = new ESTSign();
            estSign.testEstSign();

            Console.WriteLine("------------------Long Imza at------------------");
            ESXLongSign esxLongSign = new ESXLongSign();
            esxLongSign.testEsxlongSign();

            Console.WriteLine("------------------BES TO EST yap------------------");
            Converts converts = new Converts();
            converts.testConvertBES_1();

            Console.WriteLine("finished!"); 
        }
    }
}
