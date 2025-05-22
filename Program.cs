using System;
using CoreScanner;

namespace ZebraScannerHeadless
{
    class Program
    {
        // Declare CoreScannerClass
        static CCoreScannerClass cCoreScannerClass;

        private static void Main(string[] args)
        {
            try
            {
                // Instantiate CoreScanner Class
                cCoreScannerClass = new CCoreScannerClass();

                // Call Open API
                short[] scannerTypes = new short[1];    // Scanner Types you are interested in
                scannerTypes[0] = 1;                    // 1 for all scanner types
                short numberOfScannerTypes = 1;         // Size of the scannerTypes array 
                int status;                             // Extended API return code

                cCoreScannerClass.Open(0, scannerTypes, numberOfScannerTypes, out status);

                // Subscribe for barcode events in cCoreScannerClass
                cCoreScannerClass.BarcodeEvent += new _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);

                // Let's subscribe for events
                int opcode = 1001;  // Method for Subscribe events 
                string outXML;      // XML Output
                string inXML = "<inArgs>" +
                                    "<cmdArgs>" +
                                        "<arg-int>1</arg-int>" + // Number of events you want to subscribe
                                        "<arg-int>1</arg-int>" + // Comma separated event IDs
                                    "</cmdArgs>" +
                                "</inArgs>";

                cCoreScannerClass.ExecCommand(opcode, ref inXML, out outXML, out status);
                Console.WriteLine(outXML);

                // Keep the application running and ready for barcode scans
                Console.WriteLine("Ready. Scan a barcode!");
                Console.WriteLine("Press Ctrl+C to exit.");
                while (true)
                    System.Threading.Thread.Sleep(1000);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Something wrong please check... " + exp.Message);
            }
        }

        // Barcode event handler (called on barcode scan)
        static void OnBarcodeEvent(short eventType, ref string pscanData)
        {
            // Print the raw event XML
            Console.WriteLine($"[Barcode Event] XML:\n{pscanData}");

            // Attempt to parse and print the decoded barcode string
            try
            {
                int start = pscanData.IndexOf("<datalabel>");
                int end = pscanData.IndexOf("</datalabel>");
                if (start >= 0 && end > start)
                {
                    string hex = pscanData.Substring(start + 11, end - (start + 11)).Trim();
                    string[] bytes = hex.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string ascii = "";
                    foreach (var b in bytes)
                    {
                        if (b.StartsWith("0x"))
                        {
                            ascii += Convert.ToChar(Convert.ToByte(b, 16));
                        }
                    }
                    Console.WriteLine($"[Decoded Barcode]: {ascii}");
                }
            }
            catch
            {
                Console.WriteLine("[Could not parse barcode data]");
            }
        }
    }
}
