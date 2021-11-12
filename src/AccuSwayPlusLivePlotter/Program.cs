using System;

using AccuSwayPlusLib;



namespace AccuSwayPlusLivePlotter
{
    public class Program
    {
        public static readonly String portName = "/dev/tty.usbserial-146310";
        public static readonly float[,] callibrationMatrix = new float[6, 12]{
            {  0.0467f, -0.0012f,  0.0002f, -0.0452f, -0.0018f, -0.0012f,  0.0465f, -0.0007f, -0.0007f, -0.0457f, -0.0004f, -0.0006f },
            { -0.0008f,  0.0453f, -0.0033f,  0.0004f,  0.0433f, -0.0015f,  0.0003f, -0.0458f,  0.0003f,  0.0026f, -0.0445f,  0.0002f },
            {  0.0000f, -0.0065f, -0.2689f, -0.0057f,  0.0000f, -0.2626f, -0.0028f, -0.0051f, -0.2655f, -0.0018f, -0.0033f, -0.2681f },
            {  0.0000f,  0.0000f, -2.0084f,  0.0000f,  0.0000f, -1.9616f,  0.0000f,  0.0000f,  1.9606f,  0.0000f,  0.0000f,  1.9748f },
            {  0.0000f,  0.0000f,  1.9770f,  0.0000f,  0.0000f, -1.8534f,  0.0000f,  0.0000f,  1.8780f,  0.0000f,  0.0000f, -1.8813f },
            { -0.4757f,  0.4731f,  0.0000f,  0.4601f, -0.4524f,  0.0000f,  0.4732f, -0.4779f,  0.0000f, -0.4655f,  0.4653f,  0.0000f }
        };
        public static readonly float[] offsetVector = new float[12]{ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

        public static void Main(string[] args)
        {
            AccuSwayPlus acs = AccuSwayPlus.Create(portName, callibrationMatrix, offsetVector);
            byte[] zeroData = acs.AutoZero();
            Console.WriteLine("zero");
            System.Threading.Thread.Sleep(2500);

            acs.StartData(OnACSData);
            System.Threading.Thread.Sleep(25000);
            acs.StopData();
            Console.WriteLine("stop");
        }

        public static void OnACSData(Object sender, ACSData data) {
            Console.WriteLine($"({data.Fx},{data.Fy},{data.Fz},{data.Mx},{data.My},{data.Mz})");
        }
    }
}
