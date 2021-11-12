using System;
using System.IO.Ports;



namespace AccuSwayPlusLib
{
    public class AccuSwayPlus: IDisposable 
    {
        public event EventHandler<ACSData> DataReceived;

        private SerialPort port = null;

        private EventHandler<ACSData> activeCallback = null;

        private float[,] callibrationMatrix = null;
        private float[] offsetVector = null;

        public AccuSwayPlus(SerialPort port, float[,] callibrationMatrix, float[] offsetVector) {
            this.port = port;
            this.callibrationMatrix = callibrationMatrix;
            this.offsetVector = offsetVector;

            if(!this.port.IsOpen) {
                this.port.Open();
            }
            if(this.port.BytesToRead > 0) {
                // Console.WriteLine($"Clearing out {this.port.BytesToRead} bytes of old data");
                this.port.DiscardInBuffer();
            }
            // this.port.DataReceived += new SerialDataReceivedEventHandler(AccuSwayPlus.OnData);

            byte[] cmdBuffer = { 0x72 };
            byte[] ackBuffer = { 0x00 };
            // Console.WriteLine("Setting Baud Rate...");
            for(int i = 0; i < 5; i++) {
                this.port.Write(cmdBuffer, 0, 1);
                int bytesRead = this.port.Read(ackBuffer, 0, 1);
                if(bytesRead == 1) { break; }
            }
            if(ackBuffer[0] != 0x72) {
                throw new Exception($"Expected acknowledgement of 0x72, but got 0x{ackBuffer[0]:x}");
            }
        }
        public static AccuSwayPlus Create(String portName, float[,] callibrationMatrix, float[] offsetVector) {
            SerialPort port = new SerialPort(portName, 57600, Parity.None, 8, StopBits.One);
            port.Handshake = Handshake.None;
            port.ReadTimeout = 5000;
            port.WriteTimeout = 100;
            return new AccuSwayPlus(port, callibrationMatrix, offsetVector);
        }

        public void Dispose() {
            if(this.port != null) { this.port.Close(); }
            this.port = null;
        }

        private void SendCommand(byte command, byte acknowledgement) {
            // Console.WriteLine("Sending Command...");
            byte[] cmdBuffer = { command };
            this.port.Write(cmdBuffer, 0, 1);

            // Console.WriteLine("Waiting for acknowledgement...");
            int ack = this.port.ReadByte();
            if(ack != acknowledgement) {
                throw new Exception($"Expected ACK of 0x{acknowledgement:x}, but got 0x{ack:x}");
            }
        }
        private int ReadBytes(byte[] buffer, int size) {
            int index = 0;
            while(index < size) {
                // Console.WriteLine($"    Reading byte {index}...");
                int res = this.port.ReadByte();
                if(res == -1) { break; }
                buffer[index++] = (byte)res;
            }
            return index;
        }



        public void StartData(EventHandler<ACSData> callback) {
            this.SendCommand(0x51, 0x55);
            
            this.port.DataReceived += (this.OnSensorData);
            this.activeCallback = callback;
            this.DataReceived += (this.activeCallback);
        }
        public void StopData() {
            byte[] stopBuffer = { 0x52 };
            this.port.Write(stopBuffer, 0, 1);

            this.port.DataReceived -= (this.OnSensorData);
            this.DataReceived -= (this.activeCallback);
            this.activeCallback = null;
        }

        public byte[] AutoZero() {
            this.SendCommand(0x53, 0x54);

            byte[] dataBuffer = new byte[12];
            int bytesRead = this.ReadBytes(dataBuffer, 12);
            if(bytesRead != 12) {
                throw new Exception($"Expected 12 bytes of zero data, but read {bytesRead}");
            }

            return dataBuffer;
        }

        public String GetSerialNumber() {
            this.SendCommand(0x58, 0x78);

            byte[] dataBuffer = new byte[4];
            int bytesRead = this.ReadBytes(dataBuffer, 12);
            if(bytesRead != 12) {
                throw new Exception($"Expected 4 bytes of serial number data, but read {bytesRead}");
            }
            return System.Text.Encoding.ASCII.GetString(dataBuffer, 0, 12);
        }


        private int[] sensorBuffer = new int[12];
        private void OnSensorData(object sender, SerialDataReceivedEventArgs e) {
            SerialPort sp = (SerialPort)sender;
            while(sp.BytesToRead >= 24) {
                byte[] dataBuffer = new byte[24];
                int bytesRead = sp.Read(dataBuffer, 0, 24);
                if (bytesRead != 24) { throw new Exception($"Failed to read 24 bytes for data sample, instead read {bytesRead}"); }
                // Console.Write($"(0x{dataBuffer[0]:x2}{dataBuffer[1]:x2}, 0x{dataBuffer[2]:x2}{dataBuffer[3]:x2}, 0x{dataBuffer[4]:x2}{dataBuffer[5]:x2}) ");
                // Console.Write($"(0x{dataBuffer[6]:x2}{dataBuffer[7]:x2}, 0x{dataBuffer[8]:x2}{dataBuffer[9]:x2}, 0x{dataBuffer[10]:x2}{dataBuffer[11]:x2}) ");
                // Console.Write($"(0x{dataBuffer[12]:x2}{dataBuffer[13]:x2}, 0x{dataBuffer[14]:x2}{dataBuffer[15]:x2}, 0x{dataBuffer[16]:x2}{dataBuffer[17]:x2}) ");
                // Console.Write($"(0x{dataBuffer[18]:x2}{dataBuffer[19]:x2}, 0x{dataBuffer[20]:x2}{dataBuffer[21]:x2}, 0x{dataBuffer[22]:x2}{dataBuffer[23]:x2})");
                // Console.WriteLine();

                for(int i = 0; i < 24; i += 2) {
                    sensorBuffer[i / 2] = ACSData.parseRawWord(dataBuffer[i], dataBuffer[i + 1]);
                }

                ACSData data = ACSData.fromRaw(sensorBuffer, this.callibrationMatrix);
                this.OnDataReceived(data);
            }
        }
        protected virtual void OnDataReceived(ACSData e) {
            EventHandler<ACSData> handler = this.DataReceived;
            handler?.Invoke(this, e);
        }
    }

    public class ACSData : EventArgs
    {
        public float Fx { get; set; }
        public float Fy { get; set; }
        public float Fz { get; set; }
        public float Mx { get; set; }
        public float My { get; set; }
        public float Mz { get; set; }

        public ACSData(float Fx, float Fy, float Fz, float Mx, float My, float Mz) {
            this.Fx = Fx;
            this.Fy = Fy;
            this.Fz = Fz;
            this.Mx = Mx;
            this.My = My;
            this.Mz = Mz;
        }
        public static ACSData fromRaw(int[] data, float[,] calibrationMatrix)
        {
            float[] values = new float[6];

            for(int i = 0; i < 6; i++) {
                for (int j = 0; j < 12; j++) {
                    values[i] += calibrationMatrix[i, j] * data[j];
                }
            }

            return new ACSData(values[0], values[1], values[2], values[3], values[4], values[5]);
        }

        public static int parseRawWord(byte lo, byte hi) {
            return ((int)lo | (((int)hi & 0x0f) << 8)) - 2048;
        }

        public override String ToString() {
            return $"ACSData(F=({this.Fx}, {this.Fy}, {this.Fz}), M=({this.Mx}, {this.My}, {this.Mz}))";
        }
    }
}
