using System;
using Xunit;

using AccuSwayPlusLib;

namespace AccuSwayPlusLib.Tests
{
    public class ACSConfigFixture
    {
        public readonly String portName = "/dev/tty.usbserial-146310";
        public float[,] callibrationMatrix { get; private set; }
        public float[] offsetVector { get; private set; }

        public ACSConfigFixture() {
            this.callibrationMatrix = new float[6,12]{
                {  0.0467f, -0.0012f,  0.0002f, -0.0452f, -0.0018f, -0.0012f,  0.0465f, -0.0007f, -0.0007f, -0.0457f, -0.0004f, -0.0006f },
                { -0.0008f,  0.0453f, -0.0033f,  0.0004f,  0.0433f, -0.0015f,  0.0003f, -0.0458f,  0.0003f,  0.0026f, -0.0445f,  0.0002f },
                {  0.0000f, -0.0065f, -0.2689f, -0.0057f,  0.0000f, -0.2626f, -0.0028f, -0.0051f, -0.2655f, -0.0018f, -0.0033f, -0.2681f },
                {  0.0000f,  0.0000f, -2.0084f,  0.0000f,  0.0000f, -1.9616f,  0.0000f,  0.0000f,  1.9606f,  0.0000f,  0.0000f,  1.9748f },
                {  0.0000f,  0.0000f,  1.9770f,  0.0000f,  0.0000f, -1.8534f,  0.0000f,  0.0000f,  1.8780f,  0.0000f,  0.0000f, -1.8813f },
                { -0.4757f,  0.4731f,  0.0000f,  0.4601f, -0.4524f,  0.0000f,  0.4732f, -0.4779f,  0.0000f, -0.4655f,  0.4653f,  0.0000f }
            };
            this.offsetVector = new float[12]{ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        }
    }


    public class SerialTests
    {
        [Fact(Skip="nope")]
        public void Test1()
        {
            AccuSwayPlusLib.SerialUtil.PrintAllPorts();
        }
    }

    public class ACSTests: IClassFixture<ACSConfigFixture>
    {
        ACSConfigFixture fixture;
        public ACSTests(ACSConfigFixture fixture) {
            this.fixture = fixture;
        }

        [Fact(Skip="Sub-test")]
        public void TestZero()
        {
            Console.WriteLine("Constructing ACS+ instance...");
            AccuSwayPlus acs = AccuSwayPlus.Create(this.fixture.portName, this.fixture.callibrationMatrix, this.fixture.offsetVector);
            Console.WriteLine("Auto-zeroing...");
            byte[] zeroData = acs.AutoZero();
            Console.WriteLine($"{zeroData[0]:x} {zeroData[1]:x} {zeroData[2]:x} {zeroData[3]:x} {zeroData[4]:x} {zeroData[5]:x}");
            Console.WriteLine($"{zeroData[6]:x} {zeroData[7]:x} {zeroData[8]:x} {zeroData[9]:x} {zeroData[01]:x} {zeroData[11]:x}");
        }
        [Fact(Skip="Not supported by our version of the ACS+")]
        public void TestSerialNumber()
        {
            Console.WriteLine("Constructing ACS+ instance...");
            AccuSwayPlus acs = AccuSwayPlus.Create(this.fixture.portName, this.fixture.callibrationMatrix, this.fixture.offsetVector);
            Console.WriteLine("Getting Serial Number...");
            String sn = acs.GetSerialNumber();
            Console.WriteLine($"'{sn}'");
        }


        [Fact]
        public void TestDataCollection()
        {
            Console.WriteLine("Constructing ACS+ instance...");
            AccuSwayPlus acs = AccuSwayPlus.Create(this.fixture.portName, this.fixture.callibrationMatrix, this.fixture.offsetVector);
            Console.WriteLine("Auto-zeroing...");
            byte[] zeroData = acs.AutoZero();

            Console.WriteLine("Waiting 2500ms...");
            EventAggregator<ACSData> agg = new EventAggregator<ACSData>();
            System.Threading.Thread.Sleep(2500);
            Console.WriteLine("Starting data for 500ms...");
            acs.StartData(agg.OnEvent);
            System.Threading.Thread.Sleep(500);
            Console.WriteLine("Stopping data...");
            acs.StopData();
            foreach(ACSData data in agg.events) {
                Console.WriteLine(data.ToString());
            }
            Console.WriteLine($"Collected {agg.events.Count} samples.");
        }
    }
}
