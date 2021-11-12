using System;
using System.IO.Ports;



namespace AccuSwayPlusLib
{
    public class SerialUtil
    {
        public static void PrintAllPorts()
        {
            String[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                Console.WriteLine(port);
            }
        }
    }
}
