using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;

namespace MT_MDM
{
    public enum SerialBufferEventType
    {
        Data = 1,
        Disconenct = 2,
    }
    public class SerialBufferEventArgs
    {
        public byte sbVal;
        public SerialBufferEventType sbType;

        internal SerialBufferEventArgs(SerialBufferEventType type, Byte val)
        {
            sbVal = val;
            sbType = type;
        }

        public SerialBufferEventType Type
        {
            get { return sbType; }
        }
        public Byte Value
        {
            get { return sbVal; }
        }
    }
    public class SerialBuffer
    {
        //public delegate void SerialBufferEventHandler(object source, EventArgs e);
        public event EventHandler<SerialBufferEventArgs> SerialData;
        private SerialPort port = new SerialPort();
        
        private static Queue<byte> _serialBuffer = new Queue<byte>();
        private static object syncObj = new object();
        public  byte temp = 0;

        public SerialBuffer()
        {
            port.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (port.BytesToRead != 0)
            {
                var data = port.ReadByte();
                OnSerialDataRdy((byte)data);
            }
      
        }

        public bool Connect(int Baud, string portName)
        {
            bool ok = true;
            port.BaudRate = Baud;
            port.PortName = portName;
            port.Parity = Parity.None;
            port.StopBits = (StopBits) 1;
            port.DataBits = 8;
            try
            {
                port.Open();
            }
            catch
            {
                ok = false;
            }

            return ok;
        }

        public void Send(byte val)
        {
            port.Write(new Byte[]{val},0,1);
        }
        //public void AddData(byte val)
        //{
        //    lock(syncObj) {
        //        _serialBuffer.Enqueue(val);
        //        temp = val;
        //    }
        //    OnSerialDataRdy(val);
        //}

        //public static uint GetData()
        //{
        //    uint result = 0;

        //    lock (syncObj)
        //    {
        //        if (_serialBuffer.Count > 0)
        //            result= _serialBuffer.Dequeue();
        //        else
        //            result = 0x100;
        //    }

        //    return result;
        //}

        protected virtual void OnSerialDataRdy(Byte val)
        {
            if(SerialData != null)
                SerialData(this, new SerialBufferEventArgs(SerialBufferEventType.Data, val));
        }

        //public int BufferSize()
        //{
        //    return _serialBuffer.Count;
        //}

    }

    //public class SerialIn
    //{
    //    public static void OnSerialBufRdy(object source, EventArgs e)
    //    {
    //        //Ymodem.serialDataRdy = true;
    //        // Debug.WriteLine("SerialIn Event, Buffer {0} ", (char)SerialBuffer.temp);
    //    }
    //}
}